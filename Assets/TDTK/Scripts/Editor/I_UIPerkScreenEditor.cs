using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

namespace TDTK{
	
	[CustomEditor(typeof(UIPerkScreen))] [CanEditMultipleObjects]
	public class I_UIPerkScreenEditor : _TDInspector {

		private UIPerkScreen instance;
		
		public override void Awake(){
			base.Awake();
			instance = (UIPerkScreen)target;
			
			instance.SetInstance();
			
			TDE.Init();
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			EditorGUILayout.Space();
			
			DrawDefaultInspector();
			
			EditorGUILayout.Space();
			
			
			if(!instance.customLayout) return;
			
			
				string text="Fill up the item list using using the child in Item List Parent. This save you the trouble of manually assinging the item one by one ";
				text+="but you will still have to manually assign the perk-ID";
				text+="\nThis will clear the existing list";
				EditorGUILayout.HelpBox(text, MessageType.Info);
				
				cont=new GUIContent("Item List Parent:", "");
				instance.perkListParent=(Transform)EditorGUILayout.ObjectField(cont, instance.perkListParent, typeof(Transform), true);
				
				if(GUILayout.Button("Fill List from Parent")){
					FillListFromParent();
				}
			
				
			EditorGUILayout.Space();
			
				
				text="Auto generate the item for all eixisting perks. You can then rearrange the item manually";
				text+="\nThis will clear the existing list";
				EditorGUILayout.HelpBox(text, MessageType.Info);
			
				cont=new GUIContent("Perk Item Prefab:", "");
				instance.perkItemPrefab=(GameObject)EditorGUILayout.ObjectField(cont, instance.perkItemPrefab, typeof(GameObject), true);
				
				if(GUILayout.Button("Generate Perk Buttons")){
					GenerateButtons();
				}
			
				
			EditorGUILayout.Space();
				
				text="Rename and assign icon for each item according to their corresponding perk.";
				text+="\nThis function will also check for any duplicating items or item that doesn't have a valid perk-ID";
				EditorGUILayout.HelpBox(text, MessageType.Info);
				
				if(GUILayout.Button("Verify and Name Existing Item")){
					VerifyAndNameButtons();
				}
			
				
			EditorGUILayout.Space();
		}
		
		
		private Perk GetPerkFromID(int id){
			for(int i=0; i<TDE.perkDB.perkList.Count; i++){
				if(TDE.perkDB.perkList[i].prefabID==id) return TDE.perkDB.perkList[i];
			}
			return null;
		}
		
		private void VerifyAndNameButtons(){
			List<int> existingList=new List<int>();
			
			for(int i=0; i<instance.itemList.Count; i++){
				if(instance.itemList[i]==null || instance.itemList[i].rootObj==null){
					instance.itemList.RemoveAt(i);	i-=1; continue;
				}
				
				Perk perk=GetPerkFromID(instance.itemList[i].linkedPerkPID);
				if(perk!=null){
					GameObject imgObj=instance.itemList[i].rootObj.transform.Find("Image").gameObject;
					Image imgIcon=imgObj.GetComponent<Image>();
					imgIcon.sprite=perk.icon;
					
					instance.itemList[i].rootObj.name="pItem_"+perk.prefabID+"_"+perk.name;
					
					if(existingList.Contains(perk.prefabID)){
						Debug.Log("Perk item contain duplicating ID, index-"+i+"   perk-"+perk.name);
					}
					else existingList.Add(perk.prefabID);
				}
				else{
					instance.itemList[i].rootObj.name="pItem_";
				}
			}
			
			for(int i=0; i<instance.itemList.Count; i++){
				if(instance.itemList[i].rootObj.name=="pItem_"){
					Debug.Log("Perk item ID at index-"+i+" not corresponding to any of the existing perks");
				}
			}
		}
		
		private void FillListFromParent(){
			if(instance.perkListParent==null){
				Debug.Log("Invalid Action - 'Perk List Parent' not assigned!");
				return;
			}
			
			instance.itemList.Clear();	//ClearItemList();
			
			int count=0;
			foreach(Transform child in instance.perkListParent){
				if(!child.gameObject.activeInHierarchy || child.gameObject.GetComponent<Button>()==null) continue;
				
				instance.itemList.Add(new UIPerkScreen.UIPerkItem());
				instance.itemList[count].rootObj=child.gameObject;
				
				count+=1;
			}
		}
		
		
		private void GenerateButtons(){
			if(instance.perkItemPrefab==null){
				Debug.Log("Invalid Action - No perk item prefab!");
				return;
			}
			
			ClearItemList();
			
			for(int i=0; i<TDE.perkDB.perkList.Count; i++){
				instance.itemList.Add(UIPerkScreen.UIPerkItem.Clone(instance.perkItemPrefab, "PerkItem"+(i)));
				instance.itemList[i].linkedPerkPID=TDE.perkDB.perkList[i].prefabID;
				
				instance.itemList[i].rootObj.name="pItem_"+TDE.perkDB.perkList[i].prefabID+"_"+TDE.perkDB.perkList[i].name;
				
				GameObject imgObj=instance.itemList[i].rootObj.transform.Find("Image").gameObject;
				Image imgIcon=imgObj.GetComponent<Image>();
				imgIcon.sprite=TDE.perkDB.perkList[i].icon;
				
				instance.itemList[i].rootObj.SetActive(true);
			}
		}
		
		
		private void ClearItemList(){
			for(int i=0; i<instance.itemList.Count; i++){
				if(instance.itemList[i].rootObj!=instance.perkItemPrefab) DestroyImmediate(instance.itemList[i].rootObj);
			}
			instance.itemList.Clear();
		}
		
	}

}