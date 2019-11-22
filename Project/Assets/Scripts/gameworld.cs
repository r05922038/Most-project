using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

public class gameworld : MonoBehaviour {
	/*game_npc*/
	GameObject player ;
	public float xDirector,zDirector;
	private CharacterMotor characterMoter;
	public bool jump;

	/* data_structure */
	public string robotFileName,obstacleFileName;
	public int numNpc,numObstacle;
	public NPC[] Npcs;
	public Obstacle[] Obstacles;
	public int[,,] Bitmap;//only floor
	public float[,] TerrianHeight;
	public int dataSize,multiple_128;

	/*path*/
	Tnode[] PathT_arr;
	Tnode[] npcPath;
	int[] npcPath_iPathTarr;

	//gui
	public Camera camera2,camera;
	public GameObject projectTarget;
	public Vector2 siteInScreen_npc,siteInScreen_target,siteInData_target;
	public GameObject[] userObject,trajectory;
	public bool[] isPlaced;
	public Texture lose,win,start4_content,Return,start,start1,start2,start3,start4,fullyellow,fullred,smallShoot,smallYellow,middleYellow,largeYellow,smallRed,middleRed,largeRed,smallBlackYellow,middleBlackYellow,largeBlackYellow,smallBlackRed,middleBlackRed,largeBlackRed,catalogue;
	public float speedShoot;
	public float size_sUserObstacle,size_mUserObstacle,size_lUserObstacle,height_Over,height_unOver;
	//	public int[] isRed; 

	//catalogue
	public bool gameStart;

