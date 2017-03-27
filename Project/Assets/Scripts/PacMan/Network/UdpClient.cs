using Lidgren.Network;

using Foundation;
using iCarus.Network;
using iCarus.Singleton;

namespace PacMan
{
    public class UdpClient : SingletonBehaviour<UdpClient>
    {
        public string playerName { get; private set; }
        public MessageDispatcher messageDispatcher { get { return mConnector.dispatcher; } }

        public void Connect(string playerName)
        {
            this.playerName = playerName;
            if (mConnector.connectionStatus != NetConnectionStatus.None || 
                mConnector.connectionStatus != NetConnectionStatus.Disconnected)
            {
                mConnector.Stop();
            }

            UdpConnector.Configuration netConfig = new UdpConnector.Configuration()
            {
                host = AppConfig.Instance.pacMan.host,
                port = AppConfig.Instance.pacMan.port,
                netPeerConfig = new NetPeerConfiguration(AppConfig.Instance.pacMan.appIdentifier)
                {
                    DefaultOutgoingMessageCapacity = 1024,
                },
                onNetStatusChanged = OnNetStatusChanged,
            };
            mConnector.Start(netConfig);
            mConnector.Connect(playerName);
        }

        void Update()
        {
            mConnector.Update();
        }

        void OnNetStatusChanged(UdpConnector connector, NetConnectionStatus status, string reason)
        {
            GameLog.InfoFormat("Connection status changed {0} {1}", status, reason);
        }

        UdpConnector mConnector;
    }
}
