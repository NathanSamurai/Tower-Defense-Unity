using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {
	
	[CreateAssetMenu(fileName = "AbilityDB", menuName = "TDTK_DB/AbilityDB", order = 1)]
	public class Ability_DB : ScriptableObject {
		
		public Sprite rscIcon;
		public List<Ability> abilityList=new List<Ability>();
		
		public static Ability_DB LoadDB(){
			return Resources.Load("DB/AbilityDB", typeof(Ability_DB)) as Ability_DB;
		}
		
		#region runtime code
		public static Ability_DB instance;
		public static Ability_DB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();
			UpdateLabel();
			return instance;
		}
	
		public static Ability_DB GetDB(){ return Init(); }
		public static List<Ability> GetList(){ return Init().abilityList; }
		public static Ability GetItem(int index){ Init(); return (index>=0 && index<instance.abilityList.Count) ? instance.abilityList[index] : null; }
		
		public static Sprite GetRscIcon(){ return Init().rscIcon; }
		public static void SetRscIcon(Sprite icon){ 
			Init().rscIcon=icon;
			
			#if UNITY_EDITOR
			EditorUtility.SetDirty(instance);
			#endif
		}
		
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
		
		
		//[HideInInspector]
		[Space(10)] public bool copiedFromOldDB=false;
		public static void CopyFromOldDB(){		Init();
			if(instance.copiedFromOldDB) return;
			
			instance.copiedFromOldDB=true;
			instance.abilityList=new List<Ability>( AbilityDB.GetList1() );
			instance.rscIcon=AbilityDB.GetRscIcon1();
		}
		public static bool UpdatedToPost_2018_3(){ Init(); return instance.copiedFromOldDB; }
		
	}
	
}
