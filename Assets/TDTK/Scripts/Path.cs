using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TDTK{

	public class Path : MonoBehaviour {
		
		#region static method
		private static List<Path> individualPathList=new List<Path>();	//all the full path, constructed in runtime
		private static List<List<Path>> completePathList=new List<List<Path>>();	//all the full path, constructed in runtime
		public static List<List<Path>> GetCompletePathList(){ return completePathList; }
		
		
		public static void Init(){	//setup all possible path combination in runtime
			individualPathList=new List<Path>();
			completePathList=new List<List<Path>>();
			Path[] pathList = FindObjectsOfType(typeof(Path)) as Path[];
			for(int i=0; i<pathList.Length; i++){
				pathList[i]._Init(); 
				individualPathList.Add(pathList[i]);
			}
			for(int i=0; i<pathList.Length; i++){ 
				if(!pathList[i].IsStart()) continue;
				pathList[i].SetupLinkRecursively();
			}
			UpdateDistToEndOnAllPath();
			if(AStar.EnableFlyingBypass()) UpdateBypassRouteOnAllPath();
			
			//WIP, not working yet
			//BuildPlatformWalkableExtension[] extension = FindObjectsOfType(typeof(BuildPlatformWalkableExtension)) as BuildPlatformWalkableExtension[];
			//for(int i=0; i<extension.Length; i++) extension[i].Init();
		}
		public void SetupLinkRecursively(List<Path> list=null){
			if(list==null) list=new List<Path>();
			list.Add(this);
			if(!loop && nextPath.Count>0){
				for(int i=0; i<nextPath.Count; i++){
					if(nextPath[i]==this) continue;
					nextPath[i].SetupLinkRecursively(new List<Path>( list ));
				}
			}
			else{
				completePathList.Add(list);
				
				#if UNITY_EDITOR
				string text="";
				for(int i=0; i<list.Count; i++){
					text+=list[i].gameObject.name+"   ";
				}
				Debug.Log(completePathList.Count+"  "+text);
				#endif
			}
		}
		
		public static List<Path> GetAllStartingPath(){	//called by SpawnManager only, to retrive all starting path
			List<Path> list=new List<Path>();
			
			if(!Application.isPlaying){	//for SpawnGenerator to generate wave not in runtime
				Path[] pathList = FindObjectsOfType(typeof(Path)) as Path[];
				for(int i=0; i<pathList.Length; i++){
					bool hasParent=false;
					for(int n=0; n<pathList.Length; n++){
						hasParent=(i!=n & pathList[n].nextPath.Contains(pathList[i]));
						if(hasParent) break;
					}
					
					if(!hasParent) list.Add(pathList[i]);
				}
			}
			else for(int i=0; i<completePathList.Count; i++) list.Add(completePathList[i][0]);
			
			return list;
		}
		
		
		public static List<Path> GetAllStartingPathOfPath(Path path, List<Path> pList=null){
			if(pList==null) pList=new List<Path>();
			
			if(path.IsStart()) pList.Add(path);
			else{
				for(int i=0; i<path.prevPath.Count; i++){
					pList=GetAllStartingPathOfPath(path.prevPath[i], pList);
				}
			}
			
			return pList;
		}
		
		
		
		public static Path GetNextShortestPath(Path path, bool bypass=false){		//get the nearest path to destination from the path nextPath
			List<int> list=new List<int>();
			for(int i=0; i<completePathList.Count; i++){			//get all potential path
				if(completePathList[i].Contains(path)) list.Add(i);
			}
			
			float shortestDist=Mathf.Infinity; int shortestIdx=0; int tgtIdx=0;
			
			bool hasValidPath=false;
			for(int j=0; j<list.Count; j++){
				bool isPathValid=true;
				List<Path> cPath=completePathList[list[j]];
				
				int idx=cPath.IndexOf(path)+1; float dist=0;
				
				for(int i=idx; i<cPath.Count; i++){
					if(!bypass){
						isPathValid=!cPath[i].IsBlocked() & (i>0 & !cPath[i].IsEntryBlocked(cPath[i-1]));
						if(!isPathValid) break;
					}
					
					int nextPathIdx1=0;
					if(cPath[i-1].HasBranchingPlatformEnd()){
						nextPathIdx1=cPath[i-1].nextPath.IndexOf(cPath[i]);
						dist+=cPath[i-1].GetDistance(false, nextPathIdx1, bypass);
					}
					dist+=Vector3.Distance(cPath[i-1].GetLastWP(nextPathIdx1), cPath[i].GetFirstWP());
					
					int nextPathIdx2=0;
					if(cPath[i].HasBranchingPlatformEnd() && i<cPath.Count-1) nextPathIdx2=cPath[i].nextPath.IndexOf(cPath[i+1]);
					dist+=cPath[i].GetDistance(false, nextPathIdx2, bypass);
				}
				
				if(isPathValid && dist<shortestDist){ 
					hasValidPath=true; shortestDist=dist; shortestIdx=list[j]; tgtIdx=idx;
				}
			}
			
			if(completePathList[shortestIdx][tgtIdx]==path) return null;
			
			if(!hasValidPath) return null;
			
			return completePathList[shortestIdx][tgtIdx];
		}
		
		
		//ignorelist is used when the function is used for determining the availability of the alternate path
		public static bool HasValidDestination(Path path, List<Path> ignoreList=null){
			if(ignoreList==null) ignoreList=new List<Path>();
			
			if(path.IsBlocked()) return false;
			
			if(path.IsEnd()) return true;
			
			for(int i=0; i<path.nextPath.Count; i++){
				if(ignoreList.Contains(path.nextPath[i])) continue;
				if(HasValidDestination(path.nextPath[i], ignoreList)) return true;
			}
			
			return false;
		}
		
		
		
		//check if a path has an alternate route, used to determine if a platform can/cannot be blocked entirely
		//ignore list is other path that uses the same platform, they are ignore in this check since they will be going through the same check
		public static bool HasAlternatePath(Path path, List<Path> ignoreList=null){
			//return false;	//enable this to prevent path blocking even when there are alternate path
			
			if(path.IsStart()) return false;
			
			for(int i=0; i<path.prevPath.Count; i++){
				Path parentPath=path.prevPath[i];
				for(int n=0; n<parentPath.nextPath.Count; n++){
					if(parentPath.nextPath[n]==path) continue;
					if(ignoreList!=null && ignoreList.Contains(parentPath.nextPath[n])) continue;
					if(parentPath.nextPath[n].hasValidDestination && !parentPath.nextPath[n].IsEntryBlocked(parentPath)){
						if(HasValidDestination(parentPath.nextPath[n], ignoreList)) return true;
					}
				}
				if(HasAlternatePath(parentPath, ignoreList)) return true;
			}
			
			return false;
		}
		
		
		//called at the initiation process of the path, (flying bypass route doesnt change during game)
		//call this if you use flying by pass and if you have change any of the waypoint position during runtime
		public static void UpdateBypassRouteOnAllPath(){
			for(int i=0; i<individualPathList.Count; i++){
				if(individualPathList[i].nextPath.Count<=1) continue;
				
				Path nextP=GetNextShortestPath(individualPathList[i], true);
				individualPathList[i].nextShortestIdxBypass=individualPathList[i].nextPath.IndexOf(nextP);
			}
		}
		
		//called whenever there's change on the path (a tower is built/sold) to recalculate the next shortest path
		public static void UpdateDistToEndOnAllPath(){
			for(int i=0; i<individualPathList.Count; i++){
				if(individualPathList[i].nextPath.Count<=1) continue;
				
				Path nextP=GetNextShortestPath(individualPathList[i]);
				int newIdx=individualPathList[i].nextPath.IndexOf(nextP);
				
				if(newIdx!=individualPathList[i].nextShortestIdx){
					individualPathList[i].nextShortestIdx=newIdx;
					individualPathList[i].UpdateCreepPathInBranchingPlatformEnd();	//inform all unit on the last platform to change their path
				}
			}
			
			for(int i=0; i<individualPathList.Count; i++){
				individualPathList[i].hasValidDestination=HasValidDestination(individualPathList[i]);
				individualPathList[i].UpdateDistToEnd();
			}
		}
		#endregion
		
		
		
		public float cachedDistToEnd=0;
		public float GetDistanceToEnd(bool recalculate=false){
			if(recalculate) UpdateDistToEnd();
			return cachedDistToEnd;
		}
		public void UpdateDistToEnd(){	//the result is only correct if nextShortestIdx is true, run UpdateDistToFinalDestOnAllPath
			cachedDistToEnd=CalculateDistToEnd();
		}
		public float CalculateDistToEnd(float dist=0){	//this is called recursively
			if(!hasValidDestination) return Mathf.Infinity;
			dist+=GetDistance();
			if(IsEnd()) return dist;
			dist+=Vector3.Distance(GetLastWP(), nextPath[nextShortestIdx].GetFirstWP());
			return nextPath[nextShortestIdx].CalculateDistToEnd(dist);
		}
		
		
		public int nextShortestIdx=0;
		public Path GetNextShortestPath(){ return nextPath[nextShortestIdx]; }
		public int nextShortestIdxBypass=0;
		public Path GetNextShortestPathFlying(){ return nextPath[nextShortestIdxBypass]; }
		
		
		private List<bool> blockedEntryPoint=new List<bool>();	//correspond to each prevPath
		public List<int> blockedSec=new List<int>();
		public bool hasValidDestination=true;
		
		public float dynamicOffset=0;
		//public static bool alignOffsetToPathDir=false;
		
		public bool loop=false;
		public bool warpToStart=false;
		
		public List<Transform> waypointTList=new List<Transform>();
		public List<WPSection> wpSecList=new List<WPSection>();
		public WPSection GetWpSec(int idx){ return wpSecList[idx]; }
		
		public Vector3 GetFirstWP(){ return waypointTList[0].position; }
		public Vector3 GetLastWP(int idx=0){ return _GetLastWP(idx); }//waypointTList[waypointTList.Count-1].position; }
		
		private List<Path> prevPath=new List<Path>();
		public List<Path> nextPath=new List<Path>();
		
		public bool IsStart(){ return prevPath.Count==0 ? true : false ;}
		public bool IsEnd(){ return loop || nextPath.Count==0 ? true : false ;}
		
		public int GetWPCount(){ return wpSecList.Count; }
		public List<Vector3> GetWP(int idx, bool bypass=false){ return wpSecList[idx].GetWaypointList(nextShortestIdx, bypass); }
		
		
		public bool HasBranchingPlatformEnd(){ return wpSecList.Count>0 && wpSecList[wpSecList.Count-1].branch; }
		public Vector3 _GetLastWP(int idx){
			if(!HasBranchingPlatformEnd()) return waypointTList[waypointTList.Count-1].position;
			return wpSecList[wpSecList.Count-1].GetExitPoint(idx);
		}
		
		
		public Vector3 GetSpawnPoint(){ return wpSecList[0].GetEntryPoint(); }
		
		
		
		public void _Init(){
			creepOnPath.Clear();
			
			dynamicOffset=Mathf.Min(dynamicOffset, TowerManager.GetGridSize()*0.45f);
			
			if(!loop){
				for(int i=0; i<nextPath.Count; i++){
					if(nextPath[i]==null){ nextPath.RemoveAt(i); i-=1; continue; }
					nextPath[i].prevPath.Add(this);
					nextPath[i].blockedEntryPoint.Add(false);
				}
			}
			
			if(fitPathToTerrain) FitPathToTerrain();
			
			for(int i=0; i<waypointTList.Count; i++){
				if(waypointTList[i]==null){ waypointTList.RemoveAt(i); i-=1; continue; }
				
				bool isPlatform=false;
				if(waypointTList[i].gameObject.layer==TDTK.GetLayerPlatform()){
					BuildPlatform platform=waypointTList[i].GetComponent<BuildPlatform>();
					if(platform!=null){
						if(i>0 && i<waypointTList.Count-1){
							isPlatform=true;
							wpSecList.Add(new WPSection(i, this, platform, waypointTList[i-1].position, waypointTList[i+1].position));
							platform.AddPath(this);
						}
						if(i==waypointTList.Count-1 && nextPath.Count>0){
							isPlatform=true;
							
							List<Vector3> nextPosList=new List<Vector3>();
							for(int n=0; n<nextPath.Count; n++) nextPosList.Add(nextPath[n].GetFirstWP());
							
							wpSecList.Add(new WPSection(i, this, platform, waypointTList[i-1].position, nextPosList));
							platform.AddPath(this);
						}
					}
				}
					
				if(!isPlatform) wpSecList.Add(new WPSection(i, this, waypointTList[i]));
			}
			
			UpdateDistance();			//Update path distance
			UpdateDistance(true);		//for bypass
		}
		
		
		[Space(8)]
		public bool fitPathToTerrain;
		public float fitPath_stepSize=0.25f;
		public float fitPath_heightOffset=0;
		
		//[ContextMenu("CastDown")]
		public void FitPathToTerrain(){
			for(int i=0; i<waypointTList.Count; i++){
				if(waypointTList[i]!=null) continue;
				waypointTList.RemoveAt(i); i-=1;
			}
			
			Vector3 lastPoint=waypointTList[0].position;
			List<Transform> newList=new List<Transform>{ new GameObject("wp0").transform };
			newList[newList.Count-1].position=lastPoint;
			
			int counter=0;
			
			for(int i=1; i<waypointTList.Count; i++){
				if(waypointTList[i].gameObject.layer==TDTK.GetLayerPlatform() || 
					waypointTList[i-1].gameObject.layer==TDTK.GetLayerPlatform()){
						counter+=1;	
						newList.Add(waypointTList[i]);
						continue;
				}
				
				lastPoint=waypointTList[i-1].position;
				
				while(true){
					float dist=Vector3.Distance(waypointTList[i].position, lastPoint);
					Vector3 v=(waypointTList[i].position-waypointTList[i-1].position).normalized * Mathf.Min(fitPath_stepSize, dist);
					
					lastPoint=lastPoint+v;	Vector3 hitPoint=lastPoint;
					
					//if(showGizmo) Debug.DrawLine(lastPoint+new Vector3(0, 2, 0), lastPoint-new Vector3(0, 2, 0), Color.green, 1);
					
					RaycastHit hit;
					if(Physics.Linecast(lastPoint+new Vector3(0, 999, 0), lastPoint-new Vector3(0, 999, 0), out hit)){
						hitPoint=hit.point+new Vector3(0, fitPath_heightOffset, 0);
						//if(showGizmo) Debug.Log(counter+"  Hit - "+hitPoint);
					}
					
					counter+=1;
					newList.Add(new GameObject("wp"+counter).transform);
					newList[newList.Count-1].position=hitPoint;
					
					if(dist<=fitPath_stepSize) break;
				}
			}
			
			for(int i=0; i<newList.Count; i++) newList[i].parent=transform;
			
			//for(int i=0; i<waypointTList.Count; i++) DestroyImmediate(waypointTList[i].gameObject);
			
			waypointTList=newList;
		}
		
		
		
		[Space(8)]
		public List<float> cachedDistList=new List<float>();
		
		private float cachedDistance;	//store the path total length in  runtime
		private float cachedDistanceBypass;	//store the path total length bypassing all the obstacle in runtime, for flying creep
		public float GetDistance(bool recalculate=false, int nextPathIdx=0, bool bypass=false){
			if(recalculate) UpdateDistance(bypass);
			
			if(bypass) return cachedDistanceBypass;
			
			if(nextPathIdx<0) nextPathIdx=nextShortestIdx;
			
			return !HasBranchingPlatformEnd() ? cachedDistance : cachedDistList[nextPathIdx];
		}
		public void UpdateDistance(bool bypass=false){
			if(!bypass){
				cachedDistance=0;
				for(int i=1; i<wpSecList.Count; i++){
					cachedDistance+=Vector3.Distance(wpSecList[i-1].GetExitPoint(), wpSecList[i].GetEntryPoint());
					if(wpSecList[i].isPlatform){
						if(wpSecList[i].branch){
							
							cachedDistList=new List<float>();
							while(cachedDistList.Count<nextPath.Count) cachedDistList.Add(cachedDistance);
							
							for(int j=0; j<nextPath.Count; j++){
								List<Vector3> subPath=wpSecList[i].GetWaypointList(j);
								for(int n=1; n<subPath.Count; n++) cachedDistList[j]+=Vector3.Distance(subPath[n-1], subPath[n]);
							}
						}
						else{
							List<Vector3> subPath=wpSecList[i].GetWaypointList();
							for(int n=1; n<subPath.Count; n++) cachedDistance+=Vector3.Distance(subPath[n-1], subPath[n]);
						}
					}
				}
				//Debug.Log(name+"   "+cachedDistance);
			}
			else{
				cachedDistanceBypass=0;
				for(int i=1; i<wpSecList.Count; i++){
					cachedDistanceBypass+=Vector3.Distance(wpSecList[i-1].GetExitPoint(), wpSecList[i].GetEntryPoint());
				}
				//Debug.Log(name+"   "+cachedDistanceBypass);
			}
		}
		
		
		
		
		/*
		public float GetDistanceFromPoint(int wpIdx, int subWpIdx, Vector3 curPos){	//for creep, not in used yet
			List<Vector3> subList=GetWP(wpIdx);
			float dist=Vector3.Distance(curPos, subList[subWpIdx]);
			for(int i=subWpIdx; i<subList.Count-1; i++){
				dist+=Vector3.Distance(subList[i], subList[i+1]);
			}
			for(int i=wpIdx+1; i<wpSecList.Count; i++){
				dist+=Vector3.Distance(wpSecList[i-1].GetExitPoint(), wpSecList[i].GetEntryPoint());
				if(wpSecList[i].isPlatform){
					List<Vector3> subPath=wpSecList[i].wpList;
					for(int n=1; n<subPath.Count; n++) dist+=Vector3.Distance(subPath[n-1], subPath[n]);
				}
			}
			return dist;
		}
		*/
		
		
		
		
		public void RemoveBlockedEntry(Path prevP){ blockedEntryPoint[prevPath.IndexOf(prevP)]=false; }
		public void AddBlockedEntry(Path prevP){ blockedEntryPoint[prevPath.IndexOf(prevP)]=true; }
		public bool IsEntryBlocked(Path prevP){ return blockedEntryPoint[prevPath.IndexOf(prevP)]; }
		
		public void AddBlockSec(int idx){ if(!blockedSec.Contains(idx)) blockedSec.Add(idx); }
		public void RemoveBlockSec(int idx){ blockedSec.Remove(idx); }
		public bool IsBlocked(){ return blockedSec.Count>0; }
		public bool IsSecBlocked(int idx){ return blockedSec.Contains(idx); }
		
		
		public List<UnitCreep> creepOnPath=new List<UnitCreep>();
		public void OnCreepEnter(UnitCreep unit){ creepOnPath.Add(unit); }
		public void OnCreepExit(UnitCreep unit){ creepOnPath.Remove(unit); }
		
		
		//recalculate path on one of the waypoint platform, called when tower is built or sold
		public void UpdatePlatformPath(BuildPlatform platform, UnitTower tower=null){	
			bool requireUpdate=false;
			for(int i=0; i<wpSecList.Count; i++){
				if(wpSecList[i].platform==platform){
					if(tower!=null){	//if the tower isn't built in the path, there's no need to update the path
						bool inPath=false;
						int count=wpSecList[i].branch ? nextPath.Count : 1;
						
						for(int j=0; j<count; j++){
							List<Vector3> subPath=wpSecList[i].GetWaypointList(j);
							for(int n=0; n<subPath.Count; n++){
								if(tower.GetPos()==subPath[n]){ inPath=true; break; }
							
								if(AStar.EnableDiagonal() || AStar.EnableSmoothing()){
									if(n<subPath.Count-1){
										if(subPath[n].x!=subPath[n+1].x && subPath[n].z!=subPath[n+1].z){
											float gridSize=TowerManager.GetGridSize()*(AStar.EnableFreeSmoothing() ? 1.75f : 1.15f);
											if(Vector3.Distance(subPath[n], tower.GetPos())<gridSize){ 
												inPath=true; break; 
											}
										}
									}
								}
							}
						}
						
						if(!inPath) continue;
					}
					
					wpSecList[i].UpdatePlatformPath();
					
					bool pathIsBlocked=wpSecList[i].wpList.Count==0;
					if(i==wpSecList.Count-1 && HasBranchingPlatformEnd()){
						for(int n=0; n<nextPath.Count; n++){
							if(wpSecList[i].wpListList[n].Count>=0){ pathIsBlocked=false; break; }
						}
					}
					
					if(pathIsBlocked){	//if(wpSecList[i].wpList.Count==0){
						//add to a list and then call reverse later, 
						//in case calling Reverse() on the unit cause it to reverse to prev path intantly and remove itself on the path
						List<UnitCreep> reverseList=new List<UnitCreep>();	
						for(int n=0; n<creepOnPath.Count; n++){
							if(creepOnPath[n]==null) continue;
							if(creepOnPath[n].wpIdx>i) continue;
							if(creepOnPath[n].wpIdx==0){				//fix added on 18Jan2018
								reverseList.Add(creepOnPath[n]);
								continue;
							}
							List<Vector3> newPath=wpSecList[i].GetPathForUnit(creepOnPath[n]);
							if(newPath.Count==0) reverseList.Add(creepOnPath[n]); 
						}
						for(int n=0; n<reverseList.Count; n++) reverseList[n].Reverse();
					}
					else{
						for(int n=0; n<creepOnPath.Count; n++){
							if(creepOnPath[n].wpIdx!=i) continue;
							creepOnPath[n].ForceAltPath();
						}
					}
					
					requireUpdate=true;
				}
			}
			
			if(requireUpdate) UpdateDistance();
		}
		
		
		//called when next shortest path is changed, to update creep path on the final platform
		public void UpdateCreepPathInBranchingPlatformEnd(){
			if(!HasBranchingPlatformEnd()) return;
			
			for(int n=0; n<creepOnPath.Count; n++){
				if(creepOnPath[n].wpIdx!=wpSecList.Count-1) continue;
				creepOnPath[n].ForceAltPath();
			}
		}
		
		
		//use to check if a node can be used for building
		//There's 2 pass of this, first to determine which path of the platform is blocked, next to determine if there's any alt-path to those potentially blocked path
		public bool CheckForNode(BuildPlatform platform, int nodeIdx){	
			for(int i=0; i<wpSecList.Count; i++){
				if(wpSecList[i].platform==platform){ 
					if(!wpSecList[i].CheckForNode(nodeIdx)) return false;
				}
			}
			return true;
		}
		
		public bool CheckForNodeAltPath(BuildPlatform platform, int nodeIdx, List<Path> ignoreList){	
			for(int i=0; i<wpSecList.Count; i++){
				if(wpSecList[i].platform==platform){ if(!wpSecList[i].CheckForNodeAlt(nodeIdx, ignoreList)) return false; }
			}
			return true;
		}
		
		
		public List<Vector3> GetAllWaypointList(){
			List<Vector3> list=new List<Vector3>();
			list.Add(wpSecList[0].GetExitPoint());
			for(int i=1; i<wpSecList.Count; i++){
				list.Add(wpSecList[i].GetEntryPoint());
				if(wpSecList[i].isPlatform){
					if(!wpSecList[i].branch){
						List<Vector3> subPath=wpSecList[i].wpList;
						for(int n=1; n<subPath.Count; n++) list.Add(subPath[n]);
					}
					else{
						List<Vector3> subPath=wpSecList[i].GetWaypointList(nextShortestIdx);
						for(int n=1; n<subPath.Count; n++) list.Add(subPath[n]);
					}
				}
			}
			return list;
		}
		
		
		//Obsolete, no longer in used, check GetPathOffset in UnitCreep.cs instead
		/*
		public Vector3 GetPathOffset(int wpIdx, int subWpIdx, float magnitude, Path prevPath=null, List<Vector3> subList=null){
			//return Vector3.zero;
			
			Vector3 lastP=Vector3.zero;
			Vector3 thisP=Vector3.zero;
			Vector3 nextP=Vector3.zero;
			
			if(subList==null) subList=wpSecList[wpIdx].GetWaypointList();
			
			if(subList.Count==1){
				if(wpIdx==0){	//first WP
					if(prevPath!=null){
						lastP=prevPath.wpSecList[prevPath.wpSecList.Count-1].GetExitPoint();
						thisP=subList[0];
						nextP=wpSecList[wpIdx+1].GetEntryPoint();
					}
					else{
						if(loop){
							lastP=wpSecList[wpSecList.Count-1].GetExitPoint();
							thisP=subList[0];
							nextP=wpSecList[wpIdx+1].GetEntryPoint();
						}
						else{
							lastP=subList[0];
							thisP=(subList[0]+wpSecList[wpIdx+1].GetEntryPoint())/2;
							nextP=wpSecList[wpIdx+1].GetEntryPoint();
						}
					}
				}
				else if(wpIdx==wpSecList.Count-1){	//last WP
					if(!IsEnd()){
						lastP=wpSecList[wpIdx-1].GetExitPoint();
						thisP=subList[0];
						nextP=GetNextShortestPath().wpSecList[0].GetEntryPoint();
					}
					else{
						if(loop){
							lastP=wpSecList[wpIdx-1].GetExitPoint();
							thisP=subList[0];
							nextP=wpSecList[0].GetEntryPoint();
						}
						else{
							lastP=wpSecList[wpIdx-1].GetExitPoint();
							thisP=(subList[0]+wpSecList[wpIdx-1].GetExitPoint())/2;
							nextP=subList[0];
						}
					}
				}
				else{
					lastP=wpSecList[wpIdx-1].GetExitPoint();
					thisP=subList[0];
					nextP=wpSecList[wpIdx+1].GetEntryPoint();
				}
			}
			else{
				//Debug.Log(subWpIdx+"   "+subList.Count);
				if(subList.Count==0){ Debug.Log("subList is empty?");	return Vector3.zero;}
				
				//return Vector3.zero;
				if(subWpIdx==0){
					lastP=wpSecList[wpIdx-1].GetExitPoint();
					thisP=subList[0];
					nextP=subList[1];
				}
				else if(subWpIdx==subList.Count-1){
					lastP=subList[subWpIdx-1];
					thisP=subList[subWpIdx];
					nextP=wpSecList[wpIdx].GetEntryPoint();
				}
				else{
					lastP=subList[subWpIdx-1];
					thisP=subList[subWpIdx];
					nextP=subList[subWpIdx+1];
				}
			}
			
			
			//Vector3 p1=new Vector3(.25f, 0, .25f);
			//Vector3 p2=new Vector3(-.25f, 0, -.25f);
			//Debug.DrawLine(lastP+p1, lastP+p2, Color.green, 1);
			//Debug.DrawLine(thisP+p1, thisP+p2, Color.green, 1);
			//Debug.DrawLine(nextP+p1, nextP+p2, Color.green, 1);
			
			//Vector3 p3=new Vector3(.25f, 0, -.25f);
			//Vector3 p4=new Vector3(-.25f, 0, .25f);
			//Debug.DrawLine(lastP+p3, lastP+p4, Color.green, 1);
			//Debug.DrawLine(thisP+p3, thisP+p4, Color.green, 1);
			//Debug.DrawLine(nextP+p3, nextP+p4, Color.green, 1);
			//Debug.Log(wpIdx+"    "+lastP+"   "+thisP+"   "+nextP+"    "+wpSecList[wpIdx].GetWaypointList()[0]);
			
			
			lastP.y=0;	thisP.y=0;	nextP.y=0;
			
			Vector3 dir1=(thisP-lastP).normalized;
			Vector3 dir2=(nextP-thisP).normalized;
			Vector3 dir=(dir1+dir2).normalized;
			
			float angle=Vector3.Angle(dir1, dir2);
			
			return new Vector3(-dir.z, 0, dir.x)*magnitude*(1+0.5f*Mathf.Sin(angle*Mathf.Deg2Rad));
		}
		*/
		
		
		#region context menu
		[ContextMenu ("Auto Fill Waypoint")]
		public void FillWP(){
			if(Application.isPlaying) return;
			waypointTList=new List<Transform>();
			foreach(Transform child in transform) waypointTList.Add(child);
			//ReorientWP();
		}
		//[ContextMenu ("Reorient Waypoint")]
		//void ReorientWP(){
			//for(int i=0; i<waypointTList.Count; i++){
			//	if(waypointTList[i].gameObject.layer==TDTK.GetLayerPlatform()) continue;
			//	if(i<waypointTList.Count-1) waypointTList[i].LookAt(waypointTList[i+1]);
			//	else waypointTList[i].rotation=waypointTList[i-1].rotation;
			//}
		//}
		#endregion
		
		
		//have path connect to path
		//end of path1 can be connect to path2.1, and path2.2, and path2.3 and so on to branch out
		//end of path2.n can be connected to start path3 (rejoin), or just goes on as independent path
		[Space(10)] 
		public bool showGizmo=true;
		public Color gizmoColor=Color.blue;
		
		private float pointRadius=.1f;
		
		void OnDrawGizmos(){
			if(!showGizmo) return;
			if(waypointTList.Count<1) return;
			
			
			Gizmos.color=gizmoColor;
			
			//draw waypoint on path
			if(!Application.isPlaying){
				Transform thisWP=null;	int startingIdx=-1;
				for(int i=0; i<waypointTList.Count; i++){
					if(waypointTList[i]==null) continue;
					Gizmos.DrawSphere(waypointTList[i].position, pointRadius);
					thisWP=waypointTList[i]; startingIdx=i;	break;
				}
				
				if(startingIdx>=0){
					for(int i=startingIdx+1; i<waypointTList.Count; i++){
						if(waypointTList[i]==null) continue;
						Gizmos.DrawSphere(waypointTList[i].position, pointRadius);
						Gizmos.DrawLine(thisWP.position, waypointTList[i].position);
						thisWP=waypointTList[i];
					}
				}
				
				//for(int i=1; i<waypointTList.Count; i++){
				//	if(waypointTList[i-1]==null || waypointTList[i]==null) continue;
				//	if(i==1) Gizmos.DrawSphere(waypointTList[i-1].position, pointRadius);
				//	Gizmos.DrawSphere(waypointTList[i].position, pointRadius);
				//	Gizmos.DrawLine(waypointTList[i-1].position, waypointTList[i].position);
				//}
			}
			else{
				for(int i=1; i<wpSecList.Count; i++){
					Gizmos.DrawLine(wpSecList[i-1].GetExitPoint(), wpSecList[i].GetEntryPoint());
					if(wpSecList[i].isPlatform){
						if(!wpSecList[i].branch){
							List<Vector3> subPath=wpSecList[i].wpList;
							for(int n=1; n<subPath.Count; n++){
								Gizmos.DrawLine(subPath[n-1], subPath[n]);
								Gizmos.DrawSphere(subPath[n], pointRadius);
							}
							Gizmos.DrawSphere(wpSecList[i].GetEntryPoint(), pointRadius);
							Gizmos.DrawSphere(wpSecList[i].GetExitPoint(), pointRadius);
						}
						else{
							for(int j=0; j<wpSecList[i].wpListList.Count; j++){
								List<Vector3> subPath=wpSecList[i].wpListList[j];
								for(int n=1; n<subPath.Count; n++) Gizmos.DrawLine(subPath[n-1], subPath[n]);
								Gizmos.DrawSphere(wpSecList[i].GetEntryPoint(), pointRadius);
								Gizmos.DrawSphere(wpSecList[i].GetExitPoint(), pointRadius);
							}
						}
					}
					else Gizmos.DrawSphere(wpSecList[i].wpT.position, pointRadius);
				}
			}
			
			
			//draw connection to next path
			if(Application.isPlaying && wpSecList[wpSecList.Count-1].branch){
				for(int i=0; i<nextPath.Count; i++){
					Gizmos.DrawLine(wpSecList[wpSecList.Count-1].GetExitPoint(i), nextPath[i].waypointTList[0].position);
				}
			}
			else{
				Transform lastWP=null;
				for(int i=waypointTList.Count-1; i>=0; i--){
					if(waypointTList[i]==null) continue;
					lastWP=waypointTList[i]; break;
				}
				if(lastWP==null) return;
				
				if(IsEnd()){
					Gizmos.color=Color.red;
					Gizmos.DrawSphere(lastWP.position, pointRadius*1.2f);
					Gizmos.color=gizmoColor;
				}
				else{
					for(int i=0; i<nextPath.Count; i++){
						if(nextPath[i]==null) continue;
						
						Transform nextWP=null;
						for(int n=0; n<nextPath[i].waypointTList.Count; n++){
							if(nextPath[i].waypointTList[n]==null) continue;
							nextWP=nextPath[i].waypointTList[n]; break;
						}
						if(nextWP==null) return;
						
						Gizmos.DrawLine(lastWP.position, nextWP.position);
					}
				}
			}
		}
		
		
	}

}