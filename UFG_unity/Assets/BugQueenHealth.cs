using UnityEngine;
using BNG;
using Photon.Pun;
using System.Collections;

public class BugQueenHealth : MonoBehaviourPun
{
    public int health;
    public GameObject deathEffect;
    public Transform _spawn;
    private PlayerTeleport teleport;
    private ScreenFader fader;
    private GameObject inventory;

    void Start()
    {
        teleport = GameObject.Find("PlayerController").GetComponent<PlayerTeleport>();
        fader = GameObject.Find("CenterEyeAnchor").GetComponent<ScreenFader>();
        inventory = GameObject.Find("Inventory");

        fader.FadeInSpeed = 10f;

        GameObject spawnGO = new GameObject();
        spawnGO.name = "Spawn";
        Transform XRRig = GameObject.Find("XR Rig Advanced").transform;
        spawnGO.transform.position = XRRig.position;
        spawnGO.transform.localRotation = XRRig.localRotation;
        _spawn = spawnGO.transform;
    }


    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.K))
    //     {
    //         fader.DoFadeIn();
    //     }

    //     if (Input.GetKeyDown(KeyCode.L))
    //     {
    //         fader.DoFadeOut();
    //     }
    // }


    void OnTriggerEnter(Collider collider)
    {
        if (!photonView.IsMine)
            return;

        if (collider.tag == "Weapon")
        {
            photonView.RPC("DeathEffectOnNetwork", RpcTarget.All);
            StartCoroutine(Damage());
        }
    }

    IEnumerator Damage()
    {
        health--;
        inventory.SetActive(false);
        if (health <= 0)
        {
            EnemyManager.Instance.SetQueenDead();
            yield break;
        }
        fader.DoFadeIn();
        GameObject.Find("ScreenFader").GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0.04f);
        teleport.TeleportPlayerToTransform(_spawn);
        yield return new WaitForSeconds(3f);
        fader.DoFadeOut();
        inventory.SetActive(true);
        photonView.RPC("MakeVisibleAgain", RpcTarget.All);
    }


    protected GameObject SpawnVFX(GameObject effect, Vector3 scale)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        GetComponent<Collider>().enabled = false;
        GameObject vfx = Instantiate(effect, transform.position, Quaternion.identity); //Quaternion.Euler(-90f, 0f, 0f)
        vfx.transform.localScale = scale;

        return vfx;
    }

    [PunRPC]
    void DeathEffectOnNetwork()
    {
        GameObject vfx = SpawnVFX(deathEffect, new Vector3(1f, 1f, 1f));
        Destroy(vfx, 1.0f);
    }

    [PunRPC]
    void MakeVisibleAgain()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
