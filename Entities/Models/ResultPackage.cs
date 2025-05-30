// QuickStart/Entities/Models/ResultPackage.cs
using System.Runtime.Serialization;
using QuickStart.Utilities;

namespace QuickStart.Entities.Models
{
    [DataContract(Name = "ResultPackage", Namespace = "http://schemas.datacontract.org/2004/07/ATSCADA.WebService")]
    public class ResultPackage
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string TimeStamp { get; set; }
    }

    public static class ResultPackageExtensionMethods
    {
        public static ResultPackage Decrypt(this ResultPackage resultPackage)
        {
            return new ResultPackage
            {
                Name = resultPackage.Name.DecryptAddress(),
                Value = resultPackage.Value.DecryptValue(),
                Status = resultPackage.Status,
                TimeStamp = resultPackage.TimeStamp
            };
        }

        public static ResultPackage[] Decrypt(this ResultPackage[] resultPackages)
        {
            if (resultPackages == null) return null;
            var packageDecrypt = new ResultPackage[resultPackages.Length];
            for (var i = 0; i < resultPackages.Length; i++)
            {
                packageDecrypt[i] = resultPackages[i].Decrypt();
            }
            return packageDecrypt;
        }
    }
}