using InexperiencedDeveloper.Core;
using InexperiencedDeveloper.Multiplayer.Riptide.ClientDev.UI;
using Riptide;
using Riptide.Utils;
using System;
using UnityEngine;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev
{
    public enum ServerToClientCommand : ushort
    {
        SyncTicks = 1,
        PlayerConnected,
        PlayerJoined,
        PopulateLobbyList,
        PlayerStart,
        PlayerSpawned,
        PlayerMove,
        SyncPosition,
    }

    public enum ClientToServerRequest : ushort
    {
        ConnectRequest = 1,
        HostRequest,
        JoinRequest,
        StartRequest,
        SpawnRequest,
        MoveRequest,
    }

    public class NetworkManager : Singleton<NetworkManager>
    {
        public Client Client { get; private set; }

        private ushort serverTick;
        public ushort ServerTick
        {
            get => serverTick;
            set
            {
                serverTick = value;
                LerpTick = (ushort)(value - TicksBetweenPositionUpdates);
            }
        }
        public ushort LerpTick { get; private set; }
        private ushort ticksBetweenPositionUpdates;
        public ushort TicksBetweenPositionUpdates
        {
            get => ticksBetweenPositionUpdates;
            set
            {
                ticksBetweenPositionUpdates = value;
                LerpTick = (ushort)(ServerTick - value);
            }
        }

        [SerializeField] private string ip;
        [SerializeField] private ushort port;
        [SerializeField] private ushort tickDivergenceTolerance = 1;


        //DEBUG
        private ushort id;

        private void Start()
        {
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
            Client = new Client();
            Client.Connected += OnConnect;
            Client.Disconnected += OnDisconnect;
            Client.ConnectionFailed += OnFailedToConnect;
            Connect();
            ServerTick = 2;
        }

        private void FixedUpdate()
        {
            Client.Update();
            ServerTick++;
        }

        private void OnApplicationQuit()
        {
            Client.Disconnect();
        }

        public void Connect()
        {
            Client.Connect($"{ip}:{port}");
        }

        public void HostLobby(string lobbyName)
        {
            UIManager.Instance.SendHostRequest(lobbyName, Client.Id);
        }

        public void JoinLobby(string lobbyName)
        {
            Lobby lobby = null;
            print($"Lobby name: {lobbyName}");
            foreach(var l in LobbyManager.Lobbies.Values)
            {
                print($"Lobbies: {lobbyName}");
                if(lobbyName == l.LobbyName)
                {
                    lobby = l;
                    break;
                }
            }
            if (lobby != null)
            {
                UIManager.Instance.SendJoinRequest(lobby.LobbyId, Client.Id);
            }
            else
            {
                Debug.LogError($"Lobby {lobbyName} not found");
            }
        }

        public void StartGame()
        {
            LobbyManager.Instance.SendStartReq();
        }

        private void OnConnect(object sender, EventArgs e)
        {
            UIManager.Instance.SendName();
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            UIManager.Instance.OnDisconnect();
        }

        private void OnFailedToConnect(object sender, EventArgs e)
        {
            UIManager.Instance.OnDisconnect();
        }

        private void SetTick(ushort serverTick)
        {
            if (Mathf.Abs(ServerTick - serverTick) > tickDivergenceTolerance)
            {
                Debug.Log($"Client tick: {ServerTick} -> {serverTick}");
                ServerTick = serverTick;
            }
        }

        [MessageHandler((ushort)ServerToClientCommand.SyncTicks)]
        #region ServerToClient Message Handler
        private static void Sync(Message msg)
        {
            Instance.SetTick(msg.GetUShort());
        }
        #endregion

    }
}

