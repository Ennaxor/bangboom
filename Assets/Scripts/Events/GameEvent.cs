using System.Collections.Generic;
using UnityEngine;

namespace BangBoom.Events
{
    public abstract class GameEvent<T> : ScriptableObject
    {
        private readonly List<GameEventListener<T>> eventListeners = new List<GameEventListener<T>>();

        public void Trigger(T obj)
        {
            for (var i = eventListeners.Count - 1; i >= 0; --i)
            {
                eventListeners[i].OnEventTriggered(obj);
            }
        }

        public void RegisterListener(GameEventListener<T> listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
            }
        }
        
        public void UnregisterListener(GameEventListener<T> listener)
        {
            if (eventListeners.Contains(listener))
            {
                eventListeners.Remove(listener);
            }
        }
    }
    
    
    public abstract class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> eventListeners = new List<GameEventListener>();

        public void Trigger()
        {
            for (var i = eventListeners.Count - 1; i >= 0; --i)
            {
                eventListeners[i].OnEventTriggered();
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
            }
        }
        
        public void UnregisterListener(GameEventListener listener)
        {
            if (eventListeners.Contains(listener))
            {
                eventListeners.Remove(listener);
            }
        }
    }
}