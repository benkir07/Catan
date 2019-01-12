using System.Xml.Serialization;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Net;

public class NetworkManager : MonoBehaviour {

    public string serverIP = "127.0.0.1";
    public int serverPort = 12345;
    private TcpClient ClientSocket { get; } = new TcpClient();
    private NetworkStream socketStream;
    private StreamReader socketReader;
    private StreamWriter socketWriter;

    public int Available
    {
        get
        {
            return ClientSocket.Available;
        }
    }

    private Player player;

    /// <summary>
    /// Runs as the program starts, connects to the server.
    /// </summary>
    private void Start()
    {
        player = GetComponent<Player>();

        try
        {
            ClientSocket.Connect(IPAddress.Parse(serverIP), serverPort);
        }
        catch
        {
            Debug.Log("Could not find server at " + serverIP + ":" + serverPort);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
            return;
        }

        socketStream = ClientSocket.GetStream();
        socketReader = new StreamReader(socketStream);
        socketWriter = new StreamWriter(socketStream)
        {
            AutoFlush = true
        };
    }

    /// <summary>
    /// Runs every tick, waits for server to say ready for game start
    /// </summary>
    private void Update()
    {
        if (ClientSocket.Available != 0 && !player.enabled)
        {
            if (ReadLine() == "Game Start")
            {
                GameObject.Find("Wait Message").SetActive(false);
                player.enabled = true;
            }
        }
    }

    /// <summary>
    /// Runs as the application closes
    /// Closes the connection with the server
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
    /// Writes a message to the server
    /// </summary>
    /// <param name="message">The message to write</param>
    public void WriteLine(string message)
    {
        socketWriter.WriteLine(message);
    }

    /// <summary>
    /// Reads a message from the server
    /// </summary>
    /// <returns>The message</returns>
    public string ReadLine()
    {
        string data = socketReader.ReadLine();
        socketWriter.WriteLine("V");
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
        char[] xmlPlaces = new char[len];
        socketReader.Read(xmlPlaces, 0, len);
        socketReader.ReadLine(); //There is always a spare newline after my way of serialization
        socketWriter.WriteLine("V");

        StringReader xml = new StringReader(new string(xmlPlaces));
        return (T)serializer.Deserialize(xml);
    }
}
