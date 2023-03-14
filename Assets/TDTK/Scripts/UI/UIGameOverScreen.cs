using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TDTK;

namespace TDTK{

	public class UIGameOverScreen : UIScreen {
		
		public Text lbGameOverMsg;
		
		public UIButton buttonContinue;
		public UIButton buttonRestart;
		public UIButton buttonMainMenu;
		
		private static UIGameOverScreen instance;
		
		public override void Awake(){
			base.Awake();
			
			instance=this;
		}
		
		public override void Start(){ 
			base.Start();
			
			buttonContinue.Init();		buttonContinue.button.onClick.AddListener(() => OnContinueButton());
			
			buttonRestart.Init();		buttonRestart.button.onClick.AddListener(() => OnRestartButton());
			
			buttonMainMenu.Init();		buttonMainMenu.button.onClick.AddListener(() => OnMenuButton());
			
			thisObj.SetActive(false);
		}
		
		
		public void OnContinueButton(){
			GameControl.NextLevel();
		}
		public void OnRestartButton(){
			GameControl.RestartLevel();
		}
		public void OnMenuButton(){
			GameControl.MainMenu();
		}
		
		
		
		public static void Show(bool playerWon){ if(instance!=null) instance._Show(playerWon); }
		public void _Show(bool playerWon){
			if(playerWon) lbGameOverMsg.text="Level Completed";
			else lbGameOverMsg.text="Game Over";
			
			buttonContinue.button.interactable=playerWon;
			//buttonContinue.SetActive(playerWon);
			
			UIControl.BlurFadeIn();
			
			base._Show();
		}
		public static void Hide(){ 
			UIControl.BlurFadeOut();
			
			if(instance!=null && instance.thisObj.activeInHierarchy) instance._Hide();
		}
		
	}

}
