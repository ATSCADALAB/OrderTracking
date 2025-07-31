// EmailService/Message.cs - CẢI THIỆN HIỂN THỊ TÊN EMAIL
using Microsoft.AspNetCore.Http;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailService
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public List<MailboxAddress> Cc { get; set; }
        public List<MailboxAddress> Bcc { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public IFormFileCollection Attachments { get; set; }

        // Constructor cũ để tương thích ngược
        public Message(IEnumerable<string> to, string subject, string content, IFormFileCollection attachments)
        {
            To = new List<MailboxAddress>();
            Cc = new List<MailboxAddress>();
            Bcc = new List<MailboxAddress>();

            // ✅ THAY ĐỔI: Sử dụng tên từ email thay vì chữ "email"
            To.AddRange(to.Select(x => new MailboxAddress(ExtractNameFromEmail(x), x)));
            Subject = subject;
            Content = content;
            Attachments = attachments;
        }

        // Constructor mới hỗ trợ CC/BCC
        public Message(IEnumerable<string> to, string subject, string content,
                      IFormFileCollection attachments = null,
                      IEnumerable<string> cc = null,
                      IEnumerable<string> bcc = null)
        {
            To = new List<MailboxAddress>();
            Cc = new List<MailboxAddress>();
            Bcc = new List<MailboxAddress>();

            // ✅ THAY ĐỔI: Tự động tạo tên hiển thị từ email
            To.AddRange(to.Select(x => new MailboxAddress(ExtractNameFromEmail(x), x)));

            if (cc != null)
                Cc.AddRange(cc.Select(x => new MailboxAddress(ExtractNameFromEmail(x), x)));

            if (bcc != null)
                Bcc.AddRange(bcc.Select(x => new MailboxAddress(ExtractNameFromEmail(x), x)));

            Subject = subject;
            Content = content;
            Attachments = attachments;
        }

        // ✅ CONSTRUCTOR MỚI: Cho phép chỉ định tên cụ thể
        public Message(Dictionary<string, string> toEmailsWithNames, string subject, string content,
                      IFormFileCollection attachments = null,
                      Dictionary<string, string> ccEmailsWithNames = null,
                      Dictionary<string, string> bccEmailsWithNames = null)
        {
            To = new List<MailboxAddress>();
            Cc = new List<MailboxAddress>();
            Bcc = new List<MailboxAddress>();

            // Thêm TO với tên cụ thể
            foreach (var emailPair in toEmailsWithNames)
            {
                To.Add(new MailboxAddress(emailPair.Value, emailPair.Key)); // Value = Name, Key = Email
            }

            // Thêm CC với tên cụ thể
            if (ccEmailsWithNames != null)
            {
                foreach (var emailPair in ccEmailsWithNames)
                {
                    Cc.Add(new MailboxAddress(emailPair.Value, emailPair.Key));
                }
            }

            // Thêm BCC với tên cụ thể
            if (bccEmailsWithNames != null)
            {
                foreach (var emailPair in bccEmailsWithNames)
                {
                    Bcc.Add(new MailboxAddress(emailPair.Value, emailPair.Key));
                }
            }

            Subject = subject;
            Content = content;
            Attachments = attachments;
        }

        // ✅ PHƯƠNG THỨC MỚI: Tự động tạo tên hiển thị từ email
        private static string ExtractNameFromEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            var localPart = email.Split('@')[0];

            // Loại bỏ số và ký tự đặc biệt, chuyển thành tên đẹp
            var cleanName = System.Text.RegularExpressions.Regex.Replace(localPart, @"[\d\._\-]+", " ");
            cleanName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleanName.Trim());

            // Nếu không có gì hợp lý, trả về phần trước @
            return string.IsNullOrWhiteSpace(cleanName) ? localPart : cleanName;
        }

        // Methods để thêm CC/BCC với tên tự động
        public void AddCc(string email)
        {
            Cc.Add(new MailboxAddress(ExtractNameFromEmail(email), email));
        }

        public void AddCc(IEnumerable<string> emails)
        {
            Cc.AddRange(emails.Select(x => new MailboxAddress(ExtractNameFromEmail(x), x)));
        }

        public void AddBcc(string email)
        {
            Bcc.Add(new MailboxAddress(ExtractNameFromEmail(email), email));
        }

        public void AddBcc(IEnumerable<string> emails)
        {
            Bcc.AddRange(emails.Select(x => new MailboxAddress(ExtractNameFromEmail(x), x)));
        }

        // ✅ METHODS MỚI: Thêm email với tên cụ thể
        public void AddCcWithName(string email, string displayName)
        {
            Cc.Add(new MailboxAddress(displayName, email));
        }

        public void AddBccWithName(string email, string displayName)
        {
            Bcc.Add(new MailboxAddress(displayName, email));
        }

        public void ClearCc()
        {
            Cc.Clear();
        }

        public void ClearBcc()
        {
            Bcc.Clear();
        }
    }
}