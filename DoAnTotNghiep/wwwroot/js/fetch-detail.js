document.addEventListener('DOMContentLoaded', function () {
    const urlParams = new URLSearchParams(window.location.search);
    const productId = urlParams.get('id');

    if (!productId) {
        console.error("Lỗi: Không tìm thấy ID sản phẩm trên link!");
        return;
    }

    // Gọi API lấy dữ liệu
    fetch(`/api/SanPhamApi/${productId}`)
        .then(res => res.json())
        .then(result => {
            // ĐOẠN NÀY QUAN TRỌNG: Kiểm tra xem dữ liệu nằm ở đâu
            // Thử lấy từ result, nếu không có thì thử result.data, nếu không có nữa thì thử result.value
            const sp = result.value || result.data || result;

            console.log("Dữ liệu thực tế nhận được:", sp);

            if (!sp || (!sp.tenSanPham && !sp.TenSanPham)) {
                console.error("Lỗi: API trả về nhưng không có dữ liệu sản phẩm!", sp);
                return;
            }

            // 1. Đổ Tên (Check cả viết Hoa và viết Thường)
            const ten = sp.tenSanPham || sp.TenSanPham || "Tên không xác định";
            if (document.getElementById('detail-name')) {
                document.getElementById('detail-name').innerText = ten;
            }

            // 2. Đổ Giá (Check cả viết Hoa và viết Thường)
            const giaRaw = sp.giaTien || sp.GiaTien || 0;
            if (document.getElementById('detail-price')) {
                document.getElementById('detail-price').innerText = new Intl.NumberFormat('vi-VN', {
                    style: 'currency',
                    currency: 'VND'
                }).format(giaRaw);
            }

            // 3. Đổ Mô Tả
            const mota = sp.moTa || sp.MoTa || "Sản phẩm công nghệ chính hãng.";
            if (document.getElementById('detail-desc')) {
                document.getElementById('detail-desc').innerText = mota;
            }

            // 4. Đổ Ảnh
            const anh = sp.hinhAnh || sp.HinhAnh;
            const imgTag = document.getElementById('detail-img');
            if (imgTag) {
                // Nếu có ảnh trong SQL thì lấy, không thì lấy ảnh mặc định
                imgTag.src = anh ? anh : 'img/product/details/product-details-1.jpg';
            }
        })
        .catch(err => {
            console.error("Lỗi kết nối API:", err);
        });
});