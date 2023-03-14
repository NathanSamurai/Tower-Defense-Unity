using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{

	[System.Serializable]
	public class TDItem{
		public int prefabID=-1;
		public int instanceID=-1;	//correspond to the index in abilityList/perkList
		
		public Sprite icon;
		public string name="Item";
		public string desp="Item's description";
		
		public void Clone(TDItem src, TDItem tgt){
			tgt.prefabID=src.prefabID;	tgt.instanceID=src.instanceID;
			tgt.icon=src.icon;	tgt.name=src.name;		tgt.desp=src.desp;
		}
	}
	
	[System.Serializable]
	public class Ability : TDItem{
		public bool requireTargetSelection=true;
		public bool requireUnitAsTarget=false;
		
		public enum _TargetType{ Hostile, Friendly, All }
		public _TargetType targetType;
		
		
		public int useLimit=-1;
		public int useCount=0;		//variable for counting usage during runtime
		
		public float currentCD=0;		//variable for counting cd during runtime, ability only available when this is <0
		
		public float cost=10;
		
		public enum _EffectType{ Default, Custom }
		public _EffectType effectType=_EffectType.Default;
		
		public float effectDelay=0;
		
		public Stats stats=new Stats();
		
		//~ public GameObject spawnOnActivateObj;
		//~ public bool autoDestroyObj=false;
		//~ public float delayBeforeDestroy=2;
		
		public Transform tgtSelectIndicator;
		
		public VisualObject effectOnActivate=new VisualObject();
		public VisualObject effectOnTarget=new VisualObject();
		
		public AudioClip soundOnActivate;
		
		public bool hideInInspector=false;
		
		
		//~ public Func<Vector3, int> activateCallback = (point) => { return 0; };
		
		
		
		public Ability Clone(){	//return ObjectCopier.Clone(this);
			Ability clone=new Ability();	//(Ability)base.Clone();
			
			base.Clone(this, clone);
			
			//clone.prefabID=prefabID;	clone.instanceID=instanceID;
			//clone.icon=icon;	clone.name=name;	clone.desp=desp;
			
			clone.hideInInspector=hideInInspector;
			clone.requireTargetSelection=requireTargetSelection;
			clone.requireUnitAsTarget=requireUnitAsTarget;
			clone.targetType=targetType;
			clone.useLimit=useLimit;
			clone.useCount=useCount;
			clone.cost=cost;
			clone.effectType=effectType;
			clone.effectDelay=effectDelay;
			clone.stats=stats.Clone();
			clone.tgtSelectIndicator=tgtSelectIndicator;
			clone.effectOnActivate=effectOnActivate.Clone();
			clone.effectOnTarget=effectOnTarget.Clone();
			clone.soundOnActivate=soundOnActivate;
			clone.hideInInspector=hideInInspector;
			//clone.hitCallback=hitCallback;
			return clone;
		}
		
		public void Init(int ID, Transform abManagerT){
			instanceID=ID;
			useCount=0;
			currentCD=0;
			
			stats.VerifyBaseStats(false, false, true);
			//stats.effectOnHit.SetType(ID, this);
			
			if(requireTargetSelection && tgtSelectIndicator!=null){
				tgtSelectIndicator=MonoBehaviour.Instantiate(tgtSelectIndicator.gameObject).transform;
				tgtSelectIndicator.parent=abManagerT;
				tgtSelectIndicator.gameObject.SetActive(false);
			}
			
			//~ activateCallback = (point) => { 
				//~ if(type!=_Type.Default) return 0;
				
				//~ if(!autoDestroyObj) ObjectPoolManager.Spawn(spawnOnActivateObj, point, Quaternion.identity);
				//~ else ObjectPoolManager.Spawn(spawnOnActivateObj, point, Quaternion.identity, delayBeforeDestroy);
				
				//~ List<Unit> tgtList=new List<Unit>();
				
				//~ if(targetType==_TargetType.Hostile){
					//~ List<Unit> tgtList=SpawnManager.GetUnitsWithinRange(this, stats.aoeRange);
				//~ }
				//~ else if(targetType==_TargetType.Friendly){
					//~ List<Unit> tgtList=TowerManager.GetUnitsWithinRange(this, stats.aoeRange);
				//~ }
				//~ else{
					//~ List<Unit> tgtList=SpawnManager.GetUnitsWithinRange(this, stats.aoeRange);
					//~ List<Unit> towerList=TowerManager.GetUnitsWithinRange(this, stats.aoeRange);
					//~ for(int i=0; i<list2.Count; i++) tgtList.Add(towerList[i]);
				//~ }
				
				//~ for(int i=0; i<tgtList.Count; i++){
					//~ tgtList[i].ApplyAttack(new AttackInfo(aInfo.srcUnit, tgtList[i], false));
				//~ }
				
				//~ return 0;
			//~ };
		}
		
		
		public bool CanTargetAll(){ return targetType==_TargetType.All; }
		public bool MustTargetHostile(){ return targetType==_TargetType.Hostile; }
		public bool MustTargetFriendly(){ return targetType==_TargetType.Friendly; }
		
		
		
		public void Activate(Vector3 pos=default(Vector3)){
			useCount+=1;
			currentCD=GetCooldown();
			
			if(AbilityManager.UseRscManagerForCost()) RscManager.SpendRsc(GetCostRsc());
			else AbilityManager.SpendRsc(GetCost());
			
			
			effectOnActivate.Spawn(pos, Quaternion.identity);
			AudioManager.PlaySound(soundOnActivate);
			
			
			//activateCallback();
			if(effectType!=_EffectType.Default) return;
			
			AbilityManager.GetInstance().StartCoroutine(AbilityEffect(pos));
		}
		
		IEnumerator AbilityEffect(Vector3 pos){
			if(effectDelay>0) yield return new WaitForSeconds(effectDelay);
			
			List<Unit> tgtList=new List<Unit>();
			
			if(targetType==_TargetType.Hostile){
				if(!requireTargetSelection) tgtList=SpawnManager.GetActiveUnitList();
				else tgtList=SpawnManager.GetUnitsWithinRange(pos, stats.aoeRange);
			}
			else if(targetType==_TargetType.Friendly){
				if(!requireTargetSelection) tgtList=TowerManager.GetActiveUnitList();
				else tgtList=TowerManager.GetUnitsWithinRange(pos, stats.aoeRange);
			}
			else{
				if(!requireTargetSelection){
					tgtList=SpawnManager.GetActiveUnitList();
					List<Unit> towerList=TowerManager.GetActiveUnitList();
					for(int i=0; i<towerList.Count; i++) tgtList.Add(towerList[i]);
				}
				else{
					tgtList=SpawnManager.GetUnitsWithinRange(pos, stats.aoeRange);
					List<Unit> towerList=TowerManager.GetUnitsWithinRange(pos, stats.aoeRange);
					for(int i=0; i<towerList.Count; i++) tgtList.Add(towerList[i]);
				}
			}
			
			for(int i=0; i<tgtList.Count; i++){
				effectOnTarget.Spawn(tgtList[i].GetTargetPoint(), Quaternion.identity);
				tgtList[i].ApplyAttack(new AttackInfo(this, tgtList[i], false));
			}
			
			yield return null;
		}
		
		
		public enum _Status{ Ready, OnCooldown, InsufficientRsc, UseLimitReached }
		public _Status IsReady(){
			if(currentCD>0) return _Status.OnCooldown;
			if(UseLimitReached()) return _Status.UseLimitReached;
			if(AbilityManager.UseRscManagerForCost() && !RscManager.HasSufficientRsc(GetCostRsc())) return _Status.InsufficientRsc;
			if(!AbilityManager.UseRscManagerForCost() && !AbilityManager.HasSufficientRsc(GetCost())) return _Status.InsufficientRsc;
			return _Status.Ready;
		}
		
		public bool OnCooldown(){ return currentCD>0; }
		public float GetCDRatio(){ return 1-(currentCD/GetCooldown()); }
		
		public void IterateCooldown(){
			if(currentCD<0) return;
			currentCD-=Time.fixedDeltaTime;
		}
		
		public bool UseLimitReached(){ return useLimit>0 && useCount>=GetUseLimit(); }
		public string GetUseLimitText(){
			if(useLimit<=0) return "";
			int limit=GetUseLimit();
			return (limit-useCount)+"/"+limit;
		}
		
		
		
		public int GetDamageType(){ return stats.damageType; }
		
		public int GetUseLimit(){ return useLimit + (int)Mathf.Round(PerkManager.GetModAbilityUseLimit(prefabID)); }
		
		public int GetCost(){ return (int)Mathf.Round((cost + GetModCost()) * GetMulCost()); }
		public List<float> GetCostRsc(){
			List<float> list=RscManager.ApplyModifier(new List<float>(stats.cost), PerkManager.GetModAbilityCostRsc(prefabID));
			return RscManager.ApplyMultiplier(list, PerkManager.GetMulAbilityCostRsc(prefabID));
		}
		
		public float GetDamageMin(){ 	return (stats.damageMin + GetModDmgMin()) * GetMulDmgMin(); }
		public float GetDamageMax(){ 	return (stats.damageMax + GetModDmgMax()) * GetMulDmgMax(); }
		public float GetAOERange(){	return (stats.aoeRange + GetModAOE()) * GetMulAOE(); }
		public float GetCooldown(){ 	return (stats.cooldown + GetModCD()) * GetMulCD(); }
		public float GetHit(){ 				return (stats.hit + GetModHit()) * GetMulHit(); }
		public float GetCrit(){ 			return (stats.critChance + GetModCrit()) * GetMulCrit(); }
		public float GetCritMultiplier(){	return (stats.critMultiplier + GetModCritMul()) * GetMulCritMul(); }
		public float GetEffectOnHitChance(){	 return (stats.effectOnHitChance + GetModEffOnHitChance()) * GetMulEffOnHitChance(); }
		
		public float GetModCost(){ 		return PerkManager.GetModAbilityCost(prefabID); }
		public float GetModDmgMin(){ 	return PerkManager.GetModAbilityDmgMin(prefabID); }
		public float GetModDmgMax(){	return PerkManager.GetModAbilityDmgMax(prefabID); }
		public float GetModAOE(){ 		return PerkManager.GetModAbilityAOE(prefabID); }
		public float GetModCD(){ 		return PerkManager.GetModAbilityCD(prefabID); }
		public float GetModHit(){ 		return PerkManager.GetModAbilityHit(prefabID); }
		public float GetModCrit(){ 		return PerkManager.GetModAbilityCrit(prefabID); }
		public float GetModCritMul(){ 	return PerkManager.GetModAbilityCritMul(prefabID); }
		public float GetModEffOnHitChance(){ 	return PerkManager.GetModAbilityEffOnHitChance(prefabID); }
		
		public float GetMulCost(){ 		return PerkManager.GetMulAbilityCost(prefabID); }
		public float GetMulDmgMin(){ 	return PerkManager.GetMulAbilityDmgMin(prefabID); }
		public float GetMulDmgMax(){ 	return PerkManager.GetMulAbilityDmgMax(prefabID); }
		public float GetMulAOE(){ 		return PerkManager.GetMulAbilityAOE(prefabID); }
		public float GetMulCD(){ 			return PerkManager.GetMulAbilityCD(prefabID); }
		public float GetMulHit(){ 			return PerkManager.GetMulAbilityHit(prefabID); }
		public float GetMulCrit(){ 		return PerkManager.GetMulAbilityCrit(prefabID); }
		public float GetMulCritMul(){ 	return PerkManager.GetMulAbilityCritMul(prefabID); }
		public float GetMulEffOnHitChance(){ 	return PerkManager.GetMulAbilityEffOnHitChance(prefabID); }
		
		
		public List<Effect> GetEffectOnHit(){ 
			List<Effect> list=new List<Effect>();
			
			List<int> overrideIDList=PerkManager.GetAbilityOverrideOnHitEff(prefabID);
			
			if(overrideIDList!=null){
				for(int i=0; i<overrideIDList.Count; i++){
					list.Add(Effect_DB.GetPrefab(overrideIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
				}
				return list;
			}
			
			for(int i=0; i<stats.effectOnHitIDList.Count; i++){
				list.Add(Effect_DB.GetPrefab(stats.effectOnHitIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
			}
			
			List<int> appendIDList=PerkManager.GetAbilityAppendOnHitEff(prefabID);
			if(appendIDList!=null){
				for(int i=0; i<appendIDList.Count; i++){
					list.Add(Effect_DB.GetPrefab(appendIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
				}
			}
			
			for(int i=0; i<list.Count; i++) list[i].SetType(i, this);
			return list;
			
			//~ int overrideID=PerkManager.GetAbilityOverrideOnHitEff(prefabID);//GetPerkOverrideOnHitEff();
			//~ if(overrideID<0 && stats.effectOnHitID<0) return null;
			//~ else{
				//~ //return Effect_DB.GetPrefab(stats.effectOnHitID).Clone();
				
				//~ Effect effect=Effect_DB.GetPrefab(overrideID>=0 ? overrideID : stats.effectOnHitID).ModifyWithPerk();
				//~ effect.SetType(instanceID, this);
				//~ return effect;
			//~ }
		}
		//public int GetPerkOverrideOnHitEff(){ return PerkManager.GetAbilityOverrideOnHitEff(prefabID); }
		
		
	}
	
	
}