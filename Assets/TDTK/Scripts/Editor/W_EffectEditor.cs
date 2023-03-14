using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace TDTK {
	
	public class EffectEditorWindow : TDEditorWindow {
		
		[MenuItem ("Tools/TDTK/EffectEditor", false, 10)]
		static void OpenEffectEditor () { Init(); }
		
		private static EffectEditorWindow window;
		
		public static void Init () {
			window = (EffectEditorWindow)EditorWindow.GetWindow(typeof (EffectEditorWindow), false, "EffectEditor");
			window.minSize=new Vector2(420, 300);
			
			TDE.Init();
			
			window.InitLabel();
			
			//if(prefabID>=0) window.selectID=Effect_DB.GetPrefabIndex(prefabID);
		}
		
		
		private static string[] effTypeLabel;
		private static string[] effTypeTooltip;
		
		public void InitLabel(){
			int enumLength = Enum.GetValues(typeof(Effect._EffType)).Length;
			effTypeLabel=new string[enumLength];
			effTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				effTypeLabel[i]=((Effect._EffType)i).ToString();
				if((Effect._EffType)i==Effect._EffType.Modifier) effTypeTooltip[i]="The value in the effect will be directly added to the target unit";
				if((Effect._EffType)i==Effect._EffType.Multiplier) effTypeTooltip[i]="The value in the effect will be be used to multiply the target unit's base value";
			}
		}
		
		
		public void OnGUI(){
			TDE.InitGUIStyle();
			
			if(!CheckIsPlaying()) return;
			if(window==null) Init();
			
			
			List<Effect> effectList=Effect_DB.GetList();
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(Effect_DB.GetDB(), "abilityDB");
			
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")){
				TDE.SetDirty();
				GUI.FocusControl(null);
			}
			
			if(!Effect_DB.UpdatedToPost_2018_3()){
				GUI.color=new Color(0, 1f, 1f, 1f);
				if(GUI.Button(new Rect(Math.Max(260, window.position.width-230), 5, 100, 25), "Copy Old DB")){
					Effect_DB.CopyFromOldDB();
					//SelectItem(0);
					Select(0);	selectID=0;
				}
				GUI.color=Color.white;
			}
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(effectList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){ if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false; }
			else{ if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true; }
			
			Vector2 v2=DrawEffectList(startX, startY, effectList);
			startX=v2.x+25;
			
			if(effectList.Count==0) return;
			if(selectID>=effectList.Count) return;
			
			if(newSelectDelay>0){
				newSelectDelay-=1;		GUI.FocusControl(null);
				if(newSelectDelay==0) _SelectItem();
				else Repaint();
			}
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX, window.position.height-startY);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawEffectConfigurator(startX, startY, effectList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) TDE.SetDirty();
		}
		
		
		
		Vector2 DrawEffectConfigurator(float startX, float startY, Effect item){
			float maxX=startX;
			
				startY=TDE.DrawBasicInfo(startX, startY, item);
			
			spaceX+=12;
			
				TDE.Label(startX, startY+=spaceY, width, height, "Stackable:", "Check if the effect can stack if apply on a same unit with repeatably");
				item.stackable=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.stackable);
				
				TDE.Label(startX, startY+=spaceY, width, height, " - Ignore Source:", "Check if the effect stackablity takes acount of effect source\nFor example, when checked, two instances of the same effect will stack even if it comes from a tower and an ability");
				//if(item.stackable) 
					item.ignoreSource=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.ignoreSource);
				//else TDE.Label(startX+spaceX, startY, widthS, height, "-", "");
				
				TDE.Label(startX, startY+=spaceY, width, height, "Duration:", "The long the effect will last (in second)");
				item.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), item.duration);
			
			startY+=10;
				
				TDE.Label(startX, startY+=spaceY, width, height, "Effect Attributes:", "", TDE.headerS);	startX+=12;	spaceX-=12;
				
				if(GUI.Button(new Rect(startX+spaceX, startY, widthS*2, height), "Reset")) item.Reset();
				
				TDE.Label(startX, startY+=spaceY, width, height, "Stun Target:", "Check if the effect effect will stun its target");
				item.stun=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.stun);
			
			startY+=10;	startX-=12;	spaceX-=12;
			
				int type=(int)item.effType;		contL=TDE.SetupContL(effTypeLabel, effTypeTooltip);
				TDE.Label(startX+12, startY+=spaceY, width, height, "Effect Type:", "", TDE.headerS);
				type = EditorGUI.Popup(new Rect(startX+spaceX+23, startY, 2*widthS+3, height), new GUIContent(""), type, contL);
				item.effType=(Effect._EffType)type;
				
				if(GUI.Button(new Rect(startX+spaceX+23+2*widthS+5, startY, widthS*2-12, height), "Reset")) item.Reset();
				
				//TDE.Label(startX, startY+=spaceY, width, height, "Multipliers:", "", TDE.headerS);
				
				//bool turret=item.editTurret, bool aoe=false, bool support=false, bool resource=false, bool mine=false
				//startY=DrawStats(startX, startY+=spaceY, item.stats, _EType.Effect, false, item.editTurret, item.editAOE, false, false, item.editMine);
				startY=DrawStats(startX, startY+=spaceY, item.stats, _EType.Effect);
				
				
			//startY+=spaceY;
			
				TDE.Label(startX, startY+=spaceY, width, height, "Visual Effect:", "", TDE.headerS);	startX+=12;	spaceX-=12;
				
					startY=DrawVisualObject(startX, startY+=spaceY, item.hitVisualEffect, "On Hit:", "The effect to spawn when the effect first hit a unit");
					
					TDE.Label(startX, startY+=spaceY, width, height, "Active:", "The visual object to stay on target as long as the effect is active");
					item.activeVisualEffect=(Transform)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.activeVisualEffect, typeof(Transform), true);
			
			startY+=spaceY*2;	startX-=12;
			
				GUIStyle style=new GUIStyle("TextArea");	style.wordWrap=true;
				cont=new GUIContent("Unit description (for runtime and editor): ", "");
				EditorGUI.LabelField(new Rect(startX, startY, 400, height), cont);
				item.desp=EditorGUI.DelayedTextField(new Rect(startX, startY+spaceY-3, 270, 150), item.desp, style);
			
			return new Vector2(maxX, startY+170);
		}
		
		
		
		
		
		
		
		
		protected Vector2 DrawEffectList(float startX, float startY, List<Effect> abilityList){
			List<EItem> list=new List<EItem>();
			for(int i=0; i<abilityList.Count; i++){
				EItem item=new EItem(abilityList[i].prefabID, abilityList[i].name, abilityList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(int idx=-1){ return window._NewItem(idx); }
		private int _NewItem(int idx=-1){
			Effect item=null;
			if(idx<0){ item=new Effect(); item.Reset(); }
			if(idx>=0) item=Effect_DB.GetList()[idx].Clone();
			
			item.prefabID=TDE.GenerateNewID(Effect_DB.GetPrefabIDList());
			
			Effect_DB.GetList().Add(item);
			Effect_DB.UpdateLabel();
			
			return Effect_DB.GetList().Count-1;
		}
		
		protected override void DeleteItem(){
			Effect_DB.GetList().RemoveAt(deleteID);
			Effect_DB.UpdateLabel();
		}
		
		protected override void SelectItem(){ }
		private void _SelectItem(){ 
			selectID=newSelectID;
			if(Effect_DB.GetList().Count<=0) return;
			selectID=Mathf.Clamp(selectID, 0, Effect_DB.GetList().Count-1);
			
			Repaint();
		}
		//~ protected override void SelectItem(){ SelectItem(selectID); }
		//~ private void SelectItem(int newID){  
			//~ EditorGUI.FocusTextInControl(null);	selectedNewItem=0;
			
			//~ selectID=newID;
			//~ if(Effect_DB.GetList().Count<=0) return;
			//~ selectID=Mathf.Clamp(selectID, 0, Effect_DB.GetList().Count-1);
		//~ }
		
		protected override void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		protected override void ShiftItemDown(){ if(selectID<Effect_DB.GetList().Count-1) ShiftItem(1); }
		private void ShiftItem(int dir){
			Effect item=Effect_DB.GetList()[selectID];
			Effect_DB.GetList()[selectID]=Effect_DB.GetList()[selectID+dir];
			Effect_DB.GetList()[selectID+dir]=item;
			selectID+=dir;
		}
		
		
		
	}
	
}