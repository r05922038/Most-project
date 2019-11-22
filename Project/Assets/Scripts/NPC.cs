using UnityEngine;
using System.Collections;

public class NPC : MonoBehaviour {

	public float radius_Outer;
	public float radius_Inner;
	public float radius_Foot;

	public float height_Size;
	public float height_toFloor;

	public float maxStepHeight;
	public float maxStepLength;
	public float maxJumpHeight;

	//site
	public Configuration initSite;//move + rotate
	public Configuration goalSite;//move + rotate
	
	//potential value
	public int numControlpt;//fixed
	public Vector2[] Controlpts;//fixed

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
