using System.Net;
using System.Net.Sockets;

namespace UniFan.Network
{
    public static class NetworkUtility
    {
        /// <summary>
        /// 解析ip或者域名
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static IPAddress ParseIpAddress(string ip)
        {
            try
            {
                IPAddress[] addressArray = Dns.GetHostAddresses(ip);
                IPAddress address = null;
                if ( addressArray.Length > 0)
                {
                    foreach (var addr in addressArray)
                    {
                        //优先使用ipv4
                        if (addr.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return addr;
                        }
                    }
                    //没有ipv4就用第一个
                    address = addressArray[0];
                }
                else
                {
                    address = IPAddress.Parse(ip);
                }
                return address;
            }
            catch
            {
                return null;
            }
        }
    }
}