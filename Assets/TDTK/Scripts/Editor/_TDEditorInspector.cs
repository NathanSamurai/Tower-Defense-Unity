using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

namespace TDTK{

	public class _TDInspector : Editor {
		
		protected GUIContent cont;
		protected GUIContent contN=GUIContent.none;
		protected GUIContent[] contL;
		
		
		public virtual void Awake(){ TDE.Init(); }
		
		
		public override void OnInspectorGUI(){
			TDE.InitGUIStyle();
		}
		
		
		protected static bool showDefaultEditor=false;
		protected void DefaultInspector(){
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
				showDefaultEditor=EditorGUILayout.Foldout(showDefaultEditor, "Show default editor", TDE.foldoutS);
			EditorGUILayout.EndHorizontal();
			
			if(showDefaultEditor) DrawDefaultInspector();
			
			EditorGUILayout.Space();
		}
		
		
		protected void DrawVisualObject(VisualObject vo, GUIContent gContent){
			vo.obj=(GameObject)EditorGUILayout.ObjectField(gContent, vo.obj, typeof(GameObject), true);
			
			cont=new GUIContent(" - Auto Destroy:", "Check if the spawned effect should be destroyed automatically");
			if(vo.obj!=null) vo.autoDestroy=EditorGUILayout.Toggle(cont, vo.autoDestroy);
			else EditorGUILayout.LabelField(" - Auto Destroy:", "-");
			
			cont=new GUIContent(" - Effect Duration:", "How long before the spawned effect object is destroyed");
			if(vo.obj!=null && vo.autoDestroy) vo.duration=EditorGUILayout.FloatField(cont, vo.duration);
			else EditorGUILayout.LabelField(" - Effect Duration:", "-");
		}
		
		
	}

}