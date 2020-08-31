using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Remote.Abstractions;

namespace Assets.Scripts.Remote
{
    public static partial class ServerConnectionFactory
    {
        public static IServerConnection CreateServerConnection(
            string username,
            string password,
            string serverAddress,
            bool requiresAuthentication = true)
        {
            return new ServerConnection(username, password, serverAddress, requiresAuthentication);
        }
    }
}
