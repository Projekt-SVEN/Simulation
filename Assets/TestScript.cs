using Assets.Scripts.Remote;
using Assets.Scripts.Remote.Abstractions;
using System;
using UnityEngine;

public class TestScript : MonoBehaviour
{

    [SerializeField]
    private string Username;

    [SerializeField]
    private string Password;

    [SerializeField]
    private string ServerAddress;

    [SerializeField]
    private bool RequiresAuthentication;

    [SerializeField]
    private string Device;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Username: {Username}");
        Debug.Log($"Password: {Password}");
        Debug.Log($"ServerAddress: {ServerAddress}");
        Debug.Log($"RequiresAuthentication: {RequiresAuthentication}");

        Debug.Log("Tests erfolgen");

        IServerConnection connection = ServerConnectionFactory.CreateServerConnection(Username, Password, ServerAddress,
                RequiresAuthentication);

        LsfInfoSchnittstelle lsfSchnittstelle = new LsfInfoSchnittstelle(connection, Device);

        bool lecture;
        bool isbreak;
        DateTime? nextbreak;

        lsfSchnittstelle.GetStates(out lecture, out isbreak, out nextbreak);

        Debug.Log(lecture);
        Debug.Log(isbreak);
        if (nextbreak != null)
        {
            Debug.Log(nextbreak.ToString());
        }
        else
        {
            Debug.Log("Time: null");
        }
    }
}
