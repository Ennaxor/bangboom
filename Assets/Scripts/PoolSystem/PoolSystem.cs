using System.Collections.Generic;
using UnityEngine;

namespace BangBoom.Pool
{
    public class PoolSystem : MonoBehaviour
    {
        [SerializeField] private GameObject poolObject;
        [SerializeField] private int initialAmount = 5;
        [SerializeField] private Transform poolContainer;
    
        private readonly List<GameObject> poolList = new List<GameObject>();
    
        private void Start()
        {
            InitPool();
        }
    
        private void InitPool()
        {
            for (var i = 0; i < initialAmount; ++i)
            {
                var gameObject = Instantiate(poolObject, poolContainer);
                gameObject.SetActive(false);
                poolList.Add(gameObject);
            }
        }
    
        public GameObject Request()
        {
            GameObject pooledObject;
            
            if (poolList.Count > 0)
            {
                pooledObject = poolList[0];
                poolList.RemoveAt(0);
            }
            else
            {
                pooledObject = Instantiate(poolObject, poolContainer);
            }
            
            pooledObject.SetActive(true);
            return pooledObject;
        }
        
        public void Return(GameObject gameObject)
        {
            gameObject.SetActive(false);
            poolList.Add(gameObject);
        }
    }
}