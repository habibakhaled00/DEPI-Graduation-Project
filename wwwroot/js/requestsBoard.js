const apiBase = "/api/HelpRequests";
let currentPage = 1;
const pageSize = 9;
let totalCount = 0;

const searchInput = document.getElementById("searchInput");
const categoryFilter = document.getElementById("categoryFilter");
const statusFilter = document.getElementById("statusFilter");
const container = document.getElementById("requestsContainer");
const template = document.getElementById("requestCardTemplate");
const loadingIndicator = document.getElementById("loadingIndicator");
const paginationEl = document.getElementById("pagination");
const CurrentUID = window.CurrentUID || "";

let debounceTimer;
searchInput.addEventListener("input", () => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => { currentPage = 1; loadRequests(); }, 350);
});
categoryFilter.addEventListener("change", () => { currentPage = 1; loadRequests(); });
statusFilter.addEventListener("change", () => { currentPage = 1; loadRequests(); });

async function loadRequests() {
    loadingIndicator.classList.remove("d-none");
    container.innerHTML = "";

    const params = new URLSearchParams({
        page: currentPage,
        pageSize: pageSize
    });
    if (searchInput.value.trim()) params.append("search", searchInput.value.trim());
    if (categoryFilter.value) params.append("categoryId", categoryFilter.value);
    if (statusFilter.value) params.append("status", statusFilter.value);

    try {
        const res = await fetch(`${apiBase}?${params.toString()}`);
        totalCount = parseInt(res.headers.get("X-Total-Count") || "0", 10);
        const data = await res.json();
        renderRequests(data);
        renderPagination();
    } catch (err) {
        container.innerHTML = `<p class="text-danger text-center">Failed to load requests.</p>`;
    } finally {
        loadingIndicator.classList.add("d-none");
    }
}

function renderRequests(requests) {
    container.innerHTML = "";
    if (requests.length === 0) {
        container.innerHTML = `<p class="text-muted text-center">No help requests found.</p>`;
        return;
    }

    requests.forEach(r => {
        const node = template.content.cloneNode(true);
        node.querySelector(".req-title").textContent = r.title;
        node.querySelector(".req-category").textContent = r.categoryName;
        node.querySelector(".req-description").textContent =
            r.description.length > 100 ? r.description.substring(0, 100) + "..." : r.description;
        node.querySelector(".req-address").textContent = r.address;
        node.querySelector(".req-meta").textContent =
            `Posted by ${r.requesterName} • ${new Date(r.createdAt).toLocaleDateString()} • ${r.volunteerCount} applicant(s)`;

        const badge = node.querySelector(".req-status-badge");
        badge.textContent = r.status;
        badge.classList.add(...statusClasses(r.status));

        node.querySelector(".req-details-btn").href = `/HelpRequests/Details/${r.requestId}`;

        const actionBtn = node.querySelector(".req-volunteer-btn");
        const isOwner = CurrentUID && CurrentUID === r.userId;

        if (isOwner) {
            actionBtn.innerHTML = `<i class="bi bi-people"></i> Manage`;
            actionBtn.classList.replace("btn-success", "btn-primary");
            actionBtn.addEventListener("click", () => {
                window.location.href = `/HelpRequests/ManageVolunteers/${r.requestId}`;
            });
        } else if (r.currentUserVolunteerStatus === "Accepted") {
            actionBtn.innerHTML = `<i class="bi bi-chat-dots"></i> Chat`;
            actionBtn.classList.replace("btn-success", "btn-primary");
            actionBtn.addEventListener("click", () => {
                window.location.href = `/Chat/${r.requestId}`;
            });
        } else if (r.currentUserVolunteerStatus === "Pending") {
            actionBtn.disabled = true;
            actionBtn.innerHTML = `<i class="bi bi-hourglass-split"></i> Pending`;
            actionBtn.classList.replace("btn-success", "btn-outline-secondary");
        } else if (r.currentUserVolunteerStatus === "Rejected") {
            actionBtn.disabled = true;
            actionBtn.innerHTML = `<i class="bi bi-x-circle"></i> Rejected`;
            actionBtn.classList.replace("btn-success", "btn-outline-danger");
        } else if (r.status !== "Open" && r.status !== "Pending") {
            actionBtn.disabled = true;
            actionBtn.innerHTML = `<i class="bi bi-lock"></i> Unavailable`;
            actionBtn.classList.replace("btn-success", "btn-outline-secondary");
        } else {
            actionBtn.addEventListener("click", () => volunteerFor(r.requestId, actionBtn));
        }

        const cardEl = node.querySelector('.nh-card, [class*="card"]') || node.firstElementChild;
        if (cardEl) {
            cardEl.style.animationDelay = `${requests.indexOf(r) * 60}ms`;
            cardEl.classList.add('fade-up');
        }
        container.appendChild(node);
    });
}

function statusClasses(status) {
    const map = {
        'Open':      ['status-badge', 'status-open'],
        'Pending':   ['status-badge', 'status-pending'],
        'Accepted':  ['status-badge', 'status-accepted'],
        'Completed': ['status-badge', 'status-completed'],
        'Cancelled': ['status-badge', 'status-accepted'], // re-use style
    };
    return map[status] || ['status-badge', 'status-pending'];
}

async function volunteerFor(requestId, btn) {
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Applying...`;

    try {
        const res = await fetch(`/api/Volunteer/apply/${requestId}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" }
        });

        if (res.ok) {
            btn.innerHTML = `<i class="bi bi-hourglass-split"></i> Pending`;
            btn.classList.replace("btn-success", "btn-outline-secondary");
            btn.disabled = true;
            if (window.nhToast) window.nhToast("You have volunteered for this request!");
            setTimeout(loadRequests, 1200);
        } else if (res.status === 401) {
            window.location.href = "/Account/Login";
        } else {
            let errorText = "Could not apply.";
            try {
                const errObj = await res.json();
                errorText = errObj.message || errorText;
            } catch {
                try { errorText = await res.text() || errorText; } catch {}
            }
            if (window.nhToast) window.nhToast(errorText, "error");
            btn.disabled = false;
            btn.innerHTML = `<i class="bi bi-hand-thumbs-up"></i> Volunteer`;
        }
    } catch (err) {
        alert("Network error. Please try again.");
        btn.disabled = false;
        btn.innerHTML = `<i class="bi bi-hand-thumbs-up"></i> Volunteer`;
    }
}

function renderPagination() {
    paginationEl.innerHTML = "";
    const totalPages = Math.ceil(totalCount / pageSize);
    if (totalPages <= 1) return;

    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement("li");
        li.className = `page-item ${i === currentPage ? "active" : ""}`;
        li.innerHTML = `<a class="page-link" href="#">${i}</a>`;
        li.addEventListener("click", (e) => {
            e.preventDefault();
            currentPage = i;
            loadRequests();
            window.scrollTo({ top: 0, behavior: "smooth" });
        });
        paginationEl.appendChild(li);
    }
}

document.addEventListener("DOMContentLoaded", loadRequests);
