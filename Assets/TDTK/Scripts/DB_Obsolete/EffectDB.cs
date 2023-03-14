using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {

	public class EffectDB : MonoBehaviour {
		
		public List<Effect> effectList=new List<Effect>();
		
		public static EffectDB LoadDB(){
			GameObject obj=Resources.Load("DB/EffectDB_Obsolete", typeof(GameObject)) as GameObject;
			return obj.GetComponent<EffectDB>();
		}
		
		public static EffectDB instance;
		public static EffectDB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();		//UpdateLabel();
			return instance;
		}
		
		public static List<Effect> GetList1(){ return Init().effectList; }
		
		
		/*
		#region runtime code
		public static EffectDB GetDB(){ return Init(); }
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
		public static int GetPrefabIndex(Effect effect){
			if(effect==null) return -1;
			return GetPrefabIndex(effect.prefabID);
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
		*/
	}
	
	
	
	
	

}