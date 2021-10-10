using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace BangBoom
{
    public class EnterRoomController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private InputField createInput;
        [SerializeField] private InputField joinInput;

        public void CreateRoom()
        {
            PhotonNetwork.CreateRoom(createInput.text);
        }

        public void JoinRoom()
        {
            PhotonNetwork.JoinRoom(joinInput.text);
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LoadLevel(Scenes.GameScene);
        }
    }
}
