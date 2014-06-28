using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using D3DDetour;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Media;
using System.Drawing;



namespace EveModel
{
	
	

	public class Frame
	{
		public delegate void Message(string msg, Color? col);
		public static event Message OnMessage;
		const D3DVersion dxver = D3DVersion.Direct3D9;
		public event EventHandler OnFrame;
		public static EveClient Client { get; private set; }
		protected static readonly object _frameLock = new object();
		
		static Frame()
		{
			
		}

		public Frame()
		{
			while(Pulse.Hook == null) {
				Application.DoEvents();
				Thread.Sleep(1000);
				Pulse.Initialize(D3DVersion.Direct3D9);
			}
			D3DHook.OnFrame += new EventHandler(OnD3DFrame);

			
		}
		/// <summary>
		/// This method will be called for every frame captured by D3DDetour
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="lse"></param>
		public void OnD3DFrame(object sender,EventArgs e)
		{
			lock (_frameLock)
			{
					using (EveClient _client = new EveClient())
					{
						Client = _client;
						if (OnFrame != null)
							OnFrame(this, new EventArgs());
						Client = null;
						
					}
			}

		}
		public void Dispose()
		{
			if (Client != null) {
				Client.Dispose();
			}
			D3DHook.OnFrame -= new EventHandler(OnD3DFrame);
			Pulse.Shutdown();
			
		}
		/// <summary>
		/// Log
		/// </summary>
		/// <param name="text">Text to log</param>
		public static void Log(string text, Color? col = null)
		{
			try {
				if (OnMessage != null)
				{
					OnMessage(DateTime.UtcNow.ToString() + " " + text.ToString(), col);
				}
			} catch (Exception) {
				return;
			}
			
		}

		/// <summary>
		/// Logs an objects property names and values
		/// </summary>
		/// <param name="obj">Object whos properties should be exposed</param>
		public static void ExposeObject(object obj)
		{
			Log("Exposing: " + obj.GetType().Name);
			foreach (var item in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var property = obj.GetType().GetProperty(item.Name);
				var index = property.GetIndexParameters().Count();
				if (index == 0)
				{
					Log(item.Name + ": " + obj.GetType().GetProperty(item.Name).GetValue(obj, null));
				}
				else
				{
					Log(item.Name + ": " + obj.GetType().GetProperty(item.Name).PropertyType + "[" + index + "]");
				}
			}
		}
	}
}

