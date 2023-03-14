using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

namespace TDTK{
	
	[CustomEditor(typeof(UnitTower))] [CanEditMultipleObjects]
	public class I_UnitTowerEditor : _TDInspector {

		private UnitTower instance;
		public override void Awake(){
			base.Awake();
			instance = (UnitTower)target;
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			EditorGUILayout.Space();
			
			if(!EditorApplication.isPlaying){
				#if UNITY_2018_3_OR_NEWER
				
					bool isPrefab=PrefabUtility.GetPrefabAssetType(instance)==PrefabAssetType.Regular;
					bool isInDB=Tower_DB.GetPrefabIDList().Contains(instance.prefabID);
					
					if(isInDB){
						EditorGUILayout.HelpBox("Editing tower using Inspector is not recommended.\nPlease use the editor window instead", MessageType.Info);
						if(GUILayout.Button("Tower Editor Window")) UnitTowerEditorWindow.Init(instance.prefabID);
					}
					else{
						if(instance.prefabID>=0){ instance.prefabID=-1; EditorUtility.SetDirty(instance); }
						
						if(isPrefab){
							EditorGUILayout.Space();
							
							EditorGUILayout.HelpBox("This prefab hasn't been added to database hence it won't be accessible to the game.", MessageType.Warning);
							GUI.color=new Color(1f, 0.7f, .2f, 1f);
							if(GUILayout.Button("Add Prefab to Database")){
								string assetPath=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance);
								GameObject rootObj = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
								UnitTower rootInstance=rootObj.GetComponent<UnitTower>();
								
								UnitTowerEditorWindow.Init();
								UnitTowerEditorWindow.NewItem(rootInstance);
								UnitTowerEditorWindow.Init(rootInstance.prefabID);		//call again to select the instance in editor window
								
								instance.prefabID=rootInstance.prefabID;
							}
							GUI.color=Color.white;
						}
						else{
							string text="Tower object won't be available to be deployed to game, or accessible in TDTK editor until it's made a prefab and added to TDTK database.";
							text+="\n\nYou can still edit the tower using default inspector. However it's not recommended";
							EditorGUILayout.HelpBox(text, MessageType.Warning);
							
							EditorGUILayout.Space();
							if(GUILayout.Button("Tower Editor Window")) UnitTowerEditorWindow.Init(instance.prefabID);
						}
					}
					
				#else
				
					PrefabType type=PrefabUtility.GetPrefabType(instance);
					
					if(type==PrefabType.Prefab || type==PrefabType.PrefabInstance){
						UnitTower prefab=instance;
						if(type==PrefabType.PrefabInstance) prefab=(UnitTower)PrefabUtility.GetPrefabParent(instance);
						bool existInDB=Tower_DB.GetPrefabIndex(prefab)>=0;
						
						if(!existInDB){
							if(instance.prefabID>=0){ Debug.Log("reset prefabID"); instance.prefabID=-1; EditorUtility.SetDirty(instance); }
							
							EditorGUILayout.Space();
							
							EditorGUILayout.HelpBox("This prefab hasn't been added to database hence it won't be accessible to the game.", MessageType.Warning);
							GUI.color=new Color(1f, 0.7f, .2f, 1f);
							if(GUILayout.Button("Add Prefab to Database")){
								UnitTowerEditorWindow.Init();
								UnitTowerEditorWindow.NewItem(instance);
								UnitTowerEditorWindow.Init();		//call again to select the instance in editor window
							}
							GUI.color=Color.white;
						}
						else{
							EditorGUILayout.HelpBox("Editing tower using Inspector is not recommended.\nPlease use the editor window instead", MessageType.Info);
							if(GUILayout.Button("Tower Editor Window")) UnitTowerEditorWindow.Init(instance.prefabID);
						}
						
						EditorGUILayout.Space();
					}
					else{
						if(instance.prefabID>=0){ Debug.Log("reset prefabID"); instance.prefabID=-1; EditorUtility.SetDirty(instance); }
						
						string text="Tower object won't be available to be deployed to game, or accessible in TDTK editor until it's made a prefab and added to TDTK database.";
						text+="\n\nYou can still edit the tower using default inspector. However it's not recommended";
						EditorGUILayout.HelpBox(text, MessageType.Warning);
						
						EditorGUILayout.Space();
						if(GUILayout.Button("Tower Editor Window")) UnitTowerEditorWindow.Init(instance.prefabID);
					}
					
				#endif
			}
			
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}