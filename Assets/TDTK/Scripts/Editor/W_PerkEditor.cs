using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace TDTK {
	
	public class PerkEditorWindow : TDEditorWindow {
		
		[MenuItem ("Tools/TDTK/PerkEditor", false, 10)]
		static void OpenPerkEditor () { Init(); }
		
		private static PerkEditorWindow window;
		
		public static void Init () {
			window = (PerkEditorWindow)EditorWindow.GetWindow(typeof (PerkEditorWindow), false, "PerkEditor");
			window.minSize=new Vector2(420, 300);
			
			TDE.Init();
			
			InitLabel();
			
			//if(prefabID>=0) window.selectID=Perk_DB.GetPrefabIDIndex(prefabID);
		}
		
		
		private static string[] perkTypeLabel;
		private static string[] perkTypeTooltip;
		
		private static string[] effTypeLabel;
		private static string[] effTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_PerkType)).Length;
			perkTypeLabel=new string[enumLength];
			perkTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				perkTypeLabel[i]=((_PerkType)i).ToString();
				
				if((_PerkType)i==_PerkType.NewTower) 	perkTypeTooltip[i]="Add a new tower to the game";
				if((_PerkType)i==_PerkType.NewAbility) 	perkTypeTooltip[i]="Add a new ability to the game";
				
				if((_PerkType)i==_PerkType.ModifyTower) 		perkTypeTooltip[i]="Modify the attribute of a single/multiple/all tower(s)";
				if((_PerkType)i==_PerkType.ModifyAbility) 		perkTypeTooltip[i]="Modify the attribute of a single/multiple/all ability(s)";
				if((_PerkType)i==_PerkType.ModifyEffect) 		perkTypeTooltip[i]="Modify the attribute of a single/multiple/all effect(s)";
				if((_PerkType)i==_PerkType.ModifyPerkCost) 	perkTypeTooltip[i]="Modify the cost of a single/multiple/all perk(s)";
				
				if((_PerkType)i==_PerkType.GainLife) 		perkTypeTooltip[i]="Give player life";
				if((_PerkType)i==_PerkType.LifeCap) 		perkTypeTooltip[i]="Modify player life's capacity";
				if((_PerkType)i==_PerkType.LifeRegen) 		perkTypeTooltip[i]="Modify player life's regeneration rate";
				if((_PerkType)i==_PerkType.LifeGainWaveCleared) 		perkTypeTooltip[i]="Add a modifier/multiplier to life gain value when a wave is cleared";
				
				if((_PerkType)i==_PerkType.GainRsc) 		perkTypeTooltip[i]="Give player resource";
				if((_PerkType)i==_PerkType.RscRegen) 		perkTypeTooltip[i]="Modify player resource regeneration rate";
				
				if((_PerkType)i==_PerkType.RscGain) 					perkTypeTooltip[i]="Add a modifier/multiplier to resource gain value whenever player gain resource";
				if((_PerkType)i==_PerkType.RscGainCreepDestroyed)perkTypeTooltip[i]="Add a modifier/multiplier to resource gain value when a creep is destroyed";
				if((_PerkType)i==_PerkType.RscGainWaveCleared) 	perkTypeTooltip[i]="Add a modifier/multiplier to resource gain value when a wave is cleared";
				if((_PerkType)i==_PerkType.RscGainResourceTower) perkTypeTooltip[i]="Add a modifier/multiplier to resource gain value when gaining resource from resource tower";
				
				if((_PerkType)i==_PerkType.AbilityRscCap) 	perkTypeTooltip[i]="Modify AbilityManager's resource capacity";
				if((_PerkType)i==_PerkType.AbilityRscRegen) 	perkTypeTooltip[i]="Modify AbilityManager's resource regeneration rate";
				if((_PerkType)i==_PerkType.AbilityRscGainWaveCleared) 	perkTypeTooltip[i]="Add a modifier/multiplier to ability resource gain value when a wave is cleared";
				
				if((_PerkType)i==_PerkType.UnlockTowerUpgrade) 	perkTypeTooltip[i]="Unlock next upgrade tower of a single or a few particular towers\nOnly works on next upgrade tower\nLimit to a maximum of 4 towers";
			}
			
			enumLength = Enum.GetValues(typeof(Perk._EffType)).Length;
			effTypeLabel=new string[enumLength];
			effTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				effTypeLabel[i]=((Perk._EffType)i).ToString();
				if((Perk._EffType)i==Perk._EffType.Modifier) effTypeTooltip[i]="The value in the effect will be directly added to the target unit";
				if((Perk._EffType)i==Perk._EffType.Multiplier) effTypeTooltip[i]="The value in the effect will be be used to multiply the target unit's base value";
			}
		}
		
		
		
		public void OnGUI(){
			TDE.InitGUIStyle();
			
			if(!CheckIsPlaying()) return;
			if(window==null) Init();
			
			
			List<Perk> perkList=Perk_DB.GetList();
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(Perk_DB.GetDB(), "abilityDB");
			
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")){
				TDE.SetDirty();
				GUI.FocusControl(null);
			}
			
			if(!Perk_DB.UpdatedToPost_2018_3()){
				GUI.color=new Color(0, 1f, 1f, 1f);
				if(GUI.Button(new Rect(Math.Max(260, window.position.width-230), 5, 100, 25), "Copy Old DB")){
					Perk_DB.CopyFromOldDB();
					//SelectItem(0);
					Select(0);	selectID=0;
				}
				GUI.color=Color.white;
			}
			
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(perkList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){ if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false; }
			else{ if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true; }
			
			Vector2 v2=DrawPerkList(startX, startY, perkList);
			startX=v2.x+25;
			
			if(perkList.Count==0) return;
			if(selectID>=perkList.Count) return;
			
			if(newSelectDelay>0){
				newSelectDelay-=1;		GUI.FocusControl(null);
				if(newSelectDelay==0) _SelectItem();
				else Repaint();
			}
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX, window.position.height-startY);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawPerkConfigurator(startX, startY, perkList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) TDE.SetDirty();
		}
		
		
		private bool showTypeDesp=true;
		Vector2 DrawPerkConfigurator(float startX, float startY, Perk item){
			float maxX=startX;
			
			startY=TDE.DrawBasicInfo(startX, startY, item);
			
			
			int type=(int)item.type;		contL=TDE.SetupContL(perkTypeLabel, perkTypeTooltip);
			TDE.Label(startX, startY+=spaceY+5, width, height, "Perk Type:", "Specify what the perk do");
			type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), type, contL);
			item.type=(_PerkType)type;
			
			showTypeDesp=EditorGUI.ToggleLeft(new Rect(startX+spaceX+width+2, startY, width, 20), "Show Description", showTypeDesp);
			if(showTypeDesp){
				EditorGUI.HelpBox(new Rect(startX, startY+=spaceY, width+spaceX, 40), perkTypeTooltip[(int)item.type], MessageType.Info);
				startY+=45-height;
			}
			
			startY+=10;
			
			
			startY=DrawBasicSetting(startX, startY, item)+10;
			
			
			startY=DrawEffectSetting(startX, startY, item);
			
			
			startY+=spaceY;
			
				GUIStyle style=new GUIStyle("TextArea");	style.wordWrap=true;
				cont=new GUIContent("Perk description (for runtime and editor): ", "");
				EditorGUI.LabelField(new Rect(startX, startY, 400, 20), cont);
				item.desp=EditorGUI.DelayedTextField(new Rect(startX, startY+spaceY-3, 270, 150), item.desp, style);
			
			return new Vector2(maxX, startY+170);
		}
		
		
		
		private bool foldBasicSetting=true;
		protected float DrawBasicSetting(float startX, float startY, Perk item){
			//TDE.Label(startX, startY+=spaceY, width, height, "General Setting", "", TDE.headerS);
			string textF="General Setting ";//+(!foldBasicSetting ? "(show)" : "(hide)");
			foldBasicSetting=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldBasicSetting, textF, TDE.foldoutS);
			if(!foldBasicSetting) return startY;
			
			startX+=12;
			
			//~ TDE.Label(startX, startY+=spaceY, width, height, "Gained on Wave:", "");	CheckColor(item.autoUnlockOnWave, -1);
			//~ item.autoUnlockOnWave=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.autoUnlockOnWave);	ResetColor();
			
			//~ startY+=10;
			
			TDE.Label(startX, startY+=spaceY, width, height, "Cost:", "The cost of PerkManager resource required for the perk\nUsed when 'Use RscManager For Cost' is disabled in PerkManager");
			item.cost=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.cost);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Cost (Rsc):", "The cost of RscManager resource required for the perk\nUsed when 'Use RscManager For Cost' is enabled in PerkManager");
			//~ while(item.costRsc.Count<Rsc_DB.GetCount()) item.costRsc.Add(0);
			//~ while(item.costRsc.Count>Rsc_DB.GetCount()) item.costRsc.RemoveAt(item.costRsc.Count-1);
			
			RscManager.MatchRscList(item.costRsc, 0);
			
			float cachedX=startX;
			for(int i=0; i<Rsc_DB.GetCount(); i++){
				if(i>0 && i%3==0){ startX=cachedX; startY+=spaceY; }	if(i>0) startX+=widthS+2;
				TDE.DrawSprite(new Rect(startX+spaceX, startY, height, height), Rsc_DB.GetIcon(i), Rsc_DB.GetName(i));
				item.costRsc[i]=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+height, startY, widthS-height, height), item.costRsc[i]);
			}
			startX=cachedX;
			
			//TDE.Label(startX, startY+=spaceY, width, height, "Repeatabe:", "");	
			//item.repeatable=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.repeatable);
			startY+=10;
			
			TDE.Label(startX, startY+=spaceY, width, height, "AutoUnlockOnWave:", "If given a value, the perk will automatically be purchased for the player upon completing the specified wave");	CheckColor(item.autoUnlockOnWave, 0);
			item.autoUnlockOnWave=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.autoUnlockOnWave);	ResetColor();
			
			startY+=10;
			
			TDE.Label(startX, startY+=spaceY, width, height, "Min Level:", "The minimum level required before the perk becomes available\n\nThis is value of 'Level ID' in GameControl");	CheckColor(item.minLevel, 0);
			item.minLevel=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.minLevel);	ResetColor();
			
			TDE.Label(startX, startY+=spaceY, width, height, "Min Wave:", "The minimum wave required before the perk becomes available");	CheckColor(item.minWave, 0);
			item.minWave=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.minWave);	ResetColor();
			
			TDE.Label(startX, startY+=spaceY, width, height, "Min Perk Count:", "The minimum number of perk purchased required before the perk becomes available");	CheckColor(item.minPerkCount, 0);
			item.minPerkCount=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.minPerkCount);	ResetColor();
			
			
			TDE.Label(startX, startY+=spaceY, width, height, "Prereq Perk:", "Perk(s) required to be purchased before the perk becomes available");
			for(int i=0; i<item.prereq.Count; i++){
				TDE.Label(startX+spaceX-20, startY, widthS, height, "-");
				
				int index=Perk_DB.GetPrefabIndex(item.prereq[i]);
				if(index<0){ item.prereq.RemoveAt(i); i-=1; continue; }
				
				index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Perk_DB.label);
				
				int prefabID=Perk_DB.GetItem(index).prefabID;
				if(prefabID!=item.prefabID && !item.prereq.Contains(prefabID)) item.prereq[i]=prefabID;
				
				if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")){ item.prereq.RemoveAt(i); i-=1; }
				
				startY+=spaceY;
			}
			
			int newID=-1;		CheckColor(item.prereq.Count, 0);
			newID=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newID, Perk_DB.label);
			if(newID>=0 && !item.prereq.Contains(newID)) item.prereq.Add(newID);
			startY+=10;	
			ResetColor();
			
			return startY;
		}
		
		
		public float DrawEffectTypeSetting(float startX, float startY, Perk item){
			if(!item.SupportModNMul()) return startY;
			
			int type=(int)item.effType;		contL=TDE.SetupContL(effTypeLabel, effTypeTooltip);
			TDE.Label(startX, startY, width, height, "Effect Type:", "", TDE.headerS);
			type = EditorGUI.Popup(new Rect(startX+spaceX, startY, 2*widthS+3, height), new GUIContent(""), type, contL);
			item.effType=(Perk._EffType)type;
			item.effect.effType=(Effect._EffType)type;
			
			
			if(GUI.Button(new Rect(startX+spaceX+2*widthS+5, startY, widthS*2-12, height), "Reset")){
				if(item.effType==Perk._EffType.Modifier){
					item.gain=0;
					for(int i=0; i<item.gainList.Count; i++) item.gainList[i]=0;
				
					if(item.type==_PerkType.ModifyAbility) item.costMul=0;
					if(item.type==_PerkType.ModifyEffect) item.effect.duration=0; 
				}
				if(item.effType==Perk._EffType.Multiplier){
					item.gain=1;
					for(int i=0; i<item.gainList.Count; i++) item.gainList[i]=1;
					
					if(item.type==_PerkType.ModifyAbility)item.costMul=1;
					if(item.type==_PerkType.ModifyEffect)item.effect.duration=1;
				}
				
				item.effect.Reset(true);
			}
			
			
			return startY+spaceY;
		}
		
		
		private bool foldStats=true;
		protected float DrawEffectSetting(float startX, float startY, Perk item){
			//TDE.Label(startX, startY, spaceX*2, height, "Perk Effect Attribute", "", TDE.headerS);	startY+=spaceY;
			string text="Perk Effect Attribute ";//+ (!foldStats ? "(show)" : "(hide)");
			foldStats=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldStats, text, TDE.foldoutS);
			if(!foldStats) return startY+spaceY;
			
			startY+=spaceY;	startX+=12;	
			
			if(item.type==_PerkType.NewTower){
				int index=item.newTowerPID>=0 ? Tower_DB.GetPrefabIndex(item.newTowerPID) : -1;
				TDE.Label(startX, startY, width, height, "New Tower:", "The new tower to be added to game");
				index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Tower_DB.label);
				if(index>=0) item.newTowerPID=Tower_DB.GetItem(index).prefabID;
				//if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")) item.newTowerPID=-1;
				
				index=item.replaceTowerPID>=0 ? Tower_DB.GetPrefabIndex(item.replaceTowerPID) : -1;
				TDE.Label(startX, startY+=spaceY, width, height, " - Replacing:", "OPTIONAL - exiting tower that will be replaced");
				index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Tower_DB.label);
				if(index>=0) item.replaceTowerPID=Tower_DB.GetItem(index).prefabID;
				if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")) item.replaceTowerPID=-1;
			}
			
			else if(item.type==_PerkType.NewAbility){
				int index=item.newAbilityPID>=0 ? Ability_DB.GetPrefabIndex(item.newAbilityPID) : -1;
				TDE.Label(startX, startY, width, height, "New Ability:", "The new ability to be added to game");
				index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Ability_DB.label);
				if(index>=0) item.newAbilityPID=Ability_DB.GetItem(index).prefabID;
				
				index=item.replaceAbilityPID>=0 ? Ability_DB.GetPrefabIndex(item.replaceAbilityPID) : -1;
				TDE.Label(startX, startY+=spaceY, width, height, " - Replacing:", "OPTIONAL - exiting ability that will be replaced");
				index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Ability_DB.label);
				if(index>=0) item.replaceAbilityPID=Ability_DB.GetItem(index).prefabID;
				if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")) item.replaceAbilityPID=-1;
			}
			
			else if(item.type==_PerkType.UnlockTowerUpgrade){
				TDE.Label(startX, startY, width, height, "Apply To All Towers:", "Check to apply to all towers");	
				item.applyToAll=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.applyToAll);
				
				if(!item.applyToAll){
					TDE.Label(startX, startY+=spaceY, width, height, "Target Tower:", "The target towers which this perk should be applied to");
					for(int i=0; i<item.towerPIDList.Count; i++){
						if(item.towerPIDList[i]<0){ item.towerPIDList.RemoveAt(i); i-=1; continue; } 
						
						int index=Tower_DB.GetPrefabIndex(item.towerPIDList[i]);
						if(index<0){ item.towerPIDList.RemoveAt(i); i-=1; continue; } 
						
						index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Tower_DB.label);
						int prefabID=Tower_DB.GetItem(index).prefabID;
						if(prefabID!=item.prefabID && !item.towerPIDList.Contains(prefabID)) item.towerPIDList[i]=prefabID;
						
						if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")){ item.towerPIDList.RemoveAt(i); i-=1; }
						
						startY+=spaceY;
					}
					
					int newIdx=-1;
					newIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newIdx, Tower_DB.label);
					if(newIdx>=0 && !item.towerPIDList.Contains(Tower_DB.GetItem(newIdx).prefabID)){
						item.towerPIDList.Add(Tower_DB.GetItem(newIdx).prefabID);
					}
				}
				
				startY+=spaceY*0.5f;
				
				TDE.Label(startX, startY+=spaceY, width, height, "Unlock All Upgrade:", "Check to unlock all possible upgrade on the target tower");	
				item.unlockAllUpgrade=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.unlockAllUpgrade);
				
				if(!item.unlockAllUpgrade){
					if(item.applyToAll || item.towerPIDList.Count>1){
						for(int i=0; i<4; i++){
							string txt="";
							if(i==0) txt="first";
							if(i==1) txt="second";
							if(i==2) txt="third";
							if(i==3) txt="fourth";
							
							bool flag=item.towerUpgradeList.Contains(i);	bool cachedFlag=flag;
							
							TDE.Label(startX, startY+=spaceY, width+100, height, " - Unlock "+txt+" upgradable Tower:", "");	
							flag=EditorGUI.Toggle(new Rect(startX+spaceX+100, startY, widthS, height), flag);
							
							if(flag!=cachedFlag){
								if(flag) item.towerUpgradeList.Add(i);
								else item.towerUpgradeList.Remove(i);
							}
						}
					}
					else if(item.towerPIDList.Count==1){
						UnitTower tower=Tower_DB.GetPrefab(item.towerPIDList[0]);
						
						if(tower.upgradeTowerList.Count>0){
							for(int i=0; i<tower.upgradeTowerList.Count; i++){
								bool flag=item.towerUpgradeList.Contains(i);	bool cachedFlag=flag;
							
								TDE.Label(startX, startY+=spaceY, width+50, height, " - Unlock "+tower.upgradeTowerList[i].unitName+":", "");	
								flag=EditorGUI.Toggle(new Rect(startX+spaceX+50, startY, widthS, height), flag);
								
								if(flag!=cachedFlag){
									if(flag) item.towerUpgradeList.Add(i);
									else item.towerUpgradeList.Remove(i);
								}
							}
						}
						else{
							TDE.Label(startX, startY+=spaceY, width*3, height, " - Selected prefab has no upgrade tower", "");	
						}
					}
				}
				else{
					int count=0;
					if(item.applyToAll || item.towerPIDList.Count>1) count=4;
					else{
						UnitTower tower=Tower_DB.GetItem(item.towerPIDList[0]);
						count=tower.upgradeTowerList.Count;
					}
					
					if(count>0){
						for(int i=0; i<count; i++){
							string txt="";
							if(i==0) txt="first";
							if(i==1) txt="second";
							if(i==2) txt="third";
							if(i==3) txt="fourth";
							
							TDE.Label(startX, startY+=spaceY, width+100, height, " - Unlock "+txt+" upgradable Tower:", "");	
							TDE.Label(startX+spaceX+100, startY, widthS, height, "-");
						}
					}
					else{
						TDE.Label(startX, startY+=spaceY, width*3, height, " - Selected prefab has no upgrade tower", "");	
					}
				}
			}
			
			else if(item.UseGainValue() || item.UseGainList()){
				startY=DrawEffectTypeSetting(startX, startY, item);
				
				string txtType=item.IsMultiplier() ? "Multiplier:" : "Modifier:" ;
				if(!item.SupportModNMul()) txtType="Gain:";
				
				if(item.UseGainValue()){
					string txt=item.UseGainList() ? "Global " : "" ;
					
					TDE.Label(startX, startY, width, height, txt+txtType);//"Gain Value:", "");
					item.gain=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.gain);
					startY+=spaceY;
				}
			
				if(item.UseGainList()){
					if(item.gainList.Count<Rsc_DB.GetCount()) item.gainList.Add(0);
					if(item.gainList.Count>Rsc_DB.GetCount()) item.gainList.Remove(item.gainList.Count-1);
					
					for(int i=0; i<item.gainList.Count; i++){
						TDE.DrawSprite(new Rect(startX, startY, height, height), Rsc_DB.GetIcon(i));
						TDE.Label(startX+height, startY, width-height, height, " - "+Rsc_DB.GetName(i));	//" - "+txtType, "");
						item.gainList[i]=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.gainList[i]);
						if(i<item.gainList.Count-1) startY+=spaceY;
					}
				}
				else startY-=spaceY;
			}
			
			else if(item.UseStats()){
				string textItem="";
				if(item.type==_PerkType.ModifyTower) textItem="towers";
				if(item.type==_PerkType.ModifyAbility) textItem="abilities";
				if(item.type==_PerkType.ModifyEffect) textItem="effects";
				
				TDE.Label(startX, startY, width, height, "Apply To All:", "Check to apply to all "+textItem);	
				item.applyToAll=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.applyToAll);
				
				if(!item.applyToAll){
					startY+=spaceY;
					if(item.type==_PerkType.ModifyTower){
						TDE.Label(startX, startY, width, height, "Target Tower:", "The target towers which this perk should be applied to");
						for(int i=0; i<item.towerPIDList.Count; i++){
							if(item.towerPIDList[i]<0){ item.towerPIDList.RemoveAt(i); i-=1; continue; } 
							
							int index=Tower_DB.GetPrefabIndex(item.towerPIDList[i]);
							index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Tower_DB.label);
							int prefabID=Tower_DB.GetItem(index).prefabID;
							if(prefabID!=item.prefabID && !item.towerPIDList.Contains(prefabID)) item.towerPIDList[i]=prefabID;
							
							if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")){ item.towerPIDList.RemoveAt(i); i-=1; }
							
							startY+=spaceY;
						}
						
						int newIdx=-1;
						newIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newIdx, Tower_DB.label);
						if(newIdx>=0 && !item.towerPIDList.Contains(Tower_DB.GetItem(newIdx).prefabID)){
							item.towerPIDList.Add(Tower_DB.GetItem(newIdx).prefabID);
						}
					}
					if(item.type==_PerkType.ModifyAbility){
						TDE.Label(startX, startY, width, height, "Target Ability:", "The target abilities which this perk should be applied to");
						for(int i=0; i<item.abilityPIDList.Count; i++){
							int index=Ability_DB.GetPrefabIndex(item.abilityPIDList[i]);
							index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Ability_DB.label);
							int prefabID=Ability_DB.GetItem(index).prefabID;
							if(prefabID!=item.prefabID && !item.abilityPIDList.Contains(prefabID)) item.abilityPIDList[i]=prefabID;
							
							if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")){ item.abilityPIDList.RemoveAt(i); i-=1; }
							
							startY+=spaceY;
						}
						
						int newIdx=-1;
						newIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newIdx, Ability_DB.label);
						if(newIdx>=0 && !item.abilityPIDList.Contains(Ability_DB.GetItem(newIdx).prefabID)){
							item.abilityPIDList.Add(Ability_DB.GetItem(newIdx).prefabID);
						}
					}
					if(item.type==_PerkType.ModifyEffect){
						TDE.Label(startX, startY, width, height, "Target Effect:", "The target effects which this perk should be applied to");
						for(int i=0; i<item.effectPIDList.Count; i++){
							int index=Effect_DB.GetPrefabIndex(item.effectPIDList[i]);
							index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Effect_DB.label);
							int prefabID=Effect_DB.GetItem(index).prefabID;
							
							if(prefabID!=item.prefabID && !item.effectPIDList.Contains(prefabID)) item.effectPIDList[i]=prefabID;
							
							if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")){ item.effectPIDList.RemoveAt(i); i-=1; }
							
							startY+=spaceY;
						}
						
						int newIdx=-1;
						newIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newIdx, Effect_DB.label);
						if(newIdx>=0 && !item.effectPIDList.Contains(Effect_DB.GetItem(newIdx).prefabID)){
							item.effectPIDList.Add(Effect_DB.GetItem(newIdx).prefabID);
						}
					}
				}
				
				startY+=spaceY+10;
				
				startY=DrawEffectTypeSetting(startX, startY, item)-spaceY;
				
				//~ startY+=spaceY;
				
				_EType eType=_EType.PerkT;
				
				if(item.type==_PerkType.ModifyAbility){
					eType=_EType.PerkA;
					
					TDE.Label(startX, startY+=spaceY, width, height, "Use Limit:", "Modify the use limit of the ability");
					if(item.effType==Perk._EffType.Multiplier) TDE.Label(startX+spaceX, startY, widthS, height, "-");
					else item.gain=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.gain);
					
					TDE.Label(startX, startY+=spaceY, width, height, "Cost:", "Modify/Multiply the activation cost of the ability");
					item.costMul=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.costMul);
					//startY+=spaceY;
				}
				else if(item.type==_PerkType.ModifyEffect){
					eType=_EType.PerkE;
					
					TDE.Label(startX, startY+=spaceY, width, height, "Duration:", "Modify the duration of the effect");
					item.effect.duration=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.effect.duration);
					
					TDE.Label(startX, startY+=spaceY+5, width, height, "Stun:", "Check to enable the effec to stun. This will only override the default value if it's set to true");
					item.effect.stun=EditorGUI.Toggle(new Rect(startX+spaceX, startY, height, height), item.effect.stun);
					//startY+=spaceY;
				}
				
				startY=DrawStats(startX-12, startY+=spaceY, item.effect.stats, eType)-spaceY;
				//startY=DrawStats(startX-12, startY+=spaceY, item.effect.stats, eType, false, item.effect.editTurret, item.effect.editAOE, false, false, item.effect.editMine)-spaceY;
			}
			
			else if(item.IsForPerk()){
				TDE.Label(startX, startY, width, height, "Apply To All:", "Check to apply to all perk");	
				item.applyToAll=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.applyToAll);
				
				if(!item.applyToAll){
					TDE.Label(startX, startY+=spaceY, width, height, "Target Perk:", "The target perk which this perk affect should be applied to");
					for(int i=0; i<item.perkPIDList.Count; i++){
						int index=Perk_DB.GetPrefabIndex(item.perkPIDList[i]);
						index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, Perk_DB.label);
						int prefabID=Perk_DB.GetItem(index).prefabID;
						if(prefabID!=item.prefabID && !item.perkPIDList.Contains(prefabID)) item.perkPIDList[i]=prefabID;
						
						if(GUI.Button(new Rect(startX+spaceX+width+10, startY, height, height), "-")){ item.perkPIDList.RemoveAt(i); i-=1; }
						
						startY+=spaceY;
					}
					
					int newID=-1;
					newID=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), newID, Perk_DB.label);
					if(newID>=0 && !item.perkPIDList.Contains(newID)) item.perkPIDList.Add(newID);
					startY+=spaceY+10;
				}
				
				TDE.Label(startX, startY, width, height, "Perk Rsc Multiplier:", "Modify/Multiply the purchase cost of the ability");
				item.costMul=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+25, startY, widthS, height), item.costMul);
				
				if(item.gainList.Count<Rsc_DB.GetCount()) item.gainList.Add(0);
				if(item.gainList.Count>Rsc_DB.GetCount()) item.gainList.Remove(item.gainList.Count-1);
				
				for(int i=0; i<item.gainList.Count; i++){
					TDE.DrawSprite(new Rect(startX, startY+=spaceY, height, height), Rsc_DB.GetIcon(i));
					TDE.Label(startX+height, startY, width-height, height, " - "+Rsc_DB.GetName(i)+":", "");
					item.gainList[i]=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+25, startY, widthS, height), item.gainList[i]);
				}
			}
			
			return startY+spaceY;
		}
		
		
		
		
		
		protected Vector2 DrawPerkList(float startX, float startY, List<Perk> perkList){
			List<EItem> list=new List<EItem>();
			for(int i=0; i<perkList.Count; i++){
				EItem item=new EItem(perkList[i].prefabID, perkList[i].name, perkList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(int idx=-1){ return window._NewItem(idx); }
		private int _NewItem(int idx=-1){
			Perk item=null;
			if(idx<0){ item=new Perk(); item.effect.Reset(); }
			if(idx>=0) item=Perk_DB.GetList()[idx].Clone();
			
			item.prefabID=TDE.GenerateNewID(Perk_DB.GetPrefabIDList());
			
			Perk_DB.GetList().Add(item);
			Perk_DB.UpdateLabel();
			
			return Perk_DB.GetList().Count-1;
		}
		
		protected override void DeleteItem(){
			Perk_DB.GetList().RemoveAt(deleteID);
			Perk_DB.UpdateLabel();
		}
		
		protected override void SelectItem(){ }
		private void _SelectItem(){ 
			selectID=newSelectID;
			if(Perk_DB.GetList().Count<=0) return;
			selectID=Mathf.Clamp(selectID, 0, Perk_DB.GetList().Count-1);
			
			Repaint();
		}
		//~ protected override void SelectItem(){ SelectItem(selectID); }
		//~ private void SelectItem(int newID){  
			//~ EditorGUI.FocusTextInControl(null);	selectedNewItem=0;
			
			//~ selectID=newID;
			//~ if(Perk_DB.GetList().Count<=0) return;
			//~ selectID=Mathf.Clamp(selectID, 0, Perk_DB.GetList().Count-1);
		//~ }
		
		protected override void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		protected override void ShiftItemDown(){ if(selectID<Perk_DB.GetList().Count-1) ShiftItem(1); }
		private void ShiftItem(int dir){
			Perk item=Perk_DB.GetList()[selectID];
			Perk_DB.GetList()[selectID]=Perk_DB.GetList()[selectID+dir];
			Perk_DB.GetList()[selectID+dir]=item;
			selectID+=dir;
		}
		
		
		
	}
	
}