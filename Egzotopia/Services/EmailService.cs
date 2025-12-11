using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Egzotopia.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // --- 1. SİPARİŞ ONAY MAİLİ (Yeni Eklenen) ---
        public bool SendOrderConfirmation(string toEmail, int orderId, decimal amount)
        {
            string subject = $"Siparişiniz Alındı! - #{orderId}";
            string plainText = $"Siparişiniz başarıyla alındı. Tutar: {amount} ₺. Sipariş No: {orderId}";

            // E-posta Tasarımı (HTML)
            string htmlBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px; max-width: 600px; margin: 0 auto; background-color: #ffffff;'>
                    <h2 style='color: #2E8B57; text-align: center; border-bottom: 2px solid #2E8B57; padding-bottom: 10px;'>
                        🛍️ Siparişiniz Onaylandı!
                    </h2>
                    <p style='font-size: 16px; color: #333;'>Merhaba,</p>
                    <p style='color: #555;'>Egzotopia'yı tercih ettiğiniz için teşekkür ederiz. Siparişiniz başarıyla alınmıştır.</p>
                    
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 5px 0;'><strong>Sipariş No:</strong> #{orderId}</p>
                        <p style='margin: 5px 0;'><strong>Toplam Tutar:</strong> <span style='color: #2E8B57; font-weight: bold;'>{amount:C2}</span></p>
                        <p style='margin: 5px 0;'><strong>Durum:</strong> <span style='color: green;'>✔ Ödeme Alındı</span></p>
                    </div>

                    <p style='font-size: 12px; color: #999; text-align: center; margin-top: 30px;'>
                        © {DateTime.Now.Year} Egzotopia - Doğayı Keşfet.
                    </p>
                </div>";

            return SendGenericEmail(toEmail, subject, plainText, htmlBody);
        }

        // --- 2. DOĞRULAMA KODU (Eski Metot) ---
        public bool SendEmail(string toEmail, string code)
        {
            return SendGenericEmail(toEmail, "Egzotopia Doğrulama Kodunuz",
                $"Doğrulama Kodunuz: {code}",
                GenerateVerificationHtml(code));
        }

        // --- YARDIMCI METOTLAR ---

        private bool SendGenericEmail(string toEmail, string subject, string plainText, string htmlBody)
        {
            try
            {
                var emailFrom = _configuration["EmailSettings:Mail"];
                var password = _configuration["EmailSettings:Password"];
                var host = _configuration["EmailSettings:Host"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("EgZotopia", emailFrom));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = plainText,
                    HtmlBody = htmlBody
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect(host, port, SecureSocketOptions.StartTls);
                    client.Authenticate(emailFrom, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GenerateVerificationHtml(string code)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px; max-width: 600px; margin: 0 auto; background-color: #ffffff;'>
                    <h2 style='color: #2E8B57; text-align: center; border-bottom: 2px solid #2E8B57; padding-bottom: 10px;'>
                        🌴 Egzotopia Doğrulama
                    </h2>
                    <div style='background-color: #f0f8ff; padding: 15px; text-align: center; margin: 20px 0; border-radius: 8px; border: 1px dashed #2E8B57;'>
                        <span style='font-size: 28px; font-weight: bold; color: #2E8B57;'>{code}</span>
                    </div>
                </div>";
        }
    }
}