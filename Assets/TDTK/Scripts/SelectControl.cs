using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TDTK;

namespace TDTK{

	public class SelectControl : MonoBehaviour {
		
		private static Unit selectedUnit;
		public static Unit GetSelectedUnit(){ return selectedUnit; }
		
		public RangeIndicator rIndicator;
		public GameObject nIndicator;
		private Transform nIndicatorT;
		
		public static SelectControl instance;
		
		void Awake(){
			instance=this;
		}
		
		void Start(){
			float gridSize=TowerManager.GetGridSize();
			nIndicatorT=nIndicator.transform;
			nIndicatorT.localScale=new Vector3(gridSize, gridSize, gridSize);
			
			_ClearAll();
		}
		
		public static void RefreshUnit(){ instance._SelectUnit(selectedUnit); }
		
		public static void SelectUnit(Unit unit){ instance._SelectUnit(unit); }
		public void _SelectUnit(Unit unit){
			selectedUnit=unit;
			
			if(selectedUnit.IsTower()){
				//selectedUnit.GetTower().IsTurret()
				//selectedUnit.GetTower().IsAOE()
				//selectedUnit.GetTower().IsMine()
				//selectedUnit.GetTower().IsSupport()
				//selectedUnit.GetTower().IsResource()
				//selectedUnit.GetTower().IsBlock()				
				
				if(selectedUnit.GetTower().IsTurret() || selectedUnit.GetTower().IsAOE() || selectedUnit.GetTower().IsSupport() || selectedUnit.GetTower().IsMine()){
					rIndicator.Show(selectedUnit);
				}
			}
		}
		public static void ClearUnit(){ instance._ClearUnit(); }
		public void _ClearUnit(){ selectedUnit=null; rIndicator.Hide(); }
		
		
		public static void SelectNode(BuildPlatform platform, int nodeID){ instance._SelectNode(platform, nodeID); }
		public void _SelectNode(BuildPlatform platform, int nodeID){
			//ClearNode();
			nIndicatorT.position=platform.GetNode(nodeID).pos;
			nIndicatorT.rotation=platform.GetRot();
			nIndicator.SetActive(true);
		}
		public static void ClearNode(){ instance.nIndicator.SetActive(false); }
		
		
		public static void ClearAll(){ instance._ClearAll(); }
		public void _ClearAll(){
			ClearUnit();
			ClearNode();
		}
		
	}

}