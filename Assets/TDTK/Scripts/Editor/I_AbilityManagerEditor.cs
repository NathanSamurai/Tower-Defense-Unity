using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(AbilityManager))]
	public class AbilityManagerEditor : _TDInspector {

		private AbilityManager instance;
		
		public override void Awake(){
			base.Awake();
			instance = (AbilityManager)target;
		}
		
		private bool showList=true;
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "AbilityManager");
			
			EditorGUILayout.Space();
				
				EditorGUIUtility.labelWidth+=35;
				cont=new GUIContent("Use RscManager For Cost:", "Check use the resources in RscManager for ability cost");
				instance.useRscManagerForCost=EditorGUILayout.Toggle(cont, instance.useRscManagerForCost);
				EditorGUIUtility.labelWidth-=35;
			
			EditorGUILayout.Space();
				
				if(!instance.useRscManagerForCost){
					GUILayout.BeginHorizontal();
					
						GUILayout.BeginVertical();
							EditorGUIUtility.labelWidth+=35;
							cont=new GUIContent("Full Resource On Start:", "Check to have the resource start at full\nOtherwise it will start at whatever value specified");
							instance.startWithFullRsc=EditorGUILayout.Toggle(cont, instance.startWithFullRsc);
							EditorGUIUtility.labelWidth-=35;
						
							cont=new GUIContent("Resource:", "The resource used  to cast ability");
							instance.rsc=EditorGUILayout.IntField(cont, instance.rsc);
							
							cont=new GUIContent("Resource Cap:", "The resource capacity. Resource cannot exceed this value");
							instance.rscCap=EditorGUILayout.IntField(cont, instance.rscCap);
							if(!Application.isPlaying && instance.startWithFullRsc) instance.rsc=instance.rscCap;
							
							cont=new GUIContent("Resource Regen Rate:", "The rate at which the resource regenerate (per second)");
							instance.rscRegenRate=EditorGUILayout.FloatField(cont, instance.rscRegenRate);
						GUILayout.EndVertical();
							
						GUILayout.BeginVertical();
							EditorGUILayout.Space();
							EditorGUILayout.Space();
							EditorGUILayout.Space();
					
							if(!instance.useRscManagerForCost){
								Sprite icon=Ability_DB.GetRscIcon();
								icon=(Sprite)EditorGUILayout.ObjectField(icon, typeof(Sprite), true, GUILayout.Width(40), GUILayout.Height(40));
								Ability_DB.SetRscIcon(icon);
							}
						GUILayout.EndVertical();
					
					GUILayout.EndHorizontal();
				}
				else{
					EditorGUILayout.LabelField("Full Rsc On Start:", "-");
					EditorGUILayout.LabelField("Resource:", "-");
					EditorGUILayout.LabelField("Resource Cap:", "-");
					EditorGUILayout.LabelField("Resource Regen Rate:", "-");
				}
				
			EditorGUILayout.Space();
				
				cont=new GUIContent("Target Select Indicator:", "the object used to indicate selected position during target selection phase");
				instance.tgtSelectIndicator=(Transform)EditorGUILayout.ObjectField(cont, instance.tgtSelectIndicator, typeof(Transform), true);
				
				
			EditorGUILayout.Space();
				
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
				showList=EditorGUILayout.Foldout(showList, "Show Ability List");
				EditorGUILayout.EndHorizontal();
				if(showList){
					
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.Space();
						if(GUILayout.Button("EnableAll") && !Application.isPlaying){
							instance.unavailablePrefabIDList=new List<int>();
						}
						if(GUILayout.Button("DisableAll") && !Application.isPlaying){
							instance.unavailablePrefabIDList=Ability_DB.GetPrefabIDList();
						}
						EditorGUILayout.Space();
					EditorGUILayout.EndHorizontal();
						
					EditorGUILayout.Space();
			
					
					List<Ability> abilityList=Ability_DB.GetList();
					for(int i=0; i<abilityList.Count; i++){
						if(abilityList[i].hideInInspector) continue;
						
						Ability ability=abilityList[i];
						
						GUILayout.BeginHorizontal();
							
							EditorGUILayout.Space();
						
							GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
							TDE.DrawSprite(GUILayoutUtility.GetLastRect(), ability.icon, ability.desp, false);
							
							GUILayout.BeginVertical();
								EditorGUILayout.Space();
								GUILayout.Label(ability.name, GUILayout.ExpandWidth(false));
								
								EditorGUI.BeginChangeCheck();
								bool flag=!instance.unavailablePrefabIDList.Contains(ability.prefabID) ? true : false;
								flag=EditorGUILayout.Toggle(new GUIContent(" - enabled: ", "check to enable the ability in this level"), flag);
								
								if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
									if(!flag && !instance.unavailablePrefabIDList.Contains(ability.prefabID))
										instance.unavailablePrefabIDList.Add(ability.prefabID);
									else if(flag) instance.unavailablePrefabIDList.Remove(ability.prefabID);
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