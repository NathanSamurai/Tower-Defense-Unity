using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace TDTK {
	
	public class UnitCreepEditorWindow : TDEditorWindow {
		
		[MenuItem ("Tools/TDTK/CreepEditor", false, 10)]
		static void OpenUnitCreepEditor () { Init(); }
		
		private static UnitCreepEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			window = (UnitCreepEditorWindow)EditorWindow.GetWindow(typeof (UnitCreepEditorWindow), false, "CreepEditor");
			window.minSize=new Vector2(420, 300);
			
			TDE.Init();
			
			InitLabel();
			
			//if(prefabID>=0) window.selectID=Creep_DB.GetPrefabIndex(prefabID);
			//window.SelectItem(window.selectID);
			
			if(prefabID>=0){
				window.selectID=Creep_DB.GetPrefabIndex(prefabID);
				window.newSelectID=window.selectID;
				window.newSelectDelay=1;
			}
			
			window._SelectItem();
		}
		
		
		
		private static string[] creepTypeLabel;
		private static string[] creepTypeTooltip;
		
		private static string[] offsetModeLabel;
		private static string[] offsetModeTooltip;
		
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(UnitCreep._CreepType)).Length;
			creepTypeLabel=new string[enumLength];
			creepTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				creepTypeLabel[i]=((UnitCreep._CreepType)i).ToString();
				if((UnitCreep._CreepType)i==UnitCreep._CreepType.Default) 		creepTypeTooltip[i]="Just a standard creep with no special ability";
				if((UnitCreep._CreepType)i==UnitCreep._CreepType.Turret) 		creepTypeTooltip[i]="Attack tower directly by fire shootObject";
				if((UnitCreep._CreepType)i==UnitCreep._CreepType.AOE) 			creepTypeTooltip[i]="Apply its effect to all tower within it's area of effective";
				if((UnitCreep._CreepType)i==UnitCreep._CreepType.Support) 	creepTypeTooltip[i]="Buff friendly creep in range";
				if((UnitCreep._CreepType)i==UnitCreep._CreepType.Spawner) 	creepTypeTooltip[i]="Periodically spawn more creep";
			}
			
			enumLength = Enum.GetValues(typeof(UnitCreep._PathOffsetMode)).Length;
			offsetModeLabel=new string[enumLength];
			offsetModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				offsetModeLabel[i]=((UnitCreep._PathOffsetMode)i).ToString();
				if((UnitCreep._PathOffsetMode)i==UnitCreep._PathOffsetMode.None) 			offsetModeTooltip[i]="No offset, creep will follow the center of the path strictly";
				if((UnitCreep._PathOffsetMode)i==UnitCreep._PathOffsetMode.Simple) 		offsetModeTooltip[i]="Direction is not taken in account when calculating offset\nThe creep will simply follow a fix offset\nMore performance friendly";
				if((UnitCreep._PathOffsetMode)i==UnitCreep._PathOffsetMode.AlignToPath) 	offsetModeTooltip[i]="Creep will follow a fix offset with move direction taken into account";
			}
		}
		
		
		
		public void OnGUI(){
			TDE.InitGUIStyle();
			
			if(!CheckIsPlaying()) return;
			if(window==null) Init();
			
			
			List<UnitCreep> creepList=Creep_DB.GetList();
			selectID=Mathf.Clamp(selectID, 0, creepList.Count-1);
			
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(Creep_DB.GetDB(), "creepDB");
			if(creepList.Count>0) Undo.RecordObject(creepList[selectID], "creep");
			
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")){
				TDE.SetDirty();
				GUI.FocusControl(null);
			}
			
			if(!Creep_DB.UpdatedToPost_2018_3()){
				GUI.color=new Color(0, 1f, 1f, 1f);
				if(GUI.Button(new Rect(Math.Max(260, window.position.width-230), 5, 100, 25), "Copy Old DB")){
					Creep_DB.CopyFromOldDB();
					//SelectItem(0);
					Select(0);	selectID=0;
				}
				GUI.color=Color.white;
			}
			
			
			UnitCreep newCreep=null;
			TDE.Label(5, 7, 150, 17, "Add New Creep:", "Drag creep prefab to this slot to add it to the list");
			newCreep=(UnitCreep)EditorGUI.ObjectField(new Rect(115, 7, 150, 17), newCreep, typeof(UnitCreep), false);
			if(newCreep!=null) Select(NewItem(newCreep));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){ if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false; }
			else{ if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true; }
			
			Vector2 v2=DrawCreepList(startX, startY, creepList);
			startX=v2.x+25;
			
			if(creepList.Count==0) return;
			if(selectID>=creepList.Count) return;
			
			if(newSelectDelay>0){
				newSelectDelay-=1;		GUI.FocusControl(null);
				if(newSelectDelay==0) _SelectItem();
				else Repaint();
			}
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX, window.position.height-startY);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
				
				EditorGUI.BeginChangeCheck();
			
				v2=DrawUnitConfigurator(startX, startY, creepList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
			
				if(EditorGUI.EndChangeCheck()){
					#if UNITY_2018_3_OR_NEWER
					/*
					GameObject unitObj=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(creepList[selectID].gameObject));
					UnitCreep selectedUnit=unitObj.GetComponent<UnitCreep>();
					//Undo.RecordObject(selectedUnit, "screep");
					selectedUnit=creepList[selectID];
					//PrefabUtility.RecordPrefabInstancePropertyModifications(selectedUnit);
					GameObject obj=PrefabUtility.SavePrefabAsset(selectedUnit.gameObject);
					*/
					
					string assetPath = AssetDatabase.GetAssetPath(creepList[selectID].gameObject);
					
					GameObject unitObj=PrefabUtility.LoadPrefabContents(assetPath);
					UnitCreep selectedCreep=unitObj.GetComponent<UnitCreep>();
					
					EditorUtility.CopySerialized(creepList[selectID], selectedCreep);
					
					PrefabUtility.SaveAsPrefabAsset(unitObj, assetPath);
					PrefabUtility.UnloadPrefabContents(unitObj);
					#endif
					
					TDE.SetDirty();
				}
			
			GUI.EndScrollView();
			
			
			//if(GUI.changed) TDE.SetDirty();
		}
		
		
		
		private bool foldStats=true;
		//private bool showTypeDesp=true;
		private Vector2 DrawUnitConfigurator(float startX, float startY, UnitCreep unit){
			float maxX=startX;
			
			startY=TDE.DrawBasicInfo(startX, startY, unit);
			
			//~ int type=(int)unit.creepType;
			//~ cont=new GUIContent("Creep Type:", "Type of the creep. Each type of creep serve a different function");
			//~ contL=TDE.SetupContL(creepTypeLabel, creepTypeTooltip);
			//~ EditorGUI.LabelField(new Rect(startX+12, startY, width, height), cont);
			//~ type = EditorGUI.Popup(new Rect(startX+spaceX+12, startY, width, height), new GUIContent(""), type, contL);
			//~ unit.creepType=(UnitCreep._CreepType)type;
			
			//~ showTypeDesp=EditorGUI.ToggleLeft(new Rect(startX+spaceX+width+12, startY, width, 20), "Show Description", showTypeDesp);
			//~ if(showTypeDesp){
				//~ EditorGUI.HelpBox(new Rect(startX, startY+=spaceY, width+spaceX, 40), creepTypeTooltip[(int)unit.creepType], MessageType.Info);
				//~ startY+=45-height;
			//~ }
			
			
			EditorGUI.LabelField(new Rect(startX+12, startY, width, height), "Creep Type", TDE.headerS);
			
			string typeText="\n\nNote that a unit is not limited to a single type"; 
			
			startX+=80;	spaceX-=30;
				TDE.Label(startX+12, startY, width, height, " - Turret:", "Check to allow unit from attack a hostile target directly"+typeText);
				unit.isTurret=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.isTurret);
				TDE.Label(startX+12, startY+=spaceY, width, height, " - AOE:", "Check to allow unit to apply its attack to all creeps within it's range in a single attack"+typeText);
				unit.isAOE=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.isAOE);
				TDE.Label(startX+12, startY+=spaceY, width, height, " - Support:", "Check to allow unit to buff friendly unit in range"+typeText);
				unit.isSupport=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.isSupport);
				TDE.Label(startX+12, startY+=spaceY, width, height, " - Spawner:", "Check to enable the unit to spawn other creep when active"+typeText);
				unit.isSpawner=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.isSpawner);
			startX-=80;	spaceX+=30;	//startY+=10;
			
			
			startY=DrawGeneralSetting(startX, startY+spaceY+10, unit);
			
			startY=DrawMovementSetting(startX, startY+spaceY, unit);
			
			
			startY+=spaceY*2;	
			
			
			
			string text="Unit Stats ";//+ !foldStats ? "(show)" : "(hide)" ;
			foldStats=EditorGUI.Foldout(new Rect(startX, startY, spaceX, height), foldStats, text, TDE.foldoutS);
			if(foldStats){
				startY=DrawStats(startX, startY+spaceY, unit.statsList[0], _EType.Creep, false, unit.IsTurret(), unit.IsAOE(), unit.IsSupport(), unit.IsResource(), unit.IsMine())-spaceY;
			
				if(unit.IsSpawner()){
					startX+=12;	startY+=10;
					
					TDE.Label(startX, startY+=spaceY, width, height, "Spawn Prefab:", "Creep prefab to be spawned");
					int idx=Creep_DB.GetPrefabIndex(unit.spawnerPrefab);
					idx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), idx, Creep_DB.label);
					unit.spawnerPrefab=Creep_DB.GetItem(idx);
					if(unit.spawnerPrefab==unit) unit.spawnerPrefab=null;
					
					if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")) unit.spawnOnDestroyed=null;
					
					TDE.Label(startX, startY+=spaceY, width, height, " - Num to Spawn:", "The amount of SpawnOnDestroyed creep to spawn when this unit is destroyed");
					if(unit.spawnerPrefab!=null) unit.spawnerCount=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), unit.spawnerCount);
					else TDE.Label(startX+spaceX, startY, widthS, height, "-");
					
					TDE.Label(startX, startY+=spaceY, width, height, " - Cooldown:", "The cooldown in second before each spawn");
					if(unit.spawnerPrefab!=null) unit.cooldown_Spawner=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.cooldown_Spawner);
					else TDE.Label(startX+spaceX, startY, widthS, height, "-");
					
					bool valid=unit.spawnerPrefab!=null && unit.spawnerCount>0;
					
					startY=DrawSpawnOverride(startX, startY, unit.spawnerOverride, valid, foldSpawnerOverride, SetFoldSpawnerOverride);
					
					startX-=12;
				}
			}
			
			
			startY=DrawCreepVisualEffect(startX, startY+spaceY, unit);
			
			startY=DrawUnitAnimation(startX, startY+spaceY, unit);
			
			startY+=spaceY;
			
			
				GUIStyle style=new GUIStyle("TextArea");	style.wordWrap=true;
				cont=new GUIContent("Unit description (for runtime and editor): ", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
				unit.desp=EditorGUI.DelayedTextField(new Rect(startX, startY+spaceY-3, 270, 150), unit.desp, style);
			
			
			return new Vector2(maxX, startY+170);
		}
		
		
		private bool foldBasicSetting=true;
		protected float DrawGeneralSetting(float startX, float startY, UnitCreep unit){
			string textF="General Creep Setting ";//+(!foldBasicSetting ? "(show)" : "(hide)");
			foldBasicSetting=EditorGUI.Foldout(new Rect(startX, startY, spaceX, height), foldBasicSetting, textF, TDE.foldoutS);
			if(!foldBasicSetting) return startY;
			
			startX+=12;
			
			//~ TDE.Label(startX, startY+=spaceY, width, height, "Flying:", "Check to set the creep as flying unit");
			//~ unit.flying=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.flying);
			//~ TDE.Label(startX, startY+=spaceY, width, height, "Turret:", "");
			//~ unit.isTurret=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.isTurret);
			//~ TDE.Label(startX, startY+=spaceY, width, height, "AOE:", "");
			//~ unit.isAOE=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.isAOE);
			//~ TDE.Label(startX, startY+=spaceY, width, height, "Support:", "");
			//~ unit.isSupport=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.isSupport);
			//~ TDE.Label(startX, startY+=spaceY, width, height, "Spawner:", "");
			//~ unit.isSpawner=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.isSpawner);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Can Be Attacked:", "Check if the unit is immuned to all form of attack");
			unit.canBeAttacked=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.canBeAttacked);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Can Be Targeted:", "Check if the unit cannot be targeted for an attack");
			unit.canBeTargeted=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.canBeTargeted);
			
			
			startY+=10;
			
			
			TDE.Label(startX, startY+=spaceY, width, height, "Immuned Effect:", "The list of effects the unit is immune to");
			for(int i=0; i<unit.effectImmunityList.Count; i++){
				TDE.Label(startX+spaceX-height, startY, width, height, "-");
				
				int effIdx=Effect_DB.GetPrefabIndex(unit.effectImmunityList[i]);
				effIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), effIdx, Effect_DB.label);
				if(effIdx>=0){
					int effID=Effect_DB.GetItem(effIdx).prefabID;
					if(effID>=0 && !unit.effectImmunityList.Contains(effID)) unit.effectImmunityList[i]=effID;
				}
				
				if(effIdx<0 || GUI.Button(new Rect(startX+spaceX+width+3, startY, height, height), "-")) unit.effectImmunityList.RemoveAt(i);
				
				startY+=spaceY;
			}
			
			int newEffID=-1;
			newEffID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newEffID, Effect_DB.label);
			if(newEffID>=0) newEffID=Effect_DB.GetItem(newEffID).prefabID;
			if(newEffID>=0 && !unit.effectImmunityList.Contains(newEffID)) unit.effectImmunityList.Add(newEffID);
			
			
			startY+=10;
			
			
			TDE.Label(startX, startY+=spaceY, width, height, "Life Lost On Dest:", "The amount of life player will lose if the creep reach destination");
			unit.lifeLostOnDestination=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), unit.lifeLostOnDestination);
			unit.lifeLostOnDestination=Mathf.Max(1, unit.lifeLostOnDestination);
			
			startY+=5;
			
			TDE.Label(startX, startY+=spaceY, width, height, "Gain On Destroyed:", "");
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Life (chance):", "The amount of life player will gain when the creep is destroyed, subject to a chance (takes value from 0-1 with 0.3 being 30%)");
			if(unit.lifeGainedOnDestroyed<=0) GUI.color=grey;
			unit.lifeGainedOnDestroyed=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), unit.lifeGainedOnDestroyed);	GUI.color=white;
			if(unit.lifeGainedOnDestroyedChance<=0) GUI.color=grey;
			unit.lifeGainedOnDestroyedChance=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), unit.lifeGainedOnDestroyedChance);	GUI.color=white;

			//TDE.Label(startX, startY+=spaceY, width, height, " - Expericene:", "Check to set the creep as flying unit");
			//if(unit.expGainOnDestroyed<=0) GUI.color=grey;
			//unit.expGainOnDestroyed=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), unit.expGainOnDestroyed);	GUI.color=white;
			
			RscManager.MatchRscList(unit.rscGainOnDestroyed, 0);
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Resource:", "The amount of resource the player will gain when the creep is destroyed");
			float cachedX=startX;	startX+=spaceX;
			for(int i=0; i<Rsc_DB.GetCount(); i++){
				//if(unit.rscGainOnDestroyed[i]==0) GUI.color=grey;
				
				if(i>0 && i%2==0){ startX=cachedX; startY+=spaceY; }	if(i>0) startX+=widthS+2;
				TDE.DrawSprite(new Rect(startX, startY, height, height), Rsc_DB.GetIcon(i), Rsc_DB.GetName(i));
				unit.rscGainOnDestroyed[i]=EditorGUI.DelayedIntField(new Rect(startX+height, startY, widthS-height, height), unit.rscGainOnDestroyed[i]);	GUI.color=white;
			}
			startX=cachedX;

			startY+=10;
			
				TDE.Label(startX, startY+=spaceY, width, height, "SpawnOnDestroyed:", "Creep prefab to be spawn when an instance of this unit is destroyed. Note that the HP of the spawned unit is inherit from the destroyed unit. Use HP-multiplier to specifiy how much of the HP should be carried forward");
				int idx=Creep_DB.GetPrefabIndex(unit.spawnOnDestroyed);
				idx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), idx, Creep_DB.label);
				unit.spawnOnDestroyed=Creep_DB.GetItem(idx);
				if(unit.spawnOnDestroyed==unit) unit.spawnOnDestroyed=null;
				
				if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")) unit.spawnOnDestroyed=null;
				
				TDE.Label(startX, startY+=spaceY, width, height, " - Num to Spawn:", "The amount of SpawnOnDestroyed creep to spawn when this unit is destroyed");
				if(unit.spawnOnDestroyed!=null) unit.sodCount=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), unit.sodCount);
				else TDE.Label(startX+spaceX, startY, widthS, height, "-");
				
				bool valid=unit.spawnOnDestroyed!=null && unit.sodCount>0;
				
				startY=DrawSpawnOverride(startX, startY, unit.sodOverride, valid, foldSodOverride, SetFoldSodOverride);
			
			
			startY+=10;
			
			TDE.Label(startX, startY+=spaceY, width, height, "Flying:", "Check to set the creep as flying");
			unit.flying=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.flying);
			
			startY=DrawUnitSetting(startX-12, startY, unit);
			
			return startY;
		}
		
		
		
		
		private bool foldSodOverride=false;
		private bool foldSpawnerOverride=false;
		
		protected delegate void foldCallback(bool flag);
		protected void SetFoldSpawnerOverride(bool flag){ foldSpawnerOverride=flag; }
		protected void SetFoldSodOverride(bool flag){ foldSodOverride=flag; }
		
		protected float DrawSpawnOverride(float startX, float startY, UnitCreep.SubUnitOverride ovrr, bool valid, bool fold, foldCallback callback){
			string text="Show Override Setting ";//+(!fold ? "(show)" : "(hide)");
			fold=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), fold, text);	callback(fold);
			if(fold){
				startX+=12;	spaceX+=30;	width+=20;
				
				GUI.color=ovrr.mulHP<=0 ? grey : white;
				TDE.Label(startX, startY+=spaceY, width, height, " - HP Multiplier:", "Set >0 to enable override of the spawned unit HP\n\nThe override value is determined by applying this multiplier value to the parent unit HP");
				if(valid) ovrr.mulHP=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), ovrr.mulHP);
				else TDE.Label(startX+spaceX, startY, widthS, height, "-");	GUI.color=white;
				
				GUI.color=ovrr.mulSH<=0 ? grey : white;
				TDE.Label(startX, startY+=spaceY, width, height, " - Shield Multiplier:", "Set >0 to enable override of the spawned unit shield\n\nThe override value is determined by applying this multiplier value to the parent unit shield");
				if(valid) ovrr.mulSH=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), ovrr.mulSH);
				else TDE.Label(startX+spaceX, startY, widthS, height, "-");	GUI.color=white;
				
				GUI.color=ovrr.mulSpd<=0 ? grey : white;
				TDE.Label(startX, startY+=spaceY, width, height, " - Speed Multiplier:", "Set >0 to enable override of the spawned unit speed\n\nThe override value is determined by applying this multiplier value to the parent unit speed");
				if(valid) ovrr.mulSpd=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), ovrr.mulSpd);
				else TDE.Label(startX+spaceX, startY, widthS, height, "-");	GUI.color=white;
				
				GUI.color=ovrr.mulRsc<=0 ? grey : white;
				TDE.Label(startX, startY+=spaceY, width, height, " - Rsc Gain Multiplier:", "Set >0 to enable override of the spawned unit resource gain on destroyed\n\nThe override value is determined by applying this multiplier value to the parent unit resource gain on destroyed");
				if(valid) ovrr.mulRsc=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), ovrr.mulRsc);
				else TDE.Label(startX+spaceX, startY, widthS, height, "-");	GUI.color=white;
				
				startX-=12;	spaceX-=30;	width-=20;
			}
			return startY;
		}
		
		
		
		protected bool foldMovement=true;
		protected float DrawMovementSetting(float startX, float startY, UnitCreep unit){
			string text="Movement Setting ";//+(!foldMovement ? "(show)" : "(hide)");
			foldMovement=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), foldMovement, text, TDE.foldoutS);
			if(foldMovement){
				startX+=12;
				
				int type=(int)unit.pathOffsetMode;
				cont=new GUIContent("Path Offset Mode:", "Gives a slight random offset to prevent creep from following the center of the path exactly\nThe actual deviation magnitude is set on individual path");
				contL=TDE.SetupContL(offsetModeLabel, offsetModeTooltip);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), type, contL);
				unit.pathOffsetMode=(UnitCreep._PathOffsetMode)type;
				
				startY+=7;
				
				TDE.Label(startX, startY+=spaceY, width, height, "Face Traveling Dir.:", "Check to have the creep face the direction their traveling in");
				unit.faceTravelingDir=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.faceTravelingDir);
				
				TDE.Label(startX, startY+=spaceY, width, height, " - Snap To Dir.:", "Check to have the creep sanp to the traveling direction instantly instead of rotate towards it");
				if(unit.faceTravelingDir) unit.snapToDir=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.snapToDir);
				else TDE.Label(startX+spaceX, startY, widthS, height, "-");
				
				TDE.Label(startX, startY+=spaceY, width, height, " - Rotate Speed:", "The speed in which the unit rotate to face the traveling direction");
				if(unit.faceTravelingDir && !unit.snapToDir) unit.rotateSpeed=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.rotateSpeed);
				else TDE.Label(startX+spaceX, startY, widthS, height, "-");
				
				if(unit.IsTurret()){
					startY+=7;
					
					TDE.Label(startX, startY+=spaceY, width, height, "Stop To Attack:", "Check to have the creep stop while attacking");
					unit.stopToAttack=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.stopToAttack);
					
					TDE.Label(startX, startY+=spaceY, width, height, " - Limit Per Stop:", "Number of attack the creep will perform everytime it stops\nOnce the limit is hit the unit will carry on moving");
					if(unit.stopToAttack) unit.attackLimitPerStop=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), unit.attackLimitPerStop);
					else TDE.Label(startX+spaceX, startY, widthS, height, "-");
					
					TDE.Label(startX, startY+=spaceY, width, height, " - Cooldown:", "Cooldown between stop\nUnit cannot stop to attack again when it's on cooldown");
					if(unit.stopToAttack) unit.stopToAttackCooldown=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), unit.stopToAttackCooldown);
					else TDE.Label(startX+spaceX, startY, widthS, height, "-");
				}
			}
			
			return startY;
		}
		
		
		private bool foldVisual=false;
		protected float DrawCreepVisualEffect(float startX, float startY, UnitCreep creep){
			string textF="Visual Setting ";//+(!foldVisual ? "(show)" : "(hide)");
			foldVisual=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldVisual, textF, TDE.foldoutS);
			if(!foldVisual) return startY;
			
			startX+=12;
			
			startY=5+DrawVisualObject(startX, startY+=spaceY, creep.effectSpawn, "Spawn Effect", "OPTIONAL: The effect object to spawn when the creep is spawned");
			startY=5+DrawVisualObject(startX, startY+=spaceY, creep.effectDestroyed, "Destroyed Effect", "OPTIONAL: The effect object to spawn when the creep is destroyed");
			startY=5+DrawVisualObject(startX, startY+=spaceY, creep.effectDestination, "Destination Effect", "OPTIONAL: The effect object to spawn when the creep reach the destination");
			
			TDE.Label(startX, startY+=spaceY, width, height, "Sound-Spawn:", "OPTIONAL - The audio clip to play when the unit is spawned");
			creep.soundSpawn=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), creep.soundSpawn, typeof(AudioClip), true);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Sound -Destination:", "OPTIONAL - The audio clip to play when the unit reaches destination");
			creep.soundDestination=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), creep.soundDestination, typeof(AudioClip), true);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Sound-Destroyed:", "OPTIONAL - The audio clip to play when the unit is destroyed");
			creep.soundDestroyed=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), creep.soundDestroyed, typeof(AudioClip), true);
			
			return startY;
		}
		
		
		
		
		
		
		protected Vector2 DrawCreepList(float startX, float startY, List<UnitCreep> creepList){
			List<EItem> list=new List<EItem>();
			for(int i=0; i<creepList.Count; i++){
				EItem item=new EItem(creepList[i].prefabID, creepList[i].unitName, creepList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(UnitCreep creep){ return window._NewItem(creep); }
		private int _NewItem(UnitCreep creep){
			if(Creep_DB.GetList().Contains(creep)) return selectID;
			
			creep.prefabID=TDE.GenerateNewID(Creep_DB.GetPrefabIDList());
			
			#if UNITY_2018_3_OR_NEWER
			GameObject obj=PrefabUtility.SavePrefabAsset(creep.gameObject);
			creep=obj.GetComponent<UnitCreep>();
			#endif
			
			Creep_DB.GetList().Add(creep);
			Creep_DB.UpdateLabel();
			
			return Creep_DB.GetList().Count-1;
		}
		
		protected override void DeleteItem(){
			Creep_DB.GetList().RemoveAt(deleteID);
			Creep_DB.UpdateLabel();
		}
		
		protected override void SelectItem(){ }
		private void _SelectItem(){ 
			selectID=newSelectID;
			if(Creep_DB.GetList().Count<=0) return;
			
			selectID=Mathf.Clamp(selectID, 0, Creep_DB.GetList().Count-1);
			UpdateObjHierarchyList(Creep_DB.GetList()[selectID].transform);
			
			Repaint();
		}
		//~ protected override void SelectItem(){ SelectItem(selectID); }
		//~ private void SelectItem(int newID){ 
			//~ selectID=newID;
			//~ if(Creep_DB.GetList().Count<=0) return; 
			
			//~ EditorGUI.FocusTextInControl(null);	selectedNewItem=0;
			
			//~ selectID=Mathf.Clamp(selectID, 0, Creep_DB.GetList().Count-1);
			//~ UpdateObjHierarchyList(Creep_DB.GetList()[selectID].transform);
		//~ }
		
		protected override void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		protected override void ShiftItemDown(){ if(selectID<Creep_DB.GetList().Count-1) ShiftItem(1); }
		private void ShiftItem(int dir){
			UnitCreep creep=Creep_DB.GetList()[selectID];
			Creep_DB.GetList()[selectID]=Creep_DB.GetList()[selectID+dir];
			Creep_DB.GetList()[selectID+dir]=creep;
			selectID+=dir;
		}
		
		
		
	}
	
}