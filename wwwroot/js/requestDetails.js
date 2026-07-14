let CurrentUID = "";

async function initAndLoad() {
    try {
        const res = await fetch('/api/UserProfile');
        if (res.ok) {
            const user = await res.json();
            CurrentUID = user.userId || "";
        }
    } catch { }
    loadDetails();
}

async function loadDetails() {
    const card = document.getElementById("detailsCard");
    const actionArea = document.getElementById("actionArea");

    try {
        const res = await fetch(`/api/HelpRequests/${requestId}`);
        if (!res.ok) {
            card.innerHTML = `
                <div class="text-center py-4">
                    <i class="bi bi-exclamation-circle text-danger fs-1 mb-3 d-block"></i>
                    <h4 style="color: var(--text-primary); font-weight: 700;">Request not found</h4>
                    <p style="color: var(--text-muted); font-size: 0.9rem;" class="mb-4">This request may have been deleted or is unavailable.</p>
                    <a href="/" class="btn btn-primary px-4" style="border-radius: var(--radius-sm);">Back to Home</a>
                </div>`;
            return;
        }
        const r = await res.json();
        const displayStatus =
            r.currentUserVolunteerStatus === "Pending"  ? "Pending"  :
            r.currentUserVolunteerStatus === "Rejected" ? "Rejected" :
            r.status;

        card.innerHTML = `
            <div class="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-2">
                <h3 class="mb-0" style="color: var(--text-primary); font-weight: 800;">${escapeHtml(r.title)}</h3>
                <span class="badge ${statusColor(displayStatus)} fs-6" style="padding: 0.5em 1em; border-radius: 999px;">${displayStatus}</span>
            </div>
            <span class="badge bg-secondary mb-3" style="font-weight: 600; padding: 0.4em 0.8em; border-radius: var(--radius-sm);">${escapeHtml(r.categoryName)}</span>
            <p class="fs-5 mt-2" style="color: var(--text-secondary); line-height: 1.6;">${escapeHtml(r.description)}</p>
            <hr style="border-top: 1px solid var(--border); opacity: 1; margin: 1.5rem 0;">
            <div class="row text-muted small g-3">
                <div class="col-md-6">
                    <p class="mb-2" style="color: var(--text-muted);"><i class="bi bi-geo-alt text-danger me-2"></i> ${escapeHtml(r.address || 'No location')}</p>
                    <p class="mb-0" style="color: var(--text-muted);"><i class="bi bi-clock me-2"></i> ${new Date(r.createdAt).toLocaleString()}</p>
                </div>
                <div class="col-md-6">
                    <p class="mb-2" style="color: var(--text-muted);"><i class="bi bi-person me-2"></i> Posted by <a href="/Account/Profile/${r.userId}" style="color: var(--accent); text-decoration: none; font-weight: 600;">${escapeHtml(r.requesterName)}</a></p>
                    <p class="mb-0" style="color: var(--text-muted);"><i class="bi bi-people me-2"></i> ${r.volunteerCount} volunteer(s)</p>
                </div>
            </div>`;

        // action buttons
        actionArea.innerHTML = "";
        const isOwner = CurrentUID && CurrentUID === r.userId;

        if (isOwner) {
            actionArea.innerHTML += `<a href="/HelpRequests/ManageVolunteers/${requestId}" class="btn btn-primary"><i class="bi bi-people"></i> Manage Volunteers</a>`;
            if (r.status === "Accepted") {
                actionArea.innerHTML += `<a href="/Chat/${requestId}" class="btn btn-outline-primary"><i class="bi bi-chat-dots"></i> Chat</a>`;
            }
        } else if (r.currentUserVolunteerStatus === "Accepted") {
            actionArea.innerHTML += `<a href="/Chat/${requestId}" class="btn btn-primary"><i class="bi bi-chat-dots"></i> Open Chat</a>`;
        } else if (r.currentUserVolunteerStatus === "Pending") {
            actionArea.innerHTML += `<span class="btn btn-outline-secondary disabled"><i class="bi bi-hourglass-split"></i> Application pending</span>`;
        } else if (r.currentUserVolunteerStatus === "Rejected") {
            actionArea.innerHTML += `<span class="btn btn-outline-danger disabled"><i class="bi bi-x-circle"></i> Application Rejected</span>`;
        } else if (r.status !== "Open" && r.status !== "Pending") {
            actionArea.innerHTML += `<span class="btn btn-outline-secondary disabled"><i class="bi bi-lock"></i> Request Closed</span>`;
        } else {
            actionArea.innerHTML += `<button class="btn btn-success" onclick="volunteerFor(this)"><i class="bi bi-hand-thumbs-up"></i> Volunteer</button>`;
        }

        // rate button (only if accepted volunteer and request owner)
        if (r.status === "Accepted" || r.status === "Completed") {
            if (isOwner) {
                // owner can rate the accepted volunteer
                const volRes = await fetch(`/api/Volunteer/request/${requestId}`);
                if (volRes.ok) {
                    const vols = await volRes.json();
                    const accepted = vols.find(v => v.status === "Accepted");
                    if (accepted) {
                        actionArea.innerHTML += `<a href="/Rate/${requestId}?userId=${accepted.userId}&name=${encodeURIComponent(accepted.userName || 'Volunteer')}" class="btn btn-warning"><i class="bi bi-star"></i> Rate Volunteer</a>`;
                    }
                }
            } else if (r.currentUserVolunteerStatus === "Accepted") {
                // volunteer can rate the owner
                actionArea.innerHTML += `<a href="/Rate/${requestId}?userId=${r.userId}&name=${encodeURIComponent(r.requesterName)}" class="btn btn-warning"><i class="bi bi-star"></i> Rate Requester</a>`;
            }
        }

        // load ratings for this request
        loadRatings();

    } catch (err) {
        card.innerHTML = `<div class="card-body text-center py-4">
            <i class="bi bi-exclamation-circle text-danger fs-2 mb-2 d-block"></i>
            <p class="text-danger mb-3">Failed to load request details.</p>
            <button class="btn btn-outline-primary btn-sm" onclick="loadDetails()"><i class="bi bi-arrow-clockwise me-1"></i>Retry</button>
        </div>`;
    }
}

