let streams = new WeakMap();

export async function startQrScanner(video) {
    if (!('BarcodeDetector' in window)) return null;
    const detector = new BarcodeDetector({ formats: ['qr_code'] });
    const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' } }, audio: false });
    streams.set(video, stream); video.srcObject = stream; await video.play();
    return await new Promise(resolve => {
        let stopped = false;
        const tick = async () => {
            if (stopped) return;
            try {
                const codes = await detector.detect(video);
                if (codes.length && codes[0].rawValue) { stopped = true; stopQrScanner(video); resolve(codes[0].rawValue); return; }
            } catch { }
            requestAnimationFrame(tick);
        };
        tick();
    });
}

export function stopQrScanner(video) {
    const stream = streams.get(video);
    if (stream) { stream.getTracks().forEach(t => t.stop()); streams.delete(video); }
    video.srcObject = null;
}
