using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TDTK;

namespace TDTK{

	[RequireComponent(typeof(LineRenderer))]
	public class RangeIndicator : MonoBehaviour {
		
		private bool refreshEveryFrame=false;
		
		public int maxSegmentCount=50;
		public float radius = 2;
		
		private int segments = 50;
		private float fov=360;
		private float angle=0;
		
		private LineRenderer line;
		
		private Unit activeUnit;

		private Transform thisT;
		private GameObject thisObj;
		
		void Awake(){
			thisT=transform;
			thisObj=gameObject;
			
			line = thisObj.GetComponent<LineRenderer>();
			line.useWorldSpace = true;
			
			thisT.rotation=Quaternion.Euler(90, 0, 0);
			
			thisObj.SetActive(false);
		}
		
		public void Show(Unit unit){
			activeUnit=unit;
			
			refreshEveryFrame=activeUnit.IsTower() && activeUnit.GetTower().isPreview;
			
			thisT.position=unit.GetPos();
			CreatePoints (unit);
			thisObj.SetActive(true);
		}
		
		public void Hide(){ thisObj.SetActive(false); }
		
		
		void Update(){
			if(activeUnit==null) Hide();
			
			if(refreshEveryFrame){
				thisT.position=activeUnit.GetPos();
				CreatePoints(activeUnit);
			}
		}
		
		
		
		
		
		void CreatePoints(Unit unit){
			float x; float z;
			
			if(unit.IsTurret()) radius=unit.GetAttackRange();
			else if(unit.IsAOE()) radius=unit.GetAttackRange_AOE();
			else if(unit.IsSupport()) radius=unit.GetAttackRange_Support();
			else if(unit.IsMine()) radius=unit.GetAOERange_Mine();
			else radius =0;
			
			fov=360;	angle=0;
			segments=maxSegmentCount;
			
			if(unit.UseDirectionalTargeting()){
				fov=unit.targetingFov;
				segments=(int)Mathf.Ceil(maxSegmentCount*fov/360f);
				
				Quaternion dir=unit.transform.rotation*Quaternion.Euler(0, unit.targetingDir, 0);
				angle=dir.eulerAngles.y-fov*0.5f;
			}
			
			line.positionCount=segments+1;
			line.startWidth=radius*.25f;
			
			for (int i = 0; i < (segments + 1); i++){
				x = Mathf.Sin (Mathf.Deg2Rad * angle) * (radius-line.startWidth*.5f);
				z = Mathf.Cos (Mathf.Deg2Rad * angle) * (radius-line.startWidth*.5f);

				line.SetPosition (i, thisT.position+new Vector3(x,0,z) );

				angle += (fov / segments);
			}
		}
		
	}

}