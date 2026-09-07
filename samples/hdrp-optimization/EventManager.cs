using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager _Instance { get { return _instance; } }

    private static EventManager _instance = null;
    private Dictionary<EVENT_TYPE, List<IListener>> LIsteners = new Dictionary<EVENT_TYPE, List<IListener>>();

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        DestroyImmediate(gameObject);
    }

    public void AddListener(EVENT_TYPE eventType, IListener listener)
    {
        List<IListener> ListenList = null;

        if(LIsteners.TryGetValue(eventType, out ListenList))
        {
            ListenList.Add(listener);
            return;
        }

        ListenList = new List<IListener> {listener};
        LIsteners.Add(eventType, ListenList);
    }

    public void PostNotification(EVENT_TYPE eventType, Component Sender, object param = null)
    {
        List<IListener> ListenList = null;

        if (!LIsteners.TryGetValue(eventType, out ListenList)) return;

        for(int i = 0; i < ListenList.Count; i++)
        {
            ListenList[i].OnEvent(eventType, Sender, param);
        }
    }

    public void RemoveEvent(EVENT_TYPE eventType) => LIsteners.Remove(eventType);

    public void RemoveRedundancies()
    {
        Dictionary<EVENT_TYPE, List<IListener>> newListeners = new Dictionary<EVENT_TYPE, List<IListener>>();

        foreach(KeyValuePair<EVENT_TYPE, List<IListener>> Item in LIsteners)
        {
            for (int i = Item.Value.Count - 1; i >= 0; i--)
            {
                if (Item.Value[i].Equals(null)) Item.Value.RemoveAt(i);
            }

            if(Item.Value.Count > 0) newListeners.Add(Item.Key, Item.Value);
        }

        LIsteners = newListeners;
    }

    private void OnLevelWasLoaded(int level)
    {
        RemoveRedundancies();
    }
}
