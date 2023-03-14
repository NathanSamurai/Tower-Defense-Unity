using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TDTK;

namespace TDTK{

	public class UIAbilityButton : UIScreen {
		
		public List<UIButton> abilityButtons=new List<UIButton>();
		private List<Slider> cooldownSlider=new List<Slider>();
		
		private GameObject buttonPrefab;
		
		public UIButton buttonClearSelect;
		
		private static UIAbilityButton instance;
		
		public override void Awake(){
			base.Awake();
			
			instance=this;
		}
		
		public override void Start(){ 
			base.Start();
			
			if(!AbilityManager.IsEnabled()){
				thisObj.SetActive(false);
				return;
			}
			
			buttonPrefab=abilityButtons[0].rootObj;		buttonPrefab.SetActive(false);
			abilityButtons.Clear();
			
			List<Ability> list=AbilityManager.GetAbilityList();
			for(int i=0; i<list.Count; i++) AddAbilityButton(i, list[i].icon, list[i].GetUseLimitText());	//list[i].name);
			
			buttonClearSelect.Init();
			buttonClearSelect.button.onClick.AddListener(() => OnClearSelectButton());
			buttonClearSelect.SetActive(false);
			
			canvasGroup.alpha=1;
		}
		
		public static void NewAbility(Ability ability){
			instance.AddAbilityButton(instance.abilityButtons.Count, ability.icon, ability.GetUseLimitText());
		}
		
		private void AddAbilityButton(int idx, Sprite icon, string txt){
			//if(idx>0) abilityButtons.Add(UIButton.Clone(abilityButtons[0].rootObj, "Button"+(idx)));
			abilityButtons.Add(UIButton.Clone(buttonPrefab, "Button"+(idx)));
			abilityButtons[idx].rootObj.SetActive(true);
			abilityButtons[idx].Init();
			
			if(UIControl.InTouchMode() && UIControl.UseDragNDropTgtMode()) abilityButtons[idx].SetClickCallback(OnAbilityButton, null);
			else abilityButtons[idx].button.onClick.AddListener(() => OnAbilityButton(idx));
			
			if(!UIControl.InTouchMode() || !UIControl.UseSelectNDeployTgtMode()) abilityButtons[idx].SetCallback(OnHoverAbilityButton, OnExitAbilityButton);
			
			//~ abilityButtons[idx].button.onClick.AddListener(() => OnAbilityButton(idx));
			//~ if(!UIControl.InTouchMode()) abilityButtons[idx].SetCallback(OnHoverAbilityButton, OnExitAbilityButton);
			
			if(icon!=null) abilityButtons[idx].image.sprite=icon;
			else abilityButtons[idx].image.enabled=false;
			
			abilityButtons[idx].label.text=txt;
			
			cooldownSlider.Add(abilityButtons[idx].rootT.GetChild(0).gameObject.GetComponent<Slider>());
		}
		
		
		public void OnHoverAbilityButton(GameObject butObj){
			int idx=UI.GetItemIndex(butObj, abilityButtons);
			UITooltip.Show(AbilityManager.GetAbility(idx), UI.GetCorner(abilityButtons[idx].rectT, 2), 3, new Vector3(0, .25f, 0));
		}
		public void OnExitAbilityButton(GameObject butObj){
			int idx=UI.GetItemIndex(butObj, abilityButtons);
			if(AbilityManager.GetPendingTargetAbilityIndex()==idx || UIControl.GetPendingAbilityIdx()==idx) return;
			UITooltip.Hide();
		}
		
		
		private int touchModeButtonIdx=-1;
		public void OnAbilityButton(GameObject butObj){ OnAbilityButton(UI.GetItemIndex(butObj, abilityButtons)); OnHoverAbilityButton(butObj); }
		public void OnAbilityButton(int idx){
			if(UIControl.UseDragNDropMode()) TowerManager.ExitDragNDropPhase();
			SelectControl.ClearUnit();
			UITowerSelect.Hide();
			
			//int idx=UI.GetItemIndex(butObj, abilityButtons);
			
			if(pendingTgtSelectIdx>=0){
				abilityButtons[pendingTgtSelectIdx].SetHighlight(false);
				pendingTgtSelectIdx=-1;
			}
			
			if(UIControl.InTargetSelectionMode()){
				if(AbilityManager.GetPendingTargetAbilityIndex()==idx || UIControl.GetPendingAbilityIdx()==idx){
					abilityButtons[idx].SetHighlight(false);
					UIControl.ClearSelectedAbility();
					buttonClearSelect.SetActive(false);
					CameraControl.EnableScrollCursorDrag();
					if(UIControl.InTouchMode()) OnExitAbilityButton(abilityButtons[idx].rootObj);
					return;
				}
			}
			
			if(UIControl.InTouchMode()){
				if(!AbilityManager.RequireTargetSelection(idx) && touchModeButtonIdx!=idx){
					if(touchModeButtonIdx>=0) ClearTouchModeSelect();
					
					OnHoverAbilityButton(abilityButtons[idx].rootObj);
					
					touchModeButtonIdx=idx;
					abilityButtons[touchModeButtonIdx].SetHighlight(true);
					buttonClearSelect.SetActive(true);
					return;
				}
				
				ClearTouchModeSelect();
			}
			
			Ability._Status status=AbilityManager.IsReady(idx);
			
			if(status!=Ability._Status.Ready){
				if(status==Ability._Status.OnCooldown) GameControl.InvalidAction("Ability is on cooldown");
				if(status==Ability._Status.InsufficientRsc) GameControl.InvalidAction("Insufficient resource");
				if(status==Ability._Status.UseLimitReached) GameControl.InvalidAction("Use limit exceeded");
				return;
			}
			
			if(AbilityManager.RequireTargetSelection(idx)){
				UIControl.SelectAbility(idx);
				abilityButtons[idx].SetHighlight(true);
				buttonClearSelect.SetActive(true);
				
				pendingTgtSelectIdx=idx;
				
				CameraControl.DisableScrollCursorDrag();
				
				if(UIControl.InTouchMode()) OnHoverAbilityButton(abilityButtons[idx].rootObj);
			}
			else{
				AbilityManager.ActivateAbility(idx);
				abilityButtons[idx].label.text=AbilityManager.GetAbility(idx).GetUseLimitText();
				
				if(UIControl.InTouchMode()) OnExitAbilityButton(abilityButtons[idx].rootObj);
			}
		}
		
