using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
public class SceneTransitionManager : MonoBehaviour
{
  
    public void CallMainScene()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();

        Invoke("CallLevelAlpha", 2);

    }

    public void CallLevelAlpha()
    {
        SceneManager.LoadScene("LevelAlpha");
    }
}

