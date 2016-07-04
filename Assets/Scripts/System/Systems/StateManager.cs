using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public enum State
{
	None,
	Boot,
	Start,
	Game,
	Win,
}

public class StateManager : MonoBehaviour
{
	[SerializeField]
	private GameObject m_transitionPrefab;

	private GameStateBase m_currentState;
	private State m_queuedState = State.None;
	private Transition m_transition;

	public State CurrentState
	{
		get
		{
			if (m_currentState == null)
				return State.None;

			return m_currentState.State;
		}
	}

	IEnumerator Start()
	{
		var obj = Instantiate(m_transitionPrefab);
		m_transition = obj.GetComponent<Transition>();
		DontDestroyOnLoad(m_transition);

		while (true)
		{
			if (m_currentState != null)
			{
				m_currentState.Update();
			}

			if (m_queuedState != State.None)
			{
				var prevState = CurrentState;
				var nextState = m_queuedState;

				EventManager.Instance.SendEvent(new EventChangeGameState(prevState, nextState));

				// Fade out previous state
				yield return m_transition.StartCoroutine(m_transition.In());

				float start = Time.time;

				// Switch state
				if (m_currentState != null)
				{
					m_currentState.End(nextState);
				}

				m_currentState = CreateState(nextState);
				m_currentState.Begin(prevState);

				// Take the hit of some expensive Start frames
				for (int i = 0; i < 5; ++i)
					yield return null;

				float minimumSplashVisibleDuration = (nextState == State.Game || prevState == State.Game) ? 1.0f : 0.0f;
				while (Time.time < start + minimumSplashVisibleDuration)
					yield return null;

				// Fade in next state
				yield return m_transition.StartCoroutine(m_transition.Out());

				m_queuedState = State.None;
			}

			// Wait one frame until next "Update"
			yield return null;
		}
	}

	public void QueueState(State state)
	{
		if (m_queuedState == State.None)
		{
			m_queuedState = state;
		}
	}

	//Hacky factory. This should maybe be done properly
	private GameStateBase CreateState(State state)
	{
		switch (state)
		{
			case State.Boot:
				return new BootState();
			case State.Start:
				return new StartState();
			case State.Game:
				return new GameState();
			case State.Win:
				return new WinState();
		}

		return null;
	}

	public void SetStateImmediately(State nextState)
	{
		State prevState = CurrentState;
		if (m_currentState != null)
		{
			m_currentState.End(nextState);
		}
		m_currentState = CreateState(nextState);
		m_currentState.Begin(prevState);
	}
}

public abstract class GameStateBase
{
	public abstract State State { get; }

	public abstract void Begin(State previousState);
	public virtual void End(State nextState) { }
	public virtual void Update() { }

	protected bool IsSceneLoaded(string sceneName)
	{
		var scene = SceneManager.GetActiveScene();

		return scene.name == sceneName;
	}
}
