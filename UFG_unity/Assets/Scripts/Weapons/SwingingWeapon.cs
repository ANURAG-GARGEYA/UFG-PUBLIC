using UnityEngine;
using BNG;
using Photon.Pun;

public class SwingingWeapon : GrabbableEvents
{
    private Rigidbody _rigidbody;
    private PhotonView _photonView;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public override void OnGrab(Grabber grabber)
    {
        _rigidbody.constraints = RigidbodyConstraints.None;
        _photonView.RequestOwnership();
        base.OnGrab(grabber);
    }
}
