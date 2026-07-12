const locBtn = document.getElementById("useLocationBtn");
if (locBtn) {
    locBtn.addEventListener("click", () => {
        if (!navigator.geolocation) {
            alert("Geolocation is not supported by your browser.");
            return;
        }
        navigator.geolocation.getCurrentPosition(pos => {
            document.getElementById("latitude").value = pos.coords.latitude;
            document.getElementById("longitude").value = pos.coords.longitude;
        }, () => alert("Unable to retrieve your location."));
    });
}

document.getElementById("createRequestForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const alertBox = document.getElementById("formAlert");
    const submitBtn = document.getElementById("submitBtn");
    alertBox.classList.add("d-none");

    const payload = {
        categoryId: parseInt(document.getElementById("categoryId").value, 10),
        title: document.getElementById("title").value.trim(),
        description: document.getElementById("description").value.trim(),
        address: document.getElementById("address").value.trim(),
        latitude: document.getElementById("latitude").value ? parseFloat(document.getElementById("latitude").value) : null,
        longitude: document.getElementById("longitude").value ? parseFloat(document.getElementById("longitude").value) : null
    };

    submitBtn.disabled = true;
    submitBtn.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Submitting...`;

    try {
        const res = await fetch("/api/HelpRequests", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            window.location.href = "/HelpRequests/Index";
        } else if (res.status === 401) {
            window.location.href = "/Account/Login";
        } else {
            const msg = await res.text();
            alertBox.textContent = msg || "Failed to submit request. Please check your inputs.";
            alertBox.classList.remove("d-none");
            submitBtn.disabled = false;
            submitBtn.textContent = "Submit Request";
        }
    } catch (err) {
        alertBox.textContent = "Network error. Please try again.";
        alertBox.classList.remove("d-none");
        submitBtn.disabled = false;
        submitBtn.textContent = "Submit Request";
    }
});
