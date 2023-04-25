using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OfflineSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _antEggPrefab, _flyEggPrefab;
    [SerializeField] private Transform _antSpawnPosition, _flySpawnPosition;

    public void SpawnEnemies()
    {
        PhotonNetwork.Instantiate(_antEggPrefab.name, _antSpawnPosition.position, Quaternion.identity);
        PhotonNetwork.Instantiate(_flyEggPrefab.name, _flySpawnPosition.position, Quaternion.identity);
    }
}
