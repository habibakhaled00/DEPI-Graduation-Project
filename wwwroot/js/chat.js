const chatWindow = document.getElementById("chatWindow");
const chatForm = document.getElementById("chatForm");
const messageInput = document.getElementById("messageInput");
const sendBtn = document.getElementById("sendBtn");
const connectionStatus = document.getElementById("connectionStatus");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect()
    .build();

connection.on("LoadHistory", (messages) => {
    chatWindow.innerHTML = "";
    messages.forEach(appendMessage);
    scrollToBottom();
});

connection.on("ReceiveMessage", (msg) => {
    appendMessage(msg);
    scrollToBottom();
});

connection.on("Unauthorized", () => {
    connectionStatus.className = "alert alert-danger py-2 small";
    connectionStatus.textContent = "You are not authorized to view this conversation.";
    messageInput.disabled = true;
    sendBtn.disabled = true;
});

connection.onreconnecting(() => {
    connectionStatus.className = "alert alert-warning py-2 small";
    connectionStatus.textContent = "Reconnecting...";
    messageInput.disabled = true;
    sendBtn.disabled = true;
});

connection.onreconnected(() => {
    connection.invoke("JoinRequestRoom", requestId);
    connectionStatus.className = "alert alert-success py-2 small";
    connectionStatus.textContent = "Connected";
    messageInput.disabled = false;
    sendBtn.disabled = false;
});

async function start() {
    try {
        await connection.start();
        await connection.invoke("JoinRequestRoom", requestId);
        connectionStatus.className = "alert alert-success py-2 small";
        connectionStatus.textContent = "Connected — messages are live.";
        messageInput.disabled = false;
        sendBtn.disabled = false;
        messageInput.focus();
    } catch (err) {
        connectionStatus.className = "alert alert-danger py-2 small";
        connectionStatus.textContent = "Failed to connect. Retrying...";
        setTimeout(start, 3000);
    }
}

chatForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    const text = messageInput.value.trim();
    if (!text) return;

    try {
        await connection.invoke("SendMessage", requestId, text);
        messageInput.value = "";
        messageInput.focus();
    } catch (err) {
        alert("Failed to send message.");
    }
});

function appendMessage(msg) {
    const isMine = msg.senderId === CurrentUID;
    const wrapper = document.createElement("div");
    wrapper.className = "d-flex flex-column " + (isMine ? "align-items-end" : "align-items-start");

    const bubble = document.createElement("div");
    bubble.className = "chat-bubble " + (isMine ? "mine" : "theirs");
    bubble.textContent = msg.content;

    const meta = document.createElement("div");
    meta.className = "chat-meta";
    meta.textContent = `${isMine ? "You" : msg.senderName} • ${new Date(msg.sentAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`;

    wrapper.appendChild(bubble);
    wrapper.appendChild(meta);
    chatWindow.appendChild(wrapper);
}

function scrollToBottom() {
    chatWindow.scrollTop = chatWindow.scrollHeight;
}

document.addEventListener("DOMContentLoaded", start);
