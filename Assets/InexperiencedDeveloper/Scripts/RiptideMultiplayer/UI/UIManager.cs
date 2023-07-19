using InexperiencedDeveloper.Core;
using Riptide;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev.UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private LevelUI currLevelUI;
        private string username;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currLevelUI = FindObjectOfType<LevelUI>();
        }

        public void Connect()
        {
            MenuUI ui = currLevelUI as MenuUI;
            if (ui != null)
            {
                ui.UserInput.interactable = false;
                username = (string.IsNullOrEmpty(ui.UserInput.text) ? "Guest" : ui.UserInput.text);
                ui.ConnectObj.SetActive(false);
                ui.LobbySelect.SetActive(true);
                ui.LobbyParent.SetActive(true);
                NetworkManager.Instance.Connect();
            }
        }

        public void JoinLobby()
        {
            MenuUI ui = currLevelUI as MenuUI;
            if (ui != null)
            {
                ui.UserInput.interactable = false;
                ui.ConnectObj.SetActive(false);
                ui.LobbySelect.SetActive(false);
                ui.LobbyParent.SetActive(true);
                string lobbyName = ui.LobbyInput.text;
                NetworkManager.Instance.JoinLobby(lobbyName);
            }
        }

        public void HostLobby()
        {
            MenuUI ui = currLevelUI as MenuUI;
            if (ui != null)
            {
                ui.UserInput.interactable = false;
                ui.ConnectObj.SetActive(false);
                ui.LobbySelect.SetActive(false);
                ui.LobbyParent.SetActive(true);
                string lobbyName = ui.LobbyInput.text;
                NetworkManager.Instance.HostLobby(lobbyName);
            }
        }

        public void StartGame()
        {
            MenuUI ui = currLevelUI as MenuUI;
            if (ui != null)
            {
                ui.UserInput.interactable = false;
                ui.ConnectObj.SetActive(false);
                ui.LobbySelect.SetActive(false);
                ui.LobbyParent.SetActive(false);
                NetworkManager.Instance.StartGame();
            }
        }

        public void OnDisconnect()
        {
            MenuUI ui = currLevelUI as MenuUI;
            if (ui != null)
            {
                ui.UserInput.interactable = true;
                ui.ConnectObj.SetActive(true);
            }
        }

        #region ClientToServer Message Sent

        public void SendName()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerRequest.ConnectRequest);
            msg.AddString("bb");
            NetworkManager.Instance.Client.Send(msg);
        }

        public void SendHostRequest(string lobbyName, ushort playerId)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerRequest.HostRequest);
            //Send player id, lobby name
            msg.AddUShort(playerId);
            msg.AddString(lobbyName);

            NetworkManager.Instance.Client.Send(msg);
        }

        public void SendJoinRequest(ushort lobbyId, ushort playerId)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerRequest.JoinRequest);
            msg.AddUShort(playerId);
            msg.AddUShort(lobbyId);

            NetworkManager.Instance.Client.Send(msg);
        }

        #endregion

    }
}

