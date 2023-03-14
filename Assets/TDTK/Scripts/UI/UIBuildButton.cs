using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace TDTK{

	public class UIBuildButton : UIScreen {
		
		public HorizontalLayoutGroup buttonLayoutGroup;
		public List<UIButton> buildButtons=new List<UIButton>();
		
		private GameObject buttonPrefab;
		
		//filled up during runtime, used for dragNdrop mode only
		[HideInInspector] public List<UnitTower> buildableList=new List<UnitTower>();	
		
		private SelectInfo sInfo;
		
		private static UIBuildButton instance;
		
		public override void Awake(){
			base.Awake();
			
			instance=this;
		}
		
		public override void Start(){ 
			base.Start();
			
			buttonPrefab=buildButtons[0].rootObj;		buttonPrefab.SetActive(false);
			buildButtons.Clear();
			
			buildableList=TowerManager.GetBuildableList();
			for(int i=0; i<buildableList.Count; i++) AddBuildButton(i, buildableList[i].icon,  buildableList[i].GetCost()[0].ToString("f0"));
			
			if(UIControl.UsePointNBuildMode()) thisObj.SetActive(false);
			else if(UIControl.UseDragNDropMode()){ UpdateBuildableStatus(); canvasGroup.alpha=1; thisObj.SetActive(true); }
		}
		
		public static void NewBuildable(UnitTower tower){
			instance.AddBuildButton(instance.buildButtons.Count, tower.icon, tower.GetCost()[0].ToString("f0"));
			instance.buildableList=TowerManager.GetBuildableList();
		}
		
		private void AddBuildButton(int idx, Sprite icon, string txt){
			//if(idx>0) buildButtons.Add(UIButton.Clone(buildButtons[0].rootObj, "Button"+(idx)));
			buildButtons.Add(UIButton.Clone(buttonPrefab, "Button"+(idx)));
			buildButtons[idx].rootObj.SetActive(true);
			buildButtons[idx].Init();
			
			if(UIControl.InTouchMode() && UIControl.UseDragNDropMode()) buildButtons[idx].SetClickCallback(OnBuildButton, null);
			else buildButtons[idx].button.onClick.AddListener(() => OnBuildButton(idx));
			
			if(!UIControl.InTouchMode() || !UIControl.UsePointNBuildMode()) buildButtons[idx].SetCallback(OnHoverBuildButton, OnExitBuildButton);
			
			if(icon!=null) buildButtons[idx].image.sprite=icon;
			else buildButtons[idx].image.enabled=false;
			
			buildButtons[idx].label.text=txt;
		}
		
		public static void ReplaceBuildable(int idx, UnitTower tower){ instance._ReplaceBuildable(idx, tower); }
		public void _ReplaceBuildable(int idx, UnitTower tower){
			buildableList[idx]=tower;
			buildButtons[idx].image.sprite=tower.icon;
			buildButtons[idx].label.text=tower.GetCost()[0].ToString("f0");
		}
		
		
		
		
		void OnEnable(){
			TDTK.onNewTowerE += OnNewTower;
		}
		void OnDisable(){
			TDTK.onNewTowerE -= OnNewTower;
		}
		void OnNewTower(UnitTower tower){
			if(UIControl.UseDragNDropMode()) UpdateBuildableStatus();
		}
		void UpdateBuildableStatus(){
			for(int i=0; i<buildButtons.Count; i++){
				buildButtons[i].button.interactable=TowerManager.CheckTowerCounterLimit(buildableList[i].prefabID, buildableList[i].limitInScene);
			}
		}
		
		
		
		public void OnHoverBuildButton(GameObject butObj){
			int idx=UI.GetItemIndex(butObj, buildButtons);
			
			if(UIControl.UseDragNDropMode()){
				UnitTower tower=buildableList[idx];
				UITooltip.Show(tower, UI.GetCorner(buildButtons[idx].rectT, 1), 0, new Vector3(0, .25f, 0));
			}
			else{
				if(UIControl.UsePieMenuForBuild()){
					UITooltip.Show(sInfo.buildableList[idx]);
				}
				else UITooltip.Show(sInfo.buildableList[idx], UI.GetCorner(buildButtons[idx].rectT, 1), 0, new Vector3(0, .25f, 0));
			}
			
			if(UIControl.UsePointNBuildMode()) TowerManager.ShowSampleTower(sInfo.buildableList[idx].prefabID, sInfo);
		}
		public void OnExitBuildButton(GameObject butObj){
			UITooltip.Hide();
			if(UIControl.UsePointNBuildMode()) TowerManager.HideSampleTower();
		}
		
		private int touchModeButtonIdx=-1;
		public void OnBuildButton(GameObject butObj){ OnBuildButton(UI.GetItemIndex(butObj, buildButtons)); OnHoverBuildButton(butObj); }
		public void OnBuildButton(int idx){
			if(!ValidForInteraction()) return;
			
			//int idx=UI.GetItemIndex(butObj, buildButtons);
			
			if(!buildButtons[idx].button.interactable) return;
			
			if(UIControl.UseDragNDropMode()) UIAbilityButton.ClearSelect();
			//Debug.Log("OnBuildButton  "+UIControl.UsePointNBuildMode()+"   "+UIControl.UseDragNDropMode());
			
			//OnHoverBuildButton(butObj);
			
			if(UIControl.UsePointNBuildMode() && UIControl.InTouchMode()){
				if(touchModeButtonIdx!=idx){
					if(touchModeButtonIdx>=0) ClearTouchModeSelect();
					
					touchModeButtonIdx=idx;
					buildButtons[touchModeButtonIdx].SetHighlight(true);
					OnHoverBuildButton(buildButtons[idx].rootObj);
					return;
				}
				
				ClearTouchModeSelect();
			}
			
			if(UIControl.UsePointNBuildMode()){
				if(!CheckCost(sInfo.buildableList[idx].GetCost())) return;
				
				TowerManager.BuildTower(sInfo.buildableList[idx], sInfo.platform, sInfo.nodeID);
				SelectControl.ClearNode();
				OnExitBuildButton(null);
				Hide();
				
				TowerManager.HideSampleTower();
			}
			
			if(UIControl.UseDragNDropMode()){
				if(!CheckCost(buildableList[idx].GetCost())) return;
				
				SelectControl.ClearUnit();
				UITowerSelect.Hide();
				
				TowerManager.CreateDragNDropTower(buildableList[idx]);
			}
		}
		
		public void ClearTouchModeSelect(){
			if(touchModeButtonIdx<0) return;
			TowerManager.HideSampleTower();
			buildButtons[touchModeButtonIdx].SetHighlight(false);
			touchModeButtonIdx=-1;
			OnExitBuildButton(null);
		}
		
		
		private bool CheckCost(List<float> cost){
			if(!RscManager.HasSufficientRsc(cost)){
				//Debug.Log("Insufficient resources");
				GameControl.InvalidAction("Insufficient resources");
				return false;
			}
			return true;
		}
		
		
		void UpdateDisplay(){
			for(int i=0; i<buildButtons.Count; i++){
				if(i<sInfo.buildableList.Count){
					buildButtons[i].image.sprite=sInfo.buildableList[i].icon;
					//buildButtons[i].label.text=sInfo.buildableList[i].unitName;
					buildButtons[i].label.text=sInfo.buildableList[i].GetCost()[0].ToString("f0");
					
					buildButtons[i].SetActive(true);
				}
				else buildButtons[i].SetActive(false);
			}
		}
		
		
		
		
		
		#region piemenu
		void Update(){
			if(UIControl.UseDragNDropMode() || !UIControl.UsePieMenuForBuild()) return;
			
			if(buttonLayoutGroup.enabled==UIControl.UsePieMenuForBuild())
				buttonLayoutGroup.enabled=!UIControl.UsePieMenuForBuild();
			
			if(sInfo==null) return;
			
			Vector3 screenPos = Camera.main.WorldToScreenPoint(sInfo.GetPos()+new Vector3(0, 1, 0))*UI.GetScaleFactor();
			List<Vector3> posList=GetPieMenuPos(sInfo.buildableList.Count, screenPos);
			
			for(int i=0; i<posList.Count; i++) buildButtons[i].rectT.localPosition=posList[i]; 
		}
		
		private Transform piePosDummyT;
		public List<Vector3> GetPieMenuPos(float num, Vector3 screenPos, float cutoff=120, int size=70){
			List<Vector3> points=new List<Vector3>();
			
			if(num==1){
				points.Add(screenPos+new Vector3(0, size, 0)*UI.GetScaleFactor());
				return points;
			}
			
			//if there's only two button to be displayed, then normal calculation doesnt apply
			if(num<=2){
				points.Add(screenPos+new Vector3(size, 10, 0)*UI.GetScaleFactor());
				points.Add(screenPos+new Vector3(-size, 10, 0)*UI.GetScaleFactor());
				return points;
			}
			
			
			//create a dummy transform which we will use to do the calculation
			if(piePosDummyT==null){
				piePosDummyT=new GameObject().transform;
				piePosDummyT.parent=transform;
				piePosDummyT.name="PiePosDummy";
			}
			
			int cutoffOffset=cutoff>0 ? 1:0;
			
			//calculate the spacing of angle and distance of button from center
			float spacing=(float)((360f-cutoff)/(num-cutoffOffset));
			//float dist=Mathf.Max((num+1)*10, 50);
			float dist=0.35f*num*size;//UIMainControl.GetScaleFactor();
			
			piePosDummyT.rotation=Quaternion.Euler(0, 0, cutoff/2);
			piePosDummyT.position=screenPos;//Vector3.zero;
			
			//rotate the dummy transform using the spacing interval, then sample the end point
			//these end point will be our button position
			for(int i=0; i<num; i++){
				points.Add(piePosDummyT.TransformPoint(new Vector3(0, -dist, 0)));
				piePosDummyT.Rotate(Vector3.forward*spacing);
			}
			
			return points;
		}
		#endregion
		
		
		
		private bool ValidForInteraction(){ return Time.time-showTime>0; }
		
		private float showTime=0;
		public static void Show(SelectInfo info, bool instant=false){ if(instance!=null) instance._Show(info, instant); }
		public void _Show(SelectInfo info, bool instant=false){
			showTime=Time.time;
			sInfo=info;
			
			UpdateDisplay();
			
			base._Show();
			//base._Show(instant);
		}
		public static void Hide(bool instant=false){
			if(instance==null || !instance.thisObj.activeInHierarchy) return;
			
			if(UIControl.UsePointNBuildMode()) TowerManager.HideSampleTower();
			
			instance.ClearTouchModeSelect();
			instance._Hide(true);
		}
		
		public static bool IsActive(){ return instance!=null && instance.thisObj.activeInHierarchy; }
		
	}

}
