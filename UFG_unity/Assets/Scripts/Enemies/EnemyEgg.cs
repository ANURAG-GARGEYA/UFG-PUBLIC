using System.Collections;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class EnemyEgg : MonoBehaviourPun
{
    public GameObject enemyPrefab;
    public int amountToSpawn;

    [SerializeField] private GameObject _destroyEffect;

    private Transform[] _waypoints;

    private Transform ClosestWaypoint
    {
        get
        {
            float minDistance = float.MaxValue;
            Transform closestWaypoint = null;

            foreach (Transform wp in _waypoints)
            {
                float currentDistance = Vector3.Distance(transform.position, wp.position);

                if (minDistance > currentDistance)
                {
                    minDistance = currentDistance;
                    closestWaypoint = wp;
                }
            }

            return closestWaypoint;
        }
    }


    void Start()
    {
        PathType pathType = enemyPrefab.GetComponent<FlyingEnemy>() != null ?
                            PathType.Flying :
                            PathType.Crawling;
        _waypoints = FindObjectsOfType<EnemyPathWaypoint>()
                        .Where(wp => wp.transform.parent.parent.GetComponent<EnemyPath>().type == pathType)
                        .Select(wp => wp.transform).ToArray();
    }


    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;

        if (collision.collider.tag != "Floor")
            return;

        SpawnEnemies();
        photonView.RPC("DestroyEffectOnNetwork", RpcTarget.All);
        photonView.RPC("DestroyOnNetwork", RpcTarget.All, 0f);
    }


    void SpawnEnemies()
    {
        EnemyPath path = ClosestWaypoint.parent.parent.GetComponent<EnemyPath>();
        Debug.Log(path.type);

        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject enemyObj = PhotonNetwork.Instantiate(enemyPrefab.name, transform.position, Quaternion.identity);
            CrawlingEnemy enemy = enemyObj.GetComponent<CrawlingEnemy>();
            enemy.Path = path;
        }
    }

    IEnumerator PhotonDestroy(int viewID, float delay)
    {
        yield return new WaitForSeconds(delay);

        PhotonView view = PhotonView.Find(viewID);
        if (view == null || !view.IsMine)
            yield break;

        PhotonNetwork.Destroy(view.gameObject);
    }

    GameObject SpawnDestroyVFX()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GameObject vfx = Instantiate(_destroyEffect, transform.position, Quaternion.identity);
        vfx.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        return vfx;
    }


    [PunRPC]
    void DestroyEffectOnNetwork()
    {
        GameObject vfx = SpawnDestroyVFX();
        Destroy(vfx, 1.0f);
    }

    [PunRPC]
    void DestroyOnNetwork(float delay)
    {
        StartCoroutine(PhotonDestroy(photonView.ViewID, delay));
    }
}
