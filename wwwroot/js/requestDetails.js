async function loadDetails() {
    const card = document.getElementById("detailsCard");
    const actionArea = document.getElementById("actionArea");

    try {
        const res = await fetch(`/api/HelpRequests/${requestId}`);
        if (!res.ok) {
            card.innerHTML = `<div class="card-body text-danger"><h4>Request not found</h4><p>This request may have been deleted.</p><a href="/" class="btn btn-primary">Back to Home</a></div>`;
            return;
        }
        const r = await res.json();

        card.innerHTML = `
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-start flex-wrap">
                    <h3 class="mb-1">${escapeHtml(r.title)}</h3>
                    <span class="badge ${statusColor(r.status)} fs-6">${r.status}</span>
                </div>
                <span class="badge bg-secondary mb-3">${escapeHtml(r.categoryName)}</span>
                <p class="fs-5">${escapeHtml(r.description)}</p>
                <hr>
                <div class="row text-muted small">
                    <div class="col-md-6">
                        <p class="mb-1"><i class="bi bi-geo-alt text-danger"></i> ${escapeHtml(r.address || 'No location')}</p>
                        <p class="mb-1"><i class="bi bi-clock"></i> ${new Date(r.createdAt).toLocaleString()}</p>
                    </div>
                    <div class="col-md-6">
                        <p class="mb-1"><i class="bi bi-person"></i> Posted by <a href="/Account/Profile/${r.userId}">${escapeHtml(r.requesterName)}</a></p>
                        <p class="mb-1"><i class="bi bi-people"></i> ${r.volunteerCount} volunteer(s)</p>
                    </div>
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
            actionArea.innerHTML += `<span class="btn btn-outline-danger disabled"><i class="bi bi-x-circle"></i> Application rejected</span>`;
        } else if (r.status === "Open" || r.status === "Pending") {
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
        card.innerHTML = `<div class="card-body text-danger">Failed to load request details.</div>`;
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
            container.innerHTML = `<div class="card shadow-sm mt-3"><div class="card-body text-muted text-center py-3">No ratings yet</div></div>`;
            return;
        }

        let html = `<div class="card shadow-sm mt-3"><div class="card-header"><h5 class="mb-0"><i class="bi bi-star-fill text-warning"></i> Ratings & Reviews</h5></div><div class="list-group list-group-flush">`;
        ratings.forEach(r => {
            const stars = '★'.repeat(r.score) + '☆'.repeat(5 - r.score);
            html += `<div class="list-group-item">
                <div class="d-flex justify-content-between align-items-center">
                    <strong>${escapeHtml(r.raterName)}</strong>
                    <span class="text-warning">${stars}</span>
                </div>
                ${r.comment ? `<p class="mb-1 mt-1">${escapeHtml(r.comment)}</p>` : ''}
                ${r.reviewContent ? `<p class="mb-0 text-muted small">${escapeHtml(r.reviewContent)}</p>` : ''}
                <small class="text-muted">${new Date(r.createdAt).toLocaleDateString()}</small>
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
        case "Open": return "bg-success";
        case "Pending": return "bg-warning text-dark";
        case "Accepted": return "bg-primary";
        case "Completed": return "bg-secondary";
        case "Cancelled": return "bg-danger";
        default: return "bg-light text-dark";
    }
}

function escapeHtml(str) {
    if (!str) return '';
    const d = document.createElement("div");
    d.textContent = str;
    return d.innerHTML;
}

document.addEventListener("DOMContentLoaded", loadDetails);
