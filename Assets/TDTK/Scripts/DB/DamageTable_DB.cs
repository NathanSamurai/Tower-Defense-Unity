using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {
	
	[CreateAssetMenu(fileName = "DamageTableDB", menuName = "TDTK_DB/DamageTable", order = 1)]
	public class DamageTable_DB : ScriptableObject {
		
		public List<ArmorType> armorTypeList=new List<ArmorType>();
		public List<DamageType> damageTypeList=new List<DamageType>();
		
		public static DamageTable_DB LoadDB(){
			return Resources.Load("DB/DamageTableDB", typeof(DamageTable_DB)) as DamageTable_DB;
		}
		
		
		#region runtime code
		public static DamageTable_DB instance;
		public static DamageTable_DB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();
			UpdateLabel();
			return instance;
		}
		
		public static DamageTable_DB GetDB(){ return Init(); }
		public static List<ArmorType> GetArmorList(){ return Init().armorTypeList; }
		public static List<DamageType> GetDamageList(){ return Init().damageTypeList; }
		
		
		public static string[] armorlb;
		public static string[] damagelb;
		public static void UpdateLabel(){
			armorlb=new string[GetArmorList().Count];
			for(int i=0; i<GetArmorList().Count; i++) armorlb[i]=i+" - "+GetArmorList()[i].name;
			
			damagelb=new string[GetDamageList().Count];
			for(int i=0; i<GetDamageList().Count; i++) damagelb[i]=i+" - "+GetDamageList()[i].name;
		}
		#endregion
		
		
		//[HideInInspector]
		[Space(10)]
		public bool copiedFromOldDB=false;
		public static void CopyFromOldDB(){		Init();
			if(instance.copiedFromOldDB) return;
			
			instance.copiedFromOldDB=true;
			instance.armorTypeList=new List<ArmorType>( DamageTableDB.GetArmorList1() );
			instance.damageTypeList=new List<DamageType>( DamageTableDB.GetDamageList1() );
		}
		public static bool UpdatedToPost_2018_3(){ Init(); return instance.copiedFromOldDB; }
		
	}

}
