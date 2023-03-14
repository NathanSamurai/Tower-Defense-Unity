using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace TDTK {
	
	
	public class ShootObject : MonoBehaviour {
		
		//code for lightning is contributed by Lynn Pye, source from - https://forum.unity.com/threads/the-best-way-to-create-a-lightning-effect.9058/
		
		public enum _Type{ Projectile, Beam, Effect, /*Missile,*/ Lightning, }
		public _Type type;
		
		[Header("Projectile")]
		
		public bool straightProjectile;
		
		[Space(8)]
		public float speed=10;
		public float elevation=2;		//the maximum height the shoot trajectory is going to reach
		public float falloffRange=1;	//below this range, the elevation will gradually decrease, try set to match the max range of the tower
		
		private float eta=1;				//estimated time to hit target, used to adjust offsetPos during runtime
		private float effElevation=1;		//actual elevation used in runtime, recalculated based on falloffRange
		
		[Space(8)]
		public bool useTrajectoryCurve;
		public float elevationT;
		public AnimationCurve trajectory;
		
		private float initialDist;
		private Vector3 initialPos;
		
		
		//offset to the targetPos for the SO to aim for, adjust in runtime to create a trajectory
		//it's always (0, value, 0), and value is consistently droping as the SO approach the target, making the SO aim above the target and drops overtime
		private Vector3 offsetPos=Vector3.zero;	
		
		[Space(5)] public List<TrailRenderer> trailList=new List<TrailRenderer>();
		
		
		[Header("Lightning")]
		public List<LineRenderer> lightningRenderers = new List<LineRenderer>();
		public float arcLength = 0.5f; // 1.0f
		public float arcVariation = 1.0f;
		public float lightningInaccuracy = 0.5f;
		public float timeOfZap = 0.25f;
		private float zapTimer;
		public float randomRange = 1.5f; // 1.0f
		public float lightningWidth = 0.15f; // 1.0f
		public float maxLgtDist = 1.5f; // 3.0f
		
		
		//[Header("Missile")]
		//public float maxDeviation=1;
		//private Vector3 missileOffset=Vector3.zero;	
		
		
		[Header("Beam")]
		public List<LineRenderer> lines=new List<LineRenderer>();
		public float beamDuration=0.5f;
		public float startWidth=0.25f;
		private List<Vector3> linePos=new List<Vector3>{ Vector3.zero, Vector3.zero };
		private Vector3 tgtPos;
		
		
		[Header("Effect")]
		public float effectDuration=0.5f;
		private bool attachToShootPoint=true;
		
		
		public float aimCooldown=0;
		
		
		[Header("Visual and Audio")]
		public VisualObject effectShoot=new VisualObject();
		public VisualObject effectHit=new VisualObject();
		
		public AudioClip shootSound;
		public AudioClip hitSound;
		
		
		[Header("Runtime Attribute (For Debugging)")]
		public Unit tgtUnit;
		public float tgtRadius=0;
		public Vector3 targetPos;
		
		public AttackInfo attackInfo;
		
		public float shootTime;
		public Transform shootPoint;
		
		private bool shot=false;
		private bool hit=false;
		
		protected GameObject thisObj;	//public GameObject GetObj(){ return thisObj; }
		protected Transform thisT;		//public Transform GetT(){ return thisT; }
		public Vector3 GetPos(){ return thisT!=null ? thisT.position : transform.position ; }
		public Quaternion GetRot(){ return thisT!=null ? thisT.rotation : transform.rotation ; }
		
		public void Awake(){
			thisT=transform;
			thisObj=gameObject;
			
			if(type==_Type.Beam){
				for(int i=0; i<lines.Count; i++){
					if(lines[i]==null){ lines.RemoveAt(i); i-=1; }
				}
				if(lines.Count==0) Debug.LogWarning("Beam type shoot-object hasn't been assigned any LineRenderer");
			}
			
			if(type==_Type.Lightning){
				for(int i=0; i<lightningRenderers.Count; i++){
					if(lightningRenderers[i]==null){ lightningRenderers.RemoveAt(i); i-=1; continue; }
					lightningRenderers[i].positionCount = 1;
					lightningRenderers[i].startWidth = lightningRenderers[i].endWidth = lightningWidth;
				}
				if(lightningRenderers.Count==0) Debug.LogWarning("Lightning type shoot-object hasn't been assigned any LineRenderer");
				zapTimer = 0;
			}
		}
		
		
		void OnEnable(){
			if(trailList==null) return;
			for(int i=0; i<trailList.Count; i++) trailList[i].Clear();
		}
		
		
		//called by Unit to fire the shoot-object, all initial calculation for a shot goes here
		public void InitShoot(AttackInfo aInfo, Transform shootP=null){
			if(aInfo.tgtUnit==null){
				ObjectPoolManager.Unspawn(thisObj);
				return;
			}
			
			attackInfo=aInfo;
			InitShoot(aInfo.tgtUnit, shootP);
			
			//Debug.Log("InitShoot1  "+attachToShootPoint+"   "+shootPoint);
			//if(attachToShootPoint) thisT.parent=shootPoint;
		}
		public void InitShoot(Unit tUnit, Transform shootP=null){
			//Debug.Log("InitShoot2  "+attachToShootPoint+"   "+shootPoint);
			
			shootPoint=shootP;
			if(attachToShootPoint) thisT.parent=shootPoint;
			
			tgtUnit=tUnit;
			tgtRadius=tgtUnit.GetRadius();
			targetPos=tgtUnit.GetTargetPoint();
			shot=true;	hit=false; shootTime=Time.time;
			
			if(type==_Type.Projectile){
				if(!straightProjectile){
					//estimate the time taken to reach the target (roughly) and calculate the effective elevation based on falloffRange
					initialDist=Vector3.Distance(GetPos(), targetPos);
					initialPos=GetPos();
					
					if(!useTrajectoryCurve){
						eta=initialDist/speed;
						effElevation=elevation*Mathf.Clamp((initialDist-(falloffRange*.5f))/falloffRange, 0, 1);
					}
				}
				thisT.localRotation=Quaternion.identity;
			}
			else if(type==_Type.Beam){
				if(shootPoint!=null) thisT.parent=shootPoint;
			}
			else if(type==_Type.Effect){
				thisT.LookAt(targetPos);
			}
			else if(type==_Type.Lightning){
				if(shootPoint!=null) thisT.parent=shootPoint;
				zapTimer = timeOfZap;
				foreach(var lineRend in lightningRenderers) {
					lineRend.positionCount = 1;
				}
			}
			/*
			else if(type==_Type.Missile){
				float dist=Vector3.Distance(GetPos(), targetPos);
				eta=dist/speed;
				
				if(shootPoint!=null) thisT.rotation=shootPoint.rotation;
				
				float rotX=maxDeviation;//Random.Range(0, maxDeviation);
				float rotY=Random.value>.5f ? -maxDeviation : maxDeviation;	//Random.Range(-maxDeviation, maxDeviation);
				missileOffset=thisT.rotation*Quaternion.Euler(-rotX, rotY, 0)*Vector3.forward*dist*0.65f;
			}
			*/
			
			effectShoot.Spawn(GetPos(), GetRot());
			AudioManager.PlaySound(shootSound, GetPos());
		}
		
		
		private void UpdateTargetPos(){
			if(tgtUnit!=null) targetPos=tgtUnit.GetTargetPoint();
			else tgtRadius=0.1f;
		}
		
		void OnDrawGizmos(){
			Gizmos.DrawLine(GetPos(), targetPos);
		}
		
		void Update(){
			if(!shot || hit) return;
			
			if(tgtUnit!=null && tgtUnit.IsDestroyed()) tgtUnit=null;
			
			UpdateTargetPos();
			
			if(type==_Type.Projectile){
				if(straightProjectile){
					thisT.LookAt(targetPos);
					
					float dist=Vector3.Distance(targetPos+offsetPos, GetPos());
					
					if(dist>tgtRadius) thisT.Translate(thisT.forward*Mathf.Min(Time.deltaTime*speed, dist), Space.World);
					else Hit(GetPos());
				}
				else{
					if(useTrajectoryCurve){
						float totalDist=Vector3.Distance(initialPos, targetPos);
						float travelledDist=(Time.time-shootTime) * speed;
						
						//get the projected position in the straight line between the shootpoint and target point based on the far the projectile has moved
						Vector3 dummyPos=initialPos + (targetPos-initialPos).normalized * travelledDist;
						dummyPos+=new Vector3(0, trajectory.Evaluate(travelledDist/totalDist), 0);
						
						if(dummyPos!=GetPos()) thisT.rotation=Quaternion.LookRotation(dummyPos-GetPos());
						
						thisT.position=dummyPos;
						
						if(travelledDist>totalDist+.1f || Vector3.Distance(targetPos, GetPos())<tgtRadius) Hit(GetPos());
					}
					else{
						//calculate the offset position based on the shoot time and eta
						float t=Mathf.Min((Time.time-shootTime)/eta, 1);
						offsetPos=new Vector3(0, 1-(t), 0)*effElevation;
						
						Vector3 dir=(targetPos+offsetPos-GetPos()).normalized;
						thisT.LookAt(GetPos()+dir);
						
						float dist=Vector3.Distance(targetPos+offsetPos, GetPos());
						
						if(dist>tgtRadius) thisT.Translate(dir*Mathf.Min(Time.deltaTime*speed, dist), Space.World);
						else Hit(GetPos());
					}
				}
			}
			else if(type==_Type.Beam){
				float durRemain=Mathf.Clamp(beamDuration-(Time.time-shootTime), 0, beamDuration);
				
				if(durRemain<=0) Hit(ModifyTargetPosWithTgtRadius());
				else{
					for(int i=0; i<lines.Count; i++){
						linePos[0]=GetPos(); linePos[1]=ModifyTargetPosWithTgtRadius();
						lines[i].SetPositions(linePos.ToArray());
						lines[i].widthMultiplier = Mathf.Lerp(startWidth, 0, 1-durRemain/beamDuration);
						//lines[i].startWidth=Mathf.Lerp(startWidth, 0, 1-durRemain/beamDuration);
						//lines[i].endWidth=Mathf.Lerp(startWidth, 0, 1-durRemain/beamDuration);
					}
				}
			}
			else if(type==_Type.Effect){
				if(Time.time-shootTime>effectDuration){
					Hit(ModifyTargetPosWithTgtRadius());
				}
			}
			else if(type==_Type.Lightning){
				foreach(var lineRend in lightningRenderers) {
					if (zapTimer > 0) {
						Vector3 lastPoint = transform.position;
						int i = 1;
						lineRend.SetPosition(0, transform.position);
						Vector3 tPos = ModifyTargetPosWithTgtRadius();
						while (Vector3.Distance(tPos, lastPoint) > maxLgtDist) {
							lineRend.positionCount = i+1;
							Vector3 fwd = tPos - lastPoint;
							fwd.Normalize();
							fwd = Randomize(fwd, lightningInaccuracy);
							fwd *= Random.Range(arcLength * arcVariation, arcLength);
							fwd += lastPoint;
							lineRend.SetPosition(i, fwd);
							i++;
							lastPoint = fwd;
						}
						lineRend.positionCount = i + 1;
						lineRend.SetPosition(i, tPos);
						zapTimer = zapTimer - Time.deltaTime;
						if (zapTimer <= 0) {
							Hit(tPos);
						}
					} else {
						lineRend.positionCount = 1;
					}
				}
			}
			/*
			else if(type==_Type.Missile){
				float t=Mathf.Min((Time.time-shootTime)/eta, 1);
				Vector3 offset=Vector3.Slerp(missileOffset, Vector3.zero, t);
				
				thisT.LookAt(targetPos+offset);
				
				float dist=Vector3.Distance(targetPos+offset, GetPos());
				
				if(dist>tgtRadius) thisT.Translate(Vector3.forward*Mathf.Min(Time.deltaTime*speed, dist), Space.Self);
				else Hit(GetPos());
			}
			*/
		}
		
		void Hit(Vector3 hitPos){
			if(hit) return;
			hit=true;
			
			effectHit.Spawn(hitPos, Quaternion.identity);
			AudioManager.PlaySound(hitSound, hitPos);
			
			if(attackInfo!=null && tgtUnit!=null){
				tgtUnit.ApplyAttack(attackInfo);
				tgtUnit=null;	attackInfo=null;
			}
			
			ObjectPoolManager.Unspawn(thisObj);
		}
		
		
		Vector3 ModifyTargetPosWithTgtRadius(){
			Vector3 dir=(GetPos()-targetPos).normalized;
			return targetPos+dir*tgtRadius;
		}
		
		
		public float GetElevationAngle(Vector3 sPos, Vector3 tPos){
			if(type!=_Type.Projectile || straightProjectile) return 0;
			
			if(useTrajectoryCurve){
				//float p1=trajectory.Evaluate(0);
				//float p2=trajectory.Evaluate(0.5f);
				//float trajectoryElevation=-Mathf.Atan(p2-p1)*Mathf.Rad2Deg;
				
				return -elevationT;
			}
			
			float dist=Vector3.Distance(sPos, tPos);
			float elev=elevation*Mathf.Clamp((dist-(falloffRange*.5f))/falloffRange, 0, 1);
			return -Mathf.Atan(elev/dist)*Mathf.Rad2Deg;
		}
		
		private Vector3 Randomize(Vector3 newVector, float deviation) {
			newVector += new Vector3(Random.Range(-1.0f * randomRange, randomRange)
				, Random.Range(-1.0f * randomRange, randomRange)
				, Random.Range(-1.0f * randomRange, randomRange)) 
				* deviation;
			newVector.Normalize();
			return newVector;
		}
		
	}
	
}