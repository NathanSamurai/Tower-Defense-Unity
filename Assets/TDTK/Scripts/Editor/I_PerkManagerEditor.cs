using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(PerkManager))]
	public class PerkManagerEditor : _TDInspector {

		private PerkManager instance;
		
		public override void Awake(){
			base.Awake();
			instance = (PerkManager)target;
		}
		
		private bool showList=true;
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "PerkManager");
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Game Scene:", "Check to to indicate if the scene is not an actual game scene\nIntend if the a perk menu scene, purchased perk wont take effect ");
				instance.inGameScene=EditorGUILayout.Toggle(cont, instance.inGameScene);
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Load From Cache:", "Check to load progress from cache if there's one\n\nIf this is the first scene, the specified setting value is used instead");
				instance.loadFromCache=EditorGUILayout.Toggle(cont, instance.loadFromCache);
				cont=new GUIContent("Cache On Loading:", "Check to cache any progress made in this scene whenever another scene is loaded (this include reloading) so that the progress can be loaded and be continued in subsequent scene\n\nProgress cached in 'Cache On Complete' will override this");
				instance.cacheOnRestart=EditorGUILayout.Toggle(cont, instance.cacheOnRestart);
				cont=new GUIContent("Cache On Complete:", "Check to cache any progress made in this scene when the level is completed so that the progress can be loaded and be continued in subsequent scene");
				instance.cacheOnLevelWon=EditorGUILayout.Toggle(cont, instance.cacheOnLevelWon);
			
			EditorGUILayout.Space();
			
			
				GUILayout.BeginHorizontal();
					
					GUILayout.BeginVertical();
						
						EditorGUIUtility.labelWidth+=35;
						cont=new GUIContent("Use RscManager For Cost:", "Check use the resources in RscManager for perk cost");
						instance.useRscManagerForCost=EditorGUILayout.Toggle(cont, instance.useRscManagerForCost);
						EditorGUIUtility.labelWidth-=35;
						
						cont=new GUIContent("Resource:", "The resource used  to cast perk");
						if(instance.useRscManagerForCost) EditorGUILayout.LabelField("Resource:", "-");
						else instance.rsc=EditorGUILayout.IntField(cont, instance.rsc);
						
					GUILayout.EndVertical();
					
					if(!instance.useRscManagerForCost){
						Sprite icon=Perk_DB.GetRscIcon();
						icon=(Sprite)EditorGUILayout.ObjectField(icon, typeof(Sprite), true, GUILayout.Width(40), GUILayout.Height(40));
						Perk_DB.SetRscIcon(icon);
					}
				
				GUILayout.EndHorizontal();
				
			
			EditorGUILayout.Space();
				
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
				showList=EditorGUILayout.Foldout(showList, "Show Perk List");
				EditorGUILayout.EndHorizontal();
				if(showList){
					
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.Space();
						if(GUILayout.Button("EnableAll") && !Application.isPlaying){
							instance.unavailablePrefabIDList=new List<int>();
						}
						if(GUILayout.Button("DisableAll") && !Application.isPlaying){
							instance.unavailablePrefabIDList=Perk_DB.GetPrefabIDList();
						}
						EditorGUILayout.Space();
					EditorGUILayout.EndHorizontal();
						
					EditorGUILayout.Space();
			
					
					List<Perk> perkList=Perk_DB.GetList();
					for(int i=0; i<perkList.Count; i++){
						if(perkList[i].hideInInspector) continue;
						
						Perk perk=perkList[i];
						
						GUILayout.BeginHorizontal();
							
							EditorGUILayout.Space();
						
							GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
							TDE.DrawSprite(GUILayoutUtility.GetLastRect(), perk.icon, perk.desp, false);
							
							GUILayout.BeginVertical();
								EditorGUILayout.Space();
								GUILayout.Label(perk.name, GUILayout.ExpandWidth(false));
								
								GUILayout.BeginHorizontal();
						
									float cachedL=EditorGUIUtility.labelWidth;	EditorGUIUtility.labelWidth=80;
									float cachedF=EditorGUIUtility.fieldWidth;	EditorGUIUtility.fieldWidth=10;
						
									EditorGUI.BeginChangeCheck();
									bool flag=!instance.unavailablePrefabIDList.Contains(perk.prefabID) ? true : false;
									flag=EditorGUILayout.Toggle(new GUIContent(" - enabled: ", "check to enable the perk in this level"), flag);
									
									if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
										if(!flag && !instance.unavailablePrefabIDList.Contains(perk.prefabID)){
											instance.unavailablePrefabIDList.Add(perk.prefabID);
											instance.purchasedPrefabIDList.Remove(perk.prefabID);
										}
										else if(flag) instance.unavailablePrefabIDList.Remove(perk.prefabID);
									}
									
									if(!instance.unavailablePrefabIDList.Contains(perk.prefabID)){
										EditorGUI.BeginChangeCheck();
										flag=instance.purchasedPrefabIDList.Contains(perk.prefabID);
										flag=EditorGUILayout.Toggle(new GUIContent(" - purchased: ", "check to set the perk as purchased right from the start"), flag);
										
										if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
											if(flag) instance.purchasedPrefabIDList.Add(perk.prefabID);
											else instance.purchasedPrefabIDList.Remove(perk.prefabID);
										}
									}
									else{
										EditorGUILayout.LabelField(" - purchased: ", "- ");
									}
									
									EditorGUIUtility.labelWidth=cachedL;	EditorGUIUtility.fieldWidth=cachedF;
									
								GUILayout.EndHorizontal();
								
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