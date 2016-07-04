using UnityEngine;
using System.Collections;

public class Systems : MonoBehaviour
{
	public static Systems Instance
	{
		get
		{
			return s_instance;
		}
	}

	static Systems s_instance;

	[SerializeField]
	GameObject m_systemsPrefab;

	[SerializeField]
	State m_startState = global::State.None;

	GameObject m_systems;

	//List your systems here
	public StateManager State { get; private set; }

	void Awake()
	{
		if (s_instance == null)
		{
			s_instance = this;
			DontDestroyOnLoad(this.gameObject);

			m_systems = Instantiate(m_systemsPrefab);
			m_systems.transform.parent = transform;

			//Fetch system components
			State = m_systems.GetComponent<StateManager>();

			// start state
			State.SetStateImmediately(m_startState);
		}
		else
		{
			Destroy(this.gameObject);
		}
	}
}
