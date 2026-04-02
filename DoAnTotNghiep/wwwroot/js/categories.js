/** 
 * Lấy danh mục từ API và hiển thị động 
 */
async function loadCategories() {
    try {
        const response = await fetch('/api/danhmuc');
        
        if (!response.ok) {
            throw new Error('Lỗi khi tải danh mục');
        }
        
        const categories = await response.json();
        
        // Render cho hero section
        renderHeroCategories(categories);
        
        // Render cho sidebar (nếu có)
        renderSidebarCategories(categories);
        
    } catch (error) {
        console.error('Lỗi tải danh mục:', error);
    }
}

/** 
 * Render danh mục trong hero section 
 */
function renderHeroCategories(categories) {
    const categoryList = document.querySelector('.hero__categories ul');
    
    if (!categoryList) return;
    
    categoryList.innerHTML = categories
        .map(cat => `<li><a href="/shop-grid?categoryId=${cat.maDanhMuc}">${cat.tenDanhMuc}</a></li>`)
        .join('');
}

/** 
 * Render danh mục trong sidebar 
 */
function renderSidebarCategories(categories) {
    const sidebarItem = document.querySelector('.sidebar__item');
    
    if (!sidebarItem) return;
    
    const categoryHTML = `
        <h4>Danh Mục</h4>
        <ul>
            ${categories.map(cat => `<li><a href="/shop-grid?categoryId=${cat.maDanhMuc}">${cat.tenDanhMuc}</a></li>`).join('')}
        </ul>
    `;
    
    sidebarItem.innerHTML = categoryHTML;
}

// Gọi hàm khi trang load xong
document.addEventListener('DOMContentLoaded', loadCategories);