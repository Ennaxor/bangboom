using BangBoom.Background;
using Bangboom.Player.Input;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

namespace BangBoom.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private CinemachineVirtualCamera followCamera;
        [SerializeField] private ScrollingBackground ScrollingBackground;

        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float minY;
        [SerializeField] private float maxY;

        private void Start()
        {
            var player = PhotonNetwork.Instantiate(playerPrefab.name, GetRandomInitialPos(), Quaternion.identity, 0);

            if(followCamera)
            {
                followCamera.Follow = player.transform;
                ScrollingBackground.Player = player;
            }
        }

        private Vector2 GetRandomInitialPos()
        {
            return new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        }
    }
}