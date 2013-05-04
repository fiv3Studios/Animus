using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
	
	public GameObject playerPrefab;
	public Transform spawnObject;
	public string gameName = "Animus_test_server";
	private float btnX,btnY,btnW,btnH;
	private bool refreshingHost;
	private HostData[] hostData;
	
	public bool menuOpen;
	private bool menuRel;
	
	// Use this for initialization
	void Start () {
		menuOpen = true;
		menuRel = true;
		btnX = (float)(Screen.width*0.05);
		btnY = (float)(Screen.width*0.05);
		btnW = (float)(Screen.width*0.1);
		btnH = (float)(Screen.width*0.1);
	}
	
	
	//server commands
	void startServer() {
		Network.InitializeServer(32,9001, !Network.HavePublicAddress());
		MasterServer.RegisterHost(gameName, "Animus Test", "Testing animus networking");
	}
	
	void refreshHostList() {
		MasterServer.RequestHostList(gameName);
		refreshingHost = true;
	}
	
	void spawnPlayer() {
		if (Network.Instantiate(playerPrefab, spawnObject.position, Quaternion.identity, 0)) {
			Debug.Log ("Spawning Player");
		}
	}
	
	void disconnect() {
		Network.Disconnect();
        MasterServer.UnregisterHost();
	}
	
	//Messages
	void OnServerInitialized() {
		Debug.Log ("Server Initialized");
		spawnPlayer();
	}
	
	void OnConnectedToServer() {
		spawnPlayer();
	}
	
	void OnMasterServerEvent(MasterServerEvent mse) {
		if (mse == MasterServerEvent.RegistrationSucceeded) {
			Debug.Log ("Registered server");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (refreshingHost) {
			if (MasterServer.PollHostList().Length > 0) {
				refreshingHost = false;
				Debug.Log (MasterServer.PollHostList().Length);
				hostData = MasterServer.PollHostList();
			}
		}
		if (Input.GetButton("menu") && menuRel) {
			Screen.lockCursor = menuOpen;
			menuRel = false;
			menuOpen = !menuOpen;
		} else if (!Input.GetButton("menu") && !menuRel) {
			menuRel = true;
		}
	}
	
	//close the connection
	void OnApplicationQuit(){
    	disconnect();
    }
	
	//GUI
	void OnGUI() {
		if (menuOpen) {//(!Network.isClient && !Network.isServer) {
			if (GUI.Button(new Rect(btnX,btnY,btnW,btnH), "Start Server")) {
				Debug.Log ("Starting Server");	
				startServer();
				menuOpen = false;
				Screen.lockCursor = true;
			}
			if (GUI.Button(new Rect(btnX,(float)(btnY*1.2+btnH),btnW,btnH), "Refresh Hosts")) {
				Debug.Log ("Refreshing");
				refreshHostList();
			}
			if (hostData != null) {
				for (int i = 0; i < hostData.Length; i++) {
					if (GUI.Button(new Rect((float)(btnX*1.5+btnW),(float)(btnY*1.2+(btnH*i)),(float)(btnW*3),(float)(btnH*0.5)), hostData[i].gameName)) {
						Network.Connect (hostData[i]);
						Debug.Log ("Connecting");
						menuOpen = false;
						Screen.lockCursor = true;
					}
				}
			}
		}
	}
}
