using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{

	public class AudioManager : MonoBehaviour {

		
		//public AK.Wwise.Event lifeLostW;
		//public static GameObject lifeLostSource;

		public static lostLifeScript lostLifeScriptReference;
		
		private List<AudioSource> audioSourceList=new List<AudioSource>();
		private List<AudioSource> audioSourceList_UI=new List<AudioSource>();
		
		private static AudioManager instance;

		
		//~ void Awake(){
			//~ if(instance!=null){
				//~ Destroy(gameObject);
				//~ return;
			//~ }
			
			//~ instance=this;
			
			//~ //DontDestroyOnLoad(gameObject);
			
			//~ //listener=gameObject.GetComponent<AudioListener>();
			
			
		//~ }
		
		public Transform cameraT;
		
		public void Awake(){
			if(instance!=null) return;
			instance=this;
			
			AudioListener.volume=GetVolumeSFX();
			
			CreateAudioSource();
			
			cameraT=Camera.main.transform;
			lostLifeScriptReference = gameObject.GetComponent<lostLifeScript>();
		}
		
		
		
		public const string vol_SaveString_SFX="TDTK_Volume_SFX";
		public const string vol_SaveString_UI="TDTK_Volume_UI";
		
		public static float GetVolumeSFX(){
			return PlayerPrefs.GetFloat(vol_SaveString_SFX, 1f);
		}
		public static void SetVolumeSFX(float value){
			AudioListener.volume=value;
			PlayerPrefs.SetFloat(vol_SaveString_SFX, value);
		}
		
		public static float GetVolumeUI(){
			return PlayerPrefs.GetFloat(vol_SaveString_UI, 1f);
		}
		public static void SetVolumeUI(float value){
			if(instance!=null){
				for(int i=0; i<instance.audioSourceList_UI.Count; i++) instance.audioSourceList_UI[i].volume=value;
			}
			PlayerPrefs.SetFloat(vol_SaveString_UI, value);
		}
		
		
		
		/*
		public static AudioManager Init(){
			//if(instance!=null) return instance;
			
			AudioManager instance = (AudioManager)FindObjectOfType(typeof(AudioManager));
			if(instance==null){
				GameObject obj=new GameObject();
				obj.name="AudioManager";
				
				instance=obj.AddComponent<AudioManager>();
			}
			
			return instance;
		}
		*/
		
		
		//private bool init=false;
		void CreateAudioSource(){
			audioSourceList=new List<AudioSource>();
			for(int i=0; i<15; i++){
				GameObject obj=new GameObject("AudioSource"+(i+1));
				
				AudioSource src=obj.AddComponent<AudioSource>();
				src.playOnAwake=false; src.loop=false; src.volume=1; //src.spatialBlend=.75f;
				obj.transform.parent=transform; obj.transform.localPosition=Vector3.zero;
				
				audioSourceList.Add(src);
			}
			
			audioSourceList_UI=new List<AudioSource>();
			for(int i=0; i<8; i++){
				GameObject obj=new GameObject("AudioSource_UI_"+(i+1));
				
				AudioSource src=obj.AddComponent<AudioSource>();
				src.playOnAwake=false; src.loop=false; src.volume=GetVolumeUI(); //src.spatialBlend=.75f;
				obj.transform.parent=transform; obj.transform.localPosition=Vector3.zero;
				
				audioSourceList_UI.Add(src);
			}
		}
		
		
		//call to play a specific clip
		public static void PlaySoundW(AK.Wwise.Event toPlay, GameObject org){
			toPlay.Post(org);
		}

		public static void PlaySound(AudioClip clip, Vector3 pos=default(Vector3)){ if(instance!=null) instance._PlaySound(clip, pos); }
		public void _PlaySound(AudioClip clip, Vector3 pos=default(Vector3)){
			if(clip==null) return;
			int Idx=GetUnusedAudioSourceIdx();
			audioSourceList[Idx].transform.position=pos;
			audioSourceList[Idx].clip=clip;		audioSourceList[Idx].Play();
		}
		
		//check for the next free, unused audioObject
		private int GetUnusedAudioSourceIdx(){
			for(int i=0; i<audioSourceList.Count; i++){ if(!audioSourceList[i].isPlaying) return i; }
			return 0;	//if everything is used up, use item number zero
		}
		
		
		
		public static void PlayUISound(AudioClip clip){ if(instance!=null) instance._PlayUISound(clip); }
		public void _PlayUISound(AudioClip clip){
			if(clip==null) return;
			int Idx=GetUnusedUIAudioSourceIdx();
			audioSourceList_UI[Idx].transform.position=cameraT.position;
			audioSourceList_UI[Idx].clip=clip;		audioSourceList_UI[Idx].Play();
		}
		
		private int GetUnusedUIAudioSourceIdx(){
			for(int i=0; i<audioSourceList_UI.Count; i++){ if(!audioSourceList_UI[i].isPlaying) return i; }
			return 0;
		}
		
		
		
		
		[Header("Sound Effect")]
		public AudioClip playerWon;
		public static void OnPlayerWon(){ 
			if(instance!=null)
			{ 
				lostLifeScriptReference.playerWonW.Post(lostLifeScriptReference.lifeLostSource);
			}
		}
		
		
		
		public AudioClip playerLost;
		public static void OnPlayerLost(){ 
			if(instance!=null)
			{
			  	
				lostLifeScriptReference.PlayerLostW.Post(lostLifeScriptReference.lifeLostSource);
			}
		}
		
		public AudioClip lostLife;
		
		
		public static void OnLostLife(){ 
			if(instance!=null){
				lostLifeScriptReference.lifeLostW.Post(lostLifeScriptReference.lifeLostSource);
				//MyUIButtonFunction.instance.HPLose();
			}
			
		}
		
		public AudioClip newWave;
		public AudioClip waveCleared;
		public static void OnNewWave(){ if(instance!=null && instance.newWave!=null) PlayUISound(instance.newWave); }
		public static void OnWaveCleared(){ if(instance!=null && instance.waveCleared!=null) PlayUISound(instance.waveCleared); }
		
		
		public AudioClip buildStart;
		public AudioClip buildComplete;
		public static void OnBuildStart(){ if(instance!=null && instance.buildStart!=null) PlayUISound(instance.buildStart); }
		public static void OnBuildComplete(){ if(instance!=null && instance.buildComplete!=null) PlayUISound(instance.buildComplete); }
		
		public AudioClip upgradeStart;
		public AudioClip upgradeComplete;
		public static void OnUpgradeStart(){ if(instance!=null && instance.upgradeStart!=null) PlayUISound(instance.upgradeStart); }
		public static void OnUpgradeComplete(){ if(instance!=null && instance.upgradeComplete!=null) PlayUISound(instance.upgradeComplete); }
		
		public AudioClip towerSold;
		public static void OnTowerSold(){ 
			if(instance!=null && instance.towerSold!=null) {
				PlayUISound(instance.towerSold); 
				
			}
		}
		
		
		public AudioClip perkPurchased;
		public static void OnPerkPurchased(){ if(instance!=null && instance.perkPurchased!=null) PlayUISound(instance.perkPurchased); }
		
		
		public AudioClip invalidAction;
		public static void OnInvalidAction(){ if(instance!=null && instance.invalidAction!=null) PlayUISound(instance.invalidAction); }
		
		
	}

}