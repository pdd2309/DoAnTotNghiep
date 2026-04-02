document.addEventListener('DOMContentLoaded', function () {
    fetch('/api/SanPhamApi')
        .then(response => response.json())
        .then(res => {
            const products = Array.isArray(res) ? res : (res.data || []);
            const productList = document.getElementById('product-list');
            if (!productList) return;

            let html = '';
            products.forEach(sp => {
                const id = sp.maSanPham || sp.MaSanPham;
                const name = sp.tenSanPham || sp.TenSanPham;
                const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(sp.giaTien || sp.GiaTien || 0);
                const img = sp.hinhAnh || sp.HinhAnh || 'img/featured/feature-1.jpg';

                html += `
                <div class="col-lg-3 col-md-4 col-sm-6">
                    <div class="featured__item">
                        <div class="featured__item__pic" onclick="location.href='shop-details.html?id=${id}'" style="background-image: url('${img}'); cursor:pointer;">
                            <ul class="featured__item__pic__hover">
                                <li><a href="#"><i class="fa fa-heart"></i></a></li>
                                <li><a href="#"><i class="fa fa-retweet"></i></a></li>
                                <li><a href="shoping-cart.html"><i class="fa fa-shopping-cart"></i></a></li>
                            </ul>
                        </div>
                        <div class="featured__item__text">
                            <h6><a href="shop-details.html?id=${id}">${name}</a></h6>
                            <h5>${price}</h5>
                        </div>
                    </div>
                </div>`;
            });
            productList.innerHTML = html;
        });
});