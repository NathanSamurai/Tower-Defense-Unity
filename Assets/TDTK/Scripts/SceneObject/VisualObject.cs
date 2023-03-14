using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK {
	
	[System.Serializable]
	public class VisualObject  {
		
		public GameObject obj;
		public bool autoDestroy=true;
		public float duration=1.5f;
		
		public GameObject Spawn(Vector3 pos){ return Spawn(pos, Quaternion.identity); }
		public GameObject Spawn(Vector3 pos, Quaternion rot){
			if(obj==null) return null;
			
			if(!autoDestroy) return ObjectPoolManager.Spawn(obj, pos, rot);
			else return ObjectPoolManager.Spawn(obj, pos, rot, duration);
		}
		
		
		public VisualObject Clone(){
			VisualObject clone=new VisualObject();
			clone.obj=obj;
			clone.autoDestroy=autoDestroy;
			clone.duration=duration;
			return clone;
		}
	}

}