/** * Lấy danh mục từ API và hiển thị động 
 */
async function loadCategories() {
    try {
        // KIỂM TRA: Nếu API của ông là DanhMucController thì link thường là /api/DanhMuc
        // Tui để /api/DanhMuc cho khớp với file C# ông gửi nhé
        const response = await fetch('/api/DanhMuc');

        if (!response.ok) {
            throw new Error('Lỗi khi tải danh mục từ API');
        }

        const categories = await response.json();

        // Render cho hero section (Thanh menu dọc ở Trang chủ)
        renderHeroCategories(categories);

        // Render cho sidebar (Cột bên trái ở trang Shop)
        renderSidebarCategories(categories);

    } catch (error) {
        console.error('Lỗi tải danh mục:', error);
    }
}

/** * Render danh mục trong hero section (Index)
 */
function renderHeroCategories(categories) {
    // Ưu tiên tìm theo ID đã đặt trong Index.cshtml
    const categoryList = document.getElementById('category-list-index') || document.querySelector('.hero__categories ul');

    if (!categoryList) return;

    categoryList.innerHTML = categories
        .map(cat => {
            // Check cả hoa thường để tránh lỗi undefined
            const id = cat.maDanhMuc || cat.MaDanhMuc;
            const name = cat.tenDanhMuc || cat.TenDanhMuc;

            if (!id || !name) return ''; // Bỏ qua nếu dữ liệu lỗi
            return `<li><a href="/Home/Shop?categoryId=${id}">${name}</a></li>`;
        })
        .join('');
}

/** * Render danh mục trong sidebar (Shop)
 */
function renderSidebarCategories(categories) {
    const sidebarUl = document.getElementById('category-list-shop');

    if (!sidebarUl) return;

    const html = categories
        .map(cat => {
            const id = cat.maDanhMuc || cat.MaDanhMuc;
            const name = cat.tenDanhMuc || cat.TenDanhMuc;

            if (!id || !name) return '';
            return `<li><a href="/Home/Shop?categoryId=${id}">${name}</a></li>`;
        })
        .join('');

    // Thêm nút "Tất cả" lên đầu để khách dễ quay lại xem toàn bộ máy
    sidebarUl.innerHTML = `<li><a href="/Home/Shop">Tất cả sản phẩm</a></li>` + html;
}

// Gọi hàm ngay khi trang web tải xong
document.addEventListener('DOMContentLoaded', loadCategories);