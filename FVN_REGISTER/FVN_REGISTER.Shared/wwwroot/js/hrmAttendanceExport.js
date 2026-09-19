window.hrmAttendanceExport = {
  download: function (bytes, fileName) {
    const blob = new Blob([new Uint8Array(bytes)], { type: "application/vnd.ms-excel" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    a.click();
    a.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
};