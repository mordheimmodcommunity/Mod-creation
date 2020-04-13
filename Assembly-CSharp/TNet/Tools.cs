using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TNet
{
    public static class Tools
    {
        public delegate void OnResolvedIPs(IPAddress local, IPAddress ext);

        private static string mChecker;

        private static IPAddress mLocalAddress;

        private static IPAddress mExternalAddress;

        public static bool isExternalIPReliable;

        private static List<NetworkInterface> mInterfaces;

        private static List<IPAddress> mAddresses;

        private static OnResolvedIPs mOnResolve;

        private static Thread mResolveThread;

        public static string ipCheckerUrl
        {
            get
            {
                return mChecker;
            }
            set
            {
                if (mChecker != value)
                {
                    mChecker = value;
                    mLocalAddress = null;
                    mExternalAddress = null;
                }
            }
        }

        public static int randomPort => 10000 + (int)(DateTime.Now.Ticks % 50000);

        public static List<NetworkInterface> networkInterfaces
        {
            get
            {
                if (mInterfaces == null)
                {
                    mInterfaces = new List<NetworkInterface>();
                    NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                    NetworkInterface[] array = allNetworkInterfaces;
                    foreach (NetworkInterface networkInterface in array)
                    {
                        if (networkInterface.Supports(NetworkInterfaceComponent.IPv4) && (networkInterface.OperationalStatus == OperationalStatus.Up || networkInterface.OperationalStatus == OperationalStatus.Unknown))
                        {
                            mInterfaces.Add(networkInterface);
                        }
                    }
                }
                return mInterfaces;
            }
        }

        public static List<IPAddress> localAddresses
        {
            get
            {
                if (mAddresses == null)
                {
                    mAddresses = new List<IPAddress>();
                    try
                    {
                        List<NetworkInterface> networkInterfaces = Tools.networkInterfaces;
                        for (int i = 0; i < networkInterfaces.size; i++)
                        {
                            NetworkInterface networkInterface = networkInterfaces[i];
                            if (networkInterface != null)
                            {
                                IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
                                if (iPProperties != null)
                                {
                                    UnicastIPAddressInformationCollection unicastAddresses = iPProperties.UnicastAddresses;
                                    foreach (UnicastIPAddressInformation item in unicastAddresses)
                                    {
                                        if (IsValidAddress(item.Address))
                                        {
                                            mAddresses.Add(item.Address);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    IPAddress[] hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
                    IPAddress[] array = hostAddresses;
                    foreach (IPAddress iPAddress in array)
                    {
                        if (IsValidAddress(iPAddress) && !mAddresses.Contains(iPAddress))
                        {
                            mAddresses.Add(iPAddress);
                        }
                    }
                    if (mAddresses.size == 0)
                    {
                        mAddresses.Add(IPAddress.Loopback);
                    }
                }
                return mAddresses;
            }
        }

        public static IPAddress localAddress
        {
            get
            {
                if (mLocalAddress == null)
                {
                    mLocalAddress = IPAddress.None;
                    List<IPAddress> localAddresses = Tools.localAddresses;
                    if (localAddresses.size > 0)
                    {
                        mLocalAddress = mAddresses[0];
                        for (int i = 0; i < mAddresses.size; i++)
                        {
                            IPAddress iPAddress = mAddresses[i];
                            string text = iPAddress.ToString();
                            if (!text.StartsWith("25."))
                            {
                                mLocalAddress = iPAddress;
                                break;
                            }
                        }
                    }
                }
                return mLocalAddress;
            }
            set
            {
                mLocalAddress = value;
                if (value != null)
                {
                    List<IPAddress> localAddresses = Tools.localAddresses;
                    for (int i = 0; i < localAddresses.size; i++)
                    {
                        if (localAddresses[i] == value)
                        {
                            return;
                        }
                    }
                }
                Console.WriteLine("[TNet] " + value + " is not one of the local IP addresses. Strange things may happen.");
            }
        }

        public static IPAddress externalAddress
        {
            get
            {
                if (mExternalAddress == null)
                {
                    mExternalAddress = GetExternalAddress();
                }
                return (mExternalAddress == null) ? localAddress : mExternalAddress;
            }
        }

        public static void ResolveIPs()
        {
            ResolveIPs(null);
        }

        public static void ResolveIPs(OnResolvedIPs del)
        {
            if (isExternalIPReliable)
            {
                del?.Invoke(localAddress, externalAddress);
                return;
            }
            if (mOnResolve == null)
            {
                mOnResolve = ResolveDummyFunc;
            }
            lock (mOnResolve)
            {
                if (del != null)
                {
                    mOnResolve = (OnResolvedIPs)Delegate.Combine(mOnResolve, del);
                }
                if (mResolveThread == null)
                {
                    mResolveThread = new Thread(ResolveThread);
                    mResolveThread.Start();
                }
            }
        }

        private static void ResolveDummyFunc(IPAddress a, IPAddress b)
        {
        }

        private static void ResolveThread()
        {
            IPAddress localAddress = Tools.localAddress;
            IPAddress externalAddress = Tools.externalAddress;
            lock (mOnResolve)
            {
                if (mOnResolve != null)
                {
                    mOnResolve(localAddress, externalAddress);
                }
                mResolveThread = null;
                mOnResolve = null;
            }
        }

        private static IPAddress GetExternalAddress()
        {
            if (mExternalAddress != null)
            {
                return mExternalAddress;
            }
            if (ResolveExternalIP(ipCheckerUrl))
            {
                return mExternalAddress;
            }
            if (ResolveExternalIP("http://icanhazip.com"))
            {
                return mExternalAddress;
            }
            if (ResolveExternalIP("http://bot.whatismyipaddress.com"))
            {
                return mExternalAddress;
            }
            if (ResolveExternalIP("http://ipinfo.io/ip"))
            {
                return mExternalAddress;
            }
            return localAddress;
        }

        private static bool ResolveExternalIP(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            try
            {
                WebClient webClient = new WebClient();
                string text = webClient.DownloadString(url).Trim();
                string[] array = text.Split(new char[1]
                {
                    ':'
                });
                if (array.Length >= 2)
                {
                    string[] array2 = array[1].Trim().Split(new char[1]
                    {
                        '<'
                    });
                    mExternalAddress = ResolveAddress(array2[0]);
                }
                else
                {
                    mExternalAddress = ResolveAddress(text);
                }
                if (mExternalAddress != null)
                {
                    isExternalIPReliable = true;
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool IsValidAddress(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                return false;
            }
            if (address.Equals(IPAddress.Loopback))
            {
                return false;
            }
            if (address.Equals(IPAddress.None))
            {
                return false;
            }
            if (address.Equals(IPAddress.Any))
            {
                return false;
            }
            if (address.ToString().StartsWith("169."))
            {
                return false;
            }
            return true;
        }

        public static IPAddress ResolveAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }
            if (address == "localhost")
            {
                return IPAddress.Loopback;
            }
            if (IPAddress.TryParse(address, out IPAddress address2))
            {
                return address2;
            }
            try
            {
                IPAddress[] hostAddresses = Dns.GetHostAddresses(address);
                for (int i = 0; i < hostAddresses.Length; i++)
                {
                    if (!IPAddress.IsLoopback(hostAddresses[i]))
                    {
                        return hostAddresses[i];
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static IPEndPoint ResolveEndPoint(string address, int port)
        {
            IPEndPoint iPEndPoint = ResolveEndPoint(address);
            if (iPEndPoint != null)
            {
                iPEndPoint.Port = port;
            }
            return iPEndPoint;
        }

        public static IPEndPoint ResolveEndPoint(string address)
        {
            int result = 0;
            string[] array = address.Split(new char[1]
            {
                ':'
            });
            if (array.Length > 1)
            {
                address = array[0];
                int.TryParse(array[1], out result);
            }
            IPAddress iPAddress = ResolveAddress(address);
            return (iPAddress == null) ? null : new IPEndPoint(iPAddress, result);
        }

        public static string GetSubnet(IPAddress ip)
        {
            if (ip == null)
            {
                return null;
            }
            string text = ip.ToString();
            int num = text.LastIndexOf('.');
            if (num == -1)
            {
                return null;
            }
            return text.Substring(0, num);
        }

        public static string GetResponse(WebRequest request)
        {
            //Discarded unreachable code: IL_006b
            string text = string.Empty;
            try
            {
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                byte[] array = new byte[2048];
                while (true)
                {
                    int num = responseStream.Read(array, 0, array.Length);
                    if (num <= 0)
                    {
                        break;
                    }
                    text += Encoding.ASCII.GetString(array, 0, num);
                }
                return text;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void Serialize(BinaryWriter writer, IPEndPoint ip)
        {
            byte[] addressBytes = ip.Address.GetAddressBytes();
            writer.Write((byte)addressBytes.Length);
            writer.Write(addressBytes);
            writer.Write((ushort)ip.Port);
        }

        public static void Serialize(BinaryReader reader, out IPEndPoint ip)
        {
            byte[] address = reader.ReadBytes(reader.ReadByte());
            int port = reader.ReadUInt16();
            ip = new IPEndPoint(new IPAddress(address), port);
        }

        public static string[] GetFiles(string directory)
        {
            //Discarded unreachable code: IL_001e
            try
            {
                if (!Directory.Exists(directory))
                {
                    return null;
                }
                return Directory.GetFiles(directory);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static bool WriteFile(string fileName, byte[] data)
        {
            //Discarded unreachable code: IL_0047
            if (data == null || data.Length == 0)
            {
                return DeleteFile(fileName);
            }
            try
            {
                string directoryName = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                File.WriteAllBytes(fileName, data);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static byte[] ReadFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    return File.ReadAllBytes(fileName);
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static bool DeleteFile(string fileName)
        {
            //Discarded unreachable code: IL_0018
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
