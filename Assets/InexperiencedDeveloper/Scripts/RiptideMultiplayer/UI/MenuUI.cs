using UnityEngine;
using TMPro;

public class MenuUI : LevelUI
{
    [Header("Connect UI")]
    [SerializeField] private GameObject connectObj;
    [SerializeField] private TMP_InputField userInput;
    public GameObject ConnectObj => connectObj;
    public TMP_InputField UserInput => userInput;

    [Header("Lobby UI")]
    [SerializeField] private GameObject lobbySelect;
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private GameObject lobbyParent;
    public GameObject LobbySelect => lobbySelect;
    public TMP_InputField LobbyInput => lobbyInput;
    public GameObject LobbyParent => lobbyParent;
}
