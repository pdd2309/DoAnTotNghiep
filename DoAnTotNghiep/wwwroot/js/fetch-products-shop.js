document.addEventListener('DOMContentLoaded', function () {
    const productList = document.getElementById('product-list-shop');
    const categoryUl = document.getElementById('category-list-shop');
    if (!productList) return;

    let allProducts = []; // Biến lưu toàn bộ sản phẩm để lọc giá nhanh

    const urlParams = new URLSearchParams(window.location.search);
    const searchString = urlParams.get('searchString')?.toLowerCase() || "";
    const categoryId = urlParams.get('categoryId');

    // --- BƯỚC A: FETCH DANH MỤC ---
    fetch('/api/DanhMuc')
        .then(response => response.json())
        .then(data => {
            let html = `<li><a href="/Home/Shop" style="${!categoryId ? 'color:#7fad39;font-weight:bold' : ''}">Tất cả</a></li>`;
            data.forEach(dm => {
                const id = dm.maDanhMuc || dm.MaDanhMuc;
                const name = dm.tenDanhMuc || dm.TenDanhMuc;
                const activeClass = categoryId == id ? 'style="color:#7fad39;font-weight:bold"' : '';
                html += `<li><a href="/Home/Shop?categoryId=${id}" ${activeClass}>${name}</a></li>`;
            });
            if (categoryUl) categoryUl.innerHTML = html;
        });

    // --- BƯỚC B: FETCH SẢN PHẨM ---
    fetch('/api/SanPhamApi')
        .then(response => response.json())
        .then(res => {
            allProducts = res.data || [];

            // Lọc sơ bộ theo Search và Category từ URL
            let filtered = allProducts;
            if (searchString) {
                filtered = filtered.filter(sp => (sp.tenSanPham || "").toLowerCase().includes(searchString));
            }
            if (categoryId) {
                filtered = filtered.filter(sp => (sp.maDanhMuc || sp.MaDanhMuc) == categoryId);
            }

            // Lưu lại danh sách sau khi lọc URL để thanh giá kéo trên đống này
            renderProducts(filtered);
            initPriceSlider(filtered);
        });

    // HÀM VẼ SẢN PHẨM
    function renderProducts(products) {
        const countElem = document.getElementById('product-count');
        if (countElem) countElem.innerText = products.length;

        if (products.length === 0) {
            productList.innerHTML = `<div class="col-12 text-center"><p>Không tìm thấy sản phẩm nào.</p></div>`;
            return;
        }

        let html = '';
        products.forEach(sp => {
            const id = sp.maSanPham || sp.MaSanPham;
            const name = sp.tenSanPham || sp.TenSanPham;
            const priceVal = sp.giaTien || sp.GiaTien || 0;
            const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(priceVal);
            const raw = sp.hinhAnh || sp.HinhAnh || '/img/featured/feature-1.jpg';
            const img = (raw.startsWith('http') || raw.startsWith('/')) ? raw : '/' + raw;

            html += `
            <div class="col-lg-4 col-md-6 col-sm-6">
                <div class="product__item">
                    <div class="product__item__pic" style="background-image: url('${img}'); background-size: cover; cursor: pointer;" onclick="location.href='/Home/Details/${id}'"></div>
                    <div class="product__item__text">
                        <h6><a href="/Home/Details/${id}">${name}</a></h6>
                        <h5>${price}</h5>
                    </div>
                </div>
            </div>`;
        });
        productList.innerHTML = html;
    }

    // HÀM KÍCH HOẠT THANH GIÁ (Khớp tiền VNĐ)
    function initPriceSlider(currentList) {
        const rangeSlider = $(".price-range"),
            minamount = $("#minamount"),
            maxamount = $("#maxamount");

        rangeSlider.slider({
            range: true,
            min: 0,
            max: 50000000, // Tối đa 50 triệu
            values: [0, 50000000],
            slide: function (event, ui) {
                minamount.val(new Intl.NumberFormat('vi-VN').format(ui.values[0]) + "đ");
                maxamount.val(new Intl.NumberFormat('vi-VN').format(ui.values[1]) + "đ");
            },
            stop: function (event, ui) {
                // Lọc sản phẩm theo giá khi buông chuột
                const filteredByPrice = currentList.filter(p => p.giaTien >= ui.values[0] && p.giaTien <= ui.values[1]);
                renderProducts(filteredByPrice);
            }
        });
        // Hiển thị giá mặc định ban đầu
        minamount.val("0đ");
        maxamount.val("50.000.000đ");
    }
});