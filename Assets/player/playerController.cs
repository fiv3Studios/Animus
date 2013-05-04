using UnityEngine;
using System.Collections;

public class playerController : MonoBehaviour {
	/*the backbone for the player, it sets all variables*/
	public bool allowMouse;
	bool allowMouseOn;
	public bool allowMotor;
	bool allowMotorOn;
	
	public MouseLook mouse1;
	public MouseLook mouse2;
	public CharacterMotion motor;
	public FPSInputController inputCont;
	public physGun physTool;
	
	void turn_master() {
		mouse1.enabled = true;
		mouse2.enabled = true;
		motor.enabled = true;
		inputCont.enabled = true;
		physTool.enabled = true;
	}
	
	void turn_slave() {
		
	}
	
	// Use this for initialization
	void Start () {
		//on start determin which scripts need to be on
		if (!networkView.isMine) {//if it is inherintly a slave
			turn_slave();
		} else {//he is a master
			turn_master();
		}
		//setup some variables
		allowMouseOn = false;
		allowMotorOn = false;
	}
	
	// Update is called once per frame
	void Update () {
		//allow the mouse or not
		if (allowMouse && !allowMouseOn) {
			allowMouseOn = true;
			mouse1.enabled = true;
			mouse2.enabled = true;
		} else if (!allowMouse && allowMouseOn) {
			allowMouseOn = false;
			mouse1.enabled = false;
			mouse2.enabled = false;
		}
		
		//allow the motion or not
		if (allowMotor && !allowMotorOn) {
			allowMotorOn = true;
			motor.enabled = true;
		} else if (!allowMotor && allowMotorOn) {
			allowMotorOn = false;
			motor.enabled = false;
		}
	}
}
