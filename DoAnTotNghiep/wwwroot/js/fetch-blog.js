document.addEventListener('DOMContentLoaded', function () {
    // Gọi API lấy dữ liệu bài viết
    fetch('/api/BaiVietApi')
        .then(response => response.json())
        .then(data => {
            const blogList = document.getElementById('blog-list');
            if (!blogList) return; // Nếu không tìm thấy chỗ chứa thì bỏ qua

            let html = '';

            // Duyệt qua từng bài viết từ SQL và tạo mã HTML
            data.forEach(blog => {
                const date = new Date(blog.ngayDang).toLocaleDateString('vi-VN');

                html += `
                <div class="col-lg-6 col-md-6 col-sm-6">
                    <div class="blog__item">
                        <div class="blog__item__pic">
                            <img src="${blog.hinhAnh}" alt="${blog.tieuDe}">
                        </div>
                        <div class="blog__item__text">
                            <ul>
                                <li><i class="fa fa-calendar-o"></i> ${date}</li>
                                <li><i class="fa fa-user-o"></i> ${blog.tacGia || 'Admin'}</li>
                            </ul>
                            <h5><a href="blog-details.html?id=${blog.maBv}">${blog.tieuDe}</a></h5>
                            <p>${blog.noiDung ? blog.noiDung.substring(0, 100) : ''}...</p>
                            <a href="blog-details.html?id=${blog.maBv}" class="blog__btn">ĐỌC THÊM <span class="arrow_right"></span></a>
                        </div>
                    </div>
                </div>`;
            });

            blogList.innerHTML = html;
        })
        .catch(error => console.error('Lỗi khi tải dữ liệu bài viết:', error));
});