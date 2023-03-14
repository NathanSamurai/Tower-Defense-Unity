using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {

	public class AbilityDB : MonoBehaviour {
		
		public Sprite rscIcon;
		public List<Ability> abilityList=new List<Ability>();
		
		public static AbilityDB LoadDB(){
			GameObject obj=Resources.Load("DB/AbilityDB_Obsolete", typeof(GameObject)) as GameObject;
			return obj.GetComponent<AbilityDB>();
		}
		
		public static AbilityDB instance;
		public static AbilityDB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();		//UpdateLabel();
			return instance;
		}
		
		public static List<Ability> GetList1(){ return Init().abilityList; }
		public static Sprite GetRscIcon1(){ return Init().rscIcon; }
		
		
		/*
		#region runtime code
		public static AbilityDB GetDB(){ return Init(); }
		public static Ability GetItem(int index){ Init(); return (index>=0 && index<instance.abilityList.Count) ? instance.abilityList[index] : null; }
		
		public static void SetRscIcon(Sprite icon){ Init().rscIcon=icon; }
		
		public static List<int> GetPrefabIDList(){ Init();
			List<int> prefabIDList=new List<int>();
			for(int i=0; i<instance.abilityList.Count; i++) prefabIDList.Add(instance.abilityList[i].prefabID);
			return prefabIDList;
		}
		
		public static Ability GetPrefab(int pID){ Init();
			for(int i=0; i<instance.abilityList.Count; i++){
				if(instance.abilityList[i].prefabID==pID) return instance.abilityList[i];
			}
			return null;
		}
		
		public static int GetPrefabIndex(int pID){ Init();
			for(int i=0; i<instance.abilityList.Count; i++){
				if(instance.abilityList[i].prefabID==pID) return i;
			}
			return -1;
		}
		public static int GetPrefabIndex(Ability ability){
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
			for(int i=0; i<abilityList.Count; i++){
				abilityList[i].prefabID=i;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
		#endif
		*/
	}
	
	
}