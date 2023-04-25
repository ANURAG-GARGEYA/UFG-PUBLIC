using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;
using BNG;

public class ScoringSystem : MonoBehaviourPun
{

    [SerializeField] private int _queenScore;
    [SerializeField] private int _queenWinningScore;
    [SerializeField] private int _playerScore;
    [SerializeField] private TimeKeeper _timeKeeper;

    public UnityEngine.UI.Slider queenProgressSlider;
    public TextMeshProUGUI enemiesText;
    public TextMeshProUGUI environmentText;

    public GameObject WinScreen;
    public GameObject LooseScreen;
    public GameObject ScoreBoard;

    public AudioClip queenWinClip;
    public AudioClip defenderWinClip;

    private AudioSource audioSource;
    private bool isGameComplete = false;
    public float QueenProgress { get { return (float)_queenScore / (float)_queenWinningScore; } }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.pitch = Time.timeScale;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 1f;

        _queenWinningScore = (int)((PropManager.Instance.OriginalPropsValue + FoodManager.Instance.OriginalFoodValue) * 0.6f);
    }


    private void Update()
    {
        if (_queenScore >= _queenWinningScore && !isGameComplete)
        {
            photonView.RPC("QueenWins", RpcTarget.All);
            isGameComplete = true;
        }

        if ((Time.time - _timeKeeper.startTime >= _timeKeeper.stopTime || EnemyManager.Instance.IsQueenDead) && !isGameComplete)
        {
            photonView.RPC("DefenderWins", RpcTarget.All);
            isGameComplete = true;
        }

        if (isGameComplete)
        {
            var rhmd = GetComponent<RotateWithHMD>();
            rhmd.enabled = true;
            if (PhotonNetworkManager.instance.RoleChoice == PlayerRole.Defender)
                rhmd.Offset = new Vector3(0f, -0.29f, -3f);
        }

        UpdateScores();
        UpdateSlider();
        UpdateTexts();
    }


    [PunRPC]
    void QueenWins()
    {
        if (PhotonNetworkManager.instance.RoleChoice == PlayerRole.Attacker)
            WinScreen.SetActive(true);

        else
            LooseScreen.SetActive(true);

        ScoreBoard.SetActive(false);
        audioSource.clip = queenWinClip;
        audioSource.Play();
    }


    [PunRPC]
    void DefenderWins()
    {
        if (PhotonNetworkManager.instance.RoleChoice == PlayerRole.Defender)
            WinScreen.SetActive(true);
        else
            LooseScreen.SetActive(true);

        ScoreBoard.SetActive(false);
        audioSource.clip = defenderWinClip;
        audioSource.Play();
    }


    void UpdateScores()
    {
        _queenScore = PropManager.Instance.DestroyedPropsValue +
                        FoodManager.Instance.StolenFoodValue +
                        EnemyManager.Instance.TotalEscapedPoints;

        _playerScore = PropManager.Instance.CurrentPropsValue +
                        FoodManager.Instance.CurrentFoodValue +
                        EnemyManager.Instance.TotalKilledPoints;
    }


    void UpdateSlider()
    {
        queenProgressSlider.value = QueenProgress;
    }


    void UpdateTexts()
    {
        string[] enemiesStr = {
            "Bugs killed:  " + EnemyManager.Instance.EnemiesKilled,
            "Bugs escaped: " + EnemyManager.Instance.EnemiesEscaped,
        };

        string[] envStr = {
            string.Format("Food:  {0}/{1}", FoodManager.Instance.CurrentFoodValue, FoodManager.Instance.OriginalFoodValue),
            string.Format("Props:  {0}/{1}", PropManager.Instance.CurrentPropsValue, PropManager.Instance.OriginalPropsValue),
        };

        //TimeText.text = FormatTime(stopTime - (Time.time - startTime));
        //var time = TimeSpan.FromSeconds((stopTime - (Time.time - startTime)));
        //TimeText.text = string.Format("{0:00}:{1:00}", time.Minutes, time.Seconds);

        enemiesText.text = string.Join("\n", enemiesStr);
        environmentText.text = string.Join("\n", envStr);
    }
}
