using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour{

	private static bool disablePooling=false;
	
	public bool showUninitiatedPoolWarning=false;
	
	public List<Pool> poolList=new List<Pool>();
	
	public static ObjectPoolManager instance;
	
	
	void Awake(){
		if(instance!=null && instance!=this){
			Debug.Log("Error, there are multiple instance of ObjectPoolManager in the scene");
			return;
		}
		
		instance=this;
	}
	public static void Init(){
		if(instance!=null) return;
		
		instance=(ObjectPoolManager)FindObjectOfType(typeof(ObjectPoolManager));
		if(instance==null){
			GameObject obj=new GameObject();
			obj.name="ObjectPoolManager";
			instance=obj.AddComponent<ObjectPoolManager>();
		}
	}
	
	public static void ClearAll(){	//not in used
		for(int i=0; i<instance.poolList.Count; i++) instance.poolList[i].Clear();
		instance.poolList=new List<Pool>();
	}
	
	public static Transform GetOPMTransform(){ return instance.transform; }
	
	
	
	
	
	public static Transform Spawn(Transform objT, float activeDuration=-1){
		return Spawn(objT.gameObject, Vector3.zero, Quaternion.identity, activeDuration).transform;
	}
	public static Transform Spawn(Transform objT, Vector3 pos, Quaternion rot, float activeDuration=-1){
		return instance._Spawn(objT.gameObject, pos, rot, activeDuration).transform;
	}
	
	public static GameObject Spawn(GameObject obj, float activeDuration=-1){
		return Spawn(obj, Vector3.zero, Quaternion.identity, activeDuration);
	}
	public static GameObject Spawn(GameObject obj, Vector3 pos, Quaternion rot, float activeDuration=-1){
		return instance._Spawn(obj, pos, rot, activeDuration);
	}
	public GameObject _Spawn(GameObject obj, Vector3 pos, Quaternion rot, float activeDuration=-1){
		if(obj==null){
			Debug.LogWarning("NullReferenceException: obj unspecified");
			return null;
		}
		
		
		if(disablePooling){
			GameObject objInstance = Instantiate(obj, pos, rot);
			if(activeDuration>0) Destroy(objInstance, activeDuration);
			return objInstance;
		}
		
		
		int ID=GetPoolID(obj);
		
		if(ID==-1){
			if(showUninitiatedPoolWarning)
				Debug.LogWarning("ObjectPoolManager: trying to spawn uninitiated object ("+obj+"), creating new pool");
			ID=_New(obj);
		}
		
		GameObject spawnedObj=poolList[ID].Spawn(pos, rot);
		
		if(activeDuration>0) StartCoroutine(UnspawnRoutine(spawnedObj, activeDuration));
		
		return spawnedObj;
	}
	
	
	
	IEnumerator UnspawnRoutine(GameObject spawnedObj, float activeDuration){
		if(activeDuration>0) yield return new WaitForSeconds(activeDuration);
		
		//this is to make sure stuff (esp creep) doesnt get respawn immediately and still have other stuff from previous spawn cycle refering (targeting at them)
		//example, creeps get respawned immediately upon death and shoot-object change course to hit them from way out of range
		if(spawnedObj!=null) spawnedObj.SetActive(false);
		yield return new WaitForSeconds(1);
		
		Unspawn(spawnedObj);
	}
	
	public static void Unspawn(Transform objT, float delay){ 
		instance.StartCoroutine(instance.UnspawnRoutine(objT.gameObject, delay));
	}
	public static void Unspawn(GameObject obj, float delay){ 
		instance.StartCoroutine(instance.UnspawnRoutine(obj, delay));
	}
	
	public static void Unspawn(Transform objT){ instance._Unspawn(objT.gameObject); }
	public static void Unspawn(GameObject obj){ instance._Unspawn(obj); }
	public void _Unspawn(GameObject obj){
		if(obj==null) return;
		
		if(disablePooling){
			Destroy(obj);
			return;
		}
		
		for(int i=0; i<poolList.Count; i++){
			if(poolList[i].Unspawn(obj)) return;
		}
		Destroy(obj);
	}
	
	
	public static int New(Transform objT, int count=2){ Init(); return instance._New(objT.gameObject, count); }
	public static int New(GameObject obj, int count=2){ Init(); return instance._New(obj, count); }
	public int _New(GameObject obj, int count=2){
		if(disablePooling) return 0;
		
		int ID=GetPoolID(obj);
		
		if(ID!=-1) poolList[ID].MatchObjectCount(count);
		else{
			Pool pool=new Pool();
			pool.prefab=obj;
			pool.MatchObjectCount(count);
			poolList.Add(pool);
			ID=poolList.Count-1;
		}
		
		return ID;
	}
	
	int GetPoolID(GameObject obj){
		for(int i=0; i<poolList.Count; i++){
			if(poolList[i].prefab==obj) return i;
		}
		return -1;
	}
	
}


[System.Serializable]
public class Pool{
	public GameObject prefab;
	
	public List<GameObject> inactiveList=new List<GameObject>();
	public List<GameObject> activeList=new List<GameObject>();
	
	public int cap=1000;
	
	
	public GameObject Spawn(Vector3 pos, Quaternion rot){
		GameObject obj=null;
		if(inactiveList.Count==0){
			obj=(GameObject)MonoBehaviour.Instantiate(prefab, pos, rot);
		}
		else{
			obj=inactiveList[0];
			obj.transform.parent=null;
			obj.transform.position=pos;
			obj.transform.rotation=rot;
			obj.SetActive(true);
			inactiveList.RemoveAt(0);
		}
		activeList.Add(obj);
		return obj;
	}
	
	public bool Unspawn(GameObject obj){
		if(activeList.Contains(obj)){
			obj.SetActive(false);
			obj.transform.parent=ObjectPoolManager.GetOPMTransform();
			activeList.Remove(obj);
			inactiveList.Add(obj);
			return true;
		}
		if(inactiveList.Contains(obj)){
			return true;
		}
		return false;
	}
	
	
	public void MatchObjectCount(int count){
		if(count>cap) return;
		int currentCount=GetTotalObjectCount();
		for(int i=currentCount; i<count; i++){
			GameObject obj=(GameObject)MonoBehaviour.Instantiate(prefab);
			obj.SetActive(false);
			obj.transform.parent=ObjectPoolManager.GetOPMTransform();
			inactiveList.Add(obj);
		}
	}
	
	public int GetTotalObjectCount(){
		return inactiveList.Count+activeList.Count;
	}
	
	public void Clear(){
		for(int i=0; i<inactiveList.Count; i++){
			if(inactiveList[i]!=null) MonoBehaviour.Destroy(inactiveList[i]);
		}
		for(int i=0; i<activeList.Count; i++){
			if(activeList[i]!=null) MonoBehaviour.Destroy(inactiveList[i]);
		}
	}
}

