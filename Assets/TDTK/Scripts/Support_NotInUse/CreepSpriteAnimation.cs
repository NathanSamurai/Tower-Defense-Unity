using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTK{

	public class CreepSpriteAnimation : MonoBehaviour {
		
		private UnitCreep unit;
		
		public bool use2DSprite=false;
		public List<Sprite> spriteList_E=new List<Sprite>();
		public List<Sprite> spriteList_NE=new List<Sprite>();
		public List<Sprite> spriteList_N=new List<Sprite>();
		public List<Sprite> spriteList_NW=new List<Sprite>();
		public List<Sprite> spriteList_W=new List<Sprite>();
		public List<Sprite> spriteList_SW=new List<Sprite>();
		public List<Sprite> spriteList_S=new List<Sprite>();
		public List<Sprite> spriteList_SE=new List<Sprite>();
		
		public SpriteRenderer spriteRenderer;
		public float spriteFrameRate=10;
		private float spriteCD=0;
		private int spriteIdx=0;
		
		private enum _2DDir{ E, NE, N, NW, W, SW, S, SE}
		private _2DDir spriteDir;
		
		
		void Awake(){
			unit=gameObject.GetComponent<UnitCreep>();
			if(spriteRenderer==null) spriteRenderer=gameObject.GetComponent<SpriteRenderer>();
		}
		
		void Update(){
			if(spriteRenderer==null || unit==null) return;
			
			UpdateMoveAngle();
			PlaySprite();
		}
		
		
		public void UpdateMoveAngle(){
			float angle=unit.GetMoveAngle();
			
			/*	//isometric	4-axis
			if(angle>0 && angle<=90) 		spriteDir=_2DDir.NE;
			else if(angle>90 && angle<=180) 	spriteDir=_2DDir.SW;
			else if(angle>180 && angle<=270) spriteDir=_2DDir.SW;
			else 								spriteDir=_2DDir.NE;
			*/
			
			/*	//top down	4-axis
			if(angle>45 && angle<=135)		spriteDir=_2DDir.E;
			else if(angle>135 && angle<=225)spriteDir=_2DDir.S;
			else if(angle>270 && angle<=360)spriteDir=_2DDir.W;
			else 								spriteDir=_2DDir.N;
			*/
			
			//top down	8-axis
			if(angle>22.5f && angle<=67.5f)			spriteDir=_2DDir.NE;
			else if(angle>67.5f && angle<=112.5f)	spriteDir=_2DDir.E;
			else if(angle>112.5f && angle<=157.5f) spriteDir=_2DDir.SE;
			else if(angle>157.5f && angle<=202.5f) spriteDir=_2DDir.S;
			else if(angle>202.5f && angle<=247.5f) spriteDir=_2DDir.SW;
			else if(angle>247.5f && angle<=292.5f) spriteDir=_2DDir.W;
			else if(angle>292.5f && angle<=337.5f) spriteDir=_2DDir.NE;
			else 									 				spriteDir=_2DDir.N;
		}
		
		void PlaySprite(){
			spriteCD-=Time.deltaTime;
			if(spriteCD<=0){
				spriteCD=1/spriteFrameRate;
				spriteIdx=(spriteIdx+1)%GetSpriteIdxLimit();
				spriteRenderer.sprite=GetSprite(spriteIdx);
			}
		}
		
		public int GetSpriteIdxLimit(){
			if(spriteDir==_2DDir.E) 	return spriteList_E.Count;
			if(spriteDir==_2DDir.NE) 	return spriteList_NE.Count;
			if(spriteDir==_2DDir.N) 	return spriteList_N.Count;
			if(spriteDir==_2DDir.NW)	return spriteList_NW.Count;
			if(spriteDir==_2DDir.W) 	return spriteList_W.Count;
			if(spriteDir==_2DDir.SW)	return spriteList_SW.Count;
			if(spriteDir==_2DDir.S) 	return spriteList_S.Count;
			if(spriteDir==_2DDir.SE) 	return spriteList_SE.Count;
			return 0;
		}
		
		public Sprite GetSprite(int idx){
			if(spriteDir==_2DDir.E &&	 spriteList_E.Count>idx) 	return spriteList_E[idx];
			if(spriteDir==_2DDir.NE && 	spriteList_NE.Count>idx) return spriteList_NE[idx];
			if(spriteDir==_2DDir.N && 	spriteList_N.Count>idx) 	return spriteList_N[idx];
			if(spriteDir==_2DDir.NW && spriteList_NW.Count>idx)return spriteList_NW[idx];
			if(spriteDir==_2DDir.W && 	spriteList_W.Count>idx) 	return spriteList_W[idx];
			if(spriteDir==_2DDir.SW && spriteList_SW.Count>idx)return spriteList_SW[idx];
			if(spriteDir==_2DDir.S && 	spriteList_S.Count>idx) 	return spriteList_S[idx];
			if(spriteDir==_2DDir.SE && spriteList_SE.Count>idx) 	return spriteList_SE[idx];
			return null;
		}
		
	}

}