using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UITooltip : UIScreen {
		
		public Text labelName;
		public Text labelDesp;
		private RectTransform rectName;
		private RectTransform rectDesp;
		
		private float minHeight=120;
		
		public List<UIObject> rscItemList=new List<UIObject>();
		
		private static UITooltip instance;
		
		public override void Awake(){
			base.Awake();
			
			instance=this;
		}
		
		public override void Start(){
			base.Start();
			
			for(int i=0; i<RscManager.GetResourceCount(); i++){
				if(i>0) rscItemList.Add(new UIObject(UI.Clone(rscItemList[0].rootObj)));
				rscItemList[i].Init();
				rscItemList[i].SetImage(RscManager.GetRscIcon(i));
			}
			
			rectName=labelName.gameObject.GetComponent<RectTransform>();
			rectDesp=labelDesp.gameObject.GetComponent<RectTransform>();
			
			canvasGroup.alpha=0;
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			//thisObj.SetActive(false);
		}
		
		
		void Update(){
			if(minHeight<=0) rectT.sizeDelta=new Vector2(rectT.sizeDelta.x, 43+rectName.sizeDelta.y+rectDesp.sizeDelta.y);
			else rectT.sizeDelta=new Vector2(rectT.sizeDelta.x, Mathf.Max(minHeight, 43+rectName.sizeDelta.y+rectDesp.sizeDelta.y));
		}
		
		
		public static void Show(UnitTower tower){
			instance.rectT.localPosition=new Vector3(10, 10, 0);
			instance._Show(tower, instance.rectT.position, 0, Vector3.zero);
		}
		
		public static void Show(UnitTower tower, Vector3 pos, int pivotCorner=2, Vector3 offset=default(Vector3)){ 
			instance._Show(tower, pos, pivotCorner, offset);
		}
		public void _Show(UnitTower tower, Vector3 pos, int pivotCorner=2, Vector3 offset=default(Vector3)){
			SetPivot(pivotCorner);
			
			labelName.text=tower.unitName;
			labelDesp.text=tower.desp;			labelDesp.enabled=true;
			
			List<float> cost=tower.GetCost();
			for(int i=0; i<RscManager.GetResourceCount(); i++){
				rscItemList[i].SetImage(RscManager.GetRscIcon(i));
				rscItemList[i].SetLabel(i<cost.Count ? cost[i].ToString("f0") : "0");
			}
			
			minHeight=120;
			
			rectT.position=pos+offset;
			
			canvasGroup.alpha=1;
			thisObj.SetActive(true);
		}
		
		
		public static void ShowSell(UnitTower tower, Vector3 pos, int pivotCorner=2, Vector3 offset=default(Vector3)){ 
			instance._ShowSell(tower, pos, pivotCorner, offset);
		}
		public void _ShowSell(UnitTower tower, Vector3 pos, int pivotCorner=2, Vector3 offset=default(Vector3)){
			SetPivot(pivotCorner);
			
			labelName.text="Sell "+tower.unitName;
			labelDesp.enabled=false;
			
			List<float> cost=tower.GetSellValue();
			for(int i=0; i<RscManager.GetResourceCount(); i++){
				rscItemList[i].SetImage(RscManager.GetRscIcon(i));
				rscItemList[i].SetLabel(i<cost.Count ? cost[i].ToString("f0") : "0");
			}
			
			minHeight=-1;
			rectT.position=pos+offset;
			
			canvasGroup.alpha=1;
			thisObj.SetActive(true);
		}
			
		public static void ShowUpgrade(UnitTower tower, int uIdx, Vector3 pos, int pivotCorner=2, Vector3 offset=default(Vector3)){ 
			instance._ShowUpgrade(tower, uIdx, pos, pivotCorner, offset);
		}
		public void _ShowUpgrade(UnitTower tower, int uIdx, Vector3 pos, int pivotCorner=2, Vector3 offset=default(Vector3)){
			SetPivot(pivotCorner);
			
			if(tower.GetUpgradeType()==0) labelName.text="Upgrade";
			else labelName.text="Upgrade to "+tower.GetUpgradeTower(uIdx).unitName;
			
			labelDesp.enabled=false;
			
			List<float> cost=tower.GetUpgradeCost(uIdx);
			for(int i=0; i<RscManager.GetResourceCount(); i++){
				rscItemList[i].SetImage(RscManager.GetRscIcon(i));
				rscItemList[i].SetLabel(i<cost.Count ? cost[i].ToString("f0") : "0");
			}
			
			minHeight=-1;
			rectT.position=pos+offset;
			
			canvasGroup.alpha=1;
			thisObj.SetActive(true);
		}
		
		
		public static void Show(Ability ability, Vector3 pos, int pivotCorner=2, Vector3 offset=default(Vector3)){ 
			instance._Show(ability, pos, pivotCorner, offset);
		}
		public void _Show(Ability ability, Vector3 pos, int pivotCorner=2, Vector3 offset=default(Vector3)){
			SetPivot(pivotCorner);
			
			labelName.text=ability.name;
			labelDesp.text=ability.desp;	labelDesp.enabled=true;
			
			if(AbilityManager.UseRscManagerForCost()){
				List<float> cost=ability.GetCostRsc();
				for(int i=0; i<RscManager.GetResourceCount(); i++){
					rscItemList[i].SetImage(RscManager.GetRscIcon(i));
					rscItemList[i].SetLabel(i<cost.Count ? cost[i].ToString("f0") : "0");
					rscItemList[i].SetActive(true);
				}
			}
			else{
				rscItemList[0].SetImage(Ability_DB.GetRscIcon());
				rscItemList[0].SetLabel(ability.GetCost().ToString("f0"));
				for(int i=1; i<RscManager.GetResourceCount(); i++) rscItemList[i].SetActive(false);
			}
			
			minHeight=120;
			
			rectT.position=pos+offset;
			
			canvasGroup.alpha=1;
			thisObj.SetActive(true);
		}
		
		
		
		private void SetPivot(int pivotCorner){
			if(pivotCorner==0) rectT.pivot=new Vector3(0, 0);
			if(pivotCorner==1) rectT.pivot=new Vector3(0, 1);
			if(pivotCorner==2) rectT.pivot=new Vector3(1, 1);
			if(pivotCorner==3) rectT.pivot=new Vector3(1, 0);
		}
		
		
		
		public static void Hide(){ 
			instance.canvasGroup.alpha=0;
			//instance.thisObj.SetActive(false);
		}
	
	}

}