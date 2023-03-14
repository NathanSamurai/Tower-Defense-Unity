using System.Collections;
using System.Collections.Generic;

using UnityEngine;
//using UnityEditor.Animations;


namespace TDTK{
	
	
	public class Unit : MonoBehaviour {
		
		public static bool damagePerShootPoint=false;
		
		protected Unit GetUnit(){ return this; }
		
		public enum _UnitType{ Tower, Creep }
	
		//private _UnitType type;
		public virtual _UnitType GetUnitType(){ return _UnitType.Tower; }
		public virtual bool IsTower(){ return false; }
		public virtual bool IsCreep(){ return false; }
		public virtual UnitTower GetTower(){ return null; }
		public virtual UnitCreep GetCreep(){ return null; }
		
		//for creep that use stop to attack
		public virtual void CreepAttackCount(){  }
		public virtual bool CreepIsOnAttackCD(){ return false; }
		public virtual float GetCreepSpeedMul(){ return 1; }
		
		
		public virtual bool IsPreview(){ return false; }
		public virtual bool InConstruction(){ return false; }
		
		public virtual void Destroyed(bool spawnEffDestroyed=true, bool destroyedByAttack=true){ }
		
		
		
		public virtual bool IsTurret(){ return false; }
		public virtual bool IsAOE(){ return false; }
		public virtual bool IsSupport(){ return false; }
		public virtual bool IsResource(){ return false; }
		public virtual bool IsMine(){ return false; }
		public virtual bool IsSpawner(){ return false; }
		
		
		//public bool isTurret=true;
		//public bool isAOE=false;
		// public bool isSupport=false;
		//public bool isResource=false;
		//public bool isMine=false;
		//public bool isSpawner=false;
		
		[Space(5)]
		public bool resetTargetOnAttack=false;
		private bool targetReset=true;
		
		
		
		
		
		//for tower only
		public virtual UnitTower._TargetGroup GetTargetGroup(){ return UnitTower._TargetGroup.All; }
		
		//For creep only
		public virtual bool IsFlying(){ return false; }
		
		public virtual float GetPathDist(){ return 0; }
		public virtual int GetWPIdx(){ return 0; }
		public virtual int GetSubWPIdx(){ return 0; }
		public virtual float GetDistToNextWP(){ return 0; }
		public virtual float GetDistToTargetPos(){ return 0; }
		
		
		public int prefabID=-1;
		public int instanceID=-1;
		public string unitName="";
		public Sprite icon;
		public string desp="unit description";
		
		[Space(8)] 
		public bool canBeAttacked=true;
		public bool canBeTargeted=true;
		public float unitRadius=.25f;
		public Transform targetPoint;
		public float GetRadius(){ return unitRadius; }
		public Vector3 GetTargetPoint(){ return targetPoint!=null ? targetPoint.position : GetT().position; }
		
		[Space(10)]
		public float hp=10;
		
		public float sh=0;
		public float shStagger=0;
		
		public float cooldown=0;
		//public float cooldownAttack=0;
		public float cooldownAOE=0;
		public float cooldownMine=0;
		public float cooldownRsc=0;
		//public float cooldownSpawner=0;
		
		public int level=0;
		public List<Stats> statsList=new List<Stats>{ new Stats() };
		
		
		public List<int> effectImmunityList=new List<int>();
		
		
		[Space(10)] 
		public List<Unit> attackTargetList=new List<Unit>();
		public void SetAttackTarget(Unit tgt){ attackTargetList.Clear();	attackTargetList.Add(tgt) ; }
		
		public bool HasTarget(){ return GetTarget()!=null; }
		public Unit GetTarget(){ 
			//Unit[] targets = attackTargetList.ToArray();
			return attackTargetList.Count>0 ? attackTargetList[0] : null ; }
		
		public void ClearTarget(){ attackTargetList.Clear(); }
		
		[Space(10)] 
		//public Unit attackTarget;
		//public Unit GetAttackTarget(){ return attackTarget ; }
		//public bool HasTarget(){ return attackTarget!=null ; }
		//public void ClearAttackTarget(){ attackTarget=null; }
		
		
		public ShootObject shootObject;
		public List<Transform> shootPoint=new List<Transform>();
		public float shootPointSpacing=.2f;
		
		public VisualObject shootEffObj_AOE;
		public VisualObject shootEffObj_Resource;
		public VisualObject shootEffObj_Mine;
		//public GameObject shootEffectObj;	//for aoe, resource and mine tower
		public float shootEffectDuration;
		
		
		
		public Transform turretPivot;
		public Transform barrelPivot;
		public bool aimInXAxis=true;
		
		private Quaternion turretDefaultRot;
		private Quaternion barrelDefaultRot;
		
		
		public bool snapAiming=true;
		public float aimSpeed=20;
		public bool aimed=false;
		public void SetAim(bool flag){ aimed=(flag | snapAiming | turretPivot==null); }
		private float aimCD=0;
		
		
		
		protected GameObject thisObj;	public GameObject GetObj(){ return thisObj; }
		protected Transform thisT;		public Transform GetT(){ return thisT; }
		public Vector3 GetPos(){ return thisT!=null ? thisT.position : transform.position ; }
		
		public virtual void Awake(){
			thisT=transform;
			thisObj=gameObject;
			
			bool checkCD=IsTurret() || IsAOE() || IsResource() || IsSpawner();
			bool checkHit=IsTurret() || IsAOE() || IsMine();
			
			if(statsList.Count==0) statsList.Add(new Stats());
			for(int i=0; i<statsList.Count; i++) statsList[i].VerifyBaseStats(true, checkCD, checkHit);
			
			for(int i=0; i<shootPoint.Count; i++){ if(shootPoint[i]==null) shootPoint.RemoveAt(i); }
			if(shootPoint.Count==0) shootPoint.Add(thisT);
			
			activeEffectMod=new Effect();	activeEffectMod.SetAsModifier();
			activeEffectMul=new Effect();	activeEffectMul.SetAsMultiplier();
			
			if(IsTurret()) targetCountPerAttack=Mathf.Max(1, targetCountPerAttack);
			
			if(snapAiming || turretPivot==null) aimed=true;
			
			if(turretPivot!=null) turretDefaultRot=turretPivot.localRotation;
			if(barrelPivot!=null) barrelDefaultRot=barrelPivot.localRotation;
			
			InitAnimation();
		}
		
