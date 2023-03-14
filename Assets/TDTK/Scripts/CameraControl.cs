using System.Collections;
using System.Collections.Generic;

using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TDTK {

	public class CameraControl : MonoBehaviour {
		
		public static Camera mainCam;
		public static Camera GetMainCam(){
			//return CamSelect.GetMainCam();
			return InitMainCamera();
		}
		
		public static Camera InitMainCamera(){
			if(mainCam!=null) return mainCam;
			mainCam=Camera.main;
			if(mainCam==null) Debug.LogWarning("Main camera not found");
			return mainCam;
		}
		
		//inputID=-1 - mouse cursor, 	inputID>=0 - touch finger index
		public static bool IsCursorOnUI(int inputID=-1){
			EventSystem eventSystem = EventSystem.current;
			return ( eventSystem!=null && eventSystem.IsPointerOverGameObject( inputID ) );
		}
		
		
		public bool useTouchInput=false;
		
		public bool enableZoom=true;
		public bool enableRotate=true;
		public bool scrollKey=true;				//move pivot using keyboard
		public bool scrollCursorDrag=true;		//move pivot using on screen cursor drag (works for touch input)
		public bool scrollCursorOnEdge=true;	//move pivot by placing cursor on edge of the screen
		
		public float scrollCursorOnEdgeTH=10;
		
		[Space(5)] public bool avoidClipping=false;
		private bool hasObstacle=false;
		
		
		[Header("Sensitivity")]
		public float scrollSpeed=5;
		public float zoomSpeed=5;
		public float rotateSpeed=1;
		
		
		[Header("Limit")]
		public bool enablePositionLimit=true;
		public float minPosX=-10;		//limit when zoom all the way in (if linkPanLimitToZoom are enabled)
		public float maxPosX=10;
		
		public float minPosZ=-10;
		public float maxPosZ=10;
		
		public float minZoomDistance=8;
		public float maxZoomDistance=30;
		
		public float minRotateAngle=10;
		public float maxRotateAngle=89;
		
		
		
		
		
		private float currentZoom=0;
		private Transform camT;
		
		private Transform thisT;
		public static CameraControl instance;
		
		void Awake(){
			thisT=transform;
			instance=this;
		}
		
		void Start(){
			camT=thisT.GetChild(0);//Camera.main.transform;
			currentZoom=camT.localPosition.z;
		}
		
		private float initialMousePosX;	private float initialRotX;
		private float initialMousePosY;	private float initialRotY;
		
		private Vector3 lastMousePos;
		private Vector3 mouseDeltaPos;
		
		private Vector2 initialCursorP;
		private Vector2 initialRot;
		
		private bool dragRotating=false;
		private bool dragScrolling=false;
		
		void Update(){
			//RenderSettings.skybox.SetFloat("_Rotation", Time.time * 2);
			
			if(useTouchInput) TouchInput();
			else MouseInput();
		}
		
		
		void TouchInput(){
			
			if(scrollCursorDrag && !tempDisableScrollCursorDrag && scrollSpeed!=0){
				float moveH=0;	float moveV=0;
				
				if(Input.GetMouseButtonDown(0) && Input.touchCount<2){
					if(!dragScrolling && !IsCursorOnUI()){
						lastMousePos=Input.mousePosition;
						dragScrolling=true;
					}
				}
				else if(Input.GetMouseButton(0) && Input.touchCount<2){
					if(dragScrolling){
						mouseDeltaPos=Input.mousePosition-lastMousePos;
						lastMousePos=Input.mousePosition;
						moveH=-mouseDeltaPos.x*0.1f;
						moveV=-mouseDeltaPos.y*0.1f;
					}
				}
				else dragScrolling=false;
				
				/*	//camera would jump from one finger to another when having 2 finger on screen (left-both-right-both-left...)
				if(Input.GetMouseButtonDown(0) && !IsCursorOnUI()){
					lastMousePos=Input.mousePosition;
					dragScrolling=true;
				}
				else if(Input.GetMouseButton(0) && dragScrolling){
					mouseDeltaPos=Input.mousePosition-lastMousePos;
					lastMousePos=Input.mousePosition;
					moveH=-mouseDeltaPos.x*0.1f;
					moveV=-mouseDeltaPos.y*0.1f;
				}
				else dragScrolling=false;
				*/
				
				if(moveH!=0){
					Vector3 dirH=transform.InverseTransformDirection(Quaternion.Euler(0, thisT.eulerAngles.y, 0)*Vector3.right);
					thisT.Translate(dirH * scrollSpeed * GetdeltaT() * moveH);
				}
				if(moveV!=0){
					Vector3 dirV=transform.InverseTransformDirection(Quaternion.Euler(0, thisT.eulerAngles.y, 0)*Vector3.forward);
					thisT.Translate(dirV * scrollSpeed * GetdeltaT() * moveV);
				}
				
				ApplyHorizontalLimit(moveH, moveV);
			}
			
			
			if(enableZoom){
				if(Input.touchCount==2){
					Touch touch1 = Input.GetTouch(0);
					Touch touch2 = Input.GetTouch(1);

					// Find the position in the previous frame of each touch.
					Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
					Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

					if(Vector2.Angle(touch1PrevPos, touch2PrevPos)<15){			
						// Find the magnitude of the vector (the distance) between the touches in each frame.
						float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
						float touchDeltaMag = (touch1.position - touch2.position).magnitude;

						// Find the difference in the distances between each frame.
						float zoomInput = prevTouchDeltaMag - touchDeltaMag;
						currentZoom=Mathf.Clamp(currentZoom+zoomSpeed*zoomInput*.1f, -maxZoomDistance, -minZoomDistance);
					}
				}
				Zoom();
			}
			
			if(enableRotate){
				if(Input.touchCount==2){
					Touch touch1 = Input.touches[0];
					Touch touch2 = Input.touches[1];
					
					Vector2 delta1=touch1.deltaPosition.normalized;
					Vector2 delta2=touch2.deltaPosition.normalized;
					Vector2 delta=(delta1+delta2)/2;
					
					float rotX=thisT.rotation.eulerAngles.x-delta.y*rotateSpeed;
					float rotY=thisT.rotation.eulerAngles.y+delta.x*rotateSpeed;
					rotX=Mathf.Clamp(rotX, minRotateAngle, maxRotateAngle);
					
					thisT.rotation=Quaternion.Euler(rotX, rotY, 0);
				}
			}
		}
		
		
		void MouseInput(){
			if(enableRotate){
				if(Input.GetMouseButtonDown(1) && !IsCursorOnUI()){
					lastMousePos=Input.mousePosition;
					dragRotating=true;
				}
				else if(Input.GetMouseButton(1) && dragRotating){
					mouseDeltaPos=Input.mousePosition-lastMousePos;
					lastMousePos=Input.mousePosition;
					
					if(mouseDeltaPos.magnitude!=0){
						float rotY=thisT.eulerAngles.y+rotateSpeed*mouseDeltaPos.x;//*Time.unscaledDeltaTime;
						float rotX=thisT.eulerAngles.x-rotateSpeed*mouseDeltaPos.y;//*Time.unscaledDeltaTime;
						rotX=Mathf.Clamp(rotX, minRotateAngle, maxRotateAngle);
						thisT.rotation=Quaternion.Euler(rotX, rotY, 0);
					}
				}
				else dragRotating=false;
			}
			
			
			if((scrollKey || (scrollCursorDrag && !tempDisableScrollCursorDrag) || scrollCursorOnEdge) && scrollSpeed!=0){
				float moveH=0;	float moveV=0;
				
				if(scrollKey){
					if(Input.GetButton("Horizontal")) moveH=Input.GetAxisRaw("Horizontal");
					if(Input.GetButton("Vertical")) moveV=Input.GetAxisRaw("Vertical");
				}
				
				if(scrollCursorDrag && !tempDisableScrollCursorDrag){
					if(Input.GetMouseButtonDown(0) && !IsCursorOnUI()){
						lastMousePos=Input.mousePosition;
						dragScrolling=true;
					}
					else if(Input.GetMouseButton(0) && dragScrolling){
						mouseDeltaPos=Input.mousePosition-lastMousePos;
						lastMousePos=Input.mousePosition;
						moveH=-mouseDeltaPos.x*0.1f;
						moveV=-mouseDeltaPos.y*0.1f;
					}
					else dragScrolling=false;
				}
				
				if(scrollCursorOnEdge){
					Vector3 mPos=Input.mousePosition;
					
					if(mPos.x>=Screen.width-scrollCursorOnEdgeTH) moveH=1;
					else if(mPos.x<=scrollCursorOnEdgeTH) moveH=-1;
					
					if(mPos.y>=Screen.height-scrollCursorOnEdgeTH) moveV=1;
					else if(mPos.y<=scrollCursorOnEdgeTH) moveV=-1;
				}
				
				if(moveH!=0){
					Vector3 dirH=transform.InverseTransformDirection(Quaternion.Euler(0, thisT.eulerAngles.y, 0)*Vector3.right);
					thisT.Translate(dirH * scrollSpeed * GetdeltaT() * moveH);
				}
				if(moveV!=0){
					Vector3 dirV=transform.InverseTransformDirection(Quaternion.Euler(0, thisT.eulerAngles.y, 0)*Vector3.forward);
					thisT.Translate(dirV * scrollSpeed * GetdeltaT() * moveV);
				}
				
				ApplyHorizontalLimit(moveH, moveV);
			}
			
			
			if(enableZoom){
				float zoomInput=Input.GetAxis("Mouse ScrollWheel");
				if(zoomInput!=0) currentZoom=Mathf.Clamp(currentZoom+zoomSpeed*zoomInput, -maxZoomDistance, -minZoomDistance);
				
				Zoom();
			}
		}
		
		
		private void ApplyHorizontalLimit(float moveH, float moveV){
			if(enablePositionLimit && (moveH!=0 || moveV!=0 || linkPanLimitToZoom)){
				Vector2 limitX=GetLimitX();
				Vector2 limitZ=GetLimitZ();
				
				float x=Mathf.Clamp(thisT.position.x, limitX.x, limitX.y);
				float z=Mathf.Clamp(thisT.position.z, limitZ.x, limitZ.y);
				thisT.position=new Vector3(x, thisT.position.y, z);
			}
		}
		
		
		private void Zoom(){
			if(avoidClipping){
				Vector3 aPos=thisT.TransformPoint(new Vector3(0, 0, currentZoom));
				Vector3 dirC=aPos-thisT.position;
				float dist=Vector3.Distance(aPos, thisT.position); RaycastHit hit;
				hasObstacle=Physics.Raycast (thisT.position, dirC, out hit, dist);
				
				if(hasObstacle){
					dist=Vector3.Distance(hit.point, thisT.position)*0.85f;
					float camZ=Mathf.Lerp(camT.localPosition.z, -dist, Time.deltaTime*50);
					camT.localPosition=new Vector3(camT.localPosition.x, camT.localPosition.y, camZ);
				}
			}
			
			if(!avoidClipping || !hasObstacle){
				currentZoom=Mathf.Clamp(currentZoom, -maxZoomDistance, -minZoomDistance);
				float camZ=Mathf.Lerp(camT.localPosition.z, currentZoom, Time.deltaTime*4);
				camT.localPosition=new Vector3(camT.localPosition.x, camT.localPosition.y, camZ);
			}
		}
		
		
		private float GetdeltaT(){ return Time.unscaledDeltaTime; }
		
		
		
		private bool tempDisableScrollCursorDrag;		//for touch input, disable drag scrolling when drag and drop tower build mode is activated
		public static void DisableScrollCursorDrag(){
			if(!UIControl.DisableCameraDragOnDragNDrop()) return;
			instance.tempDisableScrollCursorDrag=true;
		}
		public static void EnableScrollCursorDrag(){
			if(!UIControl.DisableCameraDragOnDragNDrop()) return;
			instance.tempDisableScrollCursorDrag=false;
		}
		
		
		
		
		[Header("DynamicLimit")]
		public bool linkPanLimitToZoom;
		public float limitExtX_ZOut=10;	//limit when zoom all the way out
		public float limitExtZ_ZOut=10;
		
		public Vector2 GetLimitX(){
			if(!linkPanLimitToZoom) return new Vector2(minPosX, maxPosX);
			
			float zoomRatio=1-(Mathf.Abs(camT.localPosition.z)-minZoomDistance)/(maxZoomDistance-minZoomDistance);
			return new Vector2(minPosX-zoomRatio*limitExtX_ZOut, maxPosX+zoomRatio*limitExtX_ZOut);
		}
		public Vector2 GetLimitZ(){
			if(!linkPanLimitToZoom) return new Vector2(minPosZ, maxPosZ);
			
			float zoomRatio=1-(Mathf.Abs(camT.localPosition.z)-minZoomDistance)/(maxZoomDistance-minZoomDistance);
			return new Vector2(minPosZ-zoomRatio*limitExtZ_ZOut, maxPosZ+zoomRatio*limitExtZ_ZOut);
		}
		
		
		[Space(10)] public bool showGizmo=true;
		void OnDrawGizmos(){
			if(showGizmo && enablePositionLimit){
				Vector3 p1=new Vector3(minPosX, transform.position.y, maxPosZ);
				Vector3 p2=new Vector3(maxPosX, transform.position.y, maxPosZ);
				Vector3 p3=new Vector3(maxPosX, transform.position.y, minPosZ);
				Vector3 p4=new Vector3(minPosX, transform.position.y, minPosZ);
				
				Gizmos.color=Color.green;
				GizmosDrawSquare(p1, p2, p3, p4);
				
				if(linkPanLimitToZoom){
					if(camT==null) camT=transform.GetChild(0);
					
					p1=new Vector3(minPosX-limitExtX_ZOut, transform.position.y, maxPosZ+limitExtZ_ZOut);
					p2=new Vector3(maxPosX+limitExtX_ZOut, transform.position.y, maxPosZ+limitExtZ_ZOut);
					p3=new Vector3(maxPosX+limitExtX_ZOut, transform.position.y, minPosZ-limitExtZ_ZOut);
					p4=new Vector3(minPosX-limitExtX_ZOut, transform.position.y, minPosZ-limitExtZ_ZOut);
					GizmosDrawSquare(p1, p2, p3, p4);
					
					Vector2 limitX=GetLimitX();
					Vector2 limitZ=GetLimitZ();
					
					Gizmos.color=new Color(.2f, 1f, 1f, 1f);
					p1=new Vector3(limitX.x, transform.position.y, limitZ.y);
					p2=new Vector3(limitX.y, transform.position.y, limitZ.y);
					p3=new Vector3(limitX.y, transform.position.y, limitZ.x);
					p4=new Vector3(limitX.x, transform.position.y, limitZ.x);
					GizmosDrawSquare(p1, p2, p3, p4);
				}
			}
		}
		void GizmosDrawSquare(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4){
			Gizmos.DrawLine(p1, p2);
			Gizmos.DrawLine(p2, p3);
			Gizmos.DrawLine(p3, p4);
			Gizmos.DrawLine(p4, p1);
		}
		
	}
	

}