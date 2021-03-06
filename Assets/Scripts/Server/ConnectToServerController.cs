using BangBoom.Config;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace BangBoom.Server
{
    public class ConnectToServerController : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            SceneManager.LoadScene(Scenes.LobbyScene);
        }
    }
}
