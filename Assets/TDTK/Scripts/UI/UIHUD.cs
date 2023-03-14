using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TDTK{

	public class UIHUD : MonoBehaviour {
		
		//~ public Text lbLife;
		//~ public Text lbWave;
		//~ public GameObject spaceLife;
		//~ public GameObject spaceWave;
		
		//[Space(10)]
		public UIObject waveItem;
		public GameObject spaceObjWave;
		
		[Space(10)]
		public UIObject lifeItem;
		public GameObject spaceObjLife;
		
		[Space(10)]
		public List<UIObject> rscItemList=new List<UIObject>();
		public GameObject spaceObjRsc;
		
		//public Text lbAbilityRsc;
		//public Slider sliderAbilityRsc;
		
		//public UIButton buttonSpawn;
		//public Slider sliderSpawnTimer;
		
		
		[Space(10)]
		public Slider sliderAbilityRsc;
		public Text lbAbilityRsc;
		public Image imgAbilityRsc;
		public GameObject spaceObjAbility;
		
		
		[Space(10)]
		public UIObject perkRscItem;
		//public GameObject spaceObjPerk;
		
		
		
		
		[HideInInspector] public UIButton buttonPerk;
		
		[HideInInspector] public UIButton buttonFF;
		
		[HideInInspector] public UIButton buttonPause;
		
		
		
		// Use this for initialization
		void Start () {
			GameObject lastSpaceObj=null;
			
			if(UIControl.IsGameScene()){
				lifeItem.Init();
				waveItem.Init();
				
				UpdateLifeDisplay(GameControl.GetLife());
				UpdateWaveDisplay(1);
				
				lastSpaceObj=spaceObjLife;
			}
			else{
				waveItem.rootObj.SetActive(false);
				spaceObjWave.SetActive(false);
				
				lifeItem.rootObj.SetActive(false);
				spaceObjLife.SetActive(false);
			}
			
			
			if(UIControl.IsGameScene() || PerkManager.UseRscManagerForCost()){
				for(int i=0; i<RscManager.GetResourceCount(); i++){
					if(i>0) rscItemList.Add(new UIObject(UI.Clone(rscItemList[0].rootObj)));
					rscItemList[i].Init();
					rscItemList[i].image.sprite=RscManager.GetRscIcon(i);
					
					rscItemList[i].rectT.SetSiblingIndex(rscItemList[0].rectT.GetSiblingIndex()+i);
				}
				UpdateResourceDisplay(RscManager.GetResourceList());
				
				lastSpaceObj=spaceObjRsc;
			}
			else{
				rscItemList[0].rootObj.SetActive(false);
				spaceObjRsc.SetActive(false);
			}
			
			
			if(!UIControl.IsGameScene() || !AbilityManager.IsEnabled() || AbilityManager.UseRscManagerForCost()){
				sliderAbilityRsc.gameObject.SetActive(false);
				spaceObjAbility.SetActive(false);
			}
			else{
				imgAbilityRsc.sprite=Ability_DB.GetRscIcon();
				UpdateAbilityRscDisplay(AbilityManager.GetRsc());
				lastSpaceObj=spaceObjAbility;
			}
			
			if(!PerkManager.IsEnabled() || PerkManager.UseRscManagerForCost()){
				perkRscItem.rootObj.SetActive(false);
				//spaceObjPerk.SetActive(false);
				lastSpaceObj.SetActive(false);
			}
			else{
				perkRscItem.Init();
				perkRscItem.image.sprite=Perk_DB.GetRscIcon();
				UpdatePerkRscDisplay(PerkManager.GetRsc());
			}
			
			
			
			
			//~ if(!UIControl.DisablePerkScreen()){
				//~ buttonPerk.Init();
				//~ buttonPerk.SetCallback(null, null, this.OnPerkButton, null);
			//~ }
			//~ else{
				//~ if(buttonPerk.rootObj!=null) buttonPerk.rootObj.SetActive(false);
			//~ }
			
			
			//~ buttonFF.Init();
			//~ buttonFF.SetCallback(null, null, this.OnFFButton, null);
			
			//~ buttonPause.Init();
			//~ buttonPause.SetCallback(null, null, this.OnPauseButton, null);
		}
		
		void OnEnable(){ 
			TDTK.onLifeChangedE += UpdateLifeDisplay;
			TDTK.onRscChangedE += UpdateResourceDisplay;
			TDTK.onAbilityRscChangedE += UpdateAbilityRscDisplay;
			TDTK.onPerkRscChangedE += UpdatePerkRscDisplay;
			//TDTK.onEnableSpawnE += OnEnableSpawn;
			//TDTK.onSpawnCountDownE += OnSpawnCountDown;
			TDTK.onNewWaveE += UpdateWaveDisplay;
		}
		void OnDisable(){ 
			TDTK.onLifeChangedE -= UpdateLifeDisplay;
			TDTK.onRscChangedE -= UpdateResourceDisplay;
			TDTK.onAbilityRscChangedE -= UpdateAbilityRscDisplay;
			TDTK.onPerkRscChangedE -= UpdatePerkRscDisplay;
			//TDTK.onEnableSpawnE -= OnEnableSpawn;
			//TDTK.onSpawnCountDownE -= OnSpawnCountDown;
			TDTK.onNewWaveE -= UpdateWaveDisplay;
		}
		
		
		void UpdateLifeDisplay(int life){
			if(!GameControl.CapLife()) lifeItem.label.text=life.ToString("f0");
			else lifeItem.label.text=life.ToString("f0")+"/"+GameControl.GetLifeCap().ToString("f0");
		}
		
		void UpdateResourceDisplay(List<int> list){
			for(int i=0; i<rscItemList.Count; i++) rscItemList[i].label.text=list[i].ToString();
		}
		
		void UpdateAbilityRscDisplay(int value){
			lbAbilityRsc.text=AbilityManager.GetRsc()+"/"+AbilityManager.GetRscCap();
			sliderAbilityRsc.value=AbilityManager.GetRscRatio();
		}
		
		void UpdatePerkRscDisplay(int value){
			if(perkRscItem.label.text!=null) perkRscItem.label.text=value.ToString();
		}
		
		
		
		void UpdateWaveDisplay(int wave){
			//lbWave.text="wave-"+wave+(!SpawnManager.IsEndlessMode() ? "/"+SpawnManager.GetTotalWaveCount() : "" );
			waveItem.label.text=wave+(!SpawnManager.IsEndlessMode() ? "/"+SpawnManager.GetTotalWaveCount() : "" );
			//buttonSpawn.SetActive(false);
		}
		
		/*
		void OnEnableSpawn(){
			buttonSpawn.SetActive(true);
		}
		void OnSpawnButton(GameObject butObj, int pointerID=-1){
			SpawnManager.Spawn();
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
				
				buttonSpawn.label.text="Spawn ("+time.ToString("f1")+"s)";
				sliderSpawnTimer.value=SpawnManager.GetTimeToNextWaveRatio();
			}
			else{
				buttonSpawn.label.text="Spawn";
				sliderSpawnTimer.gameObject.SetActive(false);
				
				//lbSpawnTimer.text="";
				coutingDown=false;
			}
			
			if(Input.GetKeyDown(KeyCode.Escape)){
				if(UIPauseScreen.IsActive()) UIPauseScreen.Hide();
				else UIPauseScreen.Show();
			}
		}
		*/
		
		
		
		void OnPerkButton(GameObject butObj, int pointerID=-1){ UIPerkScreen.Show(); }
		
		
		void OnFFButton(GameObject butObj, int pointerID=-1){
			if(Time.timeScale==1){
				Time.timeScale=3;
				buttonFF.label.text=">>";
			}
			else{
				Time.timeScale=1;
				buttonFF.label.text=">";
			}
		}
		
		void OnPauseButton(GameObject butObj, int pointerID=-1){ UIPauseScreen.Show(); }
		
		
	}

}