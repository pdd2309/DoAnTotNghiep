document.addEventListener('DOMContentLoaded', async function () {
    const listElement = document.getElementById('checkout-list');
    const totalElement = document.getElementById('checkout-total');
    const subtotalElement = document.getElementById('checkout-subtotal');

    if (!listElement) return;

    const fmt = v => new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(v || 0);

    // 1. Tải tóm tắt đơn hàng từ giỏ hàng hiện tại
    async function loadOrderSummary() {
        try {
            const res = await fetch('/api/Cart');
            if (!res.ok) throw new Error('Không thể tải dữ liệu giỏ hàng');

            const cart = await res.json();
            const items = cart.items || cart.Items || [];

            if (items.length === 0) {
                alert("Giỏ hàng hiện đang trống.");
                window.location.href = "/";
                return;
            }

            let html = '', total = 0;
            items.forEach(it => {
                const price = parseFloat(it.price) || 0;
                const qty = parseInt(it.quantity) || 0;
                const sub = price * qty;
                total += sub;
                html += `<li>${it.name} (x${qty}) <span>${fmt(sub)}</span></li>`;
            });

            listElement.innerHTML = html;
            if (totalElement) totalElement.innerText = fmt(total);
            if (subtotalElement) subtotalElement.innerText = fmt(total);

        } catch (err) {
            console.error("Lỗi khi tải tóm tắt đơn hàng:", err);
        }
    }

    // 2. Xử lý sự kiện đặt hàng
    const btnPlaceOrder = document.getElementById('btn-place-order');
    if (btnPlaceOrder) {
        btnPlaceOrder.addEventListener('click', async function () {
            const orderData = {
                HoTen: document.getElementById('order-name').value.trim(),
                DiaChi: document.getElementById('order-address').value.trim(),
                SDT: document.getElementById('order-phone').value.trim(),
                Email: document.getElementById('order-email').value.trim(),
                GhiChu: document.getElementById('order-note').value.trim()
            };

            // Kiểm tra các trường bắt buộc
            if (!orderData.HoTen || !orderData.DiaChi || !orderData.SDT) {
                alert("Vui lòng nhập đầy đủ các thông tin bắt buộc (*)");
                return;
            }

            try {
                // Gọi API để tạo đơn hàng mới trong hệ thống SQL
                const response = await fetch('/api/OrderApi/Create', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(orderData)
                });

                if (response.ok) {
                    const result = await response.json();
                    alert("Đặt hàng thành công. Mã đơn hàng của bạn là: #" + result.orderId);

                    // Xóa dữ liệu tạm sau khi đặt hàng thành công
                    localStorage.removeItem('shoppingCart');

                    // Chuyển hướng về trang chủ hoặc trang thông báo thành công
                    window.location.href = "/";
                } else {
                    const errorMsg = await response.text();
                    alert("Có lỗi xảy ra trong quá trình đặt hàng: " + errorMsg);
                }
            } catch (err) {
                console.error("Lỗi kết nối API đặt hàng:", err);
                alert("Không thể kết nối tới máy chủ. Vui lòng thử lại sau.");
            }
        });
    }

    loadOrderSummary();
});