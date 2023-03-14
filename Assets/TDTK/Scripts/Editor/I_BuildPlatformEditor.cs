using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(BuildPlatform))]
	public class BuildPlatformEditor : _TDInspector {

		private BuildPlatform instance;
		
		public override void Awake(){
			base.Awake();
			instance = (BuildPlatform)target;
		}
		
		private bool showList=true;
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "BuildPlatform");
			
			EditorGUILayout.Space();
				
			EditorGUILayout.Space();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
				showList=EditorGUILayout.Foldout(showList, "Show Tower List");
				EditorGUILayout.EndHorizontal();
				if(showList){
					
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.Space();
						if(GUILayout.Button("EnableAll") && !Application.isPlaying){
							instance.unavailablePrefabIDList=new List<int>();
						}
						if(GUILayout.Button("DisableAll") && !Application.isPlaying){
							instance.unavailablePrefabIDList=Tower_DB.GetPrefabIDList();
						}
						EditorGUILayout.Space();
					EditorGUILayout.EndHorizontal();
						
					EditorGUILayout.Space();
			
					
					List<UnitTower> towerList=Tower_DB.GetList();
					for(int i=0; i<towerList.Count; i++){
						if(towerList[i].hideInInspector) continue;
						
						UnitTower tower=towerList[i];
						
						GUILayout.BeginHorizontal();
							
							EditorGUILayout.Space();
						
							GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
							TDE.DrawSprite(GUILayoutUtility.GetLastRect(), tower.icon, tower.desp, false);
							
							GUILayout.BeginVertical();
								EditorGUILayout.Space();
								GUILayout.Label(tower.unitName, GUILayout.ExpandWidth(false));
								
								EditorGUI.BeginChangeCheck();
								bool flag=!instance.unavailablePrefabIDList.Contains(tower.prefabID) ? true : false;
								flag=EditorGUILayout.Toggle(new GUIContent(" - enabled: ", "check to enable the tower type to be build on this platform"), flag);
								
								if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
									if(!flag && !instance.unavailablePrefabIDList.Contains(tower.prefabID))
										instance.unavailablePrefabIDList.Add(tower.prefabID);
									else if(flag) instance.unavailablePrefabIDList.Remove(tower.prefabID);
								}
								
							GUILayout.EndVertical();
						
						GUILayout.EndHorizontal();
						
					}
					
				}
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}