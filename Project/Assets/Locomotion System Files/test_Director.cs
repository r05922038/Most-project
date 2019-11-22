using UnityEngine;
using System.Collections;

public class test_Director : MonoBehaviour {

	public float Z=0.30f;
	public float X=0.50f;
	GameObject player = GameObject.Find ("Human");

	// Use this for initialization
	void Start () {
		Z=1.0f;
		X=0.00f;
		Debug.Log (new Vector2 (3, 7).normalized);
	}
	
	// Update is called once per frame
	void Update () {
//		player = GameObject.Find ("Human");
//		player.transform.position = new Vector3 (0.3f, 0.0f, 0.5f);
//		Debug.Log(player.transform.position.x);
//		Z += 0.10f;
//		X += 0.02f;
	}
}
