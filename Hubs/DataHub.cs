// QuickStart/Hubs/DataHub.cs
using Microsoft.AspNetCore.SignalR;
using QuickStart.Shared.DataTransferObjects.Wcf;

namespace QuickStart.Hubs
{
    public class DataHub : Hub
    {
        public async Task SendData(WcfDataDto[] data)
        {
            await Clients.All.SendAsync("ReceiveData", data);
        }
    }
}