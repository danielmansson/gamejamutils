using UnityEngine;
using System.Collections;

public class EventDispatcher : MonoBehaviour
{
    EventManager m_eventManager;

    void Start()
    {
        m_eventManager = EventManager.Instance;
    }

    void Update()
    {
        m_eventManager.SendDelayedEvents(EventDelayCategory.NextFrame);
    }

    void FixedUpdate()
    {
        m_eventManager.SendDelayedEvents(EventDelayCategory.NextFixedUpdate);
    }
}
