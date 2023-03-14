using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//#pragma warning disable 0168 // variable declared but not used.
//#pragma warning disable 0219 // variable assigned but not used.
//#pragma warning disable 0414 // private field assigned but not used.

namespace TDTK {

	public delegate void SetPathCallbackTD(List<Vector3> wp);
	
	
	public class AStar : MonoBehaviour {
		
		private static AStar instance;
		
		[Tooltip("Check to enable flying creep to bypass any obstacle\nWhen enabled, flying creep will always cut a straight-line through platform and move through shortest path even when it's are blocked")]
		public bool flyingBypass=false;
		public static bool EnableFlyingBypass(){ return instance!=null ? instance.flyingBypass : true; }
		
		
		[Tooltip("Check to enable diagonal neighbour, creep will try to cut diagonally when possible")]
		public bool diagonalNeighbour=false;
		public static bool EnableDiagonal(){ return instance!=null ? instance.diagonalNeighbour : false; }
		
		
		public enum _Smoothing{ None, Diagonal, Free }
		[Tooltip("None - no path smoothing\n\nDiagonal - creep will try to cut diagonally when possible\n\nFree - creep will try to cut to the shortest straight when possible")]
		[Space(10)] public _Smoothing pSmoothing;
		public static bool EnableSmoothing(){ return instance!=null ? instance.pSmoothing!=_Smoothing.None : false; }
		public static bool EnableDiagonalSmoothing(){ return instance!=null ? instance.pSmoothing==_Smoothing.Diagonal : false; }
		public static bool EnableFreeSmoothing(){ return instance!=null ? instance.pSmoothing==_Smoothing.Free : false; }
		
		
		//~ [Tooltip("Check to enable path smoothing, creep will try to cut diagonally when possible\nOnly applicate if diagonal neighbour is not enabled")]
		//~ public bool pathSmoothing=false;
		//~ public static bool EnablePathSmoothing(){ return instance!=null ? instance.pathSmoothing : false; }
		
		
		//~ [Tooltip("Check to enable path smoothing, creep will try to cut diagonally when possible\nOnly applicate if diagonal neighbour is not enabled")]
		//~ public bool pathSmoothingFree=false;
		//~ public static bool EnablePathSmoothingF(){ return instance!=null ? instance.pathSmoothingFree : false; }
		
		
		
		
		public void Awake(){
			if(instance!=null) return;
			instance=this;
		}
		
		
		public static void Init(){
			if(instance!=null) return;
			
			instance = (AStar)FindObjectOfType(typeof(AStar));
			
			if(instance==null){
				GameObject obj=new GameObject();
				instance=obj.AddComponent<AStar>();
				obj.name="AStar";
			}
		}
		
		
		public static void ConnectPlatform(BuildPlatform plat1, BuildPlatform plat2, List<Vector3> wpList){
			
		}
		
		
		private static Transform refT;
		public static NodeTD[] GenerateNode(BuildPlatform platform, float gridSize=1, float heightOffset=0){
			if(refT==null){
				refT=new GameObject("RefT").transform;
				refT.parent=TowerManager.GetInstance().transform;
			}
			
			Transform platformT=platform.transform;
			
			float scaleX=platformT.localScale.x;
			float scaleZ=platformT.localScale.y;
			
			int countX=(int)(scaleX/gridSize);
			int countZ=(int)(scaleZ/gridSize);
			
			
			float x=-scaleX/2/scaleX;
			float z=-scaleZ/2/scaleZ;
			
			Vector3 point=platformT.TransformPoint(new Vector3(x, z, 0));
			
			refT.position=point;
			refT.rotation=platformT.rotation*Quaternion.Euler(-90, 0, 0);
			
			refT.position=refT.TransformPoint(new Vector3(gridSize/2, heightOffset, gridSize/2));
			
			NodeTD[] nodeGraph=new NodeTD[countZ*countX];
			
			LayerMask maskHeightOffset=1<<TDTK.GetLayerPlatformHeightOffset();
			Vector3 heightWindow=new Vector3(0, 500, 0);
			
			int counter=0;
			for(int i=0; i<countZ; i++){
				for(int j=0; j<countX; j++){
					RaycastHit hitInfo;
					bool flag=Physics.Linecast(refT.position+heightWindow, refT.position-heightWindow, out hitInfo, maskHeightOffset);
					nodeGraph[counter]=new NodeTD(flag ? hitInfo.point+new Vector3(0, 0.05f, 0) : refT.position, counter);
					
					//nodeGraph[counter]=new NodeTD(refT.position, counter);
					counter+=1;
					refT.position=refT.TransformPoint(new Vector3(gridSize, 0, 0));
				}
				refT.position=refT.TransformPoint(new Vector3(-(countX)*gridSize, 0, gridSize));
			}
			
			refT.position=Vector3.zero;
			refT.rotation=Quaternion.identity;
			
			//LayerMask mask=1<<TDTK.GetLayerPlatform() | 1<<TDTK.GetLayerTower() | 1<<TDTK.GetLayerTerrain() | 1<<TDTK.GetLayerNoBuild() | 1<<TDTK.GetLayerObstacle();
			LayerMask mask=1<<TDTK.GetLayerPlatform() | 1<<TDTK.GetLayerTower() | 1<<TDTK.GetLayerTerrain() | 1<<TDTK.GetLayerNoBuild() | 1<<TDTK.GetLayerPlatformHeightOffset() ;
			LayerMask maskTowerBlock=1<<TDTK.GetLayerNoBuild();
			
			counter=0;
			foreach(NodeTD cNode in nodeGraph){
				//check if there's anything within the point
				Collider[] cols=Physics.OverlapSphere(cNode.pos, gridSize*0.45f, ~mask);
				if(cols.Length>0){ cNode.SetWalkable(false); counter+=1; }
				
				cols=Physics.OverlapSphere(cNode.pos, gridSize*0.45f, maskTowerBlock);
				if(cols.Length>0){ cNode.SetBlockedForTower(true); }
			}
			
			
			float neighbourDistance=0;
			float neighbourRange=gridSize * (EnableDiagonal() ? 1.5f : 1.1f);
			//if(instance.connectDiagonalNeighbour) neighbourRange=gridSize*1.5f;
			//else neighbourRange=gridSize*1.1f;
			
			counter=0;
			//assign the neighouring  node for each node in the grid
			foreach(NodeTD currentNode in nodeGraph){
				//only if that node is walkable
				if(currentNode.IsWalkable()){
				
					//create an empty array
					List<NodeTD> neighbourNodeList=new List<NodeTD>();
					List<float> neighbourCostList=new List<float>();
					
					NodeTD[] neighbour=new NodeTD[8];
					int id=currentNode.ID;
					
					if(id>countX-1 && id<countX*countZ-countX){
						//print("middle rows");
						if(id!=countX) neighbour[0]=nodeGraph[id-countX-1];
						neighbour[1]=nodeGraph[id-countX];
						neighbour[2]=nodeGraph[id-countX+1];
						neighbour[3]=nodeGraph[id-1];
						neighbour[4]=nodeGraph[id+1];
						neighbour[5]=nodeGraph[id+countX-1];
						neighbour[6]=nodeGraph[id+countX];
						if(id!=countX*countZ-countX-1)neighbour[7]=nodeGraph[id+countX+1];
					}
					else if(id<=countX-1){
						//print("first row");
						if(id!=0) neighbour[0]=nodeGraph[id-1];
						if(nodeGraph.Length>id+1) neighbour[1]=nodeGraph[id+1];
						if(countZ>0){
							if(nodeGraph.Length>id+countX-1)	neighbour[2]=nodeGraph[id+countX-1];
							if(nodeGraph.Length>id+countX)		neighbour[3]=nodeGraph[id+countX];
							if(nodeGraph.Length>id+countX+1)	neighbour[4]=nodeGraph[id+countX+1];
						}
					}
					else if(id>=countX*countZ-countX){
						//print("last row");
						neighbour[0]=nodeGraph[id-1];
						if(id!=countX*countZ-1) neighbour[1]=nodeGraph[id+1];
						if(id!=countX*(countZ-1))neighbour[2]=nodeGraph[id-countX-1];
						neighbour[3]=nodeGraph[id-countX];
						neighbour[4]=nodeGraph[id-countX+1];
					}
					


					//scan through all the node in the grid
					foreach(NodeTD node in neighbour){
						//if this the node is not currentNode
						if(node!=null){// && node.IsWalkable()){
							//if this node is within neighbour node range
							neighbourDistance=GetHorizontalDistance(currentNode.pos, node.pos);
							if(neighbourDistance<neighbourRange){
								//if nothing's in the way between these two
								//~ if(!Physics.Linecast(currentNode.pos, node.pos, ~mask)){
									//if the slop is not too steep
									//if(Mathf.Abs(GetSlope(currentNode.pos, node.pos))<=maxSlope){
										//add to list
										//if(!node.walkable) Debug.Log("error");
										neighbourNodeList.Add(node);
										neighbourCostList.Add(neighbourDistance);
									//}//else print("too steep");
								//~ }//else print("something's in the way");
							}//else print("out of range "+neighbourDistance);
						}//else print("unwalkable");
					}

					//set the list as the node neighbours array
					currentNode.SetNeighbour(neighbourNodeList, neighbourCostList);
					
					//if(neighbourNodeList.Count==0)
						//Debug.Log("no heighbour. node number "+counter+"  "+neighbourNodeList.Count);
				}
				
				counter+=1;
			}
			
			return nodeGraph;
		}
		
