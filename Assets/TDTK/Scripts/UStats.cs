using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{
	
	[System.Serializable]
	public class Stats {	//generic stats container for tower, creep and ability (used as multiplier for perks)
		//these are used for editor only
		[HideInInspector] public bool editTurret;	
		[HideInInspector] public bool editAOE;
		[HideInInspector] public bool editMine;
		
		
		[Header("For tower only")]
		public float buildDuration=1f;
		public float sellDuration=1f;
		public float expToNextUpgrade=10;	
		
		[Header("For Tower and Ability")]
		public List<float> cost=new List<float>();
		public List<float> sellValue=new List<float>();
		
		[Header("For Unit only")]
		public float hp=5;
		
		public float sh=0;	//shield
		public float shRegen=.5f;
		public float shStagger=3f;
		
		public float hpRate=0;	//can be negate by shield and subject to damageType 
		public float shRate=0;	//doesnt subject to stagger
		
		public int armorType=0;		//only used for base stats, not for effect
		public int damageType=0;	//only used for base stats, not for effect
		
		public float speed=3;
		
		public float dodge=0;
		public float dmgReduc=0;
		public float critReduc=0;
		
		
		[Header("For Everything")]
		public float hit=1;
		public float damageMin=2;
		public float damageMax=2;
		public float attackRange=2;
		public float aoeRange=0;
		public float cooldown=1;
		
		public float critChance=0;
		public float critMultiplier=0;
		
		[Space(8)] 
		public float effectOnHitChance=1;
		//public int effectOnHitID=-1;
		
		public bool overrideExistingEffect=false;	//for perk only
		public List<int> effectOnHitIDList=new List<int>();
		
		
		[Header("For AOE")]
		public float hit_AOE=1;
		public float damageMin_AOE=2;
		public float damageMax_AOE=2;
		public float attackRange_AOE=2;
		public float cooldown_AOE=1;

		public float critChance_AOE=0;
		public float critMultiplier_AOE=0;

		public float effectOnHitChance_AOE=1;
		//public int effectOnHitID_AOE=-1;
		
		public bool overrideExistingEffect_AOE=false;	//for perk only
		public List<int> effectOnHitIDList_AOE=new List<int>();
		

		[Header("For Mine")]
		public float hit_Mine=1;
		public float damageMin_Mine=2;
		public float damageMax_Mine=2;
		public float aoeRange_Mine=0;
		public float cooldown_Mine=1;

		public float critChance_Mine=0;
		public float critMultiplier_Mine=0;

		public float effectOnHitChance_Mine=1;
		//public int effectOnHitID_Mine=-1;
		
		public bool overrideExistingEffect_Mine=false;	//for perk only
		public List<int> effectOnHitIDList_Mine=new List<int>();
		
		
		[Header("For Support")]
		public float attackRange_Support=2;
		public bool overrideExistingEffect_Support=false;	//for perk only
		public List<int> effectOnHitIDList_Support=new List<int>();
		
		
		
		[Header("For Rsctower only")]
		public float cooldown_Rsc=1;
		public List<float> rscGain=new List<float>();
		
		
		public virtual void VerifyBaseStats(bool checkHP, bool checkCD, bool checkHit){
			if(checkHP && hp<=0){
				Debug.LogWarning("Unit hp is set as 0. Adjust it to 1 instead");
				hp=Mathf.Max(1, hp);
			}
			
			if(checkCD && cooldown<=0){
				Debug.LogWarning("Unit cooldown is set to equal or less than 0. Adjust it to 1 instead");
				cooldown=1;
			}
			
			if(checkHit && hit<=0){
				Debug.LogWarning("Unit/Item hit chance is set to equal or less than 0. Adjust it to 1 instead");
				hit=1;
			}
			
			//damageMin=Mathf.Max(0, damageMin);
			//damageMax=Mathf.Max(0, damageMax);
			
			VerifyRscList(0);
		}
		
		public virtual void ResetAsBaseStat(){
			Reset(false);
			hp=1;	speed=1;
			hit=1;	damageMin=1;		damageMax=1;	attackRange=1;	cooldown=1;
		}
		
		public virtual void Reset(bool isMultiplier=false){
			cost=new List<float>();	rscGain=new List<float>();
			if(isMultiplier){
				buildDuration=1;		sellDuration=1;
				hp=1;	sh=1;		shRegen=1;		shStagger=1;			speed=1;		hpRate=1;	shRate=1;
				dodge=1;				dmgReduc=1;		critReduc=1;
				
				hit=1;					damageMin=1;			damageMax=1;
				attackRange=1;		aoeRange=1;			cooldown=1;
				critChance=1;			critMultiplier=1;		effectOnHitChance=1;
				
				hit_AOE=1;						damageMin_AOE=1;		damageMax_AOE=1;
				attackRange_AOE=1;		/*aoeRange_AOE=1;*/		cooldown_AOE=1;
				critChance_AOE=1;			critMultiplier_AOE=1;	effectOnHitChance_AOE=1;
				
				hit_Mine=1;					damageMin_Mine=1;		damageMax_Mine=1;
				/*attackRange_Mine=1;*/		aoeRange_Mine=1;		cooldown_Mine=1;
				critChance_Mine=1;			critMultiplier_Mine=1;	effectOnHitChance_Mine=1;
				
				attackRange_Support=1;
				
				cooldown_Rsc=1;				VerifyRscList(1);
			}
			else{
				buildDuration=0;		sellDuration=0;
				hp=0;	sh=0;		shRegen=0;		shStagger=0;			speed=0;		hpRate=0;	shRate=0;
				dodge=0;				dmgReduc=0;		critReduc=0;
				
				hit=0;					damageMin=0;			damageMax=0;
				attackRange=0;		aoeRange=0;			cooldown=0;
				critChance=0;			critMultiplier=0;		effectOnHitChance=0;
				
				hit_AOE=0;						damageMin_AOE=0;		damageMax_AOE=0;
				attackRange_AOE=0;		/*aoeRange_AOE=0;*/		cooldown_AOE=0;
				critChance_AOE=0;			critMultiplier_AOE=0;	effectOnHitChance_AOE=0;
				
				hit_Mine=0;					damageMin_Mine=0;		damageMax_Mine=0;
				/*attackRange_Mine=0;*/		aoeRange_Mine=0;		cooldown_Mine=0;
				critChance_Mine=0;			critMultiplier_Mine=0;	effectOnHitChance_Mine=0;
				
				attackRange_Support=0;
				
				cooldown_Rsc=0;				VerifyRscList(0);
			}
		}
		
		
		
		public void VerifyRscList(float fillValue){ 
			cost=RscManager.MatchRscList(cost, fillValue);
			rscGain=RscManager.MatchRscList(rscGain, fillValue);
		}
		
		
		public Stats Clone(){ 
			Stats clone=new Stats();
			
			clone.buildDuration=buildDuration;
			clone.sellDuration=sellDuration;
			clone.expToNextUpgrade=expToNextUpgrade;
			
			clone.cost=new List<float>( cost );
			clone.sellValue=new List<float>( sellValue );
			
			clone.hp=hp;
			
			clone.sh=sh;
			clone.shRegen=shRegen;
			clone.shStagger=shStagger;
			
			clone.hpRate=hpRate;
			clone.shRate=shRate;
			
			clone.armorType=armorType;
			clone.damageType=damageType;
			
			clone.speed=speed;
			
			clone.dodge=dodge;
			clone.dmgReduc=dmgReduc;
			clone.critReduc=critReduc;
			
			//turret
			clone.hit=hit;
			clone.damageMin=damageMin;
			clone.damageMax=damageMax;
			clone.attackRange=attackRange;
			clone.aoeRange=aoeRange;
			clone.cooldown=cooldown;
			
			clone.critChance=critChance;
			clone.critMultiplier=critMultiplier;
			
			clone.effectOnHitChance=effectOnHitChance;
			clone.effectOnHitIDList=new List<int>( effectOnHitIDList );
			//clone.effectOnHitID=effectOnHitID;
			
			//AOE
			clone.hit_AOE=hit_AOE;
			clone.damageMin_AOE=damageMin_AOE;
			clone.damageMax_AOE=damageMax_AOE;
			clone.attackRange_AOE=attackRange_AOE;
			clone.cooldown_AOE=cooldown_AOE;
			
			clone.critChance_AOE=critChance_AOE;
			clone.critMultiplier_AOE=critMultiplier_AOE;
			
			clone.effectOnHitChance_AOE=effectOnHitChance_AOE;
			clone.effectOnHitIDList_AOE=new List<int>( effectOnHitIDList_AOE );
			//clone.effectOnHitID_AOE=effectOnHitID_AOE;
			
			//Mine
			clone.hit_Mine=hit_Mine;
			clone.damageMin_Mine=damageMin_Mine;
			clone.damageMax_Mine=damageMax_Mine;
			clone.aoeRange_Mine=aoeRange_Mine;
			clone.cooldown_Mine=cooldown_Mine;
			
			clone.critChance_Mine=critChance_Mine;
			clone.critMultiplier_Mine=critMultiplier_Mine;
			
			clone.effectOnHitChance_Mine=effectOnHitChance_Mine;
			clone.effectOnHitIDList_Mine=new List<int>( effectOnHitIDList_Mine );
			//clone.effectOnHitID_Mine=effectOnHitID_Mine;
			
			
			clone.attackRange_Support=attackRange_Support;
			clone.effectOnHitIDList_Support=new List<int>( effectOnHitIDList_Support );
			
			
			clone.cooldown_Rsc=cooldown_Rsc;
			clone.rscGain=new List<float>( rscGain );
			
			
			clone.overrideExistingEffect=overrideExistingEffect;
			clone.overrideExistingEffect_AOE=overrideExistingEffect_AOE;
			clone.overrideExistingEffect_Support=overrideExistingEffect_Support;
			clone.overrideExistingEffect_Mine=overrideExistingEffect_Mine;
			
			return clone;
			
			//return ObjectCopier.Clone(this);
		}
	}
	
	
	
	[System.Serializable]
	public class Effect : TDItem {//Stats{		//attack buff/debuff to be apply on target
		public int ID=0;	//assigned during runtime, unique to unit-type/ability that cast it, used to determined if the effect existed on target if it's not stackable
		
		public enum _SrcType{ Tower, Ability, Creep, Perk, None}
		public _SrcType srcType=_SrcType.None;
		public int srcPrefabID=0;
		
		//public Unit srcUnit;
		public bool fromSupport;
		public int stack=1;		//use by support unit to prevent effect stacking
		
		public bool ignoreSource;	//when true, the effect will stackablility wont take account of the source
		public bool stackable=false;
		public float duration=1;
		//[HideInInspector] 
		public float durationRemain=1;	//runtime attribute, only used on applied effect
		
		public enum _EffType{ Modifier, Multiplier }
		public _EffType effType=_EffType.Multiplier;
		public bool IsMultiplier(){ return effType==_EffType.Multiplier; }
		
		public bool stun=false;
		public Stats stats=new Stats();
		
		
		public VisualObject hitVisualEffect;
		public Transform activeVisualEffect;
		
		
		public Effect(){} //ResetAsEffect(); }
		
		public void SetAsModifier(bool reset=true){	effType=_EffType.Modifier;		if(reset) Reset(true); }
		public void SetAsMultiplier(bool reset=true){	effType=_EffType.Multiplier;	if(reset) Reset(true); }
		public void Reset(bool isPerkEffect=false){
			stun=false;
			if(effType==_EffType.Multiplier){ stats.Reset(true);		if(isPerkEffect) duration=1; }
			if(effType==_EffType.Modifier)	{ stats.Reset(false);		if(isPerkEffect) duration=0; }
		}
		
		
		public void SetType(int id, Ability ability){
			ID=id;	srcPrefabID=ability.prefabID;
			srcType=_SrcType.Ability;
		}
		public void SetType(int id, Unit unit){
			ID=id;	srcPrefabID=unit.prefabID;		stack=1;	//srcUnit=unit;		
			if(unit.IsTower()) srcType=_SrcType.Tower;
			if(unit.IsCreep()) srcType=_SrcType.Creep;
		}
		
		public static bool FromSimilarSource(Effect eff1, Effect eff2){
			if(eff1.ignoreSource && eff2.ignoreSource){
				if(eff1.prefabID==eff2.prefabID) return true;
			}
			
			if(eff1.ID!=eff2.ID) return false;
			if(eff1.srcType!=eff2.srcType) return false;
			if(eff1.srcPrefabID!=eff2.srcPrefabID) return false;
			return true;
		}
		
		public bool FromTower(){ return srcType==_SrcType.Tower; }
		public bool FromCreep(){ return srcType==_SrcType.Creep; }
		
		
		public Effect ModifyWithPerk(){ 
			return PerkManager.ModifyEffect(Clone());
		}
		
		
		//Called by PerkManager when applying changes,  also when doing activeEffect on unit
		public void ApplyModifier(Effect effMod, float hpRateMultiplier=1){	
			stun|=effMod.stun;
			duration+=effMod.duration;
			
			stats.hp+=effMod.stats.hp;
			stats.sh+=effMod.stats.sh;
			stats.shRegen+=effMod.stats.shRegen;
			stats.shStagger+=effMod.stats.shStagger;
			
			stats.hpRate+=effMod.stats.hpRate * hpRateMultiplier;
			stats.shRate+=effMod.stats.shRate;
			
			stats.dodge+=effMod.stats.dodge;
			stats.dmgReduc+=effMod.stats.dmgReduc;
			stats.critReduc+=effMod.stats.critReduc;
			
			stats.speed+=effMod.stats.speed;
			
			
			stats.damageMin+=effMod.stats.damageMin;
			stats.damageMax+=effMod.stats.damageMax;
			stats.attackRange+=effMod.stats.attackRange;
			stats.aoeRange+=effMod.stats.aoeRange;
			stats.cooldown+=effMod.stats.cooldown;
			
			stats.hit+=effMod.stats.hit;
			stats.critChance+=effMod.stats.critChance;
			stats.critMultiplier+=effMod.stats.critMultiplier;
			stats.effectOnHitChance+=effMod.stats.effectOnHitChance;
			
			
			stats.damageMin_AOE+=effMod.stats.damageMin_AOE;
			stats.damageMax_AOE+=effMod.stats.damageMax_AOE;
			stats.attackRange_AOE+=effMod.stats.attackRange_AOE;
			stats.cooldown_AOE+=effMod.stats.cooldown_AOE;
			
			stats.hit_AOE+=effMod.stats.hit_AOE;
			stats.critChance_AOE+=effMod.stats.critChance_AOE;
			stats.critMultiplier_AOE+=effMod.stats.critMultiplier_AOE;
			stats.effectOnHitChance_AOE+=effMod.stats.effectOnHitChance_AOE;
			
			
			stats.damageMin_Mine+=effMod.stats.damageMin_Mine;
			stats.damageMax_Mine+=effMod.stats.damageMax_Mine;
			stats.aoeRange_Mine+=effMod.stats.aoeRange_Mine;
			stats.cooldown_Mine+=effMod.stats.cooldown_Mine;
			
			stats.hit_Mine+=effMod.stats.hit_Mine;
			stats.critChance_Mine+=effMod.stats.critChance_Mine;
			stats.critMultiplier_Mine+=effMod.stats.critMultiplier_Mine;
			stats.effectOnHitChance_Mine+=effMod.stats.effectOnHitChance_Mine;
			
			
			stats.attackRange_Support+=effMod.stats.attackRange_Support;
			
			stats.cooldown_Rsc+=effMod.stats.cooldown_Rsc;
		}
		//Called by PerkManager when applying changes, also when doing activeEffect on unit
		public void ApplyMultiplier(Effect effMul){		
			duration*=effMul.duration;
			
			stats.hp*=effMul.stats.hp;
			stats.sh*=effMul.stats.sh;
			stats.shRegen*=effMul.stats.shRegen;
			stats.shStagger*=effMul.stats.shStagger;
			
			stats.hpRate*=effMul.stats.hpRate;
			stats.shRate*=effMul.stats.shRate;
			
			stats.dodge*=effMul.stats.dodge;
			stats.dmgReduc*=effMul.stats.dmgReduc;
			stats.critReduc*=effMul.stats.critReduc;
			
			stats.speed*=effMul.stats.speed;
			
			
			stats.damageMin*=effMul.stats.damageMin;
			stats.damageMax*=effMul.stats.damageMax;
			stats.attackRange*=effMul.stats.attackRange;
			stats.aoeRange*=effMul.stats.aoeRange;
			stats.cooldown*=effMul.stats.cooldown;
			
			stats.hit*=effMul.stats.hit;
			stats.critChance*=effMul.stats.critChance;
			stats.critMultiplier*=effMul.stats.critMultiplier;
			stats.effectOnHitChance*=effMul.stats.effectOnHitChance;
			
			
			stats.damageMin_AOE*=effMul.stats.damageMin_AOE;
			stats.damageMax_AOE*=effMul.stats.damageMax_AOE;
			stats.attackRange_AOE*=effMul.stats.attackRange_AOE;
			stats.cooldown_AOE*=effMul.stats.cooldown_AOE;
			
			stats.hit_AOE*=effMul.stats.hit_AOE;
			stats.critChance_AOE*=effMul.stats.critChance_AOE;
			stats.critMultiplier_AOE*=effMul.stats.critMultiplier_AOE;
			stats.effectOnHitChance_AOE*=effMul.stats.effectOnHitChance_AOE;
			
			
			stats.damageMin_Mine*=effMul.stats.damageMin_Mine;
			stats.damageMax_Mine*=effMul.stats.damageMax_Mine;
			stats.aoeRange_Mine*=effMul.stats.aoeRange_Mine;
			stats.cooldown_Mine*=effMul.stats.cooldown_Mine;
			
			stats.hit_Mine*=effMul.stats.hit_Mine;
			stats.critChance_Mine*=effMul.stats.critChance_Mine;
			stats.critMultiplier_Mine*=effMul.stats.critMultiplier_Mine;
			stats.effectOnHitChance_Mine*=effMul.stats.effectOnHitChance_Mine;
			
			
			stats.attackRange_Support*=effMul.stats.attackRange_Support;
			
			stats.cooldown_Rsc*=effMul.stats.cooldown_Rsc;
		}
		
		
		
		public Effect Clone(){ 
			Effect clone=new Effect();
			
			base.Clone(this, clone);
			
			clone.ID=ID;
			clone.srcType=srcType;
			clone.srcPrefabID=srcPrefabID;
			
			clone.ignoreSource=ignoreSource;
			clone.stackable=stackable;
			clone.duration=duration;
			clone.effType=effType;
			
			clone.stun=stun;
			clone.stats=stats.Clone();
			
			clone.hitVisualEffect=hitVisualEffect.Clone();
			clone.activeVisualEffect=activeVisualEffect;
			
			return clone;
		}
		
	}
	
	
	
	public class AttackInfo{
		public Unit srcUnit;
		public Unit tgtUnit;
		
		public float damageMin=0;
		public float damageMax=0;
		public float aoeRange=0;
		
		public float critChance=0;
		public float critMultiplier=0;
		
		//public Effect effect;
		public List<Effect> effectList=new List<Effect>();
		
		//actual value
		public float damage=0;
		public bool hit=false;
		public bool critical=false;
		
		public bool UseEffect(){ return effectList.Count!=0; }//effect!=null; }
		
		
		public AttackInfo(float dmg){ hit=true;	damage=dmg; }	//not in used
		
		public AttackInfo(Unit sUnit, Unit tUnit, int type, bool useAOE=true){	//type: 0-turret, 1-aoe, 2-mine
			srcUnit=sUnit;	tgtUnit=tUnit;
			
			if(!tUnit.canBeAttacked) return;
			
			if(type==1){	//aoe
				if(Random.value>srcUnit.GetHit_AOE()-tgtUnit.GetDodge()){
					damage=1;	//otherwise the 'miss' popup text wont show up
					return;
				}
				
				damageMin=srcUnit.GetDamageMin_AOE();
				damageMax=srcUnit.GetDamageMax_AOE();
				critChance=srcUnit.GetCritChance_AOE();
				critMultiplier=tUnit.GetImmunedToCrit() ? 0 : srcUnit.GetCritMultiplier_AOE();
				
				if(Random.value<srcUnit.GetEffectOnHitChance_AOE()) effectList=srcUnit.GetEffectOnHit_AOE();
			}
			else if(type==2){	//mine
				if(Random.value>srcUnit.GetHit_Mine()-tgtUnit.GetDodge()){
					damage=1;	//otherwise the 'miss' popup text wont show up
					return;
				}
				
				damageMin=srcUnit.GetDamageMin_Mine();
				damageMax=srcUnit.GetDamageMax_Mine();
				//aoeRange=0;	//useAOE ? srcUnit.GetAOERange_Mine() : 0;			//aoe is done on tower side
				critChance=srcUnit.GetCritChance_Mine();
				critMultiplier=tUnit.GetImmunedToCrit() ? 0 : srcUnit.GetCritMultiplier_Mine();
				
				if(Random.value<srcUnit.GetEffectOnHitChance_Mine()) effectList=srcUnit.GetEffectOnHit_Mine();
			}
			else{
				if(Random.value>srcUnit.GetHit()-tgtUnit.GetDodge()){
					damage=1;	//otherwise the 'miss' popup text wont show up
					return;
				}
				
				damageMin=srcUnit.GetDamageMin();
				damageMax=srcUnit.GetDamageMax();
				aoeRange=useAOE ? srcUnit.GetAOERange() : 0;
				critChance=srcUnit.GetCritChance();
				critMultiplier=tUnit.GetImmunedToCrit() ? 0 : srcUnit.GetCritMultiplier();
				
				if(Unit.damagePerShootPoint){
					damageMin*=1f/srcUnit.shootPoint.Count;
					damageMax*=1f/srcUnit.shootPoint.Count;
				}
				
				if(Random.value<srcUnit.GetEffectOnHitChance()) effectList=srcUnit.GetEffectOnHit();
			}
			
			hit=true;
			
			critical=Random.value<critChance;
			damage=Mathf.Round(Random.Range(damageMin, damageMax))*(critical ? critMultiplier : 1);
			
			damage*=DamageTable.GetModifier(tgtUnit.GetArmorType(), srcUnit.GetDamageType());
			damage*=(1-tUnit.GetDmgReduction());
		}
		
		public AttackInfo(Ability ability, Unit tUnit, bool useAOE=true){
			tgtUnit=tUnit;
			
			if(!tUnit.canBeAttacked) return;
			
			damageMin=ability.GetDamageMin();
			damageMax=ability.GetDamageMax();
			aoeRange=useAOE ? ability.GetAOERange() : 0;
			critChance=ability.GetCrit();
			critMultiplier=tUnit.GetImmunedToCrit() ? 0 : ability.GetCritMultiplier();
			
			//~ useEffect=ability.UseEffectOnHit();
			//~ if(useEffect) effect=ability.GetEffectOnHit().Clone();
			if(Random.value<ability.GetEffectOnHitChance()){
				effectList=ability.GetEffectOnHit();
				//effect=ability.GetEffectOnHit();
			}
			
			critical=Random.value<critChance;
			damage=Mathf.Round(Random.Range(damageMin, damageMax))*(critical ? critMultiplier : 1);
			
			if(damage>0){
				damage*=DamageTable.GetModifier(tgtUnit.GetArmorType(), ability.GetDamageType());
				damage*=(1-tUnit.GetDmgReduction());
			}
			
			//Debug.Log(damage+"   "+ability.GetDamageMin()+"   "+ability.GetDamageMax());
			
			if(Random.value<ability.GetHit()-tgtUnit.GetDodge()) hit=true;
		}
	}
	
}