using UnityEngine;
using System.Collections;

public class FollowBot : MonoBehaviour
{
	[SerializeField]
	GameObject m_target;

	void Update ()
	{
		float x = Mathf.Lerp(transform.position.x, m_target.transform.position.x, Time.deltaTime * 1f);

		var pos = transform.position;
		pos.x = x;

		transform.position = pos;
	}
}
