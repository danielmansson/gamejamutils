using UnityEngine;
using System.Collections;

public class MouseSpring : MonoBehaviour
{
	public float m_stiffness = 50f;
	public float m_damping = 5f;

	Rigidbody2D m_body;

	void Start ()
	{
		m_body = GetComponent<Rigidbody2D>();
	}
	
	void FixedUpdate ()
	{
		if (Input.GetMouseButton(0))
		{
			Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			var vec = wp - m_body.position;

			m_body.AddForce(vec * m_stiffness);

			//Not very clean :)
			var dampingForce = m_body.velocity * m_damping;
			if (dampingForce.magnitude > m_body.velocity.magnitude / Time.fixedDeltaTime)
				dampingForce = dampingForce.normalized * m_body.velocity.magnitude / Time.fixedDeltaTime;

			m_body.AddForce(-m_body.velocity * m_damping);
		}
	}
}
