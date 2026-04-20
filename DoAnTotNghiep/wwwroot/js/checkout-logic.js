document.addEventListener('DOMContentLoaded', async function () {
    const listElement = document.getElementById('checkout-list');
    const totalElement = document.getElementById('checkout-total');
    const subtotalElement = document.getElementById('checkout-subtotal');
    const discountElement = document.getElementById('checkout-discount');
    const voucherMessageElement = document.getElementById('voucher-message');
    const voucherInput = document.getElementById('voucher-code');
    const applyVoucherButton = document.getElementById('btn-apply-voucher');

    const savedAddressSelect = document.getElementById('saved-address-select');
    const saveAddressCheck = document.getElementById('save-address-check');
    const addressBookMessage = document.getElementById('address-book-message');

    if (!listElement) return;

    const fmt = v => new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(v || 0);

    let currentSubTotal = 0;
    let discountAmount = 0;
    let appliedVoucherCode = '';
    let savedAddresses = [];

    function setVoucherMessage(text, isError) {
        if (!voucherMessageElement) return;
        voucherMessageElement.textContent = text || '';
        voucherMessageElement.style.color = isError ? '#dc3545' : '#28a745';
    }

    function setAddressBookMessage(text, isError) {
        if (!addressBookMessage) return;
        addressBookMessage.textContent = text || '';
        addressBookMessage.style.color = isError ? '#dc3545' : '#28a745';
    }

    function renderTotals() {
        const finalTotal = Math.max(0, currentSubTotal - discountAmount);
        if (subtotalElement) subtotalElement.innerText = fmt(currentSubTotal);
        if (discountElement) discountElement.innerText = fmt(discountAmount);
        if (totalElement) totalElement.innerText = fmt(finalTotal);
    }

    function showPaymentResultFromQuery() {
        const url = new URL(window.location.href);
        const status = url.searchParams.get('paymentStatus');
        const orderId = url.searchParams.get('orderId');

        if (status === 'success') {
            alert(`Thanh to\u00E1n VNPay th\u00E0nh c\u00F4ng. M\u00E3 \u0111\u01A1n h\u00E0ng: #${orderId || ''}`);
            localStorage.removeItem('shoppingCart');
            window.history.replaceState({}, document.title, '/Home/Checkout');
        } else if (status === 'failed') {
            alert('Thanh to\u00E1n VNPay kh\u00F4ng th\u00E0nh c\u00F4ng. Vui l\u00F2ng th\u1EED l\u1EA1i.');
            window.history.replaceState({}, document.title, '/Home/Checkout');
        }
    }

    function initPaymentMethodUi() {
        const radios = document.querySelectorAll('input[name="payment-method"]');
        const refreshActive = () => {
            document.querySelectorAll('.payment-option').forEach(label => label.classList.remove('active'));
            const checked = document.querySelector('input[name="payment-method"]:checked');
            if (checked) {
                const label = checked.closest('.payment-option');
                if (label) label.classList.add('active');
            }
        };

        radios.forEach(r => r.addEventListener('change', refreshActive));
        refreshActive();
    }

    async function loadOrderSummary() {
        try {
            const res = await fetch('/api/Cart');
            if (!res.ok) throw new Error('Kh\u00F4ng th\u1EC3 t\u1EA3i gi\u1ECF h\u00E0ng');

            const cart = await res.json();
            const items = cart.items || cart.Items || [];

            if (items.length === 0) {
                alert('Gi\u1ECF h\u00E0ng \u0111ang tr\u1ED1ng.');
                window.location.href = '/';
                return;
            }

            let html = '';
            let total = 0;

            items.forEach(it => {
                const price = parseFloat(it.price) || 0;
                const qty = parseInt(it.quantity) || 0;
                const sub = price * qty;
                total += sub;
                html += `<li>${it.name} (x${qty}) <span>${fmt(sub)}</span></li>`;
            });

            currentSubTotal = total;
            listElement.innerHTML = html;
            renderTotals();
        } catch (err) {
            console.error('Load summary failed:', err);
            alert('Kh\u00F4ng th\u1EC3 t\u1EA3i d\u1EEF li\u1EC7u gi\u1ECF h\u00E0ng.');
        }
    }

    async function applyVoucher() {
        if (!voucherInput) return;

        const code = (voucherInput.value || '').trim();
        if (!code) {
            appliedVoucherCode = '';
            discountAmount = 0;
            setVoucherMessage('\u0110\u00E3 b\u1ECF \u00E1p d\u1EE5ng m\u00E3 gi\u1EA3m gi\u00E1.', false);
            renderTotals();
            return;
        }

        try {
            const response = await fetch('/api/OrderApi/ValidateVoucher', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ voucherCode: code, subTotal: currentSubTotal })
            });

            const payload = await response.json();
            if (!response.ok) {
                appliedVoucherCode = '';
                discountAmount = 0;
                setVoucherMessage(payload.message || 'M\u00E3 gi\u1EA3m gi\u00E1 kh\u00F4ng h\u1EE3p l\u1EC7.', true);
                renderTotals();
                return;
            }

            appliedVoucherCode = payload.voucherCode || code;
            discountAmount = Number(payload.discountAmount || 0);
            setVoucherMessage(`\u00C1p d\u1EE5ng m\u00E3 ${appliedVoucherCode} th\u00E0nh c\u00F4ng.`, false);
            renderTotals();
        } catch (err) {
            console.error('Apply voucher failed:', err);
            setVoucherMessage('Kh\u00F4ng th\u1EC3 ki\u1EC3m tra m\u00E3 gi\u1EA3m gi\u00E1.', true);
        }
    }

    function composeAddress(item) {
        const parts = [item.addressLine, item.ward, item.district, item.province].filter(x => !!x && String(x).trim() !== '');
        return parts.join(', ');
    }

    function fillAddressForm(item) {
        const nameInput = document.getElementById('order-name');
        const phoneInput = document.getElementById('order-phone');
        const addressInput = document.getElementById('order-address');

        if (nameInput) nameInput.value = item.fullName || '';
        if (phoneInput) phoneInput.value = item.phone || '';
        if (addressInput) addressInput.value = composeAddress(item);
    }

    async function loadSavedAddresses() {
        if (!savedAddressSelect) return;

        try {
            const res = await fetch('/api/AddressBookApi/My');
            if (res.status === 401) {
                savedAddressSelect.style.display = 'none';
                return;
            }
            if (!res.ok) throw new Error('Load address failed');

            savedAddresses = await res.json();

            savedAddressSelect.innerHTML = '<option value="">Ch\u1ECDn \u0111\u1ECBa ch\u1EC9 \u0111\u00E3 l\u01B0u</option>';
            savedAddresses.forEach(item => {
                const text = `${item.fullName || ''} - ${item.phone || ''} - ${composeAddress(item)}`;
                const opt = document.createElement('option');
                opt.value = item.id;
                opt.textContent = item.isDefault ? `${text} (m\u1EB7c \u0111\u1ECBnh)` : text;
                savedAddressSelect.appendChild(opt);
            });

            if (window.jQuery && typeof window.jQuery.fn.niceSelect === 'function') {
                const $select = window.jQuery(savedAddressSelect);
                if ($select.next('.nice-select').length) {
                    $select.niceSelect('update');
                }
            }

            const defaultItem = savedAddresses.find(x => x.isDefault);
            if (defaultItem) {
                savedAddressSelect.value = String(defaultItem.id);

                if (window.jQuery && typeof window.jQuery.fn.niceSelect === 'function') {
                    const $select = window.jQuery(savedAddressSelect);
                    if ($select.next('.nice-select').length) {
                        $select.niceSelect('update');
                    }
                }

                fillAddressForm(defaultItem);
            }

            applySelectedAddress();
        } catch (err) {
            console.error(err);
            setAddressBookMessage('Kh\u00F4ng t\u1EA3i \u0111\u01B0\u1EE3c s\u1ED5 \u0111\u1ECBa ch\u1EC9.', true);
        }
    }

    function applySelectedAddress() {
        if (!savedAddressSelect) return;

        const id = savedAddressSelect.value;
        if (!id) return;

        const selected = savedAddresses.find(x => String(x.id) === String(id));
        if (selected) {
            fillAddressForm(selected);
            setAddressBookMessage('\u0110\u00E3 \u0111i\u1EC1n th\u00F4ng tin t\u1EEB \u0111\u1ECBa ch\u1EC9 \u0111\u00E3 l\u01B0u.', false);
        }
    }

    async function saveCurrentAddressIfNeeded() {
        if (!saveAddressCheck || !saveAddressCheck.checked) return;

        const payload = {
            fullName: (document.getElementById('order-name')?.value || '').trim(),
            phone: (document.getElementById('order-phone')?.value || '').trim(),
            addressLine: (document.getElementById('order-address')?.value || '').trim(),
            ward: null,
            district: null,
            province: null,
            isDefault: false
        };

        if (!payload.fullName || !payload.phone || !payload.addressLine) return;

        const exists = savedAddresses.some(x =>
            (x.fullName || '').trim() === payload.fullName &&
            (x.phone || '').trim() === payload.phone &&
            composeAddress(x).trim() === payload.addressLine);

        if (exists) return;

        try {
            const res = await fetch('/api/AddressBookApi', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (res.ok) {
                setAddressBookMessage('\u0110\u00E3 l\u01B0u \u0111\u1ECBa ch\u1EC9 m\u1EDBi.', false);
                await loadSavedAddresses();
            }
        } catch (err) {
            console.error(err);
        }
    }

    function getOrderData() {
        return {
            HoTen: (document.getElementById('order-name')?.value || '').trim(),
            DiaChi: (document.getElementById('order-address')?.value || '').trim(),
            SDT: (document.getElementById('order-phone')?.value || '').trim(),
            Email: (document.getElementById('order-email')?.value || '').trim(),
            GhiChu: (document.getElementById('order-note')?.value || '').trim(),
            VoucherCode: appliedVoucherCode || null
        };
    }

    function validateOrderData(orderData) {
        return !!(orderData.HoTen && orderData.DiaChi && orderData.SDT);
    }

    async function placeCodOrder(orderData) {
        const response = await fetch('/api/OrderApi/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(orderData)
        });

        if (!response.ok) {
            const errorMsg = await response.text();
            throw new Error(errorMsg || 'Create order failed');
        }

        const result = await response.json();
        alert('\u0110\u1EB7t h\u00E0ng th\u00E0nh c\u00F4ng. M\u00E3 \u0111\u01A1n: #' + result.orderId);
        localStorage.removeItem('shoppingCart');
        window.location.href = '/';
    }

    async function redirectToVnPay(orderData) {
        const response = await fetch('/api/OrderApi/CreateVnPayPayment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(orderData)
        });

        if (!response.ok) {
            const errorMsg = await response.text();
            throw new Error(errorMsg || 'Create VNPay payment failed');
        }

        const result = await response.json();
        if (!result.paymentUrl) throw new Error('Missing payment URL');

        window.location.href = result.paymentUrl;
    }

    const btnPlaceOrder = document.getElementById('btn-place-order');
    if (btnPlaceOrder) {
        btnPlaceOrder.addEventListener('click', async function () {
            const orderData = getOrderData();
            if (!validateOrderData(orderData)) {
                alert('Vui l\u00F2ng nh\u1EADp \u0111\u1EA7y \u0111\u1EE7 th\u00F4ng tin b\u1EAFt bu\u1ED9c.');
                return;
            }

            const paymentMethod = document.querySelector('input[name="payment-method"]:checked')?.value || 'COD';

            try {
                await saveCurrentAddressIfNeeded();

                if (paymentMethod === 'VNPAY') {
                    await redirectToVnPay(orderData);
                } else {
                    await placeCodOrder(orderData);
                }
            } catch (err) {
                console.error('Checkout error:', err);
                alert('C\u00F3 l\u1ED7i x\u1EA3y ra: ' + (err.message || 'Unknown error'));
            }
        });
    }

    if (savedAddressSelect) {
        savedAddressSelect.addEventListener('change', applySelectedAddress);

        if (window.jQuery) {
            window.jQuery(document).on('click', '#saved-address-select + .nice-select .option', function () {
                setTimeout(applySelectedAddress, 0);
            });
        }
    }

    if (applyVoucherButton) {
        applyVoucherButton.addEventListener('click', applyVoucher);
    }

    if (voucherInput) {
        voucherInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                applyVoucher();
            }
        });
    }

    showPaymentResultFromQuery();
    initPaymentMethodUi();
    await loadOrderSummary();
    await loadSavedAddresses();
});