		IEnumerator Start(){
			if(!IsTower()) yield break;
			yield return null;
			
			GetTower().Init();
		}
		
		
		public virtual void Update(){
			aimCD-=Time.deltaTime;
			if(!IsStunned() && aimCD<0 && IsTurret() && !IsPreview() && !InConstruction()) Aim();
		}
		
		
		public virtual void FixedUpdate(){
			if(IsPreview()) return;
			
			IterateEffect();
			
			if(!GameControl.HasGameStarted() || GameControl.IsGamePaused()) return;
			
			if(!InConstruction() && !IsDestroyed()){
				if(shStagger>0) shStagger-=Time.fixedDeltaTime;
				if(shStagger<=0 && sh<GetFullSH()) sh+=GetSHRegen()*Time.fixedDeltaTime;
				
				sh+=GetEffSHRate() * Time.fixedDeltaTime;
				
				float hpRate=GetEffHPRate();
				if(hpRate>0){
					hp += hpRate * Time.fixedDeltaTime;
				}
				else if(hpRate<0){
					hpRate=ApplyShieldDamage(-hpRate * Time.fixedDeltaTime);
					if(hpRate!=0){
						hp -= hpRate;
						if(hp<=0){ Destroyed(); return; }
					}
				}
				
				//hp+=GetEffHPRate() * Time.fixedDeltaTime;
				//if(hp<=0){ Destroyed(); return; }
				//if(GetEffHPRate()<0 || GetEffSHRate()<0) shStagger=GetSHStagger();
			}
			
			
			if(IsStunned() || InConstruction() || IsDestroyed()) return;
			
			if(cooldown>0)		cooldown-=Time.fixedDeltaTime;
			if(cooldownAOE>0)	cooldownAOE-=Time.fixedDeltaTime;
			if(cooldownMine>0)	cooldownMine-=Time.fixedDeltaTime;
			if(!SpawnManager.IsBetweenWaves() && cooldownRsc>0)	cooldownRsc-=Time.fixedDeltaTime;
			
			if(IsTurret()){
				ScanForTarget();
				Attack();
			}
		}
		
		
		protected bool turretShifted=false;
		protected bool resetingAim=false;
		protected IEnumerator ResetAim(){
			if(!turretShifted) yield break;
			if(turretPivot==null) yield break;
			
			//Debug.Log("ResetAim");
			
			resetingAim=true;
			turretShifted=false;
			
			while(true){
				if(!HasTarget()) break;
				
				bool aimReset=true;
				
				if(turretPivot!=thisT){
					turretPivot.localRotation=Quaternion.Lerp(turretPivot.localRotation, turretDefaultRot, aimSpeed*Time.deltaTime*.5f);
					aimReset=Quaternion.Angle(turretPivot.localRotation,turretDefaultRot)<1;
				}
				
				if(barrelPivot!=null){
					barrelPivot.localRotation=Quaternion.Lerp(barrelPivot.localRotation, barrelDefaultRot, aimSpeed*Time.deltaTime*.25f);
					aimReset=aimReset & Quaternion.Angle(barrelPivot.localRotation, barrelDefaultRot)<1;
				}
				
				if(aimReset) break;
				
				yield return null;
			}
			
			resetingAim=false;
		}
		
		public void Aim(){
			if(!HasTarget()){ SetAim(false); return; }
			if(turretPivot==null){ SetAim(true); return; }
			
			turretShifted=true;
			
			Vector3 tgtPoint=GetTarget().GetTargetPoint();
			float elevation=shootObject.GetElevationAngle(shootPoint[0].position, tgtPoint);
			
			if(!aimInXAxis || barrelPivot!=null) tgtPoint.y=turretPivot.position.y;
			Quaternion wantedRot=Quaternion.LookRotation(tgtPoint-turretPivot.position);
			
			if(elevation!=0 && aimInXAxis && barrelPivot==null) wantedRot*=Quaternion.Euler(elevation, 0, 0);
			
			if(snapAiming) turretPivot.rotation=wantedRot;
			else{
				turretPivot.rotation=Quaternion.Lerp(turretPivot.rotation, wantedRot, aimSpeed*Time.deltaTime);
				SetAim(Quaternion.Angle(turretPivot.rotation, wantedRot)<5);
			}
		
			if(!aimInXAxis || barrelPivot==null) return;
			Quaternion wantedRotX=Quaternion.LookRotation(GetTarget().GetTargetPoint()-barrelPivot.position);
			if(elevation!=0) wantedRotX*=Quaternion.Euler(elevation, 0, 0);
			if(snapAiming) barrelPivot.rotation=wantedRotX;
			else{
				barrelPivot.rotation=Quaternion.Lerp(barrelPivot.rotation, wantedRotX, aimSpeed*Time.deltaTime*2);
			}
		}
		
		
		public enum _TargetMode{ NearestToDestination, NearestToSelf, MostHP, LeastHP, Random, }
		public _TargetMode targetMode;
		public void CycleTargetMode(){ targetMode=(_TargetMode)(((int)targetMode+1)%5); }
		
		public int targetCountPerAttack=1;
		public bool useLOSTargeting;
		
		public float targetingFov=360;
		public float targetingDir=0;
		public bool UseDirectionalTargeting(){ return targetingFov>0 && targetingFov<360; }
		
		public bool CheckTargetLOS(Unit tgtUnit){
			RaycastHit hitInfo;
			LayerMask mask=1<<TDTK.GetLayerTower() | 1<<TDTK.GetLayerCreep();
			return !Physics.Linecast(GetTargetPoint(), tgtUnit.GetTargetPoint(), out hitInfo, ~mask);
		}
		
