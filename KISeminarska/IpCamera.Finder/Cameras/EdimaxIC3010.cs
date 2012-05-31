using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using IpCamera.Finder;

namespace IpCamera.Controler.Cameras
{
    [Serializable]
    public class EdimaxIC3010 : INetworkCamera
    {
        #region CGI querries

        /// <summary>
        /// CGI command for getting the MAC Address od the IP Camera
        /// </summary>
        private const string MACAddressURL_IP0 = "http://{0}/camera-cgi/admin/param.cgi?action=list&group= Network.Interface.I0.Active.MACAddress";

        /// <summary>
        /// CGI command for taking image
        /// </summary>
        private const string ImageURL_IP0 = "http://{0}/jpg/image.jpg";

        /// <summary>
        /// CGI command for getting the brand od the IP Camera
        /// </summary>
        private const string BrandURL_IP0 = "http://{0}/camera-cgi/admin/param.cgi?action=list&group=Brand";

        /// <summary>
        /// CGI command for getting the JPEG image resolution
        /// </summary>
        private const string CameraJpegConfig_IP0 = "http://{0}/camera-cgi/admin/param.cgi?action=list&group= Image.I0.Appearance";

        #endregion CGI querries

        #region Parametars

        /// <summary>
        /// The camera ID, usualy the MAC address
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The camera IP address
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// The camera model
        /// </summary>
        public string model { get; set; }

        /// <summary>
        /// Camera name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The camera status
        /// </summary>
        public bool connected { get; set; }

        /// <summary>
        /// Camera description
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Image width
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// Image location - X coord
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Image location - Y coord
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Camera network credentials
        /// </summary>
        public NetworkCredential credentials { get; set; }

