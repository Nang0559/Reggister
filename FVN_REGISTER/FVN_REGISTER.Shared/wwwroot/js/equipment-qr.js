const sessions = new WeakMap();
const pendingStarts = new WeakMap();

function unsupportedResult(code, message) { return { ok: false, value: null, code, message }; }

export async function startQrScanner(video) {
    if (!window.isSecureContext) return unsupportedResult('secure-context', 'Camera chỉ hoạt động trong HTTPS hoặc localhost (secure context).');
    if (!navigator.mediaDevices || typeof navigator.mediaDevices.getUserMedia !== 'function') return unsupportedResult('camera-unsupported', 'Trình duyệt không hỗ trợ navigator.mediaDevices.getUserMedia().');
    if (!('BarcodeDetector' in window)) return unsupportedResult('qr-unsupported', 'Trình duyệt có camera nhưng không hỗ trợ đọc QR trực tiếp (BarcodeDetector).');

    await stopQrScanner(video);
    const pending = { cancelled: false };
    pendingStarts.set(video, pending);

    let stream;
    try {
        stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' } }, audio: false });
    } catch (error) {
        if (pendingStarts.get(video) === pending) pendingStarts.delete(video);
        const name = error?.name || '';
        if (pending.cancelled) return unsupportedResult('stopped', null);
        if (name === 'NotAllowedError' || name === 'PermissionDeniedError') return unsupportedResult('permission-denied', 'Bạn đã từ chối quyền sử dụng camera. Hãy cấp lại quyền camera cho trang rồi thử lại.');
        if (name === 'NotFoundError' || name === 'DevicesNotFoundError') return unsupportedResult('camera-not-found', 'Không tìm thấy camera trên thiết bị.');
        if (name === 'NotReadableError' || name === 'TrackStartError') return unsupportedResult('camera-busy', 'Camera đang được ứng dụng khác sử dụng hoặc không thể khởi động.');
        return unsupportedResult('camera-error', `Không thể mở camera${name ? ` (${name})` : ''}.`);
    }

    pendingStarts.delete(video);
    if (pending.cancelled) {
        stream.getTracks().forEach(track => { try { track.stop(); } catch { } });
        return unsupportedResult('stopped', null);
    }

    const detector = new BarcodeDetector({ formats: ['qr_code'] });
    const session = { stream, stopped: false, resolve: null, raf: 0 };
    sessions.set(video, session);
    video.srcObject = stream;
    video.muted = true;
    video.autoplay = true;
    video.playsInline = true;

    try { await video.play(); }
    catch {
        await stopQrScanner(video);
        return unsupportedResult('video-play-failed', 'Không thể phát luồng camera trên trang.');
    }

    return await new Promise(resolve => {
        session.resolve = resolve;
        const tick = async () => {
            if (sessions.get(video) !== session || session.stopped) return;
            try {
                const codes = await detector.detect(video);
                const value = codes?.find(x => x?.rawValue)?.rawValue;
                if (value) {
                    await stopQrScanner(video, { result: { ok: true, value, code: 'detected', message: null } });
                    return;
                }
            } catch { /* transient detector error: keep scanning */ }
            if (!session.stopped) session.raf = requestAnimationFrame(tick);
        };
        session.raf = requestAnimationFrame(tick);
    });
}

export async function stopQrScanner(video, options = null) {
    const pending = pendingStarts.get(video);
    if (pending) {
        pending.cancelled = true;
        pendingStarts.delete(video);
    }

    const session = sessions.get(video);
    if (!session) {
        if (video) {
            try { video.pause(); video.srcObject = null; } catch { }
        }
        return;
    }

    session.stopped = true;
    if (session.raf) cancelAnimationFrame(session.raf);
    sessions.delete(video);
    session.stream?.getTracks().forEach(track => { try { track.stop(); } catch { } });
    try { video.pause(); video.srcObject = null; } catch { }

    if (session.resolve) {
        session.resolve(options?.result ?? { ok: false, value: null, code: 'stopped', message: null });
        session.resolve = null;
    }
}
