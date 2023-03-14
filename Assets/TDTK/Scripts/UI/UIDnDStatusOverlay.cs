using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIDnDStatusOverlay : MonoBehaviour {

		public Text dndLabel;	//for drag n drop tower when build is invalid
		private GameObject dndLabelObj;	
		private RectTransform dndLabelRectT;	//for drag n drop tower when build is invalid
		
		void Start () {
			if(UIControl.UseDragNDropMode() && dndLabel!=null){
				dndLabelObj=dndLabel.gameObject;
				dndLabelRectT=dndLabelObj.GetComponent<RectTransform>();
			}
			else gameObject.SetActive(false);
		}
		
		void Update () {
			if(TowerManager.InDragNDropPhase()){
				if(TowerManager.GetDragNDropTower()!=null){
					Vector3 towerPos=TowerManager.GetDragNDropTower().GetPos();
					Vector3 screenPos=Camera.main.WorldToScreenPoint(towerPos)*UI.GetScaleFactor();
					screenPos.z=0;
					screenPos.y+=50;
					dndLabelRectT.localPosition=screenPos;
					
					if(TowerManager.DnDHasValidPos()){
						dndLabel.text="";
					}
					else{
						dndLabel.gameObject.SetActive(true);
						dndLabel.text="Invalid Pos";
					}
				}
				else{
					dndLabel.text="";
				}
			}
			else{
				if(dndLabel.gameObject.activeInHierarchy) dndLabel.gameObject.SetActive(false);
			}
		}
		
	}

}