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
        public List<MailboxAddress> Cc { get; set; } // ✅ THÊM CC
        public List<MailboxAddress> Bcc { get; set; } // ✅ THÊM BCC
        public string Subject { get; set; }
        public string Content { get; set; }
        public IFormFileCollection Attachments { get; set; }

        // Constructor cũ để tương thích ngược
        public Message(IEnumerable<string> to, string subject, string content, IFormFileCollection attachments)
        {
            To = new List<MailboxAddress>();
            Cc = new List<MailboxAddress>();
            Bcc = new List<MailboxAddress>();

            To.AddRange(to.Select(x => new MailboxAddress("email", x)));
            Subject = subject;
            Content = content;
            Attachments = attachments;
        }

        // ✅ CONSTRUCTOR MỚI HỖ TRỢ CC/BCC
        public Message(IEnumerable<string> to, string subject, string content,
                      IFormFileCollection attachments = null,
                      IEnumerable<string> cc = null,
                      IEnumerable<string> bcc = null)
        {
            To = new List<MailboxAddress>();
            Cc = new List<MailboxAddress>();
            Bcc = new List<MailboxAddress>();

            To.AddRange(to.Select(x => new MailboxAddress("email", x)));

            if (cc != null)
                Cc.AddRange(cc.Select(x => new MailboxAddress("email", x)));

            if (bcc != null)
                Bcc.AddRange(bcc.Select(x => new MailboxAddress("email", x)));

            Subject = subject;
            Content = content;
            Attachments = attachments;
        }

        // ✅ METHODS ĐỂ THÊM CC/BCC SAU KHI TẠO OBJECT
        public void AddCc(string email)
        {
            Cc.Add(new MailboxAddress("email", email));
        }

        public void AddCc(IEnumerable<string> emails)
        {
            Cc.AddRange(emails.Select(x => new MailboxAddress("email", x)));
        }

        public void AddBcc(string email)
        {
            Bcc.Add(new MailboxAddress("email", email));
        }

        public void AddBcc(IEnumerable<string> emails)
        {
            Bcc.AddRange(emails.Select(x => new MailboxAddress("email", x)));
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