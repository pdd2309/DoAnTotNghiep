// Hàm gọi API để cập nhật trạng thái đơn hàng
async function xacNhanDaNhanHang(orderId) {
    if (!confirm("Ông chắc chắn đã nhận được hàng đúng không?")) return;

    try {
        const response = await fetch(`/api/OrderApi/ConfirmReceived/${orderId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' }
        });

        const result = await response.json();
        if (response.ok && result.success) {
            alert(result.message);
            location.reload(); // Load lại trang để cập nhật giao diện
        } else {
            alert(result.message || "Lỗi rồi ông ơi!");
        }
    } catch (error) {
        console.error("Lỗi:", error);
        alert("Không kết nối được server.");
    }
}