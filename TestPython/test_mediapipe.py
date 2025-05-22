import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import cv2
import numpy as np  # 프레임 복사 및 수정을 위해 추가

# MediaPipe 그리기 유틸리티 및 포즈 솔루션 임포트
from mediapipe.python.solutions import drawing_utils as mp_drawing
from mediapipe.python.solutions import pose as mp_pose

if __name__ == "__main__":
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
        fps = cap.get(cv2.CAP_PROP_FPS)

        timestamp_ms = 0  # 각 프레임의 타임스탬프 (밀리초)
        print("비디오 처리를 시작합니다...")
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                print("비디오 처리가 완료되었거나 스트림이 종료되었습니다.")
                break

            # OpenCV 프레임(BGR)을 MediaPipe가 요구하는 RGB 형식으로 변환
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            # MediaPipe Image 객체로 변환
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)

            # 비디오 모드에서 포즈 감지 수행
            # detect_for_video 메소드의 두 번째 인자는 밀리초 단위의 타임스탬프여야 합니다.
            pose_landmarker_result = landmarker.detect_for_video(mp_image, int(timestamp_ms))
            timestamp_ms += int(1000 / fps)  # 다음 프레임 타임스탬프 계산
            print(pose_landmarker_result.pose_landmarks[0])
            print(len(pose_landmarker_result.pose_landmarks[0]))
            print(pose_landmarker_result.pose_world_landmarks[0])
            print(len(pose_landmarker_result.pose_world_landmarks[0]))
            break

        # 모든 작업 완료 후 리소스 해제
        cap.release()
