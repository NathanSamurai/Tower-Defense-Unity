using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {

	public class RscDB : MonoBehaviour {
		
		public List<RscItem> rscList=new List<RscItem>();
		
		public static RscDB LoadDB(){
			GameObject obj=Resources.Load("DB/RscDB_Obsolete", typeof(GameObject)) as GameObject;
			return obj.GetComponent<RscDB>();
		}
		
		public static RscDB instance;
		public static RscDB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();		//UpdateLabel();
			return instance;
		}
		
		public static List<RscItem> GetList1(){ return Init().rscList; }
		
		
		
		//~ #region runtime code
		//~ public static RscDB GetDB(){ return Init(); }
		//~ public static RscItem GetItem(int index){ Init(); return (index>=0 && index<instance.rscList.Count) ? instance.rscList[index] : null; }
		
		//~ public static string[] rsclb;
		//~ public static void UpdateLabel(){
			//~ rsclb=new string[GetList().Count];
			//~ for(int i=0; i<GetList().Count; i++) rsclb[i]=i+" - "+GetList()[i].name;
		//~ }
		
		//~ public static int GetCount(){ return GetList().Count; }
		//~ public static string GetName(int idx){ return GetList()[idx].name; }
		//~ public static Sprite GetIcon(int idx){ return GetList()[idx].icon; }
		//~ #endregion
		
	}
	
	
	

}