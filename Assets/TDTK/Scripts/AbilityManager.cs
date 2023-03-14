using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{

	public class AbilityManager : MonoBehaviour {
		
		public static bool IsEnabled(){ return instance!=null; }
		
		public bool useRscManagerForCost=false;
		public static bool UseRscManagerForCost(){ return instance.useRscManagerForCost; }
		
		public bool startWithFullRsc=true;
		public int rsc=0;
		public int rscCap=100;
		public float rscRegenRate=2;
		private float rscRegenMultiplier=1;
		private float rscRegenCache=0;
		public static bool HasSufficientRsc(float value){ return instance.rsc>=value; }
		public static void GainRsc(int value){ 
			if(instance==null) return; 
			instance.rsc+=value; 
			TDTK.OnAbilityRscChanged(instance.rsc);
		}
		public static void SpendRsc(int value){ 
			instance.rsc-=value; 
			TDTK.OnAbilityRscChanged(instance.rsc);
		}
		
		public static int GetRsc(){ return instance.rsc; }
		public static int GetRscCap(){ return instance.rscCap; }
		public static float GetRscRatio(){ return (float)GetRsc()/(float)GetRscCap(); }
		
		public static void MultiplyRscCap(float mul){
			int gain=(int)Mathf.Round(instance.rscCap*mul)-instance.rscCap;
			instance.rsc+=gain;
			instance.rscCap+=gain;
			TDTK.OnAbilityRscChanged(instance.rsc); 
		}
		public static void ModifyRscCap(int value){
			instance.rscCap+=value;
			if(value>0) instance.rsc+=value;
			instance.rsc=Mathf.Min(instance.rsc, instance.rscCap);
			TDTK.OnAbilityRscChanged(instance.rsc); 
		}
		public static void ModifyRscRegen(float value){ instance.rscRegenRate+=value; }
		public static void ModifyRscRegenMultiplier(float value){ instance.rscRegenMultiplier+=value; }
		
		
		public List<int> unavailablePrefabIDList=new List<int>();
		
		public List<Ability> abilityList=new List<Ability>();
		public static List<Ability> GetAbilityList(){ return instance.abilityList; }
		public static Ability GetAbility(int idx){ return instance.abilityList[idx]; }
		
		private static int pendingTgtAbilityIdx=-1;
		public static int GetPendingTargetAbilityIndex(){ return pendingTgtAbilityIdx; }
		public static bool InTargetSelectionMode(){ return pendingTgtAbilityIdx>=0; }
		
		private static float tgtSelectCooldown=0;
		
		public Transform tgtSelectIndicator;
		public Transform activeTgtSelectIndicator;
		
		private static AbilityManager instance;
		public static AbilityManager GetInstance(){ return instance; }
		
		void Awake(){
			instance=this;
			
			List<Ability> dbList=Ability_DB.GetList();
			for(int i=0; i<dbList.Count; i++){
				if(!unavailablePrefabIDList.Contains(dbList[i].prefabID) && !dbList[i].hideInInspector) abilityList.Add(dbList[i].Clone());
			}
			
			tgtSelectIndicator=Instantiate(tgtSelectIndicator.gameObject).transform;
			tgtSelectIndicator.parent=transform;
			tgtSelectIndicator.gameObject.SetActive(false);
			
			pendingTgtAbilityIdx=-1;
		}
		
		IEnumerator Start(){
			yield return null;
			
			if(startWithFullRsc) rsc=rscCap;
			TDTK.OnAbilityRscChanged(instance.rsc); 
			
			for(int i=0; i<abilityList.Count; i++) abilityList[i].Init(i, transform);
		}
		
		
		public static void AddAbility(int prefabID, int replacePrefabID=-1){	//called from perk to add new ability
			if(instance==null) return;
			
			int replaceIdx=-1;
			if(replacePrefabID>=0){
				for(int i=0; i<instance.abilityList.Count; i++){
					if(instance.abilityList[i].prefabID==replacePrefabID){ replaceIdx=i; break; }
				}
			}
			
			Ability newAbility=Ability_DB.GetPrefab(prefabID).Clone();
			
			if(replaceIdx<0){
				instance.abilityList.Add(newAbility);
				newAbility.Init(instance.abilityList.Count-1, instance.transform);
			}
			else{
				instance.abilityList[replaceIdx]=newAbility;
				newAbility.Init(replaceIdx, instance.transform);
			}
			
			TDTK.OnNewAbility(newAbility);
		}
		
		
		void FixedUpdate(){
			if(rsc<rscCap){
				rscRegenCache+=rscRegenRate*rscRegenMultiplier*Time.fixedDeltaTime;
				if(rscRegenCache>1){
					float gain=Mathf.Floor(rscRegenCache);
					rscRegenCache-=gain;
					rsc+=(int)gain;
					rsc=Mathf.Min(rsc, rscCap);
					TDTK.OnAbilityRscChanged(instance.rsc); 
				}
			}
			
			for(int i=0; i<abilityList.Count; i++) abilityList[i].IterateCooldown();//currentCD-=Time.fixedDeltaTime;
		}
		
		void Update(){
			if(!InTargetSelectionMode()) return;
			
			TargetingInfo info=OnCursorDown(Input.mousePosition);
			if(info.valid){
				if(info.tgtUnit==null) activeTgtSelectIndicator.position=info.pos;
				else activeTgtSelectIndicator.position=info.tgtUnit.GetPos();
			}
			else activeTgtSelectIndicator.position=info.pos;
		}
		
		
		
		public static bool RequireTargetSelection(int idx){ return instance.abilityList[idx].requireTargetSelection; }
		public static Ability._Status IsReady(int idx){ return instance.abilityList[idx].IsReady(); }
		
		
		public static void SelectAbility(int idx){ instance._SelectAbility(idx); }
		public void _SelectAbility(int idx){
			if(!abilityList[idx].requireTargetSelection) Debug.Log("select ability that doesn't require target input");
			
			ClearSelect();
			
			if(abilityList[idx].tgtSelectIndicator==null) activeTgtSelectIndicator=tgtSelectIndicator;
			else activeTgtSelectIndicator=abilityList[idx].tgtSelectIndicator;
			
			pendingTgtAbilityIdx=idx;
			activeTgtSelectIndicator.gameObject.SetActive(true);
			
			tgtSelectCooldown=Time.time;
			
			Vector3 scale=new Vector3(1, 1, 1) * abilityList[idx].GetAOERange();
			activeTgtSelectIndicator.transform.localScale=scale;
		}

		public static void ClearSelect(){
			if(instance.activeTgtSelectIndicator!=null) instance.activeTgtSelectIndicator.gameObject.SetActive(false);
			pendingTgtAbilityIdx=-1;
		}

		public static void ActivateAbility(int idx, Vector3 pos=default(Vector3)){ instance._ActivateAbility(idx, pos); }
		public void _ActivateAbility(int idx, Vector3 pos=default(Vector3)){
			//Debug.Log("_ActivateAbility   "+idx+"   "+pendingTgtAbilityIdx);
			
			if(idx<0 && Time.time-tgtSelectCooldown<0.2) return;
			
			if(idx<0){
				if(pendingTgtAbilityIdx>=0) idx=pendingTgtAbilityIdx;
				else return;
			}
			
			//Debug.Log("_ActivateAbility");
			abilityList[idx].Activate(pos);
			ClearSelect();
			
			TDTK.OnActivateAbility(abilityList[idx]);
		}
		
		
		public static TargetingInfo OnCursorDown(Vector3 pointer, int idx=-1){
			if(idx<0){
				if(pendingTgtAbilityIdx>=0) idx=pendingTgtAbilityIdx;
				else return new TargetingInfo();
			}
			
			Ray ray = CameraControl.GetMainCam().ScreenPointToRay(pointer);
			//Transform cameraT=CameraControl.GetMainCam().transform;
			//Ray ray = new Ray(cameraT.position, cameraT.forward);
			RaycastHit hit;	LayerMask mask=1<<TDTK.GetLayerTerrain();
			
			if(GetAbility(idx).requireUnitAsTarget){
				if(GetAbility(idx).MustTargetHostile()) mask|=1<<TDTK.GetLayerCreep();
				else if(GetAbility(idx).MustTargetFriendly()) mask|=1<<TDTK.GetLayerTower();
				else mask|=1<<TDTK.GetLayerTower() | 1<<TDTK.GetLayerCreep();
			}
			
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
				if(hit.collider.gameObject.layer==TDTK.GetLayerTerrain()){
					return new TargetingInfo(hit.point, !GetAbility(idx).requireUnitAsTarget);
				}
				else{
					return new TargetingInfo(hit.collider.gameObject.GetComponent<Unit>());
				}
			}
			
			return new TargetingInfo();
		}
	}
	
	
	public class TargetingInfo{
		public bool valid=false;
		
		public Vector3 pos;
		public Unit tgtUnit;
		
		public TargetingInfo(){ valid=false; }
		public TargetingInfo(Vector3 p, bool flag){ pos=p; valid=flag; }
		public TargetingInfo(Unit u){ tgtUnit=u; pos=tgtUnit.GetPos(); valid=true; }
	}
	
}