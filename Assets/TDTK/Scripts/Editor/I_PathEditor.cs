using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(Path))]
	[CanEditMultipleObjects]
	public class I_PathEditor : _TDInspector {

		private Path instance;
		
		private bool showPath=true;
		
		public override void Awake(){
			base.Awake();
			instance = (Path)target;
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null){ Awake(); return; }
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "Path");
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
				//EditorGUILayout.LabelField("Has Valid Dest:", instance.hasValidDestination ? "true" : "false");
			
				
				cont=new GUIContent("Dynamic Offset:", "A random offset range which somewhat randomize the waypoint for each individual creep\nSet to 0 to disable and any value >0 to enable\nMax value is limited to 45% of BuildManager's grid-size\nNot recommend for path with varying height");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("dynamicOffset"), cont);
			
				cont=new GUIContent("Loop:", "Check to enable path-looping. On path that loops, creep will carry on to the looping point and repeat the path until they are destroyed");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"), cont);
			
				if(!serializedObject.isEditingMultipleObjects && instance.loop){
					cont=new GUIContent(" - Warp To Start:", "Check to warp the creep back to starting point instantly when they reach the final waypoint.\n\nOnly valid when looping is enabled");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("warpToStart"), cont);
				}
				else{
					cont=new GUIContent(" - Warp To Start:", "Check to warp the creep back to starting point instantly when they reach the final waypoint.\n\nOnly valid when looping is enabled");
					EditorGUILayout.LabelField(cont, new GUIContent("-"));
				}
				
			EditorGUILayout.Space();
				
				cont=new GUIContent("Fit Path To Terrain:", "Check to fit the waypoints to height terrain where it's applicable.\nThe path will be resampled along existing waypoints\n\nThis only works between non build-platform waypoints");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("fitPathToTerrain"), cont);
				
				if(instance.fitPathToTerrain){
					cont=new GUIContent(" - Step Size:", "The spacing between each new waypoint created");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("fitPath_stepSize"), cont);
					
					cont=new GUIContent(" - Height Offset:", "The height offset of the waypoint from the terrain");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("fitPath_heightOffset"), cont);
				}
				else{
					cont=new GUIContent(" - Step Size:", "The spacing between each new waypoint created");
					EditorGUILayout.LabelField(cont, new GUIContent("-"));
					
					cont=new GUIContent(" - Height Offset:", "The height offset of the waypoint from the terrain");
					EditorGUILayout.LabelField(cont, new GUIContent("-"));
				}
				
			EditorGUILayout.Space();
				
				if(serializedObject.isEditingMultipleObjects){
					EditorGUILayout.HelpBox("editing waypoints and next-path list on multiple instance is not supported", MessageType.Info);
				}
				else{
					showPath=EditorGUILayout.Foldout(showPath, "Show Waypoint List");
					if(showPath){
						GUILayout.BeginHorizontal();
							GUILayout.Label("", GUILayout.MaxWidth(8));
							if(GUILayout.Button("Set Childs To Waypoints")) instance.FillWP();
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
							GUILayout.Label("", GUILayout.MaxWidth(8));
							if(GUILayout.Button("Clear All Waypoints")) instance.waypointTList=new List<Transform>();
						GUILayout.EndHorizontal();
						
						EditorGUILayout.Space();
						
						for(int i=0; i<instance.waypointTList.Count; i++){
							GUILayout.BeginHorizontal();
							
							GUILayout.Label("   - Waypoint "+(i+1));
							
							instance.waypointTList[i]=(Transform)EditorGUILayout.ObjectField(instance.waypointTList[i], typeof(Transform), true);
							
							if(GUILayout.Button("+", GUILayout.MaxWidth(20), GUILayout.MaxHeight(14))){
								InsertWaypoints(i);
							}
							if(GUILayout.Button("-", GUILayout.MaxWidth(20), GUILayout.MaxHeight(14))){
								i-=RemoveWaypoints(i);
							}
							GUILayout.EndHorizontal();
						}
						
						EditorGUILayout.Space();
						
						GUILayout.BeginHorizontal();
							GUILayout.Label("", GUILayout.MaxWidth(8));
							if(GUILayout.Button("Add Waypoint")){
								AddWaypoint();
							}
							if(GUILayout.Button("Reduce Waypoint")){
								RemoveWaypoint();
							}
						GUILayout.EndHorizontal();
					}
					
					
						EditorGUILayout.Space();
						EditorGUILayout.Space();
					
					
					if(!instance.loop){
						GUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Next Path List:");
						if(GUILayout.Button("Add", GUILayout.MaxWidth(50))){
							instance.nextPath.Add(null);
						}
						GUILayout.EndHorizontal();
						
						for(int i=0; i<instance.nextPath.Count; i++){
							GUILayout.BeginHorizontal();
							GUILayout.Label("   - Path "+(i+1));
							instance.nextPath[i]=(Path)EditorGUILayout.ObjectField(instance.nextPath[i], typeof(Path), true);
							if(instance.nextPath[i]==instance) instance.nextPath[i]=null;
							if(GUILayout.Button("-", GUILayout.MaxWidth(20), GUILayout.MaxHeight(14))){
								instance.nextPath.RemoveAt(i); i-=1;
							}
							GUILayout.EndHorizontal();
						}
					}
					else{
						EditorGUILayout.LabelField("Next Path List:", "-");
					}
				}
		
			EditorGUILayout.Space();
			
				cont=new GUIContent("Show Gizmo:", "Check to enable gizmo to show the active path");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("showGizmo"), cont);
				
				cont=new GUIContent("Gizmo Color:", "Color of the gizmo\nSet different path's gizmo color to different color to help you differentiate them");
				if(instance.showGizmo || serializedObject.FindProperty("showGizmo").hasMultipleDifferentValues)
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoColor"), cont);
				else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		
		
		
		void InsertWaypoints(int ID){
			if(Application.isPlaying) return;
			instance.waypointTList.Insert(ID, instance.waypointTList[ID]);
		}
		int RemoveWaypoints(int ID){
			if(Application.isPlaying) return 0;
			instance.waypointTList.RemoveAt(ID);
			return 1;
		}
		void AddWaypoint(){
			if(Application.isPlaying) return;
			if(instance.waypointTList.Count==0) instance.waypointTList.Add(null);
			else instance.waypointTList.Add(instance.waypointTList[instance.waypointTList.Count-1]);
		}
		void RemoveWaypoint(){
			if(Application.isPlaying) return;
			if(instance.waypointTList.Count==0) return;
			instance.waypointTList.RemoveAt(instance.waypointTList.Count-1);
		}
		
	}

}