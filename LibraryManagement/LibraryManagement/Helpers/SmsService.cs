using System.Text;
using System.Text.Json;

namespace LibraryManagement.Helpers;

public static class SmsService
{
    // Đặt = false nếu bạn đã mua API Key của SpeedSMS/Twilio và muốn gửi tin nhắn thật
    private const bool IsDemoMode = true;
    private const string SpeedSmsAccessToken = "YOUR_SPEEDSMS_API_TOKEN";

    public static async Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
    {
        string message = $"[LIBRA] Ma xac thuc OTP lay lai mat khau cua ban la: {otpCode}. Hieu luc trong 3 phut.";

        if (IsDemoMode)
        {
            // Giả lập tin nhắn SMS gửi về điện thoại để test đồ án
            MessageBox.Show(
                $"📱 TIN NHẮN SMS MỚI (Đến số: {phoneNumber})\n\n{message}",
                "Mô phỏng SMS Gateway",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return true;
        }

        // Gửi SMS thật qua API SpeedSMS Việt Nam (khi có Token)
        using var client = new HttpClient();
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{SpeedSmsAccessToken}:x"));
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

        var payload = new
        {
            to = new[] { phoneNumber },
            content = message,
            sms_type = 2
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.speedsms.vn/index.php/sms/send", content);
        return response.IsSuccessStatusCode;
    }
}