using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class Example_PerkMenu_LoadScene : MonoBehaviour {
	
	public string sceneToLoad;
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.R) && sceneToLoad!=""){
			SceneManager.LoadScene(sceneToLoad);
		}
	}
	
}
