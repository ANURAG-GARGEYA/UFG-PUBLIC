using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaveEnemySpawner : MonoBehaviour
{
    [Serializable]
    public struct Wave
    {
        public float spawnTime;
        public int enemyCount;
        public GameObject enemyPrefab;
    }

    #region Gizmo Options
    private float _gizmoSize = 0.2f;
    private bool _isVisible = true;
    private Color _gizmoColor = Color.red;

    public bool IsVisible
    {
        get { return _isVisible; }
        set { _isVisible = value; }
    }

    public Color GizmoColor
    {
        get { return _gizmoColor; }
        set { _gizmoColor = value; }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = _gizmoColor;
        // TODO replace with an icon or mesh
        Gizmos.DrawWireCube(transform.position + new Vector3(0f, _gizmoSize / 2, 0f),
                            new Vector3(_gizmoSize, _gizmoSize, _gizmoSize));
    }
    #endregion

    public List<Wave> waves = new List<Wave>();
    public float spawnDelay;
    private bool _isSpawning = false;



    void Update()
    {
        if (waves.Count == 0 || _isSpawning)
            return;

        foreach (Wave wave in waves)
        {
            if (wave.spawnTime < Time.time && !_isSpawning)
                StartCoroutine(SpawnWave(wave));
        }

    }

    IEnumerator SpawnWave(Wave wave)
    {
        _isSpawning = true;
        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject enemy = Instantiate(wave.enemyPrefab, transform);
            yield return new WaitForSeconds(spawnDelay);
        }
        _isSpawning = false;

        waves.Remove(wave);
    }
}
