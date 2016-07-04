using UnityEngine;
using System.Collections;

public class ScalarPID
{
	public float P { get; set; }
	public float I { get; set; }
	public float D { get; set; }

	public float ValueP { get; private set; }
	public float ValueI { get; private set; }
	public float ValueD { get; private set; }

	public float ClampingI { get; set; }
	public float DampingI { get; set; }

	float m_previousError;
	float m_reset = 0f;

	public ScalarPID()
	{
		ClampingI = 100f;
	}

	public float Step(float error, float targetDelta, float timeStep)
	{
		m_previousError -= targetDelta;
		ValueP = error * P; //Current error [How far am I right now?]

		//I - I've been wrong for to long!
		float newResetTarget = m_reset + I * error * timeStep;
		//
		m_reset = Mathf.Lerp(m_reset, newResetTarget, Mathf.Clamp01((1f - DampingI)));
		m_reset = Mathf.Clamp(m_reset, -ClampingI, ClampingI);

		ValueI = m_reset;

		//D - How fast am I moving?
		//Should I chill maybe?
		ValueD = (error - m_previousError) * D;

		m_previousError = error;

		return ValueP + ValueI + ValueD;
	}
}
