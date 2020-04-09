using System.Net;

namespace TNet
{
	public abstract class LobbyServer : FileServer
	{
		public abstract int port
		{
			get;
		}

		public abstract bool isActive
		{
			get;
		}

		public abstract bool Start(int listenPort);

		public abstract void Stop();

		public abstract void AddServer(string name, int playerCount, IPEndPoint internalAddress, IPEndPoint externalAddress);

		public abstract void RemoveServer(IPEndPoint internalAddress, IPEndPoint externalAddress);
	}
}
