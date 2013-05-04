using UnityEngine;
using System.Collections;

public class physGun : MonoBehaviour {
	
	
	public GameObject cam;
	public playerController enabler;
	public GameObject philip;
	//public float temp;
	GameObject tar;
	float dist;
	Quaternion rot;
	bool on;
	Vector3 local;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton("Fire2")) {
			if (on && tar) {
				tar.rigidbody.isKinematic = true;
				tar.transform.rotation = rot;
				tar = null;
			}
		} else if (Input.GetButton("Fire1")) {
			if (!on) {
				on = true;
				//Debug.Log ("hi");
				RaycastHit hit;
				if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit)) {
					//if (hit.collider.gameObject) {
						tar = hit.collider.gameObject;
						if (tar.tag != "grab") {
							tar = null;
							on = false;
							Network.Instantiate(philip, cam.transform.position+(cam.transform.forward*3), Quaternion.identity, 0);
						} else {
							dist = hit.distance;
							rot = tar.transform.rotation;
							local = hit.transform.InverseTransformPoint(hit.point);
							tar.rigidbody.isKinematic = false;
						}
					//}
				} else {
					on = false;
					Network.Instantiate(philip, cam.transform.position+(cam.transform.forward*3), Quaternion.identity, 0);
				}
			} else {
				Vector3 dir;
				RaycastHit hit;
				if (tar) {
					Vector3 locpoint = tar.transform.TransformPoint(local);
					//temp =  Input.GetAxis("Mouse ScrollWheel");
					dist += Input.GetAxis("Mouse ScrollWheel")*2;
					dir = cam.transform.position+(cam.transform.forward*dist)-locpoint;
					//Physics.Raycast(cam.transform.position, dir, out hit);
					//if (tar.rigidbody.collider.Equals == true) {
						//tar.transform.position = cam.transform.position+(cam.transform.forward*dist);
					//if (hit.distance > dir.magnitude ||  dir.magnitude < 0.1) {
					tar.rigidbody.velocity = dir*15;
					tar.rigidbody.angularVelocity = new Vector3(0,0,0);
					tar.transform.rotation = rot;
					//}
					//}
					if (Input.GetButton("physRotate")) {
						enabler.allowMouse = false;
						//tar.transform.eulerAngles = new Vector3(tar.transform.eulerAngles.x+Input.GetAxis("Mouse Y"),tar.transform.eulerAngles.y+Input.GetAxis("Mouse X"),tar.transform.eulerAngles.z);
						///rot = rot*quatRotangle(cam.transform.right,Input.GetAxis("Mouse Y"));//tar.transform.rotation;
						tar.transform.RotateAround(locpoint,cam.transform.right,Input.GetAxis("Mouse Y"));
						tar.transform.RotateAround(locpoint,cam.transform.up,-Input.GetAxis("Mouse X"));
						rot = tar.transform.rotation;
						//rot = rot*quatRotangle(cam.transform.up,-Input.GetAxis("Mouse X"));
						//Debug.DrawLine(tar.transform.position,tar.transform.position+cam.transform.up*5,Color.red);
					} else {
						enabler.allowMouse = true;
					}
				}
			}
		} else {
			if (on && tar) {
				enabler.allowMouse = true;
				tar = null;
				on = false;
			} else if (on) {
				on = false;
			}
		}
	}
	
	void Awake() {
		if (!networkView.isMine) {
			enabled = false;//turn off the script if the player object isn't the main player
		} 	
	}
}
