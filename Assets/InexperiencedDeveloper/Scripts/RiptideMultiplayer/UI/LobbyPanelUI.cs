using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev.UI
{
    public class LobbyPanelUI : MonoBehaviour
    {
        public TMP_Text LobbyName;
        [SerializeField] private GameObject lobbyInfoPanel;
        [SerializeField] private Transform playerPanelParent;
        [SerializeField] private GameObject[] playerPanels;
        [SerializeField] private GameObject startButton;
        public GameObject LobbyInfoPanel => lobbyInfoPanel;
        public Transform PlayerPanelParent => playerPanelParent;
        public GameObject[] PlayerPanels => playerPanels;
        public GameObject StartButton => startButton;

        private void Awake()
        {
            startButton.GetComponent<Button>().onClick.AddListener(() => { UIManager.Instance.StartGame(); });
        }
    }
}

