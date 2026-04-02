document.addEventListener('DOMContentLoaded', function () {
    // 1. Kiểm tra xem có phải trang chi tiết không (tránh lỗi Illegal return)
    const detailName = document.getElementById('detail-name');
    if (!detailName) return;

    const urlParams = new URLSearchParams(window.location.search);
    const productId = urlParams.get('id');

    if (!productId) {
        console.error("Lỗi: Không tìm thấy ID sản phẩm trên link!");
        return;
    }

    // 2. Gọi API lấy dữ liệu
    fetch(`/api/SanPhamApi/${productId}`)
        .then(res => res.json())
        .then(result => {
            // Lấy dữ liệu sản phẩm từ result
            const sp = result.value || result.data || result;

            if (!sp || (!sp.tenSanPham && !sp.TenSanPham)) {
                console.error("Lỗi: API không có dữ liệu sản phẩm!", sp);
                return;
            }

            // Đổ dữ liệu ra HTML
            if (document.getElementById('detail-name'))
                document.getElementById('detail-name').innerText = sp.tenSanPham || sp.TenSanPham;

            if (document.getElementById('detail-price')) {
                const gia = sp.giaTien || sp.GiaTien || 0;
                document.getElementById('detail-price').innerText = new Intl.NumberFormat('vi-VN', {
                    style: 'currency', currency: 'VND'
                }).format(gia);
            }

            if (document.getElementById('detail-desc'))
                document.getElementById('detail-desc').innerText = sp.moTa || sp.MoTa || "Sản phẩm chính hãng.";

            const imgTag = document.getElementById('detail-img');
            if (imgTag) {
                imgTag.src = (sp.hinhAnh || sp.HinhAnh) || 'img/product/details/product-details-1.jpg';
            }

            // 3. LOGIC NÚT THÊM VÀO GIỎ (Phải nằm trong .then này để lấy được biến 'sp')
            const addBtn = document.getElementById('add-to-cart');
            if (addBtn) {
                addBtn.onclick = function () {
                    // Gọi hàm addToCart từ file cart.js
                    if (typeof addToCart === 'function') {
                        addToCart({
                            id: sp.maSanPham || sp.MaSanPham,
                            name: sp.tenSanPham || sp.TenSanPham,
                            price: sp.giaTien || sp.GiaTien,
                            image: sp.hinhAnh || sp.HinhAnh
                        });
                    } else {
                        console.error("Lỗi: Không tìm thấy hàm addToCart. Hãy kiểm tra lại file cart.js!");
                    }
                };
            }
        })
        .catch(err => console.error("Lỗi kết nối API:", err));
});