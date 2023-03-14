using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace TDTK {

	public class TDEditorWindow : EditorWindow{
		
		public Color white=Color.white;
		public Color grey=Color.grey;
		
		protected void CheckColorE(int value, int TH){ GUI.color=(value>=TH ? GUI.color : grey); }
		protected void CheckColor(int value, int TH){ GUI.color=(value>TH ? GUI.color : grey); }
		protected void ResetColor(){ GUI.color=Color.white; }
		
		protected float contentHeight=0;
		protected float contentWidth=0;
		
		protected Vector2 scrollPos;
		
		protected GUIContent cont;
		protected GUIContent[] contL;
			
		protected int spaceX=120;
		protected int spaceY=18;
		protected int width=150;
		protected int widthS=40;
		protected int height=16;
		
		protected void ResetDimension(){ spaceX=120; spaceY=18; width=150; widthS=40; height=16; }
		
		
		protected bool CheckIsPlaying(){
			ResetDimension();
			
			if(Application.isPlaying){
				EditorGUILayout.HelpBox("Cannot edit while game is playing", MessageType.Info);
				return false;
			}
			
			return true;
		}
		
		
		protected float DrawVisualObject(float startX, float startY, VisualObject vo, string label, string tooltip, bool showAutoDestroy=true){
			TDE.Label(startX, startY, width, height, label, tooltip);
			vo.obj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), vo.obj, typeof(GameObject), true);
			
			if(showAutoDestroy){
				TDE.Label(startX, startY+=spaceY, width, height, " - Auto Destroy:", "Check if the spawned effect should be destroyed automatically");
				if(vo.obj!=null) vo.autoDestroy=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), vo.autoDestroy);
				else TDE.Label(startX+spaceX, startY, width, height, "-");
			}
			
			TDE.Label(startX, startY+=spaceY, width, height, " - Effect Duration:", "How long before the spawned effect object is destroyed");
			if(vo.obj!=null && vo.autoDestroy) vo.duration=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, width, height), vo.duration);
			else TDE.Label(startX+spaceX, startY, width, height, "-");
			
			return startY;
		}
		
		
		private static bool initLabel=false;
		private static string[] targetModeLabel;
		private static string[] targetModeTooltip;
		
		private static void InitLabel(){
			if(initLabel) return;
			initLabel=true;
			
			int enumLength = Enum.GetValues(typeof(Unit._TargetMode)).Length;
			targetModeLabel=new string[enumLength];
			targetModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				targetModeLabel[i]=((Unit._TargetMode)i).ToString();
				if((Unit._TargetMode)i==Unit._TargetMode.NearestToDestination) 	targetModeTooltip[i]="Pick the target that is nearest to destination\nN/A for creep so Random will be used instead";
				if((Unit._TargetMode)i==Unit._TargetMode.NearestToSelf) 		targetModeTooltip[i]="Pick the target that is nearest to the unit itself";
				if((Unit._TargetMode)i==Unit._TargetMode.MostHP) 					targetModeTooltip[i]="Pick the target that has the most hp and shield";
				if((Unit._TargetMode)i==Unit._TargetMode.LeastHP) 				targetModeTooltip[i]="Pick the target that has least HP and shield";
				if((Unit._TargetMode)i==Unit._TargetMode.Random) 				targetModeTooltip[i]="No particular preference";
			}
		}
		
		
		//private static bool shootPointFoldout=false;
		private bool foldAnimation=false;
		//protected float DrawTowerVisualEffect(float startX, float startY, UnitTower tower){
			
		protected float DrawUnitAnimation(float startX, float startY, Unit item){
			string textF="Animation Setting ";//+(!foldVisual ? "(show)" : "(hide)");
			foldAnimation=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldAnimation, textF, TDE.foldoutS);
			if(!foldAnimation) return startY;
			
			startX+=12;
			
			int objIdx=GetIndexFromHierarchy(item.animatorT, objHierarchyList);
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Animator Object:", "The transform object which contain the Animator component");
			objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
			item.animatorT = objHierarchyList[objIdx];
			
			startY+=10;
			
			TDE.Label(startX, startY+=spaceY, width, height, "Clip Idle:", "The animation clip to play when the unit is idle");
			item.clipIdle=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipIdle, typeof(AnimationClip), true);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Clip Hit:", "The animation clip to play when the unit is hit by an attack");
			item.clipHit=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipHit, typeof(AnimationClip), true);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Clip Destroyed:", "The animation clip to play when the unit is destroyed");
			item.clipDestroyed=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipDestroyed, typeof(AnimationClip), true);
			
			TDE.Label(startX, startY+=spaceY, width, height, "Clip Attack:", "The animation clip to play when the unit attacks");
			item.clipAttack=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipAttack, typeof(AnimationClip), true);
			
			TDE.Label(startX, startY+=spaceY, width, height, " - delay:", "Unit's active target field-of-view in angle");
			item.animationAttackDelay=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.animationAttackDelay);
			
			startY+=10;
			
			if(item.IsCreep()){
				TDE.Label(startX, startY+=spaceY, width, height, "Clip Move:", "The animation clip to play when the unit is moving");
				item.clipMove=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipMove, typeof(AnimationClip), true);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Clip Spawn:", "The animation clip to play when the unit is spawned");
				item.clipSpawn=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipSpawn, typeof(AnimationClip), true);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Clip Destination:", "The animation clip to play when the unit reaches destination");
				item.clipDestination=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipDestination, typeof(AnimationClip), true);
			}
			if(item.IsTower()){
				TDE.Label(startX, startY+=spaceY, width, height, "Clip Construct:", "The animation clip to play when the unit is constructing");
				item.clipConstruct=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipConstruct, typeof(AnimationClip), true);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Clip Deconstruct:", "The animation clip to play when the unit is deconstructing");
				item.clipDeconstruct=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.clipDeconstruct, typeof(AnimationClip), true);
			}
		
			return startY;
		}
		
		

		
		
		//private static bool foldUnit=false;
		private static bool shootPointFoldout=false;
		protected float DrawUnitSetting(float startX, float startY, Unit item){
			InitLabel();
			
			//string text="Unit Setting" + (foldUnit ? "(Hide)" : "(Show)");
			//foldUnit=EditorGUI.Foldout(new Rect(startX, startY, spaceX, height), foldUnit, "Unit Setting", TDE.foldoutS);
			//if(!foldUnit) return startY;
			
			startX+=12;
			
			if(item.IsTurret()){
				int tgtMode=(int)item.targetMode;
				cont=new GUIContent("Targeting Mode:", "How the unit select target from a group of potential hostile");
				contL=TDE.SetupContL(targetModeLabel, targetModeTooltip);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				tgtMode = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), tgtMode, contL);
				item.targetMode=(UnitTower._TargetMode)tgtMode;
				
				TDE.Label(startX, startY+=spaceY, width, height, " - Reset On Attack:", "Check to force the unit to look for the most viable target on every attack (more expensive)");
				item.resetTargetOnAttack=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.resetTargetOnAttack);
				
				GUI.color=(item.targetingFov<=0 || item.targetingFov>=360) ? grey : white;
				TDE.Label(startX, startY+=spaceY, width, height, "Targeting FOV:", "Unit's active target field-of-view in angle");
				item.targetingFov=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.targetingFov);	GUI.color=white;
				item.targetingFov=Mathf.Clamp(item.targetingFov, 0, 360);
				
				startY+=5;
				
				TDE.Label(startX, startY+=spaceY, width, height, "Target Per Attack:", "The number of target the unit can shoot at in each attack");
				item.targetCountPerAttack=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), item.targetCountPerAttack);
				
				item.targetCountPerAttack=Mathf.Max(1, item.targetCountPerAttack);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Use LOS Targeting:", "Check to use Line-of-Sight targeting. Unit can't target a hostile unit behind a collider. (more expensive)");
				item.useLOSTargeting=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.useLOSTargeting);
				
				startY+=10;
			}
			
			
			int objIdx=GetIndexFromHierarchy(item.targetPoint, objHierarchyList);
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "TargetPoint:", "The transform object which indicate the center point of the unit\nThis would be the point where the shootObject and effect will be aiming at");
			objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
			item.targetPoint = objHierarchyList[objIdx];
			
			TDE.Label(startX, startY+=spaceY, width, height, "Unit Radius:", "A reference value to indicate how 'big' the target is, used to determine when a shootobject hit the unit");
			item.unitRadius=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.unitRadius);
			
			
			if(item.IsAOE()){
				startY=DrawVisualObject(startX, startY+=spaceY+10, item.shootEffObj_AOE, "Shoot Effect (AOE):", "", false);
				item.shootEffObj_AOE.autoDestroy=true;
			}
			if(item.IsMine()){
				startY=DrawVisualObject(startX, startY+=spaceY+10, item.shootEffObj_Mine, "Shoot Effect (Mine):", "", false);
				item.shootEffObj_Mine.autoDestroy=true;
			}
			if(item.IsResource()){
				startY=DrawVisualObject(startX, startY+=spaceY+10, item.shootEffObj_Resource, "Shoot Effect (Rsc):", "", false);
				item.shootEffObj_Resource.autoDestroy=true;
			}
			if(item.IsTurret()){
				startY+=10;
				
				GUIStyle style=item.shootObject==null ? TDE.conflictS : null;
				TDE.Label(startX, startY+=spaceY, width, height, "Shoot Object:", "The shoot-object to fire at each attack. Must be a prefab contain a 'ShootObject' component", style);
				item.shootObject=(ShootObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.shootObject, typeof(ShootObject), true);
			}
			
			if(item.IsTurret() || item.IsAOE() || item.IsResource() || item.IsMine()){
				startY+=10;
				
				cont=new GUIContent("ShootPoint:", "OPTIONAL - The transform which indicate the position where the Shoot-Object/Fire-effect will be fired from\nEach shootPoint assigned will fire a shootObject instance in each attack\nIf left empty, the unit transform itself will be use as the shootPoint\nThe orientation of the shootPoint matter as they dictate the orientation of the shootObject starting orientation.\n");
				shootPointFoldout=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), shootPointFoldout, cont);
				int shootPointCount=item.shootPoint.Count;
				shootPointCount=EditorGUI.DelayedIntField(new Rect(startX+spaceX, startY, widthS, height), shootPointCount);
				
				if(shootPointCount!=item.shootPoint.Count){
					while(item.shootPoint.Count<shootPointCount) item.shootPoint.Add(null);
					while(item.shootPoint.Count>shootPointCount) item.shootPoint.RemoveAt(item.shootPoint.Count-1);
				}
					
				if(shootPointFoldout){
					for(int i=0; i<item.shootPoint.Count; i++){
						objIdx=GetIndexFromHierarchy(item.shootPoint[i], objHierarchyList);
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "    - Element "+(i+1));
						objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
						item.shootPoint[i] = objHierarchyList[objIdx];
					}
				}
			
				TDE.Label(startX, startY+=spaceY, width, height, "ShootPoint Spacing:", "The time delay in second between each shoot-point during an attack");
				if(item.shootPoint.Count<=1) TDE.Label(startX+spaceX, startY, widthS, height, "-", "");
				else item.shootPointSpacing=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.shootPointSpacing);
			
				if(item.IsTurret()){
				
					startY+=10;
				
					TDE.Label(startX, startY+=spaceY, width, height, "Turret Pivot:", "OPTIONAL - The object under unit's hierarchy which is used to aim toward target\nWhen left unassigned, no aiming will be done.");
					objIdx = GetIndexFromHierarchy(item.turretPivot, objHierarchyList);
					objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
					item.turretPivot = objHierarchyList[objIdx];
					
					TDE.Label(startX, startY+=spaceY, width, height, "Barrel Pivot:", "OPTIONAL - The secondary object under unit's hierarchy which is used to aim toward target in x-axis only\nWhen left unassigned, no aiming will be done.");
					if(item.turretPivot==null) EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					else{
						objIdx = GetIndexFromHierarchy(item.barrelPivot, objHierarchyList);
						objIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objIdx, objHierarchylabel);
						item.barrelPivot = objHierarchyList[objIdx];
					}
					
					TDE.Label(startX, startY+=spaceY, width, height, "Aim In X-Axis:", "Check to have the turret aim in x-axis");
					if(item.turretPivot==null) EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					else item.aimInXAxis=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.aimInXAxis);
					
					TDE.Label(startX, startY+=spaceY, width, height, "Snap Aiming:", "Check to have the turret look at target instantly");
					if(item.turretPivot==null) TDE.Label(startX+spaceX, startY, widthS, height, "-");
					else item.snapAiming=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.snapAiming);
					
					TDE.Label(startX, startY+=spaceY, width, height, "Aim Speed:", "");
					if(item.turretPivot==null || item.snapAiming) TDE.Label(startX+spaceX, startY, widthS, height, "-");
					else item.aimSpeed=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.aimSpeed);
					
				}
			}
			
			return startY;
		}
		
		
		
		#region draw stats
		public enum _EType{ 
			//TTurret, TAOE, TSupport, TRsc, TMine, TMineCD, TBlock, 
			Tower, 
			Creep,
			//CDefault, CTurret, CAOE, CSupport, CSpawner, 
			Ability, 
			PerkT, PerkA, PerkE, 
			Effect,
		}
		
		
		public static bool IsTower(_EType type){ return type==_EType.Tower; }
		public static bool IsCreep(_EType type){ return type==_EType.Creep; }
		public static bool IsAbility(_EType type){ return type==_EType.Ability; }
		public static bool IsPerk(_EType type){ return (int)type>=(int)_EType.PerkT; }
		public static bool IsPerkT(_EType type){ return type==_EType.PerkT; }
		public static bool IsPerkA(_EType type){ return type==_EType.PerkA; }
		public static bool IsPerkE(_EType type){ return type==_EType.PerkE; }
		public static bool IsEffect(_EType type){ return type==_EType.Effect || type==_EType.PerkE; }
		
		
		//private static bool fold=false;
		public static float DrawStats(float startX, float startY, Stats item, _EType type, bool compressWidth=false, bool turret=false, bool aoe=false, bool support=false, bool resource=false, bool mine=false){
			int spaceX=120; int spaceY=18; int width=150; int widthS=40; int height=16; 
			int widthL=compressWidth ? 2+widthS*2 : width;
			
			//Debug.Log(turret+"  "+aoe+"  "+support+"  "+resource+"  "+mine);
			
			//string text="Stats "+(!fold ? "(show)" : "(hide)");
			//fold=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), fold, text, foldoutS);
			//if(fold) return startY;
			
			startX+=12;
			
			bool start=true;
			
			if(IsTower(type) || IsPerk(type)){	//cost for ability is drawn separately in AbilityEditor
				TDE.Label(startX, startY, width, height, "Cost (Rsc):", "cost of the item");
				
				RscManager.MatchRscList(item.cost, IsPerk(type) ? 1 : 0);
				
				float cachedX=startX;
				for(int i=0; i<Rsc_DB.GetCount(); i++){
					if(i>0 && i%2==0){ startX=cachedX-widthS-2; startY+=spaceY; }	if(i>0) startX+=widthS+2;
					TDE.DrawSprite(new Rect(startX+spaceX, startY, height, height), Rsc_DB.GetIcon(i), Rsc_DB.GetName(i));
					item.cost[i]=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+height, startY, widthS-height, height), item.cost[i]);
				}
				startX=cachedX;
				
				
				if(IsTower(type)){
					TDE.Label(startX, startY+=spaceY, width, height, "Sell Value (Rsc):", "sell value of the item");
					
					RscManager.MatchRscList(item.sellValue, IsPerk(type) ? 1 : 0);
					
					cachedX=startX;
					for(int i=0; i<Rsc_DB.GetCount(); i++){
						if(i>0 && i%2==0){ startX=cachedX-widthS-2; startY+=spaceY; }	if(i>0) startX+=widthS+2;
						TDE.DrawSprite(new Rect(startX+spaceX, startY, height, height), Rsc_DB.GetIcon(i), Rsc_DB.GetName(i));
						item.sellValue[i]=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+height, startY, widthS-height, height), item.sellValue[i]);
					}
					startX=cachedX;
				}
				
				
				if(!IsPerkA(type)){
					TDE.Label(startX, startY+=spaceY, width, height, "Build Duration:", "");
					item.buildDuration=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.buildDuration);
					TDE.Label(startX, startY+=spaceY, width, height, "Sell Duration:", "");
					item.sellDuration=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.sellDuration);
				}
				
				start=false;
			}
			
			if(IsEffect(type)){	
				if(!start) startY+=spaceY+5;
				
				if(!IsPerkE(type)){
					int damageType=(int)item.damageType;
					TDE.Label(startX, startY, width, height, "Damage Type:", "");
					damageType = EditorGUI.Popup(new Rect(startX+spaceX, startY, widthL, height), damageType, TDE.GetDamageLabel());
					item.damageType=damageType;
					
					startY+=spaceY;
				}
				
				TDE.Label(startX, startY, width, height, "HitPoint Rate:", "hit-point generation/degeneration per second");
				item.hpRate=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.hpRate);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Shield Rate:", "*shield generation/degeneration per second\nDoes not subject to stagger");
				item.shRate=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.shRate);
				
				
				start=false;
			}
			
			
			if(IsTower(type) || IsCreep(type) || IsPerkT(type) || IsEffect(type)){
				if(IsTower(type) || IsPerkT(type) || IsEffect(type)) startY+=spaceY;
				
				if(!start) startY+=5;
				
				TDE.Label(startX, startY, width, height, "HitPoint:", "");
				item.hp=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.hp);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Shield:", "");
				item.sh=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.sh);
				
				bool switchColor=false;
				if(item.sh<=0 && GUI.color!=Color.grey){ GUI.color=Color.grey; switchColor=true; }
					TDE.Label(startX, startY+=spaceY, width, height, " - Regen Rate:", "shield regeneration per second");
					if(item.sh>0) item.shRegen=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.shRegen);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					TDE.Label(startX, startY+=spaceY, width, height, " - Stagger Duration:", "shield regeneration will stop for this duration when the unit is hit");
					if(item.sh>0) item.shStagger=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.shStagger);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				if(switchColor) GUI.color=Color.white;
				
				start=false;
			}
			
			
			if(IsTower(type) || IsCreep(type) || IsAbility(type)){
				if(!start) startY+=5;
				
				int armorType=(int)item.armorType;	int damageType=(int)item.damageType;
				if(!IsAbility(type)){
					TDE.Label(startX, startY+=spaceY, width, height, "Armor Type:", "");
					armorType = EditorGUI.Popup(new Rect(startX+spaceX, startY, widthL, height), armorType, TDE.GetArmorLabel());
					item.armorType=armorType;	
				}
				
				if(IsAbility(type) || turret || aoe || mine){
					TDE.Label(startX, startY+=spaceY, width, height, "Damage Type:", "");
					damageType = EditorGUI.Popup(new Rect(startX+spaceX, startY, widthL, height), damageType, TDE.GetDamageLabel());
					item.damageType=damageType;
				}
				else{
					TDE.Label(startX, startY+=spaceY, width, height, "Damage Type:", "");
					TDE.Label(startX+spaceX, startY, widthL, height, "-", "");
				}
			}
			
			
			if(IsEffect(type) || IsCreep(type)){
				TDE.Label(startX, startY+=spaceY+5, width, height, "Move Speed:", "");
				item.speed=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.speed);
			}

			
			if(IsEffect(type) || IsTower(type) || IsCreep(type) || IsPerkT(type)){
				startY+=5;
				
				TDE.Label(startX, startY+=spaceY, width, height, "Dodge Chance:", "How likely will the unit dodge an attack\n"+txtTooltipChance);
				item.dodge=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.dodge);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Critical Reduc.:", "How likely will the unit negate a cirtical attack\n"+txtTooltipChance);
				item.critReduc=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critReduc);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Damage Reduc.:", "How much incoming damage will be reduced\n"+txtTooltipChance);
				item.dmgReduc=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.dmgReduc);
			}
			
			
			
			startY+=5;
			
			string tab=" - ";
			
			bool showTurret=IsEffect(type) | IsPerk(type) | turret | IsAbility(type);
			
			if(showTurret){
				if(!IsPerkA(type) & !IsAbility(type)){
					item.editTurret=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), item.editTurret, "Turret Attack Stats");//, TDE.foldoutS);
				}
				else tab="";
				
				//TDE.Label(startX, startY+=spaceY, width, height, "Turret Stats", "");//, TDE.headerS);
				//item.editTurret=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.editTurret);
				//if(item.editTurret) turret=true;
			
				if(item.editTurret || IsPerkA(type) || IsAbility(type)){
					//TDE.Label(startX, startY+=spaceY+5, width, height, "Turret Stats", "", TDE.headerS);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Damage Min/Max:");
					item.damageMin=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.damageMin);
					item.damageMax=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), item.damageMax);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Cooldown:", "");
					item.cooldown=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cooldown);
					
					if(!IsAbility(type) && !IsPerkA(type)){
						TDE.Label(startX, startY+=spaceY, width, height, tab+"Effective Radius:", "");
						item.attackRange=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.attackRange);
					}
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"AOE Radius:", "");
					item.aoeRange=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.aoeRange);
					
					startY+=5;
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Hit Chance:", "How likely will an attack will hit\n"+txtTooltipChance);
					item.hit=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.hit);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Critical Chance:", "How likely will an attack will score critical hit, applying critical multiplier to the damage cause\n"+txtTooltipChance);
					item.critChance=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critChance);
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Critical Multiplier:", "The multiplier apply to the damage when an attack crits");
					item.critMultiplier=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critMultiplier);
					
					startY+=5;
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Effect Hit Chance:", "How likely will an attack will hit\n"+txtTooltipChance+"\n\nPlease note that changes of unit 'Effect On Hit' only applies when the effect is applied by perk to unit");
					if(!IsPerk(type) && (item.effectOnHitIDList.Count<=0 || item.effectOnHitChance<=0)) GUI.color=Color.grey;
					item.effectOnHitChance=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.effectOnHitChance); 	GUI.color=Color.white;
					
					startY=DrawEffectOnHit(startX, startY+spaceY, tab, item.effectOnHitIDList, item.effectOnHitChance, compressWidth);
					
					if(IsPerk(type)){
						TDE.Label(startX, startY+=spaceY, width, height, "    Override Existing:", "Check to override the target existing effects\nOtherwise the new effect will be appended to the existing effects"+"\n\nPlease note that changes of unit 'Effect On Hit' only applies when the effect is applied by perk to unit");
						item.overrideExistingEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.overrideExistingEffect);
					}
					
					startY+=5;
				}
			}
			
			
			bool showAOE=IsEffect(type) | IsPerkT(type) | IsPerkE(type) | aoe;
			
			if(showAOE){
				item.editAOE=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), item.editAOE, "AOE Attack Stats");
				//TDE.Label(startX, startY+=spaceY, width, height, "AOE Stats", "");//, TDE.headerS);
				//item.editAOE=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.editAOE);
				//if(item.editAOE) aoe=true;
			
				if(item.editAOE){
					//TDE.Label(startX, startY+=spaceY+5, width, height, "AOE Stats", "", TDE.headerS);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Damage Min/Max:");
					item.damageMin_AOE=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.damageMin_AOE);
					item.damageMax_AOE=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), item.damageMax_AOE);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Cooldown:", "");
					item.cooldown_AOE=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cooldown_AOE);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Effective Radius:", "");
					item.attackRange_AOE=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.attackRange_AOE);
					
					startY+=5;
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Hit Chance:", "How likely will an attack will hit\n"+txtTooltipChance);
					item.hit_AOE=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.hit_AOE);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Critical Chance:", "How likely will an attack will score critical hit, applying critical multiplier to the damage cause\n"+txtTooltipChance);
					item.critChance_AOE=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critChance_AOE);
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Critical Multiplier:", "The multiplier apply to the damage when an attack crits");
					item.critMultiplier_AOE=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critMultiplier_AOE);
					
					startY+=5;
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Effect Hit Chance:", "How likely will an attack will hit\n"+txtTooltipChance+"\n\nPlease note that changes of unit 'Effect On Hit' only applies when the effect is applied by perk to unit");
					if(item.effectOnHitIDList_AOE.Count<=0 || item.effectOnHitChance_AOE<=0) GUI.color=Color.grey;
					item.effectOnHitChance_AOE=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.effectOnHitChance_AOE); 	GUI.color=Color.white;
					
					startY=DrawEffectOnHit(startX, startY+spaceY, tab, item.effectOnHitIDList_AOE, item.effectOnHitChance_AOE, compressWidth);
					
					if(IsPerk(type)){
						TDE.Label(startX, startY+=spaceY, width, height, "    Override Existing:", "Check to override the target existing effects\nOtherwise the new effect will be appended to the existing effects"+"\n\nPlease note that changes of unit 'Effect On Hit' only applies when the effect is applied by perk to unit");
						item.overrideExistingEffect_AOE=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.overrideExistingEffect_AOE);
					}
					
					startY+=5;
				}
			}
			
			
			bool showMine=IsEffect(type) | IsPerkT(type) | IsPerkE(type) | mine;
			
			if(showMine){
				item.editMine=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), item.editMine, "Mine Attack Stats");
				//TDE.Label(startX, startY+=spaceY, width, height, "Mine Stats", "");//, TDE.headerS);
				//item.editMine=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.editMine);
				//if(item.editMine) mine=true;
			
				if(item.editMine){
					//TDE.Label(startX, startY+=spaceY+5, width, height, "Mine Stats", "", TDE.headerS);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Damage Min/Max:");
					item.damageMin_Mine=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.damageMin_Mine);
					item.damageMax_Mine=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), item.damageMax_Mine);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Cooldown:", "");
					item.cooldown_Mine=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cooldown_Mine);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"AOE Radius:", "");
					item.aoeRange_Mine=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.aoeRange_Mine);
					
					startY+=5;
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Hit Chance:", "How likely will an attack will hit\n"+txtTooltipChance);
					item.hit_Mine=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.hit_Mine);
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Critical Chance:", "How likely will an attack will score critical hit, applying critical multiplier to the damage cause\n"+txtTooltipChance);
					item.critChance_Mine=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critChance_Mine);
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Critical Multiplier:", "The multiplier apply to the damage when an attack crits");
					item.critMultiplier_Mine=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.critMultiplier_Mine);
					
					startY+=5;
					
					TDE.Label(startX, startY+=spaceY, width, height, tab+"Effects HitChance:", "How likely will an attack will hit\n"+txtTooltipChance+"\n\nPlease note that changes of unit 'Effect On Hit' only applies when the effect is applied by perk to unit");
					if(item.effectOnHitIDList_Mine.Count<=0 || item.effectOnHitChance_Mine<=0) GUI.color=Color.grey;
					item.effectOnHitChance_Mine=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.effectOnHitChance_Mine); 	GUI.color=Color.white;
					
					startY=DrawEffectOnHit(startX, startY+spaceY, tab, item.effectOnHitIDList_Mine, item.effectOnHitChance_Mine, compressWidth);
					
					if(IsPerk(type)){
						TDE.Label(startX, startY+=spaceY, width, height, "    Override Existing:", "Check to override the target existing effects\nOtherwise the new effect will be appended to the existing effects"+"\n\nPlease note that changes of unit 'Effect On Hit' only applies when the effect is applied by perk to unit");
						item.overrideExistingEffect_Mine=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.overrideExistingEffect_Mine);
					}
					
					startY+=5;
				}
			}
			
			
			bool showSupport=IsEffect(type) | IsPerkT(type) | support;
			
			if(showSupport){
				if(showTurret || showAOE || showMine) startY+=8;
				
				TDE.Label(startX, startY+=spaceY, width, height, "Support Radius:", "");
				item.attackRange_Support=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.attackRange_Support);
				
				TDE.Label(startX, startY+=spaceY, width, height, "Support Effects", "");//, TDE.headerS);
				if(compressWidth) startY+=spaceY;
				startY=DrawEffectOnHit(startX, startY, tab, item.effectOnHitIDList_Support, 1, compressWidth);
				
				if(IsPerk(type)){
					TDE.Label(startX, startY+=spaceY, width, height, "    Override Existing:", "Check to override the target existing effects\nOtherwise the new effect will be appended to the existing effects");
					item.overrideExistingEffect_Support=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.overrideExistingEffect_Support);
				}
				
				startY+=5;
			}
			
			
			bool showRsc=IsEffect(type) | IsPerkT(type) | resource;
			
			if(showRsc){
				TDE.Label(startX, startY+=spaceY, width, height, "Resource Stats", "");//, TDE.headerS);
				
				TDE.Label(startX, startY+=spaceY, width, height, tab+"Rsc. Gain:", "");
				
				RscManager.MatchRscList(item.rscGain, (IsEffect(type) || IsPerk(type) ? 1 : 0));
				
				float cachedX=startX;
				for(int i=0; i<Rsc_DB.GetCount(); i++){
					if(i>0 && i%2==0){ startX=cachedX; startY+=spaceY; }	if(i>0) startX+=widthS+2;
					TDE.DrawSprite(new Rect(startX+spaceX, startY, height, height), Rsc_DB.GetIcon(i), Rsc_DB.GetName(i));
					item.rscGain[i]=EditorGUI.DelayedFloatField(new Rect(startX+spaceX+height, startY, widthS-height, height), item.rscGain[i]);
				}
				startX=cachedX;
				
				TDE.Label(startX, startY+=spaceY, width, height, tab+"Rsc. Cooldown:", "");
				item.cooldown_Rsc=EditorGUI.DelayedFloatField(new Rect(startX+spaceX, startY, widthS, height), item.cooldown_Rsc);
			}
			
			
			
			return startY+spaceY;
		}
		
		private static float DrawEffectOnHit(float startX, float startY, string tab, List<int> effectOnHitIDList, float chance, bool compressWidth){
			int spaceX=120; int spaceY=18; int width=150; int height=16; //int widthS=40; 
			
			if(chance<=0) GUI.color=Color.grey;
			
			if(compressWidth) startX+=35;
			else startX+=spaceX;
		
			for(int i=0; i<effectOnHitIDList.Count; i++){
				TDE.Label(startX-20, startY, width, height, " - ");
				
				int effIdx=Effect_DB.GetPrefabIndex(effectOnHitIDList[i]);		bool removeEff=false;
				
				effIdx = EditorGUI.Popup(new Rect(startX, startY, width, height), effIdx, Effect_DB.label);
				if(GUI.Button(new Rect(startX+width+3, startY, height, height), "-")){ effectOnHitIDList.RemoveAt(i); removeEff=true; }
				
				if(effIdx>=0 && !removeEff) effectOnHitIDList[i]=Effect_DB.GetItem(effIdx).prefabID;
				
				startY+=spaceY;
			}
			
			int newEffID=-1;
			TDE.Label(startX-20, startY, width, height, " - ");
			newEffID = EditorGUI.Popup(new Rect(startX, startY, width, height), newEffID, Effect_DB.label);
			
			if(newEffID>=0) newEffID=Effect_DB.GetItem(newEffID).prefabID;
			if(newEffID>=0 && !effectOnHitIDList.Contains(newEffID)) effectOnHitIDList.Add(newEffID);
			
			GUI.color=Color.white;
			
			return startY;
		}			
		
		
		private static string txtTooltipChance="\nTakes value from 0-1 with 0.3 being 30%, 0.75 being 75% and so on...";
		
		#endregion
		
		
		
		
		
		
		
		#region list
		protected virtual void SelectItem(){}
		protected virtual void DeleteItem(){}
		protected virtual void ShiftItemUp(){}
		protected virtual void ShiftItemDown(){}
		
		protected bool minimiseList=false;
		protected Rect visibleRectList;
		protected Rect contentRectList;
		protected Vector2 scrollPosList;
		
		public int deleteID=-1;
		public int selectID=0;
		public int newSelectID;
		public int newSelectDelay;
			
		protected void Select(int ID){
			if(selectID==ID) return;
			
			newSelectID=ID;
			newSelectDelay=5;
			
			if(selectID*35<scrollPosList.y) scrollPosList.y=selectID*35;
			if(selectID*35>scrollPosList.y+visibleRectList.height-40) scrollPosList.y=selectID*35-visibleRectList.height+40;
			
			SelectItem();
		}
		
		//~ protected void Select(int ID){
			//~ if(selectID==ID) return;
			
			//~ selectID=ID;
			//~ GUI.FocusControl("");
			
			//~ if(selectID*35<scrollPosList.y) scrollPosList.y=selectID*35;
			//~ if(selectID*35>scrollPosList.y+visibleRectList.height-40) scrollPosList.y=selectID*35-visibleRectList.height+40;
			
			//~ SelectItem();
		//~ }
		
		protected Vector2 DrawList(float startX, float startY, float winWidth, float winHeight, List<EItem> list, bool drawRemove=true, bool shiftItem=true, bool clampSelectID=true){
			float width=minimiseList ? 60 : 260;
			
			if(!minimiseList && shiftItem){
				if(GUI.Button(new Rect(startX+180, startY-20, 40, 18), "up")){
					ShiftItemUp();
					if(selectID*35<scrollPosList.y) scrollPosList.y=selectID*35;
				}
				if(GUI.Button(new Rect(startX+222, startY-20, 40, 18), "down")){
					ShiftItemDown();	
					if(visibleRectList.height-35<selectID*35) scrollPosList.y=(selectID+1)*35-visibleRectList.height+5;
				}
			}
			
			visibleRectList=new Rect(startX, startY, width+15, winHeight-startY-5);
			contentRectList=new Rect(startX, startY, width, list.Count*35+5);
			
			GUI.color=new Color(.8f, .8f, .8f, 1f);
			GUI.Box(visibleRectList, "");
			GUI.color=Color.white;
			
			scrollPosList = GUI.BeginScrollView(visibleRectList, scrollPosList, contentRectList);
			
				startY+=5;	startX+=5;
			
				for(int i=0; i<list.Count; i++){
					
					TDE.DrawSprite(new Rect(startX, startY+(i*35), 30, 30), list[i].icon);
					
					if(minimiseList){
						if(selectID==i) GUI.color = new Color(0, 1f, 1f, 1f);
						if(GUI.Button(new Rect(startX+35, startY+(i*35), 30, 30), "")) Select(i);
						GUI.color = Color.white;
						continue;
					}
					
					if(selectID==i) GUI.color = new Color(0, 1f, 1f, 1f);
					if(GUI.Button(new Rect(startX+35, startY+(i*35), 150+(!drawRemove ? 60 : 0), 30), list[i].name)) Select(i);
					GUI.color = Color.white;
					
					if(!drawRemove) continue;
					
					if(deleteID==i){
						if(GUI.Button(new Rect(startX+190, startY+(i*35), 60, 15), "cancel")) deleteID=-1;
						
						GUI.color = Color.red;
						if(GUI.Button(new Rect(startX+190, startY+(i*35)+15, 60, 15), "confirm")){
							if(selectID>=deleteID) Select(Mathf.Max(0, selectID-1));
							DeleteItem();	deleteID=-1;
						}
						GUI.color = Color.white;
					}
					else{
						if(GUI.Button(new Rect(startX+190, startY+(i*35), 60, 15), "remove")) deleteID=i;
					}
				}
			
			GUI.EndScrollView();
			
			if(clampSelectID) selectID=Mathf.Clamp(selectID, 0, list.Count-1);
			
			return new Vector2(startX+width+10, startY);
		}
		#endregion
		
		
		
		
		
		
		
		#region setup hierarchy list
		protected static int GetIndexFromHierarchy(Transform objT, Transform[] objHList){
			if(objT==null) return 0;
			for(int i=1; i<objHList.Length; i++){ if(objT==objHList[i]) return i; }
			return 0;
		}
		
		
		protected Transform[] objHierarchyList=new Transform[0];
		protected string[] objHierarchylabel=new string[0];
	
		protected void UpdateObjHierarchyList(Transform objT){//, SetObjListCallback callback){
			List<Transform> objHList=new List<Transform>();
			List<string> objNList=new List<string>();
			
			ObjectHierarchy ObjH=GetTransformInHierarchyRecursively(objT);
			
			objHList.Add(null);		objHList.Add(objT);
			objNList.Add(" - ");		objNList.Add("   -"+objT.name);
			
			for(int i=0; i<ObjH.listT.Count; i++) objHList.Add(ObjH.listT[i]);
			 for(int i=0; i<ObjH.listName.Count; i++){
				while(objNList.Contains(ObjH.listName[i])) ObjH.listName[i]+=".";
				objNList.Add(ObjH.listName[i]);
			}
			
			objHierarchyList=objHList.ToArray();
			objHierarchylabel=objNList.ToArray();
		}
		
		private static ObjectHierarchy GetTransformInHierarchyRecursively(Transform transform, string label="   "){
			ObjectHierarchy ObjH=new ObjectHierarchy();	label+="   ";
			foreach(Transform t in transform){
				ObjH.listT.Add(t);	ObjH.listName.Add(label+"-"+t.name);
				
				ObjectHierarchy tempHL=GetTransformInHierarchyRecursively(t, label);
				foreach(Transform tt in tempHL.listT) ObjH.listT.Add(tt);
				foreach(string ll in tempHL.listName) ObjH.listName.Add(ll);
			}
			return ObjH;
		}
		
		private class ObjectHierarchy{
			public List<Transform> listT=new List<Transform>();
			public List<string> listName=new List<string>();
		}
		#endregion
		
	}

}