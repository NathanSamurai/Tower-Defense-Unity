using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {
	
	[CreateAssetMenu(fileName = "RscDB", menuName = "TDTK_DB/RscDB", order = 1)]
	public class Rsc_DB : ScriptableObject {
		public List<RscItem> rscList=new List<RscItem>();
		
		public static Rsc_DB LoadDB(){
			return Resources.Load("DB/RscDB", typeof(Rsc_DB)) as Rsc_DB;
		}
		
		
		#region runtime code
		public static Rsc_DB instance;
		public static Rsc_DB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();
			UpdateLabel();
			return instance;
		}
		
		public static Rsc_DB GetDB(){ return Init(); }
		public static List<RscItem> GetList(){ return Init().rscList; }
		public static RscItem GetItem(int index){ Init(); return (index>=0 && index<instance.rscList.Count) ? instance.rscList[index] : null; }
		
		
		public static string[] rsclb;
		public static void UpdateLabel(){
			rsclb=new string[GetList().Count];
			for(int i=0; i<GetList().Count; i++) rsclb[i]=i+" - "+GetList()[i].name;
		}
		
		public static int GetCount(){ return GetList().Count; }
		public static string GetName(int idx){ return GetList()[idx].name; }
		public static Sprite GetIcon(int idx){ return GetList()[idx].icon; }
		#endregion
		
		
		//[HideInInspector]
		[Space(10)]
		public bool copiedFromOldDB=false;
		public static void CopyFromOldDB(){		Init();
			if(instance.copiedFromOldDB) return;
			
			instance.copiedFromOldDB=true;
			instance.rscList=new List<RscItem>( RscDB.GetList1() );
		}
		public static bool UpdatedToPost_2018_3(){ Init(); return instance.copiedFromOldDB; }
	}

}
