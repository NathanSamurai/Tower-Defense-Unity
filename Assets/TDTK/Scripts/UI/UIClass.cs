using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace TDTK{
	
	#region UIObject
	[System.Serializable]
	public class UIObject{
		public GameObject rootObj;
		[HideInInspector] public Transform rootT;
		[HideInInspector] public RectTransform rectT;
		
		[HideInInspector] public CanvasGroup canvasG;
		
		[HideInInspector] public Image image;
		[HideInInspector] public Text label;
		
		[HideInInspector] public UIItemCallback itemCallback;
		
		public UIObject(){}
		public UIObject(GameObject obj){ rootObj=obj; Init(); }
		
		public virtual void Init(){
			if(rootObj==null){ Debug.LogWarning("Unassgined rootObj"); return; }
			
			rootT=rootObj.transform;
			rectT=rootObj.GetComponent<RectTransform>();
			
			foreach(Transform child in rectT){
				if(child.name=="Image") image=child.GetComponent<Image>();
				else if(child.name=="Text") label=child.GetComponent<Text>();
			}
		}
		
		public static UIObject Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)){
			GameObject newObj=UI.Clone(srcObj, name, posOffset);
			return new UIObject(newObj);
		}
		
		public virtual void SetCallback(Callback enter=null, Callback exit=null){
			itemCallback=rootObj.GetComponent<UIItemCallback>();
			if(itemCallback==null) itemCallback=rootObj.AddComponent<UIItemCallback>();
			itemCallback.SetEnterCallback(enter);
			itemCallback.SetExitCallback(exit);
		}
		
		public virtual void SetActive(bool flag){ rootObj.SetActive(flag); }
		
		public void SetImage(Sprite spr){ if(image!=null) image.sprite=spr; }
		public void SetLabel(string txt){ if(label!=null) label.text=txt; }
		
		
		#region dragNdrop
		/*
		public void SetDropZone(CallbackEventData callback=null, CheckCallback enterCheckCB=null){
			itemDropZone=rootObj.GetComponent<UIDropZone>();
			if(itemDropZone==null) itemDropZone=rootObj.AddComponent<UIDropZone>();
			itemDropZone.SetDropCallback(callback);
			itemDropZone.SetEnterCheckCallback(enterCheckCB);
			
			foreach(Transform child in rectT){
				if(child.name=="HoverDummy"){
					itemDropZone.dummyT=child;
					child.gameObject.SetActive(false);
				}
				if(child.name=="HoverHighlight"){
					itemDropZone.hoverHighlight=child.gameObject;
					child.gameObject.SetActive(false);
				}
			}
		}
		public void SetDragNDrop(CallbackInputDependent start=null, CallbackInputDependent drag=null, CallbackEventData end=null, Transform parent=null){
			itemDragNDrop=rootObj.GetComponent<UIDragNDrop>();
			if(itemDragNDrop==null) itemDragNDrop=rootObj.AddComponent<UIDragNDrop>();
			itemDragNDrop.SetUIObj(this);
			itemDragNDrop.SetParentDrag(parent);
			itemDragNDrop.SetBeginCallback(start);
			itemDragNDrop.SetDragCallback(drag);
			itemDragNDrop.SetEndCallback(end);
			
			if(canvasG==null) canvasG=rootObj.AddComponent<CanvasGroup>();
		}
		*/
		#endregion
		
		
		
		//sound are not being used in this package
		public void SetSound(AudioClip eClip, AudioClip dClip){ if(itemCallback!=null) itemCallback.SetSound(eClip, dClip); }
		public void DisableSound(bool disableHover, bool disablePress){ if(itemCallback!=null) itemCallback.DisableSound(disableHover, disablePress); }
	}
	#endregion
	
	
	
	
	
	#region UIButton
	[System.Serializable]
	public class UIButton : UIObject{
		[HideInInspector] public Text label2;
		[HideInInspector] public Text lable3;
		
		//[HideInInspector] 
		public Image image2;
		
		[HideInInspector] public Image hovered;
		[HideInInspector] public Image disabled;
		[HideInInspector] public Image highlight;
		
		[HideInInspector] public Button button;
		
		public UIButton(){}
		public UIButton(GameObject obj){ rootObj=obj; Init(); }
		
		public override void Init(){
			base.Init();
			
			button=rootObj.GetComponent<Button>();
			canvasG=rootObj.GetComponent<CanvasGroup>();
			
			foreach(Transform child in rectT){
				if(child.name=="TextAlt")				label2=child.GetComponent<Text>();
				else if(child.name=="TextAlt2")		lable3=child.GetComponent<Text>();
				else if(child.name=="ImageAlt")	image2=child.GetComponent<Image>();
				else if(child.name=="Hovered") 	hovered=child.GetComponent<Image>();
				else if(child.name=="Disabled") 	disabled=child.GetComponent<Image>();
				else if(child.name=="Highlight") 	highlight=child.GetComponent<Image>();
			}
		}
		
		public static new UIButton Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)){
			GameObject newObj=UI.Clone(srcObj, name, posOffset);
			return new UIButton(newObj);
		}
		
		public override void SetCallback(Callback enter=null, Callback exit=null){ base.SetCallback(enter, exit); }
		public void SetClickCallback(Callback down=null, Callback up=null){
			itemCallback=rootObj.GetComponent<UIItemCallback>();
			if(itemCallback==null) itemCallback=rootObj.AddComponent<UIItemCallback>();
			itemCallback.SetDownCallback(down);
			itemCallback.SetUpCallback(up);
		}
		
		public override void SetActive(bool flag){ base.SetActive(flag); }
		
		public void SetHighlight(bool flag){ highlight.gameObject.SetActive(flag); }
	}
	#endregion
	
	
	
	#region callback
	public delegate void Callback(GameObject uiObj);
	
	public class UIItemCallback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler{
		
		private Callback downCB;
		private Callback upCB;
		
		public void SetDownCallback(Callback callback){ downCB=callback; }
		public void SetUpCallback(Callback callback){ upCB=callback; }
		
		public void OnPointerDown(PointerEventData eventData){ 
			if(downCB!=null) downCB(thisObj);
		}
		public void OnPointerUp(PointerEventData eventData){ 
			if(upCB!=null) upCB(thisObj);
		}
		
		
		private Callback enterCB;
		private Callback exitCB;
		
		public void SetEnterCallback(Callback callback){ enterCB=callback; }
		public void SetExitCallback(Callback callback){ exitCB=callback; }
		
		public void OnPointerEnter(PointerEventData eventData){ 
			//if(enterClip!=null && button!=null && button.interactable) AudioManager.PlayUISound(enterClip);
			if(enterCB!=null) enterCB(thisObj);
		}
		public void OnPointerExit(PointerEventData eventData){ 
			if(exitCB!=null) exitCB(thisObj);
		}
		
		
		private GameObject thisObj;
		void Awake(){
			thisObj=gameObject;
			SetupAudioClip();
		}
		
		
		//audio is not being used in this package
		private bool useCustomAudioClip=false;
		public AudioClip enterClip;
		public AudioClip downClip;
		
		void SetupAudioClip(){
			if(useCustomAudioClip) return;
			//enterClip=AudioManager.GetHoverButtonSound();
			//downClip=AudioManager.GetPressButtonSound();
		}
		
		public void SetSound(AudioClip eClip, AudioClip dClip){
			useCustomAudioClip=true;		enterClip=eClip;	downClip=dClip;
		}
		
		public void DisableSound(bool disableHover, bool disablePress){
			if(disableHover) enterClip=null;
			if(disablePress) downClip=null;
		}
	}
	#endregion
	
}
