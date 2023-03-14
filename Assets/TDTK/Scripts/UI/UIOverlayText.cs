using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIOverlayText : MonoBehaviour {
		
		//public bool alwaysVisible=false;
		//public static bool AlwaysVisible(){ return instance.alwaysVisible; }
		
		//public float duration=1.5f;
		public static float GetDuration(){ return .5f; }//instance.duration; }
		
		public GameObject rootOverlayItem;
		public List<UITextOverlayItem> overlayItemList=new List<UITextOverlayItem>();
		
		private static UIOverlayText instance;
		
		void Awake() {
			instance=this;
		}
		
		void Start(){
			if(!UIControl.ShowTextOverlay()){
				gameObject.SetActive(false);
				return;
			}
			
			for(int i=0; i<1; i++){
				if(i==0) overlayItemList.Add(rootOverlayItem.AddComponent<UITextOverlayItem>());
				else overlayItemList.Add(UI.Clone(rootOverlayItem).GetComponent<UITextOverlayItem>());
				
				overlayItemList[i].Init();
			}
		}
		
		void OnEnable(){ TDTK.onTextOverlayE += Show; }
		void OnDisable(){ TDTK.onTextOverlayE -= Show; }
		
		
		public void Show(string msg, Vector3 pos){
			int idx=GetUnusedItemIndex();
			overlayItemList[idx].Show(msg, pos, new Color(1, 1, 1, 0));
		}
		public static void Show(string msg, Vector3 pos, Color color=default(Color)){
			int idx=instance.GetUnusedItemIndex();
			instance.overlayItemList[idx].Show(msg, pos, color);
		}
		
		
		private int GetUnusedItemIndex(){
			for(int i=0; i<overlayItemList.Count; i++){
				if(overlayItemList[i].IsActive()) continue;
				return i;
			}
			
			overlayItemList.Add(UI.Clone(rootOverlayItem).GetComponent<UITextOverlayItem>());
			return overlayItemList.Count-1;
		}
		
		
		private Color defaultColor;
		private bool defaultColorSet;
		
		public static void SetDefaultColor(Color col){
			if(instance.defaultColorSet) return;
			instance.defaultColorSet=true;
			instance.defaultColor=col;
		}
		public static Color GetDefaultColor(){ return instance.defaultColor; }
	}


	public class UITextOverlayItem : MonoBehaviour {
		
		[HideInInspector] public Vector3 targetPos;
		[HideInInspector] public float duration;
		
		private Text label;
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasG;
		
		public void Init(){
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasG=thisObj.GetComponent<CanvasGroup>();
			
			label=thisObj.GetComponent<Text>();
			
			UIOverlayText.SetDefaultColor(label.color);
		}
		
		void Update(){
			targetPos+=Vector3.up * Time.deltaTime *.4f;
			UpdateScreenPos();
			
			duration-=Time.deltaTime;
			canvasG.alpha=duration>0.25f ? 1 : duration/0.25f;
			if(canvasG.alpha<=0) thisObj.SetActive(false);
		}
		
		public void Show(string msg, Vector3 pos, Color color){
			//pos+=new Vector3(Random.Range(-0.25, 0.25), Random.Range(-0.25, 0.25), Random.Range(-0.25, 0.25));
			
			targetPos=pos+new Vector3(0, .5f, 0)*Time.deltaTime;
			
			if(thisObj==null) Init();
			
			duration=UIOverlayText.GetDuration();
			
			label.text=msg;
			
			if(color.a>0) label.color=color;
			else label.color=UIOverlayText.GetDefaultColor();
			
			UpdateScreenPos();
			
			thisObj.SetActive(true);
		}
		
		void UpdateScreenPos(){
			Vector3 screenPos=Camera.main.WorldToScreenPoint(targetPos);
			screenPos.z=0;
			rectT.localPosition=screenPos*UI.GetScaleFactor();
		}
		
		public bool IsActive(){ return thisObj.activeInHierarchy; }
		
	}

}