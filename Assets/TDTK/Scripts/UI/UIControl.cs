using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIControl : MonoBehaviour {
		
		
		[Tooltip("Check to indicate if this is an actual game scene, false if otherwise (for setting up perk menu only scene)")]
		public bool isGameScene=true;
		public static bool IsGameScene(){ return instance!=null && instance.isGameScene; }
		
		[Space(5)][Tooltip("Check to enable touch mode intended for touch input where hover over build or ability button to bring up tooltip is not an available\n\nIn touch-mode, button with tooltip will need two click. first to bring up the tooltip, second to confirm click.\nOnly works for build button when using not using PointNBuild build mode\nOnly works for ability button that doesnt require target select")]
		public bool touchMode=false;
		public static bool InTouchMode(){ return instance!=null && instance.touchMode; }
		
		public enum _BuildMode{ PointNBuild, DragNDrop }
		[Space(5)][Tooltip("The build mode to use")] public _BuildMode buildMode;
		public static bool UsePointNBuildMode(){ return instance.buildMode==_BuildMode.PointNBuild; }
		public static bool UseDragNDropMode(){ return instance.buildMode==_BuildMode.DragNDrop; }
		
		[Space(5)][Tooltip("Check to disable the camera scrolling via dragging when the game is in drag and drop mode")]
		public bool disableCameraDragOnDragNDrop=true;
		public static bool DisableCameraDragOnDragNDrop(){ return instance.disableCameraDragOnDragNDrop; }
		
		[Tooltip("Offset of the tower position from the input cursor")] public Vector2 dragNDropOffset;
		
		[Space(5)][Tooltip("Check to use floating build button in 'pie' layout\nOnly valid for point and build mode")]
		public bool usePieMenuForBuild=false;
		public static bool UsePieMenuForBuild(){ return instance.usePieMenuForBuild; }
		
		
		public enum _TargetMode{ DragNDrop, SelectNDeploy }
		[Space(5)][Tooltip("The target mode to use when selecting ability target\nOnly useful for touch input when mouse cursor is not available, otherwise it's recommended that you use DragNDrop")]
		public _TargetMode targetMode=_TargetMode.DragNDrop;
		public static bool UseDragNDropTgtMode(){ return instance.targetMode==_TargetMode.DragNDrop; }
		public static bool UseSelectNDeployTgtMode(){ return instance.targetMode==_TargetMode.SelectNDeploy; }
		
		
		[Space(10)]
		[Tooltip("Check to have the unit HP overlay always visible")]
		public bool alwaysShowHPOverlay=false;
		public static bool AlwaysShowHPOverlay(){ return instance.alwaysShowHPOverlay; }
		[Tooltip("Check to show text overlay on attack hit")]
		public bool showTextOverlay=false;
		public static bool ShowTextOverlay(){ return instance.showTextOverlay; }
		
		[Space(10)][Tooltip("The reference width used in the canvas scaler\nThis value is used in calculation to get the overlays shows up in the right position")]
		public float scaleReferenceWidth=1366;
		public static float GetScaleReferenceWidth(){ return instance!=null ? instance.scaleReferenceWidth : 1366 ; }
		
		
		
		
		
		private static UIControl instance;
		
		void Awake(){
			instance=this;
		}
		
		// Use this for initialization
		void Start () {
			if(!isGameScene) return;
			
			if(TowerManager.UseFreeFormMode() && buildMode==_BuildMode.PointNBuild){
				Debug.LogWarning("PointNBuild mode is not supported when using FreeFormMode in TowerManager, changed to DragNDrop instead");
				buildMode=_BuildMode.DragNDrop;
			}
		}
		
		void OnEnable(){
			TDTK.onNewBuildableE += OnNewBuildable;
			TDTK.onNewAbilityE += OnNewAbility;
			TDTK.onGameOverE += OnGameOver;
			TDTK.onReplaceBuildableE += OnReplaceBuildable;
		}
		void OnDisable(){
			TDTK.onNewBuildableE -= OnNewBuildable;
			TDTK.onNewAbilityE -= OnNewAbility;
			TDTK.onGameOverE -= OnGameOver;
			TDTK.onReplaceBuildableE -= OnReplaceBuildable;
		}
		
		void OnReplaceBuildable(int idx, UnitTower tower){ UIBuildButton.ReplaceBuildable(idx, tower); }
		void OnNewBuildable(UnitTower tower){ UIBuildButton.NewBuildable(tower); }
		void OnNewAbility(Ability ability){ UIAbilityButton.NewAbility(ability); }
		void OnGameOver(bool playerWon){ UIGameOverScreen.Show(playerWon); }
		
		
		
		
		
		private static int pendingAbilityIdx=-1;
		public static void SelectAbility(int idx){
			if(UseDragNDropTgtMode()) AbilityManager.SelectAbility(idx);
			else instance.StartCoroutine(_SelectAbility(idx)); 
		 }
		public static IEnumerator _SelectAbility(int idx){ 
			while(true){
				if(Input.GetMouseButtonUp(0)) break;
				yield return null;
			}
			yield return null;
			pendingAbilityIdx=idx;
		}
		
		public static void ClearSelectedAbility(){
			pendingAbilityIdx=-1;
			if(AbilityManager.InTargetSelectionMode()) AbilityManager.ClearSelect();
		}
		
		public static int GetPendingAbilityIdx(){ return pendingAbilityIdx; }
		public static bool InTargetSelectionMode(){ return AbilityManager.InTargetSelectionMode() | pendingAbilityIdx>=0; }
		
		//~ void OnGUI(){
			//~ GUI.Label(new Rect(10, Screen.height/2, 130, 35), "ability:");
			//~ if(GUI.Button(new Rect(80, Screen.height/2, 130, 35), ""+targetMode)){
				//~ if(targetMode==_TargetMode.DragNDrop) targetMode=_TargetMode.SelectNDeploy;
				//~ else if(targetMode==_TargetMode.SelectNDeploy) targetMode=_TargetMode.DragNDrop;
			//~ }
			
			//~ GUI.Label(new Rect(10, Screen.height/2+40, 130, 35), "build:");
			//~ if(GUI.Button(new Rect(80, Screen.height/2+40, 130, 35), ""+buildMode)){
				//~ if(buildMode==_BuildMode.DragNDrop) buildMode=_BuildMode.PointNBuild;
				//~ else if(buildMode==_BuildMode.PointNBuild) buildMode=_BuildMode.PointNBuild;
			//~ }
		//~ }
		
		
		
		void Update () {
			if(!isGameScene) return;
			
			/*
			Test code to block a node in runtime, need to uncomment BuildPlatform.BlockNode() 
			if(Input.GetKeyDown(KeyCode.B)){
				//if the position is determined by mouse cursor
				SelectInfo sInfo=TowerManager.GetSelectInfo(Input.mousePosition);
				if(sInfo.nodeID>=0){
					sInfo.platform.BlockNode(sInfo.nodeID);
					TDTK.OnNewTower(null);	//to update pathindicator
				}
			
				//if the target platform and the block position is known in advance
				//NodeTD node=platform.GetNearestNode(blockPos)
				//if(node!=null){
				//	platform.BlockNode(node.ID);
				//	TDTK.OnNewTower(null);	//to update pathindicator
				//}
			}
			*/
			
			
			if(TowerManager.InDragNDropPhase()){
				TowerManager.SetDragNDropOffset(dragNDropOffset);
				//for mobile, to prevent getting stuck in DnD phase if user click but not drag the button
				//if(Input.touchCount==0) TowerManager.ExitDragNDropPhase();	
				return;
			}
			
			//if(Input.GetKeyDown(KeyCode.F8)){
			//	GameControl.EndGame();
			//	//GameControl.LostLife(20);
			//}
			
			//for mobile, to prevent getting stuck in target select phase if user click but not drag the button
			//if(AbilityManager.InTargetSelectionMode()){
			//	if(Input.touchCount==0) AbilityManager.ClearSelect();
			//}
			
			int pointerID=Input.touchCount==0 ? -1 : 0;
			
			if(Input.GetMouseButtonDown(0) && !UI.IsCursorOnUI(pointerID)){
				if(!InTargetSelectionMode()) OnCursorDown();
			}
			
			if(Input.GetMouseButtonUp(0) && !wasCursorOnUI){
				if(AbilityManager.InTargetSelectionMode()) OnCursorDownAbilityTargetMode();
				else if(pendingAbilityIdx>=0){
					OnCursorDownAbilityTargetMode(pendingAbilityIdx);
					pendingAbilityIdx=-1;
				}
				else{
					UIAbilityButton.ClearSelect();
				}
			}
			
			if(Input.GetMouseButtonDown(1)){
				UIAbilityButton.ClearSelect();	//ClearSelectedAbility();
				
				if(buildMode==_BuildMode.PointNBuild){
					SelectControl.ClearNode();
					UIBuildButton.Hide();
				}
				SelectControl.ClearUnit();
				UITowerSelect.Hide();
			}
			
			if(UsePointNBuildMode() && !UIBuildButton.IsActive()){
				SelectInfo sInfo=TowerManager.GetSelectInfo(Input.mousePosition);
				if(sInfo.platform!=null) SelectControl.SelectNode(sInfo.platform, sInfo.nodeID);
				else SelectControl.ClearNode();
			}
			
			wasCursorOnUI=UI.IsCursorOnUI(pointerID);	//to be used for next frame
		}
		private bool wasCursorOnUI=false;
		
		
		private void OnCursorDownAbilityTargetMode(int idx=-1){
			TargetingInfo tInfo=AbilityManager.OnCursorDown(Input.mousePosition, idx);
			
			//if -1 is passed, AbilityManager will use the idx given when AbilityManager.SelectAbility() is called
			if(tInfo.valid) AbilityManager.ActivateAbility(idx, tInfo.pos);
			else Debug.Log("target not valid");
		}
		
		
		private void OnCursorDown(){
			SelectInfo sInfo=TowerManager.GetSelectInfo(Input.mousePosition);
			
			bool select=false;
			bool build=false;
			
			if(sInfo.HasValidPoint()){
				if(sInfo.GetTower()!=null){
					select=true;
					SelectControl.SelectUnit(sInfo.GetTower());
					UITowerSelect.Show(sInfo.GetTower());
				}
				else if(buildMode==_BuildMode.PointNBuild && sInfo.AvailableForBuild() && sInfo.buildableList.Count>0){
					build=true;
					UIBuildButton.Show(sInfo);
					SelectControl.SelectNode(sInfo.platform, sInfo.nodeID);
				}
			}
			
			if(buildMode==_BuildMode.PointNBuild && !build){
				SelectControl.ClearNode();
				UIBuildButton.Hide();
			}
			if(!select){
				SelectControl.ClearUnit();
				UITowerSelect.Hide();
			}
		}
		
		
		//[Space(10)][Tooltip("The blur effect component used to blocked out the in game UI when menu screen is shown")]
		//public UnityStandardAssets.ImageEffects.BlurOptimized blurEffect;
		public static void BlurFadeIn(){}// if(instance.blurEffect!=null) UI.FadeBlur(instance.blurEffect, 0, 2); }
		public static void BlurFadeOut(){}// if(instance.blurEffect!=null) UI.FadeBlur(instance.blurEffect, 2, 0); }
	}

}