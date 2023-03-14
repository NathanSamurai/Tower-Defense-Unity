using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(GameControl))]
	public class I_GameControlEditor : _TDInspector {

		private GameControl instance;
		
		public override void Awake(){
			base.Awake();
			instance = (GameControl)target;
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "GameControl");
			
			EditorGUILayout.Space();
				
				cont=new GUIContent("Cap player Life:", "Check to cap player life. Player will always start with full life when life is capped");
				instance.capLife=EditorGUILayout.Toggle(cont, instance.capLife);
			
				cont=new GUIContent("Player's Life:", "Player Life. When reach 0, game is over");
				instance.life=EditorGUILayout.IntField(cont, instance.life);
			
				cont=new GUIContent("Player's Life Cap:", "Player Life Capacity. When enabled, player life will never exceed this value");
				if(instance.capLife){
					instance.lifeCap=EditorGUILayout.IntField(cont, instance.lifeCap);
					if(!Application.isPlaying) instance.life=instance.lifeCap;
				}
				else EditorGUILayout.LabelField("Player's Life Cap:", "-");
			
			EditorGUILayout.Space();
				
				cont=new GUIContent("Regenerate Life:", "Check to enable regeneration of player life.");
				instance.regenLife=EditorGUILayout.Toggle(cont, instance.regenLife);
				
				cont=new GUIContent("Life Regen Rate:", "The rate at which player life regenerate (in second)");
				if(instance.capLife) instance.lifeRegen=EditorGUILayout.FloatField(cont, instance.lifeRegen);
				else EditorGUILayout.LabelField("Player's Life Cap:", "-");
		
			EditorGUILayout.Space();
				
				//		public List<float> rscGainOnWin=new List<float>();
				
				RscManager.MatchRscList(instance.rscGainOnWin, 0);
				
				cont=new GUIContent("Rsc Gain On Win:", "The amount of resource the player will gain when the level is beaten\nUseful for 'Carry Over' option in PerkManager is checked");
				EditorGUILayout.LabelField(cont);
				for(int i=0; i<Rsc_DB.GetCount(); i++){
					GUILayout.BeginHorizontal();
					
					//EditorGUILayout.ObjectField(Rsc_DB.GetIcon(i), typeof(Sprite), true, GUILayout.Width(20), GUILayout.Height(20));
					//EditorGUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
					
					EditorGUILayout.LabelField(" - ", GUILayout.Width(40), GUILayout.Height(20));
					Rect rect=GUILayoutUtility.GetLastRect();	rect.x+=20;	rect.width-=20;
					TDE.DrawSprite(rect, Rsc_DB.GetIcon(i));
					
					EditorGUIUtility.labelWidth-=45;
					
					instance.rscGainOnWin[i]=EditorGUILayout.FloatField(Rsc_DB.GetName(i), instance.rscGainOnWin[i]);
					EditorGUIUtility.labelWidth+=45;
					
					GUILayout.EndHorizontal();
				}
				
				cont=new GUIContent("PerkRscGainOnWin:", "The amount of perk resource the player will gain when the level is beaten\nUseful for 'Carry Over' option in PerkManager is checked");
				instance.perkRscGainOnWin=EditorGUILayout.IntField(cont, instance.perkRscGainOnWin);
				
			EditorGUILayout.Space();
			
				cont=new GUIContent("MainMenu Name:", "Scene's name of the main menu to be loaded when return to menu on UI is called");
				instance.mainMenuName=EditorGUILayout.TextField(cont, instance.mainMenuName);
				cont=new GUIContent("NextScene Name:", "Scene's name to be loaded when this level is completed");
				instance.nextLevelName=EditorGUILayout.TextField(cont, instance.nextLevelName);
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}