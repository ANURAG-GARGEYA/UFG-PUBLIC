using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Prop : MonoBehaviourPun
{
    [SerializeField] public int points;

    [SerializeField] private GameObject _destroyEffect;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            PropManager.Instance.AddDestroyedProp(points);
            photonView.RPC("DestroyEffectOnNetwork", RpcTarget.All);
            photonView.RPC("DestroyOnNetwork", RpcTarget.All, photonView.ViewID);

        }

    }

    GameObject SpawnDestroyVFX()
    {
        GetComponent<Renderer>().enabled = false;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GameObject vfx = Instantiate(_destroyEffect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        vfx.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        return vfx;
    }


    [PunRPC]
    void DestroyEffectOnNetwork()
    {
        GameObject vfx = SpawnDestroyVFX();
        Destroy(vfx, 1.0f);
    }


    [PunRPC]
    void DestroyOnNetwork(int ViewID)
    {
        StartCoroutine(PhotonDestroy(ViewID, 1f));
    }


    IEnumerator PhotonDestroy(int viewID, float delay)
    {
        yield return new WaitForSeconds(delay);

        PhotonView view = PhotonView.Find(viewID);
        if (view == null || !view.IsMine)
            yield break;

        PhotonNetwork.Destroy(view.gameObject);
    }

}
