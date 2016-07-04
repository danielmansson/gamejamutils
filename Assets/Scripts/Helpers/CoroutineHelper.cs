using UnityEngine;
using System.Collections;

public class CoroutineHelper : MonoBehaviour
{
	static CoroutineHelper s_helper;

	public static Coroutine RunCoroutine(IEnumerator routine)
	{
		if (s_helper == null)
		{
			var go = new GameObject();
			go.name = "CoroutineHelper";
			GameObject.DontDestroyOnLoad(go);
			s_helper = go.AddComponent<CoroutineHelper>();
		}

		return s_helper.StartCoroutine(routine);
	}
}