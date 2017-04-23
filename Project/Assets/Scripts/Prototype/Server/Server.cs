using iCarus;
using iCarus.Log;
using iCarus.Network;
using iCarus.Singleton;
using Foundation;
using Lidgren.Network;

namespace Prototype
{
    public class TSLog : Logging.Define<TSLog> { }
    public class ServerException : Exception { }
    public sealed class Server : SingletonBehaviour<Server>
    {
        public UdpListener netlayer { get { return mUdpListener; } }

        public static void Initialize()
        {
            Server server = Singletons.Add<Server>();
            server.Run();
        }

        void Run()
        {
            Singletons.Add<PlayerManager>().Initialize();
            Singletons.Add<SyncManager>().Initialize();
            StartNetwork();
            TSLog.Info("Server Started!");
        }

        void StartNetwork()
        {
            UdpListener.Configuration netConfig = new UdpListener.Configuration()
            {
                netPeerConfig = new NetPeerConfiguration(AppConfig.Instance.pacMan.appIdentifier)
                {
                    LocalAddress = System.Net.IPAddress.Any,
                    Port = AppConfig.Instance.pacMan.port,
                    MaximumConnections = AppConfig.Instance.pacMan.maxConnection,
                    DefaultOutgoingMessageCapacity = 10240,
                    SimulatedDuplicatesChance = AppConfig.Instance.simulatedDuplicatesChance,
                    SimulatedLoss = AppConfig.Instance.simulatedLoss,
                    SimulatedMinimumLatency = AppConfig.Instance.simulatedMinimumLatency,
                    SimulatedRandomLatency = AppConfig.Instance.simulatedRandomLatency,
                },
                onIncommingConnection = PlayerManager.Instance.OnNewConnection,
                onConnectionStatusChanged = PlayerManager.Instance.OnConnectionStatusChanged,
            };

            mUdpListener.Start(netConfig);
        }

        void OnDestroy()
        {
            mUdpListener.Stop();
        }

        void FixedUpdate()
        {
            mUdpListener.Update();
            PlayerManager.Instance.CFixedUpdate();
            SyncManager.Instance.CFixedUpdate();
        }

        UdpListener mUdpListener = new UdpListener();
    }
}
