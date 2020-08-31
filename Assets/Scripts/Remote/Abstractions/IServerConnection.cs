using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote.Abstractions
{

    public enum CommandList
    {
        Get,
        Set,
        List
    };

    public interface IServerConnection
    {
        string ExecuteCommand(string device, string attribute, string value, CommandList command);
    }
}
