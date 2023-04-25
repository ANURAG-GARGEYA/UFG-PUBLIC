using UnityEngine;
using System.Linq;
using Photon.Pun;

public class EnemyManager : MonoBehaviourPun
{
    public static EnemyManager Instance;
    [SerializeField] private int _enemySpawnLimit;
    [SerializeField] private int[] _enemiesSpawned = new int[2];
    [SerializeField] private int[] _enemiesAlive = new int[2];
    [SerializeField] private int[] _enemiesEscaped = new int[2];
    [SerializeField] private int[] _enemiesKilled = new int[2];
    [SerializeField] private int _totalKilledPoints;
    [SerializeField] private int _totalEscapedPoints;
    [SerializeField] private bool _queenDead;

    public int EnemiesAlive { get { return _enemiesAlive.Sum(); } }
    public int CrawlingEnemiesAlive { get { return _enemiesAlive[(int)EnemyType.Crawling]; } }
    public int FlyingEnemiesAlive { get { return _enemiesAlive[(int)EnemyType.Flying]; } }
    public int EnemiesKilled { get { return _enemiesKilled.Sum(); } }
    public int CrawlingEnemiesKilled { get { return _enemiesKilled[(int)EnemyType.Crawling]; } }
    public int FlyingEnemiesKilled { get { return _enemiesKilled[(int)EnemyType.Flying]; } }
    public int EnemiesEscaped { get { return _enemiesEscaped.Sum(); } }
    public int CrawlingEnemiesEscaped { get { return _enemiesEscaped[(int)EnemyType.Crawling]; } }
    public int FlyingEnemiesEscaped { get { return _enemiesEscaped[(int)EnemyType.Flying]; } }
    public int TotalKilledPoints { get { return _totalKilledPoints; } }
    public int TotalEscapedPoints { get { return _totalEscapedPoints; } }
    public int EnemySpawnLimit { get { return _enemySpawnLimit; } }
    public int EnemiesLeftToSpawn { get { return _enemySpawnLimit - _enemiesSpawned.Sum(); } }
    public bool IsQueenDead { get { return _queenDead; } }

    void Awake()
    {
        Instance = this;
    }


    public void AddAlive(EnemyType type)
    {
        photonView.RPC("AddAliveRPC", RpcTarget.All, (int)type);
    }


    [PunRPC]
    private void AddAliveRPC(int type)
    {
        _enemiesSpawned[type]++;
        _enemiesAlive[type]++;
    }


    public void AddKilled(EnemyType type, int points)
    {
        photonView.RPC("AddKilledRPC", RpcTarget.All, (int)type, points);
    }

    [PunRPC]
    private void AddKilledRPC(int type, int points)
    {
        _enemiesAlive[type]--;
        _enemiesKilled[type]++;
        _totalKilledPoints += points;
    }


    public void AddEscaped(EnemyType type, int points)
    {
        photonView.RPC("AddEscapedRPC", RpcTarget.All, (int)type, points);
    }


    [PunRPC]
    private void AddEscapedRPC(int type, int points)
    {
        _enemiesAlive[type]--;
        _enemiesEscaped[type]++;
        _totalEscapedPoints += points;
    }


    public void SetQueenDead()
    {
        photonView.RPC("SetQueenDeadRPC", RpcTarget.All);
    }

    [PunRPC]
    private void SetQueenDeadRPC()
    {
        _queenDead = true;
    }
}


