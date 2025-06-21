mergeInto(LibraryManager.library, {
    OpenFileSelection: function () {
        var input = document.createElement('input');
        input.type = 'file';
        input.accept = 'video/*';

        input.onchange = function (event) {
            var file = event.target.files[0];
            if (file) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var arrayBuffer = e.target.result;
                    var byteArray = new Uint8Array(arrayBuffer);
                    var blob = new Blob([byteArray], { type: file.type });
                    var blobUrl = URL.createObjectURL(blob);

                    // ✅ Delay SendMessage by 1 second to give Unity time to load
                    setTimeout(function () {
                        SendMessageToUnity(blobUrl);
                    }, 2000);

                    // ✅ Unity 인스턴스가 준비되었는지 확인 후 호출
                    if (typeof unityInstance !== 'undefined' && unityInstance !== null) {
                        unityInstance.SendMessage('VideoManager', 'OnVideoSelected', blobUrl);
                    } else {
                        console.error("Unity instance is not ready yet.");
                    }
                };
                reader.readAsArrayBuffer(file);
            }
        };
        input.click();
    }
});
