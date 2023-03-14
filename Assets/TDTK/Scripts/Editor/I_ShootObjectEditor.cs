using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

namespace TDTK{
	
	[CustomEditor(typeof(ShootObject))] [CanEditMultipleObjects]
	public class I_ShootObjectEditor : _TDInspector {

		private ShootObject instance;
		public override void Awake(){
			base.Awake();
			instance = (ShootObject)target;
			
			InitLabel();
		}
		
		
		private bool labelInitiated=false;
		private static string[] soTypeLabel=new string[0];
		private static string[] soTypeTooltip=new string[0];
		
		void InitLabel(){
			if(labelInitiated) return;
			labelInitiated=true;
			
			int enumLength = Enum.GetValues(typeof(ShootObject._Type)).Length;
			soTypeLabel=new string[enumLength];
			soTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				soTypeLabel[i]=((ShootObject._Type)i).ToString();
				if((ShootObject._Type)i==ShootObject._Type.Projectile) 
					soTypeTooltip[i]="A typical projectile, travels from turret shoot-point towards target";
				if((ShootObject._Type)i==ShootObject._Type.Beam) 
					soTypeTooltip[i]="Used to render laser or any beam like effect. The shootObject doest move instead uses LineRenderer component to render beam from shoot-point to target";
				if((ShootObject._Type)i==ShootObject._Type.Effect) 
					soTypeTooltip[i]="A shootObject uses to show various firing effect. The shootObject will remain at shootPoint so it can act as a shoot effect";
				if((ShootObject._Type)i==ShootObject._Type.Lightning) 
					soTypeTooltip[i]="A shootObject uses to show lightning effect. The shootObject duses  multiple LineRenderer component to render random zig-zagging pattern from shoot-point to target (the code is contributed by user Lynn Pye)";
			}
		}
		
		
		protected SerializedProperty srlPpt;
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			Undo.RecordObject(instance, "ShootObject");
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
				srlPpt=serializedObject.FindProperty("type");
				//EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
				
				//EditorGUI.BeginChangeCheck();
				
				//cont=new GUIContent("Type:", "Type of the shoot object");
				//contL=TDE.SetupContL(soTypeLabel, soTypeTooltip);
				//int type = EditorGUILayout.Popup(cont, srlPpt.enumValueIndex, contL);
				
				//if(EditorGUI.EndChangeCheck()) srlPpt.enumValueIndex=type;
				//EditorGUI.showMixedValue=false;
			
			//EditorGUILayout.Space();
			
