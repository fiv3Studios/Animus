using UnityEngine;
using System.Collections;

public class networkSlave : MonoBehaviour {
	
	private GameObject worldSpawner;
	public Vector3 cur;
	private Vector3 lasCur;
	private Vector3 tmp;
	//modulus, because c#'s % is remainder :(
	float nfmod(float curval,float maxval) {
		float tmp;
		tmp = curval % maxval;
		if (tmp < 0) {
			tmp += maxval;
		}
    	return tmp;
    }
	
	[RPC]
	void posChange(Vector3 posIn) {
		if (!networkView.isMine) {
			tmp = posIn;
			tmp.x = Mathf.Round((worldSpawner.transform.position.x-tmp.x)/3464.1f)*3464.1f+tmp.x;
			tmp.y = tmp.y;
			tmp.z = Mathf.Round((worldSpawner.transform.position.z-tmp.z)/3000)*3000+tmp.z;
				//get the new version
				//cur.x = Mathf.Round((worldSpawner.transform.position.x-cur.x)/3464.1f);
				//cur.z = Mathf.Round((worldSpawner.transform.position.y-cur.z)/3000);
			transform.position = tmp;
		}
	}
	
	// Use this for initialization
	void Start () {
		worldSpawner = GameObject.FindGameObjectWithTag("worldSpawner");
	}
	
	// Update is called once per frame
	void Update () {
		//if the object belongs to you output your cur
		if (networkView.isMine) {
			cur.x = nfmod(transform.position.x,3464.1f);
			cur.y = transform.position.y;
			cur.z = nfmod(transform.position.z,3000);
			//send that info out
			if (cur != lasCur) {
				lasCur = cur;
				networkView.RPC("posChange", RPCMode.All, cur);//turn off the smoke to the network
			}
		}
	}
	/*
	void Awake() {
		if (networkView.isMine) {
			//enabled = false;	
		} 
	}
	*/
}