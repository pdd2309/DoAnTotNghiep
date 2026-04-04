document.addEventListener('DOMContentLoaded', function () {
    // element container
    const container = document.getElementById('product-detail-content');
    if (!container) return;

    // productId được set trong Details.cshtml via ViewBag
    if (typeof productId === 'undefined' || !productId) {
        container.innerHTML = '<div class="col-12 text-center text-danger">Không tìm thấy ID sản phẩm.</div>';
        console.error('Không tìm thấy productId (ViewBag.MaSanPham).');
        return;
    }

    fetch(`/api/SanPhamApi/${productId}`)
        .then(res => {
            if (!res.ok) throw new Error('API trả về lỗi: ' + res.status);
            return res.json();
        })
        .then(result => {
            // hỗ trợ nhiều shape: { data: {...} } hoặc trực tiếp object
            const sp = result.data || result || result.value;
            if (!sp) {
                container.innerHTML = '<div class="col-12 text-center text-danger">Không có dữ liệu sản phẩm.</div>';
                console.error('API không trả dữ liệu sản phẩm', result);
                return;
            }

            const id = sp.maSanPham || sp.MaSanPham || productId;
            const name = sp.tenSanPham || sp.TenSanPham || 'Sản phẩm';
            const desc = sp.moTa || sp.MoTa || '';
            const priceVal = sp.giaTien || sp.GiaTien || 0;
            const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(priceVal);

            // normalize image path
            const rawImg = sp.hinhAnh || sp.HinhAnh || '/img/product/details/product-details-1.jpg';
            const img = (/^https?:\/\//i.test(rawImg) || rawImg.startsWith('/')) ? rawImg : '/' + rawImg;

            // build HTML (có thể tuỳ chỉnh structure cho giống template)
            container.innerHTML = `
                <div class="col-lg-6 col-md-6">
                    <div class="product__details__pic">
                        <img id="detail-img" src="${img}" alt="${name}" style="width:100%; max-height:500px; object-fit:cover;" />
                    </div>
                </div>
                <div class="col-lg-6 col-md-6">
                    <div class="product__details__text">
                        <h3 id="detail-name">${name}</h3>
                        <div class="product__details__price"><span id="detail-price">${price}</span></div>
                        <p id="detail-desc">${desc}</p>
                        <button id="add-to-cart" class="primary-btn">Thêm vào giỏ</button>
                    </div>
                </div>
            `;

            // add-to-cart handler (sử dụng hàm addToCart nếu đã có)
            const addBtn = document.getElementById('add-to-cart');
            if (addBtn) {
                addBtn.onclick = function () {
                    if (typeof addToCart === 'function') {
                        addToCart({
                            id: id,
                            name: name,
                            price: priceVal,
                            image: img
                        });
                    } else {
                        console.warn('Hàm addToCart chưa được định nghĩa.');
                        alert('Chức năng giỏ hàng hiện chưa khả dụng.');
                    }
                };
            }
        })
        .catch(err => {
            console.error('Lỗi khi gọi API chi tiết sản phẩm:', err);
            container.innerHTML = '<div class="col-12 text-center text-danger">Không thể tải thông tin sản phẩm. Vui lòng thử lại.</div>';
        });
});