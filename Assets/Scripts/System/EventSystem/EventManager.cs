using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum EventDelayCategory
{
	Immidiate,
	NextFrame,
	NextFixedUpdate
}

public class EventArgs
{

}

public class EventManager
{
	#region Singleton implementation
	public static EventManager Instance
	{
		get
		{
			if(s_instance == null)
				s_instance = new EventManager();

			return s_instance;
		}
	}

	private static EventManager s_instance = null;
	#endregion
	
	Dictionary<System.Type, Dictionary<int, Action<EventArgs>>> m_eventHandlers;
	Dictionary<EventDelayCategory, List<EventArgs>> m_delayedEvents;

	private EventManager()
	{
        m_eventHandlers = new Dictionary<Type, Dictionary<int, Action<EventArgs>>>();
        m_delayedEvents = new Dictionary<EventDelayCategory, List<EventArgs>>();
	}
	
	public void RegisterEvent<T> (Action<T> eventHandler) where T : EventArgs
	{
        Type type = typeof(T);
        Dictionary<int, Action<EventArgs>> eventEmitters;
        
		if(!m_eventHandlers.TryGetValue(type, out eventEmitters))
		{
			eventEmitters = new Dictionary<int, Action<EventArgs>>();
			m_eventHandlers[type] = eventEmitters;
		}

        Action<EventArgs> eventEmitter = (eventArgs) => { eventHandler((T)eventArgs); };
        eventEmitters.Add(eventHandler.GetHashCode(), eventEmitter);
	}
	
	public void UnregisterEvent<T> (Action<T> eventHandler) where T : EventArgs
    {
        Type type = typeof(T);
		var eventEmitters = m_eventHandlers[type];

        if (eventEmitters == null)
        {
            throw new Exception("[EventManager] Tried to remove handler, but it was not registered: " + type.ToString());
        }
        else
        {
            eventEmitters.Remove(eventHandler.GetHashCode());
        }
	}

	public void SendEvent<T>(T eventArgs, EventDelayCategory delayCategory = EventDelayCategory.Immidiate) where T : EventArgs
	{
		if(delayCategory == EventDelayCategory.Immidiate)
		{
			SendEventImmidiate(eventArgs);
		}
		else
		{
			var delayEventList = m_delayedEvents[delayCategory];

			if(delayEventList == null)
			{
				delayEventList = new List<EventArgs>();
				m_delayedEvents[delayCategory] = null;
			}

			delayEventList.Add(eventArgs);
		}
	}

	public void SendDelayedEvents(EventDelayCategory category)
	{
		List<EventArgs> delayedEvents;
		
		if(m_delayedEvents.TryGetValue(category, out delayedEvents))
		{
			foreach (var eventArgs in delayedEvents) 
			{
				SendEventImmidiate(eventArgs);
			}
		}
	}

	private void SendEventImmidiate(EventArgs eventArgs)
	{
		var type = eventArgs.GetType();
		Dictionary<int, Action<EventArgs>> eventEmitters;

        if (m_eventHandlers.TryGetValue(type, out eventEmitters))
		{
            foreach (var eventEmitter in eventEmitters) 
			{
                eventEmitter.Value(eventArgs);
			}
		}
	}
}
