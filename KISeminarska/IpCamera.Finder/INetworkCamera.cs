using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace IpCamera.Controler
{
    /// <summary>
    /// Defines the capabilities of the camera
    /// </summary>

    public interface INetworkCamera : ISerializable
    {
        #region Parametars
        /// <summary>
        /// The camera ID, usualy the MAC address
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// The camera IP address 
        /// </summary>
        string IP { get; set; }

        /// <summary>
        /// The camera model
        /// </summary>
        string model { get; set; }

        /// <summary>
        /// Camera name
        /// </summary>
        string name { get; set; }

        /// <summary>
        /// The camera status
        /// </summary>
        bool connected { get; set; }

        /// <summary>
        /// Camera description
        /// </summary>
        string description { get; set; }

        /// <summary>
        /// Image width
        /// </summary>
        int width { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        int height { get; set; }
        
        /// <summary>
        /// Camera network credentials
        /// </summary>
        NetworkCredential credentials { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a picture from the network camera
        /// </summary>
        /// <returns></returns>
        Bitmap TakePicture();

        /// <summary>
        /// Checks if the camera is on the same IP and changes it to the latest
        /// </summary>
        void GetCurrentIP();

        /// <summary>
        /// Checks if this is the camera's new IP
        /// </summary>
        /// <param name="address">IP to be tested</param>
        /// <returns>Returns true if this is the camera's new IP</returns>
        bool CameraIsOnThisPort(IPAddress address);

        /// <summary>
        /// Checks if the camera still has the same IP address
        /// </summary>
        /// <returns>Returns true if the camera still has the same IP address</returns>
        bool CameraHasSameIP();

        /// <summary>
        /// Returns true if a camera of this type exist on the specified IPAddress
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        bool CameraExistOnPort(IPAddress ipAddress);

        /// <summary>
        /// Creates an instance of this Type with the correct parametes
        /// </summary>
        /// <returns></returns>
        INetworkCamera CreateInstance(String IP);

        /// <summary>
        /// Set the camera model
        /// </summary>
        void setModel();

        /// <summary>
        /// Set the image size
        /// </summary>
        void setImageSize();
        #endregion
    }
}
