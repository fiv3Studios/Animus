using UnityEngine;
using System.Collections;

public class weaponSystem : MonoBehaviour {
	
	//Needed components
    public int gun_animation;//animation when not using a click
    public int level;//quality of gun
    public int type;//melee, gun
   
    //components for left click
    ////////////////////////////
    //necessary
    public float l_reload_time;
    public int l_animation;
	public int l_airstrike;//is it comming from the sky
	public int l_soundEffect;//The sound effect of the shot
	public int l_muzzleFlash;//the muzzle flash of the fun
	public int l_batDrain;//the ammount of battery drained per reload
	//Type of gun:
	    //bullet
	    public bool l_bullet_use;//to use the bullets or not
		public int l_bullet_clipSize;//the ammount of ammo in the clip
	    public int l_bullet_velocity;//what is the speed of the bullets
		public int l_bullet_acceleration;//the acceleration of the bullet
	    public int l_bullet_spread;//what is the spread of the bullets
	    public int l_bullet_size;//how big are the bullets
		public int l_bullet_model;//The model the gun uses for the bullet
		public int l_bullet_areaEffect;//The area effect of the bullet (shocks anything in 10 feet of bullet)
		//common to all
		public int l_bullet_hitEffect;//The effect when the bullet hits (explosions)
			//knockback, slow, stun, acid, poison, fire, thruster-clamp, teleporter, blind, changes appearance, outputs entity, effects player controls, magnetic, emp time
		public int l_bullet_damageHealth;//how much does it hurt the target
		public int l_bullet_damageHealthCost;//of much of that damage health goes to / comes from you
		public int l_bullet_damageEnemies;//does it effect enemies
		public int l_bullet_damagePlayers;//does it effect players
		public int l_bullet_kickBack;//how much does the gun kickback
		
	    //laser
	    public bool l_laser_use;//to use laser or not
	    public int l_laser_width;//what is the width of the laser beam
	    public int l_laser_texture;//what texture to use for the laser
		public int l_laser_color;//color of beam
		public int l_laser_speed;//speed of the laser
		public int l_laser_maxLength;//The maximum laser the links goes
		public int l_laser_mustConnect;//does it have to stay connected to the gun
		public int l_laser_reflections;//How many times does the laser
		//common to all
		public int l_laser_hitEffect;//The effect when the bullet hits (explosions)
			//knockback, slow, stun, acid, poison, fire, thruster-clamp, teleporter, blind, changes appearance, outputs entity, effects player controls, magnetic
		public int l_laser_damageHealth;//how much does it hurt the target
		public int l_laser_damageHealthCost;//of much of that damage health goes to / comes from you
		public int l_laser_damageEnemies;//does it effect enemies
		public int l_laser_damagePlayers;//does it effect players
		public int l_laser_kickBack;//how much does the gun kickback
	   
	    //lightning
	   	public bool l_lightning_use;//wether or not to use lightning
		public int l_lightning_links;//how many people will it link to
		public int l_lightning_linkDist;//how far each link can be away from another
		public int l_lightning_linkDistLoss;//The ammount of link distance subtracted per link
		public int l_lightning_linkDamageLoss;//The ammount of link damage subtracted per link
		public int l_lightning_color;//the color of the lightning
		public int l_lightning_texture;//The texture of the lightning
		public int l_lightning_chargeTime;//The time to charge the gun to max power
		public int l_lightning_chargePower;//how much charge it gives to an item's battery
		//common to all
		public int l_lightning_hitEffect;//The effect when the bullet hits (explosions)
			//knockback, slow, stun, acid, poison, fire, thruster-clamp, teleporter, blind, changes appearance, outputs entity,  effects player controls, magnetic
		public int l_lightning_damageHealth;//how much does it hurt the target
		public int l_lightning_damageHealthCost;//of much of that damage health goes to / comes from you
		public int l_lightning_damageEnemies;//does it effect enemies
		public int l_lightning_damagePlayers;//does it effect players
		public int l_lightning_kickBack;//how much does the gun kickback
	
	
	    //special (phys gun, grapling hook, forces, conditions, portals, object spawner, platformer)
		//fill in when you get to it
		
