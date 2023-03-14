using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{
	
	
	public class WPSection{
		public Path parentPath;
		public int index;	//wpSec index in the path
		
		public Transform wpT;
		
		public bool isPlatform=false;
		public BuildPlatform platform;
		
		public int pathID=0;
		public List<Vector3> wpList=new List<Vector3>();
		public List<Vector3> wpListBypass=new List<Vector3>();
		
		public int entryNodeID;
		public int exitNodeID;
		
		public List<List<Vector3>> wpListList=new List<List<Vector3>>();
		public List<int> exitNodeIDList=new List<int>();
		
		public bool branch=false;
		
		
		//for a single non-platform waypoint
		public WPSection(int i, Path path, Transform t){ 
			index=i; parentPath=path; wpT=t;	wpList=new List<Vector3>{ wpT.position };
		}
		//for a platform waypoint
		public WPSection(int i, Path path, BuildPlatform p, Vector3 entryP, Vector3 exitP){ 
			index=i; parentPath=path; isPlatform=true; platform=p; platform.walkable=true; wpT=platform.transform;
			
			entryNodeID=platform.GetNearestNode(entryP, 1).ID;	//set search mode to 1 to look for walkable node only
			exitNodeID=platform.GetNearestNode(exitP, 1).ID;
			wpList=platform.searchPath(entryNodeID, exitNodeID);
			
			wpListBypass=new List<Vector3>{ GetEntryPoint(), GetExitPoint() };
		}
		
		//for last waypoint section on a path that is a build platform
		public WPSection(int i, Path path, BuildPlatform p, Vector3 entryP, List<Vector3> exitPList){
			index=i; parentPath=path; isPlatform=true; platform=p; platform.walkable=true; wpT=platform.transform;	branch=true;
			
			entryNodeID=platform.GetNearestNode(entryP, 1).ID;	//set search mode to 1 to look for walkable node only
			
			for(int n=0; n<exitPList.Count; n++){
				exitNodeIDList.Add(platform.GetNearestNode(exitPList[n], 1).ID);
				wpListList.Add(platform.searchPath(entryNodeID, exitNodeIDList[n]));
			}
			
			float minDist=Mathf.Infinity;	int shortestID=0;
			for(int n=0; n<exitNodeIDList.Count; n++){
				float dist=Vector3.Distance(GetEntryPoint(), platform.GetNode(exitNodeIDList[n]).pos);
				if(dist<minDist){ minDist=dist;	shortestID=n; }
			}
			wpListBypass=new List<Vector3>{ GetEntryPoint(), platform.GetNode(exitNodeIDList[shortestID]).pos };
		}
		
		
		
		public List<Vector3> GetWaypointList(int nextPathIdx=0, bool bypass=false){
			if(isPlatform){
				if(!bypass){
					if(branch) return wpListList[nextPathIdx];
					else return wpList;
				}
				else{
					return wpListBypass;
				}
			}
			else return new List<Vector3>{ wpT.position };
		}
		
		
		public Vector3 GetEntryPoint(){
			if(isPlatform) return platform.GetNode(entryNodeID).pos;
			else return wpT.position;
		}
		public Vector3 GetExitPoint(int i=0){
			if(isPlatform){
				if(branch) return platform.GetNode(exitNodeIDList[i]).pos;
				else return platform.GetNode(exitNodeID).pos;
			}
			else return wpT.position;
		}
		
		
		public void UpdatePlatformPath(){
			if(branch){
				bool noPath=true;
				for(int i=0; i<exitNodeIDList.Count; i++){
					wpListList[i]=platform.searchPath(entryNodeID, exitNodeIDList[i]);
					if(wpListList[i].Count>0){ 
						parentPath.nextPath[i].RemoveBlockedEntry(parentPath);
						noPath=false;
					}
					else{
						parentPath.nextPath[i].AddBlockedEntry(parentPath);
					}
				}
				if(noPath) parentPath.AddBlockSec(index);
				else parentPath.RemoveBlockSec(index);
			}
			else{
				wpList=platform.searchPath(entryNodeID, exitNodeID);
				if(wpList.Count==0) parentPath.AddBlockSec(index);
				else parentPath.RemoveBlockSec(index);
			}
			
			parentPath.UpdateDistance();
		}
		
		
		public List<Vector3> GetPathForUnit(UnitCreep unit, bool toExit=true, int blockIdx=-1){	//for creep on platform to retrieve an exit pointRadius
			int startNodeID=GetUnitNearestNodeID(unit);
			
			if(!toExit) return platform.searchPath(startNodeID, entryNodeID, blockIdx);
			
			if(branch) return platform.searchPath(startNodeID, exitNodeIDList[parentPath.nextShortestIdx], blockIdx);
			else return platform.searchPath(startNodeID, exitNodeID, blockIdx);
		}
		
		
		public bool HasExitPath(UnitCreep unit, int blockIdx=-1){
			if(branch) return true;
			
			int startNodeID=GetUnitNearestNodeID(unit);
			if(platform.searchPath(startNodeID, entryNodeID, blockIdx).Count>0) return true;
			if(platform.searchPath(startNodeID, exitNodeID, blockIdx).Count>0) return true;
			return false;
		}
		
		public int GetUnitNearestNodeID(UnitCreep unit){
			NodeTD n1=platform.GetNearestNode(unit.GetTargetPos(), 1);	//get walkable only
			NodeTD n2=platform.GetNearestNode(unit.GetLastTargetPos(), 1);
			return (Vector3.Distance(unit.GetPos(), n1.pos)>Vector3.Distance(unit.GetPos(), n2.pos)) ? n2.ID : n1.ID ;
		}
		
		
		//Called from BuildPlatform, check if build on the node will blocked the existing path
		public bool CheckForNode(int nodeIdx){
			bool blockSec=false;
			
			if(branch){
				if(nodeIdx==entryNodeID) return false;
				
				for(int i=0; i<exitNodeIDList.Count; i++){
					if(nodeIdx==exitNodeIDList[i]) continue;
					blockSec|=(platform.searchPath(entryNodeID, exitNodeIDList[i], nodeIdx, false).Count>0);
					blockSec &= parentPath.nextPath[i].hasValidDestination;
					if(blockSec) break;
				}
				return blockSec;
			}
			
			blockSec=(nodeIdx==entryNodeID || nodeIdx==exitNodeID);
			if(!blockSec) blockSec=platform.searchPath(entryNodeID, exitNodeID, nodeIdx, false).Count==0;
			return !blockSec;
		}
		//Check if there's any alternate path if the node is blocked
		public bool CheckForNodeAlt(int nodeIdx, List<Path> ignoreList){
			if(Path.HasAlternatePath(parentPath, ignoreList)){
				List<UnitCreep> cList=parentPath.creepOnPath;
				if(cList.Count<=0) return true;
				
				if(parentPath.blockedSec.Count==0){
					
				}
				else if(parentPath.blockedSec.Count==1){
					int existingBlock=parentPath.blockedSec[0];
					for(int i=0; i<cList.Count; i++){
						if(index==existingBlock){
							if(cList[i].wpIdx==index && !HasExitPath(cList[i], nodeIdx)) return false;
						}
						else if(existingBlock<index){
							if(cList[i].wpIdx>existingBlock && cList[i].wpIdx<index) return false;
							else if(cList[i].wpIdx==index && GetPathForUnit(cList[i], true, nodeIdx).Count<=0) return false;
							else if(cList[i].wpIdx==existingBlock && parentPath.GetWpSec(existingBlock).GetPathForUnit(cList[i], false).Count<=0) return false;
						}
						else if(existingBlock>index){
							if(cList[i].wpIdx<existingBlock && cList[i].wpIdx>index) return false;
							else if(cList[i].wpIdx==index && GetPathForUnit(cList[i], false, nodeIdx).Count<=0) return false;
							else if(cList[i].wpIdx==existingBlock && parentPath.GetWpSec(existingBlock).GetPathForUnit(cList[i], true).Count<=0) return false;
						}
					}
				}
				else if(parentPath.blockedSec.Count>1){
					int first=parentPath.GetWPCount()-1; int last=0;
					for(int i=0; i<parentPath.blockedSec.Count; i++){
						if(parentPath.blockedSec[i]<first) first=parentPath.blockedSec[i];
						if(parentPath.blockedSec[i]>last) last=parentPath.blockedSec[i];
					}
					
					if(index<=first){
						for(int i=0; i<cList.Count; i++){
							if(index==first){
								if(cList[i].wpIdx==index && !HasExitPath(cList[i], nodeIdx)) return false;
							}
							else if(first>index){
								if(cList[i].wpIdx<first && cList[i].wpIdx>index) return false;
								else if(cList[i].wpIdx==index && GetPathForUnit(cList[i], false, nodeIdx).Count<=0) return false;
								else if(cList[i].wpIdx==first && parentPath.GetWpSec(first).GetPathForUnit(cList[i], true).Count<=0) return false;
							}
						}
					}
					else if(index>=last){
						for(int i=0; i<cList.Count; i++){
							if(index==last){
								if(cList[i].wpIdx==index && !HasExitPath(cList[i], nodeIdx)) return false;
							}
							else if(last<index){
								if(cList[i].wpIdx>last && cList[i].wpIdx<index) return false;
								else if(cList[i].wpIdx==index && GetPathForUnit(cList[i], true, nodeIdx).Count<=0) return false;
								else if(cList[i].wpIdx==last && parentPath.GetWpSec(last).GetPathForUnit(cList[i], false).Count<=0) return false;
							}
						}
					}
				}
				return true;	//can use alternate path
			}
			
			return false;	//path will be blocked and there's no alternate path
		}
	}
	
	
}