		public static float GetHorizontalDistance(Vector3 p1, Vector3 p2){
			p1.y=0;	p2.y=0;
			return Vector3.Distance(p1, p2);
		}
		
		
		
		
		//searchMode:  0-any node, 1-walkable only, 2-unwalkable only
		//public static NodeTD GetNearestNode(Vector3 point, NodeTD[] graph){ return GetNearestNode(point, graph, 0); }
		public static NodeTD GetNearestNode(Vector3 point, NodeTD[] graph, int searchMode=0){
			float dist=Mathf.Infinity;
			float currentNearest=Mathf.Infinity;
			NodeTD nearestNode=null;
			
			if(searchMode==0){
				foreach(NodeTD node in graph){
					dist=Vector3.Distance(point, node.pos);
					if(dist<currentNearest){ currentNearest=dist; nearestNode=node; }
				}
			}
			else if(searchMode==1){
				foreach(NodeTD node in graph){
					if(!node.IsWalkable()) continue;
					dist=Vector3.Distance(point, node.pos);
					if(dist<currentNearest){ currentNearest=dist; nearestNode=node; }
				}
			}
			else if(searchMode==2){
				foreach(NodeTD node in graph){
					if(node.IsWalkable()) continue;
					dist=Vector3.Distance(point, node.pos);
					if(dist<currentNearest){ currentNearest=dist; nearestNode=node; }
				}
			}
			
			return nearestNode;
		}
		
		
		
		
		
		
		//make cause system to slow down, use with care
		public static List<Vector3> Search(NodeTD startN, NodeTD endN, NodeTD[] graph, NodeTD blockN=null, bool smoothing=true, int footprint=-1){
			Init();
			
			if(startN.IsBlocked()) return new List<Vector3>();
			
			if(blockN!=null) blockN.SetWalkable(false);
			
			bool pathFound=true;
			
			int searchCounter=0;	//used to count the total amount of node that has been searched
			
			List<NodeTD> closeList=new List<NodeTD>();
			NodeTD[] openList=new NodeTD[graph.Length];
			
			List<int> openListRemoved=new List<int>();
			int openListCounter=0;

			NodeTD currentNode=startN;
			
			float currentLowestF=Mathf.Infinity;
			float currentLowestG=Mathf.Infinity;
			int id=0;	//use element num of the node with lowest score in the openlist during the comparison process
			int i=0;		//universal int value used for various looping operation
			
			while(true){
				if(currentNode==endN) break;
				closeList.Add(currentNode);
				currentNode.listState=_ListStateTD.Close;
				
				currentNode.ProcessNeighbour(endN);
				foreach(NodeTD neighbour in currentNode.neighbourNode){
					if(neighbour.listState!=_ListStateTD.Unassigned) continue;
					if(neighbour.IsBlocked()) continue;
					
					if(IsDiagonallyBlocked(currentNode, neighbour)) continue;
					
					neighbour.listState=_ListStateTD.Open;
					if(openListRemoved.Count>0){
						openList[openListRemoved[0]]=neighbour;
						openListRemoved.RemoveAt(0);
					}
					else{
						openList[openListCounter]=neighbour;
						openListCounter+=1;
					}
				}
				
				currentNode=null;
				
				currentLowestF=Mathf.Infinity;
				currentLowestG=Mathf.Infinity;
				id=0;
				for(i=0; i<openListCounter; i++){
					if(openList[i]!=null){
						
						bool flag1=openList[i].scoreF<currentLowestF;
						bool flag2=openList[i].scoreF==currentLowestF && openList[i].scoreG<currentLowestG;
						
						if(flag1 || flag2){
							currentLowestF=openList[i].scoreF;
							currentLowestG=openList[i].scoreG;
							currentNode=openList[i];
							id=i;
						}
					}
				}
				
				//if(currentNode!=null){
				//	//Debug.Log(currentNode.GetPos()+"    "+endN.GetPos()+"    "+currentNode.scoreH+"    "+currentNode.scoreF);
				//	Debug.DrawLine(currentNode.GetPos(), currentNode.GetPos()+new Vector3(0, .75f, 0), Color.red, 0.25f);
				//}
				
				if(currentNode==null) {
					pathFound=false;
					break;
				}
				
				openList[id]=null;
				openListRemoved.Add(id);

				searchCounter+=1;
			}
			
			
			NodeTD cachedNode=currentNode;
			List<Vector3> p=new List<Vector3>();
			while(currentNode!=null){
				p.Add(currentNode.pos);
				currentNode=currentNode.parent;
			}
			
			if(pathFound){
				if(smoothing){
					if(EnableFreeSmoothing()){
						p=PathSmoothingFree(p);
					}
					else if(EnableDiagonalSmoothing() && !EnableDiagonal()){
						List<NodeTD> pn=new List<NodeTD>();	currentNode=cachedNode;
						while(currentNode!=null){
							pn.Add(currentNode);
							currentNode=currentNode.parent;
						}
						p=PathSmoothing(pn);
					}
				}
				
				p.Reverse();
			}
			
			
			if(blockN!=null) blockN.SetWalkable(true); 
			
			ResetGraph(graph);
			
			return p;
		}
		
		
		
		
		
