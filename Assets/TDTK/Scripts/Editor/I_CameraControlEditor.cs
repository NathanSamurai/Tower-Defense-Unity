using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(CameraControl))]
	public class I_CameraControlEditor : _TDInspector {

		private CameraControl instance;
		
		private float width=116;
		
		public override void Awake(){
			base.Awake();
			instance = (CameraControl)target;
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "CameraControl");
			
			width=EditorGUIUtility.labelWidth-4;
			
			EditorGUILayout.Space();
				
				cont=new GUIContent("Use Touch Input:", "Check for touch based input");
				instance.useTouchInput=EditorGUILayout.Toggle(cont, instance.useTouchInput);
			
			EditorGUILayout.Space();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("Enable Zoom:", "Check to allow player to control camera zoom level");
					EditorGUILayout.LabelField(cont, TDE.headerS, GUILayout.Width(width));
					instance.enableZoom=EditorGUILayout.Toggle(instance.enableZoom);
				EditorGUILayout.EndHorizontal();
			
				cont=new GUIContent(" - Zoom Speed:", "The sensitivity of input to zoom");
				if(instance.enableZoom) instance.zoomSpeed=EditorGUILayout.FloatField(cont, instance.zoomSpeed);
				else EditorGUILayout.LabelField(" - Zoom Speed:", "-");
			
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent(" - Zoom Limit:", "The limit of the camera zoom. This is effectively the local Z-axis position limit of the camera transform as a child of the camera pivot");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.minZoomDistance=EditorGUILayout.FloatField(instance.minZoomDistance);
					instance.maxZoomDistance=EditorGUILayout.FloatField(instance.maxZoomDistance);
				EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("Enable Rotation:", "Check to allow player to control camera view angle");
					EditorGUILayout.LabelField(cont, TDE.headerS, GUILayout.Width(width));
					instance.enableRotate=EditorGUILayout.Toggle(instance.enableRotate);
				EditorGUILayout.EndHorizontal();
			
				cont=new GUIContent(" - Rotate Speed:", "The sensitivity of input to rotation");
				if(instance.enableRotate) instance.rotateSpeed=EditorGUILayout.FloatField(cont, instance.rotateSpeed);
				else EditorGUILayout.LabelField(" - Rotate Speed:", "-");
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent(" - Elevation Limit:", "The limit of the elevation of the camera pivot, effectively the X-axis rotation. Recommend to keep the value between 10 to 89");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.minRotateAngle=EditorGUILayout.FloatField(instance.minRotateAngle);
					instance.maxRotateAngle=EditorGUILayout.FloatField(instance.maxRotateAngle);
				EditorGUILayout.EndHorizontal();
				
			EditorGUILayout.Space();
				
				EditorGUILayout.LabelField(new GUIContent("Pivot Scrolling", "The method in which the camera pivot move"), TDE.headerS);
				cont=new GUIContent(" - Key:", "Check to enable camera position scrolling by w,a,s,d and arrow key");
				instance.scrollKey=EditorGUILayout.Toggle(cont, instance.scrollKey);
				
				cont=new GUIContent(" - Cursor Drag:", "Check to enable camera position scrolling by dragging cursor on screen");
				instance.scrollCursorDrag=EditorGUILayout.Toggle(cont, instance.scrollCursorDrag);
				
				EditorGUILayout.BeginHorizontal();
				
					cont=new GUIContent(" - Cursor On Edge:", "Check to enable camera position scrolling by positioning cursor on screen edge\n\nTH being the threshold from screen edge the cursor needs to pass before the scolling take effect");
					instance.scrollCursorOnEdge=EditorGUILayout.Toggle(cont, instance.scrollCursorOnEdge);
					
					float defaultLabelWidth=EditorGUIUtility.labelWidth;	EditorGUIUtility.labelWidth=25;
					
					cont=new GUIContent("TH:", "The threshold from screen edge the cursor needs to pass before the scolling take effect");
					if(instance.scrollCursorOnEdge) instance.scrollCursorOnEdgeTH=EditorGUILayout.FloatField(cont, instance.scrollCursorOnEdgeTH);
					
					EditorGUIUtility.labelWidth=defaultLabelWidth;
				
				EditorGUILayout.EndHorizontal();
				
				
				bool enableScroll=instance.scrollKey | instance.scrollCursorDrag | instance.scrollCursorOnEdge;
				cont=new GUIContent("Scroll Speed:", "The sensitivity of input to scrolling");
				if(enableScroll) instance.scrollSpeed=EditorGUILayout.FloatField(cont, instance.scrollSpeed);
				else EditorGUILayout.LabelField("Scroll Speed:", "-");
				
				EditorGUILayout.Space();
				
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("Enable Limit:", "Check to limit the camera pivot within a fixed area");
					EditorGUILayout.LabelField(cont, TDE.headerS, GUILayout.Width(width));
					instance.enablePositionLimit=EditorGUILayout.Toggle(instance.enablePositionLimit);
				EditorGUILayout.EndHorizontal();
				//~ cont=new GUIContent("Enable Limit:", "Check to limit the camera pivot within a fixed area");
				//~ instance.enablePositionLimit=EditorGUILayout.Toggle(cont, instance.enablePositionLimit);
				
				if(instance.enablePositionLimit){
					EditorGUILayout.BeginHorizontal();
						cont=new GUIContent(" - X-Axis Limit:", "The min/max X-axis position limit of the camera pivot.\nWhen 'Link Limit To Zoom' are active, this is the limit when the camera is zoom all the way in");
						EditorGUILayout.LabelField(cont, GUILayout.Width(width));
						instance.minPosX=EditorGUILayout.FloatField(instance.minPosX);
						instance.maxPosX=EditorGUILayout.FloatField(instance.maxPosX);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
						cont=new GUIContent(" - Z-Axis Limit:", "The min/max Z-axis position limit of the camera pivot.\nWhen 'Link Limit To Zoom' are active, this is the limit when the camera is zoom all the way in");
						EditorGUILayout.LabelField(cont, GUILayout.Width(width));
						instance.minPosZ=EditorGUILayout.FloatField(instance.minPosZ);
						instance.maxPosZ=EditorGUILayout.FloatField(instance.maxPosZ);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.Space();
					
					cont=new GUIContent(" - LinkLimitToZoom:", "Check to link the area limit to the camera zoom level. The area will extend as the camera zoom out");
					instance.linkPanLimitToZoom=EditorGUILayout.Toggle(cont, instance.linkPanLimitToZoom);
					
					if(instance.linkPanLimitToZoom){
						EditorGUILayout.BeginHorizontal();
							cont=new GUIContent("    -Extension(X/Z):", "The maximum extension in each X and Z axis to be added to the limit area when the camera are zoomed all the way out");
							EditorGUILayout.LabelField(cont, GUILayout.Width(width));
							instance.limitExtX_ZOut=EditorGUILayout.FloatField(instance.limitExtX_ZOut);
							instance.limitExtZ_ZOut=EditorGUILayout.FloatField(instance.limitExtZ_ZOut);
						EditorGUILayout.EndHorizontal();
					}
					else{
						EditorGUILayout.LabelField("    -Extension(X/Z):", "-");
					}
				}
				else{
					EditorGUILayout.LabelField(" - X-Axis Limit:", "-");
					EditorGUILayout.LabelField(" - Z-Axis Limit:", "-");
				}
				
			
			EditorGUILayout.Space();
				
				cont=new GUIContent("Avoid Clipping:", "Check to stop the camera from clipping through any objects and block the line-of-sight to the pivot\nThis require the objects to have collider to work.");
				instance.avoidClipping=EditorGUILayout.Toggle(cont, instance.avoidClipping);
				
			EditorGUILayout.Space();
			
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}