using UnityEngine;
using System.Collections;

public class CharacterMotion : MonoBehaviour {


	public bool canMove = true;
	public Rigidbody rigidBody;
	public int speed;
	public Rigidbody player;
	public GameObject camera;
	public int jumpPower;
	
	public Vector3 inputMoveDirection = new Vector3(0,0,0);
	public bool inputJump;
	public bool jetpackOn;
	public ParticleSystem smoke;
	
	public float jetpackPower;//thrust force of the pack
	public float jetpackFuel;//total fuel that can be held by the pack
	private float currentJetpackFuel;
	public float fuelConsumRate;//rate at which the fuel is used 1 = (1 per second)
	public float ignitionFuel;//amount of fuel needed to ignite main engines
	
	public float STstrength;//the power of the stabalizers
	public float STstrengthOn;//the power of the stabalizers when the pack is on
	public float STfuelRate;//the amount of fuel the stabalizers use per second
	public AudioClip startSound;//sound on startup
	public AudioClip sustainSound;//sound of engines on
	public AudioClip stopSound;//sound of jetpack stopping
	
//	public float maxHeight;
//	private float jetpackPower = 0;
	private float distToGround;
	private bool jumpRelease;
//	private float heightAboveGround;
	private RaycastHit hit;
//	private Vector3 tempVec3;
	
	// Use this for initialization
	void Start () {
		// get the distance to ground
		distToGround = collider.bounds.extents.y;
		//fill the jetpack with fuel
		currentJetpackFuel = jetpackFuel;
	}
	//Pasta-ble
	[RPC]
	void smokeChange(Vector3 onSmoke) {
		if (onSmoke.x != 0) {
			smoke.enableEmission = true;
		} else {
			smoke.enableEmission = false;
		}
	}
	
	void jetpack(bool on) {
		if (on) {
			audio.PlayOneShot(startSound);
			jetpackOn = true;
			currentJetpackFuel -= ignitionFuel;
			//start burst
			player.AddRelativeForce(0,500,0,ForceMode.Acceleration);
			//start the smoke
			networkView.RPC("smokeChange", RPCMode.All, new Vector3(1,0,0));
		} else {
			jetpackOn = false;//turn off the jetpack
			if (audio.isPlaying) {
				audio.Stop();
				audio.PlayOneShot(stopSound);
			}
			//stop the smoke
			networkView.RPC("smokeChange", RPCMode.All, new Vector3(0,0,0));
		}
	}
	
