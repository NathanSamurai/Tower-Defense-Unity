using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(UIControl))]
	public class I_UIControlEditor : _TDInspector {

		private UIControl instance;
		
		public override void Awake(){
			base.Awake();
			instance = (UIControl)target;
			
			InitLabel();
		}
		
		
		private bool labelInitiated=false;
		private static string[] buildModeLabel=new string[0];
		private static string[] buildModeTooltip=new string[0];
		private static string[] targetModeLabel=new string[0];
		private static string[] targetModeTooltip=new string[0];
		
		void InitLabel(){
			if(labelInitiated) return;
			labelInitiated=true;
			
			int enumLength = Enum.GetValues(typeof(UIControl._BuildMode)).Length;
			buildModeLabel=new string[enumLength];
			buildModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				buildModeLabel[i]=((UIControl._BuildMode)i).ToString();
				if((UIControl._BuildMode)i==UIControl._BuildMode.PointNBuild) buildModeTooltip[i]="";
				if((UIControl._BuildMode)i==UIControl._BuildMode.DragNDrop) buildModeTooltip[i]="";
			}
			
			enumLength = Enum.GetValues(typeof(UIControl._TargetMode)).Length;
			targetModeLabel=new string[enumLength];
			targetModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				targetModeLabel[i]=((UIControl._TargetMode)i).ToString();
				if((UIControl._TargetMode)i==UIControl._TargetMode.SelectNDeploy) targetModeTooltip[i]="";
				if((UIControl._TargetMode)i==UIControl._TargetMode.DragNDrop) targetModeTooltip[i]="";
			}
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "UIControl");
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			cont=new GUIContent("Game Scene:", "Check to indicate if this is an actual game scene, false if otherwise (for setting up perk menu only scene)");
			EditorGUILayout.LabelField(cont);	instance.isGameScene=EditorGUILayout.Toggle(instance.isGameScene);
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			cont=new GUIContent("Touch Mode:", "Check to enable touch mode intended for touch input where hover over build or ability button to bring up tooltip is not an available\n\nIn touch-mode, button with tooltip will need two click. first to bring up the tooltip, second to confirm click.\nOnly works for build button when using not using PointNBuild build mode\nOnly works for ability button that doesnt require target select");
			EditorGUILayout.LabelField(cont);	instance.touchMode=EditorGUILayout.Toggle(instance.touchMode);
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			int buildMode=(int)instance.buildMode;
			cont=new GUIContent("Tower Build Mode:", "The build mode to use");
			contL=TDE.SetupContL(buildModeLabel, buildModeTooltip);
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(cont);
			buildMode = EditorGUILayout.Popup(buildMode, contL);
			GUILayout.EndHorizontal();
			instance.buildMode=(UIControl._BuildMode)buildMode;
			
			float x=instance.dragNDropOffset.x;
			float y=instance.dragNDropOffset.y;
			
			GUILayout.BeginHorizontal();
			cont=new GUIContent(" - DragNDrop Offset (x, y):", "Offset of the tower position from the input cursor");
			EditorGUILayout.LabelField(cont);
			if(instance.buildMode==UIControl._BuildMode.PointNBuild) EditorGUILayout.LabelField("-");	
			else{
				x=EditorGUILayout.FloatField(x);	y=EditorGUILayout.FloatField(y);
				instance.dragNDropOffset=new Vector2(x, y);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			cont=new GUIContent(" - Use 'Pie' Menu:", "Check to use floating build button in 'pie' layout");
			EditorGUILayout.LabelField(cont);	
			if(instance.buildMode!=UIControl._BuildMode.PointNBuild) EditorGUILayout.LabelField("-");	
			else instance.usePieMenuForBuild=EditorGUILayout.Toggle(instance.usePieMenuForBuild);
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			int targetMode=(int)instance.targetMode;
			cont=new GUIContent("Ability Target Mode:", "The target mode to use when selecting ability target\n\nDragNDrop only useful for touch input when mouse cursor is not available, otherwise it's recommended that you use DragNDrop");
			contL=TDE.SetupContL(targetModeLabel, targetModeTooltip);
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(cont);
			targetMode = EditorGUILayout.Popup(targetMode, contL);
			GUILayout.EndHorizontal();
			instance.targetMode=(UIControl._TargetMode)targetMode;
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			cont=new GUIContent("HP-Overlay always visible:", "Check to have the unit HP overlay always visible");
			EditorGUILayout.LabelField(cont);	instance.alwaysShowHPOverlay=EditorGUILayout.Toggle(instance.alwaysShowHPOverlay);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			cont=new GUIContent("Show hit damage overlay:", "Check to show text overlay on attack hit");
			EditorGUILayout.LabelField(cont);	instance.showTextOverlay=EditorGUILayout.Toggle(instance.showTextOverlay);
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			cont=new GUIContent("Reference Scale Width:", "The reference width used in the canvas scaler\nThis value is used in calculation to get the overlays shows up in the right position");
			EditorGUILayout.LabelField(cont);	instance.scaleReferenceWidth=EditorGUILayout.FloatField(instance.scaleReferenceWidth);
			GUILayout.EndHorizontal();
			
			
			
				//~ cont=new GUIContent("Cap player Life:", "Check to cap player life. Player will always start with full life when life is capped");
				//~ instance.capLife=EditorGUILayout.Toggle(cont, instance.capLife);
			
				//~ cont=new GUIContent("Player's Life:", "Player Life. When reach 0, game is over");
				//~ instance.life=EditorGUILayout.IntField(cont, instance.life);
			
				//~ cont=new GUIContent("Player's Life Cap:", "Player Life Capacity. When enabled, player life will never exceed this value");
				//~ if(instance.capLife){
					//~ instance.lifeCap=EditorGUILayout.IntField(cont, instance.lifeCap);
					//~ if(!Application.isPlaying) instance.life=instance.lifeCap;
				//~ }
				//~ else EditorGUILayout.LabelField("Player's Life Cap:", "-");
			
			//~ EditorGUILayout.Space();
				
				//~ cont=new GUIContent("Regenerate Life:", "Check to enable regeneration of player life.");
				//~ instance.regenLife=EditorGUILayout.Toggle(cont, instance.regenLife);
				
				//~ cont=new GUIContent("Life Regen Rate:", "The rate at which player life regenerate (in second)");
				//~ if(instance.capLife) instance.lifeRegen=EditorGUILayout.FloatField(cont, instance.lifeRegen);
				//~ else EditorGUILayout.LabelField("Player's Life Cap:", "-");
		
			//~ EditorGUILayout.Space();
				
				//~ //		public List<float> rscGainOnWin=new List<float>();
				
				//~ RscManager.MatchRscList(instance.rscGainOnWin, 0);
				
				//~ cont=new GUIContent("Rsc Gain On Win:", "The amount of resource the player will gain when the level is beaten\nUseful for 'Carry Over' option in PerkManager is checked");
				//~ EditorGUILayout.LabelField(cont);
				//~ for(int i=0; i<RscDB.GetCount(); i++){
					//~ GUILayout.BeginHorizontal();
					
					//~ //EditorGUILayout.ObjectField(RscDB.GetIcon(i), typeof(Sprite), true, GUILayout.Width(20), GUILayout.Height(20));
					//~ //EditorGUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
					
					//~ EditorGUILayout.LabelField(" - ", GUILayout.Width(40), GUILayout.Height(20));
					//~ Rect rect=GUILayoutUtility.GetLastRect();	rect.x+=20;	rect.width-=20;
					//~ TDE.DrawSprite(rect, RscDB.GetIcon(i));
					
					//~ EditorGUIUtility.labelWidth-=45;
					
					//~ instance.rscGainOnWin[i]=EditorGUILayout.FloatField(RscDB.GetName(i), instance.rscGainOnWin[i]);
					//~ EditorGUIUtility.labelWidth+=45;
					
					//~ GUILayout.EndHorizontal();
				//~ }
				
				//~ cont=new GUIContent("PerkRscGainOnWin:", "The amount of perk resource the player will gain when the level is beaten\nUseful for 'Carry Over' option in PerkManager is checked");
				//~ instance.perkRscGainOnWin=EditorGUILayout.IntField(cont, instance.perkRscGainOnWin);
				
			//~ EditorGUILayout.Space();
			
				//~ cont=new GUIContent("MainMenu Name:", "Scene's name of the main menu to be loaded when return to menu on UI is called");
				//~ instance.mainMenuName=EditorGUILayout.TextField(cont, instance.mainMenuName);
				//~ cont=new GUIContent("NextScene Name:", "Scene's name to be loaded when this level is completed");
				//~ instance.nextLevelName=EditorGUILayout.TextField(cont, instance.nextLevelName);
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}