using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK {

	public class DamageTable : MonoBehaviour {

		private static List<ArmorType> armorTypeList=new List<ArmorType>();
		private static List<DamageType> damageTypeList=new List<DamageType>();
		
		public static List<DamageType> GetAllDamageType(){ return damageTypeList; }
		public static List<ArmorType> GetAllArmorType(){ return armorTypeList; }
		
		
		private static bool init=false;
		public static void Init(){
			if(init) return;
			init=true;	LoadPrefab();
		}
		
		private static void LoadPrefab(){
			DamageTable_DB prefab=DamageTable_DB.LoadDB();
			
			if(prefab==null) Debug.LogWarning("Loading Damage Table failed");
			else{
				armorTypeList=prefab.armorTypeList;
				damageTypeList=prefab.damageTypeList;
			}
		}
		
		public static float GetModifier(int armorID=0, int dmgID=0){
			Init();
			
			armorID=Mathf.Max(0, armorID);	dmgID=Mathf.Max(0, dmgID);
			
			if(armorID>=0 && armorID<armorTypeList.Count && dmgID>=0 && dmgID<damageTypeList.Count){
				return armorTypeList[armorID].modifiers[dmgID];
			}
			
			return 1f;
		}
		
	}


	[System.Serializable]
	public class DAType {
		public string name="";
	}
	[System.Serializable]
	public class DamageType : DAType{
		
	}
	[System.Serializable]
	public class ArmorType : DAType{
		public List<float> modifiers=new List<float>();
	}

}