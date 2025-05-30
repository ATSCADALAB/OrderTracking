// QuickStart/Service/Contracts/IATSCADAService.cs
using QuickStart.Entities.Models;
using System.ServiceModel;

namespace QuickStart.Service.Contracts
{
    [ServiceContract]
    public interface IATSCADAService
    {
        [OperationContract]
        ResultPackage[] Read(string[] names);
    }
}