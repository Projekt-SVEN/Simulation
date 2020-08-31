using Assets.Scripts.Remote.Abstractions;
using System;

namespace Assets.Scripts.Remote
{
    public class LsfInfoSchnittstelle : RemoteObject
    {
        private const string IS_PAUSE_VALUE_NAME = "isPause";
        private const string IS_LECTURE_VALUE_NAME = "isVorlesung";
        private const string NEXT_LECTURE_VALUE_NAME = "nachstePause";
        private const string LAST_ATTRIBUTE_VALUE_NAME = "Attributes:";

        private static readonly char[] Separators = new char[]
        {
                ' ',
                '\n',
                '\r'
        };

        public LsfInfoSchnittstelle(IServerConnection remoteConnection, string deviceName)
            : base(remoteConnection, deviceName)
        {
        }

        
        public void GetStates(out bool lecture, out bool isBreak, out DateTime? nextBreak)
        {
            string data = RemoteConnection.ExecuteCommand(GetDeviceName, null, null, CommandList.List);

            lecture = false;
            isBreak = false;
            nextBreak = null;

            //Daten zerlegen und leer String entfernen.
            string[] dataSplit = data.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

            for(int i = 0; i < dataSplit.Length; i++)
            {
                if (IS_PAUSE_VALUE_NAME.Equals(dataSplit[i]))
                {
                    isBreak = !dataSplit[i + 1].Equals("0");
                }
                else if (IS_LECTURE_VALUE_NAME.Equals(dataSplit[i]))
                {
                    lecture = !dataSplit[i + 1].Equals("0");
                }
                else if (NEXT_LECTURE_VALUE_NAME.Equals(dataSplit[i]))
                {
                    //letztes Attribut ausgelesen Suche beenden
                    if(dataSplit[i+1].Equals(LAST_ATTRIBUTE_VALUE_NAME))
                    {
                        nextBreak = null;
                    }
                    else
                    {
                        nextBreak = DateTime.Parse(dataSplit[i + 1]);
                    }

                    break;
                }
            }
        }
    }
}