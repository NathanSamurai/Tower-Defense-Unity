using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TDTK{

	public class UnitCreep : Unit {

		public override _UnitType GetUnitType(){ return _UnitType.Creep; }
		public override bool IsCreep(){ return true; }
		public override UnitCreep GetCreep(){ return this; }
		
		
		public enum _CreepType{ Default, Turret, AOE, Support, Spawner, }
		[Header("Creep Setting")] public _CreepType creepType;
		
		//~ public override bool IsTurret(){ return creepType==_CreepType.Turret; }
		//~ public override bool IsAOE(){ return creepType==_CreepType.AOE; }
		//~ public override bool IsSupport(){ return creepType==_CreepType.Support; }
		//~ public override bool IsSpawner(){ return creepType==_CreepType.Spawner; }
		
		public bool isTurret;
		public bool isAOE;
		public bool isSupport;
		public bool isSpawner;
		
		public override bool IsTurret(){ return isTurret; }
		public override bool IsAOE(){ return isAOE; }
		public override bool IsSupport(){ return isSupport; }
		public override bool IsSpawner(){ return isSpawner; }
		//public bool IsDefault(){ return !isTurret & !isAOE & !isSupport & !isSpawner; }
		
		
		public override bool CreepIsOnAttackCD(){ return !stopToAttack ? false : stopToAttackCD>0; }
		
		
		[Header("Path")]
		public int waveIdx;
		
		public Path path;
		
		public int wpIdx=0;
		public int subWpIdx=0;
		
		[Header("Move Setting")]
		public bool flying=false;
		public override bool IsFlying(){ return flying; }
		
		public bool faceTravelingDir=true;
		public bool snapToDir=true;
		public float rotateSpeed=12;
		
		public enum _PathOffsetMode{ None, Simple, AlignToPath, }
		public _PathOffsetMode pathOffsetMode=_PathOffsetMode.AlignToPath;
		private float pathOffset=0;
		private Vector3 pathOffsetV;
		
		[Space(10)]
		public bool stopToAttack=false;
		public int attackLimitPerStop=3;
		private int attackLimitPerStopCounter=0;
		public float stopToAttackCooldown=3;
		private float stopToAttackCD=0;
		private float lastAttackTime=0;
		
		public override void CreepAttackCount(){
			lastAttackTime=Time.time;
			attackLimitPerStopCounter+=1;
			if(attackLimitPerStopCounter>=attackLimitPerStop && attackLimitPerStop>0){
				ClearTarget();
				attackLimitPerStopCounter=0;
				stopToAttackCD=stopToAttackCooldown;
			}
		}
		
		
		[Header("Gain/Lost On Destroyed/Destination")]
		[Range(0f, 1f)] public float lifeGainedOnDestroyedChance=0;
		public int lifeGainedOnDestroyed=0;
		public int expGainOnDestroyed=0;
		public List<int> rscGainOnDestroyed=new List<int>();
		
		public int lifeLostOnDestination=1;
		
		
		
		[Header("Visual and audio")]
		public VisualObject effectSpawn=new VisualObject();
		public VisualObject effectDestroyed=new VisualObject();
		public VisualObject effectDestination=new VisualObject();
		
		[Space(5)] 
		public AudioClip soundSpawn;
		public AudioClip soundDestroyed;
		public AudioClip soundDestination;

		[SerializeField] AK.Wwise.Event CreepSpawn, CreepDestroyed, CreepDestination;
		
		
		//for override default value, called from SpawnManager, the cached value are the default value for prefab, 
		//so that the prefab value is used instead of any override value when the instance is recycled by ObjectPoolManager
		private float cachedHP=-1;
		private float cachedSH=-1;
		private float cachedSpd=-1;
		private List<int> cachedRscOnDestroy=new List<int>();
		
		public void ResetHP(){ if(cachedHP>=0) statsList[level].hp=cachedHP; }
		public void ResetSH(){ if(cachedSH>=0) statsList[level].sh=cachedSH; }
		public void ResetSpeed(){ if(cachedSpd>=0) statsList[level].speed=cachedSpd; }
		public void ResetRscGainOnDestroy(){ if(cachedRscOnDestroy.Count>0) rscGainOnDestroyed=cachedRscOnDestroy; }
		
		public void SetHP(float value=-1){ 
			if(cachedHP<0) cachedHP=statsList[level].hp;
			statsList[level].hp=value>0 ? value : cachedHP; 
		}
		public void SetSH(float value=-1){ 
			if(cachedSH<0) cachedSH=statsList[level].sh;
			statsList[level].sh=value>0 ? value : cachedSH; 
		}
		public void SetSpeed(float value=-1){ 
			if(cachedSpd<0) cachedSpd=statsList[level].speed;
			statsList[level].speed=value>0 ? value : cachedSpd; 
		}
		public void SetRscGainOnDestroy(List<int> list=null){ 
			if(cachedRscOnDestroy.Count<=0) cachedRscOnDestroy=rscGainOnDestroyed;
			rscGainOnDestroyed=list!=null ? list : cachedRscOnDestroy;
		}
		
		
		
		public override void Awake(){
			base.Awake();
			
			RscManager.MatchRscList(rscGainOnDestroyed, 0);
		}
		
		
		void SpawnSubCreep(UnitCreep prefab, float orHP=-1, float orSH=-1, float orSpd=-1, int orRscExp=-1, List<int> orRsc=null){
			Vector3 dir=(subPath[subWpIdx]-GetPos()).normalized;
			Vector3 spawnPos=GetPos()+dir*Random.Range(-.5f, .5f);
			
			//~ GameObject unitObj=(GameObject)Instantiate(prefab.gameObject, spawnPos, thisT.rotation);
			GameObject unitObj=ObjectPoolManager.Spawn(prefab.gameObject, spawnPos, thisT.rotation);
			UnitCreep unitInstance=unitObj.GetComponent<UnitCreep>();
			
				unitInstance.SetHP(orHP);
				unitInstance.SetSH(orSH);
				unitInstance.SetSpeed(orSpd);
				if(orRsc!=null) unitInstance.rscGainOnDestroyed=orRsc;
			
				//~ if(orHP>0) unitInstance.SetHP(orHP);
				//~ if(orSH>0) unitInstance.SetSH(orSH);
				//~ if(orSpd>0) unitInstance.SetSpeed(orSpd);
				//~ if(orRsc!=null) unitInstance.rscGainOnDestroyed=orRsc;
			
			unitInstance.Init(waveIdx, path, wpIdx, subWpIdx, false, reverse, new List<Path>( prevPathList ), subPath);
			
			SpawnManager.SubUnitSpawned(unitInstance);
			//SpawnManager.SubUnitSpawned(unitInstance, waveIdx);
		}
		
		
		public void Init(int waveIndex, Path p, int wpIndex=-1, int subWpIndex=-1, bool resetPos=true, bool rr=false, List<Path> prevP=null, List<Vector3> sPath=null){
			waveIdx=waveIndex;
			wpIdx=wpIndex>=0 ? wpIndex : 0;
			subWpIdx=subWpIndex>=0 ? subWpIndex : 0;
			reverse=rr;
			prevPathList=prevP!=null ? prevP : new List<Path>();
			
			path=p;
			path.OnCreepEnter(this);
			
			if(sPath==null) subPath=path.GetWP(wpIdx, EnableBypass());
			else subPath=sPath;
			
			if(pathOffsetMode==_PathOffsetMode.None){
				pathOffsetV=Vector3.zero;	pathOffset=0;
			}
			else if(pathOffsetMode==_PathOffsetMode.Simple){
				float x=Random.Range(-path.dynamicOffset, path.dynamicOffset);
				float z=Random.Range(-path.dynamicOffset, path.dynamicOffset);
				pathOffsetV=new Vector3(x, 0, z);
			}
			else if(pathOffsetMode==_PathOffsetMode.AlignToPath){
				pathOffset=Random.Range(-path.dynamicOffset, path.dynamicOffset);
				lastTargetPos=subPath[subWpIdx];
				ResetPathOffset();
			}
			
		
			if(resetPos){
				thisT.position=path.GetWP(0)[0]+pathOffsetV;
				if(faceTravelingDir) thisT.rotation=Quaternion.LookRotation(path.GetWP(1)[0]+pathOffsetV-GetPos());
			}
			
			
			lastTargetPos=thisT.position;
			
			hp=GetFullHP();
			sh=GetFullSH();
			
			cooldown=GetCooldown();
			cooldownSpawner=cooldown_Spawner;
			
			effectSpawn.Spawn(GetPos(), Quaternion.identity);
			AudioManager.PlaySound(soundSpawn);
			AudioManager.PlaySoundW(CreepSpawn, gameObject);
			
			AnimReset();
			AnimPlaySpawn();
			
			//UIHPOverlay.AddUnit(this);
			TDTK.OnNewUnit(this);
		}
		
		
		public override void Update(){
			base.Update();
			
			if(faceTravelingDir && targetPos-GetPos()!=Vector3.zero){
				if(snapToDir) thisT.rotation=Quaternion.LookRotation(targetPos-GetPos());
				else{
					Quaternion wantedRot=Quaternion.LookRotation(targetPos-GetPos());
					thisT.rotation=Quaternion.Lerp(thisT.rotation, wantedRot, rotateSpeed*Time.deltaTime);
				}
			}
		}
		
		
		public override void FixedUpdate(){
			base.FixedUpdate();
			
			if(IsDestroyed()) return;
			
			if(!IsStunned()) Move(Time.fixedDeltaTime);
			
			lastAttackTime-=Time.fixedDeltaTime;
			stopToAttackCD-=Time.fixedDeltaTime;
			
			if(IsDestroyed()) return;
			if(IsStunned()) return;
			
			if(IsTurret()){	//this is just to control the creep movement and aim, the actual attack call in in Unit.cs
				if(stopToAttack){
					if((GetTarget()!=null && !CreepIsOnAttackCD()) || Time.time-lastAttackTime<0.5f) speedMultiplier=0; 
					else{
						speedMultiplier=1;
						if(GetTarget()==null && !resetingAim) StartCoroutine(ResetAim());
					}
				}
				else{
					if(GetTarget()==null && !resetingAim) StartCoroutine(ResetAim()); 
				}
			}
			
			CreepFunction();
		}
		
		
		//fixed update for resource, aoe and mine, check '#region support tower' for support
		private void CreepFunction(){
			if(!GameControl.HasGameStarted()) return;
			
			//if(cooldown>0) return;
			
			if(IsAOE() && cooldownAOE<=0){
				List<Unit> tgtList=TowerManager.GetUnitsWithinRange(this, GetAttackRange_AOE());
				if(tgtList.Count>0){
					FireMiscEffect(shootEffObj_AOE);
					cooldownAOE=GetCooldown_AOE();
					for(int i=0; i<tgtList.Count; i++) tgtList[i].ApplyAttack(new AttackInfo(this, tgtList[i], 1));
				}
				else cooldownAOE=0.1f;
			}
			
			if(IsSupport()){
				if(cooldownSupport>0) cooldownSupport-=Time.fixedDeltaTime;
				else if(cooldownSupport<=0){
					float range=GetAttackRange_Support();
					for(int i=0; i<supportTgtList.Count; i++){
						if(Vector3.Distance(GetPos(), supportTgtList[i].GetPos())<=range+0.25f) continue;
						ClearBuffOnTarget(supportTgtList[i]);
						supportTgtList.RemoveAt(i); i-=1;
					}
					
					List<Unit> tgtList=SpawnManager.GetUnitsWithinRange(this, range);
					for(int i=0; i<tgtList.Count; i++){
						if(tgtList[i]==GetUnit() || tgtList[i].IsSupport()) continue;
						if(supportTgtList.Contains(tgtList[i].GetCreep())) continue;
						
						List<Effect> list=GetEffectOnHit_Support();
						for(int n=0; n<list.Count; n++){
							list[n].fromSupport=true;
							list[n].duration=Mathf.Infinity;
							tgtList[i].ApplyEffect(list[n]);
						}
						
						//Effect effect=GetEffectOnHit();//.Clone();
						//effect.duration=Mathf.Infinity;
						//tgtList[i].ApplyEffect(effect);
						
						tgtList[i].GetCreep().supportSrcList.Add(this);
						supportTgtList.Add(tgtList[i].GetCreep());
					}
					
					cooldownSupport=0.1f;
				}
			}
			
			if(IsSpawner() && spawnerCount>0 && spawnerPrefab!=null){
				if(cooldownSpawner>0) cooldownSpawner-=Time.fixedDeltaTime;
				else if(cooldownSpawner<=0){
					float oHP=spawnerOverride.GetHP(this);
					float oSH=spawnerOverride.GetSH(this);
					float oSpd=spawnerOverride.GetSpd(this);
					int oExp=spawnerOverride.GetExp(this);
					List<int> oRsc=spawnerOverride.GetRsc(this, rscGainOnDestroyed);
					
					for(int i=0; i<spawnerCount; i++) SpawnSubCreep(spawnerPrefab, oHP, oSH, oSpd, oExp, oRsc);
					
					cooldownSpawner=cooldown_Spawner;
				}
			}
		}
		
		private float cooldownSupport;
		private float cooldownSpawner;
		
		
		#region support creep
		[Space(10)]
		public List<UnitCreep> supportTgtList=new List<UnitCreep>();	//the towers being buff by this support tower
		public List<UnitCreep> supportSrcList=new List<UnitCreep>();	//the towers being buff by this support tower
		
		public void SupportClearAllTarget(){
			for(int i=0; i<supportTgtList.Count; i++){
				ClearBuffOnTarget(supportTgtList[i]);
				supportTgtList.RemoveAt(i); i-=1;
			}
		}
		public void ClearBuffOnTarget(UnitCreep tgtCreep){
			tgtCreep.supportSrcList.Remove(this);
			
			//bool hasSimilarBuff=false;
			//for(int n=0; n<tgtCreep.supportSrcList.Count; n++){
			//	if(tgtCreep.supportSrcList[n].prefabID==prefabID){ hasSimilarBuff=true; break; }
			//}
			
			//if(!hasSimilarBuff){
				for(int n=0; n<tgtCreep.allEffectList.Count; n++){
					if(!tgtCreep.allEffectList[n].FromCreep()) continue;
					if(tgtCreep.allEffectList[n].srcPrefabID!=prefabID) continue;
					
					if(tgtCreep.allEffectList[n].stack>1){
						tgtCreep.allEffectList[n].stack-=1;
						continue;
					}
					
					tgtCreep.allEffectList[n].durationRemain=0;
					//break;
				}
			//}
		}
		#endregion
		
		
		
		
		private List<Vector3> altPath=new List<Vector3>();
		public void SetAlternatePath(List<Vector3> aPath){ altPath=aPath; }
		public void ForceAltPath(){
			if(flying && AStar.EnableFlyingBypass()) return;
			if(!path.GetWpSec(wpIdx).isPlatform) return;
			
			if(altPath.Count<=0) altPath=path.GetWpSec(wpIdx).GetPathForUnit(this, !reverse);
			if(altPath.Count<=0) return;
			
			subPath=new List<Vector3>( altPath ); 
			
			subWpIdx=subWpIdx>1 ? 1 : 0;
			if(subWpIdx>=1 && subPath.Count>1){
				if(Vector3.Distance(GetPos(), subPath[1])>TowerManager.GetGridSize()) subWpIdx=0;
			}
			
			altPath=new List<Vector3>();
		}
		
		
		public void CheckReverse(){
			
		}
		
		
		[Space(10)]
		public bool reverse=false;
		private List<Vector3> subPath=new List<Vector3>();
		private float currentSpeed=0;
		private void Move(float deltaT){
			if(path==null) return;
			
			if(!EnableBypass()){
				//if(!reverse && !path.hasValidDestination)	Reverse();
				if(reverse && path.hasValidDestination) ClearReverse(); 
			}
			
			//if(path.GetWP(wpIdx).Count>0) subPath=path.GetWP(wpIdx);
			//subWpIdx=Mathf.Min(subWpIdx, subPath.Count-1);
			
			if(subPath.Count==0){ Debug.LogWarning("no subpath?"); return; }
			if(subWpIdx>=subPath.Count){ Debug.LogWarning("subWpIdx exceed subpath length?"); subWpIdx=subPath.Count-1; return; }
			
			//subPath=path.GetWP(wpIdx, EnableBypass());	//enable this to get the creep to update wp every frame (for moving wp), doesnt work for platform
			targetPos=subPath[subWpIdx]+pathOffsetV;
			
			float dist=Vector3.Distance(targetPos, thisT.position);
			
			currentSpeed=GetSpeed() * deltaT;
			Vector3 dir=(targetPos-thisT.position).normalized;
			thisT.Translate(dir*currentSpeed, Space.World);
			
			if(dist<currentSpeed*2f) NextWaypoint();
			
			//old code, might canuse jerky movement
			//if(dist<0.05f){
			//	NextWaypoint();
			//}
			//else{
			//	currentSpeed=GetSpeed();
			//	Vector3 dir=(targetPos-thisT.position).normalized;
			//	thisT.Translate(dir*Mathf.Min(currentSpeed*deltaT, dist), Space.World);
			//}
			
			AnimPlayMove(currentSpeed);
		}
		
		public void NextWaypoint(){
			//Debug.Log("Reach   "+Time.time+"      "+targetPos+"    subWpIdx-"+subWpIdx+"    subPath.Count-"+subPath.Count);
			
			if(subPath.Count>subWpIdx) lastTargetPos=subPath[subWpIdx];
			
			if(!reverse){
				subWpIdx+=1;
				if(subWpIdx>=subPath.Count){
					subWpIdx=0;	wpIdx+=1;
					if(wpIdx>=path.GetWPCount()){
						wpIdx=0;
						
						if(path.IsEnd()){
							ReachDestination();
							return;
						}
						else{
							path.OnCreepExit(this);
							prevPathList.Add(path);
							if(EnableBypass()) path=path.GetNextShortestPathFlying();
							else path=path.GetNextShortestPath();
							path.OnCreepEnter(this);
							
							//if(!path.hasValidDestination) Reverse();
						}
					}
					
					if(path.GetWP(wpIdx).Count>0){
						subPath=path.GetWP(wpIdx, EnableBypass());
						
						if(Vector3.Distance(lastTargetPos, subPath[0])<0.05f) subPath.RemoveAt(0);
						if(subPath.Count==0){
							NextWaypoint();
							return;
						}
					}
				}
			}
			else{
				MoveReverseReachWaypoint();
			}
			
			ResetPathOffset();
		}
		
		public void MoveReverseReachWaypoint(){
			subWpIdx-=1;
			if(subWpIdx<0){
				wpIdx-=1; 
				
				bool resetSubPath=true;
				if(wpIdx<0){
					path.OnCreepExit(this);
					path=prevPathList[prevPathList.Count-1];	prevPathList.RemoveAt(prevPathList.Count-1);
					
					wpIdx=path.GetWPCount()-1;
					path.OnCreepEnter(this);
					
					if(path.HasBranchingPlatformEnd()){
						resetSubPath=false;
						subPath=path.GetWpSec(wpIdx).GetPathForUnit(this, false);
						subPath.Reverse();
						subWpIdx=Mathf.Max(0, subPath.Count-1);
					}
				}
				
				if(resetSubPath){
					subPath=path.GetWP(wpIdx, EnableBypass());
					subWpIdx=Mathf.Max(0, subPath.Count-1);
				}
			}
		}
		
		
		public void ClearReverse(){
			reverse=false;
			if(path.HasBranchingPlatformEnd()){
				subPath=path.GetWpSec(wpIdx).GetPathForUnit(this);
				subWpIdx=0;
			}
			ResetPathOffset();
		}
		public void Reverse(){
			reverse=true;
			lastTargetPos=subPath[subWpIdx];	//need to get the path offset correct, in case the the creep immediately reverse back to a valid path
			pathOffset=-pathOffset;
			MoveReverseReachWaypoint();
		}
		
		private void ResetPathOffset(){
			if(pathOffsetMode==_PathOffsetMode.AlignToPath) pathOffsetV=GetPathOffset();
		}
		
		
		private Vector3 GetPathOffset(){
			//if(!Path.alignOffsetToPathDir) return pathOffsetV;
			
			lastPathOffsetV=pathOffsetV;
			
			Vector3 lastP=GetLastTargetPos();
			Vector3 thisP=subPath[subWpIdx];
			Vector3 nextP=Vector3.zero;
			
			if(!reverse){
				if(subPath.Count>subWpIdx+1) nextP=subPath[subWpIdx+1];
				else{
					if(path.GetWPCount()>wpIdx+1){
						nextP=path.GetWP(wpIdx+1, EnableBypass())[0];
					}
					else{
						if(!path.IsEnd()){
							Path np=EnableBypass() ? path.GetNextShortestPathFlying() : path.GetNextShortestPath();
							//Debug.Log(np.gameObject);
							nextP=np.GetWP(1, EnableBypass())[0];
						}
						else{
							if(path.loop) nextP=path.GetWP(0)[0];
							else nextP=thisP+(thisP-lastP).normalized;
						}
					}
				}
			}
			else{
				if(subWpIdx>0){
					nextP=subPath[subWpIdx-1];
				}
				else{
					if(wpIdx>0){
						nextP=path.GetWP(wpIdx-1, EnableBypass())[0];
					}
					else{
						if(prevPathList.Count>0){
							Path np=prevPathList[prevPathList.Count-1]; 
							List<Vector3> wpList=np.GetWP(np.GetWPCount()-1, EnableBypass());
							nextP=wpList[wpList.Count-1];
						}
						else{
							nextP=thisP+(thisP-lastP).normalized;
						}
					}
				}
			}
			
			lastP.y=0;	thisP.y=0;	nextP.y=0;
			
			//Debug.Log(lastP+"   "+thisP+"   "+nextP+"    pathOffset-"+pathOffset);
			//Debug.DrawLine(lastP, lastP+new Vector3(0, 1f, 0), Color.red, .95f);
			//Debug.DrawLine(thisP, thisP+new Vector3(0, .8f, 0), Color.green, .95f);
			//Debug.DrawLine(nextP, nextP+new Vector3(0, .6f, 0), Color.blue, .95f);
			
			Vector3 dir1=(thisP-lastP).normalized;
			Vector3 dir2=(nextP-thisP).normalized;
			Vector3 dir=(dir1+dir2).normalized;
			
			float angle=Vector3.Angle(dir1, dir2);
			
			return new Vector3(-dir.z, 0, dir.x)*pathOffset*(1+Mathf.Min(1, (angle/90f))*.4142f);
		}
		
		
		
		private bool EnableBypass(){ return flying & AStar.EnableFlyingBypass(); }
		
		
		private List<Path> prevPathList=new List<Path>();
		private Vector3 lastTargetPos;
		private Vector3 targetPos;
		private Vector3 lastPathOffsetV;
		
		//called by GetPathForUnit() in Path.cs
		public Vector3 GetTargetPos(){ return targetPos; }	
		public Vector3 GetLastTargetPos(){ return lastTargetPos; }	
		//public void ForceNewSubPath(List<Vector3> newPath){ subPath=newPath; subWpIdx=1; }
		
		//for target scanning
		public override float GetPathDist(){ return path!=null ? path.GetDistanceToEnd() : 0 ; }
		public override int GetWPIdx(){ return wpIdx; }
		public override int GetSubWPIdx(){ return subWpIdx; }
		public override float GetDistToNextWP(){ return subWpIdx; }
		public override float GetDistToTargetPos(){ return Vector3.Distance(GetPos(), targetPos); }
		
		
		private float speedMultiplier=1;
		public override float GetCreepSpeedMul(){ return speedMultiplier; }
		
		
		
		private void ReachDestination(){
			
			
			effectDestination.Spawn(GetPos(), Quaternion.identity);
			AudioManager.PlaySound(soundDestination);
			AudioManager.PlaySoundW(CreepDestination, gameObject);
			
			GameControl.LostLife(lifeLostOnDestination);
			
			if(path.loop){
				wpIdx=-1;	subWpIdx=0;
				SpawnManager.ClearCleanWaveFlag(waveIdx);
				
				if(path.warpToStart){
					ResetPathOffset();
					thisT.position=path.GetWP(0)[0]+pathOffsetV;
					if(faceTravelingDir) thisT.rotation=Quaternion.LookRotation(path.GetWP(1)[0]+pathOffsetV-GetPos());
					subPath=path.GetWP(0, EnableBypass());
				}
			}
			else{
				path.OnCreepExit(this);
				path=null;
				
				AnimPlayMove(0);
				float animDuration=AnimPlayDestination();
				
				//enabled=false;
				SpawnManager.CreepDestroyed(this, true);
				ObjectPoolManager.Unspawn(thisObj, animDuration);
			}
			
			
			//Destroy(thisObj);
		}
		
		
		public UnitCreep spawnerPrefab;
		public int spawnerCount=1;
		public float cooldown_Spawner=5;
		public SubUnitOverride spawnerOverride=new SubUnitOverride();
		
		
		public UnitCreep spawnOnDestroyed;
		public int sodCount=1;
		public SubUnitOverride sodOverride=new SubUnitOverride();
		
		[System.Serializable]
		public class SubUnitOverride{
			public float mulHP=.5f;	//the override value, determine by applying this multiplier to the parent unit value
			public float mulSH=.5f;	
			public float mulSpd=.5f;	
			public float mulExp=.5f;	
			public float mulRsc=.5f;	
			
			public float GetHP(UnitCreep unit){ return mulHP>=0 ? unit.GetFullHP()*mulHP : -1; }
			public float GetSH(UnitCreep unit){ return mulSH>=0 ? unit.GetFullSH()*mulSH : -1; }
			public float GetSpd(UnitCreep unit){ return mulSpd>=0 ? unit.GetSpeed()*mulSpd : -1; }
			public int GetExp(UnitCreep unit){ return mulExp>=0 ? (int)Mathf.Round(unit.expGainOnDestroyed*mulExp) : -1; }
			public List<int> GetRsc(UnitCreep unit, List<int> overrideList=null){ 
				if(mulRsc>=0) overrideList=RscManager.ApplyMultiplier(new List<int>(unit.rscGainOnDestroyed), mulRsc);
				return overrideList;
			}
		}
		
		
		public override void Destroyed(bool spawnEffDestroyed=true, bool destroyedByAttack=true){
			hp=0;
			
			path.OnCreepExit(this);
			
			if(spawnEffDestroyed){
				effectDestroyed.Spawn(GetTargetPoint(), Quaternion.identity);
				AudioManager.PlaySound(soundDestroyed);
				AudioManager.PlaySoundW(CreepDestroyed, gameObject);
			}
			
			if(IsSupport()) SupportClearAllTarget();
			
			if(spawnOnDestroyed!=null && sodCount>0){
				float oHP=sodOverride.GetHP(this);
				float oSH=sodOverride.GetSH(this);
				float oSpd=sodOverride.GetSpd(this);
				int oExp=sodOverride.GetExp(this);
				List<int> oRsc=sodOverride.GetRsc(this, rscGainOnDestroyed);
				
				for(int i=0; i<sodCount; i++) SpawnSubCreep(spawnOnDestroyed, oHP, oSH, oSpd, oExp, oRsc);
			}
			
			ClearAllEffect();
			
			if(Random.value<lifeGainedOnDestroyedChance) GameControl.GainLife(lifeGainedOnDestroyed);
			RscManager.GainRsc(rscGainOnDestroyed, RscManager._GainType.CreepKilled);
			
			float animDuration=AnimPlayDestroyed();
			
			SpawnManager.CreepDestroyed(this);
			ObjectPoolManager.Unspawn(thisObj, animDuration);
			//Destroy(thisObj);
		}
		
		
		public float GetMoveAngle(){
			Quaternion rot=Quaternion.LookRotation(targetPos-lastTargetPos);
			return rot.eulerAngles.y;
		}
		
		
		public override void OnDrawGizmos(){
			base.OnDrawGizmos();
			
			if(Application.isPlaying && subPath.Count>0){
				//Gizmos.color=Color.red;
				Gizmos.DrawLine(targetPos, lastTargetPos+lastPathOffsetV);
				//Gizmos.color=Color.white;
				if(subWpIdx<subPath.Count) Gizmos.DrawLine(GetPos(), subPath[subWpIdx]);
				//Gizmos.color=Color.green;
				for(int i=1; i<subPath.Count; i++){
					Gizmos.DrawLine(subPath[i-1], subPath[i]);
				}
			}
		}
		
	}

}