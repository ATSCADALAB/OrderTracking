// EmailService/EmailConfiguration.cs - THÊM DISPLAY NAME CHO NGƯỜI GỬI
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService
{
    public class EmailConfiguration
    {
        public string From { get; set; }

        // ✅ THÊM: Tên hiển thị cho người gửi
        public string FromDisplayName { get; set; }

        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        // ✅ PHƯƠNG THỨC: Lấy MailboxAddress hoàn chỉnh cho người gửi
        public MimeKit.MailboxAddress GetFromMailboxAddress()
        {
            var displayName = !string.IsNullOrEmpty(FromDisplayName)
                ? FromDisplayName
                : "Công ty Cổ Phần Giải Pháp Kỹ Thuật Ấn Tượng"; // Default name

            return new MimeKit.MailboxAddress(displayName, From);
        }
    }
}