				if(srlPpt.hasMultipleDifferentValues){
					EditorGUILayout.HelpBox("Editing of type specify attribute is unavailable when selecting multiple shoot object of different type", MessageType.Warning);
				}
				else if(!srlPpt.hasMultipleDifferentValues){
					
					int type=(int)instance.type;
					cont=new GUIContent("Type:", "Type of the shoot object");
					contL=TDE.SetupContL(soTypeLabel, soTypeTooltip);
					type = EditorGUILayout.Popup(cont, type, contL);
					instance.type=(ShootObject._Type)type;
					srlPpt.enumValueIndex=type;
					
					if(type==(int)ShootObject._Type.Projectile){
						cont=new GUIContent("  Speed:", "The travel speed of the shootObject");
						instance.speed=EditorGUILayout.FloatField(cont, instance.speed);
						//EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"), cont);
						
						cont=new GUIContent("  Straight Projectile:", "Check to have the projectile move in a straight line directly to the target");
						instance.straightProjectile=EditorGUILayout.Toggle(cont, instance.straightProjectile);
						
						if(instance.straightProjectile){
							EditorGUILayout.LabelField("");
							EditorGUILayout.LabelField("");
							EditorGUILayout.LabelField("");
						}
						else{
							cont=new GUIContent("    Use AnimationCurve:", "Check to use simulated trajectory");
							instance.useTrajectoryCurve=EditorGUILayout.Toggle(cont, instance.useTrajectoryCurve);
							
							if(instance.useTrajectoryCurve){
								cont=new GUIContent("    Elevation (Angle):", "The elevation (in degree) of the shoot trajectory");
								instance.elevationT=EditorGUILayout.FloatField(cont, instance.elevationT);
								
								cont=new GUIContent("    Trajectory:", "The trajectory of the shoot-object");
								instance.trajectory=EditorGUILayout.CurveField(cont, instance.trajectory);
							}
							else{
								cont=new GUIContent("    Max Height:", "The maximum height in the shoot trajectory\nSet to 0 for a straight shot");
								instance.elevation=EditorGUILayout.FloatField(cont, instance.elevation);
								//EditorGUILayout.PropertyField(serializedObject.FindProperty("elevation"), cont);
								
								cont=new GUIContent("    Fall Off Range:", "The shot trajectory height will gradually decrease if get closer than this range\nIt's recommanded to match this value to the range of the tower");
								instance.falloffRange=EditorGUILayout.FloatField(cont, instance.falloffRange);
								//EditorGUILayout.PropertyField(serializedObject.FindProperty("falloffRange"), cont);
							}
						}
					}
					else if(type==(int)ShootObject._Type.Beam){
						if(serializedObject.isEditingMultipleObjects){
							EditorGUILayout.HelpBox("Assignment of LineRenderer component is not supported for multi-instance editing", MessageType.Info);
						}
						else{
							if(instance.lines.Count==0) instance.lines.Add(null);
							instance.lines[0]=(LineRenderer)EditorGUILayout.ObjectField("  LineRenderer", instance.lines[0], typeof(LineRenderer), true);
						}
						
						cont=new GUIContent("  Beam Duration:", "The active duration of the beam");
						instance.beamDuration=EditorGUILayout.FloatField(cont, instance.beamDuration);
						//EditorGUILayout.PropertyField(serializedObject.FindProperty("beamDuration"), cont);
						
						cont=new GUIContent("  Start Width:", "The starting width of the beam");
						instance.startWidth=EditorGUILayout.FloatField(cont, instance.startWidth);
						//EditorGUILayout.PropertyField(serializedObject.FindProperty("startWidth"), cont);
					}
					else if(type==(int)ShootObject._Type.Effect){
						cont=new GUIContent("  Effect Duration:", "How long the effect will last");
						instance.effectDuration=EditorGUILayout.FloatField(cont, instance.effectDuration);
						//EditorGUILayout.PropertyField(serializedObject.FindProperty("effectDuration"), cont);
					}
					else if(type==(int)ShootObject._Type.Lightning){
						if(serializedObject.isEditingMultipleObjects){
							EditorGUILayout.HelpBox("Assignment of LineRenderer component is not supported for multi-instance editing", MessageType.Info);
						}
						else{
							if(instance.lightningRenderers.Count==0) instance.lightningRenderers.Add(null);
							
							var list = instance.lightningRenderers;
							int newCount = Mathf.Max(1, EditorGUILayout.IntField("  LineRendererCount:", list.Count));
							while (newCount < list.Count) list.RemoveAt( list.Count - 1 );
							while (newCount > list.Count) list.Add(null);
							 
							for(int i = 0; i < list.Count; i++){
								list[i] = (LineRenderer)EditorGUILayout.ObjectField("     - Renderer"+i, list[i], typeof(LineRenderer), true);
							}
						}
						
						cont=new GUIContent("  Lightning Duration:", "The active duration of the lightning");
						instance.timeOfZap=EditorGUILayout.FloatField(cont, instance.timeOfZap);
						
						cont=new GUIContent("  Lightning Width:", "The width of the individual lightning beam");
						instance.lightningWidth=EditorGUILayout.FloatField(cont, instance.lightningWidth);
					}
					
				}
				
			EditorGUILayout.Space();
			EditorGUILayout.Space();
				
				cont=new GUIContent("Effect (Shoot):", "The visual effect to spawn at the shoot-point when the shoot-object is fired");
				DrawVisualObject(instance.effectShoot, cont);
				
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				
				cont=new GUIContent("Effect (Hit):", "The visual effect to spawn at the hit-point when the shoot-object hit its target");
				DrawVisualObject(instance.effectHit, cont);
				
			EditorGUILayout.Space();
			EditorGUILayout.Space();
				
				cont=new GUIContent("Shoot Sound:", "The audio clip to play when the shoot-object fires");
				instance.shootSound=(AudioClip)EditorGUILayout.ObjectField(cont, instance.shootSound, typeof(AudioClip), true);
				
				cont=new GUIContent("Hit Sound:", "The audio clip to play when the shoot-object hits");
				instance.hitSound=(AudioClip)EditorGUILayout.ObjectField(cont, instance.hitSound, typeof(AudioClip), true);
				
			EditorGUILayout.Space();
			
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}