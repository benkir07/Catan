using System.Xml.Serialization;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Net;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    private TcpClient ClientSocket = new TcpClient();
    private StreamReader socketReader;
    private StreamWriter socketWriter;
    public LobbyManager LobbyManager;

    public int Available
    {
        get
        {
            return ClientSocket.Available;
        }
    }

    /// <summary>
    /// Connects to the server using parameters on the UI.
    /// Called by UI elements.
    /// </summary>
    public void Connect()
    {
        Transform UI = GameObject.Find("Menu Canvas/Connection").transform;
        string name = UI.Find("Name").GetComponent<TMP_InputField>().text;
        if (name.Length == 0)
        {
            UI.Find("Errors").GetComponent<TextMeshProUGUI>().text = "Choose a username.";
            return;
        }
        string ipString = UI.Find("IP").GetComponent<TMP_InputField>().text;
        if (!IPAddress.TryParse(ipString, out IPAddress ip))
        {
            UI.Find("Errors").GetComponent<TextMeshProUGUI>().text = "IP address invalid.";
            print(ipString);
            return;
        }
        string portString = UI.Find("Port").GetComponent<TMP_InputField>().text;
        if (!int.TryParse(portString, out int port))
        {
            UI.Find("Errors").GetComponent<TextMeshProUGUI>().text = "Port must be a number.";
            print(portString);
            return;
        }
        try
        {
            ClientSocket.Connect(ip, port);

            socketReader = new StreamReader(ClientSocket.GetStream());
            socketWriter = new StreamWriter(ClientSocket.GetStream())
            {
                AutoFlush = true
            };

            WriteLine(name);

            string ans = ReadLine();
            if (ans != "")
            {
                UI.Find("Errors").GetComponent<TextMeshProUGUI>().text = ans;
                ClientSocket.Close();
                ClientSocket = new TcpClient();
                return;
            }

            UI.gameObject.SetActive(false);
            LobbyManager.gameObject.SetActive(true);
            LobbyManager.YourName = name;
            LobbyManager.network = this;
        }
        catch
        {
            UI.Find("Errors").GetComponent<TextMeshProUGUI>().text = "Could not find server at " + ip + ":" + port + System.Environment.NewLine + "Try another address";
            return;
        }
    }

    /// <summary>
    /// Writes a message to the server.
    /// </summary>
    /// <param name="message">The message to write</param>
    public void WriteLine(string message)
    {
        socketWriter.WriteLine(message);
    }

    /// <summary>
    /// Reads a message from the server.
    /// </summary>
    /// <returns>The message sent from the server</returns>
    public string ReadLine()
    {
        string data = socketReader.ReadLine();
        socketWriter.WriteLine("Got it");
        return data;
    }

    /// <summary>
    /// Reads an xml object from the socket and deserializes it.
    /// </summary>
    /// <typeparam name="T">The type expected to be presented in the xml code</typeparam>
    /// <returns>The deserialized object</returns>
    public T Deserialize<T>()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));

        int len = int.Parse(ReadLine());

        char[] xmlString = new char[len];
        int read = 0;
        while (read < len)
        {
            read += socketReader.Read(xmlString, read, len - read);
        }
        socketWriter.WriteLine("V");

        StringReader xml = new StringReader(new string(xmlString));
        return (T)serializer.Deserialize(xml);
    }

    /// <summary>
    /// Runs right before the application closes.
    /// Closes the connection with the server.
    /// </summary>
    public void OnApplicationQuit()
    {
        try
        {
            ClientSocket.Close();
        }
        catch
        {

        }
    }

}
