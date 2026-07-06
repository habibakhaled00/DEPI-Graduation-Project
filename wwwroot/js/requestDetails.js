async function loadDetails() {
    const card = document.getElementById("detailsCard");
    const actionArea = document.getElementById("actionArea");

    try {
        const res = await fetch(`/api/HelpRequests/${requestId}`);
        if (!res.ok) {
            card.innerHTML = `<div class="card-body text-danger">Request not found.</div>`;
            return;
        }
        const r = await res.json();

        card.innerHTML = `
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-start">
                    <h3>${escapeHtml(r.title)}</h3>
                    <span class="badge ${statusColor(r.status)} fs-6">${r.status}</span>
                </div>
                <p class="text-muted mb-3">${escapeHtml(r.categoryName)}</p>
                <p>${escapeHtml(r.description)}</p>
                <hr>
                <p class="mb-1"><i class="bi bi-geo-alt"></i> ${escapeHtml(r.address)}</p>
                <p class="mb-1"><i class="bi bi-person"></i> Posted by ${escapeHtml(r.requesterName)}</p>
                <p class="text-muted small mb-0"><i class="bi bi-clock"></i> ${new Date(r.createdAt).toLocaleString()}</p>
                <p class="text-muted small"><i class="bi bi-people"></i> ${r.volunteerCount} applicant(s)</p>
            </div>`;

        actionArea.innerHTML = "";

        const isOwner = CurrentUID && CurrentUID === r.userId;

        if (isOwner) {
            const manageBtn = document.createElement("a");
            manageBtn.href = `/HelpRequests/ManageVolunteers/${requestId}`;
            manageBtn.className = "btn btn-primary";
            manageBtn.innerHTML = `<i class="bi bi-people"></i> Manage Volunteers`;
            actionArea.appendChild(manageBtn);

            if (r.status === "Accepted" || r.status === "Completed") {
                const chatBtn = document.createElement("a");
                chatBtn.href = `/Chat/${requestId}`;
                chatBtn.className = "btn btn-outline-primary";
                chatBtn.innerHTML = `<i class="bi bi-chat-dots"></i> Open Chat`;
                actionArea.appendChild(chatBtn);
            }
        } else if (r.currentUserVolunteerStatus === "Accepted") {
            const chatBtn = document.createElement("a");
            chatBtn.href = `/Chat/${requestId}`;
            chatBtn.className = "btn btn-outline-primary";
            chatBtn.innerHTML = `<i class="bi bi-chat-dots"></i> Open Chat`;
            actionArea.appendChild(chatBtn);
        } else if (r.currentUserVolunteerStatus === "Pending") {
            const pending = document.createElement("span");
            pending.className = "btn btn-outline-secondary disabled";
            pending.innerHTML = `<i class="bi bi-hourglass-split"></i> Application pending`;
            actionArea.appendChild(pending);
        } else if (r.currentUserVolunteerStatus === "Rejected") {
            const rejected = document.createElement("span");
            rejected.className = "btn btn-outline-danger disabled";
            rejected.innerHTML = `<i class="bi bi-x-circle"></i> Application rejected`;
            actionArea.appendChild(rejected);
        } else if (r.status === "Open") {
            const volBtn = document.createElement("button");
            volBtn.className = "btn btn-success";
            volBtn.innerHTML = `<i class="bi bi-hand-thumbs-up"></i> Volunteer for this Request`;
            volBtn.addEventListener("click", () => volunteerFor(volBtn));
            actionArea.appendChild(volBtn);
        } else {
            const unavailable = document.createElement("span");
            unavailable.className = "btn btn-outline-secondary disabled";
            unavailable.innerHTML = `<i class="bi bi-lock"></i> Not available`;
            actionArea.appendChild(unavailable);
        }
    } catch (err) {
        card.innerHTML = `<div class="card-body text-danger">Failed to load request details.</div>`;
    }
}

async function volunteerFor(btn) {
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Applying...`;
    try {
        const res = await fetch(`/api/Volunteer/apply/${requestId}`, { method: "POST" });
        if (res.ok) {
            btn.innerHTML = `<i class="bi bi-check-circle"></i> Applied`;
        } else if (res.status === 401) {
            window.location.href = "/Identity/Account/Login";
        } else {
            alert(await res.text());
            btn.disabled = false;
            btn.innerHTML = `<i class="bi bi-hand-thumbs-up"></i> Volunteer for this Request`;
        }
    } catch {
        alert("Network error.");
        btn.disabled = false;
    }
}

function statusColor(status) {
    switch (status) {
        case "Open": return "bg-success";
        case "Pending": return "bg-warning text-dark";
        case "Accepted": return "bg-primary";
        case "Completed": return "bg-secondary";
        case "Cancelled": return "bg-danger";
        default: return "bg-light text-dark";
    }
}

function escapeHtml(str) {
    const div = document.createElement("div");
    div.textContent = str;
    return div.innerHTML;
}

document.addEventListener("DOMContentLoaded", loadDetails);