	//userObstacle
	public int numUserObstacle,curBFS_numObstacle,cur_numUserObstacle;

	
	// Use this for initialization
	void Start () {

		gameStart = false;

		xDirector = 0.0f;
		zDirector = 0.0f;
		jump = false;
		collision_inwalk = false;
		this.characterMoter = GetComponent<CharacterMotor> ();
		characterMoter.maxForwardSpeed = 4;
		end = false;
		finish= false;

		robotFileName = "/NPC.txt";
		obstacleFileName = "/Obstacles.txt";
		numUserObstacle=12;
		cur_numUserObstacle = 0;
		multiple_128 = 1;
		dataSize = 128*multiple_128;
		ReadFile();
		Bitmap = new int[Npcs[0].numControlpt, dataSize, dataSize];
		TerrianHeight=new float[dataSize, dataSize];
		Npcs [0].maxJumpHeight = 1f;

		player = GameObject.Find ("Human");
		getBitmap ();
		player.transform.position = new Vector3 (dataToGame(Npcs[0].initSite.Vertice.x), TerrianHeight[Mathf.RoundToInt(Npcs[0].initSite.Vertice.y),Mathf.RoundToInt(Npcs[0].initSite.Vertice.x)], dataToGame(Npcs[0].initSite.Vertice.y));
		Npcs [0].height_toFloor = player.transform.position.y;
		successfulBFS = BFS();
		curBFS_numObstacle = numObstacle;
		smoothing ();

//		projectTarget = GameObject.Find ("projectTarget");
		setProjectTarget ();
		trajectory=new GameObject[20];
		for (int i=0; i<20; i++)
			trajectory [i] = GameObject.Find ("trajectory" + i);
		setTrajectory ();

//		shootReset ();
		speedShoot = 500;
//		isRed=new int[6]{0,0,0,0,0,0};
		siteInScreen_target.x = Screen.width / 2 - 20;
		siteInScreen_target.y = Screen.height*0.65f-40;//Screen.height / 2 - 20;
		isPlaced = new bool[]{false,false,false,false,false,false,false,false,false,false,false,false,};
		siteInScreen_npc = new Vector2 (Screen.width / 2 , 500f);
		userObject = new GameObject[numUserObstacle];
		camera.depth = 0;
		camera2.depth = 0;
		size_sUserObstacle = Npcs [0].radius_Outer+1f ;
		size_mUserObstacle = Npcs [0].radius_Outer * 2;
		size_lUserObstacle = Npcs [0].radius_Outer * 2+1f;
		height_Over = 1.0f;
		height_unOver = 1.5f;

		Catalogue = true;
		Start4_content = false;
		Start3_content = false;
		Start2_content = false;
		Start1_content = false;
	}
	public void ReadFile(){
		/*robot*/
		string path = Application.dataPath + robotFileName;
		if (!File.Exists(path))
			return;
		StreamReader sr = File.OpenText (path);
		string input = ""; 
		string[] nums;
		input = sr.ReadLine ();// # number of robots
		input = sr.ReadLine ();
		numNpc = int.Parse (input);
		Npcs = new NPC[numNpc];
		for(int i=0;i<numNpc;i++)
			Npcs[i]=new NPC();
		for (int a=0; a<numNpc; a++) {
			input = sr.ReadLine ();// # robots # (number)
			input = sr.ReadLine ();// # radius1
			input = sr.ReadLine ();
			Npcs[a].radius_Outer = float.Parse(input)*multiple_128;
			input = sr.ReadLine ();// # radius2
			input = sr.ReadLine ();
			Npcs[a].radius_Inner = float.Parse(input)*multiple_128;
			input = sr.ReadLine ();// # radius3
			input = sr.ReadLine ();
			Npcs[a].radius_Foot = float.Parse(input)*multiple_128;
			input = sr.ReadLine ();// # height_size
			input = sr.ReadLine ();
			Npcs[a].height_Size = float.Parse(input);
			input = sr.ReadLine ();// # height_tofloor
			input = sr.ReadLine ();
			Npcs[a].height_toFloor = float.Parse(input);
			input = sr.ReadLine ();// # maxStephieght
			input = sr.ReadLine ();
			Npcs[a].maxStepHeight = float.Parse(input);
			input = sr.ReadLine ();// # maxSteplenght
			input = sr.ReadLine ();
			Npcs[a].maxStepLength = float.Parse(input)*multiple_128;
			input = sr.ReadLine ();// # initial configuration
			input = sr.ReadLine ();
			nums = input.Split (' ');
			Npcs [a].initSite= new Configuration(new Vector2(Single.Parse (nums [0])*multiple_128,Single.Parse (nums [1])*multiple_128),Single.Parse (nums [2]));
			input = sr.ReadLine ();// # goal configuration
			input = sr.ReadLine ();
			nums = input.Split (' ');
			Npcs [a].goalSite= new Configuration(new Vector2(Single.Parse (nums [0])*multiple_128,Single.Parse (nums [1])*multiple_128),Single.Parse (nums [2]));
			input = sr.ReadLine ();// # number of control points
			input = sr.ReadLine ();
			Npcs [a].numControlpt = int.Parse (input);
			Npcs [a].Controlpts = new Vector2 [Npcs [a].numControlpt];
			for (int b=0; b<Npcs[a].numControlpt; b++) {
				input = sr.ReadLine ();// # control point # (number)
				input = sr.ReadLine ();
				nums = input.Split (' ');
				Npcs [a].Controlpts [b] = new Vector2(Single.Parse (nums [0]),Single.Parse (nums [1]));
			}
		}
		sr.Close(); 
		/*obstacle*/
		path = Application.dataPath + obstacleFileName;
		if (!File.Exists(path))
			return;
		sr = File.OpenText(path);
		input = sr.ReadLine();// # number of obstacles
		input = sr.ReadLine();
		numObstacle = int.Parse(input);
		Obstacles = new Obstacle[numObstacle+numUserObstacle];
		for(int i=0;i<numObstacle;i++)
			Obstacles[i]=new Obstacle();
		for (int a=0; a<numObstacle; a++){
			input = sr.ReadLine();// # obstacles # (number)
			input = sr.ReadLine();// # height_size
			input = sr.ReadLine();
			Obstacles[a].height_Size=float.Parse(input);
			input = sr.ReadLine();// # height_tofloor
			input = sr.ReadLine();
			Obstacles[a].height_toFloor=float.Parse(input);
			input = sr.ReadLine();// # number of polygons
			input = sr.ReadLine();
			Obstacles [a].numPolygon = int.Parse(input);
			Obstacles [a].Polygons = new Polygon[ Obstacles [a].numPolygon ];
			for(int i=0;i<Obstacles [a].numPolygon;i++)
				Obstacles [a].Polygons[i]=new Polygon();
			for (int b=0; b<Obstacles[a].numPolygon; b++){
				input = sr.ReadLine();// # polygons # (number)
				input = sr.ReadLine();// # number of vertices
				input = sr.ReadLine();
				Obstacles [a].Polygons [b].numVertice = int.Parse(input);
				Obstacles [a].Polygons [b].Vertices = new Vector2[ Obstacles [a].Polygons [b].numVertice ];
				input = sr.ReadLine();// # vertices
				for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++){
					input = sr.ReadLine();
					nums = input.Split(' '); 
					Obstacles [a].Polygons [b].Vertices [c].Set(Single.Parse (nums [0])*multiple_128,Single.Parse (nums [1])*multiple_128);
				}
			}
			input = sr.ReadLine();// # initial configuration
			input = sr.ReadLine();
			nums = input.Split(' ');
			Obstacles [a].initSite= new Configuration(new Vector2(Single.Parse (nums [0])*multiple_128,Single.Parse (nums [1])*multiple_128),Single.Parse(nums [2]));
			for (int b=0; b<Obstacles[a].numPolygon; b++){
				for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++)
					Obstacles [a].Polygons [b].Vertices [c] = RotateBy(Obstacles [a].Polygons [b].Vertices [c]-Obstacles [a].initSite.Vertice, Obstacles [a].initSite.Angle)+Obstacles [a].initSite.Vertice;
			}
			
		}
		sr.Close();
	}
	public Vector2 RotateBy (Vector2 v,float angle){
		angle*= Mathf.Deg2Rad;
		var ca = Mathf.Cos(angle);// var -> float
		var sa = Mathf.Sin(angle);
		return new Vector2( (float)(v.x * ca - v.y * sa), (float)(v.x * sa + v.y * ca));
	}
	public void getBitmap(){//only for 1 npc, Bitmap:floor
		int empty = 2*(dataSize-1);
		int obstacle = 2*dataSize-1;
		for (int cpt=0; cpt<Npcs[0].numControlpt; cpt++){
			/*initialize Bitmap*/
			for (int x=0; x< dataSize ; x++){
				for (int y=0; y< dataSize ; y++){
					Bitmap [cpt,x, y] = empty;
					TerrianHeight[x, y]=0;
				}
			}
			/*draw obstacles on Bitmap*/
			int d, basis, x_index, y_index;//edges of the polygon
			float dx, dy;
			int x_max, x_min;//a polygon
			int[,] frame = new int[dataSize, 2];//a polygon
			for (int a=0; a < numObstacle+cur_numUserObstacle; a++){
//				if(Obstacles[a].height_Size+Obstacles[a].height_toFloor <= Npcs[0].height_toFloor+Npcs[0].maxStepHeight)
//					continue;
				for (int b=0; b<Obstacles[a].numPolygon; b++){
					x_min = dataSize;
					x_max = -1;
					for (int i=0; i<dataSize; i++){
						frame [i, 0] = dataSize;//store y_min
						frame [i, 1] = -1;//store y_max
					}
					for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++){//a polygon
						basis = (c == 0) ? (Obstacles [a].Polygons [b].numVertice - 1) : (c - 1);
						d = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(Obstacles [a].Polygons [b].Vertices [c].x - Obstacles [a].Polygons [b].Vertices [basis].x), Mathf.Abs(Obstacles [a].Polygons [b].Vertices [c].y - Obstacles [a].Polygons [b].Vertices [basis].y)));
						dx = ((Obstacles [a].Polygons [b].Vertices [c].x - Obstacles [a].Polygons [b].Vertices [basis].x) / d);
						dy = ((Obstacles [a].Polygons [b].Vertices [c].y - Obstacles [a].Polygons [b].Vertices [basis].y) / d);
						for (int i=0; i<=d; i++){//edges of the polygon
							
							x_index = Mathf.RoundToInt(Obstacles [a].Polygons [b].Vertices [basis].x + i * dx);
							y_index = Mathf.RoundToInt(Obstacles [a].Polygons [b].Vertices [basis].y + i * dy);

							if (frame [x_index, 0] > y_index)
								frame [x_index, 0] = y_index;
							if (frame [x_index, 1] < y_index)
								frame [x_index, 1] = y_index;
							if (x_min > x_index)
								x_min = x_index;
							if (x_max < x_index)
								x_max = x_index;
						}
					}
					for (int x=x_min; x<=x_max; x++){
						for (int y=frame[x,0]; y<=frame[x,1]; y++){
							Bitmap [cpt,y, x] = obstacle;
							if(TerrianHeight[y,x]<Obstacles[a].height_Size+Obstacles[a].height_toFloor)
								TerrianHeight[y,x]=Obstacles[a].height_Size+Obstacles[a].height_toFloor;
						}
					}
				}
			}
			/*potential wave from goal*/
			Vector2 Cpt;
			int[,,] wave = new int[2*dataSize, dataSize * 2, 2];//Li,sucessors,(x,y)
			int[] cost = new int[2*dataSize];//default 0
			int temp;
			float defTerrianHeight;
			Cpt=Npcs[0].Controlpts[cpt];//==goalsite
			Bitmap [cpt,Mathf.FloorToInt(Cpt.y), Mathf.FloorToInt(Cpt.x)] = 0;
			wave [0, 0, 0] = Mathf.FloorToInt(Cpt.x);
			wave [0, 0, 1] = Mathf.FloorToInt(Cpt.y);
			cost[0]=1;
			for (int i=0; (cost[i]>0)&&(i< (2*dataSize-1)); i++){
				for (int j=0; j<cost[i]; j++){
					//up
					y_index = wave [i, j, 1] + 1;
					if (y_index <= (dataSize-1)){
						x_index = wave [i, j, 0];
						defTerrianHeight=Mathf.Abs(TerrianHeight[y_index, x_index]-TerrianHeight[y_index-1, x_index]);
						if (defTerrianHeight<=Npcs[0].maxStepHeight){
							if(Bitmap [cpt,y_index, x_index]==empty||Bitmap [cpt,y_index, x_index]>i + 1){
								Bitmap [cpt,y_index, x_index] = i + 1;
								wave [i + 1, cost[i+1], 0] = x_index;
								wave [i + 1, cost[i+1], 1] = y_index;
								cost[i+1]++;
							}
						}
						else if(defTerrianHeight<=Npcs[0].maxJumpHeight){
							temp=(i+10 < 2*dataSize)?i+10:2*dataSize-1;
							if(Bitmap [cpt,y_index, x_index]==empty||Bitmap [cpt,y_index, x_index]>temp){
								Bitmap [cpt,y_index, x_index] = temp;
								wave [temp, cost[temp], 0] = x_index;
								wave [temp, cost[temp], 1] = y_index;
								cost[temp]++;
							}
						}
					}
					//down
					y_index = wave [i, j, 1] - 1;
					if (y_index >= 0){
						x_index = wave [i, j, 0];
						defTerrianHeight=Mathf.Abs(TerrianHeight[y_index, x_index]-TerrianHeight[y_index+1, x_index]);
						if (defTerrianHeight<=Npcs[0].maxStepHeight){
							if(Bitmap [cpt,y_index, x_index]==empty||Bitmap [cpt,y_index, x_index]>i + 1){
								Bitmap [cpt,y_index, x_index] = i + 1;
								wave [i + 1, cost[i+1], 0] = x_index;
								wave [i + 1, cost[i+1], 1] = y_index;
								cost[i+1]++;
							}
						}
						else if(defTerrianHeight<=Npcs[0].maxJumpHeight){
							temp=(i+10 < 2*dataSize)?i+10:2*dataSize-1;
							if(Bitmap [cpt,y_index, x_index]==empty||Bitmap [cpt,y_index, x_index]>temp){
								Bitmap [cpt,y_index, x_index] = temp;
								wave [temp, cost[temp], 0] = x_index;
								wave [temp, cost[temp], 1] = y_index;
								cost[temp]++;
							}
						}
					}
					//right
					x_index = wave [i, j, 0] + 1;
					if (x_index <= (dataSize-1)){
						y_index = wave [i, j, 1];
						defTerrianHeight=Mathf.Abs(TerrianHeight[y_index, x_index]-TerrianHeight[y_index, x_index-1]);
						if (defTerrianHeight<=Npcs[0].maxStepHeight){
							if(Bitmap [cpt,y_index, x_index]==empty||Bitmap [cpt,y_index, x_index]>i + 1){
								Bitmap [cpt,y_index, x_index] = i + 1;
								wave [i + 1, cost[i+1], 0] = x_index;
								wave [i + 1, cost[i+1], 1] = y_index;
								cost[i+1]++;
							}
						}
						else if(defTerrianHeight<=Npcs[0].maxJumpHeight){
							temp=(i+10 < 2*dataSize)?i+10:2*dataSize-1;
							if(Bitmap [cpt,y_index, x_index]==empty||Bitmap [cpt,y_index, x_index]>temp){
								Bitmap [cpt,y_index, x_index] = temp;
								wave [temp, cost[temp], 0] = x_index;
								wave [temp, cost[temp], 1] = y_index;
								cost[temp]++;
							}
						}
					}
					//left
					x_index = wave [i, j, 0] - 1;
					if (x_index >= 0){
						y_index = wave [i, j, 1];
						defTerrianHeight=Mathf.Abs(TerrianHeight[y_index, x_index]-TerrianHeight[y_index, x_index+1]);
						if (defTerrianHeight<=Npcs[0].maxStepHeight){
							if(Bitmap [cpt,y_index, x_index]==empty||Bitmap [cpt,y_index, x_index]>i + 1){
								Bitmap [cpt,y_index, x_index] = i + 1;
								wave [i + 1, cost[i+1], 0] = x_index;
								wave [i + 1, cost[i+1], 1] = y_index;
								cost[i+1]++;
							}
						}
						else if(defTerrianHeight<=Npcs[0].maxJumpHeight){
							temp=(i+10 < 2*dataSize)?i+10:2*dataSize-1;
							if(Bitmap [cpt,y_index, x_index]==empty||Bitmap [cpt,y_index, x_index]>temp){
								Bitmap [cpt,y_index, x_index] = temp;
								wave [temp, cost[temp], 0] = x_index;
								wave [temp, cost[temp], 1] = y_index;
								cost[temp]++;
							}
						}
					}
				}
			}
		}
	}
	public float cross (Vector2 o,Vector2 a,Vector2 b){
				return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
	}
	public float min_PointToSegament(Vector2 p, Vector2 p1, Vector2 p2){
		Vector2 v = p2 - p1;
		Vector2 v1 = p - p1;
		Vector2 v2 = p - p2;
		if (Vector2.Dot(v, v1) < 0)
			return v1.magnitude;
		if (Vector2.Dot(v, v2) > 0)
			return v2.magnitude;
		return Mathf.Abs (cross (p1,p2,p)) / v.magnitude;
	}
	public bool collision_inWalk (Vector2 v,float cur_npcHeighttoFloor){
		for (int iObstacle=curBFS_numObstacle; iObstacle<numObstacle+cur_numUserObstacle; iObstacle++) {
			for (int iPolygon=0; iPolygon<Obstacles[iObstacle].numPolygon; iPolygon++) {
				for (int iVertice=0,jVertice=Obstacles[iObstacle].Polygons[iPolygon].numVertice-1; iVertice<Obstacles[iObstacle].Polygons[iPolygon].numVertice; jVertice=iVertice++){
					if (min_PointToSegament (v, Obstacles [iObstacle].Polygons [iPolygon].Vertices [iVertice], Obstacles [iObstacle].Polygons [iPolygon].Vertices [jVertice]) < Npcs[0].radius_Outer )
					    	
					    //&& ( (minPointToSegament < Npcs[0].radius_Foot) || (Obstacles [iObstacle].height_Size + Obstacles [iObstacle].height_toFloor-next_npcHeighttoFloor > Npcs [0].maxStepHeight)))
						//(Obstacles [iObstacle].height_Size + Obstacles [iObstacle].height_toFloor-next_npcHeighttoFloor > Npcs [0].maxStepHeight) &&
						//					if (min_PointToSegament (v, Obstacles [iObstacle].Polygons [iPolygon].Vertices [iVertice], Obstacles [iObstacle].Polygons [iPolygon].Vertices [iVertice + 1]) < r && !overcome(v))		
						return true;
				}
//				if (min_PointToSegament (v, Obstacles [iObstacle].Polygons [iPolygon].Vertices [Obstacles [iObstacle].Polygons [iPolygon].numVertice - 1], Obstacles [iObstacle].Polygons [iPolygon].Vertices [0]) < Npcs[0].radius_Outer)
				    //&& ( (minPointToSegament < Npcs[0].radius_Foot) || (Obstacles [iObstacle].height_Size + Obstacles [iObstacle].height_toFloor-next_npcHeighttoFloor > Npcs [0].maxStepHeight)))
					//&& (Obstacles [iObstacle].height_Size + Obstacles [iObstacle].height_toFloor-next_npcHeighttoFloor > Npcs [0].maxStepHeight)
					//				if (min_PointToSegament (v, Obstacles [iObstacle].Polygons [iPolygon].Vertices [Obstacles [iObstacle].Polygons [iPolygon].numVertice - 1], Obstacles [iObstacle].Polygons [iPolygon].Vertices [0]) < r &&!overcome(v))
//					return true;
			}	
		}
		return false;	
	}
	public bool unstable(Vector2 v,float r){//foot
		int down = (Mathf.RoundToInt (v.y - r) < 0) ? 0 : Mathf.RoundToInt (v.y - r);
		int up= (Mathf.RoundToInt (v.y + r) > (dataSize-1)) ? (dataSize-1) : Mathf.RoundToInt (v.y + r);
		int left = (Mathf.RoundToInt (v.x - r) < 0) ? 0 : Mathf.RoundToInt (v.x - r);
		int right = (Mathf.RoundToInt (v.x + r) > (dataSize-1)) ? (dataSize-1) : Mathf.RoundToInt (v.x + r);

		float maxHeight = TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)];
		float minHeight = TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)];

		if (TerrianHeight [down, Mathf.RoundToInt (v.x)] > maxHeight)
			maxHeight = TerrianHeight [down, Mathf.RoundToInt (v.x)];
		if (TerrianHeight [down, Mathf.RoundToInt (v.x)] < minHeight)
			minHeight = TerrianHeight [down, Mathf.RoundToInt (v.x)];

		if (TerrianHeight [up, Mathf.RoundToInt (v.x)] > maxHeight)
			maxHeight = TerrianHeight [up, Mathf.RoundToInt (v.x)];
		if (TerrianHeight [up, Mathf.RoundToInt (v.x)] < minHeight)
			minHeight = TerrianHeight [up, Mathf.RoundToInt (v.x)];

		if (TerrianHeight [Mathf.RoundToInt (v.y), left] > maxHeight)
			maxHeight = TerrianHeight [Mathf.RoundToInt (v.y), left];
		if (TerrianHeight [Mathf.RoundToInt (v.y), left] < minHeight)
			minHeight = TerrianHeight [Mathf.RoundToInt (v.y), left];

		if (TerrianHeight [Mathf.RoundToInt (v.y), right] > maxHeight)
			maxHeight = TerrianHeight [Mathf.RoundToInt (v.y), right];
		if (TerrianHeight [Mathf.RoundToInt (v.y),right] < minHeight)
			minHeight = TerrianHeight [Mathf.RoundToInt (v.y), right];

		if (TerrianHeight [down, left] > maxHeight)
			maxHeight = TerrianHeight [down, left];
		if (TerrianHeight [down, left] < minHeight)
			minHeight = TerrianHeight [down, left];

		if (TerrianHeight [down, right] > maxHeight)
			maxHeight = TerrianHeight [down, right];
		if (TerrianHeight [down, right] < minHeight)
			minHeight = TerrianHeight [down, right];

		if (TerrianHeight [up, left] > maxHeight)
			maxHeight = TerrianHeight [up, left];
		if (TerrianHeight [up, left] < minHeight)
			minHeight = TerrianHeight [up, left];

		if (TerrianHeight [up, right] > maxHeight)
			maxHeight = TerrianHeight [up, right];
		if (TerrianHeight [up, right] < minHeight)
			minHeight = TerrianHeight [up, right];

		if (Mathf.Abs (maxHeight - minHeight) > Npcs [0].maxJumpHeight)//step
						return true;
		return false;
	}
	public bool collision (Vector2 v,float cur_npcHeighttoFloor){//only for 1 npc, only for outer radius
		//boundary
		if (v.x < Npcs[0].radius_Outer || v.x >= (dataSize - Npcs[0].radius_Outer))
			return true;
		if (v.y < Npcs[0].radius_Outer || v.y >= (dataSize - Npcs[0].radius_Outer))
			return true;
		//others
		if(unstable (v, Npcs [0].radius_Foot))
			return true;
		for (int iObstacle=0; iObstacle<numObstacle+cur_numUserObstacle; iObstacle++) {
//			if (Obstacles [iObstacle].height_Size + Obstacles [iObstacle].height_toFloor <= Npcs [0].height_toFloor + Npcs [0].maxStepHeight)
//				continue;
			for (int iPolygon=0; iPolygon<Obstacles[iObstacle].numPolygon; iPolygon++) {
				for (int iVertice=0,jVertice=Obstacles[iObstacle].Polygons[iPolygon].numVertice-1; iVertice<Obstacles[iObstacle].Polygons[iPolygon].numVertice; jVertice=iVertice++){
					if ((min_PointToSegament (v, Obstacles [iObstacle].Polygons [iPolygon].Vertices [iVertice], Obstacles [iObstacle].Polygons [iPolygon].Vertices [jVertice]) < Npcs[0].radius_Outer) 
					    &&(Obstacles [iObstacle].height_Size + Obstacles [iObstacle].height_toFloor-cur_npcHeighttoFloor > Npcs [0].maxJumpHeight))//step
						return true;
				}
//				if ((min_PointToSegament (v, Obstacles [iObstacle].Polygons [iPolygon].Vertices [Obstacles [iObstacle].Polygons [iPolygon].numVertice - 1], Obstacles [iObstacle].Polygons [iPolygon].Vertices [0]) < Npcs[0].radius_Outer )
//					    &&(Obstacles [iObstacle].height_Size + Obstacles [iObstacle].height_toFloor-cur_npcHeighttoFloor > Npcs [0].maxStepHeight))
//					return true;
			}	
		}
		return false;		
	}
