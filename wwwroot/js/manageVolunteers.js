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
        container.innerHTML = `<p class="text-muted text-center py-4">No one has applied yet.</p>`;
        return;
    }

    applicants.forEach(a => {
        const node = template.content.cloneNode(true);
        node.querySelector(".applicant-name").textContent = a.userName;
        node.querySelector(".applicant-date").textContent = `Applied ${new Date(a.appliedDate).toLocaleString()}`;

        const statusBadge = node.querySelector(".applicant-status");
        statusBadge.textContent = a.status;
        statusBadge.classList.add(...statusClasses(a.status));

        const actions = node.querySelector(".applicant-actions");
        if (a.status !== "Pending") {
            actions.innerHTML = `<span class="text-muted small fst-italic">No actions available</span>`;
        } else {
            node.querySelector(".accept-btn").addEventListener("click", (e) => acceptVolunteer(a.volunteerId, e.target));
            node.querySelector(".reject-btn").addEventListener("click", (e) => rejectVolunteer(a.volunteerId, e.target));
        }

        container.appendChild(node);
    });
}

function statusClasses(status) {
    switch (status) {
        case "Pending": return ["bg-warning", "text-dark"];
        case "Accepted": return ["bg-success"];
        case "Rejected": return ["bg-danger"];
        default: return ["bg-light", "text-dark"];
    }
}

async function acceptVolunteer(volunteerId, btn) {
    if (!confirm("Accept this volunteer? Other pending applicants will be automatically rejected.")) return;
    btn.disabled = true;
    try {
        const res = await fetch(`/api/Volunteer/accept/${volunteerId}`, { method: "PUT" });
        if (res.ok) {
            await loadApplicants();
        } else {
            alert(await res.text());
            btn.disabled = false;
        }
    } catch {
        alert("Network error.");
        btn.disabled = false;
    }
}

async function rejectVolunteer(volunteerId, btn) {
    if (!confirm("Reject this volunteer's application?")) return;
    btn.disabled = true;
    try {
        const res = await fetch(`/api/Volunteer/reject/${volunteerId}`, { method: "PUT" });
        if (res.ok) {
            await loadApplicants();
        } else {
            alert(await res.text());
            btn.disabled = false;
        }
    } catch {
        alert("Network error.");
        btn.disabled = false;
    }
}

document.addEventListener("DOMContentLoaded", loadApplicants);
