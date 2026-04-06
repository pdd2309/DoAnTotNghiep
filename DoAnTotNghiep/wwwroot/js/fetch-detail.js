document.addEventListener('DOMContentLoaded', function () {
    const container = document.getElementById('product-detail-content');
    if (!container) return;

    if (typeof productId === 'undefined' || !productId) {
        container.innerHTML = '<div class="col-12 text-center text-danger">Không tìm thấy ID sản phẩm.</div>';
        return;
    }

    // --- BƯỚC 1: LOAD CHI TIẾT SẢN PHẨM ---
    fetch(`/api/SanPhamApi/${productId}`)
        .then(res => res.json())
        .then(result => {
            const sp = result.data || result || result.value;
            if (!sp) return;

            const id = sp.maSanPham || sp.MaSanPham || productId;
            const name = sp.tenSanPham || sp.TenSanPham || 'Sản phẩm';
            const desc = sp.moTa || sp.MoTa || '';
            const priceVal = sp.giaTien || sp.GiaTien || 0;
            const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(priceVal);
            const rawImg = sp.hinhAnh || sp.HinhAnh || '/img/product/details/product-details-1.jpg';
            const img = (/^https?:\/\//i.test(rawImg) || rawImg.startsWith('/')) ? rawImg : '/' + rawImg;

            container.innerHTML = `
                <div class="col-lg-6 col-md-6">
                    <div class="product__details__pic">
                        <img id="detail-img" src="${img}" alt="${name}" style="width:100%; max-height:500px; object-fit:cover;" />
                    </div>
                </div>
                <div class="col-lg-6 col-md-6">
                    <div class="product__details__text">
                        <h3>${name}</h3>
                        <div class="product__details__price"><span>${price}</span></div>
                        <p>${desc}</p>
                        <button id="add-to-cart" class="primary-btn">Thêm vào giỏ</button>
                    </div>
                </div>`;

            document.getElementById('add-to-cart').onclick = () => {
                if (typeof addToCart === 'function') {
                    addToCart({ id, name, price: priceVal, image: img });
                }
            };

            // Sau khi load xong máy thì load đánh giá
            loadReviews(productId);
        });
});

// --- BƯỚC 2: HÀM LOAD ĐÁNH GIÁ ---
async function loadReviews(id) {
    try {
        // ✅ ĐÃ FIX: Đường dẫn gọi đúng /api/SanPhamApi/...
        const res = await fetch(`/api/SanPhamApi/${id}/reviews`);
        if (!res.ok) throw new Error('Lỗi khi tải đánh giá');
        const reviews = await res.json();

        const reviewListEl = document.getElementById('review-list');
        const reviewCountEl = document.getElementById('review-count');

        if (reviewCountEl) reviewCountEl.innerText = `(${reviews.length || 0})`;
        if (!reviewListEl) return;

        if (!reviews.length) {
            reviewListEl.innerHTML = '<p class="text-center">Chưa có đánh giá nào. Hãy là người đầu tiên!</p>';
            return;
        }

        let html = '';
        reviews.forEach(review => {
            const stars = '★'.repeat(review.soSao) + '☆'.repeat(5 - review.soSao);
            html += `
            <div class="review-item mb-4 pb-4" style="border-bottom: 1px solid #ebebeb;">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <strong>${review.tenNguoiDung}</strong>
                        <span class="text-warning ml-3">${stars}</span>
                    </div>
                    <small class="text-muted">${review.ngay}</small>
                </div>
                <p class="mt-2 mb-0">${review.noiDung}</p>
            </div>`;
        });
        reviewListEl.innerHTML = html;
    } catch (err) {
        console.error('Lỗi load reviews:', err);
    }
}

// --- BƯỚC 3: HÀM GỬI ĐÁNH GIÁ ---
window.sendReview = async function () {
    const contentEl = document.getElementById('review-content');
    const starsEl = document.getElementById('review-stars');
    const content = contentEl?.value?.trim() || '';
    const stars = parseInt(starsEl?.value) || 5;

    if (!content) {
        alert('Vui lòng nhập nội dung đánh giá');
        return;
    }

    try {
        // ✅ ĐÃ FIX: Đường dẫn gọi đúng /api/SanPhamApi/reviews
        const res = await fetch('/api/SanPhamApi/reviews', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                maSanPham: parseInt(productId),
                soSao: stars,
                noiDung: content
            })
        });

        const result = await res.json();

        if (!res.ok) {
            alert(result.message || 'Lỗi gửi đánh giá');
            return;
        }

        alert('Đánh giá thành công!');
        contentEl.value = '';
        await loadReviews(productId);
    } catch (err) {
        console.error('Lỗi gửi đánh giá:', err);
        alert('Có lỗi xảy ra, vui lòng thử lại.');
    }
};