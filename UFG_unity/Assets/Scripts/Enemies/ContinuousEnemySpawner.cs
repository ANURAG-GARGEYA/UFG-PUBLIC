using System.Linq;
using System.Collections;
using UnityEngine;

public class ContinuousEnemySpawner : MonoBehaviour
{
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

    public GameObject enemyPrefab;
    public int maxEnemyCount;
    public float spawnDelay;
    public EnemyPath[] paths;

    private bool _isSpawning = false;


    void Update()
    {
        if (transform.childCount < maxEnemyCount && !_isSpawning)
            StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        _isSpawning = true;
        yield return new WaitForSeconds(spawnDelay);

        GameObject enemyObj = Instantiate(enemyPrefab, transform);
        EnemyPath[] eligiblePaths = paths.Where(p => p.EnemiesOnPath == 0).ToArray();

        if (eligiblePaths.Length > 0)
        {
            CrawlingEnemy enemy = enemyObj.GetComponent<CrawlingEnemy>();

            int randomGoalIndex = Random.Range(0, eligiblePaths.Length);
            enemy.Path = eligiblePaths[randomGoalIndex];
        }

        _isSpawning = false;
    }
}
