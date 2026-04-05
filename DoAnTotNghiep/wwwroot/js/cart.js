// cart.js - cập nhật để tự động merge localStorage lên server khi user đã đăng nhập
// Giữ nguyên hàm clientAddToCart, addToCart, updateCartCount nhưng thêm mergeLocalToServer

// client-side fallback
function clientAddToCart(product) {
    let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
    let item = cart.find(i => String(i.id) === String(product.id));
    if (item) item.quantity += 1;
    else cart.push({ id: product.id, name: product.name, price: product.price, image: product.image, quantity: 1 });
    localStorage.setItem('shoppingCart', JSON.stringify(cart));
    updateCartCount();
}

// server-side add (fallback to client if fails)
function addToCart(product) {
    if (window.appUserName) {
        fetch('/api/Cart/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ProductId: product.id, Quantity: 1, Price: product.price })
        })
        .then(res => {
            if (!res.ok) throw new Error('API lỗi');
            return res.json();
        })
        .then(() => {
            updateCartCount();
            alert(`Đã thêm ${product.name} vào giỏ.`);
        })
        .catch(() => {
            clientAddToCart(product);
            alert(`Không lưu được lên server, đã lưu tạm vào trình duyệt.`);
        });
    } else {
        clientAddToCart(product);
        alert(`Đã thêm ${product.name} vào giỏ.`);
    }
}

// Merge localStorage -> server khi user đã đăng nhập
function mergeLocalToServer() {
    try {
        if (!window.appUserName) return; // chỉ merge nếu đã login
        const raw = localStorage.getItem('shoppingCart');
        if (!raw) return;
        const localCart = JSON.parse(raw) || [];
        if (!localCart.length) return;

        // map local item shape -> CartItemDto expected by API
        const dto = {
            Items: localCart.map(i => ({
                ProductId: i.id,
                Quantity: i.quantity || 1,
                Price: i.price || 0
            }))
        };

        fetch('/api/Cart/MergeLocal', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        })
        .then(r => {
            if (!r.ok) throw new Error('Merge API lỗi ' + r.status);
            return r.json();
        })
        .then(() => {
            // xóa localStorage sau khi merge thành công
            localStorage.removeItem('shoppingCart');
            updateCartCount();
           
        })
        .catch(err => {
            console.warn('Không merge được local cart:', err);
        });
    } catch (e) {
        console.error('mergeLocalToServer lỗi:', e);
    }
}

// cập nhật số lượng hiển thị trên header (server nếu đăng nhập, ngược lại localStorage)
function updateCartCount() {
    const el = document.getElementById('cart-count');
    if (window.appUserName) {
        fetch('/api/Cart')
            .then(r => {
                if (!r.ok) throw new Error('API lỗi');
                return r.json();
            })
            .then(cart => {
                // Cart API có thể trả { items: [...] } hoặc Cart.Items
                const items = cart.items || cart.Items || cart.Items || [];
                const total = (items || []).reduce((s, i) => s + (i.quantity || i.Quantity || 0), 0) || 0;
                if (el) el.innerText = total;
            })
            .catch(() => {
                // fallback
                const cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
                if (el) el.innerText = cart.reduce((s, i) => s + (i.quantity || 0), 0);
            });
    } else {
        const cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
        if (el) el.innerText = cart.reduce((s, i) => s + (i.quantity || 0), 0);
    }
}

window.removeFromCart = function (id) {
    // xóa ở local (nếu đang dùng local) và gọi server delete nếu logged in
    if (window.appUserName) {
        fetch('/api/Cart/' + id, { method: 'DELETE' })
            .then(r => {
                if (!r.ok) throw new Error('Xóa server lỗi');
                updateCartCount();
            })
            .catch(() => {
                // fallback xóa local
                let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
                cart = cart.filter(item => String(item.id) !== String(id));
                localStorage.setItem('shoppingCart', JSON.stringify(cart));
                updateCartCount();
            });
    } else {
        let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
        cart = cart.filter(item => String(item.id) !== String(id));
        localStorage.setItem('shoppingCart', JSON.stringify(cart));
        updateCartCount();
    }
};

document.addEventListener('DOMContentLoaded', function () {
    // khi trang load, nếu đã login -> merge local cart lên server
    mergeLocalToServer();
    updateCartCount();
});