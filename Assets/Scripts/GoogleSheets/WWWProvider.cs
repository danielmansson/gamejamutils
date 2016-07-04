using UnityEngine;
using System;
using System.Collections;

public class WWWProviderResult
{
	public string error;
	public string text;
}

public interface WWWProvider
{
	void HttpGet(string url, Action<WWWProviderResult> callback);
}

public class UnityWWWProvider : WWWProvider
{
	public void HttpGet(string url, Action<WWWProviderResult> callback)
	{
		//Ugly & lazy reference to CoroutineHelper
		CoroutineHelper.RunCoroutine(PerformRequest(url, callback));
	}

	IEnumerator PerformRequest(string url, Action<WWWProviderResult> callback)
	{
		WWW www = new WWW(url);
		yield return www;

		var result = new WWWProviderResult()
		{
			error = www.error,
			text = www.text
		};

		if (callback != null)
		{
			callback(result);
		}
	}
}