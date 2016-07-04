using UnityEngine;
using System.Collections;

public class WebServerSystem : MonoBehaviour
{
	[SerializeField]
	int m_port = 8081;

	public HTTPServer Server { get; private set; }

	void Awake ()
	{
		Server = new HTTPServer(m_port);
	}
}
