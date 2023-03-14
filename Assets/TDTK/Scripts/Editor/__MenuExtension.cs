using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TDTK {

	public class _MenuExtension : EditorWindow {

		[MenuItem ("Tools/TDTK/New Scene", false, -100)]
		private static void New2 () {
			CreateEmptyScene();
			
			GameObject obj=(GameObject)Instantiate(Resources.Load("NewScenePrefab/TDTK", typeof(GameObject)));
			if(obj==null){
				Debug.LogWarning("Prefab object not found");
				return;
			}
			
			
			GameObject uiObj=(GameObject)Instantiate(Resources.Load("NewScenePrefab/UI", typeof(GameObject)));
			if(uiObj==null){
				Debug.LogWarning("Prefab object not found");
				return;
			}
			
			obj.name="TDTK";
			uiObj.name="UI";
			uiObj.transform.parent=obj.transform;
			
			
			//SpawnManager spawnManager=(SpawnManager)FindObjectOfType(typeof(SpawnManager));
			//if(spawnManager.waveList[0].subWaveList[0].unit==null)
				//spawnManager.waveList[0].subWaveList[0].unit=CreepDB.GetFirstPrefab().gameObject;
		}

		static void CreateEmptyScene(){
			EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
			//EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
			RenderSettings.skybox=null;
			RenderSettings.skybox=(Material)Resources.Load("NewScenePrefab/Skybox", typeof(Material));
			
			RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.ambientLight=new Color(.5f, .5f, .5f, .5f);
		}
		
	}

}