async function loadCategories() {
    try {
        const response = await fetch('/api/DanhMuc');
        if (!response.ok) throw new Error('Lỗi API');
        let categories = await response.json();

        categories = (categories || [])
            .filter(c => {
                const visible = c.isHienThiTrangChu ?? c.IsHienThiTrangChu;
                return visible === undefined ? true : !!visible;
            })
            .sort((a, b) => {
                const aOrder = (a.thuTuHienThi ?? a.ThuTuHienThi ?? 0);
                const bOrder = (b.thuTuHienThi ?? b.ThuTuHienThi ?? 0);
                return aOrder - bOrder;
            });

        // 1. Đổ dữ liệu vào thanh menu dọc (Hero)
        renderHeroCategories(categories);

        // 2. Đổ dữ liệu vào Slider hình động (Khúc ông vừa hỏi)
        renderSliderCategories(categories);

    } catch (error) {
        console.error('Lỗi:', error);
    }
}

function renderSliderCategories(categories) {
    const slider = document.getElementById('category-slider-list');
    if (!slider) return;

    // Xóa slider cũ nếu có để tránh trùng lặp
    if ($(slider).hasClass('owl-loaded')) {
        $(slider).trigger('destroy.owl.carousel');
        $(slider).removeClass('owl-loaded');
        $(slider).find('.owl-stage-outer').children().unwrap();
    }

    slider.innerHTML = categories.map(cat => {
        const id = cat.maDanhMuc || cat.MaDanhMuc;
        const name = cat.tenDanhMuc || cat.TenDanhMuc;

        // Nếu DB có ảnh thì ưu tiên ảnh DB, không thì fallback ảnh mặc định
        let img = cat.hinhAnh || cat.HinhAnh || `/img/categories/cat-${id % 5 + 1}.jpg`;

        return `
            <div class="col-lg-3">
                <div class="categories__item set-bg" data-setbg="${img}" style="background-image: url('${img}');">
                    <h5><a href="/Home/Shop?categoryId=${id}">${name}</a></h5>
                </div>
            </div>`;
    }).join('');

    // --- KÍCH HOẠT OWL CAROUSEL ---
    $(slider).owlCarousel({
        loop: true,
        margin: 0,
        items: 4,
        dots: false,
        nav: true,
        navText: ["<span class='fa fa-angle-left'><span/>", "<span class='fa fa-angle-right'><span/>"],
        smartSpeed: 1200,
        autoHeight: false,
        autoplay: true,
        responsive: {
            0: { items: 1 },
            480: { items: 2 },
            768: { items: 3 },
            992: { items: 4 }
        }
    });
}

function renderHeroCategories(categories) {
    const list = document.getElementById('category-list-index');
    if (!list) return;
    list.innerHTML = categories.map(cat => {
        const id = cat.maDanhMuc || cat.MaDanhMuc;
        const name = cat.tenDanhMuc || cat.TenDanhMuc;
        return `<li><a href="/Home/Shop?categoryId=${id}">${name}</a></li>`;
    }).join('');
}

document.addEventListener('DOMContentLoaded', loadCategories);