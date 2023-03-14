using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//using UnityStandardAssets.ImageEffects;

namespace TDTK{

	public class UI : MonoBehaviour {
		
		public static UI instance;
		
		public static void Init(){
			if(instance!=null) return;
			
			GameObject obj=new GameObject("UI_Utility");
			instance=obj.AddComponent<UI>();
		}
		
		
		public static float GetScaleFactor(){ return UIControl.GetScaleReferenceWidth()/Screen.width; }
		
		
		//inputID=-1 - mouse cursor, 	inputID>=0 - touch finger index
		public static bool IsCursorOnUI(int inputID=-1){
			if(inputID<0 && Input.touchCount>0) inputID=Input.touches[0].fingerId;
			
			EventSystem eventSystem = EventSystem.current;
			return ( eventSystem.IsPointerOverGameObject( inputID ) );
		}
		
		public static GameObject Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)) {
			GameObject newObj=(GameObject)MonoBehaviour.Instantiate(srcObj);
			newObj.name=name=="" ? srcObj.name : name ;
			
			newObj.transform.SetParent(srcObj.transform.parent);
			newObj.transform.localPosition=srcObj.transform.localPosition+posOffset;
			newObj.transform.localScale=srcObj.transform.localScale;
			
			return newObj;
		}
		
		
		public static void SetCallback(GameObject obj, Callback enter=null, Callback exit=null){
			UIItemCallback itemCallback=obj.GetComponent<UIItemCallback>();
			if(itemCallback==null) itemCallback=obj.AddComponent<UIItemCallback>();
			itemCallback.SetEnterCallback(enter);
			itemCallback.SetExitCallback(exit);
		}
		
		
		
		//0 - bottom left
		//1 - top left
		//2 - top right
		//3 - bottom right
		public static Vector3 GetCorner(RectTransform rectT, int corner=0){
			Vector3[] fourCornersArray=new Vector3[4];
			rectT.GetWorldCorners(fourCornersArray);
			return fourCornersArray[corner];
		}
		
		public static void SetPivot(int pivotCorner, RectTransform rect){
			if(pivotCorner==0) rect.pivot=new Vector3(0, 0);
			if(pivotCorner==1) rect.pivot=new Vector3(0, 1);
			if(pivotCorner==2) rect.pivot=new Vector3(1, 1);
			if(pivotCorner==3) rect.pivot=new Vector3(1, 0);
		}
		
		
		
		public static string IntToString(int val){	//not being used right now
			return string.Format("{0:#,0}", val);
		}
		
		
		
		public static string Color(string txt){ return "<color=#ff9632ff>"+txt+"</color>"; }		//255, 150, 64
		
		
		public static int GetItemIndex(GameObject uiObj, List<UIObject> objList){
			for(int i=0; i<objList.Count; i++){ if(objList[i].rootObj==uiObj) return i;}
			return 0;
		}
		public static int GetItemIndex(GameObject uiObj, List<UIButton> objList){
			for(int i=0; i<objList.Count; i++){ if(objList[i].rootObj==uiObj) return i;}
			return 0;
		}
		
		
		
		public static IEnumerator WaitForRealSeconds(float time){
			Init();	float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + time) yield return null;
		}
		
		
		#region canvasgroup fade
		public static void FadeOut(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){
			Init();	instance.StartCoroutine(instance._Fade(canvasGroup, 1f/duration, 1, 0, obj));
		}
		public static void FadeIn(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){ 
			Init();	instance.StartCoroutine(instance._Fade(canvasGroup, 1f/duration, 0, 1, obj)); 
		}
		public static void Fade(CanvasGroup canvasGroup, float duration=0.25f, float startValue=0.5f, float endValue=0.5f){ 
			Init();	instance.StartCoroutine(instance._Fade(canvasGroup, 1f/duration, startValue, endValue));
		}
		IEnumerator _Fade(CanvasGroup canvasGroup, float timeMul, float startValue, float endValue, GameObject obj=null){
			if(endValue>0 && obj!=null) obj.SetActive(true);
			
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(startValue, endValue, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=endValue;
			
			if(endValue<=0 && obj!=null) obj.SetActive(false);
		}
		#endregion
		
		
		#region blur
		/*
		public static void FadeBlur(UnityStandardAssets.ImageEffects.BlurOptimized blurEff, float startValue=0, float targetValue=0){
			Init();	if(blurEff==null || instance==null) return;
			instance.StartCoroutine(instance.FadeBlurRoutine(blurEff, startValue, targetValue));
		}
		//change the blur component blur size from startValue to targetValue over 0.25 second
		IEnumerator FadeBlurRoutine(UnityStandardAssets.ImageEffects.BlurOptimized blurEff, float startValue=0, float targetValue=0){
			blurEff.enabled=true;
			
			float duration=0;
			while(duration<1){
				float value=Mathf.Lerp(startValue, targetValue, duration);
				blurEff.blurSize=value;
				duration+=Time.unscaledDeltaTime*4f;	//multiply by 4 so it only take 1/4 of a second
				yield return null;
			}
			blurEff.blurSize=targetValue;
			
			if(targetValue==0) blurEff.enabled=false;
			if(targetValue==1) blurEff.enabled=true;
		}
		*/
		#endregion
		
	}

}