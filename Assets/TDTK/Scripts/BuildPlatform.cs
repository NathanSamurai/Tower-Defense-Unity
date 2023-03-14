using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TDTK;

namespace TDTK{

	public class BuildPlatform : MonoBehaviour {
		
		public List<int> unavailablePrefabIDList=new List<int>();	//prefabID of the tower can't be built on this 
		
		[HideInInspector] public Vector2 size;
		
		[Header("Run time variable")] 
		public bool walkable=false;
		public List<Path> pathList=new List<Path>(); 	//all path that use the platform as waypoint
		public void AddPath(Path path){ pathList.Add(path); }
		
		[HideInInspector] public Transform thisT;
		
		void Awake(){
			gameObject.layer=TDTK.GetLayerPlatform();	//this should have been preassigned
			thisT=transform;
		}
		
		void Start(){
			TowerManager.FormatPlatform(this);
		}
		
		//[ContextMenu("Format")] IEnumerator Start(){
		//	yield return null;
		//	formatted=false; Format(TowerManager.GetGridSize(), true);
		//}
		
		public Vector3 GetPos(){ return thisT!=null ? thisT.position : transform.position; }
		public Quaternion GetRot(){ return thisT!=null ? thisT.rotation : transform.rotation; }
		
		private bool formatted=false;
		public void Format(float gridSize=1, bool autoAdjustTextureToGrid=true){
			if(formatted) return;
			
			formatted=true;
			if(thisT==null) thisT=transform;
			
			//make sure the plane is perfectly horizontal, rotation around the y-axis is presreved
			thisT.eulerAngles=new Vector3(90, thisT.rotation.eulerAngles.y, 0);
			
			//adjusting the scale
			float scaleX=Mathf.Max(1, Mathf.Round(TDTK.GetWorldScale(thisT).x/gridSize))*gridSize;
			float scaleY=Mathf.Max(1, Mathf.Round(TDTK.GetWorldScale(thisT).y/gridSize))*gridSize;
			
			thisT.localScale=new Vector3(scaleX, scaleY, 1);
			
			//Vector2 
			size=new Vector2((int)(scaleX/gridSize), (int)(scaleY/gridSize));
			
			//adjusting the texture
			if(autoAdjustTextureToGrid){
				Material mat=thisT.GetComponent<Renderer>().material;
				
				float x=(TDTK.GetWorldScale(thisT).x)/gridSize;
				float y=(TDTK.GetWorldScale(thisT).y)/gridSize;
				
				mat.mainTextureOffset=new Vector2(0.5f, 0.5f);
				mat.mainTextureScale=new Vector2(x, y);
			}
			
			GenerateGraph(gridSize);
			
			//for(int i=0; i<nodeGraph.Length; i++){
			//	if(nodeGraph[i].IsBlockedForTower()) continue;
			//	Instantiate(tilePrefab, nodeGraph[i].GetPos(), thisT.rotation);
			//}
		}
		public void SingleNodePlatform(){
			nodeGraph=new NodeTD[1];
			nodeGraph[0]=new NodeTD(GetPos(), 0);
		}
		
		//[Space(8)]	public GameObject tilePrefab;
		
		
		public Vector3 GetTilePos(Vector3 hitPos, float gridSize=1){ return GetTilePos(this, hitPos, gridSize); }
		public static Vector3 GetTilePos(BuildPlatform platform, Vector3 hitPos, float gridSize=1){
			Vector3 v=hitPos-platform.thisT.position;	//get the vector from platform origin to hit point
			
			//transform the vector to the platform local space, so we know the (x, y)
			v=Quaternion.Euler(0, -platform.thisT.rotation.eulerAngles.y, 0) * v;	
			
			//check the size of the platform for odd/even columen and then set the offset in corresponding axis
			float osX=platform.size.x%2==0 ? gridSize/2 : 0;
			float osZ=platform.size.y%2==0 ? gridSize/2 : 0;
			
			//calculate the x and z position (this is the relative position in platform local space to the platform origin)
			float x=Mathf.Round((osX+v.x)/gridSize)*gridSize-osX;
			float z=Mathf.Round((osZ+v.z)/gridSize)*gridSize-osZ;
			
			//transform the calculated position to world space
			return platform.thisT.position+platform.thisT.TransformDirection(new Vector3(x, z, 0));
		}
		
		
		
