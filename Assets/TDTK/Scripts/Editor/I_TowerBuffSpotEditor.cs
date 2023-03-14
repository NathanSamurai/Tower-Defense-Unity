using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using TDTK;

[CustomEditor(typeof(TowerBuffSpot))]
public class I_TowerBuffSpotEditor : _TDInspector
{
	private TowerBuffSpot instance;
	
	
	public override void Awake(){
		base.Awake();
		instance = (TowerBuffSpot)target;
	}
	
	public override void OnInspectorGUI(){
		base.OnInspectorGUI();
		
		if(instance==null){ Awake(); return; }
		
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Effects On Tower:");
		
		
		for(int i=0; i<instance.buffEffectPIDList.Count; i++){
			GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(" - ", GUILayout.MaxWidth(15));
				
				int effIdx=Effect_DB.GetPrefabIndex(instance.buffEffectPIDList[i]);		bool removeEff=false;
				
				effIdx = EditorGUILayout.Popup(effIdx, Effect_DB.label, GUILayout.MinHeight(20));
				if(GUILayout.Button("-", GUILayout.MaxWidth(20), GUILayout.MaxHeight(14))){ instance.buffEffectPIDList.RemoveAt(i); removeEff=true; }
				
				if(effIdx>=0 && !removeEff) instance.buffEffectPIDList[i]=Effect_DB.GetItem(effIdx).prefabID;
			
			GUILayout.EndHorizontal();
		}
		
			
		GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(" - ", GUILayout.MaxWidth(15));
			
			int newEffIdx=-1;
			newEffIdx = EditorGUILayout.Popup(newEffIdx, Effect_DB.label);
			if(newEffIdx>=0){
				int newPID=Effect_DB.GetItem(newEffIdx).prefabID;
				//if(!instance.buffEffectPIDList.Contains(newPID)){
				//	Debug.Log("Effect already in list");
					instance.buffEffectPIDList.Add(newPID);
				//}
			}
			
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(20));
		GUILayout.EndHorizontal();
			
		
		EditorGUILayout.Space();
		
		DefaultInspector();
		
		if(GUI.changed) EditorUtility.SetDirty(instance);
	}
}
