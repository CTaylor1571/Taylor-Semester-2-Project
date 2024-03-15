using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class WinScreenReady : MonoBehaviourPunCallbacks
{
    
    [SerializeField]
    public GameObject winner;

    [SerializeField]
    public GameObject loser;

    [SerializeField]
    public GameObject text;

    private Animator winAnim;
    private Animator loseAnim;
    private bool winnerReady;
    private bool loserReady;

    [SerializeField]
    Material[] materials;

    // Start is called before the first frame update
    void Start()
    {
        winnerReady = false;
        loserReady = false;
        loser.GetComponentInParent<ParticleSystem>().Pause();
        winAnim = winner.GetComponent<Animator>();
        loseAnim = loser.GetComponent<Animator>();

        
        if (StaticScript.hostUserID == StaticScript.winnerID) // if the host is the winner
        {
            winner.GetComponentInChildren<SkinnedMeshRenderer>().material = materials[StaticScript.hostColor];
            loser.GetComponentInChildren<SkinnedMeshRenderer>().material = materials[StaticScript.guestColor];
            text.GetComponent<TextMeshProUGUI>().text = "" + GetWordFromIndex(StaticScript.hostColor) + " KNIGHT WINS";
        }
        else // if guest is the winner
        {
            winner.GetComponentInChildren<SkinnedMeshRenderer>().material = materials[StaticScript.guestColor];
            loser.GetComponentInChildren<SkinnedMeshRenderer>().material = materials[StaticScript.hostColor];
            text.GetComponent<TextMeshProUGUI>().text = "" + GetWordFromIndex(StaticScript.guestColor) + " KNIGHT WINS";
        }

    }

    public void OnPlayAgain()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("PlayAnimation", RpcTarget.All, StaticScript.hostUserID);
        }
        else
        {
            photonView.RPC("PlayAnimation", RpcTarget.All, StaticScript.guestUserID);
        }
    }

    [PunRPC]
    void PlayAnimation(string playerActorNumber)
    {
        if (playerActorNumber == StaticScript.winnerID && !winnerReady)
        {
            winnerReady = true;
            winAnim.SetTrigger("ButtonPressed");
            Debug.Log("Playing Win Animation");
        }
        else if (!loserReady)
        {
            loserReady = true;
            loseAnim.SetTrigger("ButtonPressed");
            StartCoroutine("Particles");
            Debug.Log("Playing Lose Animation");
        }
        if (loserReady && winnerReady)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine("WaitSeconds");
            }
            StaticScript.gameOver = false;
        }
    }

    IEnumerator WaitSeconds()
    {
        yield return new WaitForSeconds(7f);
        Debug.Log("Loading scene");
        PhotonNetwork.LoadLevel("SampleScene");
    }


    IEnumerator Particles(){

        loser.GetComponentInParent<ParticleSystem>().Play();
        yield return new WaitForSeconds(4.2f);
        loser.GetComponentInParent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    string GetWordFromIndex(int index)
    {
        if (index == 0)
        {
            return "BLACK";
        }
        if (index == 1)
        {
            return "GRAY";
        }
        if (index == 2)
        {
            return "PINK";
        }
        else
        {
            return "WHITE";
        }
    }


    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        Debug.Log("Other player has left room");
        StaticScript.gameOver = false;
        StaticScript.hostUserID = "";
        StaticScript.guestUserID = "";
        StaticScript.hostColor = -1;
        StaticScript.guestColor = -1;
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("ConnectScreen");
    }

    void LeaveRoom()
    {
        StaticScript.gameOver = false;
        StaticScript.hostUserID = "";
        StaticScript.guestUserID = "";
        StaticScript.hostColor = -1;
        StaticScript.guestColor = -1;
        Debug.Log("Leaving room");
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("ConnectScreen");
    }


}
