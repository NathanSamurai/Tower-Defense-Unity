using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TDTK{

	public class UnitTower : Unit {
		
		public override _UnitType GetUnitType(){ return _UnitType.Tower; }
		public override bool IsTower(){ return true; }
		public override UnitTower GetTower(){ return this; }
		
		public override bool IsPreview(){ return isPreview; }
		
		
		public enum _TowerType{ Turret, AOE, Support, Resource, Mine, Block }
		[Header("Tower Setting")] public _TowerType towerType;
		
		
		public bool isTurret=true;
		public bool isAOE;
		public bool isSupport;
		public bool isResource;
		public bool isMine;
		
		public override bool IsTurret(){ return isTurret; }
		public override bool IsAOE(){ return isAOE; }
		public override bool IsSupport(){ return isSupport; }
		public override bool IsResource(){ return isResource; }
		public override bool IsMine(){ return isMine; }
		public bool IsBlock(){ return !isTurret & !isAOE & !isSupport & !isResource & !isMine; }
		
		
		/*
		public void ConvertType(){
			isTurret=false;
			isAOE=false;
			isSupport=false;
			isResource=false;
			isMine=false;
			
			if(IsTurret()) isTurret=true;
			if(IsAOE()) isAOE=true;
			if(IsSupport()) isSupport=true;
			if(IsResource()) isResource=true;
			if(IsMine()) isMine=true;
			//if(IsBlock()) isTurret=true;
		}
		*/
		
		
		
		//~ public override bool IsTurret(){ return towerType==_TowerType.Turret; }
		//~ public override bool IsAOE(){ return towerType==_TowerType.AOE; }
		//~ public override bool IsSupport(){ return towerType==_TowerType.Support; }
		//~ public override bool IsResource(){ return towerType==_TowerType.Resource; }
		//~ public override bool IsMine(){ return towerType==_TowerType.Mine; }
		//~ public bool IsBlock(){ return towerType==_TowerType.Block; }
		
		
		public enum _TargetGroup{ All, Ground, Air }
		[Space(8)] public _TargetGroup targetGroup;
		public override _TargetGroup GetTargetGroup(){ return targetGroup; }
		
		public bool activeBetweenWave;	//for resource tower, check to enable rsc generation between wave
		
		[Space(8)]
		public bool isPreview=false;
		
		public bool prebuilt=false;
		public bool disableSelling=false;
		public int lifeCostOnDestroyed;
		
		public bool hideInInspector=false;
		
		
		private float constructDuration=1;
		private float constructRemained=1;
		public override bool InConstruction(){ return constructRemained>0; }
		public float GetConstructionStatus(){ 
			if(constructState==_ConstructState.Sell) return constructRemained/constructDuration;
			else return 1f-constructRemained/constructDuration; 
		}
		
		
		public enum _ConstructState{ Build, Sell, Upgrade }
		public _ConstructState constructState;
		//public void SetToDragNDrop(){ constructState=_ConstructState.DnD; }
		
		[Space(5)] 
		public int limitInScene=0;
		
		private int typeID=-1;		//default to prefabID, but if the tower is upgraded from something else, the typeID will be inherit from it's parent tower
											//disabled, to enable it, Check Upgrade(), TowerManager.BuildTower. And disable the TowerManager.CheckTowerCounterLimit() check in UITowerSelect
		public int GetTypeID(){ return typeID<0 ? prefabID : typeID; }
		public void SetTypeID(int ID){ typeID=ID; }
		
		[Space(5)] public bool destroyOnTrigger=true;	//for mine only
		
		[Header("Visual and Audio")]
		public VisualObject effectBuilding=new VisualObject();
		public VisualObject effectBuilt=new VisualObject();
		public VisualObject effectSold=new VisualObject();
		public VisualObject effectDestroyed=new VisualObject();
		
		[Space(5)] 
		public AudioClip soundBuilding;
		public AudioClip soundBuilt;
		public AudioClip soundUpgrading;
		public AudioClip soundUpgraded;
		public AudioClip soundSold;
		public AudioClip soundDestroyed;


		[SerializeField] AK.Wwise.Event TowerSold, TowerBuilt;
		
		[Space(10)][Tooltip("Use in Free-Form mode only, specify the space occupied by the tower")]
		public float radius=.5f;
		
		
		public override void Awake(){
			base.Awake();
			
			for(int i=0; i<upgradeTowerList.Count; i++){
				if(upgradeTowerList[i]==null) upgradeTowerList.RemoveAt(i);
			}
		}
		
		public void Init(){
			if(!isPreview){
				if(instanceID<0) TowerManager.PreBuildTower(this);
				else Build();
			}
			
			if(IsResource()) cooldown=GetCooldown_Rsc();
			
			hp=GetFullHP();
			sh=GetFullSH();
			
			TDTK.OnNewUnit(this);
			
			if(typeID<0) typeID=prefabID;
		}
		
		public int GetDisplayLevel(){
			return level + (isUpgrading ? 2 : 1 );
		}
		
		private bool isUpgrading;
		public void Build(bool isUpgrade=false){
			isUpgrading=isUpgrade;
			
			if(!isUpgrade){
				constructState=_ConstructState.Build;
				AudioManager.OnBuildStart();
				AudioManager.PlaySound(soundBuilding);
				AudioManager.PlaySoundW(TowerBuilt, gameObject);
			}
			else{
				constructState=_ConstructState.Upgrade;
				AudioManager.OnUpgradeStart();
				AudioManager.PlaySound(soundUpgrading);
			}
			
			float buildDuration=GetBuildDuration(level + (isUpgrade ? 1 : 0));
			constructDuration=buildDuration;
			constructRemained=buildDuration;
			
			effectBuilding.Spawn(GetPos(), Quaternion.identity);
			
			AnimPlayConstruct();
			
			TDTK.OnTowerConstructing(this);
		}
		
		public void Sell(){
			if(disableSelling) return;
			
			if(constructState==_ConstructState.Sell) return;
			
			constructState=_ConstructState.Sell;
			constructDuration=statsList[level].sellDuration;
			constructRemained=statsList[level].sellDuration;
			
			//effectSold.Spawn(GetPos(), Quaternion.identity);
			AudioManager.PlaySound(soundSold);
			
			AnimPlayDeconstruct();
			
			AudioManager.OnTowerSold();
			TDTK.OnTowerConstructing(this);
		}
		
		public override void FixedUpdate(){
			if(isPreview) return;
			
			base.FixedUpdate();
			
			Construction();
			
			TowerFunction();
		}
		
		void Construction(){
			if(constructRemained<0) return;
			
			constructRemained-=Time.fixedDeltaTime;
			if(constructRemained<=0){
				if(constructState==_ConstructState.Sell){
					RscManager.GainRsc(GetSellValue());
					
					effectSold.Spawn(GetPos(), Quaternion.identity);
					Destroyed(false, false);
				}
				if(constructState==_ConstructState.Upgrade){
					effectBuilt.Spawn(GetPos(), Quaternion.identity);
					
					level+=1;
					if(statsList[level].hp>statsList[level-1].hp) hp+=statsList[level].hp-statsList[level-1].hp;
					
					AudioManager.OnUpgradeComplete();
					AudioManager.PlaySound(soundUpgraded);
					
					OnSupportTowerBuilt();
					
					NewTower(this);	//for applying support
				}
				if(constructState==_ConstructState.Build){
					effectBuilt.Spawn(GetPos(), Quaternion.identity);
					AudioManager.OnBuildComplete();
					AudioManager.PlaySound(soundBuilt);
					
					OnSupportTowerBuilt();
				}
				
				isUpgrading=false;
			}
		}
		
		
		
		//fixed update for resource, aoe and mine, check '#region support tower' for support
		void TowerFunction(){
			if(!GameControl.HasGameStarted()) return;
			if(InConstruction() || IsStunned() || IsDestroyed()) return;
			
			//if(GameControl.HasGameStarted() && !GameControl.IsGamePaused()) cooldown-=Time.fixedDeltaTime;
			
			if(IsResource() && cooldownRsc<=0){
				if(!activeBetweenWave && SpawnManager.IsBetweenWaves()) return;
				
				FireMiscEffect(shootEffObj_Resource);
				
				RscManager.GainRsc(GetRscGain(), RscManager._GainType.RscTower);
				cooldownRsc=GetCooldown_Rsc();
			}
			
			if(SpawnManager.IsBetweenWaves()) return;
			
			if(IsAOE() && cooldownAOE<=0){
				List<Unit> tgtList=SpawnManager.GetUnitsWithinRange(this, GetAttackRange_AOE(), GetTargetGroup());
				if(tgtList.Count>0){
					FireMiscEffect(shootEffObj_AOE);		//AnimPlayAttack();
					
					cooldownAOE=GetCooldown_AOE();
					for(int i=0; i<tgtList.Count; i++) tgtList[i].ApplyAttack(new AttackInfo(this, tgtList[i], 1));
				}
				else cooldownAOE=0.1f;
			}
			
			if(IsMine() && cooldownMine<=0){
				List<Unit> tgtList=SpawnManager.GetUnitsWithinRange(this, TowerManager.GetGridSize()*.25f, GetTargetGroup());
				if(tgtList.Count>0){
					FireMiscEffect(shootEffObj_Mine);
					
					//tgtList[0].ApplyAttack(new AttackInfo(this, tgtList[0], 2));
					
					tgtList=SpawnManager.GetUnitsWithinRange(this, GetAOERange_Mine(), GetTargetGroup());
					for(int i=0; i<tgtList.Count; i++) tgtList[i].ApplyAttack(new AttackInfo(this, tgtList[i], 2));
					
					cooldownMine=GetCooldown_Mine();
					if(destroyOnTrigger){
						cooldownMine=999;
						Destroyed(true, false);
					}
				}
				else cooldownMine=0.1f;
			}
		}
		
		
		
		
		#region support tower
		[Space(10)]
		public List<UnitTower> supportTgtList=new List<UnitTower>();	//the towers being buff by this support tower
		public List<UnitTower> supportSrcList=new List<UnitTower>();		//the support towers buffing this tower
		public void SupportBuffTower(UnitTower tower){
			if(!IsSupport()){ Debug.Log("calling non support tower to buff?"); return; }
			
			supportTgtList.Add(tower);
			tower.supportSrcList.Add(this);
			
			List<Effect> list=GetEffectOnHit_Support();
			for(int i=0; i<list.Count; i++){
				list[i].fromSupport=true;
				list[i].duration=Mathf.Infinity;
				tower.ApplyEffect(list[i]);
			}
			
			//Effect effect=GetEffectOnHit();//.Clone();
			//effect.duration=Mathf.Infinity;
			//tower.ApplyEffect(effect);
		}
		public void SupportUnbuffTower(UnitTower tower, bool removeFromList=true){
			if(!IsSupport()){ Debug.Log("calling non support tower to unbuff?"); return; }
			
			if(removeFromList) supportTgtList.Remove(tower);
			tower.supportSrcList.Remove(this);
			
			//bool hasSimilarBuff=false;
			//for(int i=0; i<tower.supportSrcList.Count; i++){
			//	if(tower.supportSrcList[i].prefabID==prefabID && tower.supportSrcList[i].level==level){
			//		Debug.Log("has similar buff     "+tower.supportSrcList[i].name+"    "+tower);
			//		hasSimilarBuff=true; break;
			//	}
			//}
			
			//if(hasSimilarBuff) return;
			
			for(int i=0; i<tower.allEffectList.Count; i++){
				//Debug.Log(tower.allEffectList[i].FromTower()+"   "+tower.allEffectList[i].srcPrefabID+"  "+prefabID);
				
				//if(allEffectList[i].duration<Mathf.Infinity) continue;
				if(!tower.allEffectList[i].FromTower()) continue;
				if(tower.allEffectList[i].srcPrefabID!=prefabID) continue;
				
				if(tower.allEffectList[i].stack>1){
					tower.allEffectList[i].stack-=1;
					continue;
				}
				
				tower.allEffectList[i].durationRemain=0;
				break;
			}
		}
		
		
		public static void NewTower(UnitTower tower){
			if(tower.IsSupport()){
				//~ List<UnitTower> allTowerList=TowerManager.GetActiveTowerList();
				//~ for(int i=0; i<allTowerList.Count; i++){
					//~ if(allTowerList[i].IsSupport()) continue;
					//~ if(allTowerList[i]==tower) continue;
					
					//~ float dist=Vector3.Distance(allTowerList[i].GetPos(), tower.GetPos());
					//~ if(dist<tower.GetAttackRange_Support()) tower.SupportBuffTower(allTowerList[i]);
				//~ }
			}
			else{
				List<UnitTower> supportTowerList=TowerManager.GetSupportTowerList();
				for(int i=0; i<supportTowerList.Count; i++){
					float dist=Vector3.Distance(supportTowerList[i].GetPos(), tower.GetPos());
					if(dist<supportTowerList[i].GetAttackRange_Support()) supportTowerList[i].SupportBuffTower(tower);
				}
			}
		}
		public void OnSupportTowerBuilt(){
			if(IsSupport()){
				List<UnitTower> allTowerList=TowerManager.GetActiveTowerList();
				for(int i=0; i<allTowerList.Count; i++){
					if(allTowerList[i].IsSupport()) continue;
					if(allTowerList[i]==this) continue;
					
					float dist=Vector3.Distance(allTowerList[i].GetPos(), GetPos());
					if(dist<GetAttackRange_Support()) SupportBuffTower(allTowerList[i]);
				}
			}
		}
		public static void RemoveTower(UnitTower tower){
			if(tower.IsSupport()){
				for(int i=0; i<tower.supportTgtList.Count; i++){
					tower.SupportUnbuffTower(tower.supportTgtList[i], false);
				}
			}
			else{
				for(int i=0; i<tower.supportSrcList.Count; i++){
					tower.supportSrcList[i].SupportUnbuffTower(tower);
				}
			}
		}
		#endregion
		
		
		
		[Space(10)]
		public BuildPlatform buildPlatform;
		public int nodeID;
		public void SetBuildPoint(BuildPlatform platform, int ID){
			buildPlatform=platform; nodeID=ID;
		}
		
		
		[Space(10)] 
		public bool unlockUpgradeViaPerk;
		//public bool useExpForUpgrade=false;
		//public float experience=0;
		
		public List<UnitTower> upgradeTowerList=new List<UnitTower>();
		
		public int GetUpgradeOptionCount(){
			int lvl=level + (isUpgrading ? 1 :  0);
			return (statsList.Count>1 && lvl<statsList.Count-1) ? 1 : upgradeTowerList.Count;
		}
		public UnitTower GetUpgradeTower(int idx){
			if(upgradeTowerList.Count==0 || idx<0 || idx>=upgradeTowerList.Count) return null;
			return upgradeTowerList[idx];
		}
		
		private float lastReadyForUpgradeCall;
		private List<int> readyForUpgradeList=new List<int>();
		public bool ReadyForUpgrade(int idx){
			if(statsList.Count>1 && level<statsList.Count-1) return true;
			
			if(!unlockUpgradeViaPerk) return true;
			
			if(lastReadyForUpgradeCall!=Time.time){
				lastReadyForUpgradeCall=Time.time;
				readyForUpgradeList=PerkManager.GetTowerUpgradeUnlockedList(prefabID, upgradeTowerList.Count);
			}
			
			return readyForUpgradeList.Contains(idx);
		}
		
		//public bool CanUpgrade1(){
		//	if(GetUpgradeType()<0) return false;
		//	//if(useExpForUpgrade && experience<statsList[level].expToNextUpgrade) return false;
		//	return true;
		//}
		public int GetUpgradeType(){
			if(level<statsList.Count-1) return 0;
			if(upgradeTowerList.Count>0) return 1;
			return -1;
		}
		
		public void Upgrade(int upgradeIdx=0){
			if(GetUpgradeType()==0){
				RemoveTower(this);		//Debug.Log("RemoveTower");
				RscManager.SpendRsc(GetUpgradeCost(0));
				Build(true);
			}
			else{
				TowerManager.RemoveTower(this);
				if(buildPlatform!=null) buildPlatform.RemoveTower(nodeID, false);
				if(TowerManager.UseFreeFormMode()) buildPlatform.transform.parent=null;
				TowerManager.BuildTower(upgradeTowerList[upgradeIdx], buildPlatform, nodeID, true, true);//, typeID);		//include the typeID to have tower limitInScene based on built tower rather than prefabID
				Destroy(thisObj);
			}
		}
		
		
		public List<float> GetUpgradeCost(int upgradeIdx=0){
			if(GetUpgradeType()==0){ //return statsList[level+1].cost;
				//return RscManager.ApplyMultiplier(new List<float>( statsList[level+1].cost ), PerkManager.GetUnitCost(prefabID));
				List<float> list=RscManager.ApplyModifier(new List<float>(statsList[level+1].cost), PerkManager.GetModUnitCost(prefabID));
				return RscManager.ApplyMultiplier(list, PerkManager.GetMulUnitCost(prefabID));
			}
			else{ //return upgradeTowerList[upgradeIdx].GetCost();
				//~ return RscManager.ApplyMultiplier(new List<float>( upgradeTowerList[upgradeIdx].GetCost() ), PerkManager.GetUnitCost(upgradeTowerList[upgradeIdx].prefabID));
				return upgradeTowerList[upgradeIdx].GetCost(0);
			}
		}
		public List<float> GetCost(int lvl=-1){
			if(lvl<0) lvl=level;
			//List<float> list=new List<float>(statsList[level].cost);
			//List<float> listMod=PerkManager.GetModUnitCost(prefabID);
			//List<float> listMul=PerkManager.GetMulUnitCost(prefabID);
			List<float> list=RscManager.ApplyModifier(new List<float>(statsList[lvl].cost), PerkManager.GetModUnitCost(prefabID));
			return RscManager.ApplyMultiplier(list, PerkManager.GetMulUnitCost(prefabID));
			
			//return RscManager.ApplyMultiplier(new List<float>( statsList[level].cost ), PerkManager.GetUnitCost(prefabID));
		}
		
		public List<float> GetSellValue(){ 
			return RscManager.ApplyMultiplier(new List<float>( statsList[level].sellValue ), RscManager.GetSellMultiplier());
		}
		
		public float GetBuildDuration(int lvl){ return (statsList[lvl].buildDuration+PerkManager.GetModUnitBuildDur(prefabID)) *  PerkManager.GetMulUnitBuildDur(prefabID); }
		public float GetSellDuration(int lvl){ return (statsList[lvl].sellDuration+PerkManager.GetModUnitSellDur(prefabID)) *  PerkManager.GetMulUnitSellDur(prefabID); }
		
		
		
		public override void Destroyed(bool spawnEffDestroyed=true, bool destroyedByAttack=true){
			float animDuration=0;
			
			if(spawnEffDestroyed){
				effectDestroyed.Spawn(GetPos(), Quaternion.identity);
				AudioManager.PlaySound(soundDestroyed);
			}
			
			if(destroyedByAttack){
				if(lifeCostOnDestroyed>0) GameControl.LostLife(lifeCostOnDestroyed);
				animDuration=AnimPlayDestroyed();
			}
			
			ClearAllEffect();
			
			TowerManager.RemoveTower(this);
			if(buildPlatform!=null) buildPlatform.RemoveTower(nodeID);
			if(TowerManager.UseFreeFormMode()) buildPlatform.transform.parent=thisT;
			Destroy(thisObj, animDuration);
		}
	}

}