        #endregion Parametars

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        internal EdimaxIC3010() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ID">The camera MAC address</param>
        /// <param name="IP">The camera IP address</param>
        /// <param name="IP">Whether the camera is connected</param>
        internal EdimaxIC3010(string newID, string newIP, bool newConnected)
        {
            this.ID = newID;
            this.IP = newIP;
            this.name = string.Empty;
            this.model = string.Empty;
            this.connected = newConnected;
            this.description = string.Empty;
            this.credentials = new NetworkCredential("admin", "1234");
            this.X = 0;
            this.Y = 0;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">Serialization Info</param>
        /// <param name="ctxt">Streaming Context</param>
        public EdimaxIC3010(SerializationInfo info, StreamingContext ctxt)
        {
            this.ID = (string)info.GetValue("ID", typeof(string));
            this.IP = (string)info.GetValue("IP", typeof(string));
            this.model = (string)info.GetValue("model", typeof(string));
            this.name = (string)info.GetValue("name", typeof(string));
            this.connected = (bool)info.GetValue("connected", typeof(bool));
            this.description = (string)info.GetValue("description", typeof(string));
            this.width = (int)info.GetValue("width", typeof(int));
            this.height = (int)info.GetValue("height", typeof(int));
            this.credentials = new NetworkCredential();
            this.credentials.UserName = (string)info.GetValue("username", typeof(string));
            this.credentials.Password = (string)info.GetValue("password", typeof(string));
            this.X = (double)info.GetValue("X", typeof(double));
            this.Y = (double)info.GetValue("Y", typeof(double));
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Takes image from the IP Camera and converts it into Bitmap
        /// </summary>
        /// <returns></returns>
        public Bitmap TakePicture()
        {
            //Rechecks the IP
            GetCurrentIP();
            if (this.connected)
            {
                //sourceURL is the URL with which we will get the image from the camera
                string sourceURL = string.Format(ImageURL_IP0, IP);
                byte[] buffer = new byte[100000];
                int read, total = 0;

                // create HTTP request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sourceURL);
                request.Credentials = this.credentials;

                // get response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // get response stream
                using (Stream stream = response.GetResponseStream())
                {
                    // read data from stream
                    StreamReader reader = new StreamReader(stream);
                    while ((read = stream.Read(buffer, total, 1000)) != 0)
                    {
                        total += read;
                    }
                    return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new MemoryStream(buffer, 0, total));
                }
            }
            else return null;
        }

        /// <summary>
        /// If the cameras IP is changed, it will change it to the new IP
        /// </summary>
        public void GetCurrentIP()
        {
            if (!CameraHasSameIP())
            {
                this.IP = string.Empty;
                List<IPAddress> list = NetworkCamScanner.GetActiveIPs();
                foreach (IPAddress address in list)
                {
                    if (CameraIsOnThisPort(address))
                    {
                        this.IP = address.ToString();
                    }
                }
                if (string.IsNullOrWhiteSpace(this.IP))
                {
                    this.connected = false;
                }
            }
        }

        /// <summary>
        /// Checks if this is the camera's new IP
        /// </summary>
        /// <param name="address">IP to be tested</param>
        /// <returns>Returns true if this is the camera's new IP</returns>
        public bool CameraIsOnThisPort(IPAddress address)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(MACAddressURL_IP0, address.ToString()));
            request.Credentials = this.credentials;
            request.Timeout = 1000;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        String tmp = reader.ReadLine();
                        String newID = tmp.Split('=')[1];
                        return newID == this.ID;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the camera still has the same IP address
        /// </summary>
        /// <returns>Returns true if the camera still has the same IP address</returns>
        public bool CameraHasSameIP()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(MACAddressURL_IP0, IP));
            request.Credentials = this.credentials;
            request.Timeout = 3000;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        String tmp = reader.ReadLine();
                        String newID = tmp.Split('=')[1];
                        return newID == this.ID;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an EDIMAX camera exists on the given IP address
        /// </summary>
        /// <param name="ipAddress">IP Address</param>
        /// <returns>Returns true if a camera exists</returns>
        public bool CameraExistOnPort(IPAddress ipAddress)
        {
            string testURL = string.Format(BrandURL_IP0, ipAddress.ToString());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(testURL);
            request.Credentials = new NetworkCredential("admin", "1234");
            request.Timeout = 1500;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        String read = reader.ReadLine();
                        String[] tmp = read.Split('=');
                        //TODO: change this to the camera model so it's more specific
                        return tmp[1] == "EDIMAX";
                    }
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates instance if INetworkCamera with IP=newIP
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public INetworkCamera CreateInstance(string newIP)
        {
            string newID = string.Empty;
            string newModel = string.Empty;
            HttpWebRequest requestID = (HttpWebRequest)WebRequest.Create(string.Format(MACAddressURL_IP0, newIP));
            requestID.Credentials = new NetworkCredential("admin", "1234");
            requestID.Timeout = 1500;
            HttpWebResponse responseID = (HttpWebResponse)requestID.GetResponse();
            using (Stream stream = responseID.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream);
                String tmp = reader.ReadLine();
                newID = tmp.Split('=')[1];
            }
            return new EdimaxIC3010(newID, newIP, true);
        }

        /// <summary>
        /// Sets the camera's model
        /// </summary>
        public void setModel()
        {
            string testURL = string.Format(BrandURL_IP0, IP);
            HttpWebRequest requestModel = (HttpWebRequest)WebRequest.Create(testURL);
            requestModel.Credentials = new NetworkCredential("admin", "1234");
            requestModel.Timeout = 1500;
            HttpWebResponse responseModel = (HttpWebResponse)requestModel.GetResponse();
            using (Stream stream = responseModel.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream);
                reader.ReadLine();
                String tmp = reader.ReadLine();
                this.model = tmp.Split('=')[1];
            }
        }

        /// <summary>
        /// Set the image size
        /// </summary>
        public void setImageSize()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CameraJpegConfig_IP0, this.IP));
            request.Credentials = this.credentials;
            request.Timeout = 1500;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream);
                string tmp = reader.ReadLine();
                tmp = tmp.Split('=')[1];
                this.width = Convert.ToInt16(tmp.Split('x')[0]);
                this.height = Convert.ToInt16(tmp.Split('x')[1]);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("ID", this.ID);
            info.AddValue("IP", this.IP);
            info.AddValue("model", this.model);
            info.AddValue("name", this.name);
            info.AddValue("connected", this.connected);
            info.AddValue("description", this.description);
            info.AddValue("width", this.width);
            info.AddValue("height", this.height);
            info.AddValue("username", this.credentials.UserName);
            info.AddValue("password", this.credentials.Password);
            info.AddValue("X", this.X);
            info.AddValue("Y", this.Y);
        }

        #endregion Methods
    }
}