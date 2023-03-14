using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(TowerManager))]
	public class TowerManagerEditor : _TDInspector {

		private TowerManager instance;
		
		public override void Awake(){
			base.Awake();
			instance = (TowerManager)target;
		}
		
		private bool showList=true;
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "TowerManager");
			
			EditorGUILayout.Space();
				
				cont=new GUIContent("Grid Size:", "The size of a single tile on the grid");
				instance.gridSize=EditorGUILayout.FloatField(cont, instance.gridSize);
				
				cont=new GUIContent("Auto Adjust Texture:", "Check to automatically adjust the texture on the platform to fit the grid");
				instance.autoAdjustTextureToGrid=EditorGUILayout.Toggle(cont, instance.autoAdjustTextureToGrid);
				
			EditorGUILayout.Space();
			
				cont=new GUIContent("Free Form Mode:", "Check to enable free form placement of tower, they can be placed anywhere in the world (not limited to platform) as long as the space is not occupied by other towers or obstacle\n\nOnly Applicable for DragNDrop mode");
				instance.freeFormMode=EditorGUILayout.Toggle(cont, instance.freeFormMode);
			
				cont=new GUIContent("Cast For Terrain:", "Check to have the raycast to get a position from terrain object in the scene when trying to look for a build pos.\n\nThis is only useful for DragNDrop mode when the tower need to place on the terrain even there is no valid build point\nHowever you need to make sure the platform are not obstructed by the terrain collider\n\nThis is always true when using free-form mode");
				if(!instance.freeFormMode) instance.raycastForTerrain=EditorGUILayout.Toggle(cont, instance.raycastForTerrain);
				else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
				
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
								flag=EditorGUILayout.Toggle(new GUIContent(" - enabled: ", "check to enable the tower in this level"), flag);
								
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