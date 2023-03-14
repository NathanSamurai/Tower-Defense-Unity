using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {
	
	[CreateAssetMenu(fileName = "PerkDB", menuName = "TDTK_DB/PerkDB", order = 1)]
	public class Perk_DB : ScriptableObject {
		
		public Sprite rscIcon;
		public List<Perk> perkList=new List<Perk>();
		
		public static Perk_DB LoadDB(){
			return Resources.Load("DB/PerkDB", typeof(Perk_DB)) as Perk_DB;
		}
		
		#region runtime code
		public static Perk_DB instance;
		public static Perk_DB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();
			UpdateLabel();
			return instance;
		}
	
		public static Perk_DB GetDB(){ return Init(); }
		public static List<Perk> GetList(){ return Init().perkList; }
		public static Perk GetItem(int index){ Init(); return (index>=0 && index<instance.perkList.Count) ? instance.perkList[index] : null; }
		
		public static Sprite GetRscIcon(){ return Init().rscIcon; }
		public static void SetRscIcon(Sprite icon){ 
			Init().rscIcon=icon;
			
			#if UNITY_EDITOR
			EditorUtility.SetDirty(instance);
			#endif
		}
		
		public static List<int> GetPrefabIDList(){ Init();
			List<int> prefabIDList=new List<int>();
			for(int i=0; i<instance.perkList.Count; i++) prefabIDList.Add(instance.perkList[i].prefabID);
			return prefabIDList;
		}
		
		public static Perk GetPrefab(int pID){ Init();
			for(int i=0; i<instance.perkList.Count; i++){
				if(instance.perkList[i].prefabID==pID) return instance.perkList[i];
			}
			return null;
		}
		
		public static int GetPrefabIndex(int pID){ Init();
			for(int i=0; i<instance.perkList.Count; i++){
				if(instance.perkList[i].prefabID==pID) return i;
			}
			return -1;
		}
		public static int GetPrefabIndex(Perk ability){
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
			for(int i=0; i<perkList.Count; i++){
				perkList[i].prefabID=i;
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
			instance.perkList=new List<Perk>( PerkDB.GetList1() );
			instance.rscIcon=PerkDB.GetRscIcon1();
		}
		public static bool UpdatedToPost_2018_3(){ Init(); return instance.copiedFromOldDB; }
		
	}

}
