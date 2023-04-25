using UnityEngine;
using Photon.Pun;
using BNG;

public class EggHolster : MonoBehaviourPunCallbacks
{
    public SnapZone holster;
    public GameObject eggPrefab;


    public void AddEgg()
    {
        Grabbable eggGrabbable = PhotonNetwork.Instantiate(eggPrefab.name, transform.position, transform.rotation).GetComponent<Grabbable>();
        eggGrabbable.transform.position = holster.transform.position;
        holster.GrabGrabbable(eggGrabbable);
    }
}