		//Credit to Nameless Game for contributing the code for targeting multiple target
		public void ScanForTarget(){
			for(int i=0; i<attackTargetList.Count; i++){
				bool removeTarget=false;
				
				if(!removeTarget && attackTargetList[i]==null) removeTarget=true;
				
				if(!removeTarget &&  attackTargetList[i].IsDestroyed()) removeTarget=true;
				
				if(!removeTarget){
					float dist=Vector3.Distance(GetPos(), attackTargetList[i].GetPos());
					if(dist>GetDetectionRange(attackTargetList[i])) removeTarget=true;
				}
				
				if(!removeTarget && useLOSTargeting && !CheckTargetLOS(attackTargetList[i])) removeTarget=true;
				
				if(removeTarget){
					attackTargetList.RemoveAt(i);	i-=1;		continue;
				}
			}
			
			if(attackTargetList.Count>=targetCountPerAttack) return;
			
			if(CreepIsOnAttackCD()) return;	//for creep only
			
			List<Unit> unitList=null;
			if(IsTower()) unitList=SpawnManager.GetUnitsWithinRange(this, GetAttackRange(), GetTargetGroup());
			else unitList=TowerManager.GetUnitsWithinRange(this, GetAttackRange());
			
			for(int i=0; i<unitList.Count; i++){
				if(!unitList[i].canBeTargeted || attackTargetList.Contains(unitList[i])){
					unitList.RemoveAt(i);	i-=1;
				}
			}
			
			if(targetingFov>0 && targetingFov<360){
				Quaternion curDir=thisT.rotation*Quaternion.Euler(0, targetingDir, 0);
				for(int i=0; i<unitList.Count; i++){
					Quaternion dirToTarget=Quaternion.LookRotation(unitList[i].GetPos()-GetPos());
					if(Quaternion.Angle(curDir, dirToTarget)>targetingFov*0.5f){ unitList.RemoveAt(i);	i-=1; }
				}
			}
			
			if(useLOSTargeting){
				for(int i=0; i<unitList.Count; i++){
					if(CheckTargetLOS(unitList[i])) continue;
					unitList.RemoveAt(i); i-=1;
				}
			}
			
			if(unitList.Count<=0) return;
			
			if(IsCreep() && targetMode==_TargetMode.NearestToDestination) targetMode=_TargetMode.Random;
			
			int requiredTargetCount=targetCountPerAttack-attackTargetList.Count;
			int tmpTargetIdx=-1;
			List<int> newTargetsIdx = new List<int>();
			
			if (unitList.Count <= requiredTargetCount) {
				for (int i = 0; i < unitList.Count; i++) {
					newTargetsIdx.Add(i);
				}
			}
			else if (targetMode == _TargetMode.Random) {
				for (int i = 0; i < requiredTargetCount; i++) {
					do {
						tmpTargetIdx = Random.Range(0, unitList.Count);
					} while (newTargetsIdx.Contains(tmpTargetIdx));
					newTargetsIdx.Add(tmpTargetIdx);
				}
			}
			else if (targetMode == _TargetMode.NearestToSelf) {
				for (int i = 0; i < requiredTargetCount; i++) {
					float nearest = Mathf.Infinity;
					for (int j = 0; j < unitList.Count; j++) {
						if (newTargetsIdx.Contains(j)) continue;
						float dist = Vector3.Distance(GetPos(), unitList[j].GetPos());
						if (dist < nearest) { tmpTargetIdx = j; nearest = dist; }
					}
					newTargetsIdx.Add(tmpTargetIdx);
				}
			}
			else if (targetMode == _TargetMode.MostHP) {
				for (int i = 0; i < requiredTargetCount; i++) {
					float mostHP = 0;
					for (int j = 0; j < unitList.Count; j++) {
						if (newTargetsIdx.Contains(j)) continue;
						if (unitList[j].hp + unitList[j].sh > mostHP) { tmpTargetIdx = j; mostHP = unitList[j].hp + unitList[j].sh; }
					}
					newTargetsIdx.Add(tmpTargetIdx);
				}
			}
			else if (targetMode == _TargetMode.LeastHP) {
				for (int i = 0; i < requiredTargetCount; i++) {
					float leastHP = Mathf.Infinity;
					for (int j = 0; j < unitList.Count; j++) {
						if (newTargetsIdx.Contains(j)) continue;
						if (unitList[j].hp + unitList[j].sh < leastHP) { tmpTargetIdx = j; leastHP = unitList[j].hp + unitList[j].sh; }
					}
					newTargetsIdx.Add(tmpTargetIdx);
				}
			}
			else if (targetMode == _TargetMode.NearestToDestination) {
				for (int i = 0; i < requiredTargetCount; i++) {
					float pathDist = Mathf.Infinity; int furthestWP = 0; int furthestSubWP = 0; float distToDest = Mathf.Infinity;
					for (int j = 0; j < unitList.Count; j++) {
						if (newTargetsIdx.Contains(j)) continue;

						float pDist = unitList[j].GetPathDist();
						int wpIdx = unitList[j].GetWPIdx();
						int subWpIdx = unitList[j].GetSubWPIdx();
						float tgtDistToDest = unitList[j].GetDistToTargetPos();

						if (pDist < pathDist) {
							tmpTargetIdx = j; pathDist = pDist; furthestWP = wpIdx; furthestSubWP = subWpIdx; distToDest = tgtDistToDest;
						}
						else if (pDist == pathDist) {
							if (furthestWP < wpIdx) {
								tmpTargetIdx = j; pathDist = pDist; furthestWP = wpIdx; furthestSubWP = subWpIdx; distToDest = tgtDistToDest;
							}
							else if (furthestWP == wpIdx) {
								if (furthestSubWP < subWpIdx) {
									tmpTargetIdx = j; pathDist = pDist; furthestWP = wpIdx; furthestSubWP = subWpIdx; distToDest = tgtDistToDest;
								}
								else if (furthestSubWP == subWpIdx && tgtDistToDest < distToDest) {
									tmpTargetIdx = j; pathDist = pDist; furthestWP = wpIdx; furthestSubWP = subWpIdx; distToDest = tgtDistToDest;
								}
							}
						}
					}
					newTargetsIdx.Add(tmpTargetIdx);
				}
			}
			
			if(newTargetsIdx.Count>0) {
				foreach(int targetIdx in newTargetsIdx) {
					attackTargetList.Add(unitList[targetIdx]);
				}
				if(snapAiming) Aim();
			}
		}
		
		
		/*
		public void ScanForTarget_SingleTarget(){
			if(attackTarget!=null){
				if(attackTarget.IsDestroyed()) attackTarget=null;
				else{
					float dist=Vector3.Distance(GetPos(), attackTarget.GetPos());
					if(dist>GetDetectionRange(attackTarget)) attackTarget=null;
					
					if(attackTarget!=null && useLOSTargeting && !CheckTargetLOS(attackTarget)) attackTarget=null;
					
					if(attackTarget!=null) return;
				}
			}
			
			if(CreepIsOnAttackCD()) return;	//for creep only
			//if(cooldownAttack>0) return;
			
			List<Unit> unitList=null;
			if(IsTower()) unitList=SpawnManager.GetUnitsWithinRange(this, GetAttackRange(), GetTargetGroup());
			else unitList=TowerManager.GetUnitsWithinRange(this, GetAttackRange());
			
			for(int i=0; i<unitList.Count; i++){
				if(unitList[i].canBeTargeted) continue;
				unitList.RemoveAt(i);	i-=1;
			}
			
			if(targetingFov>0 && targetingFov<360){
				Quaternion curDir=thisT.rotation*Quaternion.Euler(0, targetingDir, 0);
				for(int i=0; i<unitList.Count; i++){
					Quaternion dirToTarget=Quaternion.LookRotation(unitList[i].GetPos()-GetPos());
					if(Quaternion.Angle(curDir, dirToTarget)>targetingFov*0.5f){ unitList.RemoveAt(i);	i-=1; }
				}
			}
			
			if(useLOSTargeting){
				for(int i=0; i<unitList.Count; i++){
					if(CheckTargetLOS(unitList[i])) continue;
					unitList.RemoveAt(i); i-=1;
				}
			}
			
			if(unitList.Count<=0) return;
			
			if(IsCreep() && targetMode==_TargetMode.NearestToDestination) targetMode=_TargetMode.Random;
			
			int newTargetIdx=-1;
			
			if(unitList.Count==1){
				newTargetIdx=0;
			}
			else if(targetMode==_TargetMode.Random){
				newTargetIdx=Random.Range(0, unitList.Count);
			}
			else if(targetMode==_TargetMode.NearestToSelf){
				float nearest=Mathf.Infinity;
				for(int i=0; i<unitList.Count; i++){
					float dist=Vector3.Distance(GetPos(), unitList[i].GetPos());
					if(dist<nearest){ newTargetIdx=i; nearest=dist; }
				}
			}
			else if(targetMode==_TargetMode.MostHP){
				float mostHP=0;
				for(int i=0; i<unitList.Count; i++){
					if(unitList[i].hp+unitList[i].sh>mostHP){ newTargetIdx=i; mostHP=unitList[i].hp+unitList[i].sh; }
				}
			}
			else if(targetMode==_TargetMode.LeastHP){
				float leastHP=Mathf.Infinity;
				for(int i=0; i<unitList.Count; i++){
					if(unitList[i].hp+unitList[i].sh<leastHP){ newTargetIdx=i; leastHP=unitList[i].hp+unitList[i].sh; }
				}
			}
			else if(targetMode==_TargetMode.NearestToDestination){
				float pathDist=Mathf.Infinity; int furthestWP=0; int furthestSubWP=0; float distToDest=Mathf.Infinity;
				for(int i=0; i<unitList.Count; i++){
					float pDist=unitList[i].GetPathDist();
					int wpIdx=unitList[i].GetWPIdx();
					int subWpIdx=unitList[i].GetSubWPIdx();
					float tgtDistToDest=unitList[i].GetDistToTargetPos();
					
					if(pDist<pathDist){
						newTargetIdx=i; pathDist=pDist; furthestWP=wpIdx; furthestSubWP=subWpIdx; distToDest=tgtDistToDest;
					}
					else if(pDist==pathDist){
						if(furthestWP<wpIdx){
							newTargetIdx=i; pathDist=pDist; furthestWP=wpIdx; furthestSubWP=subWpIdx; distToDest=tgtDistToDest;
						}
						else if(furthestWP==wpIdx){
							if(furthestSubWP<subWpIdx){
								newTargetIdx=i; pathDist=pDist; furthestWP=wpIdx; furthestSubWP=subWpIdx; distToDest=tgtDistToDest;
							}
							else if(furthestSubWP==subWpIdx && tgtDistToDest<distToDest){
								newTargetIdx=i; pathDist=pDist; furthestWP=wpIdx; furthestSubWP=subWpIdx; distToDest=tgtDistToDest;
							}
						}
					}
				}
			}
			
			
			if(newTargetIdx>=0){
				attackTarget=unitList[newTargetIdx];
				if(snapAiming) Aim();
			}
			
		}
		*/
		
