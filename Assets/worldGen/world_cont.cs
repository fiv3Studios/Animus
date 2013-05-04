using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//IMPORTANT- 3464.1

#region classes
public class vox {
	//Draculla is the initial vampire
	public GameObject drac;
	//the gameobject it is "linked" to
	public GameObject zelda;
	//Fuck newtonian physics
	public int dir;
	//The z position the vox is at
	public int zPos;
	//the z scale of the object
	public float scale;
	public bool spawned;
	//the moded world[] pos
	public Vector2 modPos;
	//the REAL world[] pos
	public Vector2 relPos;
	public vox(GameObject inDrac, int inDir, int inZPos, float inScale, bool inSpawned) {
		drac = inDrac;
		dir = inDir;
		zPos = inZPos;
		scale = inScale;
		spawned = inSpawned;
	}
}

public class col {
	public List<vox> cell = new List<vox>();
}

public class tower {
	public bool free;
	public int type;
	public int index;
	public Vector3 pos;
	public tower() {
		free = false;
		type = 0;
	}
}

[System.Serializable]
public class tempGameObject {
	public List<GameObject> obj = new List<GameObject>();
	public bool center;
	public List<int> impact = new List<int>();
}
#endregion

public class world_cont : MonoBehaviour {
	
	#region Variable declarations
	//user settings
	public int renDist;
	
	//used for update
	public bool loaded;
	public int objLoadedPerTick;
	private bool spawnsDone;
	private int updateFrame;
	private Vector3 lasUpdatePos;
	private Vector3 curUpdatePos;
	public int curXDelScan;
	public int curYDelScan;
	
	//basic world variables
	public static int worldSize = 100;
	public static int worldHeight = 100;
	public static int citySize = 60;
	public col[,] world = new col[worldSize,worldSize];
	
	//The different chunks of the world that can be placed
		//basic floor of the world
	public List<tempGameObject> floor = new List<tempGameObject>();
		//City bases
	public List<tempGameObject> city_roads = new List<tempGameObject>();
		//floors of the city buildings
	public List<tempGameObject> city_base = new List<tempGameObject>();
		//tops of buildings
	public List<tempGameObject> city_mid = new List<tempGameObject>();
		//City nessesities
	public List<tempGameObject> city_top = new List<tempGameObject>();
	
	//city variables
	public int novusRadius;
	public int fillStep;
	public List<tower> building_node = new List<tower>();
	public int hold;
	//generation step?
	public int curAction;
	//array to hold voxel plane
	tower[,] city = new tower[citySize,citySize];
	List<Vector2> city_node = new List<Vector2>();
	List<Vector4> city_node_vein = new List<Vector4>();
	//vein counter
	public int city_veinCnt;
	//speed of generation in cycles per tick
	public int city_speed;
	
	//outskirts variables
	//array to hold voxel plane
	tower[,] outskirts = new tower[worldSize,worldSize];
	#endregion
	
	//Initalize the matrix
	private void initalize() {
		//create a base vox to put in every cell
		for (int x = 0; x < worldSize; x++) {
			for (int y = 0; y < worldSize; y++) {
				//in every column put a basePlate
				world[x,y] = new col();
			}
		}
	}
	
	//Make a flat world, of which we can then edit
	private void flatLand() {
		//create a base vox to put in every cell
		for (int x = 0; x < worldSize; x++) {
			for (int y = 0; y < worldSize; y++) {
				//in every column put a basePlate
				world[x,y].cell.Add(new vox(floor[0].obj[0],0,0,1,false));
			}
		}
	}
	
	//debug function
	private void deb() {
		int modx = 13;
		int mody = 12;
		world[modx,mody].cell.Add(new vox(floor[0].obj[0],0,0,1,false));
		
		world[modx+1*(mody%2),mody+1].cell.Add(new vox(floor[0].obj[0],0,0,2,false));
		world[modx+1,mody].cell.Add(new vox(floor[0].obj[0],0,0,3,false));
		world[modx+1*(mody%2),mody-1].cell.Add(new vox(floor[0].obj[0],0,0,4,false));
		world[modx-1*((mody+1)%2),mody-1].cell.Add(new vox(floor[0].obj[0],0,0,5,false));
		world[modx-1,mody].cell.Add(new vox(floor[0].obj[0],0,0,6,false));
		world[modx-1*((mody+1)%2),mody+1].cell.Add(new vox(floor[0].obj[0],0,0,7,false));

	}
	
