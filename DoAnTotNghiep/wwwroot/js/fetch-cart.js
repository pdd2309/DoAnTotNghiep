document.addEventListener('DOMContentLoaded', async function () {
    const cartTable = document.getElementById('cart-table-body');
    const totalElement = document.getElementById('cart-total');
    const subtotalElement = document.getElementById('cart-subtotal');
    if (!cartTable) return;

    const fmt = v => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v || 0);

    let cachedItems = [];

    function initPriceMap() {
        const priceMap = {};
        cachedItems.forEach(it => {
            priceMap[it.productId || it.id] = parseFloat(it.price) || 0;
        });
        sessionStorage.setItem('cartPriceMap', JSON.stringify(priceMap));
        return priceMap;
    }

    function getPriceFromMap(productId) {
        const priceMap = JSON.parse(sessionStorage.getItem('cartPriceMap') || '{}');
        return parseFloat(priceMap[productId]) || 0;
    }

    async function renderCart() {
        if (window.appUserName) {
            try {
                const res = await fetch('/api/Cart');
                if (!res.ok) throw new Error('API lỗi');
                const cart = await res.json();
                const items = cart.items || cart.Items || [];

                if (!items.length) {
                    cartTable.innerHTML = '<tr><td colspan="5" style="text-align:center; padding:50px;"><h5>Giỏ hàng đang trống!</h5><br><a href="/" class="primary-btn">MUA SẮM NGAY</a></td></tr>';
                    if (totalElement) totalElement.innerText = fmt(0);
                    if (subtotalElement) subtotalElement.innerText = fmt(0);
                    updateCartCount();
                    return;
                }

                cachedItems = items;
                initPriceMap();

                let html = '', total = 0;
                items.forEach(it => {
                    const price = parseFloat(it.price) || 0;
                    const qty = parseInt(it.quantity) || 0;
                    const subtotal = price * qty;
                    total += subtotal;

                    const img = it.image
                        ? (it.image.startsWith('/') || it.image.startsWith('http') ? it.image : '/' + it.image)
                        : '/img/product-placeholder.jpg';

                    const rowId = `cart-row-${it.productId}`;

                    html += `
                    <tr id="${rowId}">
                        <td class="shoping__cart__item">
                            <img src="${img}" alt="${it.name || 'Sản phẩm'}" style="width:80px;height:80px;object-fit:cover;margin-right:20px;" onerror="this.src='/img/product-placeholder.jpg'">
                            <h5>${it.name || 'Sản phẩm'}</h5>
                        </td>
                        <td class="shoping__cart__price">${fmt(price)}</td>
                        <td class="shoping__cart__quantity">
                            <div class="quantity">
                                <div class="pro-qty">
                                    <input type="number" data-product-id="${it.productId}" data-price="${price}" min="1" value="${qty}" class="qty-input" style="width:80px;text-align:center;border:1px solid #ebebeb;">
                                    </div>
                            </div>
                        </td>
                        <td class="shoping__cart__total item-subtotal" data-product-id="${it.productId}">${fmt(subtotal)}</td>
                        <td class="shoping__cart__item__close">
                            <button type="button" class="btn-remove" data-product-id="${it.productId}" style="background:none;border:none;cursor:pointer;font-size:24px;color:red;">✕</button>
                        </td>
                    </tr>`;
                });

                cartTable.innerHTML = html;
                if (totalElement) totalElement.innerText = fmt(total);
                if (subtotalElement) subtotalElement.innerText = fmt(total);

                attachQuantityListeners(); // Vẫn giữ để khi gõ số nó tự update tiền
                attachRemoveListeners();
                updateCartCount();
                return;
            } catch (err) {
                console.warn('Lỗi khi tải giỏ từ server:', err);
            }
        }

        // --- LOCAL STORAGE FALLBACK ---
        let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
        if (!cart.length) {
            cartTable.innerHTML = '<tr><td colspan="5" style="text-align:center; padding:50px;"><h5>Giỏ hàng đang trống!</h5><br><a href="/" class="primary-btn">MUA SẮM NGAY</a></td></tr>';
            if (totalElement) totalElement.innerText = fmt(0);
            if (subtotalElement) subtotalElement.innerText = fmt(0);
            updateCartCount();
            return;
        }

        cachedItems = cart;
        initPriceMap();

        let html = '', total = 0;
        cart.forEach(it => {
            const price = parseFloat(it.price) || 0;
            const qty = parseInt(it.quantity) || 0;
            const subtotal = price * qty;
            total += subtotal;

            const img = (it.image && (it.image.startsWith('/') || it.image.startsWith('http')))
                ? it.image
                : ('/' + (it.image || 'img/featured/feature-1.jpg'));

            const rowId = `cart-row-${it.id}`;

            html += `
            <tr id="${rowId}">
                <td class="shoping__cart__item">
                    <img src="${img}" alt="" style="width:80px;height:80px;object-fit:cover;margin-right:20px;">
                    <h5>${it.name}</h5>
                </td>
                <td class="shoping__cart__price">${fmt(price)}</td>
                <td class="shoping__cart__quantity">
                    <div class="quantity">
                        <div class="pro-qty">
                            <input type="number" data-product-id="${it.id}" data-price="${price}" min="1" value="${qty}" class="qty-input-local" style="width:80px;text-align:center;border:1px solid #ebebeb;">
                            </div>
                    </div>
                </td>
                <td class="shoping__cart__total item-subtotal-local" data-product-id="${it.id}">${fmt(subtotal)}</td>
                <td class="shoping__cart__item__close">
                    <button type="button" class="btn-remove-local" data-product-id="${it.id}" style="background:none;border:none;cursor:pointer;font-size:24px;color:red;">✕</button>
                </td>
            </tr>`;
        });

        cartTable.innerHTML = html;
        if (totalElement) totalElement.innerText = fmt(total);
        if (subtotalElement) subtotalElement.innerText = fmt(total);

        attachQuantityListenersLocal();
        attachRemoveListenersLocal();
        updateCartCount();
    }

    function attachQuantityListeners() {
        document.querySelectorAll('.qty-input').forEach(input => {
            input.addEventListener('change', (e) => {
                const productId = e.target.getAttribute('data-product-id');
                const newQty = parseInt(e.target.value) || 1;
                if (newQty < 1) { e.target.value = 1; return; }
                updateQuantityAndDisplay(productId, newQty);
            });
        });
    }

    function attachQuantityListenersLocal() {
        document.querySelectorAll('.qty-input-local').forEach(input => {
            input.addEventListener('change', (e) => {
                const productId = e.target.getAttribute('data-product-id');
                const newQty = parseInt(e.target.value) || 1;
                if (newQty < 1) { e.target.value = 1; return; }
                updateQuantityAndDisplayLocal(productId, newQty);
            });
        });
    }

    function attachRemoveListeners() {
        document.querySelectorAll('.btn-remove').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                e.preventDefault();
                const productId = btn.getAttribute('data-product-id');
                if (confirm('Bạn chắc chắn muốn xóa sản phẩm này?')) {
                    try {
                        const res = await fetch(`/api/Cart/${productId}`, { method: 'DELETE' });
                        if (!res.ok) throw new Error('Xóa lỗi');
                        renderCart();
                    } catch (err) {
                        console.error('Lỗi xóa:', err);
                        alert('Không thể xóa sản phẩm');
                    }
                }
            });
        });
    }

    function attachRemoveListenersLocal() {
        document.querySelectorAll('.btn-remove-local').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const productId = btn.getAttribute('data-product-id');
                if (confirm('Bạn chắc chắn muốn xóa sản phẩm này?')) {
                    let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
                    cart = cart.filter(item => String(item.id) !== String(productId));
                    localStorage.setItem('shoppingCart', JSON.stringify(cart));
                    renderCart();
                }
            });
        });
    }

    async function updateQuantityAndDisplay(productId, quantity) {
        let price = getPriceFromMap(productId);
        const subtotal = price * quantity;
        const subtotalCell = document.querySelector(`.item-subtotal[data-product-id="${productId}"]`);
        if (subtotalCell) subtotalCell.innerText = fmt(subtotal);

        let total = 0;
        const priceMap = JSON.parse(sessionStorage.getItem('cartPriceMap') || '{}');
        document.querySelectorAll('.qty-input').forEach(inp => {
            const prodId = inp.getAttribute('data-product-id');
            const p = parseFloat(priceMap[prodId]) || 0;
            const q = parseInt(inp.value) || 0;
            total += p * q;
        });
        if (totalElement) totalElement.innerText = fmt(total);
        if (subtotalElement) subtotalElement.innerText = fmt(total);

        try {
            await fetch('/api/Cart/Update', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ProductId: productId, Quantity: quantity, Price: price })
            });
            updateCartCount();
        } catch (err) { console.error('Lỗi update qty:', err); }
    }

    function updateQuantityAndDisplayLocal(productId, quantity) {
        const priceMap = JSON.parse(sessionStorage.getItem('cartPriceMap') || '{}');
        let price = parseFloat(priceMap[productId]) || 0;
        const subtotal = price * quantity;
        const subtotalCell = document.querySelector(`.item-subtotal-local[data-product-id="${productId}"]`);
        if (subtotalCell) subtotalCell.innerText = fmt(subtotal);

        let total = 0;
        document.querySelectorAll('.qty-input-local').forEach(inp => {
            const prodId = inp.getAttribute('data-product-id');
            const p = parseFloat(priceMap[prodId]) || 0;
            const q = parseInt(inp.value) || 0;
            total += p * q;
        });
        if (totalElement) totalElement.innerText = fmt(total);
        if (subtotalElement) subtotalElement.innerText = fmt(total);

        let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
        const item = cart.find(i => String(i.id) === String(productId));
        if (item) {
            item.quantity = quantity;
            localStorage.setItem('shoppingCart', JSON.stringify(cart));
        }
        updateCartCount();
    }

    renderCart();
});