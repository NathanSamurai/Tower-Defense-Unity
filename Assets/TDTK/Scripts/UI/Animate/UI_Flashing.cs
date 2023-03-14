using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class UI_Flashing : MonoBehaviour {

	public bool inverse;
	public float min=0f;
	public float max=1f;
	
	public float frequency=1;
	private float counter=0;
	
	private CanvasGroup canvasG;
	
	void Awake() {
		canvasG=gameObject.GetComponent<CanvasGroup>();
		if(canvasG==null) canvasG=gameObject.AddComponent<CanvasGroup>();
	}
	
	void Update () {
		counter=(counter+Time.deltaTime)%(1f/frequency);
		if(inverse) canvasG.alpha=Mathf.Lerp(min, max, counter);
		else  canvasG.alpha=Mathf.Lerp(max, min, counter);
	}
	
}