	//modulus, because c#'s % is remainder :(
	int nfmod(int curval,int maxval) {
		int tmp;
		tmp = curval % maxval;
		if (tmp < 0) {
			tmp += maxval;
		}
    	return tmp;
    }
	
	#region functions to get placement positions
	private bool compareShift(List<int> input1, List<bool> input2, int shift) {
		bool pass;
		pass = true;
		for (int i = 0; i < 6; i++) {
			//Debug.Log (nfmod(i+shift,6));
			if ((input1[i]==1) != input2[nfmod(i+shift,6)] && input1[i] != -1) {
				pass = false;
				break;
			}
		}
		return pass;
	}
	
	private List<bool> getPossible(Vector2 cur) {
		List<bool> compare = new List<bool>();
		int modx = nfmod((int)cur.x,citySize);
		int mody = nfmod((int)cur.y,citySize);
		tower tmp,tow1,tow2,tow3,tow4,tow5,tow6;
		int x, y;
		
		y = nfmod((int)cur.y+1,citySize);
		x = nfmod((int)(cur.x+1*(mody%2)),citySize);
		Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30),new Vector3(34.641f*x+(17.3205f*(y%2)),10,y*30),Color.green);
		tow1 = city[x,y];
		y = nfmod((int)cur.y,citySize);
		x = nfmod((int)cur.x+1,citySize);
		Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30),new Vector3(34.641f*x+(17.3205f*(y%2)),20,y*30),Color.green);
		tow2 = city[x,y];
		y = nfmod((int)cur.y-1,citySize);
		x = nfmod((int)(cur.x+1*(mody%2)),citySize);
		Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30),new Vector3(34.641f*x+(17.3205f*(y%2)),30,y*30),Color.green);
		tow3 = city[x,y];
		
		y = nfmod((int)cur.y-1,citySize);
		x = nfmod((int)(cur.x-1*((mody+1)%2)),citySize);
		Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30),new Vector3(34.641f*x+(17.3205f*(y%2)),40,y*30),Color.green);
		tow4 = city[x,y];
		y = nfmod((int)cur.y,citySize);
		x = nfmod((int)cur.x-1,citySize);
		Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30),new Vector3(34.641f*x+(17.3205f*(y%2)),50,y*30),Color.green);
		tow5 = city[x,y];
		y = nfmod((int)cur.y+1,citySize);
		x = nfmod((int)(cur.x-1*((mody+1)%2)),citySize);
		Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30),new Vector3(34.641f*x+(17.3205f*(y%2)),60,y*30),Color.green);
		tow6 = city[x,y];
		
		compare.Add(false);
		compare.Add(false);
		compare.Add(false);
		compare.Add(false);
		compare.Add(false);
		compare.Add(false);
		
		if (tow1.free) {
			if (tow1.type == 0) {
				compare[0] = true;
			}
		}
		if (tow2.free) {
			if (tow2.type == 0) {
				compare[1] = true;
			}
		}
		if (tow3.free) {
			if (tow3.type == 0) {
				compare[2] = true;
			}
		}
		if (tow4.free) {
			if (tow4.type == 0) {
				compare[3] = true;
			}
		}
		if (tow5.free) {
			if (tow5.type == 0) {
				compare[4] = true;
			}
		}
		if (tow6.free) {
			if (tow6.type == 0) {
				compare[5] = true;
			}
		}
		if (compare[0]) {
			y = nfmod((int)cur.y+1,citySize);
			x = nfmod((int)(cur.x+1*(mody%2)),citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+5),new Vector3(34.641f*x+(17.3205f*(y%2)),10,y*30+5),Color.red);
		} else if (compare[1]) {
			y = nfmod((int)cur.y,citySize);
			x = nfmod((int)cur.x+1,citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+5),new Vector3(34.641f*x+(17.3205f*(y%2)),20,y*30+5),Color.red);
		} else if (compare[2]) {
			y = nfmod((int)cur.y-1,citySize);
			x = nfmod((int)(cur.x+1*(mody%2)),citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+5),new Vector3(34.641f*x+(17.3205f*(y%2)),30,y*30+5),Color.red);
		} else if (compare[3]) {;
			y = nfmod((int)cur.y-1,citySize);
			x = nfmod((int)(cur.x-1*((mody+1)%2)),citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+5),new Vector3(34.641f*x+(17.3205f*(y%2)),40,y*30+5),Color.red);
		} else if (compare[4]) {
			y = nfmod((int)cur.y,citySize);
			x = nfmod((int)cur.x-1,citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+5),new Vector3(34.641f*x+(17.3205f*(y%2)),50,y*30+5),Color.red);
		} else if (compare[5]) {
			y = nfmod((int)cur.y+1,citySize);
			x = nfmod((int)(cur.x-1*((mody+1)%2)),citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+5),new Vector3(34.641f*x+(17.3205f*(y%2)),60,y*30+5),Color.red);
		}
		/*if (dir) {
			//see what directions it can go properly
			for (int i = 0; i < 6; i++) {
				if (compareShift(compare,input,i)) {
					output.Add(i);
				}
			}
			Debug.Log(output.Count);
			return output;
		} else {*/
		return compare;
	}
	
	private List<int> getPossibleDir(List<int> input1, List<bool> input2) {
		List<int> output = new List<int>();
		for (int i = 0; i < 6; i++) {
			if (compareShift (input1, input2, i)) {
				output.Add(i);
			}
		}
		return output;
	}
	
	private void removeFromPool(Vector2 cur, List<int> input, int dir) {
		dir = 6-dir;
		int modx = nfmod((int)cur.x,citySize);
		int mody = nfmod((int)cur.y,citySize);
		int x = 0;
		int y = 0;
		if (input[nfmod(0+dir,6)] == 1) {
			x = nfmod((int)(cur.x+1*(mody%2)),citySize);
			y = nfmod((int)cur.y+1,citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+10),new Vector3(34.641f*x+(17.3205f*(y%2)),10,y*30+10),Color.yellow);
			city[x,y].free = false;
		}
		if (input[nfmod(1+dir,6)] == 1) {
			x = nfmod((int)cur.x+1,citySize);
			y = nfmod((int)cur.y,citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+10),new Vector3(34.641f*x+(17.3205f*(y%2)),20,y*30+10),Color.yellow);
			city[x,y].free = false;
		}
		if (input[nfmod(2+dir,6)] == 1) {
			x = nfmod((int)(cur.x+1*(mody%2)),citySize);
			y = nfmod((int)cur.y-1,citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+10),new Vector3(34.641f*x+(17.3205f*(y%2)),30,y*30+10),Color.yellow);
			city[x,y].free = false;
		}
		
		if (input[nfmod(3+dir,6)] == 1) {
			x = nfmod((int)(cur.x-1*((mody+1)%2)),citySize);
			y = nfmod((int)cur.y-1,citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+10),new Vector3(34.641f*x+(17.3205f*(y%2)),40,y*30+10),Color.yellow);
			city[x,y].free = false;
		}
		if (input[nfmod(4+dir,6)] == 1) {
			x = nfmod((int)cur.x-1,citySize);
			y = nfmod((int)cur.y,citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+10),new Vector3(34.641f*x+(17.3205f*(y%2)),50,y*30+10),Color.yellow);
			city[x,y].free = false;
		}
		if (input[nfmod(5+dir,6)] == 1) {
			x = nfmod((int)(cur.x-1*((mody+1)%2)),citySize);
			y = nfmod((int)cur.y+1,citySize);
			Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(y%2)),0,y*30+10),new Vector3(34.641f*x+(17.3205f*(y%2)),60,y*30+10),Color.yellow);
			city[x,y].free = false;
		}
	}
	#endregion
	
	#region city generation
	private bool genDisk() {
		int modx;
		int mody;
		float dist;
		int temp; //garbage variable (one use only variable)
		int max = (novusRadius-3)*(novusRadius-3);
		//create the main disk
		for (int x = -novusRadius; x < novusRadius; x++) {
			for (int y = -novusRadius; y < novusRadius; y++) {
				city[nfmod(x,citySize),nfmod(y,citySize)] = new tower();
				//if it is within a certain distance make some pannels
				dist = Mathf.Sqrt((x*1.2f)*(x*1.2f)+y*y);
				if (dist < (novusRadius-5)) {
					modx = nfmod(x,worldSize);
					mody = nfmod(y,worldSize);
					//value of the "mountain"
					temp = (int)(novusRadius-dist);
					temp = temp*temp;
					if (dist < novusRadius-8) {
						if (dist >= 2) {
							world[modx,mody].cell.Add(new vox(floor[0].obj[0],0,-20-temp,1+temp,false));
						}
						//world[modx,mody].cell.Add(new vox(floor[0],Quaternion.identity,0,1,false));
					} else {
						world[modx,mody].cell.Add(new vox(floor[0].obj[0],0,-21-temp,20+temp,false));
					}
					if (dist < novusRadius-5) {
						city[nfmod(x,citySize),nfmod(y,citySize)].free = true;
					}
				}
				if (dist < novusRadius) {
					modx = nfmod(x,worldSize);
					mody = nfmod(y,worldSize);
					if (dist > novusRadius-2) {
						world[modx,mody].cell.Add(new vox(floor[0].obj[0],0,-max-20,max-5,false));
					} else {
						//world[modx,mody].cell.Add(new vox(floor[0].obj[0],0,-max-20,1,false));
					}
				}
			}
		}
		//make the seed of the roads
		city_node_vein.Add(new Vector4(1,3,1,0));
		city_node_vein.Add(new Vector4(3,0,2,0));
		city_node_vein.Add(new Vector4(1,-3,3,0));
		city_node_vein.Add(new Vector4(-2,-3,4,0));
		city_node_vein.Add(new Vector4(-3,0,5,0));
		city_node_vein.Add(new Vector4(-2,3,6,0));
		world[0,0].cell.Add(new vox(floor[0].obj[0],0,0,1,false));
		return true;
	}
	
	private bool genRoad(int height) {
		//generate the maze (roads) using the matrix
		//This is based off of a maze generation algorithm
		int modx;
		int mody;
		tower tmp; //garbage variable (one use only variable)
		Vector2 cur;
		Vector4 cur3;
		int adj, ind;
		List<bool> compare = new List<bool>();
		for (int i = 0; i < city_speed; i++) {
			
			//make the veins
			#region Veins
			if (city_node_vein.Count > 0) {
				cur3 = city_node_vein[0];
				city_node_vein.RemoveAt(0);
				modx = nfmod((int)cur3.x,worldSize);
				mody = nfmod((int)cur3.y,worldSize);
				tmp = city[nfmod((int)cur3.x,citySize),nfmod((int)cur3.y,citySize)];
				if (tmp.free && tmp.type != 1) {
					tmp.type = 1;//set it to a "road"
					tmp.index =  world[modx,mody].cell.Count;
					world[modx,mody].cell.Add(new vox(city_roads[0].obj[0],0,height,1,false));
					if (cur3.z == 1) {
						if (cur3.w%city_veinCnt == 0) {
							city_node_vein.Add(new Vector4(cur3.x+1*(mody%2),cur3.y-1,3,1));
						}
						city_node_vein.Add(new Vector4(cur3.x+1*(mody%2),cur3.y+1,cur3.z,cur3.w+1));
						city_node.Add(new Vector2(cur3.x+1,cur3.y));
						city_node.Add(new Vector2(cur3.x+1*(mody%2),cur3.y-1));
						city_node.Add(new Vector2(cur3.x-1,cur3.y));
						city_node.Add(new Vector2(cur3.x-1*((mody+1)%2),cur3.y+1));
					} else if (cur3.z == 2) {
						if (cur3.w%city_veinCnt == 0) {
							city_node_vein.Add(new Vector4(cur3.x-1*((mody+1)%2),cur3.y-1,4,1));
						}
						city_node_vein.Add(new Vector4(cur3.x+1,cur3.y,cur3.z,cur3.w+1));
						city_node.Add(new Vector2(cur3.x+1*(mody%2),cur3.y+1));
						city_node.Add(new Vector2(cur3.x+1*(mody%2),cur3.y-1));
						city_node.Add(new Vector2(cur3.x-1*((mody+1)%2),cur3.y-1));
						city_node.Add(new Vector2(cur3.x-1*((mody+1)%2),cur3.y+1));
					} else if (cur3.z == 3) {
						if (cur3.w%city_veinCnt == 0) {
							city_node_vein.Add(new Vector4(cur3.x-1,cur3.y,5,1));
						}
						city_node_vein.Add(new Vector4(cur3.x+1*(mody%2),cur3.y-1,cur3.z,cur3.w+1));
						city_node.Add(new Vector2(cur3.x+1*(mody%2),cur3.y+1));
						city_node.Add(new Vector2(cur3.x+1,cur3.y));
						city_node.Add(new Vector2(cur3.x-1*((mody+1)%2),cur3.y-1));
						city_node.Add(new Vector2(cur3.x-1,cur3.y));
					} else if (cur3.z == 4) {
						if (cur3.w%city_veinCnt == 0) {
							city_node_vein.Add(new Vector4(cur3.x-1*((mody+1)%2),cur3.y+1,6,1));
						}
						city_node_vein.Add(new Vector4(cur3.x-1*((mody+1)%2),cur3.y-1,cur3.z,cur3.w+1));
						city_node.Add(new Vector2(cur3.x+1,cur3.y));
						city_node.Add(new Vector2(cur3.x+1*(mody%2),cur3.y-1));
						city_node.Add(new Vector2(cur3.x-1,cur3.y));
						city_node.Add(new Vector2(cur3.x-1*((mody+1)%2),cur3.y+1));
					} else if (cur3.z == 5) {
						if (cur3.w%city_veinCnt == 0) {
							city_node_vein.Add(new Vector4(cur3.x+1*(mody%2),cur3.y+1,1,1));
						}
						city_node_vein.Add(new Vector4(cur3.x-1,cur3.y,cur3.z,cur3.w+1));
						city_node.Add(new Vector2(cur3.x+1*(mody%2),cur3.y+1));
						city_node.Add(new Vector2(cur3.x+1*(mody%2),cur3.y-1));
						city_node.Add(new Vector2(cur3.x-1*((mody+1)%2),cur3.y-1));
						city_node.Add(new Vector2(cur3.x-1*((mody+1)%2),cur3.y+1));
					} else if (cur3.z == 6) {
						if (cur3.w%city_veinCnt == 0) {
							city_node_vein.Add(new Vector4(cur3.x+1,cur3.y,2,1));
						}
						city_node_vein.Add(new Vector4(cur3.x-1*((mody+1)%2),cur3.y+1,cur3.z,cur3.w+1));
						city_node.Add(new Vector2(cur3.x+1*(mody%2),cur3.y+1));
						city_node.Add(new Vector2(cur3.x+1,cur3.y));
						city_node.Add(new Vector2(cur3.x-1*((mody+1)%2),cur3.y-1));
						city_node.Add(new Vector2(cur3.x-1,cur3.y));
					}
				}
			}
			#endregion
			#region Maze part of the city
			if (city_node.Count > 0 && city_node_vein.Count == 0) {
				float tempx, tempy;
				ind = (int)(Random.value*(city_node.Count-1));
				cur = city_node[ind];
				city_node.RemoveAt(ind);
				
				tmp = city[nfmod((int)cur.x,citySize),nfmod((int)cur.y,citySize)];
				adj = 0;
				modx = nfmod((int)cur.x,worldSize);
				mody = nfmod((int)cur.y,worldSize);
				
				for (int j = 0; j < city_node.Count; j++) {
					tempx = 34.641f*city_node[j].x+(17.3205f*(mody%2));
					tempy = city_node[j].y*30;
					//Debug.DrawLine(new Vector3(tempx,0,tempy),new Vector3(tempx,50,tempy), Color.green, 2, false);
				}
				
				compare = getPossible(cur);
				//get the adj value of the list
				for (int j = 0; j < 6; j++) {
					if (compare[j] == true) {
						adj++;
					}
				}
				if ((adj >= 5 || (Random.value < 0.10f & adj >= 4)) && adj > 0) {
					tmp.type = 1;//set it to a "road"
					tmp.index =  world[modx,mody].cell.Count;
					world[modx,mody].cell.Add(new vox(city_roads[0].obj[0],0,height,1,false));
					
					//Add the surrounding as nodes
					if (compare[0]) {
						city_node.Add(new Vector2(cur.x+1*(mody%2),cur.y+1));
					}
					if (compare[1]) {
						city_node.Add(new Vector2(cur.x+1,cur.y));
					}
					if (compare[2]) {
						city_node.Add(new Vector2(cur.x+1*(mody%2),cur.y-1));
					}
					if (compare[3]) {
						city_node.Add(new Vector2(cur.x-1*((mody+1)%2),cur.y-1));
					}
					if (compare[4]) {
						city_node.Add(new Vector2(cur.x-1,cur.y));
					}
					if (compare[5]) {
						city_node.Add(new Vector2(cur.x-1*((mody+1)%2),cur.y+1));
					}
				}
			} else if (city_node_vein.Count == 0) {
				return true;
			}
			#endregion
		}
		return false;
	}
	
	private bool genFillRoad() {
		int modx;
		int mody;
		int dir;
		//place the buildings and roads
		List<int> dirPos = new List<int>();
		//replace all of the temp models with the REAL models
		int x = fillStep;
		for (int y = -novusRadius; y < novusRadius; y++) {
			modx = nfmod(x,citySize);
			mody = nfmod(y,citySize);
			if (city[modx,mody].type == 1) {
				for (int t = 1; t < city_roads.Count; t++) {
					dirPos = getPossibleDir(city_roads[t].impact, getPossible(new Vector2(x,y)));
					if (dirPos.Count > 0) {
						world[nfmod(x,worldSize),nfmod(y,worldSize)].cell[city[modx,mody].index].spawned = false;
						world[nfmod(x,worldSize),nfmod(y,worldSize)].cell[city[modx,mody].index].drac = city_roads[t].obj[0];
						world[nfmod(x,worldSize),nfmod(y,worldSize)].cell[city[modx,mody].index].dir =  dirPos[0];
						break;
					}
				}
			}
		}
		if (fillStep >= novusRadius) {
			return true;
		} else {
			fillStep++;
			return false;
		}
	}
	
	private bool genFillBuild(int scan, int chance, int height) {
		int modx;
		int mody;
		int dir;
		int rand, t;
		tower tmp;
		//place the buildings and roads
		List<int> dirPos = new List<int>();
		//replace all of the temp models with the REAL models
		for (int m = 0; m < 10; m++) {
			int x = -fillStep;
			for (int y = -novusRadius; y < novusRadius; y++) {
				modx = nfmod(x,citySize);
				mody = nfmod(y,citySize);
				if (city[modx,mody].free == true && Random.value*100 < chance) {
					rand = 2;//(int)(Random.value*city_base.Count);
					for (int k = 1; k < city_base.Count; k++) {
						t = scan;//q;//nfmod(rand+(city_base.Count-q),city_base.Count);
						if (city_base[t].center == true && city[modx,mody].type == 0) {
							dirPos = getPossibleDir(city_base[t].impact, getPossible(new Vector2(x,y)));
							if (dirPos.Count > 0) {
								dir = (int)Mathf.Floor(Random.value*dirPos.Count);
								world[nfmod(x,worldSize),nfmod(y,worldSize)].cell.Add(new vox(city_base[t].obj[(int)Mathf.Floor(Random.value*city_base[t].obj.Count)],dirPos[dir],height+1,1,false));
								city[modx,mody].type = 1;
								//add it to the nodes
								tmp = new tower();
								tmp.free = true;
								tmp.index = dirPos[dir];
								tmp.type = t;
								tmp.pos = new Vector3(x,y,height+10);
								building_node.Add (tmp);
								
								removeFromPool(new Vector2(x,y),city_base[t].impact, dirPos[dir]);
								break;
								//world[nfmod(x,worldSize),nfmod(y,worldSize)].cell[city[modx,mody].index].drac = city_base[0].obj;
							}
						} else if (city_base[t].center == false && city[modx,mody].type == 1) {
							dirPos = getPossibleDir(city_base[t].impact, getPossible(new Vector2(x,y)));
							if (dirPos.Count > 0) {
								dir = (int)Mathf.Floor(Random.value*dirPos.Count);
								world[nfmod(x,worldSize),nfmod(y,worldSize)].cell.Add(new vox(city_base[t].obj[(int)Mathf.Floor(Random.value*city_base[t].obj.Count)],dirPos[dir],height+1,1,false));
								Debug.DrawLine(new Vector3(34.641f*x+(17.3205f*(mody%2)),0,y*30),new Vector3(34.641f*x+(17.3205f*(mody%2)),5,y*30),Color.red);
								city[modx,mody].type = 1;
								removeFromPool(new Vector2(x,y),city_base[t].impact, dirPos[dir]);
								//add it to the nodes
								tmp = new tower();
								tmp.free = true;
								tmp.index = dirPos[dir];
								tmp.type = t;
								tmp.pos = new Vector3(x,y,height+10);
								building_node.Add (tmp);
								//Debug.Log (dirPos[dir]);
								break;
								//world[nfmod(x,worldSize),nfmod(y,worldSize)].cell[city[modx,mody].index].drac = city_base[0].obj;
							}
						}
					}
					//world[nfmod(x,worldSize),nfmod(y,worldSize)].cell.Add(new vox(city_base[0].obj,0,1,1,false));
					//world[nfmod(x,worldSize),nfmod(y,worldSize)].cell[city[modx,mody].index].drac = city_base[0].obj;
				} else if (city[modx,mody].free == true && city[modx,mody].type == 0) {
					world[nfmod(x,worldSize),nfmod(y,worldSize)].cell.Add(new vox(floor[0].obj[0],0,height,1,false));
					city[modx,mody].type = 1;
				}
			}
			if (fillStep >= novusRadius) {
				return true;
			} else {
				fillStep++;
			}
		}
		return false;
	}
	
	private bool growBuild(int chance, int height, int equ) {
		tower cur;
		tower tmp;
		int tmp2, dist;
		float x, y;
		for (int q = 0; q < 20; q++ ) {
			if (building_node.Count > 0) {
				tmp2 = (int)(Random.value*building_node.Count-1);
				cur = building_node[tmp2];
				building_node.RemoveAt(tmp2);
				world[nfmod((int)cur.pos.x,worldSize),nfmod((int)cur.pos.y,worldSize)].cell.Add(new vox(city_mid[cur.type].obj[(int)Mathf.Floor(Random.value*city_mid[cur.type].obj.Count)],cur.index,1+(int)cur.pos.z,1,false));
				
				x = cur.pos.x;
				y = cur.pos.y;
				if (equ == 0) {
					dist = novusRadius-(int)(Mathf.Sqrt(x*x+y*y));
					dist = dist*dist/3;
				} else {
					dist = (int)(Mathf.Sqrt(x*x+y*y));
					dist = dist*dist/3;
				}
				if (cur.pos.z < height+dist && Random.value*100 < chance) {
					tmp = new tower();
					tmp.free = true;
					tmp.index = cur.index;
					tmp.type = cur.type;
					tmp.pos = new Vector3(cur.pos.x,cur.pos.y,cur.pos.z+10);
					building_node.Add (tmp);
				}
			} else {
				return true;
			}
		}
		return false;
	}
	
	#endregion
	
	bool genWorld() {
		int modx;
		int mody;
		float dist;
		for (int x = -worldSize/2; x < worldSize/2; x++) {
			for (int y = -worldSize/2; y < worldSize/2; y++) {
				modx = nfmod(x,worldSize);
				mody = nfmod(y,worldSize);
				outskirts[modx,mody] = new tower();
				//if it is outside the city make some pannels
				dist = Mathf.Sqrt((x*1.2f)*(x*1.2f)+y*y);
				if (dist < novusRadius) {
					outskirts[modx,mody].free = false;
					//world[modx,mody].cell.Add(new vox(floor[0].obj[0],0,-29,2,false));
				} else {
					world[modx,mody].cell.Add(new vox(floor[0].obj[0],0,-30,1,false));
				}
			}
		}
		return true;
	}
	
	//check and spawn the world
	void spawn() {
		int modx;
		int mody;
		int curSpawned = 0;
		int offX = (int)lasUpdatePos.x;
		int offY = (int)lasUpdatePos.y;
		for (int x = -renDist+offX; x < renDist+offX; x++) {
			for (int y = -renDist+offY; y < renDist+offY; y++) {
				modx = nfmod(x,worldSize);
				mody = nfmod(y,worldSize);
				foreach (vox i in world[modx,mody].cell) {
					if (i.spawned == false) {
						if (objLoadedPerTick < curSpawned) {
							spawnsDone = false;
							return;
						}
						curSpawned++;
						//Debug.Log ("hola");
						i.spawned = true;
						if (i.zelda) {
							Destroy(i.zelda);
						}
						i.modPos = new Vector2(modx,mody);
						i.relPos = new Vector2(x,y);
						i.zelda = (GameObject)Instantiate(i.drac,new Vector3(34.641f*x+(17.3205f*(mody%2)),i.zPos,30*y),Quaternion.identity);
						i.zelda.transform.localScale = new Vector3(1,i.scale,1);
						i.zelda.transform.rotation = Quaternion.Euler(0,i.dir*60,0);
						/*
						if (i.drac == city_base[0].obj[0] || i.drac == city_base[0].obj[1] || i.drac == city_mid[0].obj[0] || i.drac == city_mid[0].obj[1]) {
							i.zelda.renderer.material.color = Color.red;
						} else if (i.drac == city_base[1].obj[0] || i.drac == city_mid[1].obj[0] || i.drac == city_mid[1].obj[1]) {
							i.zelda.renderer.material.color = Color.blue;
						} else if (i.drac == city_base[2].obj[0] || i.drac == city_mid[2].obj[0] || i.drac == city_mid[2].obj[1]) {
							i.zelda.renderer.material.color = Color.green;
						}
						*/
					}
				}
			}
		}
		spawnsDone = true;
	}
	
	void delScan() {
		float dist;
		Vector2 pos;
		int offX = (int)lasUpdatePos.x;
		int offY = (int)lasUpdatePos.y;
		//incriment the scan to the next cell
		curXDelScan++;
		if (curXDelScan >= worldSize) {
			curXDelScan = 0;
			curYDelScan++;
			if (curYDelScan >= worldSize) {
				curYDelScan = 0;
			}
		}
		if (world[curXDelScan,curYDelScan].cell.Count > 0) {
			pos = world[curXDelScan,curYDelScan].cell[0].relPos;
			pos.x = pos.x-offX;
			pos.y = pos.y-offY;
			if (Mathf.Sqrt (pos.x*pos.x+pos.y*pos.y) > renDist+5) {
				foreach (vox i in world[curXDelScan,curYDelScan].cell) {
					if (i.zelda) {
						i.spawned = false;
						Destroy(i.zelda);
					}
				}
			}
		}
	}
	
	//turn the userPosition into matrix reletive
	Vector3 convPos(Vector3 pos) {
		Vector3 tmp;
		tmp.x = (int)(pos.x/34.641f);
		tmp.y = (int)(pos.z/30);
		tmp.z = (int)(pos.y/25);
		return tmp;
	}
	
	// Use this for initialization
	void Start () {
		curAction = 0;
		loaded = false;
		Random.seed = 5;
		//set the frame as 0
		updateFrame = 0;
		//initialize the world with matter and spawn
		initalize();
		//initalize the position
		lasUpdatePos = convPos(transform.position);
		//set the deletion scanner at 0,0
		curYDelScan = 0;
		curXDelScan = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (loaded) {
			updateFrame += 1;
			if (updateFrame > 60 || !spawnsDone) {
				updateFrame = 0;
				curUpdatePos = convPos(transform.position);
				if (curUpdatePos != lasUpdatePos || !spawnsDone) {
					lasUpdatePos = curUpdatePos;
					spawn();
				}
			}
			for (int i = 0; i < 50; i++) {
				delScan();
			}
		} else {
			//Build the world cereal "Now with chunks! :D"
			if (curAction == 0) {
				if (genDisk()) {
					curAction++;
				}
			} else if (curAction == 1) {
				if (genRoad(0)) {
					curAction++;
					fillStep = -novusRadius;
				}
			} else if (curAction == 2) {
				if (genFillRoad()) {
					curAction++;
					fillStep = -novusRadius;
					hold = city_base.Count-1;
				}
			} else if (curAction == 3) {
				if (hold >= 0) {
					if (genFillBuild(hold,80,0)) {
						hold--;
						fillStep = -novusRadius;
					}
				}
				
				if (growBuild(90,0,0) && fillStep <= -novusRadius && hold <= 0) {
					curAction++;
					//Refresh the city matrix
					for (int x = -novusRadius; x < novusRadius; x++) {
						for (int y = -novusRadius; y < novusRadius; y++) {
							city[nfmod(x,citySize),nfmod(y,citySize)] = new tower();
							if (Mathf.Sqrt((x*1.2f)*(x*1.2f)+y*y) <= novusRadius-2) {
								city[nfmod(x,citySize),nfmod(y,citySize)].free = true;
							}
						}
					}
					//add the road seeds
					city_node_vein.Add(new Vector4(1,3,1,0));
					city_node_vein.Add(new Vector4(3,0,2,0));
					city_node_vein.Add(new Vector4(1,-3,3,0));
					city_node_vein.Add(new Vector4(-2,-3,4,0));
					city_node_vein.Add(new Vector4(-3,0,5,0));
					city_node_vein.Add(new Vector4(-2,3,6,0));
				}
			} else if (curAction == 4) {
				if (genRoad(-(novusRadius-3)*(novusRadius-3)-20)) {
					curAction++;
					fillStep = -novusRadius;
				}
			} else if (curAction == 5) {
				if (genFillRoad()) {
					curAction++;
					fillStep = -novusRadius;
					hold = city_base.Count-1;
				}
			} else if (curAction == 6) {
				if (hold >= 0) {
					if (genFillBuild(hold,100,-(novusRadius-3)*(novusRadius-3)-20)) {
						hold--;
						fillStep = -novusRadius;
					}
				}
				
				if (growBuild(90,-(novusRadius-3)*(novusRadius-3)-20,1) && fillStep <= -novusRadius && hold <= 0) {
					curAction++;
				}
			} else if (curAction == 7) {
				if (genWorld ()) {
					curAction++;
				}
			} else {
				//(novusRadius-3)*(novusRadius-3)
				loaded = true;
			}
			spawn();
		}
	}
}