/*	public bool overcome(Vector2 v,float r){
//		return Mathf.Abs (TerrianHeight [(int)p.y, (int)p.x] - TerrianHeight [(int)v.y, (int)v.x]) <= Npcs [0].maxStepHeight;
		if (Mathf.Abs (TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)] - TerrianHeight [Mathf.RoundToInt(v.y - r), Mathf.RoundToInt(v.x)]) > Npcs [0].maxStepHeight)
						return false;
		if (Mathf.Abs (TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)] - TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x- r)]) > Npcs [0].maxStepHeight)
			return false;
		if (Mathf.Abs (TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)] - TerrianHeight [Mathf.RoundToInt(v.y + r),Mathf.RoundToInt(v.x)]) > Npcs [0].maxStepHeight)
			return false;
		if (Mathf.Abs (TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)] - TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x+ r)]) > Npcs [0].maxStepHeight)
			return false;
		if (Mathf.Abs (TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)] - TerrianHeight [Mathf.RoundToInt(v.y+ r), Mathf.RoundToInt(v.x+ r)]) > Npcs [0].maxStepHeight)
			return false;
		if (Mathf.Abs (TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)] - TerrianHeight [Mathf.RoundToInt(v.y+ r), Mathf.RoundToInt(v.x- r)]) > Npcs [0].maxStepHeight)
			return false;
		if (Mathf.Abs (TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)] - TerrianHeight [Mathf.RoundToInt(v.y- r), Mathf.RoundToInt(v.x+ r)]) > Npcs [0].maxStepHeight)
			return false;
		if (Mathf.Abs (TerrianHeight [Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.x)] - TerrianHeight [Mathf.RoundToInt(v.y- r), Mathf.RoundToInt(v.x- r)]) > Npcs [0].maxStepHeight)
			return false;
		return true;
	}*/
	public int getU(Vector2 v){
		int u = 0;
		for (int i =0; i<Npcs[0].numControlpt; i++)
			u += Bitmap [i,Mathf.FloorToInt(v.y),Mathf.FloorToInt(v.x)];
		return u;
	}
	public bool successfulBFS;
	public bool BFS(){//only for 1 npc, only for outer radius
		Debug.Log("bfs");
		Vector2 childVertice = new Vector2();
		/*test init_robot collision*/
//		if(collision(Npcs[0].initSite.Vertice,Npcs[0].radius_Outer))
//			return false;
		LinkedList<Tnode> pathT = new LinkedList<Tnode>();
		LinkedList<Tnode>[] Open = new LinkedList<Tnode>[2* dataSize * Npcs [0].numControlpt + 1];
		for (int i=0; i<Open.Length; i++)
			Open [i] = new LinkedList<Tnode>();
		bool[,] Visited = new bool[dataSize, dataSize];
		for (int a=0; a<dataSize; a++){
			for (int b=0; b<dataSize; b++)
					Visited [a, b] = false;
		}
		int tempU, i_child, i_open;
		Tnode child ;
		Tnode cur = new Tnode(0, -1, Npcs [0].initSite.Vertice);
		int i_minOpen = getU(cur.pos);
		Open [i_minOpen].AddFirst(cur);
		pathT.AddFirst(cur);
		Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(cur.pos.y)] = true;
		bool i_valid=false;
		bool Success = false;
		
		curBFS_numObstacle = numObstacle + cur_numUserObstacle;
		while (i_minOpen>=0 && !Success){//i_minOpen<0 : Open is empty
			/* get parent ( cur from Open[] ) */
			cur=Open [i_minOpen].First();
			Open [i_minOpen].RemoveFirst();
			if (Open [i_minOpen].Count() == 0){
				for (i_open=i_minOpen+1; i_open<Open.Length && Open [i_open].Count()==0; i_open++);
				i_minOpen =(i_open == Open.Length)?-1:i_open;
			}
			/* generate 6 childs ( childO to Open[], childT to pathT ) */
			for (i_child=1; i_child<=4; i_child++,i_valid=false){
				switch (i_child){
				case 1:/* x+1 */
					if (Mathf.FloorToInt(cur.pos.x) < (dataSize-1)){
						childVertice.x = cur.pos.x + 1;
						if (!Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(cur.pos.y)]){
							childVertice.y = cur.pos.y;
							i_valid = true;
						}
					}
					break;
				case 2:/* x-1 */
					if (Mathf.FloorToInt(cur.pos.x) > 0){
						childVertice.x = cur.pos.x - 1;
						if (!Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(cur.pos.y)]){
							childVertice.y = cur.pos.y;
							i_valid = true;
						}
					}
					break;
				case 3:/* y+1 */
					if (Mathf.FloorToInt(cur.pos.y) < (dataSize-1)){
						childVertice.y = cur.pos.y + 1;
						if (!Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(childVertice.y)]){
							childVertice.x = cur.pos.x;
							i_valid = true;
						}
					}
					break;
				case 4:/* y-1 */
					if (Mathf.FloorToInt(cur.pos.y) > 0){
						childVertice.y = cur.pos.y - 1;
						if (!Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(childVertice.y)]){
							childVertice.x = cur.pos.x;
							i_valid = true;
						}
					}
					break;
				}
				if (i_valid){   
					Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(childVertice.y)] = true;
					if (!collision(childVertice,TerrianHeight[Mathf.RoundToInt(cur.pos.y),Mathf.RoundToInt(cur.pos.x)])){
						child = new Tnode(pathT.Count, cur.i_pathT, childVertice);
						tempU = getU(child.pos);
						Open [tempU].AddFirst(child);
						pathT.AddLast(child);
						if (tempU < i_minOpen || i_minOpen == -1)
							i_minOpen = tempU;
						
						if (Mathf.Abs(child.pos.x-Npcs[0].goalSite.Vertice.x)< 1f && Mathf.Abs(child.pos.y-Npcs[0].goalSite.Vertice.y)< 1f){
							Success = true;
							child.pos = Npcs [0].goalSite.Vertice;
							break;
						}
					}
				}
			}
		}
		if (Success){
			PathT_arr = new Tnode[pathT.Count];
			pathT.CopyTo(PathT_arr, 0);
			int index = PathT_arr.Length - 1;
			int totalNode_npcPath = 1;
			while (true){
				index = PathT_arr [index].iParent_pathT;
				if (index == -1)
					break;
				totalNode_npcPath++;
			}
			npcPath_iPathTarr=new int[totalNode_npcPath];
			npcPath_iPathTarr[0]=0;
			int i_path=totalNode_npcPath-1;
			index = PathT_arr.Length - 1;
			Tnode p;
			while (true){
				npcPath_iPathTarr[i_path--]=index;//NOT FOR 5
				p = PathT_arr [index];
				index = p.iParent_pathT;
				if (index == -1)
					break;
			}
			return true;
		}
		return false;
	}
	public float dataToGame (float from){
				return from / 4;
	}
	LinkedList<Tnode> recDivide(int head,int tail){		
		LinkedList<Tnode> temp = new LinkedList<Tnode>();		
		if (tail == head){
			temp.AddLast(new Tnode(npcPath [head]));
			return temp;
		}
		if ((tail - head) == 1){
			temp.AddLast(new Tnode(npcPath [head]));
			temp.AddLast(new Tnode(npcPath [tail]));
			return temp;
		}		
		Vector2 deff = npcPath[tail].pos - npcPath [head].pos;
		int maxDeff;
		float xDeff,yDeff;
		maxDeff = (Mathf.Abs(deff.x) >= Mathf.Abs(deff.y)) ? Mathf.CeilToInt(Mathf.Abs(deff.x)) : Mathf.CeilToInt(Mathf.Abs(deff.y));
		if (maxDeff==0){
			temp.AddLast(new Tnode(npcPath [head]));
			return temp;
		}		
		xDeff = deff.x / maxDeff;
		yDeff=deff.y / maxDeff;
		Vector2 tempV,cur;
		int i;
		temp.AddLast(new Tnode(0,0,npcPath [head].pos));
		cur = npcPath [head].pos;
		for (i=1; i<=maxDeff; i++){
			tempV.x=npcPath [head].pos.x+i*xDeff;
			tempV.y=npcPath [head].pos.y+i*yDeff;
			if(collision(tempV,TerrianHeight[Mathf.RoundToInt(cur.y),Mathf.RoundToInt(cur.x)]))
				break;
			temp.AddLast(new Tnode(0,0,tempV));
			cur=tempV;
		}
		if (i > maxDeff){
			return temp;
		} 
		else{
			return concat(recDivide(head,Mathf.FloorToInt((tail-head)/2)+head), recDivide(Mathf.FloorToInt((tail-head)/2+head)+1,tail));
		}
	}
	LinkedList<Tnode> concat(LinkedList<Tnode> list1,LinkedList<Tnode> list2){
		foreach (Tnode item in list2)
			list1.AddLast(item);
		return list1;
	}
	void smoothing(){		
		npcPath=new Tnode[npcPath_iPathTarr.Length];
		for (int i=0; i<npcPath.Length; i++)
			npcPath [i] = PathT_arr [npcPath_iPathTarr [i]];		
		LinkedList<Tnode> tempPath = recDivide(0, npcPath.Length - 1);
		npcPath= new Tnode[tempPath.Count];
		tempPath.CopyTo(npcPath, 0);
		int curLength_tempPath = tempPath.Count;
		int next;
		while (true) {
			tempPath = recDivide (0, npcPath.Length - 1);
			next = tempPath.Count;
			if (next == curLength_tempPath){
				npcPath = new Tnode[tempPath.Count];
				tempPath.CopyTo (npcPath, 0);
				break;
			}
			npcPath = new Tnode[tempPath.Count];
			tempPath.CopyTo (npcPath, 0);
			curLength_tempPath = next;
		}
	}

	public bool collision_inwalk,finish;
	IEnumerator gotoTarget(){//npcPath
		Vector2 cur,next;
		Vector2 director;
		int cur_trajectory=0;
		float curY, preY, exceptTime;
		bool collision = false;
		bool toJump= false;
		cur = npcPath [0].pos;//
		for (int i=0; i<(npcPath.Length-1)&&!end&&!collision; i++) {
//			cur = npcPath [i].pos;
			if(i+2<=npcPath.Length-1 && 
			   TerrianHeight[Mathf.RoundToInt(npcPath [i+2].pos.y),Mathf.RoundToInt(npcPath [i+2].pos.x)]
			   -TerrianHeight[Mathf.RoundToInt(cur.y),Mathf.RoundToInt(cur.x)]>Npcs[0].maxStepHeight){
				if(collision_inWalk(npcPath [i+1].pos,TerrianHeight[Mathf.RoundToInt(cur.y),Mathf.RoundToInt(cur.x)])
				   ||collision_inWalk(npcPath [i+2].pos,TerrianHeight[Mathf.RoundToInt(npcPath [i+1].pos.y),Mathf.RoundToInt(npcPath [i+1].pos.x)])){
					Npcs[0].initSite.Vertice=cur;
					collision=true;
					break;
				}
				else{
					next=npcPath [i+2].pos;
					i++;
					toJump=true;
				}
			}
			else{
				if(collision_inWalk(npcPath [i+1].pos,TerrianHeight[Mathf.RoundToInt(cur.y),Mathf.RoundToInt(cur.x)])){
					Npcs[0].initSite.Vertice=cur;
					collision=true;
					break;
				}
				else{
					next=npcPath [i+1].pos;
				}
			}
			Npcs [0].height_toFloor=TerrianHeight[Mathf.RoundToInt(cur.y),Mathf.RoundToInt(cur.x)];

			//set trajectory
			if(npcPath.Length-i-5 > 20){
				trajectory[cur_trajectory].transform.position= new Vector3 (dataToGame (npcPath [i + 5 +20].pos.x),
				                                                            TerrianHeight [Mathf.RoundToInt (npcPath [i + 5+20].pos.y), Mathf.RoundToInt (npcPath [i + 5+20].pos.x)],
				                                                            dataToGame (npcPath [i + 5+20].pos.y));
				cur_trajectory++;
				if(cur_trajectory==20)
					cur_trajectory=0;
			}
			else if(npcPath.Length-i-5 > 0){
				trajectory[cur_trajectory].SetActive(false);
				cur_trajectory++;
				if(cur_trajectory==20)
					cur_trajectory=0;
			}

			if(toJump){
				toJump=false;
//				Debug.Log("jump cur/next "+cur+next);
				jump=true;
				while(!characterMoter.jumping)
					yield return new WaitForSeconds(0.00001f);
				jump=false;
				player.transform.position=new Vector3(dataToGame(next.x),player.transform.position.y,dataToGame(next.y));
				//TerrianHeight[Mathf.RoundToInt(next.y),Mathf.RoundToInt(next.x)]
				preY=player.transform.position.y;
				yield return new WaitForSeconds(0.001f);
				curY=player.transform.position.y;
				while(Mathf.Abs(curY-preY)>0.001f){
//					Debug.Log("preY/curY "+preY+"/"+curY);
					preY=curY;
					yield return new WaitForSeconds(0.001f);
					curY=player.transform.position.y;
				}
				Npcs[0].initSite.Vertice.Set(player.transform.position.x*4,player.transform.position.z*4);
				collision=true;
				break;
//				player.transform.position=new Vector3(dataToGame(next.x),TerrianHeight[Mathf.RoundToInt(next.y),Mathf.RoundToInt(next.x)],dataToGame(next.y));
			}
	/*		else if(TerrianHeight[Mathf.RoundToInt(cur.y),Mathf.RoundToInt(cur.x)]-TerrianHeight[Mathf.RoundToInt(next.y),Mathf.RoundToInt(next.x)]>Npcs[0].maxStepHeight){
				//				Debug.Log("jump");
				jump=true;
				while(player.transform.position.y>=TerrianHeight[Mathf.RoundToInt(cur.y),Mathf.RoundToInt(cur.x)])
					yield return new WaitForSeconds(0.00001f);
				jump=false;
				director=(next-cur).normalized;
				xDirector=director.x;
				zDirector=director.y;
				yield return new WaitForSeconds(0.01f);
				xDirector=0;
				zDirector=0;
				//				player.transform.position=new Vector3(dataToGame(next.x/4),player.transform.position.y,dataToGame(next.y/4));
				//				yield return new WaitForSeconds(0.1f);
				//				player.transform.position=new Vector3(dataToGame(next.x),player.transform.position.y,dataToGame(next.y));

				player.transform.position=new Vector3(dataToGame(next.x),player.transform.position.y,dataToGame(next.y));
			}*/
			else{
//				Debug.Log("walk cur/next "+cur+next);
				director=(next-cur).normalized;
				exceptTime=Time.time;
				xDirector=director.x;
				zDirector=director.y;
				//			player.transform.position=new Vector3(dataToGame(cur.x),Npcs [0].height_toFloor,dataToGame(cur.y));
				//wait for npc arrive next
				while(true){
					if(xDirector>0 && zDirector>0){
						if(player.transform.position.x>=dataToGame(next.x) || player.transform.position.z>=dataToGame(next.y))
							break;	
					}
					else if(xDirector==1 && zDirector==0){
						if(player.transform.position.x>=dataToGame(next.x) )
							break;	
					}
					else if(xDirector==0 && zDirector==1){
						if(player.transform.position.z>=dataToGame(next.y) )
							break;	
					}
					else if(xDirector==-1 && zDirector==0){
						if(player.transform.position.x<=dataToGame(next.x) )
							break;	
					}
					else if(xDirector==0 && zDirector==-1){
						if(player.transform.position.z<=dataToGame(next.y) )
							break;	
					}
					else if(xDirector>0 && zDirector<0){
						if(player.transform.position.x>=dataToGame(next.x) || player.transform.position.z<=dataToGame(next.y))
							break;	
					}
					else if(xDirector<0 && zDirector<0){
						if(player.transform.position.x<=dataToGame(next.x) || player.transform.position.z<=dataToGame(next.y))
							break;	
					}else if(xDirector<0 && zDirector>0){
						if(player.transform.position.x<=dataToGame(next.x) || player.transform.position.z>=dataToGame(next.y))
							break;	
					}
					yield return new WaitForSeconds(0.000000000000001f);
					if(Time.time-exceptTime>2.5f){
						player.transform.position = new Vector3 (dataToGame (npcPath [i-1].pos.x),
						                                         TerrianHeight [Mathf.RoundToInt (npcPath [i-1].pos.y), Mathf.RoundToInt (npcPath [i-1].pos.x)],
						                                         dataToGame (npcPath [i-1].pos.y));
						Npcs[0].initSite.Vertice=npcPath [i-1].pos;
						collision=true;
						break;
					}
				}
				xDirector=0;
				zDirector=0;	
			}
//			player.transform.position=new Vector3(dataToGame(next.x),player.transform.position.y,dataToGame(next.y));
			cur.Set(player.transform.position.x*4,player.transform.position.z*4);//
	//		Debug.Log("time"+time);
//			Debug.Log("speed"+characterMoter.maxForwardSpeed);
//			Debug.Log(next);
		}		

		if (!collision&&!end) {
			yield return new WaitForSeconds (0.2f);
			finish=true;
			player.transform.position = new Vector3 (dataToGame (npcPath .Last ().pos.x), player.transform.position.y, dataToGame (npcPath .Last ().pos.y));
		} 
		else if(collision&&!end){
			collision_inwalk = true;
		}
	}
	public void globalMP(){
		getBitmap();
//		showTerrian ();
		if (successfulBFS = BFS ()) {
			smoothing ();
			setTrajectory ();
			StartCoroutine (gotoTarget ());
		}
	}
	public void showBitmap(){
		try{
			string fName="/Bitmap.txt";
			string path = Application.dataPath + fName;
			FileStream aFile = new FileStream(path, FileMode.Create); //FileMode.OpenOrCreate       
			StreamWriter sw = new StreamWriter(aFile);           
			for (int y=(dataSize-1); y>=0; y--){
				//					if(showCoordinate){
				for (int x=0; x<dataSize; x++){
					if(Bitmap [0,y,x]!=(2*dataSize-1)){
						sw.Write("("+x.ToString("D3")+","+y.ToString("D3")+")"+":"+Bitmap [0,y,x].ToString("D3")+" ");
					}else{
						sw.Write("              ");                
					}
				}
				sw.Write("\n");
				/*					}else{
						for (int x=0; x<128; x++){
							if(TerrianHeight [y,x]!=255){
						sw.Write(TerrianHeight [y,x].ToString("#.00")+" ");                      
							}else{
								sw.Write("    ");
							}
						}
						sw.Write("\n");
//					}*/
			}
			
			sw.Close();
		}
		
		catch (IOException ex){            
			Console.WriteLine(ex.Message);            
			Console.ReadLine();            
			return ;            
		}
	}
	public void showTerrian(){
		try{
			string fName="/terrian.txt";
			string path = Application.dataPath + fName;
			FileStream aFile = new FileStream(path, FileMode.Create); //FileMode.OpenOrCreate       
			StreamWriter sw = new StreamWriter(aFile);           
			for (int y=(dataSize-1); y>=0; y--){
				//					if(showCoordinate){
				for (int x=0; x<dataSize; x++){
					if(TerrianHeight [y,x]!=(2*dataSize-1)){
						sw.Write("("+x.ToString("D3")+","+y.ToString("D3")+")"+":"+TerrianHeight [y,x].ToString("#.00")+" ");
					}else{
						sw.Write("              ");                
					}
				}
				sw.Write("\n");
				/*					}else{
						for (int x=0; x<128; x++){
							if(TerrianHeight [y,x]!=255){
						sw.Write(TerrianHeight [y,x].ToString("#.00")+" ");                      
							}else{
								sw.Write("    ");
							}
						}
						sw.Write("\n");
//					}*/
			}
			
			sw.Close();
		}
		
		catch (IOException ex){            
			Console.WriteLine(ex.Message);            
			Console.ReadLine();            
			return ;            
		}
	}
	bool end;
	float time;
	private IEnumerator HandleIt()
	{
		yield return 0;
	}
	// Update is called once per frame
	void Update () {
		//clock
		if (gameStart && !finish && !end && successfulBFS) {
			time = Mathf.RoundToInt (Time.time-realstart);
			if (time == 10) {
				characterMoter.maxForwardSpeed = 6;
			} else if (time == 20) {
				characterMoter.maxForwardSpeed = 8;
			} else if (time == 30) {
				characterMoter.maxForwardSpeed = 9;
			} else if (time == 40) {
				characterMoter.maxForwardSpeed = 11;
			} else if (time == 50) {
				end = true;
			}
		}
		if (gameStart) {
			siteInScreen_target.x+=Input.GetAxis ("Horizontal") * Time.deltaTime*speedShoot;
			siteInScreen_target.y-=Input.GetAxis ("Vertical") * Time.deltaTime*speedShoot;
			if(siteInScreen_target.x<0)
				siteInScreen_target.x=0;
			else if(siteInScreen_target.x>Screen.width-40)
				siteInScreen_target.x=Screen.width-40;
			if(siteInScreen_target.y<Screen.height*0.1f)
				siteInScreen_target.y=Screen.height*0.1f;
			else if(siteInScreen_target.y>Screen.height*0.65f-40)
				siteInScreen_target.y=Screen.height*0.65f-40;
			setProjectTarget ();
		}
		if (collision_inwalk) {
			collision_inwalk = false;
			globalMP ();
		}
	}

	//before addUserObstacle, check cur_numUserObstacle < numUserObstacle
	bool addUserObstacle(float height, float width, float length, Vector2 site_center, float angle){
		int iObstacle = numObstacle + cur_numUserObstacle;

		Obstacles [iObstacle].numPolygon = 1;
		Obstacles [iObstacle].height_Size = height;
		Obstacles [iObstacle].initSite.Vertice = site_center;
		Obstacles [iObstacle].initSite.Angle = angle;
		Obstacles [iObstacle].Polygons=new Polygon[1];
		Obstacles [iObstacle].Polygons [0].numVertice = 4;
		Obstacles [iObstacle].Polygons [0].Vertices = new Vector2[]{
			new Vector2 (site_center.x - width / 2.00f, site_center.y - length / 2.00f),
			new Vector2 (site_center.x - width / 2.00f, site_center.y + length / 2.00f),
			new Vector2 (site_center.x + width / 2.00f, site_center.y + length / 2.00f),
			new Vector2 (site_center.x + width / 2.00f, site_center.y - length / 2.00f),
		};
		//rotate
		//and check boundary
		for (int i=0; i<Obstacles [iObstacle].Polygons [0].numVertice; i++) {
			Obstacles [iObstacle].Polygons [0].Vertices [i] = RotateBy (Obstacles [numObstacle + cur_numUserObstacle].Polygons [0].Vertices [i] - site_center, angle) + site_center;
			if ((Obstacles [iObstacle].Polygons [0].Vertices [i].x < 0 || Obstacles [iObstacle].Polygons [0].Vertices [i].x >= dataSize)
			    ||(Obstacles [iObstacle].Polygons [0].Vertices [i].y < 0 || Obstacles [iObstacle].Polygons [0].Vertices [i].y >= dataSize))
				return false;
		}
		//check no cover goal_site
		//only for rectangle
		float test_angle = 0f;
		Vector2 Basis;
		Vector2 cur=new Vector2(Obstacles [iObstacle].Polygons [0].Vertices [3].x - Npcs [0].goalSite.Vertice.x, Obstacles [iObstacle].Polygons [0].Vertices [3].y - Npcs [0].goalSite.Vertice.y);
		for (int iVertice=0,jVertice=3; iVertice<4; jVertice=iVertice++) {
			if (min_PointToSegament (Npcs [0].goalSite.Vertice, Obstacles [iObstacle].Polygons [0].Vertices [iVertice], Obstacles [iObstacle].Polygons [0].Vertices [jVertice]) <= Npcs [0].radius_Outer)
				return false;
			Basis=cur;
			cur=new Vector2(Obstacles [iObstacle].Polygons [0].Vertices [iVertice].x - Npcs [0].goalSite.Vertice.x, Obstacles [iObstacle].Polygons [0].Vertices [iVertice].y - Npcs [0].goalSite.Vertice.y);
			test_angle += Vector2.Angle(cur, Basis);
		}
		if (Mathf.Abs(test_angle - 360) < 1f)
			return false;

		Obstacles [iObstacle].height_toFloor = TerrianHeight [Mathf.RoundToInt(site_center.y), Mathf.RoundToInt(site_center.x)];

		//show the obstacle in game
		userObject [cur_numUserObstacle] = GameObject.Find ("UserObject"+cur_numUserObstacle);
		userObject [cur_numUserObstacle].transform.position = new Vector3 (dataToGame(site_center.x),
		                                                                   Obstacles [numObstacle + cur_numUserObstacle].height_toFloor + height/2.00f,
		                                                                   dataToGame(site_center.y));
		userObject [cur_numUserObstacle].transform.localScale = new Vector3 (dataToGame(width), height, dataToGame(length));//+0.3f
		userObject [cur_numUserObstacle].transform.Rotate(new Vector3(0f,-angle,0f));
		userObject [cur_numUserObstacle].renderer.material.mainTexture = (height <= Npcs [0].maxJumpHeight)?fullyellow:fullred;

		//change terrianHeight
		int a = numObstacle + cur_numUserObstacle;
		int d, basis, x_index, y_index;//edges of the polygon
		float dx, dy;
		int x_max, x_min;//a polygon
		int[,] frame = new int[dataSize, 2];//a polygon
		for (int b=0; b<Obstacles[a].numPolygon; b++){
			x_min = dataSize;
			x_max = -1;
			for (int i=0; i<dataSize; i++){
				frame [i, 0] = dataSize;//store y_min
				frame [i, 1] = -1;//store y_max
			}
			for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++){//a polygon
				basis = (c == 0) ? (Obstacles [a].Polygons [b].numVertice - 1) : (c - 1);
				d = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(Obstacles [a].Polygons [b].Vertices [c].x - Obstacles [a].Polygons [b].Vertices [basis].x), Mathf.Abs(Obstacles [a].Polygons [b].Vertices [c].y - Obstacles [a].Polygons [b].Vertices [basis].y)));
				dx = ((Obstacles [a].Polygons [b].Vertices [c].x - Obstacles [a].Polygons [b].Vertices [basis].x) / d);
				dy = ((Obstacles [a].Polygons [b].Vertices [c].y - Obstacles [a].Polygons [b].Vertices [basis].y) / d);
				for (int i=0; i<=d; i++){//edges of the polygon
					
					x_index = Mathf.RoundToInt(Obstacles [a].Polygons [b].Vertices [basis].x + i * dx);
					y_index = Mathf.RoundToInt(Obstacles [a].Polygons [b].Vertices [basis].y + i * dy);
					
					if (frame [x_index, 0] > y_index)
						frame [x_index, 0] = y_index;
					if (frame [x_index, 1] < y_index)
						frame [x_index, 1] = y_index;
					if (x_min > x_index)
						x_min = x_index;
					if (x_max < x_index)
						x_max = x_index;
				}
			}
			for (int x=x_min; x<=x_max; x++){
				for (int y=frame[x,0]; y<=frame[x,1]; y++){
					if(TerrianHeight[y,x]<Obstacles[a].height_Size+Obstacles[a].height_toFloor)
						TerrianHeight[y,x]=Obstacles[a].height_Size+Obstacles[a].height_toFloor;
				}
			}
		}
		//for collsion_inWalk() in gotoTarget()
		cur_numUserObstacle++;
		return true;
	}

	void setTrajectory (){
		int max = (npcPath.Length - 5 > 20) ? 20 : (npcPath.Length - 5 > 0) ? npcPath.Length - 5: 0;
		for (int i=0; i<max; i++) {
				trajectory[i].SetActive(true);
				trajectory [i].transform.position = new Vector3 (dataToGame (npcPath [i + 5].pos.x),
				                                                 TerrianHeight [Mathf.RoundToInt (npcPath [i + 5].pos.y), Mathf.RoundToInt (npcPath [i + 5].pos.x)],
				                                                 dataToGame (npcPath [i + 5].pos.y));
		}
		for (int i=max; i<20; i++) 
			trajectory[i].SetActive(false);
	}
	Vector2 screenToData(Vector2 from,float angle){
		from += new Vector2 (20f, 20f);
		Vector2 to=RotateBy (new Vector2(from.x - siteInScreen_npc.x,siteInScreen_npc.y-from.y), angle);
		to.x = to.x * (15.2f/240f) + player.transform.position.x*4f ;
		to.y = to.y * (15.2f/240f) + player.transform.position.z*4f;
		return to;
	}
