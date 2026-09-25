let streams = new WeakMap();
let decoderPromise = null;

function getCameraError(error) {
    if (!error) return "Không thể mở camera.";

    switch (error.name) {
        case "NotAllowedError":
        case "PermissionDeniedError":
            return "Trình duyệt chưa được cấp quyền camera. Hãy cho phép Camera rồi thử lại.";
        case "NotFoundError":
        case "DevicesNotFoundError":
            return "Không tìm thấy camera trên thiết bị.";
        case "NotReadableError":
        case "TrackStartError":
            return "Camera đang được ứng dụng khác sử dụng hoặc không thể truy cập.";
        case "OverconstrainedError":
            return "Không tìm được camera phù hợp trên thiết bị.";
        case "SecurityError":
            return "Trình duyệt đang chặn quyền truy cập camera.";
        default:
            return error.message || "Không thể mở camera.";
    }
}

async function loadJsQr() {
    if (typeof window.jsQR === "function") {
        return window.jsQR;
    }

    if (!decoderPromise) {
        decoderPromise = new Promise((resolve, reject) => {
            const existing = document.querySelector('script[data-fvn-jsqr="1"]');

            if (existing) {
                if (typeof window.jsQR === "function") {
                    resolve(window.jsQR);
                    return;
                }

                existing.addEventListener("load", () => {
                    if (typeof window.jsQR === "function") resolve(window.jsQR);
                    else reject(new Error("Không tải được bộ giải mã QR."));
                }, { once: true });

                existing.addEventListener("error", () => {
                    reject(new Error("Không tải được bộ giải mã QR."));
                }, { once: true });

                return;
            }

            const script = document.createElement("script");
            script.src = "https://cdn.jsdelivr.net/npm/jsqr@1.4.0/dist/jsQR.js";
            script.async = true;
            script.dataset.fvnJsqr = "1";

            script.onload = () => {
                if (typeof window.jsQR === "function") {
                    resolve(window.jsQR);
                } else {
                    reject(new Error("Bộ giải mã QR không sẵn sàng."));
                }
            };

            script.onerror = () => {
                reject(new Error("Không thể tải bộ giải mã QR. Kiểm tra kết nối mạng hoặc chính sách Content-Security-Policy."));
            };

            document.head.appendChild(script);
        });
    }

    return decoderPromise;
}

async function openCamera(video) {
    if (!video) throw new Error("Không tìm thấy vùng hiển thị camera.");

    if (!window.isSecureContext) {
        throw new Error("Camera chỉ hoạt động trên HTTPS hoặc localhost.");
    }

    if (!navigator.mediaDevices?.getUserMedia) {
        throw new Error("Trình duyệt không hỗ trợ truy cập camera.");
    }

    try {
        // Ưu tiên camera sau trên điện thoại, nhưng vẫn cho phép browser tự chọn
        // nếu thiết bị không có camera sau.
        return await navigator.mediaDevices.getUserMedia({
            video: {
                facingMode: { ideal: "environment" },
                width: { ideal: 1280 },
                height: { ideal: 720 }
            },
            audio: false
        });
    } catch (error) {
        throw new Error(getCameraError(error));
    }
}

function stopQrScanner(video) {
    const stream = streams.get(video);

    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        streams.delete(video);
    }

    if (video) {
        video.srcObject = null;
    }
}

async function scanWithBarcodeDetector(video, detector) {
    try {
        const codes = await detector.detect(video);
        const value = codes?.find(x => x.rawValue)?.rawValue;
        return value || null;
    } catch {
        return null;
    }
}

async function scanWithJsQr(video, jsQR, canvas, context) {
    if (video.readyState < HTMLMediaElement.HAVE_CURRENT_DATA ||
        !video.videoWidth ||
        !video.videoHeight) {
        return null;
    }

    // Giảm kích thước frame để không làm nghẽn UI/Blazor Server.
    const maxWidth = 960;
    const scale = Math.min(1, maxWidth / video.videoWidth);
    const width = Math.max(1, Math.round(video.videoWidth * scale));
    const height = Math.max(1, Math.round(video.videoHeight * scale));

    if (canvas.width !== width || canvas.height !== height) {
        canvas.width = width;
        canvas.height = height;
    }

    context.drawImage(video, 0, 0, width, height);

    const imageData = context.getImageData(0, 0, width, height);
    const result = jsQR(imageData.data, imageData.width, imageData.height, {
        inversionAttempts: "attemptBoth"
    });

    return result?.data || null;
}

export async function startQrScanner(video) {
    if (streams.has(video)) {
        stopQrScanner(video);
    }

    const stream = await openCamera(video);
    streams.set(video, stream);
    video.srcObject = stream;

    try {
        await video.play();
    } catch (error) {
        stopQrScanner(video);
        throw new Error("Không thể phát hình ảnh từ camera: " + (error?.message || ""));
    }

    // BarcodeDetector là đường quét nhanh nếu browser hỗ trợ.
    // Nếu không hỗ trợ, tự động chuyển sang jsQR.
    let barcodeDetector = null;

    if ("BarcodeDetector" in window) {
        try {
            barcodeDetector = new BarcodeDetector({ formats: ["qr_code"] });
        } catch {
            barcodeDetector = null;
        }
    }

    let jsQR = null;
    let canvas = null;
    let context = null;

    if (!barcodeDetector) {
        jsQR = await loadJsQr();

        canvas = document.createElement("canvas");
        context = canvas.getContext("2d", { willReadFrequently: true });

        if (!context) {
            stopQrScanner(video);
            throw new Error("Không khởi tạo được bộ xử lý hình ảnh QR.");
        }
    }

    return await new Promise((resolve, reject) => {
        let stopped = false;

        const finish = (value, error) => {
            if (stopped) return;
            stopped = true;
            stopQrScanner(video);

            if (error) reject(error);
            else resolve(value);
        };

        let lastScan = 0;

        const tick = async (timestamp) => {
            if (stopped) return;

            // Khoảng 8-10 lần/giây là đủ cho QR camera và nhẹ hơn nhiều
            // so với requestAnimationFrame liên tục.
            if (timestamp - lastScan >= 110) {
                lastScan = timestamp;

                try {
                    const value = barcodeDetector
                        ? await scanWithBarcodeDetector(video, barcodeDetector)
                        : await scanWithJsQr(video, jsQR, canvas, context);

                    if (value) {
                        finish(value.trim(), null);
                        return;
                    }
                } catch (error) {
                    finish(null, error);
                    return;
                }
            }

            requestAnimationFrame(tick);
        };

        requestAnimationFrame(tick);
    });
}

export { stopQrScanner };
