import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import cv2
import numpy as np  # 프레임 복사 및 수정을 위해 추가

# MediaPipe 그리기 유틸리티 및 포즈 솔루션 임포트
from mediapipe.python.solutions import drawing_utils as mp_drawing
from mediapipe.python.solutions import pose as mp_pose
from mediapipe.framework.formats import landmark_pb2
import time

if __name__ == "__main__":
    s_time = time.time()
    model_path = "./ckpts/pose_landmarker_lite.task"
    input_video_path = "./data/IMG_3888.MOV"  # 입력 비디오 경로
    output_video_path = "./results/output_pose_estimation_video.mp4"  # 출력 비디오 경로

    # MediaPipe PoseLandmarker 작업 생성 옵션 설정
    BaseOptions = mp.tasks.BaseOptions
    PoseLandmarker = mp.tasks.vision.PoseLandmarker
    PoseLandmarkerOptions = mp.tasks.vision.PoseLandmarkerOptions
    VisionRunningMode = mp.tasks.vision.RunningMode

    options = PoseLandmarkerOptions(
        base_options=BaseOptions(model_asset_path=model_path), running_mode=VisionRunningMode.VIDEO
    )

    with PoseLandmarker.create_from_options(options) as landmarker:
        # 입력 비디오 로드
        cap = cv2.VideoCapture(input_video_path)
        if not cap.isOpened():
            print(f"오류: 비디오 파일을 열 수 없습니다: {input_video_path}")
            cap.release()
            exit()

        # 비디오 정보 가져오기 (FPS, 너비, 높이)
        fps = int(cap.get(cv2.CAP_PROP_FPS))
        frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

        # VideoWriter
        fourcc = cv2.VideoWriter_fourcc(*"mp4v")
        out = cv2.VideoWriter(output_video_path, fourcc, fps, (frame_width, frame_height))

        alpha_mask = np.zeros((frame_height, frame_width, 3), dtype=np.uint8)
        alpha_mask[:, :] = (0, 255, 0)  # 초록색

        timestamp_ms = 0  # 각 프레임의 타임스탬프 (밀리초)
        print("비디오 처리를 시작합니다...")
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                print("비디오 처리가 완료되었거나 스트림이 종료되었습니다.")
                break

            path_mask = np.zeros((frame_height, frame_width), dtype=np.uint8)

            # OpenCV 프레임(BGR)을 MediaPipe가 요구하는 RGB 형식으로 변환
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            # MediaPipe Image 객체로 변환
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)

            # 비디오 모드에서 포즈 감지 수행
            # detect_for_video 메소드의 두 번째 인자는 밀리초 단위의 타임스탬프여야 합니다.
            pose_landmarker_result = landmarker.detect_for_video(mp_image, int(timestamp_ms))
            timestamp_ms += int(1000 / fps)  # 다음 프레임 타임스탬프 계산

            # Landmark가 감지 될 경우, 프레임에 그리기
            if pose_landmarker_result and pose_landmarker_result.pose_landmarks:
                points = [
                    (int(lmk.x * frame_width), int(lmk.y * frame_height))
                    for lmk in pose_landmarker_result.pose_landmarks[0]
                ]

                for connection in mp_pose.POSE_CONNECTIONS:
                    start_idx, end_idx = connection
                    if start_idx < len(points) and end_idx < len(points):
                        pt1 = points[start_idx]
                        pt2 = points[end_idx]
                        cv2.line(path_mask, pt1, pt2, 255, 2)  # 경로 기록용 흑백 선

                for idx, pt in enumerate(points):
                    if idx in [11, 12, 13, 14, 15, 16, 23, 24, 25, 26, 27, 28]:
                        cv2.circle(frame, pt, 3, (0, 0, 255), -1)  # 빨간 점, 반지름 3

                # 투명한 컬러 오버레이 생성
                mask_colored = cv2.bitwise_and(alpha_mask, alpha_mask, mask=path_mask)
                overlayed_frame = cv2.addWeighted(frame, 1.0, mask_colored, 0.5, 0)  # 0.5 투명도

                out.write(overlayed_frame)
            else:
                out.write(frame)

        # 모든 작업 완료 후 리소스 해제
        cap.release()
        out.release()
    e_time = time.time()
    print(f"비디오 처리 완료. 총 시간: {e_time - s_time:.2f}초")
