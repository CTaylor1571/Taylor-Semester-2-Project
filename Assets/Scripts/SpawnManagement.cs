using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagement : MonoBehaviourPunCallbacks
{

    public GameObject playerPrefab;


    // Start is called before the first frame update
    void Start()
    {
        if (PlayerMovement.LocalPlayerInstance == null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Is master client");
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 1.5f, -22f), Quaternion.identity, 0);
                
            }
            else
            {
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 1.5f, 13f), Quaternion.identity, 0);
            }
        }

    }
}
