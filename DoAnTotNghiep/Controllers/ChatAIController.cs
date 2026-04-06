using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DoAnTotNghiep.Controllers
{
    public class ChatRequest { public string message { get; set; } }

    [Route("api/[controller]")]
    [ApiController]
    public class ChatAI : ControllerBase
    {
        // BƯỚC 1: DÁN CHÍNH XÁC KEY COHERE CỦA ÔNG VÀO ĐÂY
        private readonly string _rawKey = "RVwqL1DEzK0RI2UGmuJ8J1FK0ONFBpe2eUbSrkgT";

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.message)) return Ok(new { reply = "Hỏi gì đi Đông ơi!" });

                // Tự động làm sạch Key để tránh lỗi ASCII Header
                string cleanKey = Regex.Replace(_rawKey, @"[^\x00-\x7F]+", "").Trim();

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {cleanKey}");

                var requestBody = new
                {
                    // BƯỚC 2: THIẾT LẬP NỘI DUNG TƯ VẤN CÔNG NGHỆ
                    message = "Bạn là trợ lý ảo tên Lôi Đỏ của cửa hàng công nghệ Tech Store. " +
                              "Nhiệm vụ: Chỉ tư vấn về Laptop, Điện thoại, Linh kiện máy tính. " +
                              "Lưu ý: Tuyệt đối KHÔNG nhắc đến thực phẩm hay đồ hữu cơ. " +
                              "Trả lời cực ngắn gọn bằng tiếng Việt: " + request.message,
                    model = "command-a-03-2025",
                    max_tokens = 300
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // BƯỚC 3: GỬI YÊU CẦU ĐẾN ĐÚNG ĐỊA CHỈ API V1
                var response = await client.PostAsync("https://api.cohere.ai/v1/chat", content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Trả về lỗi chi tiết để soi cho dễ nếu vẫn thất bại
                    return Ok(new { reply = "AI báo lỗi: " + response.StatusCode + ". Chi tiết: " + result });
                }

                using var doc = JsonDocument.Parse(result);
                string aiReply = doc.RootElement.GetProperty("text").GetString();

                return Ok(new { reply = aiReply });
            }
            catch (Exception ex) { return Ok(new { reply = "Lỗi hệ thống: " + ex.Message }); }
        }
    }
}