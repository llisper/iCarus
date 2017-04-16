using Foundation;
using iCarus.Network;
using Lidgren.Network;

namespace Prototype.GameState
{
    public class Connect : GameState
    {
        public override void Start()
        {
            Game.Instance.netlayer.Stop();
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
                onNetStatusChanged = OnNetStatusChanged,
            };
            Game.Instance.netlayer.Start(config);
        }

        public override GameState Update() { return null; }

        public override void Destroy()
        {
            Game.Instance.netlayer.onNetStatusChanged -= OnNetStatusChanged;
        }

        void OnNetStatusChanged(UdpConnector netlayer, NetConnectionStatus status, string reason)
        {
            if (status > NetConnectionStatus.None && status <= NetConnectionStatus.Connected)
            {
                GameStateLog.Info("Connect: " + status);
                if (status == NetConnectionStatus.Connected)
                    Game.Instance.TransitGameStateTo<VerifyIdentity>();
            }
            else
            {
                GameStateLog.Error("Connect: " + status);
                Game.Instance.TransitGameStateTo<Error>();
            }
        }
    }
}
