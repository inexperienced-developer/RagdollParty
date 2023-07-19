using InexperiencedDeveloper.ActiveRagdoll;
using Riptide;
using UnityEngine;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev
{
    public class NetPlayer : MonoBehaviour
    {
        public Player Player;
        public ushort Id { get; private set; }
        public bool IsLocal { get; private set; }
        public bool IsHost { get; private set; }
        public Lobby MyLobby { get; private set; }

        public string Username { get; private set; }

        private void Awake()
        {
            Init(1, "a");
        }

        private void OnDestroy()
        {
            NetPlayerManager.NetPlayers.Remove(Id);
            print("Removed " + Id);
        }

        public bool Init(ushort id, string username)
        {
            NetPlayerManager.RagdollSetup(this);
            return true;
            //IsLocal = NetworkManager.Instance.Client.Id == id;
            //Id = id;
            //this.Username = username;
            //return IsLocal;
        }

        public void JoinLobby(Lobby lobby)
        {
            MyLobby = lobby;
            IsHost = MyLobby.Host.Id == Id;
            if (lobby.LobbyUI != null)
                lobby.LobbyUI.SetActive(true);
            else
                Debug.LogWarning("No lobby UI");
        }

        public void Update()
        {
            if (IsLocal)
                SendInputs();
        }

        private void FixedUpdate()
        {
            if(Player == null)
            {
                Player = GetComponent<Player>();
                return;
            }

        }


        #region ClientToServer Message Sender

        private void SendInputs()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, ClientToServerRequest.MoveRequest);
            Vector2 movement = new Vector2(Player.Controls.Movement.x, Player.Controls.Movement.z);
            Vector2 lookDir = new Vector2(Player.Controls.CameraPitchAngle, Player.Controls.CameraYawAngle);
            bool jump = Player.Controls.Jump;

            msg.AddVector2(movement);
            msg.AddVector2(lookDir);
            msg.AddBool(jump);
            NetworkManager.Instance.Client.Send(msg);
        }



        #endregion

        #region ServerToClient Message Handler

        #endregion

    }//class
}//namespace

