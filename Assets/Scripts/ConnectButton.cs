using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;

public class ConnectButton : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameObject inputTextObj;

    [SerializeField]
    GameObject resultTextObj;

    [SerializeField]
    GameObject startButton;

    [SerializeField]
    GameObject colors;

    string textInput;

    bool connectedToLobby;


    private void Start()
    {
        Debug.Log("Scene entered");
        colors.SetActive(false);
        GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Soundtrack");
        startButton.GetComponent<Button>().enabled = false;
        startButton.SetActive(false);
        connectedToLobby = false;
        gameObject.GetComponent<Button>().enabled = false;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN.");
        gameObject.GetComponent<Button>().enabled = true;
    }

    public void OnButtonPress()
    {
        if (!connectedToLobby)
        {
            textInput = inputTextObj.GetComponent<TextMeshProUGUI>().text;
            Debug.Log("Attempting to join lobby name: " + textInput);
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.PublishUserId = true;
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.JoinOrCreateRoom(textInput, roomOptions, TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Attempting to leave lobby");
            GetComponentInChildren<TextMeshProUGUI>().text = "Connect";
        }
            
    }

    public void StartButtonPressed()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("StartCountdown", RpcTarget.AllViaServer, "jup", "and jup.");
        }
    }

    [PunRPC]
    void StartCountdown(string a, string b)
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Starting countdown");
        StartCoroutine("Countdown");
    }

    [PunRPC]
    void SetActorNumbers(string host, string guest)
    {
        StaticScript.hostUserID = host;                                                       // send the actor number to the non-host player
        StaticScript.guestUserID = guest;
        Debug.Log("Host number: " + host + ". Guest number: " + guest);
    }
    

    void OnCountdownEnd()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Loading scene");
            PhotonNetwork.LoadLevel("SampleScene");
        }
    }

    IEnumerator Countdown()
    {
        resultTextObj.GetComponent<TextMeshProUGUI>().color = Color.white;
        resultTextObj.GetComponentInChildren<TextMeshProUGUI>().text = "Match starting in 3...";
        yield return new WaitForSeconds(1);
        colors.GetComponent<ColorSelectionScript>().CountdownStarted();
        resultTextObj.GetComponentInChildren<TextMeshProUGUI>().text = "Match starting in 2...";
        yield return new WaitForSeconds(1);
        resultTextObj.GetComponentInChildren<TextMeshProUGUI>().text = "Match starting in 1...";
        yield return new WaitForSeconds(1);
        OnCountdownEnd();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created lobby with name: " + textInput);
        resultTextObj.GetComponent<TextMeshProUGUI>().text = "Succesfully created room.";
        resultTextObj.GetComponent<TextMeshProUGUI>().color = Color.green;
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined lobby with name: " + textInput);

        GameObject.Find("Lobby Input Field").GetComponent<TMP_InputField>().enabled = false;
        connectedToLobby = true;
        resultTextObj.GetComponent<TextMeshProUGUI>().text = "Successfully joined room: " + textInput + ". Waiting for other player.";
        resultTextObj.GetComponent<TextMeshProUGUI>().color = Color.green;
        GetComponentInChildren<TextMeshProUGUI>().text = "Leave";

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            startButton.GetComponent<Button>().enabled = false;
            startButton.GetComponent<Image>().color = new Color(1f, 76f / 255f, 64f / 255f);
            
        }

        colors.SetActive(true);
        colors.GetComponent<ColorSelectionScript>().OnStart();
    }

    public override void OnLeftRoom()
    {
        connectedToLobby = false;
        Debug.Log("Lobby successfully left");
        resultTextObj.GetComponent<TextMeshProUGUI>().text = "";
        GameObject.Find("Lobby Input Field").GetComponent<TMP_InputField>().enabled = true;

        if (startButton.activeInHierarchy)
        {
            startButton.SetActive(false);
        }
        colors.SetActive(false);
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Another player has entered the lobby");
        resultTextObj.GetComponent<TextMeshProUGUI>().text = "Another player has entered the lobby.";
        resultTextObj.GetComponent<TextMeshProUGUI>().color = Color.green;
        if (PhotonNetwork.IsMasterClient)
        {
            StaticScript.hostUserID = "" + PhotonNetwork.LocalPlayer.ActorNumber;                                                       // HERE IS WHERE I SET ACTOR NUMBER!!!!
            StaticScript.guestUserID = "" + newPlayer.ActorNumber;
            Debug.Log("This client is the master client.");
            startButton.GetComponent<Image>().color = new Color(76f / 255f, 1f, 64f / 255f);
            startButton.GetComponent<Button>().enabled = true;
            photonView.RPC("SetActorNumbers", RpcTarget.All, StaticScript.hostUserID, StaticScript.guestUserID);          // call SendActorNumbers RPC
        }
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        Debug.Log("Other player has left the lobby");
        resultTextObj.GetComponent<TextMeshProUGUI>().text = "Other player has left the lobby.";
        resultTextObj.GetComponent<TextMeshProUGUI>().color = Color.red;
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            startButton.GetComponent<Image>().color = new Color(1f, 76f / 255f, 64f / 255f);
            startButton.GetComponent<Button>().enabled = false;
        }
    }



    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create lobby with name: " + textInput);
        resultTextObj.GetComponent<TextMeshProUGUI>().text = "Failed to create room.";
        resultTextObj.GetComponent<TextMeshProUGUI>().color = Color.red;
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join lobby with name: " + textInput);
        resultTextObj.GetComponent<TextMeshProUGUI>().text = "Failed to join room. Lobby might be full.";
        resultTextObj.GetComponent<TextMeshProUGUI>().color = Color.red;
    }
}
