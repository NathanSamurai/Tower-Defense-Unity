using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateYAxis : MonoBehaviour {

	public float speed=5;
	private Transform thisT;
	
	void Start () { thisT=transform; }
	
	void Update () {
		thisT.Rotate(Vector3.up*speed*Time.deltaTime*35, Space.World);
	}
	
}