		public static bool IsDiagonallyBlocked(NodeTD srcN, NodeTD tgtN){
			if(!EnableDiagonal()) return false;
			
			//neighbour is not diagonal, so definitely not blocked
			if(Vector3.Distance(srcN.GetPos(), tgtN.GetPos())<TowerManager.GetGridSize()*1.1f) return false;		
			
			foreach(NodeTD subN in srcN.neighbourNode){
				if(subN==tgtN) continue;
				if(Vector3.Distance(subN.GetPos(), tgtN.GetPos())>TowerManager.GetGridSize()*1.1) continue;
				
				if(subN.IsBlocked(true)) return true;
			}
			
			/*
			Vector3 dir=(tgtN.GetPos()-srcN.GetPos()).normalized;
			if(dir.x*dir.z!=0){
				foreach(NodeTD subN in srcN.neighbourNode){
					if(subN==tgtN) continue;
					if(Vector3.Distance(subN.GetPos(), tgtN.GetPos())<TowerManager.GetGridSize()*1.1){
						if(subN.IsBlocked()) return true;
					}
				}
			}
			*/
			
			return false;
		}
		
		
		
		public static List<Vector3> PathSmoothingFree(List<Vector3> srcPath, bool reverse=false){
			//for(int n=0; n<srcPath.Count-1; n++) Debug.DrawLine(srcPath[n], srcPath[n+1], Color.white, 2);
			
			for(int i=0; i<srcPath.Count-2; i++){
				int skip=0;
				
				Vector3 srcNode=srcPath[i];
				for(int n=i+2; n<srcPath.Count; n++){
					LayerMask mask=1<<TDTK.GetLayerTower() | 1<<TDTK.GetLayerObstacle();
					
					Vector3 dir=(srcPath[n]-srcNode).normalized;
					Vector3 dirO=Vector3.Cross(Vector3.up, dir) * TowerManager.GetGridSize() * 0.45f; 
					
					bool flag1=Physics.Linecast(srcNode, srcPath[n], mask);
					bool flag2=Physics.Linecast(srcNode+dirO, srcPath[n]+dirO, mask);
					bool flag3=Physics.Linecast(srcNode-dirO, srcPath[n]-dirO, mask);
					
					//Debug.DrawLine(srcNode, srcPath[n], flag1 ? Color.red : Color.green, 2);
					//Debug.DrawLine(srcNode+dirO, srcPath[n]+dirO, flag2 ? Color.red : Color.green, 2);
					//Debug.DrawLine(srcNode-dirO, srcPath[n]-dirO, flag3 ? Color.red : Color.green, 2);
					
					if(!flag1 && !flag2 && !flag3) skip+=1;
					else break;
				}
				
				for(int n=0; n<skip; n++) srcPath.RemoveAt(i+1);
			}
			
			List<Vector3> path=new List<Vector3>();
			for(int i=0; i<srcPath.Count; i++) path.Add(srcPath[i]);
			
			path=ResamplePath(path);
			
			//for(int i=0; i<path.Count-1; i++) Debug.DrawLine(path[i], path[i]+new Vector3(0, 1, 0), Color.white, 2);
			
			if(!reverse){
				path.Reverse();
				path=PathSmoothingFree(path, true);
				path.Reverse();
			}
			
			return path;
		}
		
