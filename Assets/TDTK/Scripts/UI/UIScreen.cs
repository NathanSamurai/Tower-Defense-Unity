using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIScreen : MonoBehaviour {

		protected GameObject thisObj;
		protected RectTransform rectT;
		protected CanvasGroup canvasGroup;
		
		public virtual void Awake(){
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			//canvasGroup.interactable=false;
			//canvasGroup.blocksRaycasts=false;
			
			canvasGroup.alpha=0;
			rectT.anchoredPosition=new Vector3(0, 0, 0);
		}

		// Use this for initialization
		public virtual void Start(){ }
		
		
		public virtual void _Show(float duration=0.25f){ _Show(false, duration); }
		public void _Show(bool instant, float duration=0.25f){
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			if(!instant) UI.FadeIn(canvasGroup, duration, thisObj);
			else{
				canvasGroup.alpha=1;
				thisObj.SetActive(true);
			}
		}
		public virtual void _Hide(float duration=0.25f){ _Hide(false, duration); }
		public void _Hide(bool instant, float duration=0.25f){
			//canvasGroup.interactable=false;
			//canvasGroup.blocksRaycasts=false;
			
			if(!instant) UI.FadeOut(canvasGroup, duration, thisObj);
			else{
				canvasGroup.alpha=0;
				thisObj.SetActive(false);
			}
		}
		
	}
	
}
