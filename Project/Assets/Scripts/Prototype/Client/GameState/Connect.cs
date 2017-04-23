using Foundation;
using iCarus.Network;
using Lidgren.Network;

namespace Prototype.GameState
{
    public class Connect : GameState
    {
        public override void Start()
        {
            game.netlayer.Stop();
            game.netlayer.Start(BuildConfiguration(OnNetStatusChanged));
            game.netlayer.Connect();
        }

        protected override void Update() { }

        protected override void Destroy()
        {
            game.netlayer.onNetStatusChanged -= OnNetStatusChanged;
        }

        public static UdpConnector.Configuration BuildConfiguration(UdpConnector.OnNetStatusChanged onNetStatusChanged)
        {
            UdpConnector.Configuration config = new UdpConnector.Configuration()
            {
                host = AppConfig.Instance.pacMan.host,
                port = AppConfig.Instance.pacMan.port,
                netPeerConfig = new NetPeerConfiguration(AppConfig.Instance.pacMan.appIdentifier)
                {
                    DefaultOutgoingMessageCapacity = AppConfig.Instance.defaultOutgoingMessageCapacity,
                    SimulatedDuplicatesChance = AppConfig.Instance.simulatedDuplicatesChance,
                    SimulatedLoss = AppConfig.Instance.simulatedLoss,
                    SimulatedMinimumLatency = AppConfig.Instance.simulatedMinimumLatency,
                    SimulatedRandomLatency = AppConfig.Instance.simulatedRandomLatency,
                },
                onNetStatusChanged = onNetStatusChanged,
            };
            return config;
        }

        void OnNetStatusChanged(UdpConnector netlayer, NetConnectionStatus status, string reason)
        {
            if (!ConnectionLost(status))
            {
                GameStateLog.Info("connect net status:" + status);
                if (status == NetConnectionStatus.Connected)
                    TransitTo<VerifyIdentity>();
            }
            else
            {
                GameStateLog.ErrorFormat("connect net status:{0}, reason:{1}", status, reason);
                TransitTo<Error>(reason);
            }
        }
    }
}
