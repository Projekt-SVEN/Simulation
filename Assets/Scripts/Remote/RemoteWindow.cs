using Assets.Scripts.Remote.Abstractions;
using System;

namespace Assets.Scripts.Remote
{
    public class RemoteWindow : RemoteObject
    {
        public RemoteWindow(IServerConnection remoteConnection, string deviceName)
            : this(remoteConnection, getDeviceName: deviceName, setDeviceName: deviceName)
        {
        }

        public RemoteWindow(IServerConnection remoteConnection, string getDeviceName, string setDeviceName)
           : base(remoteConnection, getDeviceName, setDeviceName)
        {
        }

        public bool GetState()
        {
            string serverResult = GetAttribute("status");

            if (serverResult.Equals("geschlossen\n"))
            {
                return false;
            }
            else if(serverResult.Equals("offen\n"))
            {
                return true;
            }

            throw new Exception("Retrieved invalid value from remote connection.");
        }

        public void SetState(bool value)
        {
            SetAttribute("Open/Close", value.ToString());
        }
    }
}
