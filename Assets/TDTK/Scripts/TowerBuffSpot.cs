using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBuffSpot : MonoBehaviour
{
	public static List<TowerBuffSpot> allBuffSpot=new List<TowerBuffSpot>();
	
	public static List<int> GetBuffPIDList(Vector3 pos){
		for(int i=0; i<allBuffSpot.Count; i++){
			if(allBuffSpot[i].ComparePos(pos)){
				return new List<int>( allBuffSpot[i].buffEffectPIDList );
			}
		}
		return new List<int>();
	}
	
	public static void ClearList(){ allBuffSpot.Clear(); }
	
	
	
	
    public List<int> buffEffectPIDList=new List<int>();
	
	void Awake(){
		allBuffSpot.Add(this);
	}
	
	public bool ComparePos(Vector3 pos){
		float th=TDTK.TowerManager.GetGridSize() * 0.25f;
		pos.y=0;
		return (Mathf.Abs(transform.position.x-pos.x)<th && Mathf.Abs(transform.position.z-pos.z)<th);
	}
}
