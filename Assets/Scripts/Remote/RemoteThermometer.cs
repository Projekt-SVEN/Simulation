using Assets.Scripts.Remote.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    public class RemoteThermometer : RemoteObject
    {
        public RemoteThermometer(IServerConnection remoteConnection, string deviceName)
            : this(remoteConnection, getDeviceName: deviceName, setDeviceName: deviceName)
        {
        }

        public RemoteThermometer(IServerConnection remoteConnection, string getDeviceName, string setDeviceName)
           : base(remoteConnection, getDeviceName, setDeviceName)
        {
        }

        public float GetState()
        {
            string serverResult = GetAttribute("temp");

            if (float.TryParse(serverResult, out float value))
            {
                if (!float.IsNaN(value) && !float.IsInfinity(value))
                {
                    return value;
                }
            }

            throw new Exception("Retrieved invalid value from remote connection.");
        }

        public void SetState(float value)
        {
            SetAttribute("temperature", value.ToString());
        }
    }
}
