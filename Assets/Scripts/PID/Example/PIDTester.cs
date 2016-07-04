using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PIDTester : MonoBehaviour
{
	ScalarPID m_pid;

	public float m_p;
	public float m_i;
	public float m_d;
	public float m_clampingI = 100f;
	public float m_dampingI = 0f;

	public float totalTime = 10f;
	public float startTarget = 5f;
	public float startValue = 0f;
	public float timeStep = 1f / 60f;
	public float externalForce = 0f;
	public float maxMotorForce = 1000f;

	[System.Serializable]
	class Entry
	{
		public float t;
		public float target;
		public float x;
		public float v;
		public float dx;
		public float pv;
		public float iv;
		public float dv;
	}

	List<Entry> m_entries = new List<Entry>();

	void Start()
	{

	}

	void Update()
	{
		m_entries = new List<Entry>();
		int n = (int)(totalTime / timeStep) + 1;
		for (int i = 0; i < n; i++)
		{
			float target = startTarget * (i < n / 2 ? 0.5f : 1f);

			var entry = new Entry()
			{
				t = i * timeStep,
				target = target
			};

			m_entries.Add(entry);
		}

		m_entries[0].x = startValue;
		m_pid = new ScalarPID();

		m_pid.P = m_p;
		m_pid.I = m_i;
		m_pid.D = m_d;
		m_pid.DampingI = m_dampingI;
		m_pid.ClampingI = m_clampingI;

		for (int i = 0; i < n - 1; i++)
		{
			m_entries[i].x += m_entries[i].v * timeStep;

			float prevTarget = i == 0 ? 0f : m_entries[i - 1].target;
			float target = m_entries[i].target;
			float error = m_entries[i].target - m_entries[i].x;

			float dx = m_pid.Step(error, prevTarget - target, timeStep);

			m_entries[i].dx = dx;
			m_entries[i].pv = m_pid.ValueP;
			m_entries[i].iv = m_pid.ValueI;
			m_entries[i].dv = m_pid.ValueD;

			m_entries[i + 1].v = m_entries[i].v + Mathf.Clamp(dx, 0, maxMotorForce) * timeStep;
			m_entries[i + 1].x += m_entries[i].x;

			m_entries[i + 1].v += externalForce * timeStep;

		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawLine(Vector3.zero, Vector3.right * totalTime);

		foreach (var e in m_entries)
		{
			Vector3 pos = new Vector3();
			pos.x = e.t;

			pos.y = e.x;
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(pos, timeStep);

			pos.y = e.target;
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(pos, timeStep);

			pos.y = 0f;
			Gizmos.color = Color.black;
			Gizmos.DrawSphere(pos, timeStep);
		}
	}
}
