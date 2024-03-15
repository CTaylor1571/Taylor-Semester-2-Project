using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class ColorSelectionScript : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject[] colors;

    [SerializeField] private GameObject mySelection;
    [SerializeField] private GameObject mySelectionBackground;

    [SerializeField] private GameObject otherSelection;
    [SerializeField] private GameObject otherSelectionBackground;

    int selectedColor = -1;
    int otherSelectedColor = -1;


    // If you are host and other player not joined yet, otherSelection will be hidden.
    // when joining as a guest, must receive host's selection before randomly setting selectedColor
    // when other leaves, remove otherSelection

    // have to add button clicking


    public void OnStart()
    {
        mySelection.SetActive(false);
        mySelectionBackground.SetActive(false);
        otherSelection.SetActive(false);
        otherSelectionBackground.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            selectedColor = Random.Range(0, 4);
            photonView.RPC("SendSelection", RpcTarget.AllViaServer, selectedColor, true, false);
        }
        colors[0].GetComponent<Button>().onClick.AddListener(() => ColorClicked(0));
        colors[1].GetComponent<Button>().onClick.AddListener(() => ColorClicked(1));
        colors[2].GetComponent<Button>().onClick.AddListener(() => ColorClicked(2));
        colors[3].GetComponent<Button>().onClick.AddListener(() => ColorClicked(3));
    }

    void ColorClicked(int color)
    {
        if (color == selectedColor || color == otherSelectedColor)
        {
            return;
        }
        photonView.RPC("SendSelection", RpcTarget.AllViaServer, color, PhotonNetwork.IsMasterClient, false);
    }
    

    [PunRPC]
    void SendSelection(int selected, bool sentFromHost, bool initialHostSend)
    {
        if ((PhotonNetwork.IsMasterClient && !sentFromHost) || (!PhotonNetwork.IsMasterClient && sentFromHost))  // if the other player sent it
        {
            if (selected == selectedColor) // if they somehow selected my color, does nothing
            {
                return;
            }
            otherSelectedColor = selected;
            otherSelection.SetActive(true);
            otherSelectionBackground.SetActive(true);
            otherSelection.transform.position = colors[otherSelectedColor].transform.position;
            otherSelectionBackground.transform.position = colors[otherSelectedColor].transform.position;

            if (initialHostSend)
            {
                Debug.Log("Received initial send from host");
                selectedColor = Random.Range(0, 4);
                while (selectedColor == otherSelectedColor)
                {
                    selectedColor = Random.Range(0, 4);
                }
                photonView.RPC("SendSelection", RpcTarget.AllViaServer, selectedColor, false, false);
            }

        }
        else                                                                                                     // if I sent it
        {
            if (selected == otherSelectedColor) // if I somehow select the other person's color, does nothing
            {
                return;
            }
            selectedColor = selected;
            mySelectionBackground.SetActive(true);
            mySelection.SetActive(true);
            mySelection.transform.position = colors[selectedColor].transform.position;
            mySelectionBackground.transform.position = colors[selectedColor].transform.position;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        photonView.RPC("SendSelection", RpcTarget.AllViaServer, selectedColor, true, true);
    }

    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        otherSelection.SetActive(false);
        otherSelectionBackground.SetActive(false);
        otherSelectedColor = -1;
    }
    public override void OnLeftRoom()
    {
        otherSelection.SetActive(false);
        otherSelectionBackground.SetActive(false);
        otherSelectedColor = -1;
        mySelection.SetActive(false);
        mySelectionBackground.SetActive(false);
        selectedColor = -1;
        colors[0].GetComponent<Button>().onClick.RemoveListener(() => ColorClicked(0));
        colors[1].GetComponent<Button>().onClick.RemoveListener(() => ColorClicked(1));
        colors[2].GetComponent<Button>().onClick.RemoveListener(() => ColorClicked(2));
        colors[3].GetComponent<Button>().onClick.RemoveListener(() => ColorClicked(3));
    }

    public void CountdownStarted()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StaticScript.hostColor = selectedColor;
            StaticScript.guestColor = otherSelectedColor;
        }
        else
        {
            StaticScript.guestColor = selectedColor;
            StaticScript.hostColor = otherSelectedColor;
        }
        colors[0].GetComponent<Button>().onClick.RemoveListener(() => ColorClicked(0));
        colors[1].GetComponent<Button>().onClick.RemoveListener(() => ColorClicked(1));
        colors[2].GetComponent<Button>().onClick.RemoveListener(() => ColorClicked(2));
        colors[3].GetComponent<Button>().onClick.RemoveListener(() => ColorClicked(3));
        Debug.Log("Host color: " + StaticScript.hostColor + ". Guest color: " +  StaticScript.guestColor + ".");
    }
}
