using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ScoreKeeper : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    int kills = 0;
    // Start is called before the first frame update

    public static ScoreKeeper instance;
    private void Start()
    {
        instance = this;
    }


    public void UpdateKill()
    {
        kills++;
        ScoreText.text = "Kills : " + kills;
       // throw new NotImplementedException();
    }
}
