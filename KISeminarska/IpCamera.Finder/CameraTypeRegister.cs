using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IpCamera.Controler.Cameras;

namespace IpCamera.Controler
{
    public static class CameraTypeRegister
    {
        public static List<INetworkCamera> RegisteredCameraModels = new List<INetworkCamera>();

        static CameraTypeRegister()
        {
            // For start we will have only one camera model, 
            // but later we might have more
            RegisteredCameraModels.Add(new EdimaxIC3010());
        }
    }
}
