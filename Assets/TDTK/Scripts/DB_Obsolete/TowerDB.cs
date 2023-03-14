using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDTK {

	public class TowerDB : MonoBehaviour {
		
		[HideInInspector] public List<GameObject> objList=new List<GameObject>();
		public List<UnitTower> towerList=new List<UnitTower>();
		
		public static TowerDB LoadDB(){
			GameObject obj=Resources.Load("DB/TowerDB_Obsolete", typeof(GameObject)) as GameObject;
			return obj.GetComponent<TowerDB>();
		}
		
		public static TowerDB instance;
		public static TowerDB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();
			//instance.FillObjectList();		UpdateLabel();
			return instance;
		}
		
		public static List<UnitTower> GetList1(bool verify=true){
			Init();
			//if(verify) VerifyList();
			return instance.towerList;
		}
		
		
		/*
		#region runtime code
		public static TowerDB GetDB(){ return Init(); }
		public static UnitTower GetItem(int index){ Init(); return (index>=0 && index<instance.towerList.Count) ? instance.towerList[index] : null; }
		
		public static void VerifyList(){
			#if UNITY_2018_3_OR_NEWER
			for(int i=0; i<instance.towerList.Count; i++){
				if(instance.towerList[i]!=null){
					if(instance.objList.Count>i)
						instance.objList[i]=instance.towerList[i].gameObject;
					else
						instance.objList.Add(instance.towerList[i].gameObject);
					continue;
				}
				
				if(i<instance.objList.Count && instance.objList[i]!=null){
					UnitTower tower=instance.objList[i].GetComponent<UnitTower>();
					if(tower!=null){
						instance.towerList[i]=tower;
						continue;
					}
				}
				
				instance.towerList.RemoveAt(i);	i-=1;
			}
			
			while(instance.objList.Count>instance.towerList.Count){
				instance.objList.RemoveAt(instance.objList.Count-1);
			}
			#else
			for(int i=0; i<instance.towerList.Count; i++){
				if(instance.towerList[i]==null){ instance.towerList.RemoveAt(i);	i-=1; }
			}
			#endif
		}
		
		public static List<int> GetPrefabIDList(){ Init();
			List<int> prefabIDList=new List<int>();
			for(int i=0; i<instance.towerList.Count; i++) prefabIDList.Add(instance.towerList[i].prefabID);
			return prefabIDList;
		}
		
		public static UnitTower GetPrefab(int pID){ Init();
			for(int i=0; i<instance.towerList.Count; i++){
				if(instance.towerList[i].prefabID==pID) return instance.towerList[i];
			}
			return null;
		}
		
		public static int GetPrefabIndex(int pID){ Init();
			for(int i=0; i<instance.towerList.Count; i++){
				//Debug.Log(i+"   "+instance.towerList[i]+"  "+instance.towerList[i].prefabID+"   "+pID);
				if(instance.towerList[i].prefabID==pID) return i;
			}
			return -1;
		}
		public static int GetPrefabIndex(UnitTower tower){
			if(tower==null) return -1;
			return GetPrefabIndex(tower.prefabID);
		}
		
		public static string[] label;
		public static void UpdateLabel(){
			label=new string[GetList(false).Count];
			for(int i=0; i<label.Length; i++) label[i]=i+" - "+GetItem(i).unitName;
		}
		#endregion
		
		
		#if UNITY_EDITOR
		[ContextMenu ("Reset PrefabID")]
		public void ResetPrefabID(){
			for(int i=0; i<towerList.Count; i++){
				towerList[i].prefabID=i;
				UnityEditor.EditorUtility.SetDirty(towerList[i]);
			}
		}
		
		//[ContextMenu ("Fill ObjectList")]
		public void FillObjectList(){
			objList=new List<GameObject>();
			for(int i=0; i<towerList.Count; i++) objList.Add(towerList[i].gameObject);
		}
		#endif
		*/
	}
	
	
	
	
	

}