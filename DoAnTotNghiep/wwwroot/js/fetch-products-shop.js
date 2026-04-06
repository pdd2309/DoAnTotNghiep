document.addEventListener('DOMContentLoaded', function () {
    const productList = document.getElementById('product-list-shop');
    const categoryUl = document.getElementById('category-list-shop');
    if (!productList) return;

    // 1. Lấy tham số từ URL (searchString và categoryId)
    const urlParams = new URLSearchParams(window.location.search);
    const searchString = urlParams.get('searchString')?.toLowerCase() || "";
    const categoryId = urlParams.get('categoryId');

    // --- BƯỚC A: FETCH DANH MỤC ĐỔ VÀO SIDEBAR ---
    fetch('/api/DanhMuc')
        .then(response => response.json())
        .then(data => {
            let html = `<li><a href="/Home/Shop" style="${!categoryId ? 'color:#7fad39;font-weight:bold' : ''}">Tất cả</a></li>`;
            data.forEach(dm => {
                const id = dm.maDanhMuc || dm.MaDanhMuc;
                const name = dm.tenDanhMuc || dm.TenDanhMuc;
                // Nếu đang chọn danh mục này thì cho nó hiện màu xanh
                const activeClass = categoryId == id ? 'style="color:#7fad39;font-weight:bold"' : '';
                html += `<li><a href="/Home/Shop?categoryId=${id}" ${activeClass}>${name}</a></li>`;
            });
            if (categoryUl) categoryUl.innerHTML = html;
        })
        .catch(err => console.error("Lỗi fetch danh mục:", err));

    // --- BƯỚC B: FETCH VÀ LỌC SẢN PHẨM ---
    fetch('/api/SanPhamApi')
        .then(response => response.json())
        .then(res => {
            let products = res.data || [];

            // 2. Lọc theo searchString (nếu có)
            if (searchString) {
                products = products.filter(sp => {
                    const name = (sp.tenSanPham || sp.TenSanPham || "").toLowerCase();
                    return name.includes(searchString);
                });
            }

            // 3. Lọc theo categoryId (nếu có)
            if (categoryId) {
                products = products.filter(sp => {
                    const spCatId = sp.maDanhMuc || sp.MaDanhMuc;
                    return spCatId == categoryId;
                });
            }

            // Cập nhật số lượng
            const countElem = document.getElementById('product-count');
            if (countElem) countElem.innerText = products.length;

            if (products.length === 0) {
                productList.innerHTML = `<div class="col-12 text-center"><p>Không tìm thấy sản phẩm nào.</p></div>`;
                return;
            }

            // 4. Hiển thị
            let html = '';
            products.forEach(sp => {
                const id = sp.maSanPham || sp.MaSanPham;
                const name = sp.tenSanPham || sp.TenSanPham;
                const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(sp.giaTien || sp.GiaTien || 0);
                const raw = sp.hinhAnh || sp.HinhAnh || '/img/featured/feature-1.jpg';
                const img = (/^https?:\/\//i.test(raw) || raw.startsWith('/')) ? raw : '/' + raw;

                html += `
                <div class="col-lg-4 col-md-6 col-sm-6">
                    <div class="product__item">
                        <div class="product__item__pic set-bg" style="background-image: url('${img}'); background-size: cover; cursor: pointer;" onclick="location.href='/Home/Details/${id}'"></div>
                        <div class="product__item__text">
                            <h6><a href="/Home/Details/${id}">${name}</a></h6>
                            <h5>${price}</h5>
                        </div>
                    </div>
                </div>`;
            });
            productList.innerHTML = html;
        });
});