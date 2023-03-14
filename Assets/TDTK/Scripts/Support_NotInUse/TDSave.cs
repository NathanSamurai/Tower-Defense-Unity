using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK {

	public class TDSave : MonoBehaviour {

		public static void SaveGeneralProgress(){
			//level progress
			PlayerPrefs.SetInt("TDTK_HighestLvlID", GameControl.GetHighestLevelID());
			
			//resource
			List<int> rscList=RscManager.GetCachedList();
			PlayerPrefs.SetInt("TDTK_RscCount", rscList.Count);
			for(int i=0; i<rscList.Count; i++) PlayerPrefs.SetInt("TDTK_Rsc_"+i, rscList[i]);
			
			//Perk
			PlayerPrefs.SetInt("TDTK_PerkRsc", PerkManager.GetCachedRsc());
			List<int> perkList=PerkManager.GetCachedPurchasedIDList();
			PlayerPrefs.SetInt("TDTK_PerkCount", perkList.Count);
			for(int i=0; i<perkList.Count; i++) PlayerPrefs.SetInt("TDTK_PerkID_"+i, perkList[i]);
		}
		
		//call this in RscManager Awake(), replace it with rscList
		public static List<int> LoadRsc(){
			List<int> rscList=new List<int>();
			int count=PlayerPrefs.GetInt("TDTK_RscCount", 0);
			for(int i=0; i<count; i++) rscList.Add(PlayerPrefs.GetInt("TDTK_Rsc_"+i, 0));
			return rscList;
		}
		
		
		public static int LoadPerkRsc(){ return PlayerPrefs.GetInt("TDTK_PerkRsc"); }
		//call this in PerkManager Awake(), replace it with purchasedPrefabIDList
		public static List<int> LoadPerk(){
			List<int> perkList=new List<int>();
			int count=PlayerPrefs.GetInt("TDTK_PerkCount", 0);
			for(int i=0; i<count; i++) perkList.Add(PlayerPrefs.GetInt("TDTK_PerkID_"+i, 0));
			return perkList;
		}
		
		
		/*
		public static void SavePerk(){
			List<int> perkList=PerkManager.GetCachedPurchasedIDList();
			PlayerPrefs.SetInt("TDTK_PerkCount", perkList.Count);
			for(int i=0; i<perkList.Count; i++) PlayerPrefs.SetInt("TDTK_PerkID_"+i, perkList[i]);
		}
		public static List<int> LoadPerk(){
			List<int> perkList=new List<int>();
			int count=PlayerPrefs.GetInt("TDTK_PerkCount", 0);
			for(int i=0; i<count; i++) perkList.Add(PlayerPrefs.GetInt("TDTK_PerkID_"+i, 0));
			return perkList;
		}
		*/
		
	}

}