		public void BuildTower(int nodeIdx, UnitTower tower=null, bool updatePath=true){
			nodeGraph[nodeIdx].SetTower(tower);
			if(updatePath) UpdatePath(tower);
		}
		public void RemoveTower(int nodeIdx, bool updatePath=true){
			nodeGraph[nodeIdx].ClearTower();
			if(updatePath) UpdatePath();
		}
		
		//test function, no longer in use
		/*
		public void BlockNode(int nodeIdx){
			nodeGraph[nodeIdx].SetWalkable(false);
			UpdatePath();
		}
		public void UnblockNode(int nodeIdx){
			nodeGraph[nodeIdx].SetWalkable(true);
			UpdatePath();
		}
		*/
		
		
		public bool CheckForNode(int nodeIdx){
			if(!walkable) return true;
			
			List<Path> blockedPathList=new List<Path>();
			
			for(int i=0; i<pathList.Count; i++){
				if(!pathList[i].CheckForNode(this, nodeIdx)) blockedPathList.Add(pathList[i]);
			}
			
			for(int i=0; i<blockedPathList.Count; i++){
				if(!blockedPathList[i].CheckForNodeAltPath(this, nodeIdx, blockedPathList)) return false;
			}
			
			return true;
		}
		
		
		public void UpdatePath(UnitTower tower=null){
			if(walkable) StartCoroutine(_UpdatePath(tower));
		}
		IEnumerator _UpdatePath(UnitTower tower=null){
			yield return null;
			for(int i=0; i<pathList.Count; i++) pathList[i].UpdatePlatformPath(this, tower);
			Path.UpdateDistToEndOnAllPath();
		}
		
		
		
		//the graph-node covering this platform
		private NodeTD[] nodeGraph;
		public NodeTD[] GetNodeGraph(){ return nodeGraph; }
		public void OverrideNodeGraph(NodeTD[] newGraph){ nodeGraph=newGraph; }	//for extension
		
		public NodeTD GetNode(int idx){ return nodeGraph[idx]; }
		
		public void GenerateGraph(float gridSize){ nodeGraph=AStar.GenerateNode(this, gridSize); }
		
		//searchMode:  0-any node, 1-walkable only, 2-unwalkable only
		public NodeTD GetNearestNode(Vector3 point, int searchMode=0){ 
			NodeTD node=AStar.GetNearestNode(point, nodeGraph, searchMode);
			if(node==null){
				Debug.Log(nodeGraph.Length+"    node is null");
				return nodeGraph[0];
			}
			return node;
		}
		
		
		public List<Vector3> searchPath(int entryID, int exitID, int blockIdx=-1, bool smooth=true){
			List<Vector3> list=AStar.Search(nodeGraph[entryID], nodeGraph[exitID], nodeGraph, blockIdx>=0 ? nodeGraph[blockIdx] : null, smooth);
			AStar.ResetGraph(nodeGraph);
			return list;
		}
		
		
		
		[Space(10)] public bool GizmoShowNodes=true;
		void OnDrawGizmos(){
			if(GizmoShowNodes){
				if(nodeGraph!=null && nodeGraph.Length>0){
					foreach(NodeTD node in nodeGraph){
						if(node.IsBlocked()){
							Gizmos.color=Color.red;
							Gizmos.DrawSphere(node.pos, .15f);
						}
						else if(node.IsBlockedForTower()){
							Gizmos.color=new Color(.5f, .5f, .5f, 1f);
							Gizmos.DrawSphere(node.pos, .15f);
						}
						else{
							Gizmos.color=Color.white;
							Gizmos.DrawSphere(node.pos, .15f);
						}
					}
				}
			}
		}
		
	}

}