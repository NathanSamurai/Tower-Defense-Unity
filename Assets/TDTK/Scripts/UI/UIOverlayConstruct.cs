using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIOverlayConstruct : MonoBehaviour {
		
		public GameObject rootOverlayItem;
		public List<UIConstructOverlayItem> overlayItemList=new List<UIConstructOverlayItem>();
		
		//private static UIConstructOverlay instance;
		
		void Awake() {
			//instance=this;
			
			for(int i=0; i<2; i++){
				if(i==0) overlayItemList.Add(rootOverlayItem.AddComponent<UIConstructOverlayItem>());
				else overlayItemList.Add(UI.Clone(rootOverlayItem).GetComponent<UIConstructOverlayItem>());
				
				overlayItemList[i].Init();
			}
		}
		
		
		void OnEnable(){ 
			TDTK.onTowerConstructingE += _AddTower;
		}
		void OnDisable(){ 
			TDTK.onTowerConstructingE -= _AddTower;
		}
		
		
		//public static void AddTower(UnitTower tower){ instance._AddTower(tower); }
		public void _AddTower(UnitTower tower){
			int index=GetUnusedItemIndex();
			
			overlayItemList[index].SetTower(tower);
			overlayItemList[index].gameObject.SetActive(true);
		}
		
		
		private int GetUnusedItemIndex(){
			for(int i=0; i<overlayItemList.Count; i++){
				if(overlayItemList[i].tower!=null) continue;
				return i;
			}
			
			overlayItemList.Add(UI.Clone(rootOverlayItem).GetComponent<UIConstructOverlayItem>());
			return overlayItemList.Count-1;
		}
		
	}




	public class UIConstructOverlayItem : MonoBehaviour {
		
		[HideInInspector] public UnitTower tower;
		
		private Slider slider;
		private GameObject thisObj;
		private RectTransform rectT;
		//private CanvasGroup canvasG;
		
		public void Init(){
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			//canvasG=thisObj.GetComponent<CanvasGroup>();
			
			slider=thisObj.GetComponent<Slider>();
			thisObj.SetActive(false);
		}
		
		void Update(){
			if(tower==null){
				if(thisObj.activeInHierarchy) thisObj.SetActive(false);
				return;
			}
			
			slider.value=tower.GetConstructionStatus();
			
			UpdateScreenPosition();
			
			if(!tower.InConstruction()){
				tower=null;
				thisObj.SetActive(false);
			}
		}
		
		public void SetTower(UnitTower tgtTower){ 
			tower=tgtTower;
			
			if(thisObj==null) Init();
			
			slider.value=tower.GetConstructionStatus();
			
			UpdateScreenPosition();
		}
		
		public void UpdateScreenPosition(){
			Vector3 screenPos=Camera.main.WorldToScreenPoint(tower.GetPos());//+new Vector3(0, 0, 0));
			screenPos.z=0;
			rectT.localPosition=(screenPos+new Vector3(0, -20, 0))*UI.GetScaleFactor();
		}
		
	}
	
}