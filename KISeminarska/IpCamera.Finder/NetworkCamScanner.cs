using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IpCamera.Controler;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Text.RegularExpressions;


namespace IpCamera.Finder
{
    /// <summary>
    /// Scans the network for cameras
    /// </summary>
    public class NetworkCamScanner
    {
        private const string IPAddressRegex = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

        /// <summary>
        /// Removes the last array from the IP address
        /// Transforms it from XXX.XXX.XXX.XXX to XXX.XXX.XXX.
        /// Returns string.Empty if the IP address is not in the required form.
        /// </summary>
        /// <param name="IP">IP address</param>
        /// <returns></returns>
        public static string SplitIP(string IP)
        {
            #region Input Validation for the public methods
            if (string.IsNullOrWhiteSpace(IP))
            {
                throw new ArgumentNullException("IP");
            }
            #endregion

            string splitIp = string.Empty;

            // Check with regular expression if the address is correct IP address
            Regex ipAddressRegex = new Regex(IPAddressRegex);
            if (ipAddressRegex.Match(IP).Success)
            {
                string[] IParray = IP.Split('.');
                splitIp = string.Join(".", IParray, 0, IParray.Length - 1); // join all except the last one.
                splitIp += ".";
            }

            return splitIp;
        }

        /// <summary>
        /// Returns a list of the active IP addresses on the network
        /// </summary>
        /// <returns></returns>
        public static List<IPAddress> GetActiveIPs()
        {
            List<IPAddress> list = new List<IPAddress>();
            // Get the local DNS HostName
            string localHostName = Dns.GetHostName();
            // Number of ports that should be scanned
            int numberOfPorts = 255;
            // Get the IP addresses of the host
            IPAddress[] addresses = Dns.GetHostAddresses(localHostName);
            foreach (IPAddress address in addresses)
            {
                int scannedPorts = 0;
                string splitIP = SplitIP(address.ToString());
                if (!string.IsNullOrWhiteSpace(splitIP))
                {
                    // describe the ping action
                    Action<int> pingAction = delegate(int port)
                    {
                        IPAddress checkIP = IPAddress.Parse(splitIP + port);
                        try
                        {
                            Ping ping = new Ping();
                            PingReply reply = ping.Send(checkIP, 50);
                            if (reply.Status == IPStatus.Success)
                            {
                                list.Add(checkIP);
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            scannedPorts++;
                        }
                    };

                    // Loop through all the ports and do an asynchronious ping
                    for (int i = 0; i <= numberOfPorts; i++)
                    {
                        Action<int> asyncPing = AsyncActions.MakeAsync(pingAction, null, null);
                        asyncPing(i);
                    }
                    // TODO, marjanp: Timeout?
                    while (scannedPorts <= numberOfPorts)
                    {
                        // Do nothing, the wait will be reset by the last ping action
                        // Maybe a timeout would be good not to wait forever
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// This method will scan the ports for network cameras.
        /// If new cameras are found then they will be added to an existing list..
        /// </summary>
        public static List<INetworkCamera> Scan(List<IPAddress> list)
        {
            List<INetworkCamera> connectedCameras = new List<INetworkCamera>();
            // All the IP Camera models that we can control are registered in CameraTypeRegister,
            // So get the cameras from there
            foreach (INetworkCamera registeredCameraModel in CameraTypeRegister.RegisteredCameraModels)
            {
                foreach (IPAddress address in list)
                {
                    if (registeredCameraModel.CameraExistOnPort(address))
                    {
                        connectedCameras.Add(registeredCameraModel.CreateInstance(address.ToString()));
                    }
                }
            }
            return connectedCameras;
        }
    }
}
