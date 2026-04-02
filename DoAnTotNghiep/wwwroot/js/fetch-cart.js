document.addEventListener('DOMContentLoaded', function () {
    // 1. Tìm đúng cái ID trong HTML Ogani của Đông
    const cartTable = document.getElementById('cart-table-body');
    const totalElement = document.getElementById('cart-total');
    const subtotalElement = document.getElementById('cart-subtotal');

    if (!cartTable) {
        console.error("Không tìm thấy thẻ <tbody id='cart-table-body'> trong HTML!");
        return;
    }

    function renderCart() {
        // Lấy đồ từ túi ra
        let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
        console.log("Giỏ hàng đang có:", cart); // Check trong F12 Console nhé

        if (cart.length === 0) {
            cartTable.innerHTML = '<tr><td colspan="5" style="text-align:center; padding:50px;"><h5>Giỏ hàng đang trống!</h5><br><a href="index.html" class="primary-btn">MUA SẮM NGAY</a></td></tr>';
            if (totalElement) totalElement.innerText = "0 đ";
            if (subtotalElement) subtotalElement.innerText = "0 đ";
            return;
        }

        let html = '';
        let totalMoney = 0;

        cart.forEach(item => {
            // Ép kiểu để tính toán không bị lỗi
            const price = parseFloat(item.price) || 0;
            const qty = parseInt(item.quantity) || 0;
            const subtotal = price * qty;
            totalMoney += subtotal;

            html += `
            <tr>
                <td class="shoping__cart__item">
                    <img src="${item.image}" alt="" style="width: 80px; margin-right: 20px;">
                    <h5>${item.name}</h5>
                </td>
                <td class="shoping__cart__price">
                    ${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)}
                </td>
                <td class="shoping__cart__quantity">
                    <div class="quantity">
                        <div class="pro-qty">
                            <input type="text" value="${qty}" readonly>
                        </div>
                    </div>
                </td>
                <td class="shoping__cart__total">
                    ${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(subtotal)}
                </td>
                <td class="shoping__cart__item__close">
                    <span class="icon_close" onclick="removeFromCart('${item.id}')" style="cursor:pointer;"></span>
                </td>
            </tr>`;
        });

        cartTable.innerHTML = html;
        const formattedTotal = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(totalMoney);
        if (totalElement) totalElement.innerText = formattedTotal;
        if (subtotalElement) subtotalElement.innerText = formattedTotal;
    }

    // Đưa hàm xóa ra ngoài để gọi từ HTML
    window.removeFromCart = function (id) {
        let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
        // So sánh chuỗi cho chắc ăn vì ID từ HTML hay là String
        cart = cart.filter(item => String(item.id) !== String(id));
        localStorage.setItem('shoppingCart', JSON.stringify(cart));
        renderCart();
        // Cập nhật con số trên icon Header
        if (typeof updateCartCount === 'function') updateCartCount();
    };

    renderCart();
});