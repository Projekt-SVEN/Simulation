
using System;

namespace Assets.Scripts.Remote.Abstractions
{
    public abstract class RemoteObject
    {
        protected RemoteObject(IServerConnection remoteConnection, string deviceName)
            : this (remoteConnection, deviceName, deviceName)
        {
        }

        protected RemoteObject(IServerConnection remoteConnection, string getDeviceName, string setDeviceName)
        {
            GetDeviceName = getDeviceName;
            SetDeviceName = setDeviceName;
            RemoteConnection = remoteConnection;
        }

        public string GetDeviceName { get; }
        public string SetDeviceName { get; }

        protected IServerConnection RemoteConnection { get; }

        public string GetAttribute(string attribute) => RemoteConnection.ExecuteCommand(GetDeviceName, attribute, null, CommandList.Get);

        public void SetAttribute(string attribute, string value) => RemoteConnection.ExecuteCommand(SetDeviceName, attribute, value, CommandList.Get);
    }
}
