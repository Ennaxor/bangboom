using Bangboom.Player.Input;
using Photon.Pun;
using UnityEngine;

namespace BangBoom.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;

        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float minY;
        [SerializeField] private float maxY;

        private void Start()
        {
            PhotonNetwork.Instantiate(playerPrefab.name, GetRandomInitialPos(), Quaternion.identity, 0);
        }

        private Vector2 GetRandomInitialPos()
        {
            return new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        }
    }
}