		private int pendingTgtSelectIdx=-1;
		
		//public static bool PendingTouchModeInput(){ return instance.touchModeButtonIdx>=0; }
		
		public static void ClearTouchModeSelect(){ instance._ClearTouchModeSelect(); }
		public void _ClearTouchModeSelect(){
			if(touchModeButtonIdx<0) return;
			abilityButtons[touchModeButtonIdx].SetHighlight(false);
			touchModeButtonIdx=-1;
			OnExitAbilityButton(null);
			
			CameraControl.EnableScrollCursorDrag();
		}
		
		
		void OnEnable(){
			TDTK.onActivateAbilityE += OnActivateAbility;
		}
		void OnDisable(){
			TDTK.onActivateAbilityE -= OnActivateAbility;
		}
		
		
		void Update(){
			List<Ability> list=AbilityManager.GetAbilityList();
			for(int i=0; i<list.Count; i++){
				if(!list[i].OnCooldown()){
					if(cooldownSlider[i].value<=1) cooldownSlider[i].value=1;
					if(abilityButtons[i].image2.enabled && !list[i].UseLimitReached()) abilityButtons[i].image2.enabled=false;
				}
				else cooldownSlider[i].value=list[i].GetCDRatio();
			}
		}
		
		
		void OnActivateAbility(Ability ability){
			abilityButtons[ability.instanceID].label.text=ability.GetUseLimitText();
			abilityButtons[ability.instanceID].image2.enabled=true;
			abilityButtons[ability.instanceID].SetHighlight(false);
			
			UITooltip.Hide();
			buttonClearSelect.SetActive(false);
			
			pendingTgtSelectIdx=-1;
			
			CameraControl.EnableScrollCursorDrag();
		}
		
		
		
		public static void ClearSelect(){ instance.OnClearSelectButton(); }
		
		public void OnClearSelectButton(){
			if(!AbilityManager.IsEnabled()) return;
			
			if(touchModeButtonIdx>=0) ClearTouchModeSelect();
			else{
				CameraControl.EnableScrollCursorDrag();
				UIControl.ClearSelectedAbility();
				for(int i=0; i<abilityButtons.Count; i++){
					abilityButtons[i].SetHighlight(false);
				}
			}
			
			buttonClearSelect.SetActive(false);
			
			UITooltip.Hide();
		}
		
		
		//public static void Show(SelectInfo info, bool instant=false){ if(instance!=null) instance._Show(info, instant); }
		//~ public void _Show(SelectInfo info, bool instant=false){
			//base._Show();
			////base._Show(instant);
		//}
		//public static void Hide(bool instant=false){ 
			//if(instance!=null && instance.thisObj.activeInHierarchy) instance._Hide(instant);
		//}
		
		
		
	}

}
