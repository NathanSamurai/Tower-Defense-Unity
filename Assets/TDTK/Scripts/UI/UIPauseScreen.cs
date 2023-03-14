using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TDTK;

namespace TDTK{

	public class UIPauseScreen : UIScreen {
		
		[Space(5)]
		public Slider sliderVolumeSFX;
		public Slider sliderVolumeUI;
		
		[Space(8)]
		public UIButton buttonResume;
		public UIButton buttonRestart;
		public UIButton buttonMainMenu;
		
		private static UIPauseScreen instance;
		
		public override void Awake(){
			base.Awake();
			
			instance=this;
		}
		
		public override void Start(){ 
			base.Start();
			
			if(sliderVolumeSFX!=null){
				sliderVolumeSFX.value=AudioManager.GetVolumeSFX();
				sliderVolumeSFX.onValueChanged.AddListener(delegate { OnSFXVolumeChanged(); });
			}
			if(sliderVolumeUI!=null){
				sliderVolumeUI.value=AudioManager.GetVolumeUI();
				sliderVolumeUI.onValueChanged.AddListener(delegate { OnUIVolumeChanged(); });
			}
			
			buttonResume.Init();		buttonResume.button.onClick.AddListener(() => OnResumeButton());
			buttonRestart.Init();		buttonRestart.button.onClick.AddListener(() => OnRestartButton());
			buttonMainMenu.Init();		buttonMainMenu.button.onClick.AddListener(() => OnMenuButton());
			
			thisObj.SetActive(false);
		}
		
		
		public void OnResumeButton(){
			Hide();
		}
		public void OnRestartButton(){
			GameControl.RestartLevel();
		}
		public void OnMenuButton(){
			GameControl.MainMenu();
		}
		
		
		void OnSFXVolumeChanged(){
			AudioManager.SetVolumeSFX(sliderVolumeSFX.value);
		}
		void OnUIVolumeChanged(){
			AudioManager.SetVolumeUI(sliderVolumeUI.value);
		}
		
		
		
		private float cachedTimeScale=1;
		
		public static void Show(){ if(instance!=null) instance._Show(); }
		public override void _Show(float duration=0.25f){
			cachedTimeScale=Time.timeScale;
			Time.timeScale=0;
			
			UIControl.BlurFadeIn();
			
			base._Show();
		}
		public static void Hide(){ if(instance!=null) instance._Hide(); }
		public override void _Hide(float duration=0.25f){
			Time.timeScale=cachedTimeScale;
			
			UIControl.BlurFadeOut();
			
			base._Hide();
		}
		
		public static bool IsActive(){ return instance!=null & instance.thisObj.activeInHierarchy; }
		

	}

}
