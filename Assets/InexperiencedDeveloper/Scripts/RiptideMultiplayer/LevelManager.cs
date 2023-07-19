using InexperiencedDeveloper.Core;
using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev
{
    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private int mainMenuIndex;

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
            if(scene.buildIndex != mainMenuIndex)
                SendSceneLoaded(NetPlayerManager.LocalPlayer.MyLobby.LobbyId);
        }

        public void LoadLevel(int levelIndex)
        {
            SceneManager.LoadScene(levelIndex);
        }

        #region ClientToServer Message Sender
        public void SendSceneLoaded(ushort lobbyId)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerRequest.SpawnRequest);

            msg.AddUShort(lobbyId);

            NetworkManager.Instance.Client.Send(msg);
        }

        #endregion
    }
}

