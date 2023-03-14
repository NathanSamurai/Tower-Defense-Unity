using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK {

	public class RscManager : MonoBehaviour {
		
		public static List<int> cachedList=new List<int>();
		public static List<int> GetCachedList(){ return cachedList; }
		public bool carryOver=false;
		
		public float sellValueMultiplier=1f;
		public static float GetSellMultiplier(){ return instance.sellValueMultiplier; }
		
		public bool regenerateRsc=false;
		public List<float> regenModList=new List<float>();
		public List<float> regenMulList=new List<float>();
		private List<float> regenCachedList=new List<float>();
		
		private static List<RscItem> rscItemList=new List<RscItem>();
		public List<int> rscList=new List<int>{ 100 };
		
		
		public static List<int> GetResourceList(){ return instance.rscList; }
		public static int GetResourceCount(){ 
			if(!Application.isPlaying) return Rsc_DB.GetCount();
			return Init() ? instance.rscList.Count : Rsc_DB.GetCount();
		}
		
		
		private static RscManager instance;
		
		void Awake() {
			if(instance!=null && instance!=this){
				Debug.LogWarning("Multiple RscManager in scene! ");
				return;
			}
			
			rscItemList=new List<RscItem>( Rsc_DB.GetList() );
			if(rscItemList.Count!=rscList.Count){
				while(rscList.Count<rscItemList.Count) rscList.Add(0);
				while(rscList.Count>rscItemList.Count) rscList.RemoveAt(rscList.Count-1);
				
				while(regenModList.Count<rscItemList.Count) regenModList.Add(0);
				while(regenModList.Count>rscItemList.Count) regenModList.RemoveAt(regenModList.Count-1);
				
				while(regenMulList.Count<rscItemList.Count) regenMulList.Add(1);
				while(regenMulList.Count>rscItemList.Count) regenMulList.RemoveAt(regenMulList.Count-1);
			}
			
			for(int i=0; i<rscItemList.Count; i++){
				//regenModList.Add(rscItemList[i].regenRate);
				//regenMulList.Add(rscItemList[i].regenRateMultiplier);
				regenCachedList.Add(0);
			}
			
			if(carryOver && cachedList.Count==rscList.Count){
				for(int i=0; i<rscList.Count; i++){
					if(!rscItemList[i].enableCarry) continue;
					rscList[i]=cachedList[i];
				}
			}
			
			instance=this;
		}
		
		public static bool Init(){
			if(instance!=null) return true;
			instance = (RscManager)FindObjectOfType(typeof(RscManager));
			if(instance==null){ Debug.LogWarning("No RscManager in scene!"); return false; }
			return true;
		}
		
		
		/*
		private static bool rscLoaded=false;
		public void Start(){
			if(rscLoaded) return;
			rscLoaded=true;
			
			List<int> valueList=TDSave.LoadRsc();
			if(rscList.Count>=2 && valueList.Count>=2) rscList[1]=valueList[1];
			else Debug.Log("rscList count is less than 2");
		}
		*/

		
		
		public static void CachedRsc(){	//called when level is won
			if(!instance.carryOver) return;
			cachedList=new List<int>();
			for(int i=0; i<instance.rscList.Count; i++){
				if(rscItemList[i].enableCarry) cachedList.Add(instance.rscList[i]);
				else cachedList.Add(-1);
			}
		}
		
		
		void FixedUpdate(){
			RegenerateRsc();
		}
		
		
		public void RegenerateRsc(){
			if(!regenerateRsc) return;
			
			bool requireUpate=false;
			
			for(int i=0; i<rscList.Count; i++){
				//if(rscItemList[i].regenRate==0) continue;
				regenCachedList[i]+=GetRegenRate(i)*Time.fixedDeltaTime;
				if(regenCachedList[i]>1){
					float gain=Mathf.Floor(regenCachedList[i]);
					rscList[i]+=(int)gain;
					regenCachedList[i]-=gain;
					requireUpate=true;
				}
			}
			
			if(requireUpate) TDTK.OnRscChanged(rscList);
		}
		public float GetRegenRate(int idx){
			//return (rscItemList[idx].regenRate+regenModList[idx])*regenMulList[idx];// + PerkManager.GetRecRegenRate(idx);
			return (regenModList[idx])*regenMulList[idx];// + PerkManager.GetRecRegenRate(idx);
		}
		public static void ModifyRegenModRate(List<float> list){
			for(int i=0; i<instance.regenModList.Count; i++) instance.regenModList[i]+=list[i];
		}
		public static void ModifyRegenMulRate(List<float> list){
			for(int i=0; i<instance.regenMulList.Count; i++) instance.regenMulList[i]+=list[i];
		}
		
		
		
		
		public static string GetRscName(int idx){ return idx>=rscItemList.Count ? "" : rscItemList[idx].name; }
		public static Sprite GetRscIcon(int idx){ return idx>=rscItemList.Count ? null :rscItemList[idx].icon; }
		
		//~ void OnGUI(){
			//~ string text="";
			//~ for(int i=0; i<rscList.Count; i++) text+=rscList[i]+"   ";
			//~ GUI.Label(new Rect(300, 20, 200, 25), text);
		//~ }
		
		
		public static List<float> IntToFloatList(List<int> list){
			List<float> newList=new List<float>();
			for(int i=0; i<list.Count; i++) newList.Add(list[i]);
			return newList;
		}
		
		
		public static List<int> MatchRscList(List<int> list, int fillValue){
			int count=Rsc_DB.GetCount();
			if(count!=list.Count){ while(list.Count<count) list.Add(fillValue); }
			if(count!=list.Count){ while(list.Count>count) list.RemoveAt(list.Count-1); }
			return list;
		}
		public static List<float> MatchRscList(List<float> list, float fillValue){
			int count=Rsc_DB.GetCount();
			if(count!=list.Count){ while(list.Count<count) list.Add(fillValue); }
			if(count!=list.Count){ while(list.Count>count) list.RemoveAt(list.Count-1); }
			return list;
		}
		
		public enum _GainType{ Generic, CreepKilled, WaveCleared, RscTower }
		
		//~ public static void GainRsc(List<int> list, float mul=1){ instance._GainRsc(IntToFloatList(list), mul); }
		//~ public static void GainRsc(List<float> list, float mul=1){ instance._GainRsc(list, mul); }
		//~ public void _GainRsc(List<float> list, float mul=1){
		public static void GainRsc(List<int> list, _GainType type=_GainType.Generic){ instance._GainRsc(IntToFloatList(list), type); }
		public static void GainRsc(List<float> list, _GainType type=_GainType.Generic){ instance._GainRsc(list, type); }
		public void _GainRsc(List<float> list, _GainType type=_GainType.Generic){
			if(!VerifyRscList(list.Count)) return;
			
			//mul*=PerkManager.GetRscGainMul();
			//if(mul!=1) list=ApplyMultiplier(list, mul);
			
			if(type==_GainType.Generic)				list=PerkManager.ApplyRscGain(list);
			else if(type==_GainType.CreepKilled)		list=PerkManager.ApplyRscGainCreepKilled(list);
			else if(type==_GainType.WaveCleared)	list=PerkManager.ApplyRscGainWaveCleared(list);
			else if(type==_GainType.RscTower)		list=PerkManager.ApplyRscGainRscTower(list);
			
			for(int i=0; i<list.Count; i++) rscList[i]+=(int)Mathf.Round(list[i]);
			
			//Debug.Log("Gain  "+list[0]+"    "+rscList[0]+"   "+type);
			
			TDTK.OnRscChanged(rscList);
		}
		
		//~ public static bool SpendRsc(List<int> list, float mul=1){ return instance._SpendRsc(IntToFloatList(list), mul); }
		//~ public static bool SpendRsc(List<float> list, float mul=1){ return instance._SpendRsc(list, mul); }
		//~ public bool _SpendRsc(List<float> list, float mul=1){
		public static bool SpendRsc(List<int> list){ return instance._SpendRsc(IntToFloatList(list)); }
		public static bool SpendRsc(List<float> list){ return instance._SpendRsc(list); }
		public bool _SpendRsc(List<float> list){
			//Debug.Log("SpendRsc  "+list[0]+"    "+rscList[0]);
			
			if(!VerifyRscList(list.Count)) return false;
			
			//~ if(mul!=1) list=ApplyMultiplier(list, mul);
			if(!_HasSufficientRsc(list)) return false;
			
			//Debug.Log(" - SpendRsc  "+list[0]+"    "+rscList[0]);
			
			for(int i=0; i<list.Count; i++) rscList[i]-=(int)Mathf.Round(list[i]);
			
			TDTK.OnRscChanged(rscList);
			return true;
		}
		
		
		
		public static List<int> ApplyModifier(List<int> list, List<float> mod){
			if(mod==null || list.Count!=mod.Count) return list;
			for(int i=0; i<list.Count; i++) list[i]=list[i]+(int)mod[i];
			return list;
		}
		public static List<float> ApplyModifier(List<float> list, List<float> mod){
			if(mod==null || list.Count!=mod.Count) return list;
			for(int i=0; i<list.Count; i++) list[i]=list[i]+mod[i];
			return list;
		}
		
		public static List<int> ApplyMultiplier(List<int> list, float mul=1){
			for(int i=0; i<list.Count; i++) list[i]=(int)Mathf.Round(list[i]*mul);
			return list;
		}
		public static List<float> ApplyMultiplier(List<float> list, List<float> mul){
			if(mul==null || list.Count!=mul.Count) return list;
			for(int i=0; i<list.Count; i++) list[i]=list[i]*mul[i];
			return list;
		}
		public static List<float> ApplyMultiplier(List<float> list, float mul=1){
			for(int i=0; i<list.Count; i++) list[i]=list[i]*mul;
			return list;
		}
		
		
		public static bool HasSufficientRsc(List<int> list){ return instance._HasSufficientRsc(IntToFloatList(list)); }
		public static bool HasSufficientRsc(List<float> list){ return instance._HasSufficientRsc(list); }
		public bool _HasSufficientRsc(List<float> list){
			if(!VerifyRscList(list.Count)){
				return false;
			}
			
			for(int i=0; i<list.Count; i++){
				if(list[i]>rscList[i]) return false;
			}
			return true;
		}
		
		public static bool VerifyRscList(int count){
			if(instance.rscList.Count<count){
				return false;
			}
			return true;
		}
		
	}
	
	
	[System.Serializable]
	public class RscItem{
		public string name="";
		public Sprite icon;
		
		//public float regenRate=0;
		//public float regenRateMultiplier=1;
		
		public bool enableCarry=true;
	}
	
}