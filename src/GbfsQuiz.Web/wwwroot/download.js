// Triggers a client-side file download from in-memory text (used for history export).
// Same-origin blob URL — compatible with the strict Content-Security-Policy.
window.downloadTextFile = (filename, mimeType, text) => {
    const blob = new Blob([text], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = filename;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    URL.revokeObjectURL(url);
};