async function loadRatings() {
    const container = document.getElementById("ratingsSection");
    if (!container) return;

    try {
        const res = await fetch(`/api/Ratings/request/${requestId}`);
        if (!res.ok) return;
        const ratings = await res.json();
        if (!ratings.length) {
            container.innerHTML = `<div class="nh-card mt-3"><div class="card-body text-muted text-center py-3">No ratings yet</div></div>`;
            return;
        }

        let html = `
        <div class="nh-card p-4 mt-3">
            <h5 class="mb-3" style="color: var(--text-primary); font-weight: 700; font-size: 1.1rem; border-bottom: 1px solid var(--border); padding-bottom: 0.75rem;">
                <i class="bi bi-star-fill text-warning me-2"></i>Ratings & Reviews
            </h5>
            <div class="d-flex flex-column gap-3">`;

        ratings.forEach(r => {
            const stars = '★'.repeat(r.score) + '☆'.repeat(5 - r.score);
            html += `
                <div class="p-3 rounded" style="background: rgba(255, 255, 255, 0.02); border: 1px solid var(--border);">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <strong style="color: var(--text-primary); font-size: 0.95rem;">${escapeHtml(r.raterName)}</strong>
                        <span class="text-warning" style="letter-spacing: 2px;">${stars}</span>
                    </div>
                    ${r.comment ? `<p class="mb-1" style="color: var(--text-secondary); font-size: 0.9rem;">${escapeHtml(r.comment)}</p>` : ''}
                    ${r.reviewContent ? `<p class="mb-2 text-muted" style="font-size: 0.82rem;">${escapeHtml(r.reviewContent)}</p>` : ''}
                    <small class="text-muted" style="font-size: 0.78rem;"><i class="bi bi-calendar3 me-1"></i>${new Date(r.createdAt).toLocaleDateString()}</small>
                </div>`;
        });
        html += `</div></div>`;
        container.innerHTML = html;
    } catch {}
}

async function volunteerFor(btn) {
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Applying...`;
    try {
        const res = await fetch(`/api/Volunteer/apply/${requestId}`, { method: "POST" });
        if (res.ok) {
            btn.innerHTML = `<i class="bi bi-check-circle"></i> Applied!`;
            btn.className = "btn btn-outline-secondary disabled";
            if (window.nhToast) window.nhToast("You have volunteered!");
            setTimeout(() => location.reload(), 1200);
        } else if (res.status === 401) {
            window.location.href = "/Account/Login";
        } else {
            let msg = "Could not apply.";
            try { const e = await res.json(); msg = e.message || msg; } catch {}
            if (window.nhToast) window.nhToast(msg, "error");
            btn.disabled = false;
            btn.innerHTML = `<i class="bi bi-hand-thumbs-up"></i> Volunteer`;
        }
    } catch {
        if (window.nhToast) window.nhToast("Network error.", "error");
        btn.disabled = false;
        btn.innerHTML = `<i class="bi bi-hand-thumbs-up"></i> Volunteer`;
    }
}

function statusColor(s) {
    switch(s) {
        case "Open":      return "bg-success";
        case "Pending":   return "bg-warning text-dark";
        case "Accepted":  return "bg-primary";
        case "Completed": return "bg-secondary";
        case "Cancelled": return "bg-danger";
        case "Rejected":  return "bg-danger";
        default: return "bg-light text-dark";
    }
}

function escapeHtml(str) {
    if (!str) return '';
    const d = document.createElement("div");
    d.textContent = str;
    return d.innerHTML;
}

document.addEventListener("DOMContentLoaded", initAndLoad);