/*	bool isValidInData(Vector2 v,float width,float length){
		if (v.x - width / 2.00f < 0 || v.x + width / 2.00f > 128)
			return false;
		if (v.y - length / 2.00f < 0 || v.y + length / 2.00f > 128)
			return false;
		return true;
	}*/
/*	void shootReset(){
		siteInScreen_target.x = Screen.width / 2 - 60;
		siteInScreen_target.y = Screen.height / 2 - 60;
		moveShoot = false;	
	}*/
	void setProjectTarget(){
		npcAngle=-player.transform.rotation.eulerAngles.y;
		siteInData_target=screenToData(siteInScreen_target,npcAngle);
		//check if can put obstacles
		if((Mathf.RoundToInt (siteInData_target.y)<0||Mathf.RoundToInt (siteInData_target.y)>=dataSize)
			||(Mathf.RoundToInt (siteInData_target.x)<0||Mathf.RoundToInt (siteInData_target.x)>=dataSize)){
			projectTarget.SetActive(false);
		}
		else{
			projectTarget.SetActive(true);
			projectTarget.transform.position = new Vector3 (dataToGame(siteInData_target.x), TerrianHeight [Mathf.RoundToInt (siteInData_target.y), Mathf.RoundToInt (siteInData_target.x)], dataToGame(siteInData_target.y));
			projectTarget.transform.eulerAngles = new Vector3 (0, -npcAngle, 0);
		}
	}
	public float realstart;
	private GUIStyle guiStyle = new GUIStyle();
	public float npcAngle;
	bool Catalogue,Start4_content,Start3_content,Start2_content,Start1_content;
	void OnGUI() {
		if (Catalogue&&!gameStart)
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height),catalogue);
		if (Catalogue&&!gameStart && GUI.Button (new Rect (50, 450, 150, 80), start)) {
			realstart=Time.time;						
			StartCoroutine (gotoTarget ());
			gameStart = true;
		}
		if (Catalogue&&!gameStart && GUI.Button (new Rect (50, 350, 150, 80), start1)) {

		}
		if (Catalogue&&!gameStart && GUI.Button (new Rect (50, 250, 150, 80), start2)) {
			
		}if (Catalogue&&!gameStart && GUI.Button (new Rect (50, 150, 150, 80), start3)) {
			
		}if (Catalogue&&!gameStart && GUI.Button (new Rect (50, 50, 150, 80), start4)) {
			Catalogue=false;
			Start4_content=true;

		}
		if(Start4_content)
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height),start4_content);
		if(Start4_content && GUI.Button (new Rect (10, Screen.height-65, 100, 55),Return)){
			Start4_content=false;
			Catalogue=true;
		}



