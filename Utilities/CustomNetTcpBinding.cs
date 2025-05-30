// QuickStart/Utilities/CustomNetTcpBinding.cs
using System.Net.Security;
using System.ServiceModel.Channels;

namespace QuickStart.Utilities
{
    public class CustomNetTcpBinding : CustomBinding
    {
        public CustomNetTcpBinding() : base()
        {
        }

        public override BindingElementCollection CreateBindingElements()
        {
            return new BindingElementCollection
            {
                new BinaryMessageEncodingBindingElement { MessageVersion = MessageVersion.Soap12WSAddressing10 },
                SecurityBindingElement.CreateUserNameOverTransportBindingElement(),
                new AutoSecuredTcpTransportElement()
            };
        }

        public override string Scheme => "net.tcp";
    }

    public class AutoSecuredTcpTransportElement : TcpTransportBindingElement
    {
        public AutoSecuredTcpTransportElement()
        {
            MaxReceivedMessageSize = 2147483647;
            MaxBufferSize = 2147483647;
            ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = 10000;
            // Bỏ MaxPendingConnections vì không tồn tại trong .NET 8
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(ISecurityCapabilities))
                return (T)(ISecurityCapabilities)new AutoSecuredTcpSecurityCapabilities();
            return base.GetProperty<T>(context);
        }
    }

    public class AutoSecuredTcpSecurityCapabilities : ISecurityCapabilities
    {
        public ProtectionLevel SupportedRequestProtectionLevel => ProtectionLevel.EncryptAndSign;
        public ProtectionLevel SupportedResponseProtectionLevel => ProtectionLevel.EncryptAndSign;
        public bool SupportsClientAuthentication => true;
        public bool SupportsClientWindowsIdentity => true;
        public bool SupportsServerAuthentication => true;
    }
}