window.cameraMockEnabled = false;

window.startVideo = async function (videoElementId) {

    const video = document.getElementById(videoElementId);
    const mock = document.getElementById("mockImage");

    if (!video) {
        return false;
    }

    // Estado inicial
    video.style.display = "none";

    if (mock) {
        mock.classList.add("d-none");
        mock.style.display = "none";
    }

    try {

        // Verificar soporte
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            throw new Error("Camera API not supported");
        }

        // Intentar abrir cámara
        const stream = await navigator.mediaDevices.getUserMedia({
            video: {
                facingMode: "user",
                width: { ideal: 1280 },
                height: { ideal: 720 }
            },
            audio: false
        });

        // Cámara OK
        video.srcObject = stream;

        await video.play();

        video.style.display = "block";

        if (mock) {
            mock.classList.add("d-none");
            mock.style.display = "none";
        }

        return true;

    } catch (error) {

        console.error("Error accessing camera:", error);

        // SI FALLA -> MOSTRAR MOCK
        video.style.display = "none";

        if (mock) {
            mock.classList.remove("d-none");
            mock.style.display = "block";
        }

        return false;
    }
};

window.captureFrame = async function (videoElementId, canvasElementId) {

    const video = document.getElementById(videoElementId);
    const canvas = document.getElementById(canvasElementId);
    const mock = document.getElementById("mockImage");

    if (!canvas) {
        return null;
    }

    const context = canvas.getContext("2d");

    // ============================
    // CAPTURA REAL
    // ============================
    if (video && video.srcObject) {

        const size = Math.min(Math.min(video.videoWidth, video.videoHeight) * 0.5, 600);

        const x = (video.videoWidth - size) / 2;
        const y = (video.videoHeight - size) / 2;

        canvas.width = size;
        canvas.height = size;

        context.clearRect(0, 0, canvas.width, canvas.height);

        context.save();

        context.beginPath();
        context.arc(size / 2, size / 2, size / 2, 0, 2 * Math.PI);
        context.closePath();
        context.clip();

        context.drawImage(
            video,
            x,
            y,
            size,
            size,
            0,
            0,
            size,
            size
        );

        context.restore();

        return canvas.toDataURL("image/jpeg", 0.85);
    }

    // ============================
    // CAPTURA MOCK
    // ============================
    if (mock) {

        const img = new Image();
        img.src = "/images/mock-persona.png";

        await new Promise((resolve, reject) => {
            img.onload = resolve;
            img.onerror = reject;
        });

        const size = Math.min(img.width, img.height);

        canvas.width = size;
        canvas.height = size;

        context.clearRect(0, 0, canvas.width, canvas.height);

        context.save();

        context.beginPath();
        context.arc(size / 2, size / 2, size / 2, 0, 2 * Math.PI);
        context.closePath();
        context.clip();

        context.drawImage(
            img,
            0,
            0,
            size,
            size
        );

        context.restore();

        return canvas.toDataURL("image/jpeg", 0.85);
    }

    return null;
};

window.stopVideo = function (videoElementId) {

    const video = document.getElementById(videoElementId);

    if (!video || !video.srcObject) {
        return;
    }

    const tracks = video.srcObject.getTracks();

    tracks.forEach(track => track.stop());

    video.srcObject = null;
};