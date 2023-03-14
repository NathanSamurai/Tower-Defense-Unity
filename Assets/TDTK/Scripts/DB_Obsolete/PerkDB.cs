using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {

	public class PerkDB : MonoBehaviour {
		
		public Sprite rscIcon;
		public List<Perk> perkList=new List<Perk>();
		
		public static PerkDB LoadDB(){
			GameObject obj=Resources.Load("DB/PerkDB_Obsolete", typeof(GameObject)) as GameObject;
			return obj.GetComponent<PerkDB>();
		}
		
		public static PerkDB instance;
		public static PerkDB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();
			return instance;
		}
		
		public static List<Perk> GetList1(){ return Init().perkList; }
		public static Sprite GetRscIcon1(){ return Init().rscIcon; }
		
		/*
		#region runtime code
		
		
		public static PerkDB GetDB(){ return Init(); }
		public static Perk GetItem(int index){ Init(); return (index>=0 && index<instance.perkList.Count) ? instance.perkList[index] : null; }
		
		public static void SetRscIcon(Sprite icon){ Init().rscIcon=icon; }
		
		public static List<int> GetPrefabIDList(){
			Init();
			List<int> prefabIDList=new List<int>();
			for(int i=0; i<instance.perkList.Count; i++) prefabIDList.Add(instance.perkList[i].prefabID);
			return prefabIDList;
		}
		
		public static int GetPrefabIndex(int pID){
			for(int i=0; i<instance.perkList.Count; i++){
				if(instance.perkList[i].prefabID==pID) return i;
			}
			return -1;
		}
		public static int GetPrefabIndex(Perk perk){
			if(perk==null) return -1;
			return GetPrefabIndex(perk.prefabID);
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
			for(int i=0; i<perkList.Count; i++){
				perkList[i].prefabID=i;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
		#endif
		*/
	}
	
	
	
	

}