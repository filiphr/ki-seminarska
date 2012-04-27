using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IpCamera.Controler;
using System.Runtime.Serialization;

namespace IpCamera.Finder.Test
{
    [Serializable]
    public class Cameras
    {
        #region Parametars
        /// <summary>
        /// A list of all the cameras in the 
        /// </summary>
        public List<INetworkCamera> cameras { get; set; }
        #endregion

        #region Construcors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Cameras()
        {
            this.cameras = new List<INetworkCamera>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cameras">List of cameras on the network</param>
        public Cameras(List<INetworkCamera> cameras)
        {
            this.cameras = cameras;
        }

                /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">Serialization Info</param>
        /// <param name="ctxt">Streaming Context</param>
        public Cameras(SerializationInfo info, StreamingContext ctxt)
        {
            this.cameras = (List<INetworkCamera>)info.GetValue("cameras", typeof(List<INetworkCamera>));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks ig this camera exists in the list of cameras
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <returns>True if the camera is in the list, false if it isn't</returns>
        public bool ExistsInCameras(INetworkCamera camera)
        {
            foreach (INetworkCamera cam in cameras)
            {
                if (cam.ID == camera.ID)
                {
                    return true;
                }
            }
            return false;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("cameras", this.cameras);
        }
        #endregion
    }
}
