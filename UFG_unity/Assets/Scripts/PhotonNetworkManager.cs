using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using BNG;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif


public enum PlayerRole { Defender, Attacker }



public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager instance;

    public Text ServerMessage;
    public string RoomId;
    public GameObject PlayerNetworkInstance, Inventory, CharecterSelectScreen, RoomIDUIScreen, LocalPlayerHead, LocalPlayerRightHand, LocalPlayerLeftHand, LocalPlayerBody, XrRigAdvanced;
    public GameObject antEggPrefab;
    public GameObject flyEggPrefab;

    public GameObject FlySwatter;

    public GameObject Timer;

    public PlayerRole RoleChoice;


    [SerializeField] private bool _isOfflineScene;
    void Start()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
        if (_isOfflineScene)
            PhotonNetwork.OfflineMode = true;

        if (RoleChoice == PlayerRole.Defender)
            Inventory.SetActive(false);

        instance = this;
        PhotonNetwork.ConnectUsingSettings();
    }


    public override void OnConnectedToMaster()
    {
        ServerMessage.text = "Connected to server, Enter the room id to create or join the room.";
        Debug.Log("Connected to server, Enter the room id to create or join the room.");
        base.OnConnectedToMaster();
        OnCreateRoomClicked();
    }


    public void OnCreateRoomClicked()
    {
        RoomOptions _roomoptions = new RoomOptions();
        _roomoptions.IsOpen = true;
        _roomoptions.MaxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom(RoomId, _roomoptions, typedLobby: default);

    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        ServerMessage.text = "Joining room failed. Creating a room";
        Debug.Log("Joining room failed. Creating a room");
        base.OnJoinRandomFailed(returnCode, message);
    }


    public override void OnCreatedRoom()
    {
        ServerMessage.text = "Created room sucessfully.";
        Debug.Log("Created room sucessfully.");
        base.OnCreatedRoom();
    }


    public override void OnJoinedRoom()
    {
        ServerMessage.text = "Joined room sucessfully.";
        Debug.Log("Joined room sucessfully.");
        OnCharecterSelected();

        if (PhotonNetwork.PlayerList.Length > 1 &&!_isOfflineScene)
            photonView.RPC("StartGame", RpcTarget.All) ;
        //else
        //    Time.timeScale = 0;
        base.OnJoinedRoom();

    }
    
    [PunRPC]
    void StartGame()
    {
        Time.timeScale = 1;
        Timer.SetActive(true);
        FlySwatter.SetActive(true);
    }

    public void OnCharecterSelected()
    {
        GameObject player = PhotonNetwork.Instantiate(PlayerNetworkInstance.name, transform.position, transform.rotation);
        player.GetComponent<PlayerNetworkInstance>().RoleChoice = RoleChoice;
        if (RoleChoice == PlayerRole.Defender)
        {
            foreach (GameObject prop in GameObject.FindGameObjectsWithTag("Property"))
                prop.GetPhotonView().RequestOwnership();
            XrRigAdvanced.transform.position = new Vector3(-0.675f, 1.0f, -0.8f);
            XrRigAdvanced.transform.localRotation = Quaternion.identity;
        }

        if (RoleChoice == PlayerRole.Attacker)
        {
            SetupEggHolster("HolsterLeft", antEggPrefab);
            SetupEggHolster("HolsterRight", flyEggPrefab);

            XrRigAdvanced.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }

        CharecterSelectScreen.SetActive(false);
        FoodManager.Instance.AddUntouchedFood();
    }

    public void SetupEggHolster(string holsterName, GameObject egg)
    {
        Transform inventory = XrRigAdvanced.transform.Find("Inventory");
        inventory.GetComponent<RotateWithHMD>().Offset = new Vector3(0f, 0.12f, 0.17f);

        GameObject holsterGO = inventory.Find(holsterName).gameObject;
        SnapZone holsterSnap = holsterGO.GetComponent<SnapZone>();
        holsterSnap.DuplicateItemOnGrab = true;

        EggHolster eggHolster = holsterGO.AddComponent<EggHolster>();
        eggHolster.holster = holsterSnap;
        egg.transform.localScale = new Vector3(3f, 3f, 3f);
        eggHolster.eggPrefab = egg;
        eggHolster.AddEgg();
    }


    public void OnQuitClicked()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        Debug.Log("Connected to server ? " + PhotonNetwork.IsConnectedAndReady);

        Invoke("QuitApplication", 2);
    }
    void QuitApplication()
    {
        Application.Quit();
    }
}



