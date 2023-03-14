using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TDTK;

namespace TDTK{

	public class PathIndicator : MonoBehaviour {
		
		public Path path;
		public LineRenderer rend;
		
		[HideInInspector] public float scrollSpeed = 0.5F;
		
		private Path nextPath;
		
		
		void Start(){
			UpdatePath();
		}
		
		void Update () {
			float offset = Time.time * -scrollSpeed;
			rend.material.mainTextureOffset=new Vector2(offset, 0);
			
			//rend.material.mainTextureScale=new Vector2(dist*0.2f, 1);
		}
		
		
		void OnEnable(){
			TDTK.onNewTowerE += UpdatePath;
		}
		void OnDisable(){
			TDTK.onNewTowerE -= UpdatePath;
		}
		
		
		void UpdatePath(UnitTower tower=null){ StartCoroutine(_UpdatePath()); }
		IEnumerator _UpdatePath(){
			yield return null;
			yield return null;	//wait 2 frames to make sure the new path has been established
			
			//float dist=path.GetDistance();
			List<Vector3> allPosList=path.GetAllWaypointList();
			
			if(!path.IsEnd()){
				nextPath=path.GetNextShortestPath();
				while(nextPath!=null){
					//dist+=nextPath.GetDistance();
					
					List<Vector3> newList=nextPath.GetAllWaypointList();
					for(int i=0; i<newList.Count; i++) allPosList.Add(newList[i]);
					
					if(!nextPath.IsEnd()) nextPath=nextPath.GetNextShortestPath();
					else{
						if(nextPath.loop && !nextPath.warpToStart){
							allPosList.Add(nextPath.GetFirstWP());
							//dist+=Vector3.Distance(nextPath.GetFirstWP(), nextPath.GetLastWP());
						}
						nextPath=null;
					}
				}
			}
			else if(path.loop && !path.warpToStart){
				allPosList.Add(path.GetFirstWP());
				//dist+=Vector3.Distance(path.GetFirstWP(), path.GetLastWP());
			}
			
			rend.positionCount=allPosList.Count;
			rend.SetPositions(allPosList.ToArray());
			
			//yield return null;
		}
		
		
	}

}