using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateRotate : MonoBehaviour {

	public enum _Axis{X, Y, Z}
	public _Axis axis;
	
	public float speed=1;
	public float rotateDuration=2;
	public float cooldown=1;
	
	private float sign=1;
	private float currentCD=.5f;
	private float currentTimer=0;
	
	private Transform thisT;
	
	void Start () { 
		thisT=transform; 
		sign=Random.value<0.5f ? 1 : -1;
		currentTimer=rotateDuration*0.5f;
	}
	
	void Update () {
		currentCD-=Time.deltaTime;
		if(currentCD>0) return;
		
		currentTimer-=Time.deltaTime;
		
		if(currentTimer<=0){
			currentCD=cooldown;
			currentTimer=rotateDuration;
			sign=sign*-1;
		}
		
		if(axis==_Axis.X) thisT.Rotate(Vector3.right*speed*sign, Space.World);
		if(axis==_Axis.Y) thisT.Rotate(Vector3.up*speed*sign, Space.World);
		if(axis==_Axis.Z) thisT.Rotate(Vector3.forward*speed*sign, Space.World);
	}
	
}