//		GUI.Button (new Rect (siteInScreen_npc.x, siteInScreen_npc.y, 30, 30), smallYellow);//test siteInScreen_npc
		if (gameStart) {
						GUI.DrawTexture (new Rect (siteInScreen_target.x, siteInScreen_target.y, 40, 40), smallShoot);
						if (!isPlaced [0] && GUI.Button (new Rect (0, 0, Screen.width / 12f, Screen.height * 0.1f), smallYellow)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_Over, size_sUserObstacle, size_sUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [0] = true;
								}
						}
						if (isPlaced [0] && GUI.Button (new Rect (0, 0, Screen.width / 12f, Screen.height * 0.1f), smallBlackYellow)) {
						}
						if (!isPlaced [1] && GUI.Button (new Rect (Screen.width / 12f, 0, Screen.width / 12f, Screen.height * 0.1f), smallYellow)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_Over, size_sUserObstacle, size_sUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [1] = true;
								}
						}
						if (isPlaced [1] && GUI.Button (new Rect (Screen.width / 12f, 0, Screen.width / 12f, Screen.height * 0.1f), smallBlackYellow)) {
						}
						if (!isPlaced [2] && GUI.Button (new Rect ((Screen.width / 12f) * 2, 0, Screen.width / 12f, Screen.height * 0.1f), middleYellow)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_Over, size_mUserObstacle, size_mUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [2] = true;
								}
						}
						if (isPlaced [2] && GUI.Button (new Rect ((Screen.width / 12f) * 2, 0, Screen.width / 12f, Screen.height * 0.1f), middleBlackYellow)) {
						}
						if (!isPlaced [3] && GUI.Button (new Rect ((Screen.width / 12f) * 3, 0, Screen.width / 12f, Screen.height * 0.1f), middleYellow)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_Over, size_mUserObstacle, size_mUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [3] = true;
								}
						}
						if (isPlaced [3] && GUI.Button (new Rect ((Screen.width / 12f) * 3, 0, Screen.width / 12f, Screen.height * 0.1f), middleBlackYellow)) {
						}
						if (!isPlaced [4] && GUI.Button (new Rect ((Screen.width / 12f) * 4, 0, Screen.width / 12f, Screen.height * 0.1f), largeYellow)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_Over, size_lUserObstacle, size_lUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [4] = true;
								}
						}
						if (isPlaced [4] && GUI.Button (new Rect ((Screen.width / 12f) * 4, 0, Screen.width / 12f, Screen.height * 0.1f), largeBlackYellow)) {
						}
						if (!isPlaced [5] && GUI.Button (new Rect ((Screen.width / 12f) * 5, 0, Screen.width / 12f, Screen.height * 0.1f), largeYellow)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_Over, size_lUserObstacle, size_lUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [5] = true;
								}
						}
						if (isPlaced [5] && GUI.Button (new Rect ((Screen.width / 12f) * 5, 0, Screen.width / 12f, Screen.height * 0.1f), largeBlackYellow)) {
						}
						if (!isPlaced [6] && GUI.Button (new Rect ((Screen.width / 12f) * 6, 0, Screen.width / 12f, Screen.height * 0.1f), largeRed)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_unOver, size_lUserObstacle, size_lUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [6] = true;
								}
						}
						if (isPlaced [6] && GUI.Button (new Rect ((Screen.width / 12f) * 6, 0, Screen.width / 12f, Screen.height * 0.1f), largeBlackRed)) {
			
						}
						if (!isPlaced [7] && GUI.Button (new Rect ((Screen.width / 12f) * 7, 0, Screen.width / 12f, Screen.height * 0.1f), largeRed)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_unOver, size_lUserObstacle, size_lUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [7] = true;
								}
						}
						if (isPlaced [7] && GUI.Button (new Rect ((Screen.width / 12f) * 7, 0, Screen.width / 12f, Screen.height * 0.1f), largeBlackRed)) {
						}
						if (!isPlaced [8] && GUI.Button (new Rect ((Screen.width / 12f) * 8, 0, Screen.width / 12f, Screen.height * 0.1f), middleRed)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_unOver, size_mUserObstacle, size_mUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [8] = true;
								}
						}
						if (isPlaced [8] && GUI.Button (new Rect ((Screen.width / 12f) * 8, 0, Screen.width / 12f, Screen.height * 0.1f), middleBlackRed)) {
						}
						if (!isPlaced [9] && GUI.Button (new Rect ((Screen.width / 12f) * 9, 0, Screen.width / 12f, Screen.height * 0.1f), middleRed)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_unOver, size_mUserObstacle, size_mUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [9] = true;
								}
						}
						if (isPlaced [9] && GUI.Button (new Rect ((Screen.width / 12f) * 9, 0, Screen.width / 12f, Screen.height * 0.1f), middleBlackRed)) {
						}
						if (!isPlaced [10] && GUI.Button (new Rect ((Screen.width / 12f) * 10, 0, Screen.width / 12f, Screen.height * 0.1f), smallRed)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_unOver, size_sUserObstacle, size_sUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [10] = true;
								}
						}
						if (isPlaced [10] && GUI.Button (new Rect ((Screen.width / 12f) * 10, 0, Screen.width / 12f, Screen.height * 0.1f), smallBlackRed)) {
						}
						if (!isPlaced [11] && GUI.Button (new Rect ((Screen.width / 12f) * 11, 0, Screen.width / 12f, Screen.height * 0.1f), smallRed)) {
//			npcAngle=-player.transform.rotation.eulerAngles.y;
//			siteInData_target=screenToData(siteInScreen_target,npcAngle);
								if (cur_numUserObstacle < numUserObstacle && addUserObstacle (height_unOver, size_sUserObstacle, size_sUserObstacle, siteInData_target, npcAngle)) {
										isPlaced [11] = true;
								}
						}
						if (isPlaced [11] && GUI.Button (new Rect ((Screen.width / 12f) * 11, 0, Screen.width / 12f, Screen.height * 0.1f), smallBlackRed)) {
						}
						guiStyle.fontSize = 50;
						guiStyle.normal.textColor = Color.cyan;
						GUI.Label (new Rect (Screen.width - 60, Screen.height * 0.1f, 60, Screen.height * 0.1f), time.ToString (), guiStyle);
				}
		if (gameStart && finish)
			GUI.Label (new Rect (Screen.width-Screen.width*0.8f, Screen.height*0.375f, Screen.width*0.67f, Screen.height/2), lose);
		if(gameStart && ( (end&&!finish) || !successfulBFS ))
		   GUI.Label (new Rect (Screen.width-Screen.width*0.8f, Screen.height*0.375f, Screen.width*0.67f, Screen.height/2), win);
		}
