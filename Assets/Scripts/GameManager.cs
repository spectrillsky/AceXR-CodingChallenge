using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks, IInRoomCallbacks, IMatchmakingCallbacks, ILobbyCallbacks, IConnectionCallbacks
{
    public static GameManager Instance;

    #region Config
    [SerializeField] GameObject sceneLoadingScreen;
    [SerializeField] GameObject playerPrefab;
    public const string SCENE_PROP = "scene";
    [SerializeField] private GameSettings settings;
    #endregion

    #region Debug
    public const string TEST_ROOM_NAME = "test-room";
    #endregion

    #region State
    [SerializeField] PlayerController localPlayer;
    [SerializeField] bool changingScenes = false;
    public Scene LocalScene => SceneManager.GetActiveScene();
    #endregion

    void Awake()
    {

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnSceneChanged;

        //Invoke("Connect", 1);
        Connect();
    }

    #region Lobby Management
    void Connect()
    {
        //PhotonNetwork.ConnectToRegion("us");
        PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.ConnectToBestCloudServer();
    }

    #endregion
    #region Scene Management
    public void Server_ChangeScene(string sceneName)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        SetRoomScene(sceneName);
        changingScenes = true;

        PhotonNetwork.LoadLevel(sceneName);
    }

    [PunRPC]
    public void RPC_ChangeScene(string sceneName)
    {
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        //sceneLoadingScreen.SetActive(true);
        if(localPlayer)
            PhotonNetwork.Destroy(localPlayer.photonView);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //sceneLoadingScreen.SetActive(false);
        SetPlayerScene(scene);
        CreatePlayer();
    }

    private void SetRoomScene(string sceneName)
    {
        Hashtable hash = new Hashtable();
        hash[SCENE_PROP] = sceneName;
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    private string GetRoomScene()
    {
        if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SCENE_PROP, out object scene))
            return scene.ToString();
        return null;
    }

    private void SetPlayerScene(Scene scene)
    {
        if (!PhotonNetwork.InRoom) return;

        Hashtable hash = new Hashtable();
        hash[SCENE_PROP] = scene.name;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        if (AllPlayersLoaded())
        {
        }
    }

    private void OnPlayerSynced(Player player)
    {

    }

    private bool AllPlayersLoaded()
    {
        //Way cleaner to build extensions to PUN Player or a dedicated wrapper but okay for now.
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue(SCENE_PROP, out object playerScene))
            {
                Debug.Log($"[Debug] RoomScene: {GetRoomScene()} PlayerScene: {playerScene}");
                if(GetRoomScene() != playerScene.ToString())
                    return false;
            }
            else
                return false;
        }
        return true;
        
    }
    #endregion

    #region Room Management
    public void CreateRoom()
    {
        if (PhotonNetwork.InRoom) return;

        PhotonNetwork.CreateRoom(TEST_ROOM_NAME, GetDefaultRoomOptions(), TypedLobby.Default);
    }

    RoomOptions GetDefaultRoomOptions()
    {
        RoomOptions roomOptions = new RoomOptions();
        Hashtable initialProperties = new Hashtable();
        initialProperties[SCENE_PROP] = settings.DefaultScene.Name;
        roomOptions.CustomRoomProperties = new Hashtable();

        return roomOptions;
    }

    public void JoinRoom()
    {
        if (PhotonNetwork.InRoom) return;

        PhotonNetwork.JoinOrCreateRoom(TEST_ROOM_NAME, GetDefaultRoomOptions(), TypedLobby.Default);
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom) return;

        PhotonNetwork.LeaveRoom();
    }
    #endregion

    void CreatePlayer()
    {
        if (!localPlayer)
            PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
    }

    #region Callbacks
    #region PUN
    #region Room
    void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    void IInRoomCallbacks.OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }

    void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
    }

    void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
    {
    }
    #endregion
    #region Matchmaking
    void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    void IMatchmakingCallbacks.OnCreatedRoom()
    {
        Server_ChangeScene(settings.DefaultScene.Name);
    }

    void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnJoinedRoom()
    {
    }

    void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnLeftRoom()
    {
    }
    #endregion
    #region Lobby
    void ILobbyCallbacks.OnJoinedLobby()
    {
        Debug.Log($"[Lobby] On Joined Lobby");
        JoinRoom();
    }

    void ILobbyCallbacks.OnLeftLobby()
    {
    }

    void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
    {
    }

    void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }
    #endregion

    #region Connection
    void IConnectionCallbacks.OnConnected()
    {
        Debug.Log($"[Connection] On Connected");
    }

    void IConnectionCallbacks.OnConnectedToMaster()
    {
        Debug.Log($"[Connection] On Connected to master");
        PhotonNetwork.JoinLobby();

    }

    void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
    {
    }

    void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
    {
    }
    #endregion
    #endregion
    #region Scene Management
    #endregion
    #endregion
}
