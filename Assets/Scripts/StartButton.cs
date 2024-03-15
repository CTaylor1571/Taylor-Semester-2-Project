using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{

    [SerializeField]
    GameObject connectButton;

    public void OnStartButtonPress()
    {
        Debug.Log("Start button pressed");
        connectButton.GetComponent<ConnectButton>().StartButtonPressed();
    }

    
}
