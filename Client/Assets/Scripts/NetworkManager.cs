using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Net;

public class NetworkManager : MonoBehaviour {

    public string serverIP = "127.0.0.1";
    public int serverPort = 12345;
    public TcpClient clientSocket { get; } = new TcpClient();
    public NetworkStream socketStream { get; private set; }
    public StreamReader socketReader { get; private set; }
    public StreamWriter socketWriter { get; private set; }

    private GameManager gameManager;

    private void Start()
    {
        gameManager = gameObject.GetComponent<GameManager>();

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
        if (clientSocket.Available != 0 && !gameManager.enabled)
        {
            if (socketReader.ReadLine() == "Game Start")
            {
                GameObject.Find("Wait Message").SetActive(false);
                gameManager.enabled = true;
            }
        }
    }
}
