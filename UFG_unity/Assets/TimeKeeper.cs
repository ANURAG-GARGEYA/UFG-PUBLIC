using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Photon.Pun;
public class TimeKeeper : MonoBehaviourPun
{
    
    [HideInInspector]public float startTime;
    [HideInInspector] public float stopTime = 160f;
    public TextMeshProUGUI TimeText;



    private void OnEnable()
    {
        photonView.RPC("StartGame", RpcTarget.All);
    }

    [PunRPC]
    void StartGame()
    {
        startTime = Time.time;
        GetComponent<AudioSource>().Play();

    }

        
    // Update is called once per frame
    void Update()
    {
        updateTimeText();
    }

    void updateTimeText()
    {
        var time = TimeSpan.FromSeconds((stopTime - (Time.time - startTime)));
        TimeText.text = string.Format("{0:00}:{1:00}", time.Minutes, time.Seconds);
    }
}
