// File: FVN_REGISTER.Shared/wwwroot/js/deviceId.js
window.getOrCreateDeviceId = function () {
    let id = localStorage.getItem('fvn_device_id');
    if (!id) {
        id = crypto.randomUUID();
        localStorage.setItem('fvn_device_id', id);
    }
    return id;
};