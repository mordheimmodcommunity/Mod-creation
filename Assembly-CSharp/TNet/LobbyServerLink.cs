using System;
using System.Net;
using System.Threading;

namespace TNet
{
    public class LobbyServerLink
    {
        private LobbyServer mLobby;

        private long mNextSend;

        protected GameServer mGameServer;

        protected Thread mThread;

        protected IPEndPoint mInternal;

        protected IPEndPoint mExternal;

        protected bool mShutdown;

        public virtual bool isActive => mLobby != null && mExternal != null;

        public LobbyServerLink(LobbyServer lobbyServer)
        {
            mLobby = lobbyServer;
        }

        public virtual void Start()
        {
            mShutdown = false;
        }

        public virtual void Stop()
        {
            if (!mShutdown)
            {
                mShutdown = true;
                if (mExternal != null && mLobby != null)
                {
                    mLobby.RemoveServer(mInternal, mExternal);
                }
            }
        }

        public virtual void SendUpdate(GameServer gameServer)
        {
            if (!mShutdown)
            {
                mGameServer = gameServer;
                if (mExternal != null)
                {
                    long num = DateTime.Now.Ticks / 10000;
                    mNextSend = num + 3000;
                    mLobby.AddServer(mGameServer.name, mGameServer.playerCount, mInternal, mExternal);
                }
                else if (mThread == null)
                {
                    mThread = new Thread(SendThread);
                    mThread.Start();
                }
            }
        }

        private void SendThread()
        {
            mInternal = new IPEndPoint(Tools.localAddress, mGameServer.tcpPort);
            mExternal = new IPEndPoint(Tools.externalAddress, mGameServer.tcpPort);
            if (mLobby is UdpLobbyServer)
            {
                while (!mShutdown)
                {
                    long num = DateTime.Now.Ticks / 10000;
                    if (mNextSend < num && mGameServer != null)
                    {
                        mNextSend = num + 3000;
                        mLobby.AddServer(mGameServer.name, mGameServer.playerCount, mInternal, mExternal);
                    }
                    Thread.Sleep(10);
                }
            }
            else
            {
                mLobby.AddServer(mGameServer.name, mGameServer.playerCount, mInternal, mExternal);
            }
            mThread = null;
        }
    }
}
