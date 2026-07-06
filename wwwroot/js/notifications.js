// Global real-time notifications (runs on every page for logged-in users)
(function () {
    const toastContainer = document.getElementById("toastContainer");
    if (!toastContainer || typeof signalR === "undefined") return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", (message) => {
        showToast(message);
        bumpNotifCount();
    });

    connection.start().catch(err => console.warn("Notification hub not connected:", err.message));

    function showToast(message) {
        const toastEl = document.createElement("div");
        toastEl.className = "toast align-items-center text-bg-primary border-0";
        toastEl.setAttribute("role", "alert");
        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>`;
        toastContainer.appendChild(toastEl);
        const toast = new bootstrap.Toast(toastEl, { delay: 6000 });
        toast.show();
        toastEl.addEventListener("hidden.bs.toast", () => toastEl.remove());
    }

    function bumpNotifCount() {
        const badge = document.getElementById("notifCount");
        if (!badge) return;
        const current = parseInt(badge.textContent || "0", 10) || 0;
        badge.textContent = current + 1;
        badge.classList.remove("d-none");
    }
})();
