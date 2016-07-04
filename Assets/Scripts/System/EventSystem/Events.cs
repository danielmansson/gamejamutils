using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventChangeGameState : EventArgs
{
	public State From;
	public State To;

	public EventChangeGameState(State from, State to)
	{
		From = from;
		To = to;
	}
}

public class EventLevelLoad : EventArgs
{
	public string LevelName;

	public EventLevelLoad(string name)
	{
		LevelName = name;
	}
}