	// Update is called once per frame
	void Update () {
		float dt = Time.deltaTime;
		float jetPower = 4.0f;
		//player.transform.position = new Vector3(player.transform.position.x,player.transform.position.y-0.1f,player.transform.position.z);
		Physics.Raycast(player.transform.position, new Vector3(0,-1,0), out hit);
		
		inputMoveDirection.y = player.velocity.y;
		if (inputJump) {
			//if the player is in the air and presses jump, or if the pack is already on and jump is being held
			if ((hit.distance > distToGround + 0.01f && jumpRelease) || jetpackOn) {
				if (currentJetpackFuel > 0) {//if there is fuel in the tank
					if (jetpackOn) {
						//start the smoke
						networkView.RPC("smokeChange", RPCMode.All, new Vector3(1,0,0));
					}
					currentJetpackFuel -= dt*fuelConsumRate;//remove jetpack fuel at the consume rate
					//send out the key to tell the thruster is on to network
					
					//turn on the justpack (if it is just starting remove the ignition amount)
					if (!jetpackOn) {
						if (currentJetpackFuel > ignitionFuel) {
							jetpack(true);
						} else {
							audio.volume = 0.15f;
							audio.PlayOneShot(stopSound);
						}
					}
					if (!audio.isPlaying && jetpackOn) {
						audio.volume = 0.3f;
						audio.loop = true;
						audio.clip = sustainSound;
						audio.Play();
					}
					if (audio.volume < 0.3f) {
						audio.volume += 0.05f;
					}
					
					jetPower = jetpackPower*dt;
					player.AddForce(camera.transform.up.x*jetPower*2*dt,camera.transform.up.y*jetPower*2*dt,camera.transform.up.z*jetPower*dt,ForceMode.Acceleration);
				} else {//there is no fuel left (turn off thrusters)
					currentJetpackFuel = 0;//make sure it is at 0
					jetpack(false);//jet is off, no fuel
				}
			} else if (jumpRelease) {//if the player is on the ground and the jetpack is cold then just jump
				inputMoveDirection.y = jumpPower;
			} else {//if the player has continued to hold the jump button on a normal jump
				player.AddRelativeForce(0,-9.8f*player.mass*dt,0,ForceMode.Acceleration);
			}
			jumpRelease = false;
		} else {
			if (audio.isPlaying) {
				if (audio.volume > 0.15f) {
					audio.volume -= 0.1f;
				}
			}
			jumpRelease = true;//The jump button has been released
			networkView.RPC("smokeChange", RPCMode.All, new Vector3(0,0,0));//turn off the smoke to the network
			player.AddRelativeForce(0,-9.8f*player.mass*dt,0,ForceMode.Acceleration);//apply gravity like normal
			if (hit.distance < distToGround+0.01f) {//if they are touching the ground
				jetpack(false);
			}
		}
		
		//add to the jetpack fuel
		if (jetpackOn == false) {
			if (currentJetpackFuel < jetpackFuel) {
				currentJetpackFuel += dt;//add one fuel per second
			} else {
				currentJetpackFuel = jetpackFuel;//make sure it cannot exceed the tank
			}
		}
		
		if (hit.distance < distToGround+0.01f) {//if they are touching the ground
			//give them normal movement
			inputMoveDirection.x = (player.velocity.x+inputMoveDirection.x*speed)/2;
			inputMoveDirection.z = (player.velocity.z+inputMoveDirection.z*speed)/2;
		} else {//if they are in the air with no space being held, don't touch the planar movement
			if (new Vector3(inputMoveDirection.x,0,inputMoveDirection.z).magnitude > 0 && currentJetpackFuel > 0) {
				//remove the fuel that the stabalizers use
				currentJetpackFuel -= STfuelRate*dt;
				//add the stabalizers if the player uses them
				if (jetpackOn) {
					player.AddForce(inputMoveDirection.x*STstrengthOn*dt,0,inputMoveDirection.z*STstrengthOn*dt,ForceMode.Acceleration);
				} else {
					player.AddForce(inputMoveDirection.x*STstrength*dt,0,inputMoveDirection.z*STstrength*dt,ForceMode.Acceleration);
				}
			}
			inputMoveDirection.x = player.velocity.x;
			inputMoveDirection.z = player.velocity.z;
		}

		
		player.velocity = inputMoveDirection;
	}
	
	void OnGUI() {
		GUI.Box(new Rect(15,15,210,40),"");
		GUI.Box(new Rect(20,20,190*(currentJetpackFuel/jetpackFuel)+10,30),"");
		//Temporary crosshair for physgun stuff
		GUI.Box(new Rect(Screen.width/2,Screen.height/2,10,10),"");
	}
	
	void Awake() {
		if (!networkView.isMine) {
			rigidbody.detectCollisions = false;
			enabled = false;	
		} 
	}
}

/*
		if (inputJump) {
			Physics.Raycast(player.transform.position, new Vector3(0,-1,0), out hit);
			if (hit.distance < maxHeight) {
				if (jetpackPower < jetpackMaxPower) {
					jetpackPower+=0.1f;
				}
				jetPower = jetpackPower*((maxHeight-hit.distance)/maxHeight);
			} else {
				if (jetpackPower < 0) {
					jetpackPower+=0.1f;
				} else {
					jetpackPower = 0;
				}
				jetPower = jetpackPower;
			}
			inputMoveDirection.x = camera.transform.up.x*jetpackMaxPower*5;
			inputMoveDirection.y = camera.transform.up.y*jetPower;
			inputMoveDirection.z = camera.transform.up.z*jetpackMaxPower*5;
		} else {
			jetpackPower = player.velocity.y;
			inputMoveDirection.x = (player.velocity.x+inputMoveDirection.x*speed)/2;
			inputMoveDirection.z = (player.velocity.z+inputMoveDirection.z*speed)/2;
			inputMoveDirection.y = player.velocity.y;
		}
		
		player.velocity = inputMoveDirection;
		*/