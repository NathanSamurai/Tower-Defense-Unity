using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectIndicator : MonoBehaviour {

	private ParticleSystem pSystem;
	
	private bool init=false;
	
	void OnEnable(){
		if(!init){
			init=true;
			pSystem=gameObject.GetComponent<ParticleSystem>();
		}
		
		if(pSystem==null) return;
		
		pSystem.Stop();
		
		var main = pSystem.main;
        main.startRotation = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
		
		pSystem.Play();
	}
	
}
