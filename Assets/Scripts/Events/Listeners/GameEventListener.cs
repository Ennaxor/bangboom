using UnityEngine;
using UnityEngine.Events;

namespace BangBoom.Events
{
    public abstract class GameEventListener<T> : MonoBehaviour
    {
        public GameEvent<T> GameEvent;
        public UnityEvent<T> Response;

        private void OnEnable()
        {
            GameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            GameEvent.UnregisterListener(this);
        }
        
        public void OnEventTriggered(T obj)
        {
            Response.Invoke(obj);
        }
    }
    
    public abstract class GameEventListener : MonoBehaviour
    {
        public GameEvent GameEvent;
        public UnityEvent Response;

        private void OnEnable()
        {
            GameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            GameEvent.UnregisterListener(this);
        }
        
        public void OnEventTriggered()
        {
            Response.Invoke();
        }
    }
}