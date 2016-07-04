using UnityEngine;
using System.Collections;

public class BotController : MonoBehaviour
{
	[System.Serializable]
	public class PIDConfig
	{
		public float p;
		public float i;
		public float d;
		public float dampingI;
		public float clampingI = 100f;
	}

	[SerializeField]
	Rigidbody2D m_wheel;
	[SerializeField]
	HingeJoint2D m_wheelMotor;

	[SerializeField]
	Rigidbody2D m_body;

	[Header("Balancer")]
	[SerializeField]
	bool m_useBalancer = true;
	[SerializeField]
	PIDConfig m_balanceConfig;
	[SerializeField]
	float m_targetRotation = 0f;

	ScalarPID m_balancePID = new ScalarPID();
	float m_prevTargetRotation;

	[Header("Mover")]
	[SerializeField]
	bool m_useMover = false;
	[SerializeField]
	PIDConfig m_moverConfig;
	[SerializeField]
	float m_targetVelocity;

	ScalarPID m_moverPID = new ScalarPID();
	float m_prevTargetVelocity;

	void Start ()
	{
	}
	
	void FixedUpdate ()
	{
		UpdatePIDConfig();
		
		//Mover
		{
			float error = m_targetVelocity - m_body.velocity.x;
			var value = m_moverPID.Step(error, m_targetVelocity - m_prevTargetVelocity, Time.fixedDeltaTime);
			m_prevTargetRotation = m_targetVelocity;

			if (m_useMover)
			{
				//This is the target for the next PID controller
				m_targetRotation = -value;
			}
		}

		//Balancer
		{
			float error = Mathf.DeltaAngle(m_body.rotation, m_targetRotation);
			var value = m_balancePID.Step(error, m_targetRotation - m_prevTargetRotation, Time.fixedDeltaTime);
			m_prevTargetRotation = m_targetRotation;

			m_wheelMotor.motor = new JointMotor2D()
			{
				motorSpeed = value,
				maxMotorTorque = m_wheelMotor.motor.maxMotorTorque
			};

			m_wheelMotor.useMotor = m_useBalancer;
		}
	}

	void UpdatePIDConfig()
	{
		m_balancePID.P = m_balanceConfig.p;
		m_balancePID.I = m_balanceConfig.i;
		m_balancePID.D = m_balanceConfig.d;
		m_balancePID.DampingI = m_balanceConfig.dampingI;
		m_balancePID.ClampingI = m_balanceConfig.clampingI;

		m_moverPID.P = m_moverConfig.p;
		m_moverPID.I = m_moverConfig.i;
		m_moverPID.D = m_moverConfig.d;
		m_moverPID.DampingI = m_moverConfig.dampingI;
		m_moverPID.ClampingI = m_moverConfig.clampingI;
	}
}