		public static List<Vector3> PathSmoothing_Buggy(List<NodeTD> srcPath){
			int spacing=0;	int lastValidSpace=0;	//int iteration=0;
			for(int i=0; i<srcPath.Count-2; i++){
				NodeTD srcNode=srcPath[i];
				
				bool blocked=false;
				while(spacing<srcPath.Count-i-1 && !blocked){
					spacing+=1;
					NodeTD nextNode=srcPath[i+spacing];
					
					Vector3 dir=(nextNode.GetPos()-srcNode.GetPos()).normalized;
					if(dir.x*dir.z==0 || Mathf.Abs(dir.x)!=Mathf.Abs(dir.z)) continue;
					
					List<NodeTD> keyNodes=new List<NodeTD>{ srcNode };
					List<NodeTD> list=GetNodeWithinDistance(srcNode, spacing-2);
					float curDist=Vector3.Distance(nextNode.GetPos(), srcNode.GetPos());
					for(int n=0; n<list.Count; n++){
						if(Vector3.Distance(nextNode.GetPos(), list[n].GetPos())>=curDist) continue;
						if(dir==(nextNode.GetPos()-list[n].GetPos()).normalized) keyNodes.Add(list[n]);
					}
					keyNodes.Add(nextNode);
					
					for(int j=0; j<keyNodes.Count-1; j++){
						if(!keyNodes[j].IsWalkable() || keyNodes[j].IsOccupied()){ blocked=true; break; }
						
						NodeTD[] neighbourList1=keyNodes[j].neighbourNode;
						NodeTD[] neighbourList2=keyNodes[j+1].neighbourNode;
						List<NodeTD> commonNeighbourList=new List<NodeTD>();
						
						for(int n=0; n<neighbourList1.Length; n++){
							for(int m=0; m<neighbourList2.Length; m++){
								if(neighbourList1[n]==neighbourList2[m]){
									if(!commonNeighbourList.Contains(neighbourList2[m])) commonNeighbourList.Add(neighbourList2[m]);
								}
							}
						}
						
						if(commonNeighbourList.Count>1){
							for(int n=0; n<commonNeighbourList.Count; n++){
								if(!commonNeighbourList[n].IsWalkable() || commonNeighbourList[n].IsOccupied()){
									blocked=true;		break;
								}
							}
						}
						else blocked=true;
						
						if(blocked) break;
					}
					
					if(!blocked) lastValidSpace=spacing;
				}
				
				if(lastValidSpace>0){
					for(int n=0; n<lastValidSpace-1; n++) srcPath.RemoveAt(i+1);
					i+=lastValidSpace-1;
				}
				spacing=0;
				lastValidSpace=0;
				
				//iteration+=1;
			}
			
			List<Vector3> path=new List<Vector3>();
			for(int i=0; i<srcPath.Count; i++) path.Add(srcPath[i].GetPos());
			
			return ResamplePath(path);
		}
		
		
		/*
		if straight, increase spacing and continue
		if not diagonal, increase spacing and continue
		*/
		
