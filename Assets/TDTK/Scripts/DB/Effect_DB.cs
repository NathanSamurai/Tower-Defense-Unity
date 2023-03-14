using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {
	
	[CreateAssetMenu(fileName = "EffectDB", menuName = "TDTK_DB/EffectDB", order = 1)]
	public class Effect_DB : ScriptableObject {
		
		public List<Effect> effectList=new List<Effect>();
		
		public static Effect_DB LoadDB(){
			return Resources.Load("DB/EffectDB", typeof(Effect_DB)) as Effect_DB;
		}
		
		#region runtime code
		public static Effect_DB instance;
		public static Effect_DB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();
			UpdateLabel();
			return instance;
		}
	
		public static Effect_DB GetDB(){ return Init(); }
		public static List<Effect> GetList(){ return Init().effectList; }
		public static Effect GetItem(int index){ Init(); return (index>=0 && index<instance.effectList.Count) ? instance.effectList[index] : null; }
		
		
		public static List<int> GetPrefabIDList(){ Init();
			List<int> prefabIDList=new List<int>();
			for(int i=0; i<instance.effectList.Count; i++) prefabIDList.Add(instance.effectList[i].prefabID);
			return prefabIDList;
		}
		
		public static Effect GetPrefab(int pID){ Init();
			for(int i=0; i<instance.effectList.Count; i++){
				if(instance.effectList[i].prefabID==pID) return instance.effectList[i];
			}
			return null;
		}
		
		public static int GetPrefabIndex(int pID){ Init();
			for(int i=0; i<instance.effectList.Count; i++){
				if(instance.effectList[i].prefabID==pID) return i;
			}
			return -1;
		}
		public static int GetPrefabIndex(Effect ability){
			if(ability==null) return -1;
			return GetPrefabIndex(ability.prefabID);
		}
		
		public static string[] label;
		public static void UpdateLabel(){
			label=new string[GetList().Count];
			for(int i=0; i<GetList().Count; i++) label[i]=i+" - "+GetList()[i].name;
		}
		#endregion
		
		
		#if UNITY_EDITOR
		[ContextMenu ("Reset PrefabID")]
		public void ResetPrefabID(){
			for(int i=0; i<effectList.Count; i++){
				effectList[i].prefabID=i;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
		#endif
		
		
		//[HideInInspector]
		[Space(10)]
		public bool copiedFromOldDB=false;
		public static void CopyFromOldDB(){		Init();
			if(instance.copiedFromOldDB) return;
			
			instance.copiedFromOldDB=true;
			instance.effectList=new List<Effect>( EffectDB.GetList1() );
		}
		public static bool UpdatedToPost_2018_3(){ Init(); return instance.copiedFromOldDB; }
		
	}

}
