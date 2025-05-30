// QuickStart/Service/WcfService.cs
using QuickStart.Entities.Models;
using QuickStart.Service.Contracts;
using QuickStart.Shared.DataTransferObjects.Wcf;
using QuickStart.Utilities;
using Microsoft.AspNetCore.SignalR;
using QuickStart.Hubs;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Service.Contracts;

namespace QuickStart.Service
{
    public class WcfService : IWcfService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<DataHub> _hubContext;
        private ChannelFactory<IATSCADAService> _channelFactory;
        private IATSCADAService _channel;
        private CancellationTokenSource _cts;
        private bool _isPolling;
        private string _address;

        public WcfService(IConfiguration configuration, IHubContext<DataHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
            _address = _configuration["WcfService:Address"] ?? "192.168.1.100:8000";
            Start();
        }

        public bool IsActive { get; private set; }

        private void Start()
        {
            var username = _configuration["WcfService:Username"] ?? "ATSCADALab___1jbyq8Yg1";
            var password = _configuration["WcfService:Password"] ?? "ATSCADA.Lab.!@#%aajUyqn61HDt";

            var binding = new CustomNetTcpBinding
            {
                OpenTimeout = TimeSpan.FromMinutes(2),
                SendTimeout = TimeSpan.FromMinutes(2),
                ReceiveTimeout = TimeSpan.FromMinutes(10)
            };
            var endpointAddress = new EndpointAddress($"net.tcp://{_address}/ATSCADAService");
            _channelFactory = new ChannelFactory<IATSCADAService>(binding, endpointAddress);
            _channelFactory.Credentials.UserName.UserName = username;
            _channelFactory.Credentials.UserName.Password = password;
            _channel = _channelFactory.CreateChannel();
            IsActive = true;
        }

        public async Task<WcfDataDto[]> ReadTagsAsync(string[] tagNames)
        {
            try
            {
                if (!IsActive) Start();

                var encryptedNames = tagNames.Select(n => n.EncryptAddress()).ToArray();
                var result = await Task.Run(() => _channel.Read(encryptedNames));
                var decryptedResult = result.Decrypt(); // Sử dụng extension method
                return decryptedResult?.Select(r => new WcfDataDto
                {
                    Name = r.Name,
                    Value = r.Value,
                    Status = r.Status,
                    TimeStamp = r.TimeStamp
                }).ToArray() ?? Array.Empty<WcfDataDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Read error: {ex.Message}");
                HandleException();
                return Array.Empty<WcfDataDto>();
            }
        }

        public async Task StartPollingAsync(string[] tagNames, int intervalMs)
        {
            if (_isPolling) return;

            _cts = new CancellationTokenSource();
            _isPolling = true;

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var data = await ReadTagsAsync(tagNames);
                    if (data.Any())
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveData", data);
                    }
                    await Task.Delay(intervalMs, _cts.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Polling error: {ex.Message}");
                    HandleException();
                    await Task.Delay(2000, _cts.Token);
                }
            }
        }

        public async Task StopPollingAsync()
        {
            if (!_isPolling) return;
            _cts?.Cancel();
            _isPolling = false;
            await Task.CompletedTask;
        }

        private void HandleException()
        {
            if (_channel is ICommunicationObject commObject &&
                (commObject.State == CommunicationState.Faulted || commObject.State == CommunicationState.Closed))
            {
                IsActive = false;
                commObject.Abort();
                _channelFactory.Close();
                Start();
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            if (_channel is ICommunicationObject commObject)
            {
                commObject.Close();
            }
            _channelFactory.Close();
            IsActive = false;
        }
    }
}