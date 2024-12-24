using System.Net.Mail;
using System.Net;

namespace QuanLyShop.Common
{
    public class EmailService
    {
        private readonly string _email;
        private readonly string _password;

        public EmailService(IConfiguration configuration)
        {
            // Lấy cấu hình từ appsettings.json
            _email = configuration["AppSettings:Email"];
            _password = configuration["AppSettings:PasswordEmail"];
        }

        public bool SendMail(string name, string subject, string content, string toMail)
        {
            bool rs = false;
            try
            {
                MailMessage message = new MailMessage
                {
                    From = new MailAddress(_email, name),
                    Subject = subject,
                    Body = content,
                    IsBodyHtml = true
                };

                message.To.Add(toMail);
                //587 là cổng phổ biến được Gmail sử dụng cho giao thức SMTP khi bật SSL/TLS. 
                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_email, _password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                smtp.Send(message);
                rs = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                rs = false;
            }

            return rs;

        }
    }
}
