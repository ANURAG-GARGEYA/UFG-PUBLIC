using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Food : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject piecePrefab;
    public int pieceCount;
    [SerializeField] private GameObject _destroyEffect;

    // states should be directly defined as child objects
    private int _currentState = 0;
    private int _currentPieceCount;

    public int PiecesLeft { get { return _currentPieceCount; } }
    public GameObject CurrentStateObject
    {
        get { return transform.Find("States").GetChild(_currentState).gameObject; }
    }

    public int TotalPoints
    {
        get { return piecePrefab.GetComponent<FoodPiece>().points * pieceCount; }
    }

    public int CurrentPoints
    {
        get { return piecePrefab.GetComponent<FoodPiece>().points * _currentPieceCount; }
    }


    void Start()
    {
        EnableOnlyOneStatePrefab();
        _currentPieceCount = pieceCount;
        // FoodManager.Instance.AddUntouchedFood();
    }



    void EnableOnlyOneStatePrefab()
    {
        Transform statesTransform = transform.Find("States");
        int stateCount = statesTransform.childCount;

        if (stateCount < 1)
            return;

        statesTransform.GetChild(0).gameObject.SetActive(true);

        for (int i = 1; i < stateCount; i++)
            statesTransform.GetChild(i).gameObject.SetActive(false);
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag != "Weapon" &&
            collision.collider.tag != "Enemy")
            return;

        Collider collider = collision.collider;
        ContactPoint contactPoint = collision.contacts[0];

        if (collider.CompareTag("Enemy"))
            ResolveEnemyCollision(contactPoint.point); //SPAWN SMALL PIECE FOR ENEMy TO PICK UP

        if (collider.CompareTag("Weapon"))
        {
            Vector3 weaponVelocity = collider.GetComponentInParent<Rigidbody>().velocity;
            if (weaponVelocity.magnitude > 1.5f)
                ResolveWeaponCollision(contactPoint);
        }
    }


    void ResolveEnemyCollision(Vector3 collisionPosition)
    {

        photonView.RPC("DamageEffectOnNetwork", RpcTarget.All);
        photonView.RPC("DropFoodPiece", RpcTarget.MasterClient, collisionPosition);

        CurrentStateObject.SetActive(false);

        if (PiecesLeft == 1)
        {
            photonView.RPC("DropFoodPiece", RpcTarget.MasterClient, transform.position);
            photonView.RPC("DestroyOnNetwork", RpcTarget.All, 0f);
            return;
        }

        _currentState++;
        CurrentStateObject.SetActive(true);
    }


    void ResolveWeaponCollision(ContactPoint contactPoint)
    {
        FoodManager.Instance.AddStolenFoodPiece(TotalPoints);

        photonView.RPC("DestroyEffectOnNetwork", RpcTarget.All);
        photonView.RPC("DestroyOnNetwork", RpcTarget.All, 0f);
    }


    [PunRPC]
    void DropFoodPiece(Vector3 position)
    {
        GameObject piece = PhotonNetwork.Instantiate(piecePrefab.name, position, Quaternion.identity);
        piece.transform.parent = transform.parent;
        piece.tag = "FoodPiece";
        if (_currentPieceCount == pieceCount)
            FoodManager.Instance.RemoveUntouchedFood();
        _currentPieceCount--;
    }


    GameObject SpawnDamageVFX()
    {
        GameObject vfx = Instantiate(_destroyEffect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        vfx.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

        return vfx;
    }


    GameObject SpawnDestroyVFX()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GameObject vfx = Instantiate(_destroyEffect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        vfx.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        return vfx;
    }


    [PunRPC]
    void DamageEffectOnNetwork()
    {
        GameObject vfx = SpawnDamageVFX();
        Destroy(vfx, 1.0f);
    }


    [PunRPC]
    void DestroyEffectOnNetwork()
    {
        GameObject vfx = SpawnDestroyVFX();
        Destroy(vfx, 1.0f);
    }


    IEnumerator PhotonDestroy(int viewID, float delay)
    {
        yield return new WaitForSeconds(delay);

        PhotonView view = PhotonView.Find(viewID);
        if (view == null || !view.IsMine)
            yield break;

        PhotonNetwork.Destroy(view.gameObject);
    }


    [PunRPC]
    void DestroyOnNetwork(float delay)
    {
        StartCoroutine(PhotonDestroy(photonView.ViewID, delay));
    }

    // TO BE USED TO SYNC CRITICAL DATA
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_currentState);
            stream.SendNext(_currentPieceCount);
        }
        else
        {
            _currentState = (int)stream.ReceiveNext();
            _currentPieceCount = (int)stream.ReceiveNext();
        }
    }
}
