using MailKit.Net.Smtp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace EmailService
{
    public class SmtpConnectionManager : IDisposable
    {
        private readonly EmailConfiguration _config;
        private readonly EmailSettings _settings;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<SmtpClient> _connections;
        private readonly Timer _cleanupTimer;
        private bool _disposed = false;

        public SmtpConnectionManager(EmailConfiguration config, EmailSettings settings)
        {
            _config = config;
            _settings = settings;
            // ✅ SỬA LỖI: Bỏ dấu * và sử dụng _settings
            _semaphore = new SemaphoreSlim(_settings.MaxConcurrentConnections, _settings.MaxConcurrentConnections);
            _connections = new ConcurrentQueue<SmtpClient>();

            // Cleanup timer mỗi 10 phút
            _cleanupTimer = new Timer(CleanupConnections, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
        }

        //public async Task<SmtpClient> GetConnectionAsync()
        //{
        //    await _semaphore.WaitAsync();

        //    // Thử lấy connection có sẵn
        //    while (_connections.TryDequeue(out var existingClient))
        //    {
        //        if (existingClient.IsConnected)
        //        {
        //            return existingClient;
        //        }
        //        else
        //        {
        //            existingClient.Dispose();
        //        }
        //    }

        //    // ✅ SỬA LỖI: Declare client bên ngoài try block
        //    SmtpClient newClient = new SmtpClient();
        //    try
        //    {
        //        await newClient.ConnectAsync(_config.SmtpServer, _config.Port, true);
        //        newClient.AuthenticationMechanisms.Remove("XOAUTH2");
        //        await newClient.AuthenticateAsync(_config.UserName, _config.Password);
        //        return newClient;
        //    }
        //    catch
        //    {
        //        _semaphore.Release();
        //        newClient?.Dispose();
        //        throw;
        //    }
        //}
        public async Task<SmtpClient> GetConnectionAsync()
        {
            await _semaphore.WaitAsync();

            while (_connections.TryDequeue(out var existingClient))
            {
                if (existingClient.IsConnected)
                {
                    return existingClient;
                }
                else
                {
                    existingClient.Dispose();
                }
            }

            SmtpClient newClient = new SmtpClient();
            try
            {
                // ✅ CHỈ BỎ QUA KIỂM TRA REVOCATION, VẪN KIỂM TRA CHỨNG CHỈ
                newClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    // Bỏ qua lỗi revocation nhưng vẫn kiểm tra các lỗi khác
                    if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                        return true;

                    // Chấp nhận nếu chỉ có lỗi revocation
                    if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors)
                    {
                        foreach (var status in chain.ChainStatus)
                        {
                            // Chỉ bỏ qua lỗi revocation
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.RevocationStatusUnknown &&
                                status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.OfflineRevocation)
                            {
                                return false;
                            }
                        }
                        return true;
                    }

                    return false;
                };

                await newClient.ConnectAsync(_config.SmtpServer, _config.Port, true);
                newClient.AuthenticationMechanisms.Remove("XOAUTH2");
                await newClient.AuthenticateAsync(_config.UserName, _config.Password);
                return newClient;
            }
            catch
            {
                _semaphore.Release();
                newClient?.Dispose();
                throw;
            }
        }
        public void ReturnConnection(SmtpClient client)
        {
            if (client?.IsConnected == true)
            {
                _connections.Enqueue(client);
            }
            else
            {
                client?.Dispose();
            }
            _semaphore.Release();
        }

        private void CleanupConnections(object state)
        {
            var connectionsToKeep = new List<SmtpClient>();

            while (_connections.TryDequeue(out var client))
            {
                if (client.IsConnected)
                {
                    connectionsToKeep.Add(client);
                }
                else
                {
                    client.Dispose();
                }
            }

            foreach (var client in connectionsToKeep)
            {
                _connections.Enqueue(client);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanupTimer?.Dispose();

                while (_connections.TryDequeue(out var client))
                {
                    client?.Dispose();
                }

                _semaphore?.Dispose();
                _disposed = true;
            }
        }
    }
}