		public static List<Vector3> PathSmoothing(List<NodeTD> srcPath){
			int spacing=2;
			
			for(int i=0; i<srcPath.Count-2; i++){
				Vector3 dir=srcPath[i+spacing].GetPos()-srcPath[i].GetPos();
				
				if(dir.x*dir.z==0 || Mathf.Abs(dir.x)!=Mathf.Abs(dir.z)){
					if(i+spacing<srcPath.Count-1){
						i-=1;
						spacing+=1;
					}
					else{
						spacing=2;
					}
					continue;
				}
				
				bool blocked=false;
				
				if(spacing==2){
					NodeTD[] neighbourList1=srcPath[i].neighbourNode;
					NodeTD[] neighbourList2=srcPath[i+spacing].neighbourNode;
					List<NodeTD> commonNeighbourList=new List<NodeTD>();
					
					for(int n=0; n<neighbourList1.Length; n++){
						for(int m=0; m<neighbourList2.Length; m++){
							if(neighbourList1[n]==neighbourList2[m]){
								if(!commonNeighbourList.Contains(neighbourList2[m])) commonNeighbourList.Add(neighbourList2[m]);
							}
						}
					}
					
					if(commonNeighbourList.Count>1){
						for(int n=0; n<commonNeighbourList.Count; n++){
							if(!commonNeighbourList[n].IsWalkable() || commonNeighbourList[n].IsOccupied()){
								blocked=true; 	break;
							}
						}
					}
					else blocked=true;
					
					if(blocked){ spacing=2; continue; }
					
					for(int n=0; n<spacing-1; n++) srcPath.RemoveAt(i+1);
				}
				else{
					List<NodeTD> cachedNList=new List<NodeTD>();
					
					int loopID=Random.Range(0, 9999);
					
					NodeTD curNode=srcPath[i];
					while(true){
						NodeTD neighbourD = curNode.GetDiagonalNode(dir);
						
						if(blocked || neighbourD==srcPath[i+spacing]) break;
						
						if(neighbourD!=null && neighbourD.neighbourNode!=null){
							if(neighbourD.IsBlocked()) blocked=true;
							else{
								for(int n=0; n<neighbourD.neighbourNode.Length; n++){
									if(neighbourD.neighbourNode[n].IsBlocked()){
										blocked=true;
										break;
									}
								}
								
								cachedNList.Add(neighbourD);
								
								curNode=neighbourD;
							}
						}
						else blocked=true;
					}
					
					if(blocked){ spacing=2; continue; }
					
					for(int n=0; n<spacing-1; n++) srcPath.RemoveAt(i+1);
					
					for(int n=0; n<cachedNList.Count; n++) srcPath.Insert(i+n+1, cachedNList[n]);
					i+=cachedNList.Count;
				}
				
				spacing=2;
			}
			
			List<Vector3> path=new List<Vector3>();
			for(int i=0; i<srcPath.Count; i++) path.Add(srcPath[i].GetPos());
			
			return path;//ResamplePath(path);
		}
		
