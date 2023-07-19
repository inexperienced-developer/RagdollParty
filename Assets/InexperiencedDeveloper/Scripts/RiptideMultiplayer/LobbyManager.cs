using InexperiencedDeveloper.Core;
using InexperiencedDeveloper.Multiplayer.Riptide.ClientDev.UI;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev
{
    public class LobbyManager : Singleton<LobbyManager>
    {
        public static Dictionary<ushort, Lobby> Lobbies = new();

        [SerializeField] private GameObject lobbyPrefab;
        [SerializeField] private Transform lobbyUIParent;
        private static GameObject lobby_prefab;
        private static Transform lobby_ui_parent;

        private delegate void OnLobbyChange(ushort lobbyId);
        private OnLobbyChange lobbyChanged;

        private void OnEnable()
        {
            lobby_prefab = lobbyPrefab;
            lobby_ui_parent = lobbyUIParent;
        }

        private static void PopulateLobbyDict(ushort lobbyId, ushort playerId, string lobbyName)
        {
            if (!Lobbies.ContainsKey(lobbyId))
            {
                CreateLobby(lobbyId, lobbyName);
            }
            else
            {
                JoinLobby(lobbyId, playerId, lobbyName);
            }
        }

        public static bool JoinLobby(ushort lobbyId, ushort playerId, string lobbyName)
        {
            if (Lobbies.ContainsKey(lobbyId))
            {
                Lobby lobby = Lobbies[lobbyId];
                NetPlayer player = NetPlayerManager.GetPlayerById(playerId);
                if (player != null)
                {
                    player.JoinLobby(Lobbies[lobbyId]);
                    lobby.Players.Add(player);
                    UpdateLobbyUI(lobbyId, player);
                    return true;
                }
                else
                {
                    Debug.LogError($"Player {playerId} doesn't exist");
                    return false;
                }

            }
            else
            {
                return CreateLobby(playerId, lobbyName);
            }

            //string lobby = lobbyName.ToLower();
            //if (!Lobbies.ContainsKey(lobby))
            //{
            //    Debug.LogError($"{lobbyName} is not an active lobby.");
            //    return false;
            //}
            //else if(Lobbies[lobby].MaxPlayers >= Lobbies[lobby].Players.Count)
            //{
            //    Debug.LogError($"{lobbyName} is full.");
            //    return false;
            //}
            //else
            //{
            //    Lobbies[lobby].Players.Add(player);
            //    return true;
            //}
        }

        private static bool CreateLobby(ushort playerId, string lobbyName)
        {
            if (!Lobbies.ContainsKey(playerId))
            {
                NetPlayer host = NetPlayerManager.GetPlayerById(playerId);
                if (host == null)
                {
                    Debug.LogError("Host doesn't exist");
                    return false;
                }
                Lobby lobby = new Lobby(host, lobbyName);
                lobby.LobbyUI = Instantiate(lobby_prefab, lobby_ui_parent);
                lobby.LobbyUI.SetActive(false);
                lobby.LobbyUI.name = $"Lobby {lobbyName}";
                Lobbies.Add(playerId, lobby);
                return JoinLobby(playerId, playerId, lobbyName);
            }
            else
            {
                Debug.LogError($"Lobby {playerId} already exists as {Lobbies[playerId].LobbyName}");
                return false;
            }
        }

        private static void UpdateLobbyUI(ushort lobbyId, NetPlayer player)
        {
            if (Lobbies.ContainsKey(lobbyId))
            {
                Lobby lobby = Lobbies[lobbyId];
                if(lobby == NetPlayerManager.LocalPlayer.MyLobby)
                {
                    GameObject lobbyUI = lobby.LobbyUI;
                    var ui = lobbyUI.GetComponent<LobbyPanelUI>();
                    for (int i = 0; i < lobby.Players.Count; i++)
                    {
                        ui.LobbyName.SetText(lobby.LobbyName);
                        GameObject newPanel = ui.PlayerPanels[i];
                        newPanel.GetComponentInChildren<TMP_Text>().SetText(lobby.Players[i].Username);
                        if (lobby.Players.Contains(player)) lobbyUI.SetActive(true);
                    }
                    ui.StartButton.GetComponent<Button>().interactable = NetPlayerManager.LocalPlayer.IsHost;
                }
                else
                {
                    lobby.LobbyUI.SetActive(false);
                }
            }
        }

        #region ClientToServer Message Sender
        public void SendStartReq()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerRequest.StartRequest);
            ushort lobbyId = NetPlayerManager.LocalPlayer.MyLobby.LobbyId;
            ushort levelIndex = 1; //Change later

            msg.AddUShort(lobbyId);
            msg.AddUShort(levelIndex);

            NetworkManager.Instance.Client.Send(msg);
        }

        #endregion

        #region ServerToClient Message Handler
        [MessageHandler((ushort)ServerToClientCommand.PopulateLobbyList)]
        private static void PopulateLobbies(Message msg)
        {
            ushort lobbyId = msg.GetUShort();
            string lobbyName = msg.GetString();
            ushort playerId = msg.GetUShort();

            PopulateLobbyDict(lobbyId, playerId, lobbyName);
        }

        [MessageHandler((ushort)ServerToClientCommand.PlayerJoined)]
        private static void ClientJoinLobby(Message msg)
        {
            ushort lobbyId = msg.GetUShort();
            ushort playerId = msg.GetUShort();
            string lobbyName = msg.GetString();

            JoinLobby(lobbyId, playerId, lobbyName);
        }

        #endregion
    }
}

