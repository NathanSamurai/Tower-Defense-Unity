using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TDTK {

	public class GameControl : MonoBehaviour {
		
		public enum _GameState{ Init, Playing, Paused, Over }
		//[HideInInspector] 
		public _GameState gameState=_GameState.Init;
		public static bool HasGameStarted(){ return instance.gameState!=_GameState.Init; }
		public static bool IsGamePlaying(){ return instance.gameState==_GameState.Playing; }
		public static bool IsGameOver(){ return instance.gameState==_GameState.Over; }
		public static bool IsGamePaused(){ return instance.gameState==_GameState.Paused; }
		
		private bool playerWon=false;
		public static bool HasPlayerWon(){ return instance.playerWon; }
		
		public int levelID=-1;	//used by level management to track progress
		public static int highestLevelID=0;
		public static int GetHighestLevelID(){ return highestLevelID; }
		
		public bool capLife=false;
		public int life=10;
		public int lifeCap=10;
		
		public bool regenLife=false;
		public float lifeRegen=0;
		public float lifeRegenMul=1;
		private float lifeRegenCached=0;
		
		public static int GetLife(){ return instance.life; }
		public static void GainLife(int value=1){	if(value==0) return;
			if(instance.capLife){
				int cap=GetLifeCap();
				if(instance.life>=cap) return;
				else value=Mathf.Min(value, cap-instance.life);
			}
			instance.life+=value;
			TDTK.OnLifeChanged(instance.life); 
		}
		public static void LostLife(int value=1){	if(value==0) return;
			instance.life=Mathf.Max(0, instance.life-value);
			TDTK.OnLifeChanged(instance.life);
			
			if(instance.life<=0) EndGame();
			else AudioManager.OnLostLife();
		}
		
		public static bool CapLife(){ return instance.capLife; }
		public static int GetLifeCap(){ return instance.lifeCap; }
		
		public static void ModifyLifeCap(int value){	if(value==0) return;
			if(!instance.capLife) return;
			instance.lifeCap+=value;
			if(value>0) instance.life+=value;
			instance.life=Mathf.Min(instance.life, instance.lifeCap);
			TDTK.OnLifeChanged(instance.life); 
		}
		
		public static void ModifyLifeRegen(float value){ instance.lifeRegen+=value; }
		public static void ModifyLifeRegenMultiplier(float value){ instance.lifeRegenMul+=value; }
		
		
		[Space(10)]
		public List<float> rscGainOnWin=new List<float>();
		public int perkRscGainOnWin=0;
		
		
		public static GameControl instance;
		
		void Awake() {
			instance=this;
			
			ObjectPoolManager.Init();
			TowerManager.Init();
			Path.Init();
			SpawnManager.Init();
			
			if(highestLevelID<levelID) highestLevelID=levelID;
		}
		
		IEnumerator Start(){
			rscGainOnWin=RscManager.MatchRscList(rscGainOnWin, 0);
			yield return null;
		}
		
		
		public static void StartGame(){
			instance.gameState=_GameState.Playing;
		}
		public static void EndGame(){
			if(instance.gameState==_GameState.Over) return;
			
			instance.playerWon=instance.life>0;
			instance.gameState=_GameState.Over;
			
			if(instance.playerWon){
				RscManager.GainRsc(instance.rscGainOnWin);
				PerkManager.GainRsc(instance.perkRscGainOnWin);
				
				RscManager.CachedRsc();			//for rsc to be carry forth to next level
				PerkManager.CachedProgress();	//for perk progress to be carry forth to next level
				AudioManager.OnPlayerWon();
				
				//UIMenu_Play.CompleteLevel();
			}
			else AudioManager.OnPlayerLost();
			
			Debug.Log("Game Over "+instance.playerWon+"    "+instance.gameState);
			TDTK.OnGameOver(instance.playerWon);
		}
		
		
		//~ void Update(){
			//~ if(Input.GetKeyDown(KeyCode.Space)){
				//~ Debug.Log("pause and unpause");
				//~ if(Time.timeScale!=0) Time.timeScale=0;
				//~ else Time.timeScale=1;
			//~ }
		//~ }
		void FixedUpdate(){
			RegenerateLife();
		}
		
		private void RegenerateLife(){
			if(!regenLife) return;
			
			if(lifeRegen==0 || (CapLife() && life>=lifeCap)) return;
		
			lifeRegenCached+=lifeRegen*lifeRegenMul*Time.fixedDeltaTime;
			if(lifeRegenCached>1){
				int gain=(int)Mathf.Floor(lifeRegenCached);
				GainLife(gain);
				lifeRegenCached-=gain;
			}
		}
		
		
		[Header("Level Management")]
		public string nextLevelName="";
		public string mainMenuName="";
		
		public static void RestartLevel(){
			Debug.Log("Restart level");
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			Time.timeScale=1;
		}
		public static void NextLevel(){
			Debug.Log("load next level");
			SceneManager.LoadScene(instance.nextLevelName);
			Time.timeScale=1;
		}
		public static void MainMenu(){
			Debug.Log("load main menu");
			SceneManager.LoadScene(instance.mainMenuName);
			Time.timeScale=1;
		}
		
		
		
		public static void InvalidAction(string msg){
			TDTK.PopupMessage(msg);
			AudioManager.OnInvalidAction();
		}
		
	}

}