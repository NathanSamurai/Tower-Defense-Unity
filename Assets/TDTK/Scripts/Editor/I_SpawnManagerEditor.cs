using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(SpawnManager))]
	public class I_SpawnManagerEditor : _TDInspector {

		private SpawnManager instance;
		
		public override void Awake(){
			base.Awake();
			instance = (SpawnManager)target;
			
			InitLabel();
		}
		
		
		private bool labelInitiated=false;
		private static string[] spawnCDTypeLabel=new string[0];
		private static string[] spawnCDTypeTooltip=new string[0];
		
		void InitLabel(){
			if(labelInitiated) return;
			labelInitiated=true;
			
			int enumLength = Enum.GetValues(typeof(SpawnManager._SpawnCDType)).Length;
			spawnCDTypeLabel=new string[enumLength];
			spawnCDTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				spawnCDTypeLabel[i]=((SpawnManager._SpawnCDType)i).ToString();
				if((SpawnManager._SpawnCDType)i==SpawnManager._SpawnCDType.Spawned) 
					spawnCDTypeTooltip[i]="Count down to next wave start as soon as the current wave done spawning";
				if((SpawnManager._SpawnCDType)i==SpawnManager._SpawnCDType.Cleared) 
					spawnCDTypeTooltip[i]="Count down to next wave start as soon as the current wave is cleared";
				if((SpawnManager._SpawnCDType)i==SpawnManager._SpawnCDType.None) 
					spawnCDTypeTooltip[i]="No count down to next wave. The spawning of next wave is initiated by player";
			}
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "SpawnManager");
			
			EditorGUILayout.Space();
				
				cont=new GUIContent("Endless Mode:", "Check to enable endless mode");
				instance.endlessMode=EditorGUILayout.Toggle(cont, instance.endlessMode);
			
				cont=new GUIContent("Generate On Start:", "Check to have the waves regenerated at the start of game. All preset setting will be overwritten.");
				if(instance.endlessMode) EditorGUILayout.LabelField("Generate On Start:", "-");
				else instance.genWaveOnStart=EditorGUILayout.Toggle(cont, instance.genWaveOnStart);
			
			
				int spawnCDType=(int)instance.spawnCDType;
				cont=new GUIContent("Spawn CD Type:", "Spawn CD Type used in this level");
				contL=TDE.SetupContL(spawnCDTypeLabel, spawnCDTypeTooltip);
				spawnCDType = EditorGUILayout.Popup(cont, spawnCDType, contL);
				instance.spawnCDType=(SpawnManager._SpawnCDType)spawnCDType;
			
				cont=new GUIContent("Skippable:", "Allow player to skip ahead and spawn the next wave");
				if(instance.spawnCDType==SpawnManager._SpawnCDType.None) EditorGUILayout.LabelField("Skippable:", "-");
				else instance.skippable=EditorGUILayout.Toggle(cont, instance.skippable);
				
				
				cont=new GUIContent("Auto Start:", "Check to start the game on a timer instead of waiting for player initiation");
				instance.autoStart=EditorGUILayout.Toggle(cont, instance.autoStart);
				
				cont=new GUIContent(" - Start Timer:", "Delay in second before the spawn start automatically");
				if(!instance.autoStart) EditorGUILayout.LabelField(" - Start Timer:", "-");
				else instance.startTimer=EditorGUILayout.FloatField(cont, instance.startTimer);
			
			EditorGUILayout.Space();
			
				EditorGUILayout.HelpBox("Editing wave Information using Inspector is not recommended.\nPlease use SpawnEditorWindow instead", MessageType.Info);
				if(GUILayout.Button("Open SpawnEditorWindow")) SpawnEditorWindow.Init();
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}