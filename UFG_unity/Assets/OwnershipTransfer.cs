using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class OwnershipTransfer : MonoBehaviour
{
    public void OngrabbedTransfer()
    {
         GetComponent<PhotonView>().RequestOwnership();
    }
}
