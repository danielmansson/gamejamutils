using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

//https://gist.github.com/aksakalli/9191056



public interface HTTPRequestHandler
{
	string GetResponseBody(HttpListenerContext context);
}

public class HTTPServer
{
	private Thread m_serverThread;
	private HttpListener m_listener;
	private int m_port;
	Dictionary<string, HTTPRequestHandler> m_requestHandlers = new Dictionary<string, HTTPRequestHandler>();

	public void AddHandler(string id, HTTPRequestHandler handler)
	{
		m_requestHandlers.Add(id, handler);
	}

	public void RemoveHandler(string id)
	{
		m_requestHandlers.Remove(id);
	}

	public int Port
	{
		get { return m_port; }
		private set { }
	}

	public HTTPServer(int port)
	{
		this.Initialize(port);
	}

	public void Stop()
	{
		m_serverThread.Abort();
		m_listener.Stop();
	}

	private void Listen()
	{
		m_listener = new HttpListener();
		m_listener.Prefixes.Add("http://*:" + m_port.ToString() + "/");
		m_listener.Start();
		while (true)
		{
			try
			{
				HttpListenerContext context = m_listener.GetContext();
				Process(context);
			}
			catch (Exception)
			{
			}
		}
	}

	private void Process(HttpListenerContext context)
	{
		string filename = context.Request.Url.AbsolutePath;
		HTTPRequestHandler handler;

		if (m_requestHandlers.TryGetValue(filename, out handler))
		{
			try
			{
				var payload = handler.GetResponseBody(context);

				context.Response.ContentType = "text/html";
				context.Response.ContentLength64 = payload.Length;
				context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
				context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
				context.Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
				context.Response.AddHeader("Pragma", "no-cache");
				context.Response.AddHeader("Expires", "0");

				byte[] buffer = Encoding.UTF8.GetBytes(payload);
				context.Response.OutputStream.Write(buffer, 0, buffer.Length);

				context.Response.StatusCode = (int)HttpStatusCode.OK;
				context.Response.OutputStream.Flush();
			}
			catch (Exception)
			{
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}
		}
		else
		{
			context.Response.StatusCode = (int)HttpStatusCode.NotFound;
		}

		context.Response.OutputStream.Close();
	}

	private void Initialize(int port)
	{
		this.m_port = port;
		m_serverThread = new Thread(this.Listen);
		m_serverThread.Start();
	}
}