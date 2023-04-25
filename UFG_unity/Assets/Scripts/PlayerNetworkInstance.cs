using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerNetworkInstance : MonoBehaviour
{
    public GameObject NetworkHead1, NetworkHead2, NetworkRightHand, NetworkLeftHand;
    public PlayerRole RoleChoice;

    private void Start()
    {

        if (RoleChoice == PlayerRole.Defender)
        {
            Debug.Log("role choice if 1");
            GameObject a = PhotonNetwork.Instantiate(NetworkHead1.name, transform.position, transform.rotation);
            a.GetComponent<NetworkCopyScript>().PartID = 1;
        }
        else if (RoleChoice == PlayerRole.Attacker)
        {
            Debug.Log("role choice if 2");
            GameObject a = PhotonNetwork.Instantiate(NetworkHead2.name, transform.position, transform.rotation);
            a.GetComponent<NetworkCopyScript>().PartID = 1;

        }
        GameObject b = PhotonNetwork.Instantiate(NetworkRightHand.name, transform.position, transform.rotation);
        b.GetComponent<NetworkCopyScript>().PartID = 2;
        GameObject c = PhotonNetwork.Instantiate(NetworkLeftHand.name, transform.position, transform.rotation);
        c.GetComponent<NetworkCopyScript>().PartID = 3;

    }

}

