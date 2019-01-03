using System.Xml.Serialization;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Net;

public class NetworkManager : MonoBehaviour {

    public string serverIP = "127.0.0.1";
    public int serverPort = 12345;
    private TcpClient clientSocket { get; } = new TcpClient();
    private NetworkStream socketStream;
    private StreamReader socketReader;
    private StreamWriter socketWriter;

    public int Available = 0;

    private Player player;

    private void Start()
    {
        player = gameObject.GetComponent<Player>();

        try
        {
            clientSocket.Connect(IPAddress.Parse(serverIP), serverPort);
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

        socketStream = clientSocket.GetStream();
        socketReader = new StreamReader(socketStream);
        socketWriter = new StreamWriter(socketStream)
        {
            AutoFlush = true
        };
    }

    private void Update()
    {
        Available = clientSocket.Available;

        if (clientSocket.Available != 0 && !player.enabled)
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
            clientSocket.Close();
        }
        catch
        {

        }
    }

    public void WriteLine(string message)
    {
        socketWriter.WriteLine(message);
    }

    public string ReadLine()
    {
        string data = socketReader.ReadLine();
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
        ReadLine(); //There is always a spare newline after my way of serialization

        StringReader xml = new StringReader(new string(xmlPlaces));
        return (T)serializer.Deserialize(xml);
    }
}
