using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{

	public class SpawnManager : MonoBehaviour {
		
		//public enum _SpawnLimit{ Finite, Infinite }
		//public _SpawnLimit spawnLimit;
		//public static bool IsEndlessMode(){ instance.spawnLimit==_SpawnLimit.Infinite; }
		public bool endlessMode=false;
		public static bool IsEndlessMode(){ return instance.endlessMode; }
		
		public enum _SpawnCDType{Spawned, Cleared, None}
		public _SpawnCDType spawnCDType;
		public bool skippable=true;
		
		public bool genWaveOnStart=false;
		
		public bool autoStart=false;
		public float startTimer=5;
		
		
		[Space(5)] [HideInInspector]
		public List<Path> pathList=new List<Path>();
		
		[Space(5)] public List<Wave> waveList=new List<Wave>();
		public static int GetTotalWaveCount(){ return instance.waveList.Count; }
	
		private int currentWaveIdx=-1;
		public static int GetCurrentWaveIndex(){ return instance!=null ? instance.currentWaveIdx : 0; }
		
		private float timeToNextWaveFull=0;
		private float timeToNextWave=0;
		public static float GetTimeToNextWaveRatio(){ return instance.timeToNextWave/instance.timeToNextWaveFull; }
		public static float GetTimeToNextWave(){ return instance.timeToNextWave; }
		private void SetTimeToNextWave(float value){ timeToNextWaveFull=value;		timeToNextWave=value; }
		
		public SpawnGenerator generator;
		[ContextMenu ("Update Generator Unit List")] 
		public void UpdateGeneratorUnitList(){ generator.UpdateUnitList(); }
		[ContextMenu ("Generate Wave")]
		public void GenerateWave(){
			Debug.Log("GenerateWave_________________");
			for(int i=0; i<waveList.Count; i++) waveList[i]=generator.Generate(i);
		}
		
		
		//all the creep active in the map, for target detection
		[Space(5)] public List<Unit> activeUnitList=new List<Unit>();
		public static List<Unit> GetActiveUnitList(){ return instance.activeUnitList; }
		public static void AddActiveUnit(Unit unit){ instance.activeUnitList.Add(unit); }
		public static void RemoveActiveUnit(Unit unit){ instance.activeUnitList.Remove(unit); }
		
		private int totalCreepCount=0;
		
		
		public GenAttribute overrideHP=new GenAttribute();
		public GenAttribute overrideSH=new GenAttribute();
		public GenAttribute overrideSpd=new GenAttribute();
		
		public float overrideWaveSpacing=-1;
		
		
		private static SpawnManager instance;
		
		void Awake(){
			if(instance!=null && instance!=this){
				Debug.Log("Error, there are multiple instance of SpawnManager in the scene");
				return;
			}
			
			instance=this;
			
			activeUnitList=new List<Unit>();
		}
		
		void Start(){
			if(IsEndlessMode()){
				waveList=new List<Wave>();
				for(int i=0; i<2; i++) waveList.Add(generator.Generate(i));
			}
			else{
				if(genWaveOnStart){
					List<Wave> newList=new List<Wave>();
					for(int i=0; i<waveList.Count; i++) newList.Add(generator.Generate(i));
					waveList=newList;
				}
			}
			
			for(int i=0; i<waveList.Count; i++) waveList[i].activeUnitCount=0;
			
			if(autoStart) StartCoroutine(StartSpawn(startTimer));
		}
		
		public static void Init(){
			if(instance==null) instance=(SpawnManager) FindObjectOfType(typeof(SpawnManager));
			instance.pathList=Path.GetAllStartingPath();
			for(int i=0; i<instance.waveList.Count; i++) instance.waveList[i].waveIdx=i;
		}
		
		
		public static bool IsBetweenWaves(){
			return instance.spawnCDType==_SpawnCDType.None & !instance.spawning & instance.activeUnitList.Count==0 ;
		}
		
		
		public static List<Unit> GetUnitsWithinRange(Unit srcUnit, float range, UnitTower._TargetGroup tgtGroup=UnitTower._TargetGroup.All){
			return GetUnitsWithinRange(srcUnit.GetPos(), range, tgtGroup);
		}
		public static List<Unit> GetUnitsWithinRange(Vector3 pos, float range, UnitTower._TargetGroup tgtGroup=UnitTower._TargetGroup.All){
			List<Unit> unitList=GetActiveUnitList();	List<Unit> tgtList=new List<Unit>();
			for(int i=0; i<unitList.Count; i++){
				if(tgtGroup==UnitTower._TargetGroup.Ground && unitList[i].IsFlying()) continue;
				if(tgtGroup==UnitTower._TargetGroup.Air && !unitList[i].IsFlying()) continue;
				if(Vector3.Distance(pos, unitList[i].GetPos())<range+unitList[i].GetRadius()) tgtList.Add(unitList[i]);
			}
			return tgtList;
		}
		
		
		private bool readyToSpawn=true;
		//~ void OnGUI(){
			//~ if(readyToSpawn && GUI.Button(new Rect(20, 20, 100, 25), "Spawn")) Spawn();
			//~ if(timeToNextWave>0) GUI.Label(new Rect(130, 20, 200, 25), "Next Wave - "+timeToNextWave.ToString("f1"));
		//~ }
		
		private void ReadyToSpawn(){
			readyToSpawn=true;
			TDTK.OnEnableSpawn();
		}
		private void StartSpawnCountDown(float duration){
			SetTimeToNextWave(duration);
			TDTK.OnSpawnCountDown();
		}
		
		
		IEnumerator StartSpawn(float duration){
			yield return new WaitForSeconds(duration);
			if(!GameControl.HasGameStarted()) _Spawn();
		}
		
		//universal function call to spawn next available wave
		//check for various condition, stop the spawn if the required condition is not met
		public static void Spawn(){ instance._Spawn(); }	
		public void _Spawn(){
			if(GameControl.IsGameOver()) return;
			
			if(!readyToSpawn) return;
			
			if(!GameControl.HasGameStarted()) SpawnNextWave();	//first spawn
			else{
				if(spawnCDType==_SpawnCDType.None){
					if(activeUnitList.Count==0) SpawnNextWave();
				}
				else if(skippable) SpawnNextWave();
			}
		}
		
		private void FixedUpdate(){	//count down to next spawn, if applicable
			if(spawnCDType==_SpawnCDType.None) return;
			if(GameControl.IsGameOver()) return;
			if(timeToNextWave<=0) return;
			
			timeToNextWave-=Time.fixedDeltaTime;
			if(timeToNextWave<=0) SpawnNextWave();
		}
		
		private void SpawnNextWave(){	//actual function to spawn next wave
			if(!GameControl.HasGameStarted()) GameControl.StartGame();
			
			if(spawning) return;
			
			readyToSpawn=false;
			SetTimeToNextWave(-1);
			currentWaveIdx+=1;
			
			if(!IsEndlessMode()) StartCoroutine(SpawnWave(waveList[currentWaveIdx]));
			else{
				waveList.Add(generator.Generate(currentWaveIdx+2));
				int waveIdx=GetListIndexFromWaveIndex(currentWaveIdx);
				StartCoroutine(SpawnWave(waveList[waveIdx]));
			}
			
			TDTK.OnNewWave(currentWaveIdx+1);
			AudioManager.OnNewWave();
		}
		
		
		
		private bool spawning=false;
		private int subSpawning=0;
		IEnumerator SpawnWave(Wave wave){
			spawning=true;
			
			//if(onSpawningE!=null) onSpawningE();
			
			Debug.Log("Spawning wave-"+wave.waveIdx);
			
			subSpawning=wave.subWaveList.Count;
			
			for(int i=0; i<subSpawning; i++){
				if(wave.subWaveList[i].prefab==null){
					subSpawning-=1;
					Debug.LogWarning("Prefab for sub-wave is unassigned");
					continue;
				}
				//StartCoroutine(SpawnSubWave(wave.subWaveList[i], wave.waveIdx));
				StartCoroutine(SpawnSubWave(wave, i));
			}
			
			while(subSpawning>0) yield return null;
			
			wave.spawned=true;
			
			//if(spawnCDType==_SpawnCDType.Cleared && !OnFinalWave() && skippable) ReadyToSpawn();
			if(spawnCDType==_SpawnCDType.Spawned && !OnFinalWave()){
				//timeToNextWave=wave.timeToNextWave;
				StartSpawnCountDown(wave.timeToNextWave);
				if(skippable) ReadyToSpawn();
			}
			
			Debug.Log("Done spawning wave-"+wave.waveIdx);
			
			spawning=false;
		}
		//~ IEnumerator SpawnSubWave(SubWave subWave, int waveIdx){
		IEnumerator SpawnSubWave(Wave wave, int subWaveIdx){
			SubWave subWave=wave.subWaveList[subWaveIdx];
			
			yield return new WaitForSeconds(subWave.delay);
			
			if(subWave.path==null) subWave.path=pathList[Random.Range(0, pathList.Count)];
			
			for(int i=0; i<subWave.spawnCount; i++){
				UnitCreep creepInstance=SpawnUnit(subWave.prefab, wave.waveIdx, subWave.path, subWave);//, subWave.HP, subWave.speed);
				AddActiveUnit(creepInstance);
				wave.activeUnitCount+=1;
				if(i<subWave.spawnCount-1) yield return new WaitForSeconds(subWave.spacing);
			}
			
			subSpawning-=1;
		}
		public UnitCreep SpawnUnit(UnitCreep creep, int waveIdx, Path path, SubWave subWave=null){
			//~ GameObject unitObj=(GameObject)Instantiate(creep.gameObject, path.GetSpawnPoint(), Quaternion.identity);
			GameObject unitObj=ObjectPoolManager.Spawn(creep.gameObject, path.GetSpawnPoint(), Quaternion.identity);
			UnitCreep unitInstance=unitObj.GetComponent<UnitCreep>();
			
			unitInstance.ResetHP();
			unitInstance.ResetSH();
			unitInstance.ResetSpeed();
			unitInstance.ResetRscGainOnDestroy();
			
			if(subWave!=null){
				if(IsOverrideMultiplier()){
					unitInstance.SetHP(subWave.HP>0 ? subWave.HP * unitInstance.statsList[0].hp  : -1);
					unitInstance.SetSH(subWave.SH>0 ? subWave.SH * unitInstance.statsList[0].sh : -1);
					unitInstance.SetSpeed(subWave.speed>0 ? subWave.speed * unitInstance.statsList[0].speed : -1);
					
					if(subWave.overrideRscGain){
						List<int> newList=new List<int>();
						int count=Mathf.Min(unitInstance.rscGainOnDestroyed.Count, subWave.rscGain.Count);
						for(int i=0; i<count; i++)
							newList.Add((int)Mathf.Round(unitInstance.rscGainOnDestroyed[i]*subWave.rscGain[i]));
						unitInstance.SetRscGainOnDestroy(newList);
					}
				}
				else{
					unitInstance.SetHP(subWave.HP>0 ? subWave.HP : -1);
					unitInstance.SetSH(subWave.SH>0 ? subWave.SH : -1);
					unitInstance.SetSpeed(subWave.speed>0 ? subWave.speed : -1);
					unitInstance.SetRscGainOnDestroy(subWave.overrideRscGain ? subWave.rscGain : null);
				}
				//if(subWave.HP>0) unitInstance.SetHP(subWave.HP);
				//if(subWave.SH>0) unitInstance.SetSH(subWave.SH);
				//if(subWave.speed>0) unitInstance.SetSpeed(subWave.speed);
				//if(subWave.overrideRscGain) unitInstance.rscGainOnDestroyed=subWave.rscGain;
				
				
			}
			else{	//reset the unit value to default value
				unitInstance.SetHP();
				unitInstance.SetSH();
				unitInstance.SetSpeed();
				unitInstance.SetRscGainOnDestroy(null);
			}
			
			unitInstance.Init(waveIdx, path);
			
			totalCreepCount+=1;	unitInstance.instanceID=totalCreepCount;
			
			return unitInstance;
		}
		
		public static void SubUnitSpawned(UnitCreep creep){//, int waveIdx){
			AddActiveUnit(creep);
			instance.totalCreepCount+=1;	creep.instanceID=instance.totalCreepCount;
			
			int waveIdx=instance.GetListIndexFromWaveIndex(creep.waveIdx);
			instance.waveList[waveIdx].activeUnitCount+=1;
			if(instance.waveList[waveIdx].cleared) Debug.LogWarning("Spawning subwave for cleared wave?");
		}
		
		
		private int GetListIndexFromWaveIndex(int waveIdx){	//used in endless mode only, since waveIdx is not equal to idx in the list
			if(!IsEndlessMode()) return waveIdx;
			
			int listIdx=0;
			for(int i=0; i<waveList.Count; i++){
				if(waveList[i].waveIdx==waveIdx){ listIdx=i; break; } 
			}
			return listIdx;
		}
		
		
		public static void ClearCleanWaveFlag(int waveIdx){	//for path looping creep that reach destination to set the wave 'dirty'
			waveIdx=instance.GetListIndexFromWaveIndex(waveIdx);
			instance.waveList[waveIdx].clean=false;
		}
		
		public static void CreepDestroyed(UnitCreep creep, bool reachDest=false){ instance._CreepDestroyed(creep, reachDest); }
		public void _CreepDestroyed(UnitCreep creep, bool reachDest=false){
			RemoveActiveUnit(creep);
			
			int waveIdx=GetListIndexFromWaveIndex(creep.waveIdx);
			
			if(reachDest) waveList[waveIdx].clean=false;
			
			waveList[waveIdx].activeUnitCount-=1;
			if(waveList[waveIdx].CheckClear()){	//wave cleared
				if(waveList[waveIdx].IsCleared()){
					Debug.LogWarning("Error? Wave already cleared?");
					return;
				}
				else{
					AudioManager.OnWaveCleared();
					waveList[waveIdx].Cleared();	//cleared=true;
					Debug.Log("Wave-"+creep.waveIdx+" cleared");
				}
				
				if(IsEndlessMode() || !OnFinalWave()){
					if(spawnCDType==_SpawnCDType.None){
						ReadyToSpawn();
					}
					if(spawnCDType==_SpawnCDType.Cleared){
						if(skippable) ReadyToSpawn();
						StartSpawnCountDown(waveList[waveIdx].timeToNextWave);
					}
				}
				else{
					bool allCleared=true;
					for(int i=0; i<waveList.Count; i++) allCleared&=waveList[i].cleared;
					if(allCleared) GameControl.EndGame();
				}
			}
		}
		
		
		public static bool OnFinalWave(){ return instance.currentWaveIdx>=instance.waveList.Count-1; }
		
		
		public SubWave._OverrideType overrideType=SubWave._OverrideType.Multiplier;
		public bool IsOverrideMultiplier(){ return overrideType==SubWave._OverrideType.Multiplier; }
		
	}
	
	
	
	[System.Serializable] public class Wave{
		public int waveIdx=-1;
		public List<SubWave> subWaveList=new List<SubWave>{ new SubWave() };
		public float timeToNextWave=5;
		
		public int activeUnitCount=0;	//only used in runtime
		
		public bool spawned=false;		//flag indicate weather all unit in the wave have been spawned, only used in runtime
		public bool cleared=false; 		//flag indicate weather the wave has been cleared, only used in runtime
		public bool clean=true;			//flag indicate weather any creep from that wave reach the destination
		
		public List<float> rscGainOnCleared=new List<float>();
		public int perkRscGainOnCleared=0;
		public int abilityRscGainOnCleared=0;
		public int lifeGainOnCleared=0;
		
		public Wave(){  }
		public Wave(int idx){ waveIdx=idx; }
		
		public Wave Clone(){ return ObjectCopier.Clone(this); }
		
		public float GetSpawnDuration(){
			float duration=0;
			for(int i=0; i<subWaveList.Count; i++) duration+=subWaveList[i].GetSpawnDuration();
			return duration;
		}
		
		public bool CheckClear(){ return spawned & activeUnitCount<=0; }
		
		public bool IsCleared(){ return cleared; }
		public void Cleared(){
			cleared=true;
			
			GameControl.GainLife(PerkManager.GetLifeGainOnWaveCleared(lifeGainOnCleared));
			RscManager.GainRsc(rscGainOnCleared, RscManager._GainType.WaveCleared);
			PerkManager.GainRsc(perkRscGainOnCleared);
			AbilityManager.GainRsc(PerkManager.GetAbRscGainOnWaveCleared(abilityRscGainOnCleared));
			
			PerkManager.WaveCleared(waveIdx+1);
		}
	}

	[System.Serializable] public class SubWave{
		public UnitCreep prefab;
		
		public float delay=0;
		public int spawnCount=1;
		public float spacing=1f;
		
		public Path path;
		
		public enum _OverrideType{ Override, Multiplier }
		//public static _OverrideType overrideType=_OverrideType.Multiplier;
		//public bool IsOverrideMultiplier(){ return overrideType==_OverrideType.Multiplier; }
		
		//for overridding default value
		public float HP=-1;
		public float SH=-1;
		public float speed=-1;
		
		public bool overrideRscGain=false;
		public List<int> rscGain=new List<int>();
		
		public SubWave Clone(){ return ObjectCopier.Clone(this); }
		
		public float GetSpawnDuration(){ return delay+spawnCount*spacing; }
	}
	
}