		public static List<Vector3> ResamplePath(List<Vector3> path){
			float gridSize=TowerManager.GetGridSize()*1.1f;
			for(int i=0; i<path.Count-1; i++){
				if(Vector3.Distance(path[i], path[i+1])<gridSize) continue;
				Vector3 dir=(path[i+1]-path[i]).normalized;
				path.Insert(i+1, path[i]+(dir * gridSize));
			}
			return path;
		}
		
		public static  List<NodeTD> GetNodeWithinDistance(NodeTD srcNode, int dist){
			if(dist<=0) new List<NodeTD>();
			
			NodeTD[] neighbourList=srcNode.neighbourNode;
			
			List<NodeTD> closeList=new List<NodeTD>();
			List<NodeTD> openList=new List<NodeTD>();
			List<NodeTD> newOpenList=new List<NodeTD>();
			
			for(int m=0; m<neighbourList.Length; m++){
				NodeTD neighbour=neighbourList[m];
				if(!newOpenList.Contains(neighbour)) newOpenList.Add(neighbour);
			}
			
			for(int i=0; i<dist; i++){
				openList=newOpenList;
				newOpenList=new List<NodeTD>();
				
				for(int n=0; n<openList.Count; n++){
					neighbourList=openList[n].neighbourNode;
					for(int m=0; m<neighbourList.Length; m++){
						NodeTD neighbour=neighbourList[m];
						if(!closeList.Contains(neighbour) && !openList.Contains(neighbour) && !newOpenList.Contains(neighbour)){
							newOpenList.Add(neighbour);
						}
					}
				}
				
				for(int n=0; n<openList.Count; n++){
					NodeTD node=openList[n];
					if(node!=srcNode && !closeList.Contains(node)) closeList.Add(node);
				}
			}
			
			return closeList;
		}
		
		
		public static IEnumerator _PathSmoothing_Debug(List<NodeTD> srcPath){
			for(int n=0; n<srcPath.Count-1; n++){ 
				Debug.DrawLine(srcPath[n].GetPos(), srcPath[n+1].GetPos(), Color.red, 10);
				Debug.DrawLine(srcPath[n].GetPos(), srcPath[n].GetPos()+new Vector3(0, .5f, 0), Color.white, 10);
			}
			
			int spacing=0;	int lastValidSpace=0;	int iteration=0;
			for(int i=0; i<srcPath.Count-2; i++){
				NodeTD srcNode=srcPath[i];
				
				
				bool blocked=false;
				while(spacing<srcPath.Count-i-1 && !blocked){
					spacing+=1;
					NodeTD nextNode=srcPath[i+spacing];
					
					Debug.DrawLine(srcNode.GetPos(), nextNode.GetPos(), Color.red, 1);
					yield return new WaitForSeconds(1);
					
					Vector3 dir=(nextNode.GetPos()-srcNode.GetPos()).normalized;
					if(dir.x*dir.z==0 || Mathf.Abs(dir.x)!=Mathf.Abs(dir.z)) continue;
					
					List<NodeTD> keyNodes=new List<NodeTD>{ srcNode };
					List<NodeTD> list=GetNodeWithinDistance(srcNode, spacing-2);
					float curDist=Vector3.Distance(nextNode.GetPos(), srcNode.GetPos());
					for(int n=0; n<list.Count; n++){
						if(Vector3.Distance(nextNode.GetPos(), list[n].GetPos())>=curDist) continue;
						if(dir==(nextNode.GetPos()-list[n].GetPos()).normalized) keyNodes.Add(list[n]);
					}
					keyNodes.Add(nextNode);
					
					Debug.Log(iteration+"   "+spacing+"    "+srcNode.GetPos()+"   "+nextNode.GetPos());
					for(int n=0; n<keyNodes.Count; n++) Debug.DrawLine(keyNodes[n].GetPos(), keyNodes[n].GetPos()+new Vector3(0, 1, 0), Color.red, 1);
					yield return new WaitForSeconds(1);
					
					Debug.Log(iteration+"   "+dir+"   keynode count: "+keyNodes.Count+"   "+spacing);
					
					for(int j=0; j<keyNodes.Count-1; j++){
						Debug.DrawLine(keyNodes[j+1].GetPos(), keyNodes[j+1].GetPos()+new Vector3(0, 1, 0), Color.blue, 2);
						Debug.DrawLine(srcNode.GetPos(), srcNode.GetPos()+new Vector3(0, 1, 0), Color.green, 2);
						Debug.DrawLine(srcNode.GetPos(), keyNodes[j+1].GetPos(), Color.white, 2);
						
						NodeTD[] neighbourList1=keyNodes[j].neighbourNode;
						NodeTD[] neighbourList2=keyNodes[j+1].neighbourNode;
						List<NodeTD> commonNeighbourList=new List<NodeTD>();
						
						for(int n=0; n<neighbourList1.Length; n++){
							for(int m=0; m<neighbourList2.Length; m++){
								if(neighbourList1[n]==neighbourList2[m]){
									if(!commonNeighbourList.Contains(neighbourList2[m])) commonNeighbourList.Add(neighbourList2[m]);
								}
							}
						}
						
						if(commonNeighbourList.Count>1){
							for(int n=0; n<commonNeighbourList.Count; n++){
								Debug.DrawLine(commonNeighbourList[n].GetPos(), commonNeighbourList[n].GetPos()+new Vector3(0, 1, 0), Color.red, 2);
								if(!commonNeighbourList[n].IsWalkable() || commonNeighbourList[n].IsOccupied()){
								
									NodeTD nn=commonNeighbourList[n];
									Debug.DrawLine(nn.GetPos()+new Vector3(.25f, 0, 0), nn.GetPos()-new Vector3(.25f, 0, 0), Color.white, 2);
									Debug.DrawLine(nn.GetPos()+new Vector3(0, 0, .25f), nn.GetPos()-new Vector3(0, .25f), Color.white, 2);
						
									blocked=true;		break;
								}
							}
						}
						else blocked=true;
						
						Debug.DrawLine(keyNodes[j+1].GetPos()+new Vector3(.25f, 0, 0), keyNodes[j+1].GetPos()-new Vector3(.25f, 0, 0), Color.white, 2);
						Debug.DrawLine(keyNodes[j+1].GetPos()+new Vector3(0, 0, .25f), keyNodes[j+1].GetPos()-new Vector3(0, .25f), Color.white, 2);
						
						if(blocked){
							Debug.Log(iteration+"   "+blocked);
							break;
						}
					}
					
					if(!blocked){
						lastValidSpace=spacing;
						Debug.Log(iteration+"   "+blocked+"  lastValidSpace "+lastValidSpace);
						yield return new WaitForSeconds(2);
					}
				}
				
				Debug.Log(iteration+" _____________  Clear block, lastValidSpace-"+lastValidSpace);
				
				if(lastValidSpace>0){
					for(int n=0; n<lastValidSpace-1; n++) srcPath.RemoveAt(i+1);
					i+=lastValidSpace-1;
				}
				spacing=0;
				lastValidSpace=0;
				
				yield return new WaitForSeconds(2);
				
				iteration+=1;
			}
			
			for(int i=0; i<srcPath.Count-1; i++){
				Debug.DrawLine(srcPath[i].GetPos(), srcPath[i+1].GetPos(), Color.white, 3);
			}
			
			yield return null;
			//return srcPath;
		}
		
		
		
		
		public static List<NodeTD> PathSmoothing_Obsolete(List<NodeTD> srcPath){
			int spacing=2;
			for(int i=0; i<srcPath.Count-2; i++){
					
					Vector3 dir=srcPath[i].GetPos()-srcPath[i+spacing].GetPos();
					if(dir.x*dir.z==0) continue;
					
					NodeTD[] neighbourList1=srcPath[i].neighbourNode;
					NodeTD[] neighbourList2=srcPath[i+spacing].neighbourNode;
					List<NodeTD> commonNeighbourList=new List<NodeTD>();
					
					for(int n=0; n<neighbourList1.Length; n++){
						for(int m=0; m<neighbourList2.Length; m++){
							if(neighbourList1[n]==neighbourList2[m]){
								if(!commonNeighbourList.Contains(neighbourList2[m])) commonNeighbourList.Add(neighbourList2[m]);
							}
						}
					}
					
					bool blocked=false;
					if(commonNeighbourList.Count>1){
						for(int n=0; n<commonNeighbourList.Count; n++){
							if(!commonNeighbourList[n].IsWalkable() || commonNeighbourList[n].IsOccupied()){
								blocked=true; 	break;
							}
						}
					}
					else blocked=true;
					
					if(blocked) continue;
					
					srcPath.RemoveAt(i+spacing-1);
					
				
			}
			return srcPath;
		}
		
		
		
		
		public static void ResetGraph(NodeTD[] nodeGraph){
			foreach(NodeTD node in nodeGraph){
				node.listState=_ListStateTD.Unassigned;
				node.parent=null;
				node.scoreH=0;
				node.scoreF=0;
				node.scoreG=0;
			}
		}
		
		
	}
	
	
	