		public float GetDetectionRange(Unit tgtUnit){ return GetAttackRange()+tgtUnit.GetRadius(); }
		
		
		public void Attack(){
			//if game is not paused
			//if(!GameControl.IsGamePaused()) cooldown-=Time.fixedDeltaTime;
			
			//if(cooldownAttack>0) return;
			if(cooldown>0) return;
			
			if(resetTargetOnAttack && !targetReset){
				targetReset=true;
				ClearTarget();
				ScanForTarget();
			}
			
			if(!aimed) return;
			if(!HasTarget()) return;
			
			targetReset=false;
			cooldown=GetCooldown();
			
			for(int i=0; i<attackTargetList.Count; i++) StartCoroutine(Shoot(attackTargetList[i]));
			//StartCoroutine(Shoot(new AttackInfo(this, GetTarget(), 0)));
			
			CreepAttackCount();
		}
		
		
		IEnumerator Shoot(Unit targetUnit){
			float attackDelay=AnimPlayAttack();
			if(attackDelay>0) yield return new WaitForSeconds(attackDelay);
			
			for(int i=0; i<shootPoint.Count; i++){
				//GameObject sObj=(GameObject)Instantiate(shootObject.gameObject, shootPoint[i].position, Quaternion.identity);
				GameObject sObj=ObjectPoolManager.Spawn(shootObject.gameObject, shootPoint[i].position, Quaternion.identity);
				ShootObject soInstance=sObj.GetComponent<ShootObject>();
				aimCD=soInstance.aimCooldown;
				
				if(!damagePerShootPoint){
					if(i==shootPoint.Count-1) soInstance.InitShoot(new AttackInfo(this, targetUnit, 0), shootPoint[i]);
					else soInstance.InitShoot(targetUnit, shootPoint[i]);
				}
				else soInstance.InitShoot(new AttackInfo(this, targetUnit, 0), shootPoint[i]);
					
				yield return new WaitForSeconds(shootPointSpacing);
			}
		}
		
		
		/*
		private void FlameThrowerAttack(Unit target){
			Vector3 aimDirection=(target-thisT.position).normalized;
			List<Unit> tgtList=SpawnManager.GetUnitsWithinRange(this, range);
			for(int i=0; i<tgtList.Count; i++){
				Vector3 tgtDirection=(target-thisT.position).normalized;
				if(Vector3.Angle(aimDirection, tgtDirection)<30){	//say the fov of the flamethrower is 60
					tgtList[i].ApplyAttack(new AttackInfo(aInfo.srcUnit, tgtList[i], false))
				}
			}
		}
		*/
		
		
		public void ApplyAttack(AttackInfo aInfo){
			if(aInfo.aoeRange>0){
				if(aInfo.srcUnit.IsTower()){
					List<Unit> tgtList=SpawnManager.GetUnitsWithinRange(this, aInfo.aoeRange);
					for(int i=0; i<tgtList.Count; i++){
						if(tgtList[i]==this) continue;
						tgtList[i].ApplyAttack(new AttackInfo(aInfo.srcUnit, tgtList[i], 0, false));
					}
				}
			}
			
			if(!canBeAttacked){
				TDTK.TextOverlay("immuned", GetTargetPoint());
				return;
			}
			
			if(IsDestroyed()) return;
			
			if(aInfo.damage>0){
				if(aInfo.hit){
					TDTK.TextOverlay(aInfo.damage.ToString(), GetTargetPoint());
					
					//for showing red color popup when critical
					//if(!aInfo.critical) TDTK.TextOverlay(aInfo.damage.ToString(), GetTargetPoint());
					//else UIOverlayText.Show(aInfo.damage.ToString(), GetTargetPoint(), new Color(1, .2f, .2f, 1));
					
					AnimPlayHit();
					
					aInfo.damage=ApplyShieldDamage(aInfo.damage);
					shStagger=GetSHStagger();
					
					hp-=aInfo.damage;
				}
				else TDTK.TextOverlay("missed", GetTargetPoint());
				
				if(hp<=0){
					Destroyed();
					return;
				}
			}
			else if(aInfo.damage<0){
				TDTK.TextOverlay("+"+aInfo.damage.ToString(), GetTargetPoint());
				
				hp-=aInfo.damage;
				hp=Mathf.Min(GetFullHP(), hp);
			}
			
			//Debug.Log("Apply attack "+aInfo.UseEffect());
			if(hp>0 && aInfo.UseEffect()){
				for(int i=0; i<aInfo.effectList.Count; i++) ApplyEffect(aInfo.effectList[i]);
				//ApplyEffect(aInfo.effect);
			}
		}
		
		private float ApplyShieldDamage(float dmg){
			if(sh<=0) return dmg;
			
			if(dmg<sh){
				sh-=dmg;
				dmg=0;
			}
			else{
				dmg-=sh;
				sh=0;
			}
			
			return dmg;
		}
		
		
		protected void FireMiscEffect(VisualObject vObj){ 
			if(vObj.obj==null) return;
			StartCoroutine(_FireMiscEffect(vObj));
		}
		IEnumerator _FireMiscEffect(VisualObject vObj){
			for(int i=0; i<shootPoint.Count; i++){
				vObj.Spawn(shootPoint[i].position, Quaternion.identity);
				//ObjectPoolManager.Spawn(shootEffectObj, shootPoint[i].position, Quaternion.identity, shootEffectDuration);
				yield return new WaitForSeconds(shootPointSpacing);
			}
		}
		
		
		public string GetStatsDescription(){	//not in used, not in live package
			string text="";
			text+="Damage: "+GetDamageMin()+"-"+GetDamageMax()+"\n";
			text+="Cooldown: "+GetCooldown()+"s"+"\n";
			return text;
		}
		
		
		#region stats
		public float GetHP(){ return hp; }
		public float GetHPRatio(){ float hpFull=GetFullHP(); return hpFull>0 ? hp/hpFull : 0;	}
		
		public float GetSH(){ return sh; }
		public float GetSHRatio(){ float shFull=GetFullSH(); return shFull>0 ? sh/shFull : 0;	}
		
		public int GetArmorType(){		return statsList[level].armorType; }
		public int GetDamageType(){	return statsList[level].damageType; }
		
		public float GetFullHP(){ 			return (statsList[level].hp + GetModHP()) * GetMulHP(); }
		public float GetFullSH(){			return (statsList[level].sh + GetModSH()) * GetMulSH(); }
		public float GetSHRegen(){ 		return (statsList[level].shRegen + GetModSHRegen()) * GetMulSHRegen(); }
		public float GetSHStagger(){	return (statsList[level].shStagger + GetModSHStagger()) * GetMulSHStagger(); }
		
		public float GetSpeed(){ 		return (statsList[level].speed + GetModSpeed()) * GetMulSpeed() * GetCreepSpeedMul();  }
		
		public float GetDamageMin(){ 	return (statsList[level].damageMin + GetModDmgMin()) * GetMulDmgMin(); }
		public float GetDamageMax(){ 	return (statsList[level].damageMax + GetModDmgMax()) * GetMulDmgMax(); }
		public float GetAttackRange(){ return (statsList[level].attackRange + GetModAttackRange()) * GetMulAttackRange(); }
		public float GetAOERange(){ 	return (statsList[level].aoeRange + GetModAOE()) * GetMulAOE(); }
		public float GetCooldown(){ 	return (statsList[level].cooldown + GetModCD()) * GetMulCD(); }
		
		public float GetHit(){ 				return (statsList[level].hit + GetModHit()) * GetMulHit(); }
		public float GetCritChance(){ 	return (statsList[level].critChance + GetModCritChance()) * GetMulCritChance(); }
		public float GetCritMultiplier(){	return (statsList[level].critMultiplier + GetModCritMul()) * GetMulCritMul(); }
		
		
		public float GetDamageMin_AOE(){ 	return (statsList[level].damageMin_AOE + GetModDmgMin_AOE()) * GetMulDmgMin_AOE(); }
		public float GetDamageMax_AOE(){ 	return (statsList[level].damageMax_AOE + GetModDmgMax_AOE()) * GetMulDmgMax_AOE(); }
		public float GetAttackRange_AOE(){	return (statsList[level].attackRange_AOE + GetModAttackRange_AOE()) * GetMulAttackRange_AOE(); }
		public float GetCooldown_AOE(){ 		return (statsList[level].cooldown_AOE + GetModCD_AOE()) * GetMulCD_AOE(); }
		
