using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateMove : MonoBehaviour {

	public enum _Axis{X, Y, Z}
	public _Axis axis;
	
	public float speed=5;
	public float magnitude=.5f;
	private Vector3 startingPos;
	private Transform thisT;
	
	void Start () { thisT=transform; startingPos=thisT.localPosition;}
	
	void Update () {
		
		if(axis==_Axis.X) thisT.localPosition=startingPos+new Vector3(magnitude*Mathf.Sin(speed*Time.time), 0, 0);
		if(axis==_Axis.Y) thisT.localPosition=startingPos+new Vector3(0, magnitude*Mathf.Sin(speed*Time.time), 0);
		if(axis==_Axis.Z) thisT.localPosition=startingPos+new Vector3(0, 0, magnitude*Mathf.Sin(speed*Time.time));
		
	}
	
}
