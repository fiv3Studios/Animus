using UnityEngine;
using System.Collections;

public class cameraControl : MonoBehaviour {
	
	public GameObject worldSpawner;
	public Camera cam;
	public AudioListener attachedListen;
	// Use this for initialization
	void Start () {
		if (networkView.isMine) {
			worldSpawner = GameObject.FindGameObjectWithTag("worldSpawner");
			worldSpawner.transform.position = transform.position;
			worldSpawner.transform.parent = transform;
			cam.enabled = true;
			attachedListen.enabled = true;
		} else {
			cam.enabled = false;
			attachedListen.enabled = false;
		}
	}
}
