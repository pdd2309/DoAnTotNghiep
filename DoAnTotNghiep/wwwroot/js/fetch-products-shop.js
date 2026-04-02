document.addEventListener('DOMContentLoaded', function () {
    const productList = document.getElementById('product-list-shop');
    if (!productList) return;

    fetch('/api/SanPhamApi')
        .then(response => response.json())
        .then(res => {
            const products = res.data || [];
            document.getElementById('product-count').innerText = products.length;

            let html = '';
            products.forEach(sp => {
                const id = sp.maSanPham || sp.MaSanPham;
                const name = sp.tenSanPham || sp.TenSanPham;
                const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(sp.giaTien || sp.GiaTien || 0);

                // Normalize image path: ensure absolute (root-relative) or full URL
                const raw = sp.hinhAnh || sp.HinhAnh || '/img/featured/feature-1.jpg';
                const img = (/^https?:\/\//i.test(raw) || raw.startsWith('/')) ? raw : '/' + raw;

                html += `
                <div class="col-lg-4 col-md-6 col-sm-6">
                    <div class="product__item">
                        <div class="product__item__pic set-bg" style="background-image: url('${img}'); background-size: cover; cursor: pointer;" onclick="location.href='/Home/Details/${id}'">
                            <ul class="product__item__pic__hover">
                                <li><a href="#"><i class="fa fa-heart"></i></a></li>
                                <li><a href="#"><i class="fa fa-shopping-cart"></i></a></li>
                            </ul>
                        </div>
                        <div class="product__item__text">
                            <h6><a href="/Home/Details/${id}">${name}</a></h6>
                            <h5>${price}</h5>
                        </div>
                    </div>
                </div>`;
            });
            productList.innerHTML = html;
        })
        .catch(err => {
            console.error("Lỗi:", err);
            productList.innerHTML = '<p class="text-danger">Không thể tải sản phẩm. Đông kiểm tra lại API nhé!</p>';
        });
});