		public float GetHit_AOE(){ 				return (statsList[level].hit_AOE + GetModHit_AOE()) * GetMulHit_AOE(); }
		public float GetCritChance_AOE(){ 	return (statsList[level].critChance_AOE + GetModCritChance_AOE()) * GetMulCritChance_AOE(); }
		public float GetCritMultiplier_AOE(){	return (statsList[level].critMultiplier_AOE + GetModCritMul_AOE()) * GetMulCritMul_AOE(); }
		
		
		public float GetAttackRange_Support(){ return (statsList[level].attackRange_Support + GetModAttackRange_Support()) * GetMulAttackRange_Support(); }
		
		
		public float GetDamageMin_Mine(){ 	return (statsList[level].damageMin_Mine + GetModDmgMin_Mine()) * GetMulDmgMin_Mine(); }
		public float GetDamageMax_Mine(){ 	return (statsList[level].damageMax_Mine + GetModDmgMax_Mine()) * GetMulDmgMax_Mine(); }
		public float GetAOERange_Mine(){ 	return (statsList[level].aoeRange_Mine + GetModAOE_Mine()) * GetMulAOE_Mine(); }
		public float GetCooldown_Mine(){ 	return (statsList[level].cooldown_Mine + GetModCD_Mine()) * GetMulCD_Mine(); }
		
		public float GetHit_Mine(){ 				return (statsList[level].hit_Mine + GetModHit_Mine()) * GetMulHit_Mine(); }
		public float GetCritChance_Mine(){ 	return (statsList[level].critChance_Mine + GetModCritChance_Mine()) * GetMulCritChance_Mine(); }
		public float GetCritMultiplier_Mine(){	return (statsList[level].critMultiplier_Mine + GetModCritMul_Mine()) * GetMulCritMul_Mine(); }
		
		
		public float GetCooldown_Rsc(){ 	return (statsList[level].cooldown_Rsc + GetModCD_Rsc()) * GetMulCD_Rsc(); }
		
		
		public float GetDodge(){ 		return Mathf.Max((statsList[level].dodge + GetModDodge()) * GetMulDodge(), 0); }
		public float GetDmgReduction(){	return Mathf.Clamp((statsList[level].dmgReduc+GetModDmgReduc()) * GetMulDmgReduc(), 0, 1); }
		public float GetCritReduction(){	return Mathf.Max((statsList[level].critReduc + GetModCritReduc()) * GetMulCritReduc(), 0); }
		public bool GetImmunedToCrit(){	return statsList[level].critReduc==Mathf.Infinity; }
		
		public List<float> GetRscGain(){ 
			List<float> list=RscManager.ApplyModifier(new List<float>(statsList[level].rscGain), activeEffectMod.stats.rscGain);
			list=RscManager.ApplyModifier(list, PerkManager.GetModUnitRscGain(prefabID));
			
			list=RscManager.ApplyMultiplier(list, activeEffectMul.stats.rscGain);
			return RscManager.ApplyMultiplier(list, PerkManager.GetMulUnitRscGain(prefabID));
		}
		
		
		public float GetModHP(){ return activeEffectMod.stats.hp + PerkManager.GetModUnitHP(prefabID); }
		public float GetModSH(){ return activeEffectMod.stats.sh + PerkManager.GetModUnitSH(prefabID); }
		public float GetModSHRegen(){ return activeEffectMod.stats.shRegen + PerkManager.GetModUnitSHRegen(prefabID); }
		public float GetModSHStagger(){ return activeEffectMod.stats.shStagger + PerkManager.GetModUnitSHStagger(prefabID); }
		
		public float GetModSpeed(){ return activeEffectMod.stats.speed; }// + PerkManager.GetModUnitSpeed(prefabID); }
		
		public float GetModDodge(){ return activeEffectMod.stats.dodge + PerkManager.GetModUnitDodge(prefabID); }
		public float GetModDmgReduc(){ return activeEffectMod.stats.dmgReduc + PerkManager.GetModUnitDmgReduc(prefabID); }
		public float GetModCritReduc(){ return activeEffectMod.stats.critReduc + PerkManager.GetModUnitCritReduc(prefabID); }
		
		public float GetModDmgMin(){ return activeEffectMod.stats.damageMin + PerkManager.GetModUnitDmgMin(prefabID); }
		public float GetModDmgMax(){ return activeEffectMod.stats.damageMax + PerkManager.GetModUnitDmgMax(prefabID); }
		public float GetModAttackRange(){ return activeEffectMod.stats.attackRange + PerkManager.GetModUnitAttackRange(prefabID); }
		public float GetModAOE(){ return activeEffectMod.stats.aoeRange + PerkManager.GetModUnitAOE(prefabID); }
		public float GetModCD(){ return activeEffectMod.stats.cooldown + PerkManager.GetModUnitCD(prefabID); }
		
		public float GetModHit(){ return activeEffectMod.stats.hit + PerkManager.GetModUnitHit(prefabID); }
		public float GetModCritChance(){ return activeEffectMod.stats.critChance + PerkManager.GetModUnitCrit(prefabID); }
		public float GetModCritMul(){ return activeEffectMod.stats.critMultiplier + PerkManager.GetModUnitCritMul(prefabID); }
		
		public float GetModDmgMin_AOE(){ return activeEffectMod.stats.damageMin_AOE + PerkManager.GetModUnitDmgMin_AOE(prefabID); }
		public float GetModDmgMax_AOE(){ return activeEffectMod.stats.damageMax_AOE + PerkManager.GetModUnitDmgMax_AOE(prefabID); }
		public float GetModAttackRange_AOE(){ return activeEffectMod.stats.attackRange_AOE + PerkManager.GetModUnitAttackRange_AOE(prefabID); }
		//public float GetModAOE_AOE(){ return activeEffectMod.stats.aoeRange_AOE + PerkManager.GetModUnitAOE_AOE(prefabID); }
		public float GetModCD_AOE(){ return activeEffectMod.stats.cooldown_AOE + PerkManager.GetModUnitCD_AOE(prefabID); }
		
		public float GetModHit_AOE(){ return activeEffectMod.stats.hit_AOE + PerkManager.GetModUnitHit_AOE(prefabID); }
		public float GetModCritChance_AOE(){ return activeEffectMod.stats.critChance_AOE + PerkManager.GetModUnitCrit_AOE(prefabID); }
		public float GetModCritMul_AOE(){ return activeEffectMod.stats.critMultiplier_AOE + PerkManager.GetModUnitCritMul_AOE(prefabID); }
		
		public float GetModAttackRange_Support(){ return activeEffectMod.stats.attackRange_Support + PerkManager.GetModUnitAttackRange_Support(prefabID); }
		
		public float GetModDmgMin_Mine(){ return activeEffectMod.stats.damageMin_Mine + PerkManager.GetModUnitDmgMin_Mine(prefabID); }
		public float GetModDmgMax_Mine(){ return activeEffectMod.stats.damageMax_Mine + PerkManager.GetModUnitDmgMax_Mine(prefabID); }
		//public float GetModAttackRange_Mine(){ return activeEffectMod.stats.attackRange_Mine + PerkManager.GetModUnitAttackRange_Mine(prefabID); }
		public float GetModAOE_Mine(){ return activeEffectMod.stats.aoeRange_Mine + PerkManager.GetModUnitAOE_Mine(prefabID); }
		public float GetModCD_Mine(){ return activeEffectMod.stats.cooldown_Mine + PerkManager.GetModUnitCD_Mine(prefabID); }
		
		public float GetModHit_Mine(){ return activeEffectMod.stats.hit_Mine + PerkManager.GetModUnitHit_Mine(prefabID); }
		public float GetModCritChance_Mine(){ return activeEffectMod.stats.critChance_Mine + PerkManager.GetModUnitCrit_Mine(prefabID); }
		public float GetModCritMul_Mine(){ return activeEffectMod.stats.critMultiplier_Mine + PerkManager.GetModUnitCritMul_Mine(prefabID); }
		
