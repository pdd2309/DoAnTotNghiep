// File: wwwroot/js/ai-chatbot.js 
const chatBtn = document.getElementById('btn-open-chat');
const chatBox = document.getElementById('ai-chat-box');
const chatClose = document.getElementById('btn-close-chat');
const chatInput = document.getElementById('chat-input');
const chatContent = document.getElementById('chat-content');
const sendBtn = document.getElementById('btn-send-chat');

if (chatBtn) {
    chatBtn.onclick = () => {
        chatBox.style.display = chatBox.style.display === 'none' ? 'flex' : 'none';
        if (chatBox.style.display === 'flex') {
            chatInput.focus(); // Tự động focus vào ô nhập khi mở
        }
    };

    chatClose.onclick = () => chatBox.style.display = 'none';

    async function sendToAI() {
        const msg = chatInput.value.trim();
        if (!msg) return;

        // 1. Hiển thị tin nhắn của người dùng
        chatContent.innerHTML += `<div style="background: #7fad39; color: white; padding: 8px 12px; border-radius: 15px; align-self: flex-end; max-width: 80%; word-wrap: break-word;">${msg}</div>`;
        chatInput.value = '';
        chatContent.scrollTop = chatContent.scrollHeight;

        // 2. Hiển thị trạng thái đang trả lời
        const loadingId = "loading-" + Date.now();
        chatContent.innerHTML += `<div id="${loadingId}" style="background: #eee; padding: 8px 12px; border-radius: 15px; align-self: flex-start; font-style: italic;">AI đang trả lời...</div>`;
        chatContent.scrollTop = chatContent.scrollHeight;

        try {
            const res = await fetch('/api/ChatAI/Ask', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: msg }) // Đảm bảo key là "message" khớp với C#
            });

            const data = await res.json();

            // Xóa dòng "đang trả lời"
            const loadingElem = document.getElementById(loadingId);
            if (loadingElem) loadingElem.remove();

            // 3. Hiển thị câu trả lời của AI
            chatContent.innerHTML += `<div style="background: #eee; padding: 8px 12px; border-radius: 15px; align-self: flex-start; max-width: 80%; word-wrap: break-word;">${data.reply}</div>`;
            chatContent.scrollTop = chatContent.scrollHeight;

        } catch (e) {
            const loadingElem = document.getElementById(loadingId);
            if (loadingElem) loadingElem.innerHTML = "Lỗi kết nối AI!";
            console.error("Chat Error:", e);
        }
    }

    sendBtn.onclick = sendToAI;
    chatInput.onkeypress = (e) => {
        if (e.key === 'Enter') {
            sendToAI();
        }
    };
}