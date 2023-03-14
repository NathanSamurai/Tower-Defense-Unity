using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TDTK;

namespace TDTK{

	public class TDTK : MonoBehaviour{
		
		public static int GetLayerTerrain(){ return 31; }
		public static int GetLayerPath(){ return 30; }
		public static int GetLayerPlatform(){ return 29; }
		public static int GetLayerTower(){ return 28; }
		public static int GetLayerCreep(){ return 27; }
		public static int GetLayerObstacle(){ return 26; }
		public static int GetLayerNoBuild(){ return 25; }
		public static int GetLayerPlatformHeightOffset(){ return 22; }
		
		
		public static Vector3 GetWorldScale(Transform transform){
			Vector3 worldScale = transform.localScale;
			Transform parent = transform.parent;
			
			while (parent != null){
				worldScale = Vector3.Scale(worldScale,parent.localScale);
				parent = parent.parent;
			}
			
			return worldScale;
		}
		
		
		
		
		
		
		public delegate void PopupMessageHandler(string msg);
		public static event PopupMessageHandler onPopupMessageE;			
		public static void PopupMessage(string msg){ if(onPopupMessageE!=null) onPopupMessageE(msg); }
		
		public delegate void textOverlayHandler(string msg, Vector3 pos);
		public static event textOverlayHandler onTextOverlayE;			
		public static void TextOverlay(string msg, Vector3 pos){ if(onTextOverlayE!=null) onTextOverlayE(msg, pos); }
		
		
		//call when game is over
		public delegate void GameOverHandler(bool playerWon);
		public static event GameOverHandler onGameOverE;			
		public static void OnGameOver(bool playerWon){ if(onGameOverE!=null) onGameOverE(playerWon); }
		
		
		public delegate void RscChangedHandler(List<int> rsclist);
		public static event RscChangedHandler onRscChangedE;			
		public static void OnRscChanged(List<int> rsclist){ if(onRscChangedE!=null) onRscChangedE(rsclist); }
		
		public delegate void LifeChangedHandler(int life);
		public static event LifeChangedHandler onLifeChangedE;			
		public static void OnLifeChanged(int life){ if(onLifeChangedE!=null) onLifeChangedE(life); }
		
		public delegate void AbilityRscChangedHandler(int rsc);
		public static event AbilityRscChangedHandler onAbilityRscChangedE;			
		public static void OnAbilityRscChanged(int rsc){ if(onAbilityRscChangedE!=null) onAbilityRscChangedE(rsc); }
		
		public delegate void PerkRscChangedHandler(int rsc);
		public static event PerkRscChangedHandler onPerkRscChangedE;			
		public static void OnPerkRscChanged(int rsc){ if(onPerkRscChangedE!=null) onPerkRscChangedE(rsc); }
		
		
		
		//call to indicate SpawnManager is ready to spawn next wave 
		public delegate void EnableSpawnHandler();
		public static event EnableSpawnHandler onEnableSpawnE;	
		public static void OnEnableSpawn(){ if(onEnableSpawnE!=null) onEnableSpawnE(); }
		
		//call to a new wave has been spawn
		public delegate void NewWaveHandler(int wave);
		public static event NewWaveHandler onNewWaveE;	
		public static void OnNewWave(int wave){ if(onNewWaveE!=null) onNewWaveE(wave); }
		
		//call when count down to next spawn started
		public delegate void SpawnCountDownHandler();
		public static event SpawnCountDownHandler onSpawnCountDownE;		
		public static void OnSpawnCountDown(){ if(onSpawnCountDownE!=null) onSpawnCountDownE(); }
		
		
		//called when a new unit has been added to the game
		public delegate void NewUnitHandler(Unit unit);
		public static event NewUnitHandler onNewUnitE;
		public static void OnNewUnit(Unit unit){ if(onNewUnitE!=null) onNewUnitE(unit); }
		
		
		//called when the tower start constructing 
		public delegate void OnTowerConstructHandler(UnitTower tower);
		public static event OnTowerConstructHandler onTowerConstructingE;	
		public static void OnTowerConstructing(UnitTower tower){ if(onTowerConstructingE!=null) onTowerConstructingE(tower); }
		
		
		//called when a new tower instance has been added to the scene, or when a tower has been removed the scene
		public delegate void NewTowerHandler(UnitTower tower);
		public static event NewTowerHandler onNewTowerE;
		public static void OnNewTower(UnitTower tower){ if(onNewTowerE!=null) onNewTowerE(tower); }
		
		//called when a new buildable tower has been added to the TowerManager
		public delegate void NewBuildableHandler(UnitTower tower);
		public static event NewBuildableHandler onNewBuildableE;
		public static void OnNewBuildable(UnitTower tower){ if(onNewBuildableE!=null) onNewBuildableE(tower); }
		
		//called when a buildable tower is replace by other tower
		public delegate void ReplaceBuildableHandler(int idx, UnitTower tower);
		public static event ReplaceBuildableHandler onReplaceBuildableE;
		public static void OnReplaceBuildable(int idx, UnitTower tower){ if(onReplaceBuildableE!=null) onReplaceBuildableE(idx, tower); }
		
		
		//called when a new buidlable ability has been added to the AbilityManager
		public delegate void NewAbilityHandler(Ability ability);
		public static event NewAbilityHandler onNewAbilityE;
		public static void OnNewAbility(Ability ability){ if(onNewAbilityE!=null) onNewAbilityE(ability); }
		
		public delegate void ActivateAbilityHandler(Ability ability);
		public static event ActivateAbilityHandler onActivateAbilityE;
		public static void OnActivateAbility(Ability ability){ if(onActivateAbilityE!=null) onActivateAbilityE(ability); }
		
	}

}