		public float GetModCD_Rsc(){ return activeEffectMod.stats.cooldown_Rsc + PerkManager.GetModUnitCD_Rsc(prefabID); }
		
		
		public float GetMulHP(){ return activeEffectMul.stats.hp * PerkManager.GetMulUnitHP(prefabID); }
		public float GetMulSH(){ return activeEffectMul.stats.sh * PerkManager.GetMulUnitSH(prefabID); }
		public float GetMulSHRegen(){ return activeEffectMul.stats.shRegen * PerkManager.GetMulUnitSHRegen(prefabID); }
		public float GetMulSHStagger(){ return activeEffectMul.stats.shStagger * PerkManager.GetMulUnitSHStagger(prefabID); }
		
		public float GetMulSpeed(){ return activeEffectMul.stats.speed; }// * PerkManager.GetMulUnitSpeed(prefabID); }
		
		public float GetMulDodge(){ return activeEffectMul.stats.dodge * PerkManager.GetMulUnitDodge(prefabID); }
		public float GetMulDmgReduc(){ return activeEffectMul.stats.dmgReduc * PerkManager.GetMulUnitDmgReduc(prefabID); }
		public float GetMulCritReduc(){ return activeEffectMul.stats.critReduc * PerkManager.GetMulUnitCritReduc(prefabID); }
		
		public float GetMulDmgMin(){ return activeEffectMul.stats.damageMin * PerkManager.GetMulUnitDmgMin(prefabID); }
		public float GetMulDmgMax(){ return activeEffectMul.stats.damageMax * PerkManager.GetMulUnitDmgMax(prefabID); }
		public float GetMulAttackRange(){ return activeEffectMul.stats.attackRange * PerkManager.GetMulUnitAttackRange(prefabID); }
		public float GetMulAOE(){ return activeEffectMul.stats.aoeRange * PerkManager.GetMulUnitAOE(prefabID); }
		public float GetMulCD(){ return activeEffectMul.stats.cooldown * PerkManager.GetMulUnitCD(prefabID); }
		
		public float GetMulHit(){ return activeEffectMul.stats.hit * PerkManager.GetMulUnitHit(prefabID); }
		public float GetMulCritChance(){ return activeEffectMul.stats.critChance * PerkManager.GetMulUnitCrit(prefabID); }
		public float GetMulCritMul(){ return activeEffectMul.stats.critMultiplier * PerkManager.GetMulUnitCritMul(prefabID); }
		
		public float GetMulDmgMin_AOE(){ return activeEffectMul.stats.damageMin_AOE * PerkManager.GetMulUnitDmgMin_AOE(prefabID); }
		public float GetMulDmgMax_AOE(){ return activeEffectMul.stats.damageMax_AOE * PerkManager.GetMulUnitDmgMax_AOE(prefabID); }
		public float GetMulAttackRange_AOE(){ return activeEffectMul.stats.attackRange_AOE * PerkManager.GetMulUnitAttackRange_AOE(prefabID); }
		//public float GetMulAOE_AOE(){ return activeEffectMul.stats.aoeRange_AOE * PerkManager.GetMulUnitAOE_AOE(prefabID); }
		public float GetMulCD_AOE(){ return activeEffectMul.stats.cooldown_AOE * PerkManager.GetMulUnitCD_AOE(prefabID); }
		
		public float GetMulHit_AOE(){ return activeEffectMul.stats.hit_AOE * PerkManager.GetMulUnitHit_AOE(prefabID); }
		public float GetMulCritChance_AOE(){ return activeEffectMul.stats.critChance_AOE * PerkManager.GetMulUnitCrit_AOE(prefabID); }
		public float GetMulCritMul_AOE(){ return activeEffectMul.stats.critMultiplier_AOE * PerkManager.GetMulUnitCritMul_AOE(prefabID); }
		
		public float GetMulAttackRange_Support(){ return activeEffectMul.stats.attackRange_Support * PerkManager.GetMulUnitAttackRange_Support(prefabID); }
		
		public float GetMulDmgMin_Mine(){ return activeEffectMul.stats.damageMin_Mine * PerkManager.GetMulUnitDmgMin_Mine(prefabID); }
		public float GetMulDmgMax_Mine(){ return activeEffectMul.stats.damageMax_Mine * PerkManager.GetMulUnitDmgMax_Mine(prefabID); }
		//public float GetMulAttackRange_Mine(){ return activeEffectMul.stats.attackRange_Mine * PerkManager.GetMulUnitAttackRange_Mine(prefabID); }
		public float GetMulAOE_Mine(){ return activeEffectMul.stats.aoeRange_Mine * PerkManager.GetMulUnitAOE_Mine(prefabID); }
		public float GetMulCD_Mine(){ return activeEffectMul.stats.cooldown_Mine * PerkManager.GetMulUnitCD_Mine(prefabID); }
		
		public float GetMulHit_Mine(){ return activeEffectMul.stats.hit_Mine * PerkManager.GetMulUnitHit_Mine(prefabID); }
		public float GetMulCritChance_Mine(){ return activeEffectMul.stats.critChance_Mine * PerkManager.GetMulUnitCrit_Mine(prefabID); }
		public float GetMulCritMul_Mine(){ return activeEffectMul.stats.critMultiplier_Mine * PerkManager.GetMulUnitCritMul_Mine(prefabID); }
		
		public float GetMulCD_Rsc(){ return activeEffectMul.stats.cooldown_Rsc * PerkManager.GetMulUnitCD_Rsc(prefabID); }
		
		
		
		
		public bool GetEffStun(){ return activeEffectMod.stun; }
		public float GetEffHPRate(){ return activeEffectMod.stats.hpRate * activeEffectMul.stats.hpRate; }
		public float GetEffSHRate(){ return activeEffectMod.stats.shRate * activeEffectMul.stats.shRate; }
		
		
		
		public float GetEffectOnHitChance(){ return (statsList[level].effectOnHitChance + GetModEffectOnHitC()) * GetMulEffectOnHitC(); }
		public float GetModEffectOnHitC(){ return activeEffectMod.stats.effectOnHitChance + PerkManager.GetModUnitEffOnHitChance(prefabID); }
		public float GetMulEffectOnHitC(){ return activeEffectMul.stats.effectOnHitChance * PerkManager.GetMulUnitEffOnHitChance(prefabID); }
		
		public float GetEffectOnHitChance_AOE(){ return (statsList[level].effectOnHitChance_AOE + GetModEffectOnHitC_AOE()) * GetMulEffectOnHitC_AOE(); }
		public float GetModEffectOnHitC_AOE(){ return activeEffectMod.stats.effectOnHitChance_AOE + PerkManager.GetModUnitEffOnHitChance_AOE(prefabID); }
		public float GetMulEffectOnHitC_AOE(){ return activeEffectMul.stats.effectOnHitChance_AOE * PerkManager.GetMulUnitEffOnHitChance_AOE(prefabID); }
		
