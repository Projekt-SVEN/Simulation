using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Assets.Scripts;
using Assets.Scripts.Remote.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Remote
{
    public class Test_Server : MonoBehaviour
    {
        [SerializeField]
        private string Username;

        [SerializeField]
        private string Password;

        [SerializeField]
        private string ServerAddress;

        [SerializeField]
        private bool RequiresAuthentication;

        // Start is called before the first frame update
        void Start()
        {
            Username = OptionsManager.Username;
            Password = OptionsManager.Password;
            ServerAddress = OptionsManager.ServerAddress;
            RequiresAuthentication = OptionsManager.RequiresAuthentication;

            Debug.Log($"Username: {Username}");
            Debug.Log($"Password: {Password}");
            Debug.Log($"ServerAddress: {ServerAddress}");
            Debug.Log($"RequiresAuthentication: {RequiresAuthentication}");

            Debug.Log("Tests erfolgen");

            IServerConnection connection = ServerConnectionFactory.CreateServerConnection(Username, Password, ServerAddress,
                    RequiresAuthentication);

            RemoteHeater remoteHeater = new RemoteHeater(connection, "Heizung");

            try
            {
                //Heizung sollt auf 0 sein (wenn richtig eingestellt)
                float start = remoteHeater.GetState();
                Debug.Log("Heizung: " + start);

                remoteHeater.SetState(30);
                float value = remoteHeater.GetState();
                Debug.Log("Heizung: " + value);

                remoteHeater.SetAttribute("temperatur", start.ToString(CultureInfo.InvariantCulture));
                value = float.Parse(remoteHeater.GetAttribute("temperatur"));
                Debug.Log("Heizung: " + value);

                Debug.Log ("Setting Data succeeded.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Setting Data failed.");
                Debug.LogError(exception);
            }
        }
    }
}