	//components for right click
    ////////////////////////////
    //necessary
    public float r_reload_time;
    public int r_animation;
	public int r_airstrike;//is it comming from the sky
	public int r_soundEffect;//The sound effect of the shot
	public int r_muzzleFlash;//the muzzle flash of the fun
	public int r_batDrain;//the ammount of battery drained per reload
	//Type of gun:
	    //bullet
	    public bool r_bullet_use;//to use the bullets or not
		public int r_bullet_clipSize;//the ammount of ammo in the clip
	    public int r_bullet_velocity;//what is the speed of the bullets
		public int r_bullet_acceleration;//the acceleration of the bullet
	    public int r_bullet_spread;//what is the spread of the bullets
	    public int r_bullet_size;//how big are the bullets
		public int r_bullet_model;//The model the gun uses for the bullet
		public int r_bullet_areaEffect;//The area effect of the bullet (shocks anything in 10 feet of bullet)
		//common to all
		public int r_bullet_hitEffect;//The effect when the bullet hits (explosions)
			//knockback, slow, stun, acid, poison, fire, thruster-clamp, teleporter, blind, changes appearance, outputs entity, effects player controls, magnetic, emp time
		public int r_bullet_damageHealth;//how much does it hurt the target
		public int r_bullet_damageHealthCost;//of much of that damage health goes to / comes from you
		public int r_bullet_damageEnemies;//does it effect enemies
		public int r_bullet_damagePlayers;//does it effect players
		public int r_bullet_kickBack;//how much does the gun kickback
		
	    //laser
	    public bool r_laser_use;//to use laser or not
	    public int r_laser_width;//what is the width of the laser beam
	    public int r_laser_texture;//what texture to use for the laser
		public int r_laser_color;//color of beam
		public int r_laser_speed;//speed of the laser
		public int r_laser_maxLength;//The maximum laser the links goes
		public int r_laser_mustConnect;//does it have to stay connected to the gun
		public int r_laser_reflections;//How many times does the laser
		//common to all
		public int r_laser_hitEffect;//The effect when the bullet hits (explosions)
			//knockback, slow, stun, acid, poison, fire, thruster-clamp, teleporter, blind, changes appearance, outputs entity, effects player controls, magnetic
		public int r_laser_damageHealth;//how much does it hurt the target
		public int r_laser_damageHealthCost;//of much of that damage health goes to / comes from you
		public int r_laser_damageEnemies;//does it effect enemies
		public int r_laser_damagePlayers;//does it effect players
		public int r_laser_kickBack;//how much does the gun kickback
	   
	    //lightning
	   	public bool r_lightning_use;//wether or not to use lightning
		public int r_lightning_links;//how many people will it link to
		public int r_lightning_linkDist;//how far each link can be away from another
		public int r_lightning_linkDistLoss;//The ammount of link distance subtracted per link
		public int r_lightning_linkDamageLoss;//The ammount of link damage subtracted per link
		public int r_lightning_color;//the color of the lightning
		public int r_lightning_texture;//The texture of the lightning
		public int r_lightning_chargeTime;//The time to charge the gun to max power
		public int r_lightning_chargePower;//how much charge it gives to an item's battery
		//common to all
		public int r_lightning_hitEffect;//The effect when the bullet hits (explosions)
			//knockback, slow, stun, acid, poison, fire, thruster-clamp, teleporter, blind, changes appearance, outputs entity,  effects player controls, magnetic
		public int r_lightning_damageHealth;//how much does it hurt the target
		public int r_lightning_damageHealthCost;//of much of that damage health goes to / comes from you
		public int r_lightning_damageEnemies;//does it effect enemies
		public int r_lightning_damagePlayers;//does it effect players
		public int r_lightning_kickBack;//how much does the gun kickback
	
	
	    //special's secondary (phys gun (freeze), grapling hook (shrink), forces, conditions, portals(second), object spawner, platformer, scope, lockon, camera control)
		//fill in when you get to it
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//left click
		if (Input.GetButton("Fire1")) {
			
		}
	}
}
