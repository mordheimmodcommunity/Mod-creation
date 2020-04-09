using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TNet
{
	public class UPnP
	{
		public enum Status
		{
			Inactive,
			Searching,
			Success,
			Failure
		}

		private class ExtraParams
		{
			public Thread th;

			public string action;

			public string request;

			public int port;

			public ProtocolType protocol;

			public OnPortRequest callback;
		}

		public delegate void OnPortRequest(UPnP up, int port, ProtocolType protocol, bool success);

		private Status mStatus;

		private IPAddress mGatewayAddress = IPAddress.None;

		private Thread mDiscover;

		private string mGatewayURL;

		private string mControlURL;

		private string mServiceType;

		private List<Thread> mThreads = new List<Thread>();

		private List<int> mPorts = new List<int>();

		public string name = "TNetServer";

		public Status status => mStatus;

		public IPAddress gatewayAddress => mGatewayAddress;

		public bool hasThreadsActive => mThreads.size > 0;

		public UPnP()
		{
			Thread thread = mDiscover = new Thread(ThreadDiscover);
			mThreads.Add(thread);
			thread.Start(thread);
		}

		~UPnP()
		{
			mDiscover = null;
			Close();
			WaitForThreads();
		}

		public void Close()
		{
			lock (mThreads)
			{
				int num = mThreads.size;
				while (num > 0)
				{
					Thread thread = mThreads[--num];
					if (thread != mDiscover)
					{
						thread.Abort();
						mThreads.RemoveAt(num);
					}
				}
			}
			int num2 = mPorts.size;
			while (num2 > 0)
			{
				int num3 = mPorts[--num2];
				int port = num3 >> 8;
				bool tcp = (num3 & 1) == 1;
				Close(port, tcp, null);
			}
		}

		public void WaitForThreads()
		{
			int num = 0;
			while (mThreads.size > 0 && num < 2000)
			{
				Thread.Sleep(1);
				num++;
			}
		}

		private void ThreadDiscover(object obj)
		{
			//Discarded unreachable code: IL_013b
			Thread item = (Thread)obj;
			string s = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nST:upnp:rootdevice\r\nMAN:\"ssdp:discover\"\r\nMX:3\r\n\r\n";
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			int port = 10000 + (int)(DateTime.Now.Ticks % 45000);
			List<IPAddress> localAddresses = Tools.localAddresses;
			for (int i = 0; i < localAddresses.size; i++)
			{
				IPAddress address = localAddresses[i];
				mStatus = Status.Searching;
				UdpClient udpClient = null;
				try
				{
					UdpClient udpClient2 = new UdpClient(new IPEndPoint(address, port));
					udpClient2.Connect(IPAddress.Broadcast, 1900);
					udpClient2.Send(bytes, bytes.Length);
					udpClient2.Close();
					udpClient = new UdpClient(new IPEndPoint(address, port));
					udpClient.Client.ReceiveTimeout = 3000;
					IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
					byte[] array;
					do
					{
						array = udpClient.Receive(ref remoteEP);
					}
					while (!ParseResponse(Encoding.ASCII.GetString(array, 0, array.Length)));
					udpClient.Close();
					lock (mThreads)
					{
						mGatewayAddress = remoteEP.Address;
						mStatus = Status.Success;
						mThreads.Remove(item);
					}
					mDiscover = null;
					return;
				}
				catch (Exception)
				{
				}
				udpClient?.Close();
				lock (mThreads)
				{
					mStatus = Status.Failure;
					mThreads.Remove(item);
				}
				mDiscover = null;
				if (mStatus == Status.Success)
				{
					break;
				}
			}
			if (mStatus != Status.Success)
			{
				Console.WriteLine("UPnP discovery failed. TNet won't be able to open ports automatically.");
			}
		}

		private bool ParseResponse(string response)
		{
			int num = response.IndexOf("LOCATION:", StringComparison.OrdinalIgnoreCase);
			if (num == -1)
			{
				return false;
			}
			num += 9;
			int num2 = response.IndexOf('\r', num);
			if (num2 == -1)
			{
				return false;
			}
			string text = response.Substring(num, num2 - num).Trim();
			int num3 = text.IndexOf("://");
			num3 = text.IndexOf('/', num3 + 3);
			mGatewayURL = text.Substring(0, num3);
			return GetControlURL(text);
		}

		private bool GetControlURL(string url)
		{
			string response = Tools.GetResponse(WebRequest.Create(url));
			if (string.IsNullOrEmpty(response))
			{
				return false;
			}
			mServiceType = "WANIPConnection";
			int num = response.IndexOf(mServiceType);
			if (num == -1)
			{
				mServiceType = "WANPPPConnection";
				num = response.IndexOf(mServiceType);
				if (num == -1)
				{
					return false;
				}
			}
			int num2 = response.IndexOf("</service>", num);
			if (num2 == -1)
			{
				return false;
			}
			int num3 = response.IndexOf("<controlURL>", num, num2 - num);
			if (num3 == -1)
			{
				return false;
			}
			num3 += 12;
			num2 = response.IndexOf("</controlURL>", num3, num2 - num3);
			if (num2 == -1)
			{
				return false;
			}
			mControlURL = mGatewayURL + response.Substring(num3, num2 - num3);
			return true;
		}

		private string SendRequest(string action, string content, int timeout, int repeat)
		{
			string str = "<?xml version=\"1.0\"?>\n<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\n<s:Body>\n<m:" + action + " xmlns:m=\"urn:schemas-upnp-org:service:" + mServiceType + ":1\">\n";
			if (!string.IsNullOrEmpty(content))
			{
				str += content;
			}
			str = str + "</m:" + action + ">\n</s:Body>\n</s:Envelope>\n";
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			string text = null;
			try
			{
				for (int i = 0; i < repeat; i++)
				{
					WebRequest webRequest = WebRequest.Create(mControlURL);
					webRequest.Timeout = timeout;
					webRequest.Method = "POST";
					webRequest.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:" + mServiceType + ":1#" + action + "\"");
					webRequest.ContentType = "text/xml; charset=\"utf-8\"";
					webRequest.ContentLength = bytes.Length;
					webRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
					text = Tools.GetResponse(webRequest);
					if (!string.IsNullOrEmpty(text))
					{
						return text;
					}
				}
			}
			catch (Exception)
			{
			}
			return null;
		}

		public void OpenTCP(int port)
		{
			Open(port, tcp: true, null);
		}

		public void OpenUDP(int port)
		{
			Open(port, tcp: false, null);
		}

		public void OpenTCP(int port, OnPortRequest callback)
		{
			Open(port, tcp: true, callback);
		}

		public void OpenUDP(int port, OnPortRequest callback)
		{
			Open(port, tcp: false, callback);
		}

		private void Open(int port, bool tcp, OnPortRequest callback)
		{
			int item = (port << 8) | (tcp ? 1 : 0);
			if (port > 0 && !mPorts.Contains(item) && mStatus != Status.Failure)
			{
				string text = Tools.localAddress.ToString();
				if (!(text == "127.0.0.1"))
				{
					mPorts.Add(item);
					ExtraParams extraParams = new ExtraParams();
					extraParams.callback = callback;
					extraParams.port = port;
					extraParams.protocol = ((!tcp) ? ProtocolType.Udp : ProtocolType.Tcp);
					extraParams.action = "AddPortMapping";
					extraParams.request = "<NewRemoteHost></NewRemoteHost>\n<NewExternalPort>" + port + "</NewExternalPort>\n<NewProtocol>" + ((!tcp) ? "UDP" : "TCP") + "</NewProtocol>\n<NewInternalPort>" + port + "</NewInternalPort>\n<NewInternalClient>" + text + "</NewInternalClient>\n<NewEnabled>1</NewEnabled>\n<NewPortMappingDescription>" + name + "</NewPortMappingDescription>\n<NewLeaseDuration>0</NewLeaseDuration>\n";
					extraParams.th = new Thread(OpenRequest);
					lock (mThreads)
					{
						mThreads.Add(extraParams.th);
					}
					extraParams.th.Start(extraParams);
				}
			}
			else
			{
				callback?.Invoke(this, port, (!tcp) ? ProtocolType.Udp : ProtocolType.Tcp, success: false);
			}
		}

		public void CloseTCP(int port)
		{
			Close(port, tcp: true, null);
		}

		public void CloseUDP(int port)
		{
			Close(port, tcp: false, null);
		}

		public void CloseTCP(int port, OnPortRequest callback)
		{
			Close(port, tcp: true, callback);
		}

		public void CloseUDP(int port, OnPortRequest callback)
		{
			Close(port, tcp: false, callback);
		}

		private void Close(int port, bool tcp, OnPortRequest callback)
		{
			int item = (port << 8) | (tcp ? 1 : 0);
			if (port > 0 && mPorts.Remove(item) && mStatus == Status.Success)
			{
				ExtraParams extraParams = new ExtraParams();
				extraParams.callback = callback;
				extraParams.port = port;
				extraParams.protocol = ((!tcp) ? ProtocolType.Udp : ProtocolType.Tcp);
				extraParams.action = "DeletePortMapping";
				extraParams.request = "<NewRemoteHost></NewRemoteHost>\n<NewExternalPort>" + port + "</NewExternalPort>\n<NewProtocol>" + ((!tcp) ? "UDP" : "TCP") + "</NewProtocol>\n";
				if (callback != null)
				{
					extraParams.th = new Thread(CloseRequest);
					lock (mThreads)
					{
						mThreads.Add(extraParams.th);
						extraParams.th.Start(extraParams);
					}
				}
				else
				{
					CloseRequest(extraParams);
				}
			}
			else
			{
				callback?.Invoke(this, port, (!tcp) ? ProtocolType.Udp : ProtocolType.Tcp, success: false);
			}
		}

		private void OpenRequest(object obj)
		{
			while (mStatus == Status.Searching)
			{
				Thread.Sleep(1);
			}
			SendRequest((ExtraParams)obj);
		}

		private void CloseRequest(object obj)
		{
			SendRequest((ExtraParams)obj);
		}

		private void SendRequest(ExtraParams xp)
		{
			string value = (mStatus != Status.Success) ? null : SendRequest(xp.action, xp.request, 10000, 3);
			if (xp.callback != null)
			{
				xp.callback(this, xp.port, xp.protocol, !string.IsNullOrEmpty(value));
			}
			if (xp.th != null)
			{
				lock (mThreads)
				{
					mThreads.Remove(xp.th);
				}
			}
		}
	}
}
