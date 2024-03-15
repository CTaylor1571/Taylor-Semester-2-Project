using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAgainButton : MonoBehaviour
{

    [SerializeField]
    GameObject manager;

    public void OnButtonPress()
    {
        Debug.Log("Rematch button pressed");
        manager.GetComponent<WinScreenReady>().OnPlayAgain();
    }


}
