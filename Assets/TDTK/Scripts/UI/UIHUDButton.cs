using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIHUDButton : MonoBehaviour {
		
		public UIButton buttonSpawn;
		public Slider sliderSpawnTimer;
		
		public UIButton buttonPerk;
		
		public UIButton buttonFF;
		
		public UIButton buttonPause;
		
		
		
		// Use this for initialization
		void Start () {
			UpdateWaveDisplay(1);
			
			buttonSpawn.Init();		
			buttonSpawn.button.onClick.AddListener(() => OnSpawnButton());
			buttonSpawn.SetActive(true);
			
			sliderSpawnTimer.gameObject.SetActive(false);
			
			if(PerkManager.IsEnabled() && buttonPerk.rootObj!=null){
				buttonPerk.Init();		buttonPerk.button.onClick.AddListener(() => OnPerkButton());
			}
			else{
				if(buttonPerk.rootObj!=null) buttonPerk.rootObj.SetActive(false);
			}
			
			buttonFF.Init();		buttonFF.button.onClick.AddListener(() => OnFFButton());
			buttonPause.Init();	buttonPause.button.onClick.AddListener(() => OnPauseButton());
		}
		
		
		void OnEnable(){ 
			TDTK.onEnableSpawnE += OnEnableSpawn;
			TDTK.onSpawnCountDownE += OnSpawnCountDown;
			TDTK.onNewWaveE += UpdateWaveDisplay;
		}
		void OnDisable(){ 
			TDTK.onEnableSpawnE -= OnEnableSpawn;
			TDTK.onSpawnCountDownE -= OnSpawnCountDown;
			TDTK.onNewWaveE -= UpdateWaveDisplay;
		}
		
		
		void OnEnableSpawn(){
			buttonSpawn.SetActive(true);
		}
		void OnSpawnButton(){
			SpawnManager.Spawn();
			buttonSpawn.SetActive(false);
		}
		
		void UpdateWaveDisplay(int wave){
			buttonSpawn.SetActive(false);
		}
		
		
		private bool coutingDown=false;
		void OnSpawnCountDown(){
			coutingDown=true; 
			Update();
			sliderSpawnTimer.gameObject.SetActive(true);
		}
		
		
		void Update(){
			if(!coutingDown) return;
			
			float time=SpawnManager.GetTimeToNextWave();
			
			if(time>0){
				//lbSpawnTimer.text="Time to next wave - "+time.ToString("f1")+"s";
				
				buttonSpawn.SetLabel("Spawn ("+time.ToString("f1")+"s)");
				sliderSpawnTimer.value=SpawnManager.GetTimeToNextWaveRatio();
			}
			else{
				buttonSpawn.SetLabel("Spawn");
				sliderSpawnTimer.gameObject.SetActive(false);
				
				//lbSpawnTimer.text="";
				coutingDown=false;
			}
		}
		
		
		
		void OnPerkButton(){ UIPerkScreen.Show(); }
		
		
		void OnFFButton(){
			if(Time.timeScale==1){
				Time.timeScale=3;
				buttonFF.SetLabel(">>");
			}
			else{
				Time.timeScale=1;
				buttonFF.SetLabel(">");
			}
		}
		
		void OnPauseButton(){ UIPauseScreen.Show(); }
		
	}

}