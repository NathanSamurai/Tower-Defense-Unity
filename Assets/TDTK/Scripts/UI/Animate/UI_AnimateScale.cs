using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_AnimateScale : MonoBehaviour {
	
	public float magnitude=.5f;
	
	private float value;
	private Vector3 defaultScale;
	private Transform thisT;
	
	void Start(){
		thisT=transform;
		defaultScale=thisT.localScale;
	}
	
	void Update () {
		value=magnitude*(0.5f+(0.5f*Mathf.Sin(10*Time.realtimeSinceStartup)));
		thisT.localScale=defaultScale+new Vector3(value, value, value);
	}
	
}
