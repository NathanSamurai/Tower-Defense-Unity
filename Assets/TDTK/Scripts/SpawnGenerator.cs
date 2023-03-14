using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{
	
	[System.Serializable] public class SpawnGenerator {
		
		public GenAttribute attSubWaveCount=new GenAttribute();
		public GenAttribute attTotalUnitCount=new GenAttribute();
		public GenAttribute attLifeGainOnCleared=new GenAttribute();
		public List<GenAttribute> attRscGainOnCleared=new List<GenAttribute>();
		public GenAttribute attPerkRscGainOnCleared=new GenAttribute();
		public GenAttribute attAbilityRscGainOnCleared=new GenAttribute();
		
		public float waveIntervalMin=5;
		public float waveIntervalMax=10;
		
		public bool useAllPath=false;	//when checked, all path will be used (given that there's enough subwave)
		public bool limitSubWaveCountToPath=true;	//when checked, the total subwave count will be limited to number of path available
		public bool similarSubWave=false;	//when checked, all subwave will be similar except they uses different path
		
		public List<GenItem> genItemList=new List<GenItem>();
		
		private bool init=false;
		public void Init(){
			if(Application.isPlaying && init) return;
			
			init=true;
			
			int count=RscManager.GetResourceCount();
			if(attRscGainOnCleared.Count<count){
				while(attRscGainOnCleared.Count<count) attRscGainOnCleared.Add(new GenAttribute(true));
			}
			else if(attRscGainOnCleared.Count>count){
				while(attRscGainOnCleared.Count>count) attRscGainOnCleared.RemoveAt(attRscGainOnCleared.Count-1);
			}
			
			for(int i=0; i<genItemList.Count; i++) genItemList[i].VerifyRsc(count);
		}
		
		
		
		
		
		public Wave Generate(int waveIdx){
			Init();
			
			/*	//example to hardcode specific wave
			if(waveIdx==9)	//wave-10
			{
				Wave bossWave=new Wave(waveIdx);
				
				SubWave subW=new SubWave();	//the boss prefab every 2s for 3 times
				subW.prefab=bossPrefab;
				subW.delay=0;
				subW.spacing=1;
				subW.spawnCount=3;
				
				bossWave.subWaveList.Add(subW);
				bossWave.timeToNextWave=10;
				
				return bossWave;
			}
			*/
			
			Wave wave=new Wave(waveIdx);
			
			if(genItemList.Count<=0){
				Debug.LogWarning("Error generating wave-"+waveIdx+", there's no unit information");
				//while(wave.rscGainOnCleared.Count<attRscGainOnCleared.Count) wave.rscGainOnCleared.Add(0);
				return wave;
			}
			
			int totalUnitCount=Mathf.Max(1, attTotalUnitCount.GetValueInt(waveIdx));
			int subWaveCount=Mathf.Min(totalUnitCount, attSubWaveCount.GetValueInt(waveIdx));
			
			//Debug.Log("Generate Wave-"+waveIdx+",     SubWave-"+subWaveCount+"   TotalUnit-"+totalUnitCount);
			
			List<int> avaiItemIdxList=new List<int>();
			List<float> itemOddsList=new List<float>();
			for(int i=0; i<genItemList.Count; i++){
				if(!genItemList[i].IsAvailable(waveIdx)) continue;
				avaiItemIdxList.Add(i);
				itemOddsList.Add(genItemList[i].GetOdds(waveIdx));
				//totalOdds+=itemOddsList[itemOddsList.Count-1];
			}
			
			if(avaiItemIdxList.Count<=0) {
				Debug.LogWarning("Error generating wave-"+waveIdx+", no unit is eligible");
				return wave;
			}
			
			wave.timeToNextWave=Random.Range(waveIntervalMin, waveIntervalMax);
			
			wave.lifeGainOnCleared=attLifeGainOnCleared.GetValueInt(waveIdx);
			for(int i=0; i<attRscGainOnCleared.Count; i++)
				wave.rscGainOnCleared.Add(attRscGainOnCleared[i].GetValueInt(waveIdx));
			
			wave.perkRscGainOnCleared=attPerkRscGainOnCleared.GetValueInt(waveIdx);
			wave.abilityRscGainOnCleared=attAbilityRscGainOnCleared.GetValueInt(waveIdx);
			
			wave.subWaveList=new List<SubWave>();
			
			List<Path> pathList=Path.GetAllStartingPath();
			int pathIdx=Random.Range(0, pathList.Count);
			List<float> pathOddsList=new List<float>();
			for(int i=0; i<pathList.Count; i++) pathOddsList.Add(1);
			
			if(limitSubWaveCountToPath) subWaveCount=Mathf.Min(subWaveCount, pathList.Count);
			
			//for distributing spawn count in subwave
			List<float> spawnCountOddsList=new List<float>();		float totalOdds=0;
			
			for(int i=0; i<subWaveCount; i++){
				SubWave subW=new SubWave();
				
				if(similarSubWave && i>0){
					SubWave srcSW=wave.subWaveList[0];
					
					subW.prefab=srcSW.prefab;
					subW.delay=srcSW.delay;
					subW.spacing=srcSW.spacing;
					subW.spawnCount=srcSW.spawnCount;
					
					subW.HP=srcSW.HP;
					subW.SH=srcSW.SH;
					subW.speed=srcSW.speed;
					
					subW.overrideRscGain=srcSW.overrideRscGain;
					subW.rscGain=srcSW.rscGain;
				}
				else{
					int rand=ChooseOption(itemOddsList);
					int itemIdx=avaiItemIdxList[rand];
				
					subW.prefab=genItemList[itemIdx].prefab;
					subW.delay=(i==0 ? 0 : Random.Range(0, 5f));
					subW.spacing=genItemList[itemIdx].GetInterval(waveIdx);
					
					if(subWaveCount==1) subW.spawnCount=totalUnitCount;
					else if(similarSubWave) subW.spawnCount=(int)Mathf.Round(totalUnitCount/subWaveCount);
					else{
						spawnCountOddsList.Add(itemOddsList[rand]);
						totalOdds+=itemOddsList[rand];
					}
					//subW.spawnCount=(int)Mathf.Max(1, Mathf.Round((itemOddsList[rand]/totalOdds)*totalUnitCount));
					
					subW.HP=genItemList[itemIdx].GetHP(waveIdx);
					subW.SH=genItemList[itemIdx].GetSH(waveIdx);
					subW.speed=genItemList[itemIdx].GetSpeed(waveIdx);
					
					subW.overrideRscGain=genItemList[itemIdx].enableRscOverride;
					if(genItemList[itemIdx].enableRscOverride) subW.rscGain=genItemList[itemIdx].GetRscGain(waveIdx);
				}
				
				if(useAllPath && subWaveCount>=pathList.Count){
					pathIdx+=1;	//cycle through all path
					if(pathIdx>=pathList.Count) pathIdx=0;
					subW.path=pathList[pathIdx];
				}
				else{
					int randPath=ChooseOption(pathOddsList);
					pathOddsList[randPath]*=0.25f;	//reduce the chance of the path being used again
					subW.path=pathList[randPath];
				}
				
				wave.subWaveList.Add(subW);
			}
			
			//distribute spawn count to each subwave
			if(subWaveCount>1 & !similarSubWave){
				for(int i=0; i<wave.subWaveList.Count; i++){
					wave.subWaveList[i].spawnCount=(int)Mathf.Max(1, Mathf.Round((spawnCountOddsList[i]/totalOdds)*totalUnitCount));
				}
			}
			
			return wave;
		}
		
		
		
		
		public static int ChooseOption(int count){ return Random.Range(0, count); }
		public static int ChooseOption(List<float> odds){
			float th=0;
			List<float> thList=new List<float>();
			for(int i=0; i<odds.Count; i++){
				th+=odds[i];
				thList.Add(th);
			}
			
			if(th==0) return -1;
			
			//DebugDisplayList(thList, "oddsList: ")
			
			float rand=Random.Range(0, thList[thList.Count-1]);
			for(int i=0; i<thList.Count; i++){
				if(rand<thList[i]) return i;
			}
			
			return 0;
		}
		public static void DebugDisplayList(List<float> list, string text=""){
			for(int i=0; i<list.Count; i++) text+=list[i]+"  ";
			Debug.Log(text);
		}
		
		
		#region editor code
		public void UpdateUnitList(){
			List<GenItem> newItemList=new List<GenItem>();
			List<UnitCreep> unitList=Creep_DB.GetList();
			
			for(int i=0; i<unitList.Count; i++){
				int idx=-1;
				for(int n=0; n<genItemList.Count; n++){
					if(genItemList[n].unitPrefabID==unitList[i].prefabID){ idx=n; break; }
				}
				
				if(idx<0){
					newItemList.Add(new GenItem(unitList[i]));
					newItemList[i].SetAllOverride(true);
				}
				else{
					newItemList.Add(genItemList[idx]);
				}
			}
			
			genItemList=newItemList;
		}
		#endregion
	}
	
	
	
	
	[System.Serializable] public class GenItem{
		public int unitPrefabID=0;
		public UnitCreep prefab;	//assign in runtime only
		
		public bool enabled=true;
		
		public int minWave=0;
		public int maxWave=-1;
		
		
		public GenAttribute attOdds=new GenAttribute();
		//public GenAttribute attCount=new GenAttribute();
		public GenAttribute attInterval=new GenAttribute();
		
		public bool enableHPOverride=true;
		public bool enableSHOverride=true;
		public bool enableSpeedOverride=true;
		public bool enableRscOverride=true;
		
		public GenAttribute attHP=new GenAttribute();
		public GenAttribute attSH=new GenAttribute();
		public GenAttribute attSpeed=new GenAttribute();
		public List<GenAttribute> attRscGain=new List<GenAttribute>();
		
		//~ public GenItem(int ID){ unitPrefabID=ID; }
		public GenItem(UnitCreep unit){
			prefab=unit; unitPrefabID=unit.prefabID; 
			VerifyRsc(RscManager.GetResourceCount());
		}
		
		public bool IsAvailable(int waveIdx){
			if(!enabled) return false;
			if(prefab==null) return false;
			if(waveIdx<minWave) return false;
			if(maxWave>0 && waveIdx>maxWave) return false;
			if(GetOdds(waveIdx)<=0) return false;
			return true;
		}
		
		public void VerifyRsc(int count){
			if(attRscGain.Count<count){
				while(attRscGain.Count<count) attRscGain.Add(new GenAttribute(true));
			}
			else if(attRscGain.Count>count){
				while(attRscGain.Count>count) attRscGain.RemoveAt(attRscGain.Count-1);
			}
		}
		
		public float GetOdds(int waveIdx){ return attOdds.GetValue(waveIdx); }
		//public int GetCount(int waveIdx){ return attCount.GetValueInt(waveIdx); }
		public float GetInterval(int waveIdx){ return attInterval.GetValue(waveIdx); }
		
		public float GetHP(int waveIdx){ return enableHPOverride ? attHP.GetValue(waveIdx) : -1; }
		public float GetSH(int waveIdx){ return enableSHOverride ? attSH.GetValue(waveIdx) : -1; }
		public float GetSpeed(int waveIdx){ return enableSpeedOverride ? attSpeed.GetValue(waveIdx) : -1; }
		
		public List<int> GetRscGain(int waveIdx){
			List<int> list=new List<int>();
			for(int i=0; i<attRscGain.Count; i++) list.Add(Mathf.Max(0, attRscGain[i].GetValueInt(waveIdx)));
			return list;
		}
		
		public void SetAllOverride(bool flag){
			enableHPOverride=flag; enableSHOverride=flag; enableSpeedOverride=flag; enableRscOverride=flag;	
		}
	}
	
	[System.Serializable] public class GenAttribute{
		public float startValue=1;
		public float incrementRate=1;
		
		[Range(-1f, 1f)] public float deviation=0;
		
		public float limitMin=-1;
		public float limitMax=-1;
		
		public GenAttribute(bool disabled=false){ if(disabled) Disable(); }
		
		public int GetValueInt(int waveIdx, bool debug=false){ return (int)Mathf.Round(GetValue(waveIdx)); }
		public float GetValue(int waveIdx, bool debug=false){ 
			float value=incrementRate*waveIdx+startValue;
			float multiplier=1+Random.Range(-deviation, deviation);
			
			value*=multiplier;
			
			if(limitMin<0 && limitMax>=0) value=Mathf.Min(limitMax, value);
			else if(limitMin>=0 && limitMax<0) value=Mathf.Max(limitMin, value);
			else if(limitMin>=0 && limitMax>=0) value=Mathf.Clamp(value, limitMin, limitMax);
			
			return value;
		}
		
		public void Disable(){ incrementRate=0; startValue=0; deviation=0; }
	}

}