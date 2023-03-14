using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIMessage : UIScreen {
		private Vector3 scale=new Vector3(1, 1, 1);
		private float scaleZoomed=1.15f;
		
		public GameObject messageObj;
		public List<UIMsgItem> msgList=new List<UIMsgItem>();
		
		private static UIMessage instance;
		
		public override void Awake () {
			base.Awake();
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			canvasGroup.alpha=1;
			
			instance=this;
			thisObj.SetActive(true);
		}
		
		public override void Start () {
			for(int i=0; i<5; i++){
				if(i==0) msgList.Add(new UIMsgItem(messageObj));
				else msgList.Add(new UIMsgItem(UI.Clone(messageObj, "TextMessage"+(msgList.Count+1))));
				msgList[i].rootObj.SetActive(false);
			}
		}
		
		
		void OnEnable(){
			TDTK.onPopupMessageE += DisplayMessage;
		}
		void OnDisable(){
			TDTK.onPopupMessageE -= DisplayMessage;
		}
		
		
		
		public static void DisplayMessage(string msg){ 
			DisplayMessageStack(msg);
			//DisplayMessageSingle(msg);
		}
		
		
		#if UNITY_EDITOR
		//~ void Update(){
			//~ if(Input.GetKeyDown(KeyCode.Space)){
				//~ Debug.Log("fire from UIMessage.cs");
				//~ DisplayMessageStack("This is a test message "+Random.Range(0, 99999)+" !!");
			//~ }
		//~ }
		#endif
		
		
		
		public static void DisplayMessageStack(string msg){ instance._DisplayMessageStack(msg); }
		void _DisplayMessageStack(string msg){
			int index=GetUnusedTextIndex();
			msgList[index].label.text=msg;
			StartCoroutine(DisplayItemRoutineStack(msgList[index]));
		}
		
		IEnumerator DisplayItemRoutineStack(UIMsgItem item){
			//item.rectT.SetAsFirstSibling();
			item.rectT.SetAsLastSibling();
			
			UI.FadeIn(item.canvasG, 0.1f, item.rootObj);
			
			StartCoroutine(ScaleRectTRoutineStack(item.rectT, .1f, scale, scale*scaleZoomed));	
			yield return StartCoroutine(UI.WaitForRealSeconds(.1f));
			StartCoroutine(ScaleRectTRoutineStack(item.rectT, .25f, scale*scaleZoomed, scale));
			
			yield return StartCoroutine(UI.WaitForRealSeconds(2.25f));
			
			UI.FadeOut(item.canvasG, 1.0f, item.rootObj);
		}
		
		IEnumerator ScaleRectTRoutineStack(RectTransform rectt, float dur, Vector3 startS, Vector3 endS){
			float timeMul=1f/dur;
			float duration=0;
			while(duration<1){
				rectt.localScale=Vector3.Lerp(startS, endS, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
		}
		
		
		
		private int GetUnusedTextIndex(){
			for(int i=0; i<msgList.Count; i++){
				if(msgList[i].rootObj.activeInHierarchy) continue;
				return i;
			}
			
			msgList.Add(new UIMsgItem(UI.Clone(messageObj, "TextMessage"+(msgList.Count+1))));
			return msgList.Count-1;
		}
		
		
		
		[System.Serializable]
		public class UIMsgItem{
			public GameObject rootObj;
			public RectTransform rectT;
			public CanvasGroup canvasG;
			
			public Text label;
			
			public UIMsgItem(){}
			public UIMsgItem(GameObject obj){ rootObj=obj; Init(); }
			
			public virtual void Init(){
				canvasG=rootObj.GetComponent<CanvasGroup>();
				rectT=rootObj.GetComponent<RectTransform>();
				
				label=rootObj.GetComponent<Text>();
				
				canvasG.interactable=false;	canvasG.blocksRaycasts=false;
			}
		}
		
	}

}