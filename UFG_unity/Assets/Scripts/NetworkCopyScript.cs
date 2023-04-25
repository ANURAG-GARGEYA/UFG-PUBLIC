using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkCopyScript : MonoBehaviour
{
    // Update is called once per frame
    public int PartID;
    public GameObject HandModel;


    private void Start()
    {
        if (GetComponent<PhotonView>().IsMine && PartID != 1)
        {
            HandModel.SetActive(false);
        }
        if (GetComponent<PhotonView>().IsMine && PhotonNetworkManager.instance.RoleChoice == PlayerRole.Attacker && PartID == 1)
        {
            HandModel.SetActive(false);
        }
    }


    void Update()
    {
        if (GetComponent<PhotonView>().IsMine)
        {

            switch (PartID)
            {
                case 1:
                    if (PhotonNetworkManager.instance.RoleChoice == PlayerRole.Defender)
                    {
                        transform.position = PhotonNetworkManager.instance.LocalPlayerHead.transform.position;
                        transform.rotation = PhotonNetworkManager.instance.LocalPlayerHead.transform.rotation;
                    }
                    else
                    {
                        transform.position = PhotonNetworkManager.instance.LocalPlayerBody.transform.position;
                        transform.rotation = PhotonNetworkManager.instance.LocalPlayerBody.transform.rotation;

                    }
                    break;


                case 2:
                    transform.position = PhotonNetworkManager.instance.LocalPlayerRightHand.transform.position;
                    transform.rotation = PhotonNetworkManager.instance.LocalPlayerRightHand.transform.rotation;
                    break;

                case 3:
                    transform.position = PhotonNetworkManager.instance.LocalPlayerLeftHand.transform.position;
                    transform.rotation = PhotonNetworkManager.instance.LocalPlayerLeftHand.transform.rotation;
                    break;
            }
        }
    }

}