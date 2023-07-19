using InexperiencedDeveloper.ActiveRagdoll;
using InexperiencedDeveloper.Core;
using InexperiencedDeveloper.Core.Controls;
using Riptide;
using System.Collections.Generic;
using UnityEngine;
using Player = InexperiencedDeveloper.ActiveRagdoll.Player;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev
{
    public class NetPlayerManager : Singleton<NetPlayerManager>
    {
        public static NetPlayer LocalPlayer { get; private set; }
        public static CameraController LocalCamera { get; private set; }
        [SerializeField] private GameObject localPlayerPrefab;
        [SerializeField] private GameObject playerPrefab;
        private static GameObject local_player_prefab;
        private static GameObject player_prefab;

        public static Dictionary<ushort, NetPlayer> NetPlayers = new();

        private void OnEnable()
        {
            local_player_prefab = localPlayerPrefab;
            player_prefab = playerPrefab;
        }

        public static NetPlayer GetPlayerById(ushort id)
        {
            if (NetPlayers.ContainsKey(id))
            {
                return NetPlayers[id];
            }
            else
            {
                Debug.LogError($"No player with ID: {id}");
                return null;
            }
        }

        private static void PreSpawnPlayer(ushort id, string username)
        {
            NetPlayer player = new GameObject().AddComponent<NetPlayer>();
            bool localPlayer = player.Init(id, username);
            player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            if (localPlayer) LocalPlayer = player;
            NetPlayers.Add(id, player);
        }

        private static void EnterGame(ushort levelIndex)
        {
            LevelManager.Instance.LoadLevel((int)levelIndex);
        }

        public static void Spawn(ushort playerId, string username, Vector3 pos, ushort lobbyId)
        {
            NetPlayer netPlayer;
            print($"Spawning {playerId}");
            print($"Our id = {NetworkManager.Instance.Client.Id}");
            if (playerId == NetworkManager.Instance.Client.Id)
            {
                netPlayer = Instantiate(local_player_prefab, pos, Quaternion.identity).GetComponent<NetPlayer>();
                if(LocalCamera == null)
                {
                    CameraController camController = Camera.main.gameObject.AddComponent<CameraController>();
                    LocalCamera = camController;
                }
            }
            else
            {
                netPlayer = Instantiate(player_prefab, pos, Quaternion.identity).GetComponent<NetPlayer>();
            }

            bool localPlayer = netPlayer.Init(playerId, username);
            netPlayer.name = $"Player {playerId} ({(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            Player player = RagdollSetup(netPlayer);
            if (LocalCamera != null && localPlayer) LocalCamera.Init(player);
            netPlayer.JoinLobby(LobbyManager.Lobbies[lobbyId]);
            netPlayer.MyLobby.PlayersInGame.Add(netPlayer);
            NetPlayers[playerId] = netPlayer;
        }

        public static Player RagdollSetup(NetPlayer netPlayer)
        {
            Ragdoll ragdoll = netPlayer.GetComponentInChildren<Ragdoll>();
            Player player = netPlayer.gameObject.AddComponent<Player>();
            GroundManager groundManager = netPlayer.gameObject.AddComponent<GroundManager>();
            PlayerControls controls = netPlayer.gameObject.AddComponent<PlayerControls>();
            RagdollMovement movement = netPlayer.gameObject.AddComponent<RagdollMovement>();
            Ball ball = netPlayer.gameObject.AddComponent<Ball>();
            CameraController camController = Camera.main.gameObject.AddComponent<CameraController>();
            LocalCamera = camController;
            player.Init();
            LocalCamera.Init(player);
            return player;
        }

        private static void SendInputToPlayer(ushort playerId, Vector2 movement, Vector2 lookDir, bool jump)
        {
            NetPlayer netPlayer = NetPlayers[playerId];
            if(netPlayer.IsLocal) { Debug.LogWarning("Double moving local Player. "); }
            Vector3 walkDir = new Vector3(movement.x, 0, movement.y);
            netPlayer.Player.Controls.ReceiveInputs(walkDir, lookDir, jump);
        }

        private static void SyncPos(ushort playerId, Vector3 pos)
        {
            NetPlayer netPlayer = NetPlayers[playerId];
            if (netPlayer.Player != null && Vector3.Distance(pos, netPlayer.Player.transform.position) > 2)
            {
                Debug.LogWarning($"Moved Player {netPlayer.Id} {Vector3.Distance(pos, netPlayer.Player.transform.position)} meters.");
                netPlayer.Player.transform.position = pos;
            }
            //netPlayer.Player.Interpolate.NewUpdate(tick, isTeleport, pos);
        }


        #region ServerToClient Message Handler
        [MessageHandler((ushort)ServerToClientCommand.PlayerConnected)]
        private static void ClientPreJoinSpawn(Message msg)
        {
            ushort id = msg.GetUShort();
            string username = msg.GetString();

            PreSpawnPlayer(id, username);
        }

        [MessageHandler((ushort)ServerToClientCommand.PlayerStart)]
        private static void ClientEnterGame(Message msg)
        {
            ushort levelIndex = msg.GetUShort();

            EnterGame(levelIndex);
        }

        [MessageHandler((ushort)ServerToClientCommand.PlayerSpawned)]
        private static void SpawnPlayer(Message msg)
        {
            ushort playerId = msg.GetUShort();
            string username = msg.GetString();
            Vector3 pos = msg.GetVector3();
            ushort lobbyId = msg.GetUShort();

            Spawn(playerId, username, pos, lobbyId);
        }

        [MessageHandler((ushort)ServerToClientCommand.PlayerMove)]
        private static void ReceiveOtherInputs(Message msg)
        {
            ushort playerId = msg.GetUShort();
            Vector2 movement = msg.GetVector2();
            Vector2 lookDir = msg.GetVector2();
            bool jump = msg.GetBool();

            SendInputToPlayer(playerId, movement, lookDir, jump);
        }

        [MessageHandler((ushort)ServerToClientCommand.SyncPosition)]
        private static void ClientSyncPosition(Message msg)
        {
            ushort playerId = msg.GetUShort();
            //ushort currentTick = msg.GetUShort();
            //bool didTeleport = msg.GetBool();
            Vector3 pos = msg.GetVector3();

            SyncPos(playerId, pos);
        }

        #endregion
    }
}

