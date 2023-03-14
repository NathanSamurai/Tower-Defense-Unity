using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIOverlayHP : MonoBehaviour {
		
		
		public GameObject rootOverlayItem;
		
		[Space(8)] //[HideInInspector]
		public List<UIHPOverlayItem> overlayItemList=new List<UIHPOverlayItem>();
		
		//private static UIOverlayHP instance;
		
		void Awake() {
			//instance=this;
			
			for(int i=0; i<30; i++){
				if(i==0) overlayItemList.Add(rootOverlayItem.AddComponent<UIHPOverlayItem>());
				else overlayItemList.Add(UI.Clone(rootOverlayItem).GetComponent<UIHPOverlayItem>());
				
				overlayItemList[i].Init();
			}
		}
		
		
		void OnEnable(){ TDTK.onNewUnitE += AddUnit; }
		void OnDisable(){ TDTK.onNewUnitE -= AddUnit; }
		
		
		//public static void AddUnit(Unit unit){ instance._AddUnit(unit); }
		public void AddUnit(Unit unit){
			int index=GetUnusedItemIndex();
			overlayItemList[index].SetUnit(unit);
		}
		
		//public static void RemoveUnit(Unit){ instance._RemoveUnit(unit); }
		//public void _RemoveUnit(Unit){  }
		
		
		private int GetUnusedItemIndex(){
			for(int i=0; i<overlayItemList.Count; i++){
				if(overlayItemList[i].unit!=null) continue;
				return i;
			}
			
			overlayItemList.Add(UI.Clone(rootOverlayItem).GetComponent<UIHPOverlayItem>());
			return overlayItemList.Count-1;
		}
		
	}


	public class UIHPOverlayItem : MonoBehaviour {
		
		[HideInInspector] public Unit unit;
		
		private Slider slider;
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasG;
		
		private Slider sliderSH;
		
		public void Init(){
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasG=thisObj.GetComponent<CanvasGroup>();
			
			slider=thisObj.GetComponent<Slider>();
			sliderSH=thisObj.transform.GetChild(2).GetComponent<Slider>();
		}
		
		void Update(){
			if(unit==null || unit.hp<=0 || !unit.GetObj().activeInHierarchy){
				unit=null;
				thisObj.SetActive(false);
				return;
			}
			
			UpdateScreenPos();
			
			slider.value=unit.GetHPRatio();
			sliderSH.value=unit.GetSHRatio();
			
			if(!UIControl.AlwaysShowHPOverlay()){
				canvasG.alpha = (slider.value>=1 && (sliderSH.value<=0 || sliderSH.value>=1)) ? 0 : 1 ;
			}
		}
		
		public void SetUnit(Unit tgtUnit){ 
			unit=tgtUnit;
			
			if(thisObj==null) Init();
			
			Update();
			
			thisObj.SetActive(true);
		}
		
		void UpdateScreenPos(){
			Vector3 screenPos=Camera.main.WorldToScreenPoint(unit.GetPos()+new Vector3(0, .5f, 0));
			screenPos.z=0;
			rectT.localPosition=screenPos*UI.GetScaleFactor();
		}
		
	}

}