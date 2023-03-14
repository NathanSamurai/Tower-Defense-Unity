using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace TDTK {
	
	public class AbilityEditorWindow : TDEditorWindow {
		
		[MenuItem ("Tools/TDTK/AbilityEditor", false, 10)]
		static void OpenAbilityEditor () { Init(); }
		
		private static AbilityEditorWindow window;
		
		public static void Init () {
			window = (AbilityEditorWindow)EditorWindow.GetWindow(typeof (AbilityEditorWindow), false, "AbilityEditor");
			window.minSize=new Vector2(420, 300);
			
			TDE.Init();
			
			InitLabel();
			
			//if(prefabID>=0) window.selectID=Ability_DB.GetPrefabIndex(prefabID);
		}
		
		
		private static string[] targetTypeLabel;
		private static string[] targetTypeTooltip;
		
		private static string[] effTypeLabel;
		private static string[] effTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(Ability._TargetType)).Length;
			targetTypeLabel=new string[enumLength];
			targetTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				targetTypeLabel[i]=((Ability._TargetType)i).ToString();
				if((Ability._TargetType)i==Ability._TargetType.Hostile) 	targetTypeTooltip[i]="Ability will only work on hostile units";
				if((Ability._TargetType)i==Ability._TargetType.Friendly) targetTypeTooltip[i]="Ability will only work on friendly units";
				if((Ability._TargetType)i==Ability._TargetType.All) 		targetTypeTooltip[i]="Ability will work on all units";
			}
			
			enumLength = Enum.GetValues(typeof(Ability._EffectType)).Length;
			effTypeLabel=new string[enumLength];
			effTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				effTypeLabel[i]=((Ability._EffectType)i).ToString();
				if((Ability._EffectType)i==Ability._EffectType.Default) 	effTypeTooltip[i]="Use the default effect";
				if((Ability._EffectType)i==Ability._EffectType.Custom) 	effTypeTooltip[i]="No effect will be applied, run your custom script using spawn on activate object";
			}
		}
		
		
		
		public void OnGUI(){
			TDE.InitGUIStyle();
			
			if(!CheckIsPlaying()) return;
			if(window==null) Init();
			
			
			List<Ability> abilityList=Ability_DB.GetList();
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(Ability_DB.GetDB(), "Ability_DB");
			
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")){
				TDE.SetDirty();
				GUI.FocusControl(null);
			}
			
			if(!Ability_DB.UpdatedToPost_2018_3()){
				GUI.color=new Color(0, 1f, 1f, 1f);
				if(GUI.Button(new Rect(Math.Max(260, window.position.width-230), 5, 100, 25), "Copy Old DB")) Ability_DB.CopyFromOldDB();
				GUI.color=Color.white;
			}
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(abilityList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){ if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false; }
			else{ if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true; }
			
			Vector2 v2=DrawAbilityList(startX, startY, abilityList);
			startX=v2.x+25;
			
			if(abilityList.Count==0) return;
			if(selectID>=abilityList.Count) return;
			
			if(newSelectDelay>0){
				newSelectDelay-=1;		GUI.FocusControl(null);
				if(newSelectDelay==0) _SelectItem();
				else Repaint();
			}
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX, window.position.height-startY);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawAbilityConfigurator(startX, startY, abilityList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed){
				//~ #if UNITY_2018_3_OR_NEWER
				//~ GameObject dbObj=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(Ability_DB.GetDB().gameObject));
				//~ Ability_DB db=dbObj.GetComponent<Ability_DB>();
				//~ Debug.Log("GetPrefabAssetType      "+PrefabUtility.GetPrefabAssetType(dbObj));
				//~ //Undo.RecordObject(selectedUnit, "stower");
				//~ db.abilityList=abilityList;
				//~ //PrefabUtility.RecordPrefabInstancePropertyModifications(selectedUnit);
				//~ GameObject obj=PrefabUtility.SavePrefabAsset(dbObj);
				//~ #endif
				
				TDE.SetDirty();
			}
		}
		
		
		
		private bool foldStats=true;
		Vector2 DrawAbilityConfigurator(float startX, float startY, Ability item){
			float maxX=startX;
			
			startY=TDE.DrawBasicInfo(startX, startY, item);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Hide In Inspector:", "");
			item.hideInInspector=EditorGUI.Toggle(new Rect(startX+width, startY, width, height), item.hideInInspector);
			
			startY=DrawGeneralSetting(startX, startY+10, item);
			
			startY+=spaceY*2;
			
			
			string text="Ability Stats ";//+(!foldStats ? "(show)" : "(hide)");
			foldStats=EditorGUI.Foldout(new Rect(startX, startY, spaceX, height), foldStats, text, TDE.foldoutS);
			if(foldStats){
				int effType=(int)item.effectType;
				TDE.Label(startX+12, startY+=spaceY, width, height, "Effect Type:", "Indicate what does the ability do");
				contL=TDE.SetupContL(effTypeLabel, effTypeTooltip);
				effType = EditorGUI.Popup(new Rect(startX+spaceX+12, startY, width, height), new GUIContent(""), effType, contL);
				item.effectType=(Ability._EffectType)effType;
				
				startY+=10;
				
				if(item.effectType!=Ability._EffectType.Default || item.effectDelay<=0) GUI.color=Color.grey;
				
				TDE.Label(startX+12, startY+=spaceY, width, height, "Effect Delay:", "Delay in second before the effects apply to target\n\nFor any visual effect to play out");
				item.effectDelay=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+12, startY, widthS, height), item.effectDelay);
				
				GUI.color=item.effectType==Ability._EffectType.Default ? Color.white : Color.grey;
				
				startY=DrawStats(startX, startY, item.stats, _EType.Ability);
				GUI.color=Color.white;
			}
			else startY+=spaceY;
			
			
			startY=DrawVisualSetting(startX, startY, item)+spaceY;
			
			
			startY+=spaceY;
			
			
				GUIStyle style=new GUIStyle("TextArea");	style.wordWrap=true;
				cont=new GUIContent("Unit description (for runtime and editor): ", "");
				EditorGUI.LabelField(new Rect(startX, startY, 400, 20), cont);
				item.desp=EditorGUI.DelayedTextField(new Rect(startX, startY+spaceY-3, 270, 150), item.desp, style);
			
			return new Vector2(maxX, startY+170);
		}
		
		
		
		private bool foldBasicSetting=true;
		protected float DrawGeneralSetting(float startX, float startY, Ability item){
			string textF="General Setting ";//+(!foldBasicSetting ? "(show)" : "(hide)");
			foldBasicSetting=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldBasicSetting, textF, TDE.foldoutS);
			if(!foldBasicSetting) return startY;
			
			startX+=12;
			
			TDE.Label(startX, startY+=spaceY, width, height, "Cost:", "The cost of AbilityManager resource required for the ability\nUsed when 'Use RscManager For Cost' is disabled in AbilityManager");
			item.cost=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cost);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Cost (Rsc):", "The cost of RscManager resource required for the ability\nUsed when 'Use RscManager For Cost' is enabled in AbilityManager");
			while(item.stats.cost.Count<Rsc_DB.GetCount()) item.stats.cost.Add(0);
			while(item.stats.cost.Count>Rsc_DB.GetCount()) item.stats.cost.RemoveAt(item.stats.cost.Count-1);
			
			float cachedX=startX;
			for(int i=0; i<Rsc_DB.GetCount(); i++){
				if(i>0 && i%3==0){ startX=cachedX; startY+=spaceY; }	if(i>0) startX+=widthS+2;
				TDE.DrawSprite(new Rect(startX+spaceX, startY, height, height), Rsc_DB.GetIcon(i), Rsc_DB.GetName(i));
				item.stats.cost[i]=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+height, startY, widthS-height, height), item.stats.cost[i]);
			}
			startX=cachedX;
			
			
			TDE.Label(startX, startY+=spaceY, width, height, "Use Limit:", "How many time the ability can be used in a single level");
			if(item.useLimit<=0) GUI.color=Color.grey;
			item.useLimit=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.useLimit);
			GUI.color=Color.white;
			
			TDE.Label(startX, startY+=spaceY, width, height, "Cooldown:", "The ability cooldown in second");
			item.stats.cooldown=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.stats.cooldown);
			
			
			int targetType=(int)item.targetType;		contL=TDE.SetupContL(targetTypeLabel, targetTypeTooltip);
			TDE.Label(startX, startY+=spaceY+10, width, height, "Target Type:", "The type of unit affected by the ability");
			targetType = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), targetType, contL);
			item.targetType=(Ability._TargetType)targetType;
			
			
			TDE.Label(startX, startY+=spaceY, width, height, "Require Target:", "Check if the ability require the player to specify a target/position");
			item.requireTargetSelection=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.requireTargetSelection);
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Unit As Target:", "Check if the ability require the player to select a specific unit as the target");
			if(!item.requireTargetSelection) EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
			else item.requireUnitAsTarget=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.requireUnitAsTarget);
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Select Indicator:", "OPTIONAL - The object used to indicate the target area during the target select phase\nIf left empty, the default indicator in AbilityManager will be used instead");
			if(!item.requireTargetSelection) EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
			else item.tgtSelectIndicator=(Transform)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.tgtSelectIndicator, typeof(Transform), true);
			
			
			return startY;
		}
		
		
		private bool foldVisual=true;
		protected float DrawVisualSetting(float startX, float startY, Ability item){
			string textF="Visual and Audio Setting ";//+(!foldVisual ? "(show)" : "(hide)");
			foldVisual=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldVisual, textF, TDE.foldoutS);
			if(!foldVisual) return startY;
			
			startX+=12;
			
			startY=DrawVisualObject(startX, startY+=spaceY, item.effectOnActivate, "On Activate Effect:", "OPTIONAL: The effect object to spawn when the ability is activated\nYou can also add custom script on this object to have your own custom ability effect");
			startY=DrawVisualObject(startX, startY+=spaceY, item.effectOnTarget, "On Target Effect:", "OPTIONAL: The effect object to spawn when on each individual target of the ability\nYou can also add custom script on this object to have your own custom ability effect");
			
			startY+=10;
			
			TDE.Label(startX, startY+=spaceY, width, height, "Sound On Activate:", "OPTIONAL - The audio clip to play when the ability is activated");
			item.soundOnActivate=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.soundOnActivate, typeof(AudioClip), true);
			
			return startY;
		}
		
		
		
		
		protected Vector2 DrawAbilityList(float startX, float startY, List<Ability> abilityList){
			List<EItem> list=new List<EItem>();
			for(int i=0; i<abilityList.Count; i++){
				EItem item=new EItem(abilityList[i].prefabID, abilityList[i].name, abilityList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(int idx=-1){ return window._NewItem(idx); }
		private int _NewItem(int idx=-1){
			Ability item=null;
			if(idx<0){ item=new Ability(); item.stats.ResetAsBaseStat(); }
			if(idx>=0) item=Ability_DB.GetList()[idx].Clone();
			
			item.prefabID=TDE.GenerateNewID(Ability_DB.GetPrefabIDList());
			
			Ability_DB.GetList().Add(item);
			Ability_DB.UpdateLabel();
			
			return Ability_DB.GetList().Count-1;
		}
		
		protected override void DeleteItem(){
			Ability_DB.GetList().RemoveAt(deleteID);
			Ability_DB.UpdateLabel();
		}
		
		protected override void SelectItem(){ }
		private void _SelectItem(){ 
			selectID=newSelectID;
			if(Ability_DB.GetList().Count<=0) return;
			selectID=Mathf.Clamp(selectID, 0, Ability_DB.GetList().Count-1);
			
			Repaint();
		}
		//~ protected override void SelectItem(){ SelectItem(selectID); }
		//~ private void SelectItem(int newID){  
			//~ EditorGUI.FocusTextInControl(null);	selectedNewItem=0;
			
			//~ selectID=newID;
			//~ if(Ability_DB.GetList().Count<=0) return;
			//~ selectID=Mathf.Clamp(selectID, 0, Ability_DB.GetList().Count-1);
		//~ }
		
		protected override void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		protected override void ShiftItemDown(){ if(selectID<Ability_DB.GetList().Count-1) ShiftItem(1); }
		private void ShiftItem(int dir){
			Ability item=Ability_DB.GetList()[selectID];
			Ability_DB.GetList()[selectID]=Ability_DB.GetList()[selectID+dir];
			Ability_DB.GetList()[selectID+dir]=item;
			selectID+=dir;
		}
		
		
		
	}
	
}