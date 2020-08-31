using System;
using System.IO;
using System.Net;
using Assets.Scripts.Remote.Abstractions;
using UnityEngine;
using Assets.Scripts.Simulation.Abstractions;

namespace Assets.Scripts.Remote
{
    public static partial class ServerConnectionFactory
    {
        private class ServerConnection : IServerConnection
        {
            private static readonly char[] ResponseStringSeparators = new char[] { '<', '>', ' ', '"' };

            /*
             * Credentials are used for Test-Environment at Fitz's Home
             *
             * TODO: move credentials to dynamically loaded Source like environment variables.
             */

            /// <summary>
            /// Username used for login at the FHEM-server.
            /// </summary>
            private string _username;

            /// <summary>
            /// Password used for login at the FHEM-server.
            /// </summary>
            private string _password;

            /// <summary>
            /// <see langword="true"/> if the Server requires authentication; otherwise <see langword="false"/>.
            /// </summary>
            private bool _requiersAuthentifiction;

            /// <summary>
            /// Cross-site request forgery token.
            /// </summary>
            /// <remarks>
            /// Used to prevent server-side cross-site scripting (XSS) attacks.
            /// </remarks>
            private string _csrfToken = null;

            /// <summary>
            /// IP address of the FHEM server.
            /// </summary>
            private string _serverAddress;

            public ServerConnection(
                string username,
                string password,
                string serverAddress,
                bool requiresAuthentication)
            {
                _username = username;
                _password = password;
                _serverAddress = serverAddress;
                _requiersAuthentifiction = requiresAuthentication;

                _csrfToken = GetCsrfToken();
            }

            private string GetCsrfToken()
            {
                string csrfToken = null;

                //BUG: Possible Injection-Attack
                Uri uri = new Uri($"http://{_serverAddress}/fhem");

                //Debug.Log(uri.OriginalString);

                WebRequest request = WebRequest.CreateHttp(uri);

                if (_requiersAuthentifiction)
                {
                    //Anmeldung bei dem Server
                    NetworkCredential myNetworkCredential = new NetworkCredential(_username, _password);

                    CredentialCache myCredentialCache = new CredentialCache();
                    myCredentialCache.Add(uri, "Basic", myNetworkCredential);

                    request.PreAuthenticate = true;
                    request.Credentials = myCredentialCache;
                }
                else
                {
                    request.Credentials = CredentialCache.DefaultCredentials;
                }

                try
                {
                    WebResponse response = request.GetResponse();

                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream);

                        while (!reader.EndOfStream)
                        {
                            string responseString = reader.ReadLine();
                            string[] tags = responseString.Split(ResponseStringSeparators);

                            for (int i = 0; i < tags.Length; i++)
                            {
                                if (tags[i].Equals("fwcsrf"))
                                {
                                    csrfToken = tags[i + 3];
                                    goto Found;
                                }
                            }
                        }

                    Found:
                        reader.Close();
                        responseStream.Close();
                    }

                    response.Close();
                }
                catch (WebException webException)
                {
                    Debug.LogWarning(webException);

                    throw new Exception("Failed to retrieve the CSRF-Token.", webException);
                }

                //Debug.Log(csrfToken);

                return csrfToken ?? throw new Exception("CSRF-Token not found in server response.");
            }

            public string ExecuteCommand(string device, string attribute, string value, CommandList command)
            {

                if (_csrfToken == null)
                {
                    _csrfToken = GetCsrfToken();
                }

                Uri uri = null;

                //BUG: Possible Injection-Attack
                if (command == CommandList.Get)
                {
                    uri = new Uri($"http://{_serverAddress}/fhem?cmd=get%20{device}%20{attribute}%20{value}&fwcsrf={_csrfToken}&XHR=1");
                }
                else if (command == CommandList.Set)
                {
                    uri = new Uri($"http://{_serverAddress}/fhem?cmd=set%20{device}%20{attribute}%20{value}&fwcsrf={_csrfToken}&XHR=1");
                }
                else if (command == CommandList.List)
                {
                    uri = new Uri($"http://{_serverAddress}/fhem?cmd=list%20{device}&fwcsrf={_csrfToken}&XHR=1");
                }

                if (uri != null)
                {
#if DebugURI
                    Debug.Log(uri.OriginalString);
#endif
                    WebRequest request = WebRequest.CreateHttp(uri);

                    if (_requiersAuthentifiction)
                    {
                        //Anmeldung bei dem Server
                        NetworkCredential myNetworkCredential = new NetworkCredential(_username, _password);

                        CredentialCache myCredentialCache = new CredentialCache();
                        myCredentialCache.Add(uri, "Basic", myNetworkCredential);

                        request.PreAuthenticate = true;
                        request.Credentials = myCredentialCache;
                    }
                    else
                    {
                        request.Credentials = CredentialCache.DefaultCredentials;
                    }

                    try
                    {
                        using (WebResponse response = request.GetResponse())
                        {
                            using (Stream responseStream = response.GetResponseStream())
                            {
                                using (StreamReader reader = new StreamReader(responseStream))
                                {
                                    return reader.ReadToEnd();
                                }
                            }
                        }
                    }
                    catch (WebException webException)
                    {
                        Debug.Log(webException);

                        _csrfToken = null;

                        throw new Exception("Error occured while requesting to set data.", webException);
                    }
                }
                else
                {
                    throw new Exception("Command not found: " + command);
                }
            }
        }
    }
}
