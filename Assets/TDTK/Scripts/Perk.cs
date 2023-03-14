using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{

	/*
	[System.Serializable] public class TDItem{	//check Ability.cs instead
		public int prefabID=-1;
		public int instanceID=-1;	//correspond to the index in abilityList/perkList
		
		public Sprite icon;
		public string name="Item";
		public string desp="Item's description";
	}
	*/
	
	public enum _PerkType{
		NewTower,
		NewAbility,
		//NewFPSWeapon,
		
		ModifyTower,						
		ModifyAbility,
		ModifyEffect,
		ModifyPerkCost,
		
		GainLife,
		LifeCap, 
		LifeRegen,						//life generate overtime rate
		LifeGainWaveCleared, 
		
		GainRsc,
		RscRegen,						//rsc generate overtime rate
		
		RscGain,							//multiplier for all rsc gain
		RscGainCreepDestroyed,	//for creep killed
		RscGainWaveCleared,		//for wave cleared
		RscGainResourceTower,		//for income from resource tower
		
		AbilityRscCap, 				//Rsc in AbilityManager, for ability only
		AbilityRscRegen,				//ability rsc generate overtime rate
		AbilityRscGainWaveCleared,
		
		UnlockTowerUpgrade,
		
		//FPSWeapon,
		//FPSWeaponSpecific,
	}
	
	
	
	[System.Serializable]
	public class Perk : TDItem{
		//public Sprite iconUnavailable;
		//public Sprite iconPurchased;
		
		public _PerkType type;
		public int groupID=0;	//for constructing multiple school of tech-tree
		
		public bool UseStats(){
			return type==_PerkType.ModifyTower || type==_PerkType.ModifyAbility || type==_PerkType.ModifyEffect;
		}
		public bool UseGainValue(){
			return type==_PerkType.GainLife || type==_PerkType.LifeCap || type==_PerkType.LifeRegen || type==_PerkType.LifeGainWaveCleared || 
			type==_PerkType.AbilityRscCap || type==_PerkType.AbilityRscRegen || type==_PerkType.AbilityRscGainWaveCleared || IsRscGain();
		}
		public bool UseGainList(){
			return type==_PerkType.GainRsc || type==_PerkType.RscRegen || IsRscGain();
		}
		public bool IsRscGain(){
			return type==_PerkType.RscGain || type==_PerkType.RscGainCreepDestroyed || type==_PerkType.RscGainWaveCleared || type==_PerkType.RscGainResourceTower;
		}
		public bool IsForPerk(){
			return type==_PerkType.ModifyPerkCost;
		}
		
		
		public bool hideInInspector=false;
		
		public int autoUnlockOnLevel=-1;
		public int autoUnlockOnWave=-1;
		
		public int cost=0;
		public List<float> costRsc=new List<float>();
		public int minLevel=0;								//min level to reach before becoming available (check GameControl.levelID)
		public int minWave=0;								//min wave to reach before becoming available
		public int minPerkCount=0;						//min perk count
		public List<int> prereq=new List<int>();	//prerequisite perk before becoming available, element is removed as the perk is unlocked in runtime
		
		public bool repeatable=false;		//can the perk can be purchased repeatably
		
		public enum _EffType{ Modifier, Multiplier }
		public _EffType effType=_EffType.Multiplier;
		public bool IsMultiplier(){ return effType==_EffType.Multiplier; }
		
		public int newTowerPID=-1;
		public int newAbilityPID=-1;
		
		public int replaceTowerPID=-1;
		public int replaceAbilityPID=-1;
		
		public bool applyToAll=false;							//for perk that modify item (tower, ability, etc)
		public List<int> towerPIDList=new List<int>();	//for perk that applies to specific towers
		public List<int> abilityPIDList=new List<int>();	//for perk that applies to specific abilities
		public List<int> perkPIDList=new List<int>();	//for perk that applies to specific perk
		public List<int> effectPIDList=new List<int>();	//for perk that applies to specific effect
		
		
		public Effect effect=new Effect();	//effect stat multiplier, used for unit, ability and effect
		public float costMul=1;					//for perk and ability cost when not using RscManager
		
		//for non unit/ability/effect thing that uses only a single attribute (life for instance), also uses as uselimit for for ability
		public float gain=0;			
		
		//for non unit/ability/effect thing that require a list (rsc gain for instance)
		public List<float> gainList=new List<float>();
		
		
		//for tower upgrade, use towerPID instead
		public bool unlockAllUpgrade;	
		public List<int> towerUpgradeList=new List<int>();	//contain index correspond to the upgradeTowerList list for the tower
		
		
		#region runtime attribute
		public enum _Status{ Ready, Purchased, PrereqNoMet, InsufficientPP, InsufficientWave, InsufficientLvl }
		public _Status status;	//runtime attribute
		public int purchasedCount=0;		//for repeatable perk
		#endregion
		
		
		public Perk Clone(){	//return ObjectCopier.Clone(this);
			Perk clone=new Perk();	//(Perk)base.Clone();
			base.Clone(this, clone);
			clone.type=type;
			clone.groupID=groupID;
			clone.hideInInspector=hideInInspector;
			
			clone.autoUnlockOnLevel=autoUnlockOnLevel;
			clone.autoUnlockOnWave=autoUnlockOnWave;
			
			clone.cost=cost;
			clone.costRsc=new List<float>( costRsc );
			clone.minLevel=minLevel;
			clone.minWave=minWave;
			clone.minPerkCount=minPerkCount;
			clone.prereq=new List<int>( prereq );
			clone.repeatable=repeatable;
			
			
			clone.effType=effType;
			
			clone.newTowerPID=newTowerPID;
			clone.newAbilityPID=newAbilityPID;
			
			clone.replaceTowerPID=replaceTowerPID;
			clone.replaceAbilityPID=replaceAbilityPID;
			
			clone.applyToAll=applyToAll;
			clone.towerPIDList=new List<int>( towerPIDList );
			clone.abilityPIDList=new List<int>( abilityPIDList );
			clone.perkPIDList=new List<int>( perkPIDList );
			clone.effectPIDList=new List<int>( effectPIDList );
			
			clone.effect=effect.Clone();
			clone.costMul=costMul;
			clone.gain=gain;
			clone.gainList=new List<float>( gainList );
			
			clone.unlockAllUpgrade=unlockAllUpgrade;
			clone.towerUpgradeList=new List<int>( towerUpgradeList );
			
			return clone;
		}
		
		
		public Perk(){}// stats.ResetAsPerk(); }
		
		public bool IsPurchased(){ return status==_Status.Purchased; }
		public bool IsAvailable(){ return UpdateStatus()==_Status.Ready; }
		
		
		public bool SupportModNMul(){
			if(type==_PerkType.NewTower) return false;
			if(type==_PerkType.NewAbility) return false;
			if(type==_PerkType.GainLife) return false;
			if(type==_PerkType.GainRsc) return false;
			return true;
		}
		
		
		public _Status UpdateStatus(){
			if(status!=_Status.Purchased){
				status=_Status.Ready;
				
				//check evel		 status=_Status.InsufficientLvl;
				if(minLevel>0 && minLevel>GameControl.GetHighestLevelID()) status=_Status.InsufficientLvl;
				if(minWave>0 && minWave>SpawnManager.GetCurrentWaveIndex()+1) status=_Status.InsufficientWave;
				if(minPerkCount>=0 && minPerkCount>PerkManager.GetPurchasedPerkCount()) status=_Status.InsufficientPP;
				
				VerifyPrereq();
				if(prereq.Count>0) return status=_Status.PrereqNoMet;
			}
			return status;
		}
		
		public string GetDespUnavailable(){
			if(status==_Status.InsufficientPP)		return "available after "+minPerkCount+" perks has been purchased";
			if(status==_Status.InsufficientWave) return "available after wave "+minWave;
			if(status==_Status.InsufficientLvl) 	return "available in later game";
			if(status==_Status.PrereqNoMet){
				string text="require ";
				for(int i=0; i<prereq.Count; i++){
					int idx=Perk_DB.GetPrefabIndex(prereq[i]);
					text+=(i>0 ? ", " : "")+Perk_DB.GetItem(idx).name;
				}
				return text;
			}
			return "";
		}
		
		public bool HasSufficientRsc(){ 
			if(PerkManager.UseRscManagerForCost()) return RscManager.HasSufficientRsc(costRsc);
			else return PerkManager.HasSufficientRsc(cost);
		}
		
		public void VerifyPrereq(){ prereq=PerkManager.VerifyPerkPrereq(prereq); }
		
		
		
		public int GetPurchaseCost(){ return (int)Mathf.Round(cost * PerkManager.GetPerkCostMul(prefabID)); }
		public List<float> GetPurchaseCostRsc(){ 
			//float costMultiplier=PerkManager.GetPerkCostMul(prefabID);
			//List<float> costList=RscManager.ApplyMultiplier(new List<float>( costRsc ), costMultiplier);
			//return RscManager.ApplyMultiplier(costList, PerkManager.GetPerkCost(prefabID));
			return RscManager.ApplyMultiplier(costRsc, PerkManager.GetPerkCost(prefabID));
		}
		
		
		//add/modify tower's hp and shield if they are increased upon unlocking of the perk
		void OnModifyTowerPurchased(){
			List<UnitTower> towerList=TowerManager.GetActiveTowerList();
			
			if(towerList==null) return;
			if(towerList.Count==0) return;
			
			if(IsMultiplier()){
				if(effect.stats.hp>1){
					for(int i=0; i<towerList.Count; i++)
						towerList[i].hp+=towerList[i].statsList[towerList[i].level].hp*effect.stats.hp;
				}
				if(effect.stats.sh>1){
					for(int i=0; i<towerList.Count; i++)
						towerList[i].sh+=towerList[i].statsList[towerList[i].level].sh*effect.stats.sh;
				}
			}
			else{
				if(effect.stats.hp>0){
					for(int i=0; i<towerList.Count; i++) towerList[i].hp+=effect.stats.hp;
				}
				if(effect.stats.sh>0){
					for(int i=0; i<towerList.Count; i++) towerList[i].sh+=effect.stats.sh;
				}
			}
		}
		
		
		public _Status Purchase(bool useRsc=true){
			if(useRsc){
				if(PerkManager.UseRscManagerForCost()) RscManager.SpendRsc(GetPurchaseCostRsc());
				else PerkManager.SpendRsc(GetPurchaseCost());
			}
			status=_Status.Purchased;
			
			if(PerkManager.InGameScene()){
				if(type==_PerkType.NewTower){
					TowerManager.AddBuildable(newTowerPID, replaceTowerPID);
				}
				if(type==_PerkType.NewAbility){
					AbilityManager.AddAbility(newAbilityPID, replaceAbilityPID);
				}
				else if(type==_PerkType.ModifyTower){// || type==_PerkType.TowerAll){
					PerkManager.AddUnitPerkID(prefabID);		SetupCallback();
					OnModifyTowerPurchased();
				}
				else if(type==_PerkType.ModifyAbility){// || type==_PerkType.AbilityAll){
					PerkManager.AddAbilityPerkID(prefabID);	SetupCallback();
				}
				else if(type==_PerkType.ModifyEffect){
					PerkManager.AddEffectPerkID(prefabID);	SetupCallback();
				}
				else if(type==_PerkType.ModifyPerkCost){
					PerkManager.AddPerkPerkID(prefabID);
				}
				else if(type==_PerkType.LifeCap){
					GameControl.ModifyLifeCap((int)gain);
				}
				else if(type==_PerkType.GainLife){
					GameControl.GainLife((int)gain);
				}
				else if(type==_PerkType.LifeRegen){
					if(!IsMultiplier()) GameControl.ModifyLifeRegen(gain);
					else GameControl.ModifyLifeRegenMultiplier(gain);
				}
				else if(type==_PerkType.LifeGainWaveCleared){
					if(!IsMultiplier()) PerkManager.lifeGainOnWaveClearedMod+=(int)Mathf.Round(gain);
					else PerkManager.lifeGainOnWaveClearedMul*=gain;
				}
				else if(type==_PerkType.GainRsc){
					RscManager.GainRsc(gainList);
				}
				else if(type==_PerkType.RscRegen){
					if(!IsMultiplier()) RscManager.ModifyRegenModRate(gainList);
					else RscManager.ModifyRegenMulRate(gainList);
				}
				else if(type==_PerkType.RscGain){
					if(!IsMultiplier()){
						PerkManager.AddRscGainMod((int)gain);
						PerkManager.AddRscGainModList(gainList);
					}
					else{
						PerkManager.AddRscGainMul(gain);
						PerkManager.AddRscGainMulList(gainList);
					}
				}
				else if(type==_PerkType.RscGainCreepDestroyed){
					if(!IsMultiplier()){
						PerkManager.AddRscGainModCreepKilled((int)gain);
						PerkManager.AddRscGainModCreepKilledList(gainList);
					}
					else{
						PerkManager.AddRscGainMulCreepKilled(gain);
						PerkManager.AddRscGainMulCreepKilledList(gainList);
					}
				}
				else if(type==_PerkType.RscGainWaveCleared){
					if(!IsMultiplier()){
						PerkManager.AddRscGainModWaveCleared((int)gain);
						PerkManager.AddRscGainModWaveClearedList(gainList);
					}
					else{
						PerkManager.AddRscGainMulWaveCleared(gain);
						PerkManager.AddRscGainMulWaveClearedList(gainList);
					}
				}
				else if(type==_PerkType.RscGainResourceTower){
					if(!IsMultiplier()){
						PerkManager.AddRscGainModRscTower((int)gain);
						PerkManager.AddRscGainModRscTowerList(gainList);
					}
					else{
						PerkManager.AddRscGainMulRscTower(gain);
						PerkManager.AddRscGainMulRscTowerList(gainList);
					}
				}
				else if(type==_PerkType.AbilityRscCap){
					if(!IsMultiplier()) AbilityManager.ModifyRscCap((int)Mathf.Round(gain));
					else AbilityManager.MultiplyRscCap((int)Mathf.Round(gain));
				}
				else if(type==_PerkType.AbilityRscRegen){
					if(!IsMultiplier()) AbilityManager.ModifyRscRegen(gain);
					else AbilityManager.ModifyRscRegenMultiplier(gain);
				}
				else if(type==_PerkType.AbilityRscGainWaveCleared){
					if(!IsMultiplier()) PerkManager.abRscGainOnWaveClearedMod+=(int)Mathf.Round(gain);
					else PerkManager.abRscGainOnWaveClearedMul*=gain;
				}
				else if(type==_PerkType.UnlockTowerUpgrade){
					PerkManager.AddTowerUpgradePerkID(prefabID);
				}
			}
			
			return status;
		}
		
		
		
		
		
		
		public bool CheckID(int prefabID){
			//if(type==_PerkType.TowerAll || type==_PerkType.AbilityAll || type==_PerkType.PerkCostMulAll) return true;
			if(applyToAll) return true;
			else if(type==_PerkType.ModifyTower) return towerPIDList.Contains(prefabID);
			else if(type==_PerkType.ModifyAbility) return abilityPIDList.Contains(prefabID);
			else if(type==_PerkType.ModifyEffect) return effectPIDList.Contains(prefabID);
			else if(type==_PerkType.ModifyPerkCost) return perkPIDList.Contains(prefabID);
			else if(type==_PerkType.UnlockTowerUpgrade) return towerPIDList.Contains(prefabID);
			return false;
		}
		
		
		
		
		#region tower & ability callback
		void SetupCallback(){
			//GetModCostMul= (prefabID) => 	{ return CheckID(prefabID) ? costMul : 1; };
			//GetModCost = (prefabID) => 		{ return CheckID(prefabID) ? stats.cost : null; };
			if(!IsMultiplier()){
				GetModUseLimit= (prefabID) => 	{ return CheckID(prefabID) ? gain : 0; };
				
				GetModCost= (prefabID) => 		{ return CheckID(prefabID) ? costMul : 0; };
				GetModCostRsc = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.cost : null; };
				GetModBuildDur = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.buildDuration : 0; };
				GetModSellDur = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.sellDuration : 0; };
				
				GetModHP = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hp : 0; };
				GetModSH = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.sh : 0; };
				GetModSHRegen = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.shRegen : 0; };
				GetModSHStagger = (prefabID) => { return CheckID(prefabID) ? effect.stats.shStagger : 0; };
				
				GetModDodge = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.dodge : 0; };
				GetModDmgReduc = (prefabID) => { return CheckID(prefabID) ? effect.stats.dmgReduc : 0; };
				GetModCritReduc = (prefabID) => { return CheckID(prefabID) ? effect.stats.dmgReduc : 0; };
				
				//GetModSpeed = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.speed : 0; };
				GetModDmgMin = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMin : 0; };
				GetModDmgMax = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMax : 0; };
				GetModRange = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.attackRange : 0; };
				GetModAOE = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.aoeRange : 0; };
				GetModCD = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.cooldown : 0; };
				GetModHit = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hit : 0; };
				GetModCrit = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.critChance : 0; };
				GetModCritMul = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.critMultiplier : 0; };
				
				GetModDmgMin_AOE = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMin_AOE : 0; };
				GetModDmgMax_AOE = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMax_AOE : 0; };
				GetModRange_AOE = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.attackRange_AOE : 0; };
				//GetModAOE_AOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.aoeRange_AOE : 0; };
				GetModCD_AOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.cooldown_AOE : 0; };
				GetModHit_AOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hit_AOE : 0; };
				GetModCrit_AOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.critChance_AOE : 0; };
				GetModCritMul_AOE = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.critMultiplier_AOE : 0; };
				
				GetModRange_Support = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.attackRange_Support : 0; };
				
				GetModDmgMin_Mine = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMin_Mine : 0; };
				GetModDmgMax_Mine = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMax_Mine : 0; };
				//GetModRange_Mine = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.attackRange_Mine : 0; };
				GetModAOE_Mine = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.aoeRange_Mine : 0; };
				GetModCD_Mine = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.cooldown_Mine : 0; };
				GetModHit_Mine = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hit_Mine : 0; };
				GetModCrit_Mine = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.critChance_Mine : 0; };
				GetModCritMul_Mine = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.critMultiplier_Mine : 0; };
				
				GetModCD_Rsc = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.cooldown_Rsc : 0; };
				GetModRscGain = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.rscGain : null; };
				
				GetModEffOnHitChance = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.effectOnHitChance : 0; };
				GetModEffOnHitChance_AOE = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.effectOnHitChance_AOE : 0; };
				GetModEffOnHitChance_Mine = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.effectOnHitChance_Mine : 0; };
			}
			
			if(IsMultiplier()){
				GetMulCost= (prefabID) => 			{ return CheckID(prefabID) ? costMul : 1; };
				GetMulCostRsc = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.cost : null; };
				GetMulBuildDur = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.buildDuration : 1; };
				GetMulSellDur = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.sellDuration : 1; };
				
				GetMulHP = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hp : 1; };
				GetMulSH = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.sh : 1; };
				GetMulSHRegen = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.shRegen : 1; };
				GetMulSHStagger = (prefabID) => { return CheckID(prefabID) ? effect.stats.shStagger : 1; };
				
				GetMulDodge = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.dodge : 1; };
				GetMulDmgReduc = (prefabID) => { return CheckID(prefabID) ? effect.stats.dmgReduc : 1; };
				GetMulCritReduc = (prefabID) => { return CheckID(prefabID) ? effect.stats.dmgReduc : 1; };
				
				//GetMulSpeed = (prefabID) => 				{ return CheckID(prefabID) ? effect.stats.speed : 1; };
				GetMulDmgMin = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMin : 1; };
				GetMulDmgMax = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMax : 1; };
				GetMulRange = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.attackRange : 1; };
				GetMulAOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.aoeRange : 1; };
				GetMulCD = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.cooldown : 1; };
				GetMulHit = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hit : 1; };
				GetMulCrit = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.critChance : 1; };
				GetMulCritMul = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.critMultiplier : 1; };
				
				GetMulDmgMin_AOE = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.damageMin_AOE : 1; };
				GetMulDmgMax_AOE = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.damageMax_AOE : 1; };
				GetMulRange_AOE = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.attackRange_AOE : 1; };
				//GetMulAOE_AOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.aoeRange_AOE : 1; };
				GetMulCD_AOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.cooldown_AOE : 1; };
				GetMulHit_AOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hit_AOE : 1; };
				GetMulCrit_AOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.critChance_AOE : 1; };
				GetMulCritMul_AOE = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.critMultiplier_AOE : 1; };
				
				GetMulRange_Support = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.attackRange_Support : 1; };
				
				GetMulDmgMin_Mine = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.damageMin_Mine : 1; };
				GetMulDmgMax_Mine = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.damageMax_Mine : 1; };
				//GetMulRange_Mine = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.attackRange_Mine : 1; };
				GetMulAOE_Mine = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.aoeRange_Mine : 1; };
				GetMulCD_Mine = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.cooldown_Mine : 1; };
				GetMulHit_Mine = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hit_Mine : 1; };
				GetMulCrit_Mine = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.critChance_Mine : 1; };
				GetMulCritMul_Mine = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.critMultiplier_Mine : 1; };
				
				GetMulCD_Rsc = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.cooldown_Rsc : 1; };
				GetMulRscGain = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.rscGain : null; };
				
				GetMulEffOnHitChance = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.effectOnHitChance : 1; };
				GetMulEffOnHitChance_AOE = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.effectOnHitChance_AOE : 1; };
				GetMulEffOnHitChance_Mine = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.effectOnHitChance_Mine : 1; };
			}
			
			GetOverrideOnHitEff = (prefabID) => 		{ 
				if(!effect.stats.overrideExistingEffect || effect.stats.effectOnHitIDList.Count==0) return null;
				return CheckID(prefabID) ? effect.stats.effectOnHitIDList : null;
			};
			GetAppendOnHitEff = (prefabID) => 	{ 
				if(effect.stats.overrideExistingEffect || effect.stats.effectOnHitIDList.Count==0) return null;
				return CheckID(prefabID) ? effect.stats.effectOnHitIDList : null;
			};
			
			GetOverrideOnHitEff_AOE = (prefabID) => 		{ 
				if(!effect.stats.overrideExistingEffect_AOE || effect.stats.effectOnHitIDList_AOE.Count==0) return null;
				return CheckID(prefabID) ? effect.stats.effectOnHitIDList_AOE : null;
			};
			GetAppendOnHitEff_AOE = (prefabID) => 	{ 
				if(effect.stats.overrideExistingEffect_AOE || effect.stats.effectOnHitIDList_AOE.Count==0) return null;
				return CheckID(prefabID) ? effect.stats.effectOnHitIDList_AOE : null;
			};
			
			GetOverrideOnHitEff_Support = (prefabID) => 		{ 
				if(!effect.stats.overrideExistingEffect_Support || effect.stats.effectOnHitIDList_Support.Count==0) return null;
				return CheckID(prefabID) ? effect.stats.effectOnHitIDList_Support : null;
			};
			GetAppendOnHitEff_Support= (prefabID) => 	{ 
				if(effect.stats.overrideExistingEffect_Support || effect.stats.effectOnHitIDList_Support.Count==0) return null;
				return CheckID(prefabID) ? effect.stats.effectOnHitIDList_Support : null;
			};
			
			GetOverrideOnHitEff_Mine = (prefabID) => 		{ 
				if(!effect.stats.overrideExistingEffect_Mine || effect.stats.effectOnHitIDList_Mine.Count==0) return null;
				return CheckID(prefabID) ? effect.stats.effectOnHitIDList_Mine : null;
			};
			GetAppendOnHitEff_Mine = (prefabID) => 	{ 
				if(effect.stats.overrideExistingEffect_Mine || effect.stats.effectOnHitIDList_Mine.Count==0) return null;
				return CheckID(prefabID) ? effect.stats.effectOnHitIDList_Mine : null;
			};
			
			
			
			/*
			GetCostMul= (prefabID) => 		{ return CheckID(prefabID) ? costMul : 1; };
			GetCost = (prefabID) => 			{ return CheckID(prefabID) ? stats.cost : null; };
			GetBuildDur = (prefabID) => 	{ return CheckID(prefabID) ? stats.buildDuration : 1; };
			GetSellDur = (prefabID) => 		{ return CheckID(prefabID) ? stats.sellDuration : 1; };
			
			GetHP = (prefabID) => 			{ return CheckID(prefabID) ? stats.hp : 1; };
			GetSH = (prefabID) => 			{ return CheckID(prefabID) ? stats.sh : 1; };
			GetSHRegen = (prefabID) => 	{ return CheckID(prefabID) ? stats.shRegen : 1; };
			GetSHStagger = (prefabID) => 	{ return CheckID(prefabID) ? stats.shStagger : 1; };
			
			//GetSpeed = (prefabID) => 			{ return CheckID(prefabID) ? stats.speed : 1; };
			GetHit = (prefabID) => 			{ return CheckID(prefabID) ? stats.hit : 1; };
			GetDodge = (prefabID) => 		{ return CheckID(prefabID) ? stats.dodge : 1; };
			GetDmgMin = (prefabID) => 		{ return CheckID(prefabID) ? stats.damageMin : 1; };
			GetDmgMax = (prefabID) => 	{ return CheckID(prefabID) ? stats.damageMax : 1; };
			GetRange = (prefabID) => 		{ return CheckID(prefabID) ? stats.attackRange : 1; };
			GetAOE = (prefabID) => 			{ return CheckID(prefabID) ? stats.aoeRange : 1; };
			GetCD = (prefabID) => 			{ return CheckID(prefabID) ? stats.cooldown : 1; };
			GetCrit = (prefabID) => 			{ return CheckID(prefabID) ? stats.critChance : 1; };
			GetCritMul = (prefabID) => 		{ return CheckID(prefabID) ? stats.critMultiplier : 1; };
			GetDmgReduc = (prefabID) => 	{ return CheckID(prefabID) ? stats.dmgReduction : 1; };
			GetRscGain = (prefabID) => 		{ return CheckID(prefabID) ? stats.rscGain : null; };
			*/
			
			
			/*
			//GetUseEffOnHit = (prefabID) => 		{ return CheckID(prefabID) ? stats.useEffectOnHit : false; };
			//~ GetHitEffDuration = (prefabID) => 		{ return CheckID(prefabID) ? effect.duration : 1; };
			//~ GetHitEffHPRate = (prefabID) => 		{ return CheckID(prefabID) ? effect.hpRate : 1; };
			//~ GetHitEffSHRate = (prefabID) => 		{ return CheckID(prefabID) ? effect.shRate : 1; };
			
			//~ GetHitEffHP = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hp : 1; };
			//~ GetHitEffSH = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.sh : 1; };
			//~ GetHitEffSHRegen = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.shRegen : 1; };
			//~ GetHitEffSHStagger = (prefabID) => 	{ return CheckID(prefabID) ? effect.stats.shStagger : 1; };
			
			//~ GetHitEffSpeed = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.speed : 1; };
			//~ GetHitEffHit = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.hit : 1; };
			//~ GetHitEffDodge = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.dodge : 1; };
			//~ GetHitEffDmgMin = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.damageMin : 1; };
			//~ GetHitEffDmgMax = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.damageMax : 1; };
			//~ GetHitEffRange = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.attackRange : 1; };
			//~ GetHitEffAOE = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.aoeRange : 1; };
			//~ GetHitEffCD = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.cooldown : 1; };
			//~ GetHitEffCrit = (prefabID) => 			{ return CheckID(prefabID) ? effect.stats.critChance : 1; };
			//~ GetHitEffCritMul = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.critMultiplier : 1; };
			//~ GetHitEffRscGain = (prefabID) => 		{ return CheckID(prefabID) ? effect.stats.rscGain : null; };
			*/
		}
		
		

		
		public Func<int, float> GetModUseLimit = (prefabID) => { return 0; };	//for ability and perk only
		
		public Func<int, float> GetModCost = (prefabID) => { return 0; };	//for ability and perk only
		public Func<int, List<float>> GetModCostRsc = (prefabID) => { return null; };
		public Func<int, float> GetModBuildDur = (prefabID) => { return 0; };
		public Func<int, float> GetModSellDur = (prefabID) => { return 0; };
		//for base stats
		public Func<int, float> GetModHP = (prefabID) => { return 0; };
		public Func<int, float> GetModSH = (prefabID) => { return 0; };
		public Func<int, float> GetModSHRegen = (prefabID) => { return 0; };
		public Func<int, float> GetModSHStagger = (prefabID) => { return 0; };
		//public Func<int, float> GetModSpeed = (prefabID) => { return 0; };
		public Func<int, float> GetModDodge = (prefabID) => { return 0; };
		public Func<int, float> GetModDmgReduc = (prefabID) => { return 0; };
		public Func<int, float> GetModCritReduc = (prefabID) => { return 0; };
		
		public Func<int, float> GetModDmgMin = (prefabID) => { return 0; };
		public Func<int, float> GetModDmgMax = (prefabID) => { return 0; };
		public Func<int, float> GetModRange = (prefabID) => { return 0; };
		public Func<int, float> GetModAOE = (prefabID) => { return 0; };
		public Func<int, float> GetModCD = (prefabID) => { return 0; };
		public Func<int, float> GetModHit = (prefabID) => { return 0; };
		public Func<int, float> GetModCrit = (prefabID) => { return 0; };
		public Func<int, float> GetModCritMul = (prefabID) => { return 0; };
		
		public Func<int, float> GetModDmgMin_AOE = (prefabID) => { return 0; };
		public Func<int, float> GetModDmgMax_AOE = (prefabID) => { return 0; };
		public Func<int, float> GetModRange_AOE = (prefabID) => { return 0; };
		//public Func<int, float> GetModAOE_AOE = (prefabID) => { return 0; };
		public Func<int, float> GetModCD_AOE = (prefabID) => { return 0; };
		public Func<int, float> GetModHit_AOE = (prefabID) => { return 0; };
		public Func<int, float> GetModCrit_AOE = (prefabID) => { return 0; };
		public Func<int, float> GetModCritMul_AOE = (prefabID) => { return 0; };
		
		public Func<int, float> GetModRange_Support = (prefabID) => { return 0; };
		
		public Func<int, float> GetModDmgMin_Mine = (prefabID) => { return 0; };
		public Func<int, float> GetModDmgMax_Mine = (prefabID) => { return 0; };
		//public Func<int, float> GetModRange_Mine = (prefabID) => { return 0; };
		public Func<int, float> GetModAOE_Mine = (prefabID) => { return 0; };
		public Func<int, float> GetModCD_Mine = (prefabID) => { return 0; };
		public Func<int, float> GetModHit_Mine = (prefabID) => { return 0; };
		public Func<int, float> GetModCrit_Mine = (prefabID) => { return 0; };
		public Func<int, float> GetModCritMul_Mine = (prefabID) => { return 0; };
		
		public Func<int, float> GetModEffOnHitChance = (prefabID) => { return 0; };				//for effectOnHitChance
		public Func<int, float> GetModEffOnHitChance_AOE = (prefabID) => { return 0; };				
		public Func<int, float> GetModEffOnHitChance_Mine = (prefabID) => { return 0; };	
		
		//for resource gain
		public Func<int, float> GetModCD_Rsc = (prefabID) => { return 0; };
		public Func<int, List<float>> GetModRscGain = (prefabID) => { return null; };
		
		
		
		public Func<int, float> GetMulCost = (prefabID) => { return 1; };		//for ability and perk only
		public Func<int, List<float>> GetMulCostRsc = (prefabID) => { return null; };
		public Func<int, float> GetMulBuildDur = (prefabID) => { return 1; };
		public Func<int, float> GetMulSellDur = (prefabID) => { return 1; };
		//for base stats
		public Func<int, float> GetMulHP = (prefabID) => { return 1; };
		public Func<int, float> GetMulSH = (prefabID) => { return 1; };
		public Func<int, float> GetMulSHRegen = (prefabID) => { return 1; };
		public Func<int, float> GetMulSHStagger = (prefabID) => { return 1; };
		//public Func<int, float> GetMulSpeed = (prefabID) => { return 1; };
		public Func<int, float> GetMulDodge = (prefabID) => { return 1; };
		public Func<int, float> GetMulDmgReduc = (prefabID) => { return 1; };
		public Func<int, float> GetMulCritReduc = (prefabID) => { return 1; };
		
		public Func<int, float> GetMulDmgMin = (prefabID) => { return 1; };
		public Func<int, float> GetMulDmgMax = (prefabID) => { return 1; };
		public Func<int, float> GetMulRange = (prefabID) => { return 1; };
		public Func<int, float> GetMulAOE = (prefabID) => { return 1; };
		public Func<int, float> GetMulCD = (prefabID) => { return 1; };
		public Func<int, float> GetMulHit = (prefabID) => { return 1; };
		public Func<int, float> GetMulCrit = (prefabID) => { return 1; };
		public Func<int, float> GetMulCritMul = (prefabID) => { return 1; };
		
		public Func<int, float> GetMulDmgMin_AOE = (prefabID) => { return 1; };
		public Func<int, float> GetMulDmgMax_AOE = (prefabID) => { return 1; };
		public Func<int, float> GetMulRange_AOE = (prefabID) => { return 1; };
		//public Func<int, float> GetMulAOE_AOE = (prefabID) => { return 1; };
		public Func<int, float> GetMulCD_AOE = (prefabID) => { return 1; };
		public Func<int, float> GetMulHit_AOE = (prefabID) => { return 1; };
		public Func<int, float> GetMulCrit_AOE = (prefabID) => { return 1; };
		public Func<int, float> GetMulCritMul_AOE = (prefabID) => { return 1; };
		
		public Func<int, float> GetMulRange_Support = (prefabID) => { return 1; };
		
		public Func<int, float> GetMulDmgMin_Mine = (prefabID) => { return 1; };
		public Func<int, float> GetMulDmgMax_Mine = (prefabID) => { return 1; };
		//public Func<int, float> GetMulRange_Mine = (prefabID) => { return 1; };
		public Func<int, float> GetMulAOE_Mine = (prefabID) => { return 1; };
		public Func<int, float> GetMulCD_Mine = (prefabID) => { return 1; };
		public Func<int, float> GetMulHit_Mine = (prefabID) => { return 1; };
		public Func<int, float> GetMulCrit_Mine = (prefabID) => { return 1; };
		public Func<int, float> GetMulCritMul_Mine = (prefabID) => { return 1; };
		
		public Func<int, float> GetMulEffOnHitChance = (prefabID) => { return 1; };				//for effectOnHitChance
		public Func<int, float> GetMulEffOnHitChance_AOE = (prefabID) => { return 1; };				
		public Func<int, float> GetMulEffOnHitChance_Mine = (prefabID) => { return 1; };				
		
		//for on hit effect
		//~ public Func<int, int> GetOverrideOnHitEff = (prefabID) => { return -1; };
		public Func<int, List<int>> GetOverrideOnHitEff = (prefabID) => { return null; };
		public Func<int, List<int>> GetAppendOnHitEff = (prefabID) => { return null; };
		
		public Func<int, List<int>> GetOverrideOnHitEff_AOE = (prefabID) => { return null; };
		public Func<int, List<int>> GetAppendOnHitEff_AOE = (prefabID) => { return null; };
		
		public Func<int, List<int>> GetOverrideOnHitEff_Support = (prefabID) => { return null; };
		public Func<int, List<int>> GetAppendOnHitEff_Support = (prefabID) => { return null; };
		
		public Func<int, List<int>> GetOverrideOnHitEff_Mine = (prefabID) => { return null; };
		public Func<int, List<int>> GetAppendOnHitEff_Mine = (prefabID) => { return null; };
		
		//for resource gain
		public Func<int, float> GetMulCD_Rsc = (prefabID) => { return 1; };
		public Func<int, List<float>> GetMulRscGain = (prefabID) => { return null; };
		
		
		
		//for towers, ability and perk
		public Func<int, List<float>> GetCost = (prefabID) => { return new List<float>(); };
		
		//for ability and perk, when not using RscManager 
		public Func<int, float> GetCostMul= (prefabID) => { return 1f; };
		
		/*
		//for towers only
		public Func<int, float> GetBuildDur = (prefabID) => { return 0; };
		public Func<int, float> GetSellDur = (prefabID) => { return 0; };
		
		
		//for base stats
		public Func<int, float> GetHP = (prefabID) => { return 0; };
		public Func<int, float> GetSH = (prefabID) => { return 0; };
		public Func<int, float> GetSHRegen = (prefabID) => { return 0; };
		public Func<int, float> GetSHStagger = (prefabID) => { return 0; };
		//public Func<int, float> GetSpeed = (prefabID) => { return 0; };
		public Func<int, float> GetHit = (prefabID) => { return 0; };
		public Func<int, float> GetDodge = (prefabID) => { return 0; };
		public Func<int, float> GetDmgMin = (prefabID) => { return 0; };
		public Func<int, float> GetDmgMax = (prefabID) => { return 0; };
		public Func<int, float> GetRange = (prefabID) => { return 0; };
		public Func<int, float> GetAOE = (prefabID) => { return 0; };
		public Func<int, float> GetCD = (prefabID) => { return 0; };
		public Func<int, float> GetCrit = (prefabID) => { return 0; };
		public Func<int, float> GetCritMul = (prefabID) => { return 0; };
		public Func<int, float> GetDmgReduc = (prefabID) => { return 0; };
		public Func<int, List<float>> GetRscGain = (prefabID) => { return new List<float>(); };
		
		
		
		//public Func<int, bool> GetUseEffOnHit = (prefabID) => { return false; };
		public Func<int, float> GetHitEffHPRate = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffSHRate = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffDuration = (prefabID) => { return 0; };
		
		public Func<int, float> GetHitEffHP = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffSH = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffSHRegen = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffSHStagger = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffSpeed = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffHit = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffDodge = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffDmgMin = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffDmgMax = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffRange = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffAOE = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffCD = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffCrit = (prefabID) => { return 0; };
		public Func<int, float> GetHitEffCritMul = (prefabID) => { return 0; };
		public Func<int, List<float>> GetHitEffRscGain = (prefabID) => { return new List<float>(); };
		*/
		#endregion
		
		
		
	}
	
}