/*	void OnGUI() {
		if (!moveShoot&&GUI.Button (new Rect (Screen.width - 50, 0, 50, 50), smallShoot))
			moveShoot = true;


		if (isRed[0]==0&&moveShoot&&GUI.Button (new Rect (Screen.width - 50, 50, 50, 50), smallYellow)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_Over,size_sUserObstacle,size_sUserObstacle,siteInData_target,npcAngle);
				isRed[0]=1;
				shootReset();
			}
		}
		if (isRed[1]==0&&moveShoot&&GUI.Button (new Rect (Screen.width - 50, 100, 50, 50), smallYellow)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_Over,size_sUserObstacle,size_sUserObstacle,siteInData_target,npcAngle);
				isRed[1]=1;
				shootReset();
			}
		}
		if (isRed[2]==0&&moveShoot&&GUI.Button (new Rect (Screen.width - 50, 150, 50, 50), middleYellow)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_Over,size_mUserObstacle,size_mUserObstacle,siteInData_target,npcAngle);
				isRed[2]=1;
				shootReset();
			}
		}
		if (isRed[3]==0&&moveShoot&&GUI.Button (new Rect (Screen.width - 50, 200, 50, 50), middleYellow)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_Over,size_mUserObstacle,size_mUserObstacle,siteInData_target,npcAngle);
				isRed[3]=1;
				shootReset();
			}
		}
		if (isRed[4]==0&&moveShoot&&GUI.Button (new Rect (Screen.width - 50, 250, 50, 50), largeYellow)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_Over,size_lUserObstacle,size_lUserObstacle,siteInData_target,npcAngle);
				isRed[4]=1;
				shootReset();
			}
		}
		if (isRed[5]==0&&moveShoot&&GUI.Button (new Rect (Screen.width - 50, 300, 50, 50), largeYellow)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_Over,size_lUserObstacle,size_lUserObstacle,siteInData_target,npcAngle);
				isRed[5]=1;
				shootReset();
			}
		}

		if(moveShoot)
			GUI.DrawTexture(new Rect(siteInScreen_target.x,siteInScreen_target.y, 120, 120), smallShoot);

		if (isRed [0]==1 && moveShoot && GUI.Button (new Rect (Screen.width - 50, 50, 50, 50), smallRed)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_unOver,size_sUserObstacle,size_sUserObstacle,siteInData_target,npcAngle);
				isRed[0]=2;
				shootReset();
			}
		}
		if (isRed [1]==1 && moveShoot && GUI.Button (new Rect (Screen.width - 50, 100, 50, 50), smallRed)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_unOver,size_sUserObstacle,size_sUserObstacle,siteInData_target,npcAngle);
				isRed[1]=2;
				shootReset();
			}
		}
		if (isRed [2]==1 && moveShoot && GUI.Button (new Rect (Screen.width - 50, 150, 50, 50), middleRed)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_unOver,size_mUserObstacle,size_mUserObstacle,siteInData_target,npcAngle);
				isRed[2]=2;
				shootReset();
			}
		}
		if (isRed [3] ==1&& moveShoot && GUI.Button (new Rect (Screen.width - 50, 200, 50, 50), middleRed)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle){
				addUserObstacle(height_unOver,size_mUserObstacle,size_mUserObstacle,siteInData_target,npcAngle);
				isRed[3]=2;
				shootReset();
			}
		}
		if (isRed [4]==1 && moveShoot && GUI.Button (new Rect (Screen.width - 50, 250, 50, 50), largeRed)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_unOver,size_lUserObstacle,size_lUserObstacle,siteInData_target,npcAngle);
				isRed[4]=2;
				shootReset();
			}
		}
		if (isRed [5]==1 && moveShoot && GUI.Button (new Rect (Screen.width - 50, 300, 50, 50), largeRed)) {
			npcAngle=-player.transform.rotation.eulerAngles.y;
			siteInData_target=screenToData(siteInScreen_target,npcAngle);
			if(cur_numUserObstacle < numUserObstacle ){
				addUserObstacle(height_unOver,size_lUserObstacle,size_lUserObstacle,siteInData_target,npcAngle);
				isRed[5]=2;
				shootReset();
			}
		}

	}*/

}
