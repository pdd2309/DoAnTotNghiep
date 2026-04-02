document.addEventListener('DOMContentLoaded', function () {
    const productList = document.getElementById('product-list');
    if (!productList) return;

    // Gọi đúng tên Controller: SanPhamApi
    fetch('/api/SanPhamApi')
        .then(response => response.json())
        .then(res => {
            // API trả về Object có chứa field "data"
            const products = res.data || [];

            let html = '';
            products.forEach(sp => {
                // Sửa lại để khớp với tên thuộc tính C# (Viết hoa chữ cái đầu)
                const id = sp.maSanPham || sp.MaSanPham;
                const name = sp.tenSanPham || sp.TenSanPham;
                const giaRaw = sp.giaTien || sp.GiaTien || 0;
                const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(giaRaw);
                const img = sp.hinhAnh || sp.HinhAnh || '/img/featured/feature-1.jpg';

                html += `
                <div class="col-lg-3 col-md-4 col-sm-6">
                    <div class="featured__item">
                        <div class="featured__item__pic" onclick="location.href='/Home/Details/${id}'" style="background-image: url('${img}'); cursor:pointer; background-size: cover; background-position: center;">
                            <ul class="featured__item__pic__hover">
                                <li><a href="#"><i class="fa fa-heart"></i></a></li>
                                <li><a href="#"><i class="fa fa-retweet"></i></a></li>
                                <li><a href="#"><i class="fa fa-shopping-cart"></i></a></li>
                            </ul>
                        </div>
                        <div class="featured__item__text">
                            <h6><a href="/Home/Details/${id}">${name}</a></h6>
                            <h5>${price}</h5>
                        </div>
                    </div>
                </div>`;
            });
            productList.innerHTML = html;
        })
        .catch(err => console.error("Lỗi lấy dữ liệu sản phẩm:", err));
});