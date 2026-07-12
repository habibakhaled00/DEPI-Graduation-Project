const container = document.getElementById("applicantsContainer");
const template = document.getElementById("applicantRowTemplate");

async function loadApplicants() {
    try {
        const res = await fetch(`/api/Volunteer/request/${requestId}`);
        if (!res.ok) {
            container.innerHTML = `<p class="text-danger">Failed to load applicants (are you the request owner?).</p>`;
            return;
        }
        const applicants = await res.json();
        renderApplicants(applicants);
    } catch (err) {
        container.innerHTML = `<p class="text-danger">Network error loading applicants.</p>`;
    }
}

function renderApplicants(applicants) {
    container.innerHTML = "";

    if (applicants.length === 0) {
        container.innerHTML = `
            <div class="nh-card text-center py-5 no-hover">
                <i class="bi bi-people fs-1 d-block mb-3 style="color: var(--text-muted);"></i>
                <h5 style="color: var(--text-primary);">No applicants yet</h5>
                <p class="mb-0 text-muted" style="font-size: 0.875rem;">Applications will appear here once neighbors volunteer.</p>
            </div>`;
        return;
    }

    const hasAnyAccepted = applicants.some(a => a.status === "Accepted");

    applicants.forEach((a, idx) => {
        const node = template.content.cloneNode(true);
        node.querySelector(".applicant-name").textContent = a.userName;
        node.querySelector(".applicant-date").textContent = `Applied ${new Date(a.appliedDate).toLocaleString()}`;

        const statusBadge = node.querySelector(".applicant-status");
        statusBadge.textContent = a.status;
        statusBadge.classList.add(...statusClasses(a.status));

        const actions = node.querySelector(".applicant-actions");
        if (a.status !== "Pending") {
            actions.innerHTML = `<span class="text-muted small fst-italic" style="font-size: 0.8rem; opacity: 0.7;">${a.status}</span>`;
            actions.style.background = "transparent";
            actions.style.border = "none";
        } else {
            const acceptBtn = node.querySelector(".accept-btn");
            const rejectBtn = node.querySelector(".reject-btn");

            if (hasAnyAccepted) {
                acceptBtn.disabled = true;
                acceptBtn.classList.replace("btn-success", "btn-outline-secondary");
                acceptBtn.title = "Cancel the active volunteer match first to accept another candidate.";
            } else {
                acceptBtn.addEventListener("click", (e) => acceptVolunteer(a.volunteerId, e.currentTarget));
            }
            
            rejectBtn.addEventListener("click", (e) => rejectVolunteer(a.volunteerId, e.currentTarget));
        }

        const cardEl = node.querySelector(".nh-card");
        if (cardEl) {
            cardEl.style.animationDelay = `${idx * 70}ms`;
        }

        container.appendChild(node);
    });

    if (window.initStagger) window.initStagger();
}

function statusClasses(status) {
    const map = {
        'Pending':   ['status-badge', 'status-pending'],
        'Accepted':  ['status-badge', 'status-accepted'],
        'Rejected':  ['status-badge', 'status-completed'], // re-use badge
    };
    return map[status] || ['status-badge', 'status-pending'];
}

function showConfirm(title, message, onConfirm) {
    const modal = document.getElementById("confirmModal");
    if (!modal) {
        if (confirm(message)) onConfirm();
        return;
    }

    document.getElementById("confirmModalTitle").innerHTML = 
        `<i class="bi bi-exclamation-triangle-fill me-2" style="color:var(--accent-3)"></i>${title}`;
    document.getElementById("confirmModalBody").textContent = message;

    const okBtn = document.getElementById("confirmOkBtn");
    const cancelBtn = document.getElementById("confirmCancelBtn");

    const cleanup = () => {
        modal.style.display = "none";
        okBtn.removeEventListener("click", onOk);
        cancelBtn.removeEventListener("click", onCancel);
    };

    const onOk = () => {
        cleanup();
        onConfirm();
    };

    const onCancel = () => {
        cleanup();
    };

    okBtn.addEventListener("click", onOk);
    cancelBtn.addEventListener("click", onCancel);
    modal.style.display = "flex";
}

function acceptVolunteer(volunteerId, btn) {
    showConfirm(
        "Accept Volunteer?",
        "Accept this volunteer? Other pending applicants will be automatically rejected.",
        async () => {
            btn.disabled = true;
            const oldText = btn.innerHTML;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span> Processing...`;

            try {
                const res = await fetch(`/api/Volunteer/accept/${volunteerId}`, { method: "PUT" });
                if (res.ok) {
                    if (window.nhToast) window.nhToast("Volunteer accepted successfully!");
                    await loadApplicants();
                } else {
                    const errText = await res.text() || "Could not accept volunteer.";
                    if (window.nhToast) window.nhToast(errText, "error");
                    btn.disabled = false;
                    btn.innerHTML = oldText;
                }
            } catch {
                if (window.nhToast) window.nhToast("Network error. Please try again.", "error");
                btn.disabled = false;
                btn.innerHTML = oldText;
            }
        }
    );
}

function rejectVolunteer(volunteerId, btn) {
    showConfirm(
        "Reject Application?",
        "Reject this volunteer's application?",
        async () => {
            btn.disabled = true;
            const oldText = btn.innerHTML;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span> Processing...`;

            try {
                const res = await fetch(`/api/Volunteer/reject/${volunteerId}`, { method: "PUT" });
                if (res.ok) {
                    if (window.nhToast) window.nhToast("Volunteer declined.");
                    await loadApplicants();
                } else {
                    const errText = await res.text() || "Could not decline volunteer.";
                    if (window.nhToast) window.nhToast(errText, "error");
                    btn.disabled = false;
                    btn.innerHTML = oldText;
                }
            } catch {
                if (window.nhToast) window.nhToast("Network error. Please try again.", "error");
                btn.disabled = false;
                btn.innerHTML = oldText;
            }
        }
    );
}

document.addEventListener("DOMContentLoaded", loadApplicants);