	public enum _ListStateTD{Unassigned, Open, Close};
	public class NodeTD{
		public int ID;
		public Vector3 pos;
		public NodeTD[] neighbourNode;
		public NodeTD[] NeighbourNodeD;	//diagonal
		public float[] neighbourCost;
		public NodeTD parent;
		private bool walkable=true;
		public float scoreG;
		public float scoreH;
		public float scoreF;
		public _ListStateTD listState=_ListStateTD.Unassigned;
		public float tempScoreG=0;
		
		private bool blockedForTower=false;
		private UnitTower occupiedTower;
		
		public NodeTD(){}
		
		public NodeTD(Vector3 position, int id){
			pos=position;
			ID=id;
		}
		
		public Vector3 GetPos(){ return pos; }
		
		public UnitTower GetTower(){ return occupiedTower; }
		public void ClearTower(){ occupiedTower=null; }
		public void SetTower(UnitTower t){
			if(occupiedTower!=null) Debug.LogWarning("Node has been occupied!"); 
			occupiedTower=t;
		}
		
		
		
		public void SetWalkable(bool flag){ walkable=flag; }
		public bool IsWalkable(){ return walkable; }
		
		public void SetBlockedForTower(bool flag){ blockedForTower=flag; }
		public bool IsBlockedForTower(){ return !walkable || blockedForTower; }
		
