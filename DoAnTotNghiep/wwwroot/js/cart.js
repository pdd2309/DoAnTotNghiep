// Hàm thêm sản phẩm vào giỏ hàng
function addToCart(product) {
    // 1. Lấy giỏ hàng hiện tại từ localStorage (nếu chưa có thì tạo mảng rỗng)
    let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];

    // 2. Kiểm tra xem món này đã có trong giỏ chưa
    let item = cart.find(i => i.id === product.id);

    if (item) {
        item.quantity += 1; // Có rồi thì tăng số lượng
    } else {
        cart.push({
            id: product.id,
            name: product.name,
            price: product.price,
            image: product.image,
            quantity: 1
        });
    }

    // 3. Lưu lại vào localStorage
    localStorage.setItem('shoppingCart', JSON.stringify(cart));

    // 4. Cập nhật con số trên icon giỏ hàng
    updateCartCount();
    alert("Đã thêm " + product.name + " vào giỏ hàng!");
}

// Hàm cập nhật số lượng hiển thị trên Header
function updateCartCount() {
    let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
    let total = cart.reduce((sum, item) => sum + item.quantity, 0);
    const cartCountElement = document.getElementById('cart-count');
    if (cartCountElement) {
        cartCountElement.innerText = total;
    }
}

// Chạy hàm cập nhật ngay khi load trang
document.addEventListener('DOMContentLoaded', updateCartCount);