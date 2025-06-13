using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly EmailSettings _emailSettings;
        private readonly SmtpConnectionManager _connectionManager;
        private readonly SemaphoreSlim _rateLimiter;

        // Tracking để rate limiting
        private static readonly ConcurrentQueue<DateTime> _recentSends = new();
        private static readonly object _lockObject = new object();

        public EmailSender(EmailConfiguration emailConfig, EmailSettings emailSettings)
        {
            _emailConfig = emailConfig;
            _emailSettings = emailSettings;
            _connectionManager = new SmtpConnectionManager(_emailConfig, _emailSettings);
            _rateLimiter = new SemaphoreSlim(1, 1); // Serialize email sending
        }

        public async Task SendEmailAsync(Message message)
        {
            await _rateLimiter.WaitAsync();
            try
            {
                // ✅ KIỂM TRA RATE LIMIT
                if (!CanSendEmail())
                {
                    throw new InvalidOperationException($"Rate limit exceeded. Max {_emailSettings.MaxEmailsPerHour} emails per hour allowed.");
                }

                SmtpClient client = null;
                try
                {
                    // ✅ LẤY CONNECTION TỪ POOL
                    client = await _connectionManager.GetConnectionAsync();
                    var mailMessage = CreateEmailMessage(message);

                    await client.SendAsync(mailMessage);

                    // ✅ GHI NHẬN THỜI GIAN GỬI
                    lock (_lockObject)
                    {
                        _recentSends.Enqueue(DateTime.UtcNow);
                    }

                    Console.WriteLine($"✅ Email sent successfully to {string.Join(", ", message.To.Select(t => t.Address))}");

                    // ✅ DELAY GIỮA CÁC EMAIL
                    await Task.Delay(TimeSpan.FromSeconds(_emailSettings.DelayBetweenEmailsSeconds));
                }
                finally
                {
                    if (client != null)
                    {
                        _connectionManager.ReturnConnection(client);
                    }
                }
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        private bool CanSendEmail()
        {
            lock (_lockObject)
            {
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);

                // Xóa các record cũ hơn 1 tiếng
                while (_recentSends.TryPeek(out var oldest) && oldest < oneHourAgo)
                {
                    _recentSends.TryDequeue(out _);
                }

                var currentCount = _recentSends.Count;
                Console.WriteLine($"📊 Emails sent in last hour: {currentCount}/{_emailSettings.MaxEmailsPerHour}");

                return currentCount < _emailSettings.MaxEmailsPerHour;
            }
        }

        // Existing method không đổi
        public void SendEmail(Message message)
        {
            SendEmailAsync(message).GetAwaiter().GetResult();
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));

            // Thêm người nhận chính
            emailMessage.To.AddRange(message.To);

            // ✅ THÊM CC
            if (message.Cc?.Any() == true)
            {
                emailMessage.Cc.AddRange(message.Cc);
            }

            // ✅ THÊM BCC
            if (message.Bcc?.Any() == true)
            {
                emailMessage.Bcc.AddRange(message.Bcc);
            }

            emailMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = string.Format("<h2 style='color:red;'>{0}</h2>", message.Content) };

            if (message.Attachments != null && message.Attachments.Any())
            {
                byte[] fileBytes;
                foreach (var attachment in message.Attachments)
                {
                    using (var ms = new MemoryStream())
                    {
                        attachment.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                    bodyBuilder.Attachments.Add(attachment.FileName, fileBytes, ContentType.Parse(attachment.ContentType));
                }
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();
            return emailMessage;
        }
    }
}