		//public bool IsBlocked1(){ return walkable & (occupiedTower==null || occupiedTower.IsMine()); }
		public bool IsBlocked(bool debug=false){ return !walkable || (occupiedTower!=null && !occupiedTower.IsMine()); }
		public bool IsOccupied(){ return occupiedTower!=null; }
		
		public void SetNeighbour(List<NodeTD> arrNeighbour, List<float> arrCost){
			neighbourNode = arrNeighbour.ToArray();
			neighbourCost = arrCost.ToArray();
		}
		
		public void ProcessNeighbour(NodeTD node){
			ProcessNeighbour(node.pos);
		}
		
		//call during a serach to scan through all neighbour, check their score against the position passed
		public void ProcessNeighbour(Vector3 pos){
			bool adjBlocked=false;		float adjModifier=0;		
			for(int i=0; i<neighbourNode.Length; i++){
				if(neighbourNode[i].IsBlocked()) continue;
				
				adjBlocked=AStar.IsDiagonallyBlocked(this, neighbourNode[i]);
				adjModifier=adjBlocked ? TowerManager.GetGridSize()*2 : 0;
				
				float dirMultiplier=1;
				if(parent!=null && (GetPos()-parent.GetPos()).normalized==(neighbourNode[i].GetPos()-GetPos()).normalized) dirMultiplier=.95f;
				
				//Debug.Log("neighbourCost-"+neighbourCost[i]+"   "+adjModifier);
				tempScoreG = scoreG + ( neighbourCost[i]+adjModifier ) * dirMultiplier;
				
				//if the neightbour state is clean (never evaluated so far in the search)
				if(neighbourNode[i].listState==_ListStateTD.Unassigned){
					//check the score of G and H and update F, also assign the parent to currentNode
					neighbourNode[i].scoreH=Vector3.Distance(neighbourNode[i].pos, pos)+adjModifier;
					neighbourNode[i].UpdateScoreF(this, tempScoreG);
				}
				//if the neighbour state is open (it has been evaluated and added to the open list)
				else if(neighbourNode[i].listState==_ListStateTD.Open){
					//calculate if the path if using this neighbour node through current node would be shorter compare to previous assigned parent node
					if(neighbourNode[i].scoreG>tempScoreG){
						//if so, update the corresponding score and and reassigned parent
						neighbourNode[i].UpdateScoreF(this, tempScoreG);
					}
				}
			}
		}
		
		private void UpdateScoreF(NodeTD newParent, float newScoreG){
			parent=newParent;
			scoreG=newScoreG;
			scoreF=scoreG+scoreH;
		}
		
		//~ public NodeTD GetNeighbourFromDir(Vector3 dir){
			//~ for(int i=0; i<NeighbourNodeD.Length; i++){
				//~ Debug.Log("GetNeighbourFromDir  "+i+"   "+(NeighbourNodeD[i].GetPos()-GetPos())+" = "+dir);
				//~ if((NeighbourNodeD[i].GetPos()-GetPos()).normalized==dir.normalized){
					//~ return NeighbourNodeD[i];
				//~ }
			//~ }
			//~ return null;
		//~ }
		
		public NodeTD GetDiagonalNode(Vector3 dir){
			for(int i=0; i<neighbourNode.Length; i++){
				Vector3 nDIr=neighbourNode[i].GetPos()-GetPos();
				
				bool match1=false;
				if(dir.x>0 && nDIr.x>0)		match1=true;
				else if(dir.x<0 && nDIr.x<0)	match1=true;
				else if(dir.z>0 && nDIr.z>0)	match1=true;
				else if(dir.z<0 && nDIr.z<0)	match1=true;
				
				if(match1 && neighbourNode[i].neighbourNode!=null){
					for(int n=0; n<neighbourNode[i].neighbourNode.Length; n++){
						nDIr=(neighbourNode[i].neighbourNode[n].GetPos()-GetPos()).normalized;
						
						if(nDIr==dir.normalized){
							return neighbourNode[i].neighbourNode[n];
						}
					}
				}
			}
			return null;
		}
	}

	

}