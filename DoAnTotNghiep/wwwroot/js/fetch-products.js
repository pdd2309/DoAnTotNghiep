document.addEventListener('DOMContentLoaded', function () {
    // Gọi API lấy danh sách sản phẩm
    fetch('/api/SanPhamApi')
        .then(response => response.json())
        .then(res => {
            console.log("Dữ liệu nhận được từ API:", res); // Dòng này để Đông kiểm tra trong F12

            // XỬ LÝ LỖI forEach: Kiểm tra xem res là mảng hay bị bọc trong đối tượng (res.data)
            const products = Array.isArray(res) ? res : (res.data || []);

            const productList = document.getElementById('product-list');
            if (!productList) {
                console.error("Không tìm thấy thẻ có id='product-list' trong index.html");
                return;
            }

            let html = '';

            // Duyệt qua danh sách sản phẩm đã xử lý
            products.forEach(sp => {
                // 1. Lấy giá tiền (Dùng GiaTien hoặc giaTien tùy theo API trả về)
                const rawPrice = sp.giaTien || sp.GiaTien || 0;
                const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(rawPrice);

                // 2. Lấy tên sản phẩm
                const name = sp.tenSanPham || sp.TenSanPham || "Sản phẩm chưa có tên";

                // 3. Xử lý hình ảnh (Nếu NULL trong SQL thì lấy ảnh mặc định)
                const image = sp.hinhAnh || sp.HinhAnh || 'img/featured/feature-1.jpg';

                // 4. Lấy mã sản phẩm cho link chi tiết
                const id = sp.maSanPham || sp.MaSanPham || 0;

                // Tạo HTML theo chuẩn Ogani
                html += `
                <div class="col-lg-3 col-md-4 col-sm-6">
                    <div class="featured__item">
                        <div class="featured__item__pic" style="background-image: url('${image}'); background-size: cover; background-position: center;">
                            <ul class="featured__item__pic__hover">
                                <li><a href="#"><i class="fa fa-heart"></i></a></li>
                                <li><a href="#"><i class="fa fa-retweet"></i></a></li>
                                <li><a href="#"><i class="fa fa-shopping-cart"></i></a></li>
                            </ul>
                        </div>
                        <div class="featured__item__text">
                            <h6><a href="shop-details.html?id=${id}">${name}</a></h6>
                            <h5>${price}</h5>
                        </div>
                    </div>
                </div>`;
            });

            // Đổ dữ liệu vào trang web
            productList.innerHTML = html;
        })
        .catch(error => {
            console.error('Lỗi kết nối API:', error);
        });
});