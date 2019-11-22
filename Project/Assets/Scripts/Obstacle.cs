using UnityEngine;
using System.Collections;

public struct Obstacle {

    //shape
    public int numPolygon;//fixed
    public Polygon[] Polygons;//fixed / move + rotate

	public float height_Size;
	public float height_toFloor;

    //site
    public Configuration initSite;//fixed

	//no use
//    public Vector2 PosInPlanner;//move

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
