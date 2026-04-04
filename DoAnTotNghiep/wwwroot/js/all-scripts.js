// File này đóng vai trò là "Tổng đài", gọi tất cả các file JS khác
const scripts = [
    "js/cart.js",           // File xử lý giỏ hàng (dùng chung)
    "js/fetch-products.js", // File hiện sản phẩm ở trang chủ
    "js/fetch-detail.js",   // File hiện chi tiết sản phẩm
    "js/fetch-cart.js"      // File hiện bảng giỏ hàng
];

// Hàm tự động chèn các thẻ <script> vào trang web (chuyển sang đường dẫn root-relative)
scripts.forEach(src => {
    const script = document.createElement('script');
    script.src = src.startsWith('/') ? src : ('/' + src); // đảm bảo root-relative
    script.async = false; // Đảm bảo các file chạy đúng thứ tự
    document.head.appendChild(script);
});

// Đoạn này dùng để hiện list đồ ở trang Checkout
if (document.getElementById('checkout-list')) {
    let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
    let html = '';
    let total = 0;
    cart.forEach(item => {
        total += item.price * item.quantity;
        html += `<li>${item.name} x ${item.quantity} <span>${new Intl.NumberFormat('vi-VN').format(item.price * item.quantity)}</span></li>`;
    });
    document.getElementById('checkout-list').innerHTML = html;
    const checkoutTotalEl = document.getElementById('checkout-total');
    if (checkoutTotalEl) {
        checkoutTotalEl.innerText = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(total);
    }
}