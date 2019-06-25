using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public string YourName;
    public NetworkManager network;
    public ScrollingArea GamesShown;
    public Vector3 lowestPos;
    private string SelectedLobby;
    public bool PasswordMenu;

    public bool _PasswordMenu
    {
        set
        {
            PasswordMenu = value;
        }
    }

    /// <summary>
    /// Initializes the needed variables.
    /// </summary>
    public void Start()
    {
        PasswordMenu = false;
        lowestPos = Prefabs.LobbyLabel.transform.localPosition + new Vector3(0, GamesShown.Distance, 0);
        transform.Find("Welcome").GetComponent<TextMeshProUGUI>().text = "Welcome," + Environment.NewLine + YourName;
    }

    /// <summary>
    /// Checks for updates from the server and updates accordingly.
    /// </summary>
    private void Update()
    {
        if (network.Available > 0)
        {
            Message message = (Message)Enum.Parse(typeof(Message), network.ReadLine());
            string name;
            string[] details;
            switch (message)
            {
                case Message.NewLobby:
                    details = network.ReadLine().Split('|');
                    AddLobby(details[0], details[1], details[2], details[3]);
                    break;

                case Message.AssignName:
                    string index = network.ReadLine();
                    name = network.ReadLine();
                    transform.Find("Lobby/Player" + index).GetComponent<TextMeshProUGUI>().text = name;
                    if (transform.Find("Lobby/Player0").GetComponent<TextMeshProUGUI>().text == this.YourName)
                    {
                        transform.Find("Lobby/Start").gameObject.SetActive(true);
                        if (transform.Find("Lobby/Player1").GetComponent<TextMeshProUGUI>().text == "")
                            transform.Find("Lobby/Start").GetComponent<Button>().interactable = false;
                        else
                            transform.Find("Lobby/Start").GetComponent<Button>().interactable = true;
                    }
                    else
                    {
                        transform.Find("Lobby/Start").gameObject.SetActive(false);
                    }
                    break;

                case Message.UpdateLobby:
                    name = network.ReadLine();
                    details = network.ReadLine().Split('|');

                    Transform newLabel = GamesShown.transform.Find(name);
                    newLabel.name = details[0];
                    newLabel.Find("Name").GetComponent<TextMeshProUGUI>().text = details[0];
                    newLabel.Find("Owner").GetComponent<TextMeshProUGUI>().text = details[1];
                    newLabel.Find("Players").GetComponent<TextMeshProUGUI>().text = details[2] + "/4";
                    newLabel.Find("Password").GetComponent<TextMeshProUGUI>().text = details[3];
                    break;

                case Message.RemoveLobby:
                    name = network.ReadLine();
                    DeleteLobby(name);
                    break;

                case Message.GameStart:
                    FindObjectOfType<GameManager>().StartGame();
                    GameObject.Find("Menu Canvas").SetActive(false);
                    break;
            }
        }
    }

    /// <summary>
    /// Selects a lobby.
    /// Called by UI elements.
    /// </summary>
    public void SelectLobby()
    {
        if (!PasswordMenu)
        {
            GameObject clicked = EventSystem.current.currentSelectedGameObject;
            foreach (Image item in GamesShown.GetComponentsInChildren<Image>())
            {
                if (item.gameObject != clicked)
                    item.enabled = false;
                else
                    item.enabled = true;
            }
            transform.Find("List/Join").GetComponent<Button>().interactable = true;
            SelectedLobby = clicked.name;
        }
    }

    /// <summary>
    /// Adds a lobby to the lobbies list.
    /// </summary>
    /// <param name="name">The lobby's name</param>
    /// <param name="owner">The player that ownes the lobby</param>
    /// <param name="playersAmount">The amount of players in the lobby</param>
    /// <param name="HasPassword">Whether the lobby has a password (Yes/No)</param>
    public void AddLobby(string name, string owner, string playersAmount, string HasPassword)
    {
        Transform newLabel = Instantiate(Prefabs.LobbyLabel, GamesShown.transform).transform;
        newLabel.name = name;
        newLabel.Find("Name").GetComponent<TextMeshProUGUI>().text = name;
        newLabel.Find("Owner").GetComponent<TextMeshProUGUI>().text = owner;
        newLabel.Find("Players").GetComponent<TextMeshProUGUI>().text = playersAmount + "/4";
        newLabel.Find("Password").GetComponent<TextMeshProUGUI>().text = HasPassword;
        newLabel.GetComponent<Button>().onClick.AddListener(SelectLobby);
        newLabel.localPosition = lowestPos - new Vector3(0, GamesShown.Distance, 0);

        GamesShown.Scroller.value = 0;
        GamesShown.Scroll(0);
        GamesShown.SpriteAmount++;
        lowestPos = newLabel.localPosition;
    }

    /// <summary>
    /// Deletes a lobby from the lobbies list.
    /// </summary>
    /// <param name="lobbyName">The lobby's names</param>
    public void DeleteLobby(string lobbyName)
    {
        GameObject toRemove = null;
        foreach (Button b in GamesShown.GetComponentsInChildren<Button>())
        {
            if (b.name == lobbyName)
                toRemove = b.gameObject;
        }

        if (toRemove != null)
        {
            foreach (Button b in GamesShown.GetComponentsInChildren<Button>())
            {
                if (b.transform.localPosition.y < toRemove.transform.localPosition.y)
                    b.transform.localPosition += new Vector3(0, GamesShown.Distance, 0);
            }
            Destroy(toRemove);
            GamesShown.SpriteAmount--;
            GamesShown.Scroller.value = 0;
            GamesShown.Scroll(0);
            lowestPos += new Vector3(0, GamesShown.Distance, 0);
        }
    }

    /// <summary>
    /// Opens the create lobby menu and resets its fields.
    /// </summary>
    public void OpenNewLobbyMenu()
    {
        if (!PasswordMenu)
        {
            transform.Find("New Menu").gameObject.SetActive(true);
            transform.Find("New Menu/Name").GetComponent<TMP_InputField>().text = YourName + "'s lobby";
            transform.Find("New Menu/Has Password").GetComponent<Toggle>().isOn = false;
            transform.Find("New Menu/Password").GetComponent<TMP_InputField>().text = "";
            transform.Find("New Menu/Error").GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    /// <summary>
    /// Tries to create a lobby.
    /// </summary>
    public void CreateLobby()
    {
        network.WriteLine(Message.NewLobby.ToString());
        network.WriteLine(transform.Find("New Menu/Name").GetComponent<TMP_InputField>().text);
        network.WriteLine(transform.Find("New Menu/Password").GetComponent<TMP_InputField>().text);

        string ans = network.ReadLine();
        if (ans == "")
        {
            transform.Find("List").gameObject.SetActive(false);
            transform.Find("New Menu").gameObject.SetActive(false);
            transform.Find("Lobby").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("New Menu/Error").GetComponent<TextMeshProUGUI>().text = ans;
        }
    }

    /// <summary>
    /// Tries to join a lobby.
    /// </summary>
    public void JoinLobby()
    {
        if (HasPassword(SelectedLobby) && !transform.Find("Password Menu").gameObject.activeInHierarchy)
        {
            transform.Find("Password Menu").gameObject.SetActive(true);
            transform.Find("Password Menu/Title").GetComponent<TextMeshProUGUI>().text = SelectedLobby;
            transform.Find("Password Menu/Password").GetComponent<TMP_InputField>().text = "";
            PasswordMenu = true;
        }
        else
        {
            network.WriteLine(Message.JoinLobby.ToString());
            network.WriteLine(SelectedLobby);

            if (HasPassword(SelectedLobby))
            {
                network.WriteLine(transform.Find("Password Menu/Password").GetComponent<TMP_InputField>().text);
                transform.Find("Password Menu").gameObject.SetActive(false);
                PasswordMenu = false;
            }

            string ans = network.ReadLine();
            if (ans == "")
            {
                transform.Find("List").gameObject.SetActive(false);
                transform.Find("Lobby").gameObject.SetActive(true);
                transform.Find("Lobby/Start").gameObject.SetActive(false);
            }
            else
            {
                transform.Find("List/Error").GetComponent<TextMeshProUGUI>().text = ans;
            }
        }
    }

    /// <summary>
    /// Leaves the current lobby.
    /// </summary>
    public void ExitLobby()
    {
        network.WriteLine(Message.ExitLobby.ToString());
        transform.Find("Lobby").gameObject.SetActive(false);
        transform.Find("List").gameObject.SetActive(true);
        transform.Find("List/Join").GetComponent<Button>().interactable = false;

        foreach (Transform item in GamesShown.GetComponentsInChildren<Transform>())
        {
            if (item != GamesShown.transform)
                Destroy(item.gameObject);
        }
        lowestPos = Prefabs.LobbyLabel.transform.localPosition + new Vector3(0, GamesShown.Distance, 0);
    }

    /// <summary>
    /// Checks if a cretain lobby is needed a password to join.
    /// </summary>
    /// <param name="lobbyName">The name of the lobby to check</param>
    /// <returns>true if needed password and false otherwise</returns>
    private bool HasPassword(string lobbyName)
    {
        return GamesShown.transform.Find(lobbyName + "/Password").GetComponent<TextMeshProUGUI>().text == "Yes";
    }

    public void Reload()
    {
        transform.Find("Lobby").gameObject.SetActive(false);
        transform.Find("List").gameObject.SetActive(true);
        transform.Find("List/Join").GetComponent<Button>().interactable = false;

        network.WriteLine(Message.JoinLobby.ToString());

        foreach (Transform item in GamesShown.GetComponentsInChildren<Transform>())
        {
            if (item != GamesShown.transform)
                Destroy(item.gameObject);
        }
        lowestPos = Prefabs.LobbyLabel.transform.localPosition + new Vector3(0, GamesShown.Distance, 0);
    }
}