		public float GetEffectOnHitChance_Mine(){ return (statsList[level].effectOnHitChance_Mine + GetModEffectOnHitC_Mine()) * GetMulEffectOnHitC_Mine(); }
		public float GetModEffectOnHitC_Mine(){ return activeEffectMod.stats.effectOnHitChance_Mine + PerkManager.GetModUnitEffOnHitChance_Mine(prefabID); }
		public float GetMulEffectOnHitC_Mine(){ return activeEffectMul.stats.effectOnHitChance_Mine * PerkManager.GetMulUnitEffOnHitChance_Mine(prefabID); }
		
		
		private List<int> cachedEffOnHitList=null;
		public List<Effect> GetEffectOnHit(){
			List<Effect> list=new List<Effect>();
			
			if(IsTower()){
				List<int> overrideIDList=PerkManager.GetUnitOverrideOnHitEff(prefabID);
				if(overrideIDList!=null){
					for(int i=0; i<overrideIDList.Count; i++){
						list.Add(Effect_DB.GetPrefab(overrideIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
					}
					for(int i=0; i<list.Count; i++) list[i].SetType(level, this);
					return list;
				}
				
				for(int i=0; i<statsList[level].effectOnHitIDList.Count; i++){
					list.Add(Effect_DB.GetPrefab(statsList[level].effectOnHitIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
				}
				
				List<int> appendIDList=PerkManager.GetUnitAppendOnHitEff(prefabID);
				if(appendIDList!=null){
					for(int i=0; i<appendIDList.Count; i++){
						list.Add(Effect_DB.GetPrefab(appendIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
					}
				}
			}
			else{
				if(statsList[level].effectOnHitIDList.Count<0) return list;
				
				if(cachedEffOnHitList==null){
					cachedEffOnHitList=new List<int>();
					for(int i=0; i<statsList[level].effectOnHitIDList.Count; i++){
						Effect effect=Effect_DB.GetPrefab(statsList[level].effectOnHitIDList[i]).Clone();
						cachedEffOnHitList.Add(Effect_DB.GetPrefabIndex(effect.prefabID));
					}
				}
				
				for(int i=0; i<cachedEffOnHitList.Count; i++) list.Add(Effect_DB.GetItem(cachedEffOnHitList[i]).Clone());
			}
			
			for(int i=0; i<list.Count; i++) list[i].SetType(level, this);
			return list;
		}
		
		private List<int> cachedEffOnHitList_AOE=null;
		public List<Effect> GetEffectOnHit_AOE(){ 
			List<Effect> list=new List<Effect>();
			
			if(IsTower()){
				List<int> overrideIDList=PerkManager.GetUnitOverrideOnHitEff_AOE(prefabID);
				if(overrideIDList!=null){
					for(int i=0; i<overrideIDList.Count; i++){
						list.Add(Effect_DB.GetPrefab(overrideIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
					}
					for(int i=0; i<list.Count; i++) list[i].SetType(level, this);
					return list;
				}
				
				for(int i=0; i<statsList[level].effectOnHitIDList_AOE.Count; i++){
					list.Add(Effect_DB.GetPrefab(statsList[level].effectOnHitIDList_AOE[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
				}
				
				List<int> appendIDList=PerkManager.GetUnitAppendOnHitEff_AOE(prefabID);
				if(appendIDList!=null){
					for(int i=0; i<appendIDList.Count; i++){
						list.Add(Effect_DB.GetPrefab(appendIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
					}
				}
			}
			else{
				if(statsList[level].effectOnHitIDList_AOE.Count<0) return list;
				
				if(cachedEffOnHitList_AOE==null){
					cachedEffOnHitList_AOE=new List<int>();
					for(int i=0; i<statsList[level].effectOnHitIDList_AOE.Count; i++){
						Effect effect=Effect_DB.GetPrefab(statsList[level].effectOnHitIDList_AOE[i]).Clone();
						cachedEffOnHitList_AOE.Add(Effect_DB.GetPrefabIndex(effect.prefabID));
					}
				}
				
				for(int i=0; i<cachedEffOnHitList_AOE.Count; i++) list.Add(Effect_DB.GetItem(cachedEffOnHitList_AOE[i]).Clone());
			}
			
			for(int i=0; i<list.Count; i++) list[i].SetType(level, this);
			return list;
		}
		
		private List<int> cachedEffOnHitList_Support=null;
		public List<Effect> GetEffectOnHit_Support(){ 
			List<Effect> list=new List<Effect>();
			
			if(IsTower()){
				List<int> overrideIDList=PerkManager.GetUnitOverrideOnHitEff_Support(prefabID);
				if(overrideIDList!=null){
					for(int i=0; i<overrideIDList.Count; i++){
						list.Add(Effect_DB.GetPrefab(overrideIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
					}
					for(int i=0; i<list.Count; i++) list[i].SetType(level, this);
					return list;
				}
				
				for(int i=0; i<statsList[level].effectOnHitIDList_Support.Count; i++){
					list.Add(Effect_DB.GetPrefab(statsList[level].effectOnHitIDList_Support[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
				}
				
				List<int> appendIDList=PerkManager.GetUnitAppendOnHitEff_Support(prefabID);
				if(appendIDList!=null){
					for(int i=0; i<appendIDList.Count; i++){
						list.Add(Effect_DB.GetPrefab(appendIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
					}
				}
			}
			else{
				if(statsList[level].effectOnHitIDList_Support.Count<0) return list;
				
				if(cachedEffOnHitList_Support==null){
					cachedEffOnHitList_Support=new List<int>();
					for(int i=0; i<statsList[level].effectOnHitIDList_Support.Count; i++){
						Effect effect=Effect_DB.GetPrefab(statsList[level].effectOnHitIDList_Support[i]).Clone();
						cachedEffOnHitList_Support.Add(Effect_DB.GetPrefabIndex(effect.prefabID));
					}
				}
				
				for(int i=0; i<cachedEffOnHitList_Support.Count; i++) list.Add(Effect_DB.GetItem(cachedEffOnHitList_Support[i]).Clone());
			}
			
			for(int i=0; i<list.Count; i++) list[i].SetType(level, this);
			return list;
		}
		
		//private List<int> cachedEffOnHitList_Mine=null;
		public List<Effect> GetEffectOnHit_Mine(){ 
			List<Effect> list=new List<Effect>();
			
			List<int> overrideIDList=PerkManager.GetUnitOverrideOnHitEff_Mine(prefabID);
			if(overrideIDList!=null){
				for(int i=0; i<overrideIDList.Count; i++){
					list.Add(Effect_DB.GetPrefab(overrideIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
				}
			}
			else{
				for(int i=0; i<statsList[level].effectOnHitIDList_Mine.Count; i++){
					list.Add(Effect_DB.GetPrefab(statsList[level].effectOnHitIDList_Mine[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
				}
				
				List<int> appendIDList=PerkManager.GetUnitAppendOnHitEff_Mine(prefabID);
				if(appendIDList!=null){
					for(int i=0; i<appendIDList.Count; i++){
						list.Add(Effect_DB.GetPrefab(appendIDList[i]).ModifyWithPerk());	//modify with perk would return a cloned effect
					}
				}
			}
			
			for(int i=0; i<list.Count; i++) list[i].SetType(level, this);
			
			return list;
		}
		
		/*
		private int cachedEffOnHitIdx=-1;
		public Effect GetEffectOnHit(){
			if(IsTower()){
				int overrideID=PerkManager.GetUnitOverrideOnHitEff(prefabID);
				if(overrideID<0 && statsList[level].effectOnHitID<0) return null;
				
				Effect effect=Effect_DB.GetPrefab(overrideID>=0 ? overrideID : statsList[level].effectOnHitID).ModifyWithPerk();
				effect.SetType(level, this);
				return effect;
			}
			else{
				if(statsList[level].effectOnHitID<0) return null;
				
				Effect effect=null;
				if(cachedEffOnHitIdx<0){
					effect=Effect_DB.GetPrefab(statsList[level].effectOnHitID).Clone();
					cachedEffOnHitIdx=Effect_DB.GetPrefabIndex(effect.prefabID);
				}
				else{
					effect=Effect_DB.GetItem(cachedEffOnHitIdx).Clone();
					//Debug.Log("use cached "+cachedEffOnHitIdx+"   "+effect.name);
				}
				
				effect.SetType(level, this);
				return effect;
			}
		}
		*/
		#endregion
		
	
		
		#region Effect
		public Effect activeEffectMod;
		public Effect activeEffectMul;
		public List<Effect> allEffectList=new List<Effect>();
		
		public void ClearAllEffect(){		//called when destroyed
			activeEffectMod=new Effect();	activeEffectMod.SetAsModifier();
			activeEffectMul=new Effect();	activeEffectMul.SetAsMultiplier();
			
			for(int i=0; i<allEffectList.Count; i++){
				if(allEffectList[i].activeVisualEffect!=null){
					ObjectPoolManager.Unspawn(allEffectList[i].activeVisualEffect);
				}
			}
			
			allEffectList=new List<Effect>();
		}
		
		public void ApplyEffect(Effect effect){
			if(effectImmunityList.Contains(effect.prefabID)) return;
			
			effect.hitVisualEffect.Spawn(GetTargetPoint(), Quaternion.identity);
			
			if(!effect.stackable){
				for(int i=0; i<allEffectList.Count; i++){
					if(Effect.FromSimilarSource(allEffectList[i], effect) && allEffectList[i].prefabID==effect.prefabID){
						allEffectList[i].durationRemain=effect.duration;
						if(effect.fromSupport) allEffectList[i].stack+=1;
						return;
					}
				}
			}
			
			if(effect.activeVisualEffect!=null){
				effect.activeVisualEffect=ObjectPoolManager.Spawn(effect.activeVisualEffect, GetTargetPoint(), Quaternion.identity);
				effect.activeVisualEffect.parent=thisT;
			}
			
			effect.durationRemain=effect.duration;
			allEffectList.Add(effect);
			UpdateActiveEffect();
		}
		public void IterateEffect(){
			bool update=false;
			for(int i=0; i<allEffectList.Count; i++){
				allEffectList[i].durationRemain-=Time.fixedDeltaTime;
				if(allEffectList[i].durationRemain<=0){
					if(allEffectList[i].activeVisualEffect!=null){
						ObjectPoolManager.Unspawn(allEffectList[i].activeVisualEffect);
					}
					
					allEffectList.RemoveAt(i);	i-=1;
					update=true;
				}
			}
			if(update) UpdateActiveEffect();
		}
		public void UpdateActiveEffect(){
			activeEffectMod=new Effect();	activeEffectMod.SetAsModifier();
			activeEffectMul=new Effect();	activeEffectMul.SetAsMultiplier();
			
			for(int i=0; i<allEffectList.Count; i++){
				activeEffectMod.stun |= allEffectList[i].stun;
				
				if(!allEffectList[i].IsMultiplier()){
					activeEffectMod.ApplyModifier(allEffectList[i], DamageTable.GetModifier(GetArmorType(), allEffectList[i].stats.damageType));
					
					for(int n=0; n<activeEffectMod.stats.rscGain.Count; n++) 
						activeEffectMod.stats.rscGain[n] += allEffectList[i].stats.rscGain[n];
				}
				else{
					activeEffectMul.ApplyMultiplier(allEffectList[i]);
					
					for(int n=0; n<activeEffectMul.stats.rscGain.Count; n++) 
						activeEffectMul.stats.rscGain[n] *= allEffectList[i].stats.rscGain[n];
				}
			}
		}
		#endregion
		
		
		public bool IsStunned(){ return GetEffStun(); }
		
		public bool IsDestroyed(){ return hp<=0 || !thisObj.activeInHierarchy; }
		
		
		#region animation
		[Header("Animation")]
		public Transform animatorT;
		protected Animator animator;
		
		[Space(5)]
		public AnimationClip clipIdle;
		public AnimationClip clipHit;
		public AnimationClip clipDestroyed;
		
		public AnimationClip clipAttack;
		public float animationAttackDelay=0;
		
		[Space(5)]
		public AnimationClip clipMove;
		public AnimationClip clipSpawn;
		public AnimationClip clipDestination;
		
		[Space(5)]
		public AnimationClip clipConstruct;
		public AnimationClip clipDeconstruct;
		
		//private bool defaultControllerLoaded=false;
		//private static AnimatorController defaultController;
		
		
		void InitAnimation(){
			if(animatorT!=null) animator=animatorT.GetComponent<Animator>();
			if(animator==null) return;
			
			//if(!defaultControllerLoaded){
			//	defaultControllerLoaded=true;
			//	defaultController=Resources.Load("DB_TDTK/TDAnimatorController.controller", typeof(AnimatorController)) as AnimatorController;
			//}
			//if(defaultController!=null) animator.runtimeAnimatorController=defaultController;
			
			
			//AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
			AnimatorOverrideController aniOverrideController = new AnimatorOverrideController();
			aniOverrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
			animator.runtimeAnimatorController = aniOverrideController;
			
			if(clipIdle!=null) 				aniOverrideController["DummyIdle"] = clipIdle;
			if(clipHit!=null) 				aniOverrideController["DummyHit"] = clipHit;
			if(clipAttack!=null) 			aniOverrideController["DummyAttack"] = clipAttack;
			if(clipDestroyed!=null) 	aniOverrideController["DummyDestroyed"] = clipDestroyed;
			
			if(clipMove!=null) 			aniOverrideController["DummyMove"] = clipMove;
			if(clipSpawn!=null) 			aniOverrideController["DummySpawn"] = clipSpawn;
			if(clipDestination!=null) 	aniOverrideController["DummyDestination"] = clipDestination;
			
			if(clipConstruct!=null) 	aniOverrideController["DummyConstruct"] = clipConstruct;
			if(clipDeconstruct!=null) 	aniOverrideController["DummyDeconstruct"] = clipDeconstruct;
		}
		
		protected void AnimPlayMove(float speed){ 
			if(animator!=null) animator.SetFloat("Speed", speed);
			
			if(animator!=null){
				if(IsStunned() && animator.enabled) animator.enabled=false;
				if(!IsStunned() && !animator.enabled) animator.enabled=true;
			}
		}
		protected void AnimPlayHit(){
			if(animator!=null && clipHit!=null) animator.SetTrigger("Hit");
		}
		protected float AnimPlayDestroyed(){
			if(animator==null) return 0;
			if(clipDestroyed!=null) animator.SetBool("Destroyed", true);
			return clipDestroyed!=null ? clipDestroyed.length : 0 ;
		}
		protected float AnimPlayAttack(){
			if(animator==null) return 0;
			if(clipAttack!=null) animator.SetTrigger("Attack");
			return animationAttackDelay;
		}
		
		protected void AnimPlaySpawn(){ 
			if(animator!=null && clipSpawn!=null) animator.SetTrigger("Spawn");
		}
		protected float AnimPlayDestination(){
			if(animator==null) return 0;
			if(clipDestination!=null) animator.SetBool("Destination", true);
			return clipDestination!=null ? clipDestination.length : 0 ;
		}
			
		protected void AnimPlayConstruct(){
			if(animator!=null && clipConstruct!=null) animator.SetTrigger("Construct");
		}
		protected void AnimPlayDeconstruct(){
			if(animator!=null && clipDeconstruct!=null) animator.SetTrigger("Deconstruct");
		}
		
		
		protected void AnimReset(){
			if(animator==null) return;
			if(!animator.isInitialized) return;
			animator.SetBool("Destroyed", false);
			animator.SetBool("Destination", false);
		}
		#endregion
		
		
		
		public virtual void OnDrawGizmos(){
			Gizmos.color=Color.red;
			if(HasTarget()){
				for(int i=0; i<attackTargetList.Count; i++){
					Debug.DrawLine(GetPos(), attackTargetList[i].GetPos());
				}
			}
			
			/*
			if(UseDirectionalTargeting()){
				Vector3 v1=thisT.rotation*Quaternion.Euler(0, targetingDir+targetingFov/2, 0)*new Vector3(0, 0, 3);
				Vector3 v2=thisT.rotation*Quaternion.Euler(0, targetingDir-targetingFov/2, 0)*new Vector3(0, 0, 3);
				Debug.DrawLine(GetPos(), GetPos()+v1);
				Debug.DrawLine(GetPos(), GetPos()+v2);
			}
			*/
		}
	}
	
}