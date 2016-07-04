using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Text;

public class DummyHTTPRequestHandler : HTTPRequestHandler
{
	public string GetResponseBody(HttpListenerContext context)
	{
		return "Hello world!";
	}
}

public class UnityLogHTTPRequestHandler : HTTPRequestHandler
{
	class LogEntry
	{
		public string message;
		public LogType type;
	}

	List<LogEntry> m_logEntries = new List<LogEntry>();
	Dictionary<LogType, string> m_colorLookup = new Dictionary<LogType, string>()
	{
		{ LogType.Log, "white" },
		{ LogType.Warning, "yellow" },
		{ LogType.Error, "red" },
		{ LogType.Exception, "blue" },
		{ LogType.Assert, "green" }
	};

	public UnityLogHTTPRequestHandler()
	{
		Application.logMessageReceived += OnLogMessage;
	}

	private void OnLogMessage(string condition, string stackTrace, LogType type)
	{
		m_logEntries.Add(new LogEntry()
		{
			message = condition,
			type = type
		});
	}

	public string GetResponseBody(HttpListenerContext context)
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("<html><head></head><body>");

		string sortStr = context.Request.QueryString["sort"];
		string limitStr = context.Request.QueryString["limit"];
		string filter = context.Request.QueryString["filter"];

		int limit = m_logEntries.Count;
		if (limitStr != null && int.TryParse(limitStr, out limit))
			limit = Mathf.Min(m_logEntries.Count, limit);

		bool descendingOrder = true;
		if (sortStr == "asc")
			descendingOrder = false;

		int count = 0;
		for (int i = 0; i < m_logEntries.Count; i++)
		{
			int index = descendingOrder ? i : (m_logEntries.Count - i - 1);
			var entry = m_logEntries[index];
			string color = m_colorLookup[entry.type];

			if (filter != null && !entry.message.Contains(filter))
				continue;

			builder.Append("<div style=\"background:" + color + "\">");
			builder.Append(entry.message);
			builder.AppendLine("</div>");

			++count;
			if (count >= limit)
			{
				break;
			}
		}

		builder.AppendLine("</body></html>");
		return builder.ToString();
	}
}

public class WebServerExample : MonoBehaviour
{
	[SerializeField]
	WebServerSystem m_webServer;

	void Start ()
	{
		m_webServer.Server.AddHandler("/dummy", new DummyHTTPRequestHandler());
		m_webServer.Server.AddHandler("/log", new UnityLogHTTPRequestHandler());
	}

	void OnGUI()
	{
		var rect = new Rect(30, 30, 200, 50);

		if (GUI.Button(rect, "Normal log"))
		{
			Debug.Log("Hello " + UnityEngine.Random.Range(0, 10));
		}

		rect.y += 60;
		if (GUI.Button(rect, "Warning"))
		{
			Debug.LogWarning("Warning " + UnityEngine.Random.Range(0, 10));
		}

		rect.y += 60;
		if (GUI.Button(rect, "Error"))
		{
			Debug.LogError("Error " + UnityEngine.Random.Range(0, 10));
		}
	}
}
