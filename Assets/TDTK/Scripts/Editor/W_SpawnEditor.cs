using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace TDTK {

	public class SpawnEditorWindow : TDEditorWindow {
		
		[MenuItem ("Tools/TDTK/SpawnEditor", false, 10)]
		static void OpenSpawnEditor () { Init(); }
		
		private bool configureGenerator=false;
		
		private static SpawnManager instance;
		private static SpawnEditorWindow window;
		
		public static void Init(SpawnManager smInstance=null) {
			// Get existing open window or if none, make a new one:
			window = (SpawnEditorWindow)EditorWindow.GetWindow(typeof (SpawnEditorWindow), false, "SpawnEditor");
			window.minSize=new Vector2(500, 300);
			
			TDE.Init();
			
			InitLabel();
			
			if(smInstance!=null) instance=smInstance;
		}
		
		
		
		private static string[] spawnCDTypeLabel;
		private static string[] spawnCDTypeTooltip;
		
		private static string[] overrideTypeLabel;
		private static string[] overrideTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(SpawnManager._SpawnCDType)).Length;
			spawnCDTypeLabel=new string[enumLength];
			spawnCDTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				spawnCDTypeLabel[i]=((SpawnManager._SpawnCDType)i).ToString();
				if((SpawnManager._SpawnCDType)i==SpawnManager._SpawnCDType.Spawned) 
					spawnCDTypeTooltip[i]="A new wave is spawn upon every wave duration countdown (with option to skip the timer)";
				if((SpawnManager._SpawnCDType)i==SpawnManager._SpawnCDType.Cleared) 
					spawnCDTypeTooltip[i]="A new wave is spawned when the current wave is cleared (with option to spawn next wave in advance)";
				if((SpawnManager._SpawnCDType)i==SpawnManager._SpawnCDType.None) 
					spawnCDTypeTooltip[i]="Each wave is treated like a round. a new wave can only take place when the previous wave is cleared. Each round require initiation from user";
			}
			
			enumLength = Enum.GetValues(typeof(SubWave._OverrideType)).Length;
			overrideTypeLabel=new string[enumLength];
			overrideTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				overrideTypeLabel[i]=((SubWave._OverrideType)i).ToString();
				if((SubWave._OverrideType)i==SubWave._OverrideType.Override) 
					overrideTypeTooltip[i]="Use the value set in the override field directly";
				if((SubWave._OverrideType)i==SubWave._OverrideType.Multiplier) 
					overrideTypeTooltip[i]="Use the value set in the override field as a multiplier to the base value of the prefab";
			}
		}
		
		
		private bool GetSpawnManager(){
			instance=(SpawnManager)FindObjectOfType(typeof(SpawnManager));
			return instance==null ? false : true ;
		}
		
		
		public void OnGUI() {
			TDE.InitGUIStyle();
			
			//if(!CheckIsPlaying()) return;
			if(window==null) Init();
			if(instance==null && !GetSpawnManager()) return;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(instance, "SpawnManager");
			
			//if(GUI.Button(new Rect(window.position.width-130, 5, 125, 25), "Save")) TDE.SetDirty();
			if(GUI.Button(new Rect(window.position.width-130, 5, 125, 25), "Creep Editor")) UnitCreepEditorWindow.Init();
			//if(GUI.Button(new Rect(window.position.width-130, 35, 125, 25), "Generate")) instance.GenerateWave();
			
			if(!instance.endlessMode && !instance.genWaveOnStart){
				string text=configureGenerator ? "Wave List" : "Configure";
				if(GUI.Button(new Rect(window.position.width-130, 35, 125, 25), text)) configureGenerator=!configureGenerator;
			}
			else configureGenerator=false;
			
			
			float startX=5;	float startY=5;	width=150;
			
			startY=DrawGeneralSetting(startX, startY)+5;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX*2, window.position.height-(startY+5));
			Rect contentRect=new Rect(startX, startY, contentWidth-20, contentHeight);
			
			GUI.color=new Color(.85f, .85f, .85f, 1f); GUI.Box(visibleRect, ""); GUI.color=white;
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				if(!instance.endlessMode && !instance.genWaveOnStart && !configureGenerator){
					startY=DrawWaveList(startX, startY+5);
					contentWidth=(subWaveBlockWidth+10)*maxSubWaveSize+20;
				}
				else{
					startY=DrawGeneratorParameter(startX, startY+5);
					contentWidth=(Rsc_DB.GetCount()+5)*(genAttBlockWidth+10)+75;
				}
				
				contentHeight=startY-visibleRect.y-spaceY*2;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
			if(GUI.changed) TDE.SetDirty();
		}
		
		
		
		private float DrawGeneralSetting(float startX, float startY){
			TDE.Label(startX, startY, width, height, "Endless Mode:", "Check to enable endless mode");
			instance.endlessMode=EditorGUI.Toggle(new Rect(startX+spaceX, startY, height, height), instance.endlessMode);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Generate On Start:", "Check to have the waves regenerated at the start of game. All preset setting will be overwritten.");
			if(instance.endlessMode) TDE.Label(startX+spaceX, startY, height, height, "-", "");
			else instance.genWaveOnStart=EditorGUI.Toggle(new Rect(startX+spaceX, startY, height, height), instance.genWaveOnStart);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Start On Timer:", "Check to start the game on a timer instead of waiting for player initiation");
			instance.autoStart=EditorGUI.Toggle(new Rect(startX+spaceX, startY, height, height), instance.autoStart);
			if(instance.autoStart) instance.startTimer=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+height+5, startY, widthS, height), instance.startTimer);
			
			int spawnCDType=(int)instance.spawnCDType;
			TDE.Label(startX, startY+=spaceY, width, height, "Countdown Type:", "Spawn count down type in this level");
			contL=TDE.SetupContL(spawnCDTypeLabel, spawnCDTypeTooltip);
			spawnCDType = EditorGUI.Popup(new Rect(startX+spaceX, startY, widthS*2+5, height), new GUIContent(""), spawnCDType, contL);
			instance.spawnCDType=(SpawnManager._SpawnCDType)spawnCDType;
			
			cont=new GUIContent(" - Skippable", "Allow player to skip ahead and spawn the next wave");
			if(instance.spawnCDType!=SpawnManager._SpawnCDType.None)
				instance.skippable=EditorGUI.ToggleLeft(new Rect(startX+spaceX+widthS*2+10, startY, width, 15), cont, instance.skippable);
			
			
			startY+=spaceY*.5f;
			
			if(!instance.endlessMode){
				TDE.Label(startX, startY+=spaceY, width, height, "WavesList ("+instance.waveList.Count+"):", "Number of waves in the level");
			
				if(GUI.Button(new Rect(startX+spaceX, startY, widthS, 15), "-1"))
					if(instance.waveList.Count>1) instance.waveList.RemoveAt(instance.waveList.Count-1);
				if(GUI.Button(new Rect(startX+spaceX+widthS+5, startY, widthS, 15), "+1"))
					instance.waveList.Add(new Wave());
				
				if(!instance.genWaveOnStart && !configureGenerator){
					if(GUI.Button(new Rect(startX+spaceX+widthS*2.5f, startY, widthS*2, height), "Generate")) instance.GenerateWave();
				}
				
				TDE.Label(window.position.width-width-7, startY, width, height, " - Show Override Setting");
				showOverrideSetting=EditorGUI.Toggle(new Rect(window.position.width-width-20, startY, width, height), showOverrideSetting);
			}
			else startY+=spaceY;//TDE.Label(startX+spaceX, startY, width, height), "-");
			
			
			
			return startY+spaceY;
		}
		
		
		
		
		public float DrawGeneratorParameter(float startX, float startY){
			instance.UpdateGeneratorUnitList();
			
			SpawnGenerator gen=instance.generator;	gen.Init();
			
			startX+=5;		spaceX+=widthS;
			
			TDE.Label(startX, startY, width, height, "Wave Interval (Min/Max):", "The minimum and maximum value of the interval (in second) between two waves");
			gen.waveIntervalMin=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), gen.waveIntervalMin);
			gen.waveIntervalMax=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+widthS, startY, widthS, height), gen.waveIntervalMax);
			
			gen.waveIntervalMin=Mathf.Max(0, gen.waveIntervalMin);		gen.waveIntervalMin=Mathf.Max(gen.waveIntervalMin, 0);
			gen.waveIntervalMax=Mathf.Max(0, gen.waveIntervalMax);	gen.waveIntervalMax=Mathf.Max(gen.waveIntervalMax, 0);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Use all path:", "When checked, all available path will be used (provided that there's enough subwave in the wave)");
			gen.useAllPath=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), gen.useAllPath);
			
			TDE.Label(startX, startY+=spaceY, width, height, "One SubWave per path:", "When checked, the total subwave count will be limited to number of path available");
			gen.limitSubWaveCountToPath=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), gen.limitSubWaveCountToPath);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Mirror SubWave:", "When checked, all subwave will be similar except they uses different path");
			gen.similarSubWave=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), gen.similarSubWave);
			
			spaceX-=widthS;
			
			
			float cachedY=startY+=spaceY*2.5f;	float cachedX=startX;
			TDE.Label(startX+12, startY-spaceY, width*2, height, "Wave Setting:", "", TDE.headerS);	
			DrawGenAttribute(startX, cachedY, gen.attSubWaveCount, "SubWave Count:", "", null, 1);	startX+=genAttBlockWidth+10;
			DrawGenAttribute(startX, cachedY, gen.attTotalUnitCount, "Total Unit Count:", "", null, 1);	startX+=genAttBlockWidth+10;
			
			startX+=15;
			
			TDE.Label(startX+12, startY-spaceY, width*2, height, "Gain On Wave Cleared:", "", TDE.headerS);	
			DrawGenAttribute(startX, cachedY, gen.attLifeGainOnCleared, "Life Gain:");		startX+=genAttBlockWidth+10;
			
			
			//cachedY=startY+spaceY;	cachedX=startX;
			for(int i=0; i<gen.attRscGainOnCleared.Count; i++){
				DrawGenAttribute(startX, cachedY, gen.attRscGainOnCleared[i], Rsc_DB.GetName(i), "", Rsc_DB.GetIcon(i));
				startX+=genAttBlockWidth+10;
			}
			//startY+=genAttBlockHeight+spaceY; startX=cachedX;
			
			DrawGenAttribute(startX, cachedY, gen.attPerkRscGainOnCleared, "Perk Rsc Gain:", "", Perk_DB.GetRscIcon());		startX+=genAttBlockWidth+10;
			DrawGenAttribute(startX, cachedY, gen.attAbilityRscGainOnCleared, "Ability Rsc Gain:", "", Ability_DB.GetRscIcon());		startX+=genAttBlockWidth+10;
			
			startY+=genAttBlockHeight+spaceY*2;	startX=cachedX;
			
			for(int i=0; i<gen.genItemList.Count; i++){
				if(gen.genItemList[i].prefab==null){
					gen.genItemList.RemoveAt(i);	i-=1;
					continue;
				}
				startY=DrawGenItem(startX, startY, gen.genItemList[i])+10;
			}
			
			return startY+spaceY*2;
		}
		
		
		private float genItemBlockWidth=0;	private float genItemBlockHeight=0;
		public float DrawGenItem(float startX, float startY, GenItem item){
			if(item.enabled) GUI.Box(new Rect(startX, startY, genItemBlockWidth, genItemBlockHeight), "");
			else{ GUI.color=grey; GUI.Box(new Rect(startX, startY, genItemBlockWidth, 6+spaceY*2), "");	GUI.color=white; }
			
			startX+=3; startY+=3;
			
			float cachedX=startX;	startX+=scrollPos.x;
			item.enabled=EditorGUI.Toggle(new Rect(startX, startY, height, height), item.enabled);
			
			TDE.DrawSprite(new Rect(startX+=height, startY, 2*height, 2*height), item.prefab.icon);
			TDE.Label(startX+=2*height+5, startY, width, height, item.prefab.unitName, "", TDE.headerS);
			
			if(item.enabled){
				
				TDE.Label(startX, startY+=spaceY, width, height, "Wave (Min/Max):", "The minimum/maximum wave in which the prefab will be spawned");
				item.minWave=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.minWave);
				item.maxWave=EditorGUI.DelayedIntField(new Rect(startX+spaceX+widthS, startY, widthS, height), item.maxWave);
				
				startX=cachedX;
				
				
				startY+=spaceY;	float cachedY=startY;
				DrawGenAttribute(startX, cachedY, item.attOdds, "Odds:", "How likely the prefab will be used in a subwave in relative to other prefab (odds/sum of all odds)");		startX+=genAttBlockWidth+10;
				DrawGenAttribute(startX, cachedY, item.attInterval, "Interval", "The spawn interval of the unit in second");	startX+=genAttBlockWidth+10;
				
				startX+=15;
				
				item.enableHPOverride=EditorGUI.Toggle(new Rect(startX+spaceX-5, startY, height, height), item.enableHPOverride);
				GUI.color=item.enableHPOverride ? white : grey;
				DrawGenAttribute(startX, cachedY, item.attHP, "HitPoint", "The hit-point of the unit\nUncheck to disable override");		startX+=genAttBlockWidth+10;	GUI.color=white;
				
				item.enableSHOverride=EditorGUI.Toggle(new Rect(startX+spaceX-5, startY, height, height), item.enableSHOverride);
				GUI.color=item.enableSHOverride ? white : grey;
				DrawGenAttribute(startX, cachedY, item.attSH, "Shield:", "The shield of the unit\nUncheck to disable override");			startX+=genAttBlockWidth+10;	GUI.color=white;
				
				item.enableSpeedOverride=EditorGUI.Toggle(new Rect(startX+spaceX-5, startY, height, height), item.enableSpeedOverride);
				GUI.color=item.enableSpeedOverride ? white : grey;
				DrawGenAttribute(startX, cachedY, item.attSpeed, "Speed:", "The speed of the unit\nUncheck to disable override");		startX+=genAttBlockWidth+10;	GUI.color=white;
				
				startX+=15;	
				for(int i=0; i<item.attRscGain.Count; i++){
					GUI.color=item.enableRscOverride ? white : grey;
					DrawGenAttribute(startX, cachedY, item.attRscGain[i], Rsc_DB.GetName(i), Rsc_DB.GetName(i)+" gain on destroyed\nUncheck to disable override", Rsc_DB.GetIcon(i));		
					if(i<item.attRscGain.Count-1) startX+=genAttBlockWidth+10;
					GUI.color=white;
				}
				
				item.enableRscOverride=EditorGUI.Toggle(new Rect(startX+spaceX-5, startY, height, height), item.enableRscOverride);
				
				startY+=genAttBlockHeight+spaceY;
				
				genItemBlockWidth=3+startX+genAttBlockWidth;
				genItemBlockHeight=5+startY-cachedY+spaceY*2;
				
				//~ start
				//~ max
			}
			else{
				TDE.Label(startX, startY+=spaceY, width, height, " - Disabled", "");
				startY+=spaceY;
			}
			
			return startY;
		}
		
		private float genAttBlockWidth=0;	private float genAttBlockHeight=0;
		public float DrawGenAttribute(float startX, float startY, GenAttribute att, string label="", string tooltip="", Sprite icon=null, float limit=-1){
			float cachedY=startY;	spaceX-=30;
			
			//GUI.Box(new Rect(startX, startY, genAttBlockWidth, genAttBlockHeight), "");
			
			if(icon!=null){
				TDE.DrawSprite(new Rect(startX+12, startY, height, height), icon);
			}
			
			if(label!=""){
				TDE.Label(startX+12+(icon!=null ? height+3 : 0), startY, width, height, label, tooltip, TDE.headerS);
			}
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Start Value:", "The starting value at wave 1  (StartValue)+wave*IncRate");
			att.startValue=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), att.startValue);
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Inc Rate:", "How much the value increase over each wave");
			att.incrementRate=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), att.incrementRate);
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Deviation:", "Randomization range for multiplier of the final value (0.2 means +/-20%, .5 means +/-50% and so on");
			att.deviation=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), att.deviation);
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Limit (Min):", "The minimum value of the attribute");
			
			GUI.color=att.limitMin>0 ? white : grey;
			att.limitMin=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), att.limitMin);	GUI.color=white;
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Limit (Max):", "The maximum value of the attribute");
			GUI.color=att.limitMax>0 ? white : grey;
			att.limitMax=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), att.limitMax);	GUI.color=white;
			
			if(limit>-1){ att.limitMin=Mathf.Max(att.limitMin, limit); }
			
			genAttBlockWidth=spaceX+widthS;
			genAttBlockHeight=startY-cachedY;
			spaceX+=30;
			
			return startY+=spaceY;
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		private bool showOverrideGenAttribute=false;
		private bool showOverrideSetting=true;
		
		public int maxSubWaveSize=1;
		public List<bool> waveFoldList=new List<bool>();
		public List<bool> waveOFoldList=new List<bool>();
		public int removeIdx=-1;
		private float DrawWaveList(float startX, float startY){
			maxSubWaveSize=1;
			
			if(showOverrideSetting){
				
				float xx=window.position.width-spaceX-widthS*2+5;
				int overrideType=(int)instance.overrideType;
				TDE.Label(xx, startY, width, height, "Override Type:");//, "Type of override being used");
				contL=TDE.SetupContL(overrideTypeLabel, overrideTypeTooltip);
				overrideType = EditorGUI.Popup(new Rect(xx+spaceX-28, startY, widthS*2, height), new GUIContent(""), overrideType, contL);
				instance.overrideType=(SubWave._OverrideType)overrideType;
				
				
				showOverrideGenAttribute=EditorGUI.Foldout(new Rect(startX, startY, widthS*2, 15), showOverrideGenAttribute, "Autofill Override Attributes", TDE.foldoutS);
				
				if(showOverrideGenAttribute){
					float cachedYY=startY+spaceY;	float cachedXX=startX;	startX+=10;
					DrawGenAttribute(startX, cachedYY, instance.overrideHP, "Override HP:", "", null, -1);	startX+=genAttBlockWidth+10;
					DrawGenAttribute(startX, cachedYY, instance.overrideSH, "Override SH:", "", null, -1);	startX+=genAttBlockWidth+10;
					DrawGenAttribute(startX, cachedYY, instance.overrideSpd, "Override Speed:", "", null, -1);	startX+=genAttBlockWidth+10;
					
					TDE.Label(startX+20, cachedYY+spaceY, width, height, "Time Between Wave:", "");
					GUI.color=instance.overrideWaveSpacing>=0 ? white : grey;
					instance.overrideWaveSpacing=EditorGUI.DelayedFloatField(new Rect(startX+20+spaceX+5, cachedYY+spaceY, widthS, height), instance.overrideWaveSpacing);	GUI.color=white;
					
					
					if(GUI.Button(new Rect(startX+20, cachedYY+genAttBlockHeight, widthS*2f, height), "Auto-Fill")){
						for(int i=0; i<instance.waveList.Count; i++){
							float overrideHP=instance.overrideHP.GetValue(i);
							float overrideSH=instance.overrideSH.GetValue(i);
							float overrideSpd=instance.overrideSpd.GetValue(i);
							for(int n=0; n<instance.waveList[i].subWaveList.Count; n++){
								if(overrideHP>0) instance.waveList[i].subWaveList[n].HP=overrideHP;
								if(overrideSH>0) instance.waveList[i].subWaveList[n].SH=overrideSH;
								if(overrideSpd>0) instance.waveList[i].subWaveList[n].speed=overrideSpd;
							}
							
							if(instance.overrideWaveSpacing>=0) instance.waveList[i].timeToNextWave=instance.overrideWaveSpacing;
						}
					}
					if(GUI.Button(new Rect(startX+30+widthS*2f, cachedYY+genAttBlockHeight, widthS*2f, height), "Clear All")){
						for(int i=0; i<instance.waveList.Count; i++){
							for(int n=0; n<instance.waveList[i].subWaveList.Count; n++){
								instance.waveList[i].subWaveList[n].HP=-1;
								instance.waveList[i].subWaveList[n].SH=-1;
								instance.waveList[i].subWaveList[n].speed=-1;
							}
						}
					}
					
					startX=cachedXX;
					startY+=genAttBlockHeight+spaceY*3.5f;
				}
				else startY+=spaceY*2f;
			}
			
			for(int i=0; i<instance.waveList.Count; i++){
				if(waveFoldList.Count<=i) waveFoldList.Add(true);
				if(waveOFoldList.Count<=i) waveOFoldList.Add(true);
				
				waveFoldList[i]=EditorGUI.Foldout(new Rect(startX, startY, widthS*2, 15), waveFoldList[i], "Wave - "+(i+1), TDE.foldoutS);
				
				if(removeIdx!=i){
					if(GUI.Button(new Rect(startX+widthS*2+10, startY, widthS*1.5f, 15), "remove")) removeIdx=i;
					if(GUI.Button(new Rect(startX+widthS*3.5f+12, startY, widthS*1.5f, 15), "Insert")) instance.waveList.Insert(i, new Wave());
				}
				else{
					if(GUI.Button(new Rect(startX+widthS*2+10, startY, widthS*1.5f, 15), "cancel")) removeIdx=-1;
					GUI.color=new Color(1, .2f, .2f, 1f);
					if(GUI.Button(new Rect(startX+widthS*3.5f+12, startY, widthS*1.5f, 15), "confirm")){
						instance.waveList.RemoveAt(i);
						removeIdx=-1;	i-=1; continue;
					}
					GUI.color=white;
				}
				
				Wave wave=instance.waveList[i];
				
				if(waveFoldList[i]){
					startX+=15;
					
					TDE.Label(startX, startY+=spaceY, width, 15, "SubWave ("+wave.subWaveList.Count+"):");
					if(GUI.Button(new Rect(startX+spaceX, startY-1, widthS, 15), "-1"))
						if(wave.subWaveList.Count>1) wave.subWaveList.RemoveAt(wave.subWaveList.Count-1);
					if(GUI.Button(new Rect(startX+spaceX+50, startY-1, widthS, 15), "+1"))
						wave.subWaveList.Add(new SubWave());
					
					float cachedY=startY+spaceY;
					for(int n=0; n<wave.subWaveList.Count; n++){
						startY=DrawSubWaveBlock(startX+(n*(subWaveBlockWidth+10)), cachedY, wave.subWaveList[n], n, i);
					}
					
					startY+=5;
					
					TDE.Label(startX, startY, width, height, "Time to Next Wave: ", "Time in second before next wave is spawned");
					wave.timeToNextWave=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), wave.timeToNextWave);
					
					float cachedX=startX;	//startY+=spaceY;
					
					TDE.Label(startX, startY+=spaceY, width, height, "Gain On Clear: ", "gain when the wave is cleared", TDE.headerS);
					TDE.Label(startX+=spaceX, startY, width, height, "Rsc: ", "Resource");		startX+=30;
					
					RscManager.MatchRscList(wave.rscGainOnCleared, 0);
					
					for(int n=0; n<Rsc_DB.GetCount(); n++){
						TDE.DrawSprite(new Rect(startX, startY, height, height), Rsc_DB.GetIcon(n), Rsc_DB.GetName(n));
						wave.rscGainOnCleared[n]=EditorGUI.DelayedFloatField(new Rect(startX+height, startY, widthS-height, height), wave.rscGainOnCleared[n]);
						startX+=widthS+2;
					}
					
					startX+=widthS*.5f;
					TDE.Label(startX, startY, width, height, "Perk rsc: ", "");
					TDE.DrawSprite(new Rect(startX+=55, startY, height, height), Perk_DB.GetRscIcon());
					wave.perkRscGainOnCleared=EditorGUI.DelayedIntField(new Rect(startX+height, startY, widthS-height, height), wave.perkRscGainOnCleared);
					
					startX+=widthS*1.5f;
					TDE.Label(startX, startY, width, height, "Ability rsc: ", "");
					TDE.DrawSprite(new Rect(startX+=65, startY, height, height), Ability_DB.GetRscIcon());
					wave.abilityRscGainOnCleared=EditorGUI.DelayedIntField(new Rect(startX+height, startY, widthS-height, height), wave.abilityRscGainOnCleared);
					
					startX=cachedX;
					
					startY+=spaceY*2f;
					startX-=15;
				}
				else{
					float cachedX=startX;	startX+=180+widthS*1.5f ;
					
					for(int n=0; n<wave.subWaveList.Count; n++){
						if(wave.subWaveList[n].prefab==null) continue;
						TDE.DrawSprite(new Rect(startX, startY, height*1.5f, height*1.5f), wave.subWaveList[n].prefab.icon);
						TDE.Label(startX+height*1.5f+2, startY, widthS, height, "x"+wave.subWaveList[n].spawnCount);
						startX+=widthS+height*1.5f;
					}
					
					startX=cachedX;
					
					startY+=spaceY*2f;
				}
				
				maxSubWaveSize=Mathf.Max(waveFoldList[i] ? instance.waveList[i].subWaveList.Count : 1, maxSubWaveSize);
			}
			
			return startY+spaceY*2;
		}
		
		
		private float subWaveBlockWidth=0;
		private float subWaveBlockHeight=0;
		private float DrawSubWaveBlock(float startX, float startY, SubWave subWave, int index, int wIdx){
			
			float spaceX=60;	float cachedY=startY;		width-=10;
			
			subWaveBlockWidth=spaceX+width+5;
			GUI.Box(new Rect(startX, startY, subWaveBlockWidth, subWaveBlockHeight), "");
			
			startX+=3; startY+=3;
			
			//TDE.Label(startX, startY, width, height, "Prefab: ", "");
			//subWave.prefab=(UnitCreep)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), subWave.prefab, typeof(UnitCreep), true);	startY+=spaceY
			
			//int index=subWave.prefab!=null ? TDEditor.GetCreepIndex(subWave.unitC.prefabID) : 0 ;
			index=Creep_DB.GetPrefabIndex(subWave.prefab);
			TDE.Label(startX, startY, width, height, "Prefab:", "The creep prefab to be spawned");
			index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Creep_DB.label);
			subWave.prefab=Creep_DB.GetItem(index);
			
			if(subWave.prefab==null) GUI.Box(new Rect(startX+subWaveBlockWidth-5-height*2, startY+spaceY, height*2, height*2), "");
			else TDE.DrawSprite(new Rect(startX+subWaveBlockWidth-7-height*2, startY+spaceY, height*2, height*2), subWave.prefab.icon);
			
			
			TDE.Label(startX, startY+=spaceY, width, height, "Delay: ", "The delay (in second) before the subwave start spawning");
			subWave.delay=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.delay);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Count: ", "How many creep will be spawned for the subwave");
			subWave.spawnCount=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), subWave.spawnCount);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Spacing: ", "The spawn spacing (in second) between each individual creep");
			subWave.spacing=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.spacing);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Path: ", "OPTIONAL: The path to used for this subwave. If left unassigned, a random path will be used");
			subWave.path=(Path)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), subWave.path, typeof(Path), true);
			
			
			if(showOverrideSetting){
				string txt=instance.IsOverrideMultiplier() ? "(Multiplier)" : "";
				TDE.Label(startX, startY+=spaceY+5f, width*2, height, "Override Setting: "+txt+"", "Attribute on the default prefab that will be overriden", TDE.headerS);	spaceX+=10;
				
				TDE.Label(startX, startY+=spaceY, width, height, " - HitPoint: ", "");	GUI.color=subWave.HP>0 ? GUI.color : grey;
				subWave.HP=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.HP);	GUI.color=white;
				
				TDE.Label(startX, startY+=spaceY, width, height, " - Shield: ", "");	GUI.color=subWave.SH>=0 ? GUI.color : grey;
				subWave.SH=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.SH);	GUI.color=white;
				
				TDE.Label(startX, startY+=spaceY, width, height, " - Speed: ", "");	GUI.color=subWave.speed>0 ? GUI.color : grey;
				subWave.speed=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.speed);	GUI.color=white;
				
				
				TDE.Label(startX, startY+=spaceY, width, height, " - Rsc Gain: ", "");
				
				RscManager.MatchRscList(subWave.rscGain, -1);
				
				subWave.overrideRscGain=true;
				for(int i=0; i<subWave.rscGain.Count; i++){ if(subWave.rscGain[i]<0) subWave.overrideRscGain=false; }
				
				float cachedX=startX;	//startY+=spaceY;
				for(int i=0; i<Rsc_DB.GetCount(); i++){
					if(i>0 && i%3==0){ startX=cachedX-widthS-2; startY+=spaceY; }	if(i>0) startX+=widthS+2;
					
					TDE.DrawSprite(new Rect(startX+spaceX, startY, height, height), Rsc_DB.GetIcon(i), Rsc_DB.GetName(i));
					GUI.color=subWave.overrideRscGain ? GUI.color : grey;
					subWave.rscGain[i]=EditorGUI.DelayedIntField(new Rect(startX+spaceX+height, startY, widthS-height, height), subWave.rscGain[i]);
					GUI.color=white;
					
					if(subWave.rscGain[i]<0 && subWave.overrideRscGain) subWave.overrideRscGain=false;
				}
				startX=cachedX;
			}
			
			width+=10;
			subWaveBlockHeight=startY-cachedY+spaceY+2;
			
			return startY+spaceY;
		}
		
		
		
	}

}