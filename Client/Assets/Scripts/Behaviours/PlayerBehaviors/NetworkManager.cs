using System.Xml.Serialization;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Net;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    private TcpClient ClientSocket { get; } = new TcpClient();
    private StreamReader socketReader;
    private StreamWriter socketWriter;

    public int Available
    {
        get
        {
            return ClientSocket.Available;
        }
    }

    /// <summary>
    /// Runs every tick, waits for server to sign that it's ready for game start.
    /// </summary>
    private void Update()
    {
        if (ClientSocket.Connected && ClientSocket.Available != 0 && !GetComponent<Player>().enabled)
        {
            if (ReadLine() == "Game Start")
            {
                GameObject.Find("Wait Message").SetActive(false);
                GetComponent<Player>().enabled = true;
            }
        }
    }

    /// <summary>
    /// Runs right before the application closes.
    /// Closes the connection with the server.
    /// </summary>
    void OnApplicationQuit()
    {
        try
        {
            ClientSocket.Close();
        }
        catch
        {

        }
    }

    /// <summary>
    /// Connects to the server using parameters on a UI.
    /// Called by UI elements.
    /// </summary>
    public void Connect()
    {
        Transform canvas = GameObject.Find("Canvas/Connection").transform;
        string ipString = canvas.Find("IP").GetComponent<TMP_InputField>().text;
        if (!IPAddress.TryParse(ipString, out IPAddress ip))
        {
            canvas.Find("Errors").GetComponent<TextMeshProUGUI>().text = "IP address invalid.";
            print(ipString);
            return;
        }
        string portString = canvas.Find("Port").GetComponent<TMP_InputField>().text;
        if (!int.TryParse(portString, out int port))
        {
            canvas.Find("Errors").GetComponent<TextMeshProUGUI>().text = "Port must be a number.";
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
        }
        catch
        {
            canvas.Find("Errors").GetComponent<TextMeshProUGUI>().text = "Could not find server at " + ip + ":" + port + System.Environment.NewLine + "Try another address";
            return;
        }

        canvas.gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("Game").gameObject.SetActive(true);
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
        //print(data);
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
        socketReader.Read(xmlString, 0, len);
        socketWriter.WriteLine("V");

        StringReader xml = new StringReader(new string(xmlString));
        return (T)serializer.Deserialize(xml);
    }
}
