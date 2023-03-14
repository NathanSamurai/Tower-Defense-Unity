using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{

	public class PerkManager : MonoBehaviour {
		
		public static bool IsEnabled(){ return instance!=null; }
		
		public bool inGameScene=true;
		public static bool InGameScene(){ return instance.inGameScene; }
		
		
		private static bool cached=false;
		private static List<int> cachedPurchasedIDList=new List<int>();	//prefabID of the perk purchased
		public static List<int> GetCachedPurchasedIDList(){ return cachedPurchasedIDList; }
		private static int cachedRsc=0;
		public static int GetCachedRsc(){ return cachedRsc; }
		
		public bool loadFromCache=false;
		public bool cacheOnLevelWon=false;
		public bool cacheOnRestart=false;
		
		
		
		public bool useRscManagerForCost=false;
		public static bool UseRscManagerForCost(){ return instance!=null && instance.useRscManagerForCost; }
		
		public int rsc=0;
		public static bool HasSufficientRsc(float value){ return instance.rsc>=value; }
		public static void GainRsc(int value){ 
			if(instance==null) return;
			instance.rsc+=value;
			if(value!=0) TDTK.OnPerkRscChanged(instance.rsc);
		}
		public static void SpendRsc(int value){
			instance.rsc-=value;
			if(value!=0) TDTK.OnPerkRscChanged(instance.rsc);
		}
		public static int GetRsc(){ return instance.rsc; }
		
		
		public List<int> unavailablePrefabIDList=new List<int>();
		public List<int> purchasedPrefabIDList=new List<int>();
		public static List<int> GetPurchasedPrefabIDList(){ return instance.purchasedPrefabIDList; }
		
		public static int GetPurchasedPerkCount(){ return instance.purchasedPrefabIDList.Count; }
		
		
		public List<Perk> perkList=new List<Perk>();
		public static List<Perk> GetPerkList(){ return instance.perkList; }
		public static Perk GetPerkFromList(int idx){ return instance.perkList[idx]; }
		
		public static Perk GetPerk(int prefabID){ 
			for(int i=0; i<instance.perkList.Count; i++){
				if(instance.perkList[i].prefabID==prefabID) return instance.perkList[i]; 
			}
			return null;
		}
		
		
		private static List<float> defaultRsclistMod=new List<float>();	//a dummy list for for getting perk modifier for rsc
		private static List<float> defaultRsclistMul=new List<float>();	//a dummy list for for getting perk multiplier for rsc
		
		
		private static PerkManager instance;
		
		void Awake(){
			instance=this;
			
			//see TDSave for reference
			//purchasedPrefabIDList=TDSave.LoadPerk();
			
			uPIDList=new List<int>();
			aPIDList=new List<int>();
			ePIDList=new List<int>();
			tuPIDList=new List<int>();
			
			if(loadFromCache && cached){
				rsc=cachedRsc;
				purchasedPrefabIDList=cachedPurchasedIDList;
			}
			
			//Load();
			
			List<Perk> dbList=Perk_DB.GetList();
			for(int i=0; i<dbList.Count; i++){
				if(!unavailablePrefabIDList.Contains(dbList[i].prefabID)){
					perkList.Add(dbList[i].Clone());
				}
			}
		}
		
		
		IEnumerator Start(){
			yield return null;
			
			defaultRsclistMod=new List<float>();
			defaultRsclistMul=new List<float>();
			
			rscGainModList=new List<int>();
			rscGainModCreepKilledList=new List<int>();
			rscGainModWaveClearedList=new List<int>();
			rscGainModRscTowerList=new List<int>();
			
			rscGainMulList=new List<float>();
			rscGainMulCreepKilledList=new List<float>();
			rscGainMulWaveClearedList=new List<float>();
			rscGainMulRscTowerList=new List<float>();
			
			for(int i=0; i<RscManager.GetResourceCount(); i++){
				defaultRsclistMod.Add(0);
				defaultRsclistMul.Add(1);
				
				rscGainModList.Add(0);
				rscGainModCreepKilledList.Add(0);
				rscGainModWaveClearedList.Add(0);
				rscGainModRscTowerList.Add(0);
				
				rscGainMulList.Add(1);
				rscGainMulCreepKilledList.Add(1);
				rscGainMulWaveClearedList.Add(1);
				rscGainMulRscTowerList.Add(1);
			}
			
			if(purchasedPrefabIDList.Count>0){
				for(int i=0; i<perkList.Count; i++){
					if(!purchasedPrefabIDList.Contains(perkList[i].prefabID)) continue;
					_PurchasePerk(i, false);
				}
			}	
			
			WaveCleared(0);
			
			yield return null;
		}
		
		
		void OnDestroy(){
			if(!inGameScene || cacheOnRestart) CachedProgress();
		}
		
		
		//private bool progressCached=false;	//
		
		public static void CachedProgress(){	//called when level is won
			if(instance==null) return;
			
			//if(!instance.cacheOnLevelWon) return;
			
			cachedRsc=instance.rsc;
			
			for(int i=0; i<instance.perkList.Count; i++){
				if(!instance.perkList[i].IsPurchased()) continue;
				if(cachedPurchasedIDList.Contains(instance.perkList[i].prefabID)) continue;
				cachedPurchasedIDList.Add(instance.perkList[i].prefabID);
			}
			
			cached=true;
			//instance.progressCached=true;
			
			//instance.Save();
		}
		
		
		
		public static void WaveCleared(int waveIdx){
			if(instance==null) return;
			
			for(int i=0; i<instance.perkList.Count; i++){
				if(instance.perkList[i].IsPurchased()) continue;
				if(instance.perkList[i].autoUnlockOnWave>0 && instance.perkList[i].autoUnlockOnWave<=waveIdx+1) instance._PurchasePerk(i, false);
			}
		}
		
		
		public static List<int> VerifyPerkPrereq(List<int> prereq){ return instance._VerifyPerkPrereq(prereq); }
		public List<int> _VerifyPerkPrereq(List<int> prereq){
			for(int i=0; i<prereq.Count; i++){
				if(purchasedPrefabIDList.Contains(prereq[i])){ prereq.RemoveAt(i); i-=1; }
			}
			return prereq;
		}
		
		public static bool PurchasePerk(int idx){ return instance._PurchasePerk(idx); }
		public bool _PurchasePerk(int idx, bool useRsc=true){
			if(useRsc && !perkList[idx].HasSufficientRsc()){
				GameControl.InvalidAction("Insufficient Resource");
				return false;
			}
			
			perkList[idx].Purchase(useRsc);
			
			if(!purchasedPrefabIDList.Contains(perkList[idx].prefabID)) purchasedPrefabIDList.Add(perkList[idx].prefabID);
			//if(perkList[idx].type==_PerkType.TowerAll || perkList[idx].type==_PerkType.Tower) uPIDList.Add(perkList[idx].prefabID);
			
			Debug.Log("_PurchasePerk   "+perkList[idx].name+"    "+perkList[idx].prefabID);
			
			if(useRsc) AudioManager.OnPerkPurchased();
			
			return true;
		}
		
		
		//[Space(8)]
		//public int purchaseID;
		//void Update(){
		//	if(Input.GetKeyDown(KeyCode.Space)) _PurchasePerk(purchaseID);
		//}
		
		
		
		#region rsc gain multiplier and modifier
		public static int rscGainMod=0;						//modifier for all rsc gain
		public static int rscGainModCreepKilled=0;		//for creep killed
		public static int rscGainModWaveCleared=0;	//for wave cleared
		public static int rscGainModRscTower=0;
		
		public static List<int> rscGainModList=new List<int>();					//modifier for all rsc gain
		public static List<int> rscGainModCreepKilledList=new List<int>();		//for creep killed
		public static List<int> rscGainModWaveClearedList=new List<int>();	//for wave cleared
		public static List<int> rscGainModRscTowerList=new List<int>();
		
		public static float rscGainMul=1;						//multiplier for all rsc gain
		public static float rscGainMulCreepKilled=1;			//for creep killed
		public static float rscGainMulWaveCleared=1;		//for wave cleared
		public static float rscGainMulRscTower=1;
		
		public static List<float> rscGainMulList=new List<float>();						//multiplier for all rsc gain
		public static List<float> rscGainMulCreepKilledList=new List<float>();		//for creep killed
		public static List<float> rscGainMulWaveClearedList=new List<float>();		//for wave cleared
		public static List<float> rscGainMulRscTowerList=new List<float>();
		
		
		
		public static void AddRscGainMul(float val){ rscGainMul*=val; }
		public static void AddRscGainMulCreepKilled(float val){ rscGainMulCreepKilled*=val; }
		public static void AddRscGainMulWaveCleared(float val){ rscGainMulWaveCleared*=val; }
		public static void AddRscGainMulRscTower(float val){ rscGainMulRscTower*=val; }
		
		public static void AddRscGainMulList(List<float> list){ rscGainMulList=RscManager.ApplyMultiplier(rscGainMulList, list); }
		public static void AddRscGainMulCreepKilledList(List<float> list){ 
			rscGainMulCreepKilledList=RscManager.ApplyMultiplier(rscGainMulCreepKilledList, list);
		}
		public static void AddRscGainMulWaveClearedList(List<float> list){ 
			rscGainMulWaveClearedList=RscManager.ApplyMultiplier(rscGainMulWaveClearedList, list);
		}
		public static void AddRscGainMulRscTowerList(List<float> list){ 
			rscGainMulRscTowerList=RscManager.ApplyMultiplier(rscGainMulRscTowerList, list);
		}
		
		public static void AddRscGainMod(int val){ rscGainMod+=val; Debug.Log(rscGainMod); GetGlobalRscGainMod(0); }
		public static void AddRscGainModCreepKilled(int val){ rscGainModCreepKilled+=val; }
		public static void AddRscGainModWaveCleared(int val){ rscGainModWaveCleared+=val; }
		public static void AddRscGainModRscTower(int val){ rscGainModRscTower+=val; }
		
		public static void AddRscGainModList(List<float> list){
			rscGainModList=RscManager.ApplyModifier(rscGainModList, list);
		}
		public static void AddRscGainModCreepKilledList(List<float> list){ 
			rscGainModCreepKilledList=RscManager.ApplyModifier(rscGainModCreepKilledList, list);
		}
		public static void AddRscGainModWaveClearedList(List<float> list){ 
			rscGainModWaveClearedList=RscManager.ApplyModifier(rscGainModWaveClearedList, list);
		}
		public static void AddRscGainModRscTowerList(List<float> list){ 
			rscGainModRscTowerList=RscManager.ApplyModifier(rscGainModRscTowerList, list);
		}
		
		
		public static List<float> ApplyRscGain(List<float> list){
			if(instance==null) return list;
			for(int i=0; i<list.Count; i++) list[i]+=GetGlobalRscGainMod(i);
			for(int i=0; i<list.Count; i++) list[i]*=GetGlobalRscGainMul(i);
			return list;
		}
		public static List<float> ApplyRscGainCreepKilled(List<float> list){
			if(instance==null) return list;
			for(int i=0; i<list.Count; i++) list[i]+=rscGainModCreepKilled+rscGainModCreepKilledList[i]+GetGlobalRscGainMod(i);
			for(int i=0; i<list.Count; i++) list[i]*=rscGainMulCreepKilled*rscGainMulCreepKilledList[i]*GetGlobalRscGainMul(i);
			return list;
		}
		public static List<float> ApplyRscGainWaveCleared(List<float> list){
			if(instance==null) return list;
			for(int i=0; i<list.Count; i++) list[i]+=rscGainModWaveCleared+rscGainModWaveClearedList[i]+GetGlobalRscGainMod(i);
			for(int i=0; i<list.Count; i++) list[i]*=rscGainMulWaveCleared*rscGainMulWaveClearedList[i]*GetGlobalRscGainMul(i);
			return list;
		}
		public static List<float> ApplyRscGainRscTower(List<float> list){
			if(instance==null) return list;
			for(int i=0; i<list.Count; i++) list[i]+=rscGainModRscTower+rscGainModRscTowerList[i]+GetGlobalRscGainMod(i);
			for(int i=0; i<list.Count; i++) list[i]*=rscGainMulRscTower*rscGainMulRscTowerList[i]*GetGlobalRscGainMul(i);
			return list;
		}
		
		public static float GetGlobalRscGainMod(int idx){ return rscGainMod+rscGainModList[idx]; }
		public static float GetGlobalRscGainMul(int idx){ return rscGainMul*rscGainMulList[idx]; }
		#endregion
		
		
		
		#region perk's cost multiplier
		public static List<int> pPIDList=new List<int>();	//prefabID of all unit related perk that has been unlocked
		public static void AddPerkPerkID(int ID){ pPIDList.Add(ID); } 
		
		public static void ModifyPerkCostMultiplier(float val, List<float> mulList){}
		
		public static float GetPerkCostMul(int prefabID, float value=1){
			for(int i=0; i<pPIDList.Count; i++){ value*=GetPerk(pPIDList[i]).GetCostMul(prefabID); } return value;
		}
		public static List<float> GetPerkCost(int prefabID, List<float> itemList=null){
			List<float> baseList=new List<float>( defaultRsclistMul );
			for(int i=0; i<pPIDList.Count; i++){ 
				itemList=GetPerk(pPIDList[i]).GetCost(prefabID);
				baseList=RscManager.ApplyMultiplier(baseList, itemList);
			} 
			return baseList;
		}
		#endregion
		
		
		
		#region life and ability rsc waveCleared gain
		public static int lifeGainOnWaveClearedMod=0;
		public static float lifeGainOnWaveClearedMul=1;
		
		public static int GetLifeGainOnWaveCleared(int gain){ return (int)Mathf.Round(gain*lifeGainOnWaveClearedMul)+lifeGainOnWaveClearedMod; }
		
		public static int abRscGainOnWaveClearedMod=0;
		public static float abRscGainOnWaveClearedMul=1;
		
		public static int GetAbRscGainOnWaveCleared(int gain){ return (int)Mathf.Round(gain*abRscGainOnWaveClearedMul)+abRscGainOnWaveClearedMod; }
		#endregion
		
		
		
		
		#region unit's perk
		public static List<int> uPIDList=new List<int>();	//prefabID of all unit related perk that has been unlocked
		public static void AddUnitPerkID(int ID){ uPIDList.Add(ID); } 
		
		
		//~ public static int GetUnitOverrideOnHitEff(int prefabID, int value=-1, int dummy=0){
			//~ for(int i=0; i<uPIDList.Count; i++){ 
				//~ dummy=GetPerk(uPIDList[i]).GetOverrideOnHitEff(prefabID);
				//~ if(dummy>=0) value=dummy;
			//~ }
			//~ return value;
		//~ }
		public static List<int> GetUnitOverrideOnHitEff(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<uPIDList.Count; i++){ 
				dummy=GetPerk(uPIDList[i]).GetOverrideOnHitEff(prefabID);
				if(dummy!=null) list=dummy;
			}
			return list;
		}
		public static List<int> GetUnitAppendOnHitEff(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<uPIDList.Count; i++){ 
				dummy=GetPerk(uPIDList[i]).GetAppendOnHitEff(prefabID);
				if(dummy!=null){
					if(list==null) list=new List<int>( dummy );
					else for(int n=0; n<dummy.Count; n++) list.Add(dummy[n]);
				}
			}
			return list;
		}
		
		public static List<int> GetUnitOverrideOnHitEff_AOE(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<uPIDList.Count; i++){ 
				dummy=GetPerk(uPIDList[i]).GetOverrideOnHitEff_AOE(prefabID);
				if(dummy!=null) list=dummy;
			}
			return list;
		}
		public static List<int> GetUnitAppendOnHitEff_AOE(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<uPIDList.Count; i++){ 
				dummy=GetPerk(uPIDList[i]).GetAppendOnHitEff_AOE(prefabID);
				if(dummy!=null){
					if(list==null) list=new List<int>( dummy );
					else for(int n=0; n<dummy.Count; n++) list.Add(dummy[n]);
				}
			}
			return list;
		}
		
		public static List<int> GetUnitOverrideOnHitEff_Support(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<uPIDList.Count; i++){ 
				dummy=GetPerk(uPIDList[i]).GetOverrideOnHitEff_Support(prefabID);
				if(dummy!=null) list=dummy;
			}
			return list;
		}
		public static List<int> GetUnitAppendOnHitEff_Support(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<uPIDList.Count; i++){ 
				dummy=GetPerk(uPIDList[i]).GetAppendOnHitEff_Support(prefabID);
				if(dummy!=null){
					if(list==null) list=new List<int>( dummy );
					else for(int n=0; n<dummy.Count; n++) list.Add(dummy[n]);
				}
			}
			return list;
		}
		
		public static List<int> GetUnitOverrideOnHitEff_Mine(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<uPIDList.Count; i++){ 
				dummy=GetPerk(uPIDList[i]).GetOverrideOnHitEff_Mine(prefabID);
				if(dummy!=null) list=dummy;
			}
			return list;
		}
		public static List<int> GetUnitAppendOnHitEff_Mine(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<uPIDList.Count; i++){ 
				dummy=GetPerk(uPIDList[i]).GetAppendOnHitEff_Mine(prefabID);
				if(dummy!=null){
					if(list==null) list=new List<int>( dummy );
					else for(int n=0; n<dummy.Count; n++) list.Add(dummy[n]);
				}
			}
			return list;
		}
		
		
		//modifier
		public static List<float> GetModUnitCost(int prefabID, List<float> modList=null){
			List<float> baseList=new List<float>( defaultRsclistMod );
			for(int i=0; i<uPIDList.Count; i++){ 
				modList=GetPerk(uPIDList[i]).GetModCostRsc(prefabID);
				baseList=RscManager.ApplyModifier(baseList, modList);
			} 
			return baseList;
		}
		public static float GetModUnitBuildDur(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModBuildDur(prefabID); } return value;
		}
		public static float GetModUnitSellDur(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModSellDur(prefabID); } return value;
		}
		
		public static float GetModUnitHP(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModHP(prefabID); } return value;
		}
		public static float GetModUnitSH(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModSH(prefabID); } return value;
		}
		public static float GetModUnitSHRegen(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModSHRegen(prefabID); } return value;
		}
		public static float GetModUnitSHStagger(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModSHStagger(prefabID); } return value;
		}
		public static float GetModUnitDodge(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModDodge(prefabID); } return value;
		}
		public static float GetModUnitDmgReduc(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModDmgReduc(prefabID); } return value;
		}
		public static float GetModUnitCritReduc(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCritReduc(prefabID); } return value;
		}
		
		public static float GetModUnitDmgMin(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModDmgMin(prefabID); } return value;
		}
		public static float GetModUnitDmgMax(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModDmgMax(prefabID); } return value;
		}
		public static float GetModUnitAttackRange(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModRange(prefabID); } return value;
		}
		public static float GetModUnitAOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModAOE(prefabID); } return value;
		}
		public static float GetModUnitCD(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCD(prefabID); } return value;
		}
		public static float GetModUnitHit(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModHit(prefabID); } return value;
		}
		public static float GetModUnitCrit(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCrit(prefabID); } return value;
		}
		public static float GetModUnitCritMul(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCritMul(prefabID); } return value;
		}
		
		public static float GetModUnitDmgMin_AOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModDmgMin_AOE(prefabID); } return value;
		}
		public static float GetModUnitDmgMax_AOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModDmgMax_AOE(prefabID); } return value;
		}
		public static float GetModUnitAttackRange_AOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModRange_AOE(prefabID); } return value;
		}
		//public static float GetModUnitAOE_AOE(int prefabID, float value=0){
		//	for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModAOE_AOE(prefabID); } return value;
		//}
		public static float GetModUnitCD_AOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCD_AOE(prefabID); } return value;
		}
		public static float GetModUnitHit_AOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModHit_AOE(prefabID); } return value;
		}
		public static float GetModUnitCrit_AOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCrit_AOE(prefabID); } return value;
		}
		public static float GetModUnitCritMul_AOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCritMul_AOE(prefabID); } return value;
		}
		
		public static float GetModUnitAttackRange_Support(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModRange_Support(prefabID); } return value;
		}
		
		public static float GetModUnitDmgMin_Mine(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModDmgMin_Mine(prefabID); } return value;
		}
		public static float GetModUnitDmgMax_Mine(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModDmgMax_Mine(prefabID); } return value;
		}
		//public static float GetModUnitAttackRange_Mine(int prefabID, float value=0){
		//	for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModRange_Mine(prefabID); } return value;
		//}
		public static float GetModUnitAOE_Mine(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModAOE_Mine(prefabID); } return value;
		}
		public static float GetModUnitCD_Mine(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCD_Mine(prefabID); } return value;
		}
		public static float GetModUnitHit_Mine(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModHit_Mine(prefabID); } return value;
		}
		public static float GetModUnitCrit_Mine(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCrit_Mine(prefabID); } return value;
		}
		public static float GetModUnitCritMul_Mine(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCritMul_Mine(prefabID); } return value;
		}
		
		public static float GetModUnitCD_Rsc(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModCD_Rsc(prefabID); } return value;
		}
		
		public static float GetModUnitEffOnHitChance(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModEffOnHitChance(prefabID); } return value;
		}
		public static float GetModUnitEffOnHitChance_AOE(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModEffOnHitChance_AOE(prefabID); } return value;
		}
		public static float GetModUnitEffOnHitChance_Mine(int prefabID, float value=0){
			for(int i=0; i<uPIDList.Count; i++){ value+=GetPerk(uPIDList[i]).GetModEffOnHitChance_Mine(prefabID); } return value;
		}
		public static List<float> GetModUnitRscGain(int prefabID, List<float> modList=null){
			List<float> baseList=new List<float>( defaultRsclistMod );
			for(int i=0; i<uPIDList.Count; i++){ 
				modList=GetPerk(uPIDList[i]).GetModCostRsc(prefabID);
				baseList=RscManager.ApplyModifier(baseList, modList);
			} 
			return baseList;
		}
		
		
		
		
		//multiplier
		public static List<float> GetMulUnitCost(int prefabID, List<float> mulList=null){
			List<float> baseList=new List<float>( defaultRsclistMul );
			for(int i=0; i<uPIDList.Count; i++){ 
				mulList=GetPerk(uPIDList[i]).GetMulCostRsc(prefabID);
				baseList=RscManager.ApplyMultiplier(baseList, mulList);
			} 
			return baseList;
		}
		public static float GetMulUnitBuildDur(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulBuildDur(prefabID); } return value;
		}
		public static float GetMulUnitSellDur(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulSellDur(prefabID); } return value;
		}
		
		public static float GetMulUnitHP(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulHP(prefabID); } return value;
		}
		public static float GetMulUnitSH(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulSH(prefabID); } return value;
		}
		public static float GetMulUnitSHRegen(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulSHRegen(prefabID); } return value;
		}
		public static float GetMulUnitSHStagger(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulSHStagger(prefabID); } return value;
		}
		public static float GetMulUnitDodge(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulDodge(prefabID); } return value;
		}
		public static float GetMulUnitDmgReduc(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulDmgReduc(prefabID); } return value;
		}
		public static float GetMulUnitCritReduc(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCritReduc(prefabID); } return value;
		}
		
		public static float GetMulUnitDmgMin(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulDmgMin(prefabID); } return value;
		}
		public static float GetMulUnitDmgMax(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulDmgMax(prefabID); } return value;
		}
		public static float GetMulUnitAttackRange(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulRange(prefabID); } return value;
		}
		public static float GetMulUnitAOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulAOE(prefabID); } return value;
		}
		public static float GetMulUnitCD(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCD(prefabID); } return value;
		}
		public static float GetMulUnitHit(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulHit(prefabID); } return value;
		}
		public static float GetMulUnitCrit(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCrit(prefabID); } return value;
		}
		public static float GetMulUnitCritMul(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCritMul(prefabID); } return value;
		}
		
		public static float GetMulUnitDmgMin_AOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulDmgMin_AOE(prefabID); } return value;
		}
		public static float GetMulUnitDmgMax_AOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulDmgMax_AOE(prefabID); } return value;
		}
		public static float GetMulUnitAttackRange_AOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulRange_AOE(prefabID); } return value;
		}
		//public static float GetMulUnitAOE_AOE(int prefabID, float value=1){
		//	for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulAOE_AOE(prefabID); } return value;
		//}
		public static float GetMulUnitCD_AOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCD_AOE(prefabID); } return value;
		}
		public static float GetMulUnitHit_AOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulHit_AOE(prefabID); } return value;
		}
		public static float GetMulUnitCrit_AOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCrit_AOE(prefabID); } return value;
		}
		public static float GetMulUnitCritMul_AOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCritMul_AOE(prefabID); } return value;
		}
		
		public static float GetMulUnitAttackRange_Support(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulRange_Support(prefabID); } return value;
		}
		
		public static float GetMulUnitDmgMin_Mine(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulDmgMin_Mine(prefabID); } return value;
		}
		public static float GetMulUnitDmgMax_Mine(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulDmgMax_Mine(prefabID); } return value;
		}
		//public static float GetMulUnitAttackRange_Mine(int prefabID, float value=1){
		//	for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulRange_Mine(prefabID); } return value;
		//}
		public static float GetMulUnitAOE_Mine(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulAOE_Mine(prefabID); } return value;
		}
		public static float GetMulUnitCD_Mine(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCD_Mine(prefabID); } return value;
		}
		public static float GetMulUnitHit_Mine(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulHit_Mine(prefabID); } return value;
		}
		public static float GetMulUnitCrit_Mine(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCrit_Mine(prefabID); } return value;
		}
		public static float GetMulUnitCritMul_Mine(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCritMul_Mine(prefabID); } return value;
		}
		
		public static float GetMulUnitCD_Rsc(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulCD_Rsc(prefabID); } return value;
		}
		
		public static float GetMulUnitEffOnHitChance(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulEffOnHitChance(prefabID); } return value;
		}
		public static float GetMulUnitEffOnHitChance_AOE(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulEffOnHitChance_AOE(prefabID); } return value;
		}
		public static float GetMulUnitEffOnHitChance_Mine(int prefabID, float value=1){
			for(int i=0; i<uPIDList.Count; i++){ value*=GetPerk(uPIDList[i]).GetMulEffOnHitChance_Mine(prefabID); } return value;
		}
		public static List<float> GetMulUnitRscGain(int prefabID, List<float> mulList=null){
			List<float> baseList=new List<float>( defaultRsclistMul );
			for(int i=0; i<uPIDList.Count; i++){ 
				mulList=GetPerk(uPIDList[i]).GetMulCostRsc(prefabID);
				baseList=RscManager.ApplyModifier(baseList, mulList);
			} 
			return baseList;
		}
		#endregion
		
		
		#region abilities perk
		public static List<int> aPIDList=new List<int>();	//prefabID of all ability related perk that has been unlocked
		public static void AddAbilityPerkID(int ID){ aPIDList.Add(ID); } 
		
		//~ public static int GetAbilityOverrideOnHitEff(int prefabID, int value=-1, int dummy=0){
			//~ for(int i=0; i<aPIDList.Count; i++){ 
				//~ dummy=GetPerk(aPIDList[i]).GetOverrideOnHitEff(prefabID);
				//~ if(dummy>=0) value=dummy;
			//~ }
			//~ return value;
		//~ }
		public static List<int> GetAbilityOverrideOnHitEff(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<aPIDList.Count; i++){ 
				dummy=GetPerk(aPIDList[i]).GetOverrideOnHitEff(prefabID);
				if(dummy!=null) list=dummy;
			}
			return list;
		}
		public static List<int> GetAbilityAppendOnHitEff(int prefabID, List<int> list=null, List<int> dummy=null){
			for(int i=0; i<aPIDList.Count; i++){ 
				dummy=GetPerk(aPIDList[i]).GetAppendOnHitEff(prefabID);
				if(dummy!=null){
					if(list==null) list=new List<int>( dummy );
					else for(int n=0; n<dummy.Count; n++) list.Add(dummy[n]);
				}
			}
			return list;
		}
		
		//modifier
		public static List<float> GetModAbilityCostRsc(int prefabID, List<float> modList=null){
			List<float> baseList=new List<float>( defaultRsclistMod );
			for(int i=0; i<uPIDList.Count; i++){ 
				modList=GetPerk(uPIDList[i]).GetModCostRsc(prefabID);
				baseList=RscManager.ApplyModifier(baseList, modList);
			} 
			return baseList;
		}
		public static float GetModAbilityCost(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModCost(prefabID); } return value;
		}
		public static float GetModAbilityUseLimit(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModUseLimit(prefabID); } return value;
		}
		
		public static float GetModAbilityDmgMin(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModDmgMin(prefabID); } return value;
		}
		public static float GetModAbilityDmgMax(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModDmgMax(prefabID); } return value;
		}
		public static float GetModAbilityAOE(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModAOE(prefabID); } return value;
		}
		public static float GetModAbilityCD(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModCD(prefabID); } return value;
		}
		public static float GetModAbilityHit(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModHit(prefabID); } return value;
		}
		public static float GetModAbilityCrit(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModCrit(prefabID); } return value;
		}
		public static float GetModAbilityCritMul(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModCritMul(prefabID); } return value;
		}
		public static float GetModAbilityEffOnHitChance(int prefabID, float value=0){
			for(int i=0; i<aPIDList.Count; i++){ value+=GetPerk(aPIDList[i]).GetModEffOnHitChance(prefabID); } return value;
		}
		
		//multiplier
		public static List<float> GetMulAbilityCostRsc(int prefabID, List<float> mulList=null){
			List<float> baseList=new List<float>( defaultRsclistMul );
			for(int i=0; i<uPIDList.Count; i++){ 
				mulList=GetPerk(uPIDList[i]).GetMulCostRsc(prefabID);
				baseList=RscManager.ApplyMultiplier(baseList, mulList);
			} 
			return baseList;
		}
		public static float GetMulAbilityCost(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulCost(prefabID); } return value;
		}
		//uselimit
		
		public static float GetMulAbilityDmgMin(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulDmgMin(prefabID); } return value;
		}
		public static float GetMulAbilityDmgMax(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulDmgMax(prefabID); } return value;
		}
		public static float GetMulAbilityAOE(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulAOE(prefabID); } return value;
		}
		public static float GetMulAbilityCD(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulCD(prefabID); } return value;
		}
		public static float GetMulAbilityHit(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulHit(prefabID); } return value;
		}
		public static float GetMulAbilityCrit(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulCrit(prefabID); } return value;
		}
		public static float GetMulAbilityCritMul(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulCritMul(prefabID); } return value;
		}
		public static float GetMulAbilityEffOnHitChance(int prefabID, float value=1){
			for(int i=0; i<aPIDList.Count; i++){ value*=GetPerk(aPIDList[i]).GetMulEffOnHitChance(prefabID); } return value;
		}
		#endregion
		
		
		
		
		#region effect effect
		public static List<int> ePIDList=new List<int>();	//prefabID of all ability related perk that has been unlocked
		public static void AddEffectPerkID(int ID){ ePIDList.Add(ID); } 
		
		public static Effect ModifyEffect(Effect eff){
			Effect effMod=new Effect();	effMod.SetAsModifier();
			Effect effMul=new Effect();		effMul.SetAsMultiplier();
			
			for(int i=0; i<ePIDList.Count; i++){
				Perk perk=GetPerk(ePIDList[i]);
				if(!perk.CheckID(eff.prefabID)) continue;
				
				effMul.stun|=perk.effect.stun;
				
				if(perk.IsMultiplier()) effMul.ApplyMultiplier(perk.effect);
				else effMod.ApplyModifier(perk.effect);
			}
			
			eff.ApplyModifier(effMod);
			eff.ApplyMultiplier(effMul);
			
			return eff;
		}
		#endregion
		
		
		
		#region tower upgrade
		public static List<int> tuPIDList=new List<int>();	//prefabID of all tower upgrade related perk that has been unlocked
		public static void AddTowerUpgradePerkID(int ID){ tuPIDList.Add(ID); }
		
		public static List<int> GetTowerUpgradeUnlockedList(int pID, int count){
			List<int> list=new List<int>();
			for(int i=0; i<tuPIDList.Count; i++){
				Perk perk=GetPerk(tuPIDList[i]);
				
				if(!perk.CheckID(pID)) continue;
				
				if(!perk.unlockAllUpgrade){
					for(int n=0; n<perk.towerUpgradeList.Count; n++){
						if(list.Contains(perk.towerUpgradeList[n])) continue;
						list.Add(perk.towerUpgradeList[n]);
						if(list.Count==count) break;
					}
				}
				else{
					list.Clear();
					for(int n=0; n<count; n++) list.Add(n);
					return list;
				}
			}
			return list;
		}
		#endregion
		
		
		
		private static bool loaded=false;
		public void Load(){
			if(loaded) return;
			loaded=true;
			
			rsc=PlayerPrefs.GetInt("TDTK_PerkRsc");
			
			List<int> purchasedPrefabIDList=new List<int>();
			int count=PlayerPrefs.GetInt("TDTK_PerkCount", 0);
			for(int i=0; i<count; i++) purchasedPrefabIDList.Add(PlayerPrefs.GetInt("TDTK_PerkID_"+i, 0));
			
			//Debug.Log("load   "+rsc+"    "+purchasedPrefabIDList.Count);
			
			TDTK.OnPerkRscChanged(rsc);
		}
		public void Save2(){
			//Debug.Log("save perks  ");
			
			PlayerPrefs.SetInt("TDTK_PerkRsc", GetCachedRsc());
			
			List<int> perkList=GetCachedPurchasedIDList();
			PlayerPrefs.SetInt("TDTK_PerkCount", perkList.Count);
			for(int i=0; i<perkList.Count; i++) PlayerPrefs.SetInt("TDTK_PerkID_"+i, perkList[i]);
		}
		
		// /*save mid-game	
		public void Save(){
			PlayerPrefs.SetInt("TDTK_PerkRsc", rsc);
			
			int count=0;
			for(int i=0; i<perkList.Count; i++){
				if(perkList[i].IsPurchased()){
					count+=1;
					PlayerPrefs.SetInt("TDTK_PerkID_"+i, perkList[i].prefabID);
				}
			}
			PlayerPrefs.SetInt("TDTK_PerkCount", count);
		}
		//*/
		
		
	}

}