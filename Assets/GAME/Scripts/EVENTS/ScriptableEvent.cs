using System;
using UnityEngine;

namespace Utils.CustomEvents
{
    [CreateAssetMenu(fileName = "Event", menuName = "Create Event", order = 0)]
    public class ScriptableEvent : ScriptableObject
    {
        public static ScriptableEvent Load(string eventName)
        {
            return Resources.Load<ScriptableEvent>($"Events/{eventName}");
        }
        
        public event Action OnInvoke;

        public void Invoke()
        {
            OnInvoke?.Invoke();
        }
    }
}
