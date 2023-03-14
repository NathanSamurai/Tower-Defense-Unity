using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TDTK;

public class Demo_Menu : MonoBehaviour {

	public class LvlInfo{
		public string lvlName="";
		public string lvlDesp="";
	}
	
	public List<UIButton> buttonList=new List<UIButton>();
	
	[Space(5)] 
	public Text lbTooltip;
	public Image imgPreview;
	
	[Space(10)]
	public List<Sprite> previewImgList=new List<Sprite>();
	
	[Space(10)]
	public List<LvlInfo> levelInfoList=new List<LvlInfo>();
	
	
	void Start () {
		for(int i=0; i<buttonList.Count; i++){
			buttonList[i].Init();	int idx=i;
			buttonList[i].button.onClick.AddListener(() => OnButton(idx));
			buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton);
			//buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnButton, null);
		}
		
		LvlInfo entry=new LvlInfo();
		entry.lvlName="Demo_Linear";
		entry.lvlDesp="A simple level with 3 linear paths";
		levelInfoList.Add(entry);
		
		entry=new LvlInfo();
		entry.lvlName="Demo_LinearLoop";
		entry.lvlDesp="A level with single path traverse through a buildable grid, allow player to alter the creep route on the run";
		levelInfoList.Add(entry);
		
		entry=new LvlInfo();
		entry.lvlName="Demo_BranchingPath";
		entry.lvlDesp="A level with branching path, each travel through island(s) of buildable grid.\nThe creep will choose the shortest route to their destination\nThis level uses procedural generation for the incoming creep";
		levelInfoList.Add(entry);
		
		entry=new LvlInfo();
		entry.lvlName="Demo_Platform";
		entry.lvlDesp="A path on top of a single buildable grid of unconventional layout\nThis level is set to endless mode and uses procedural generation for the incoming creep";
		levelInfoList.Add(entry);
		
		OnExitButton(null);
	}
	
	
	public void OnHoverButton(GameObject butObj){
		int idx=GetButtonIndex(butObj);
		
		if(idx<levelInfoList.Count) lbTooltip.text=levelInfoList[idx].lvlDesp;
		
		if(idx<previewImgList.Count){
			imgPreview.sprite=previewImgList[idx];
			imgPreview.gameObject.SetActive(true);
		}
	}
	public void OnExitButton(GameObject butObj){
		lbTooltip.text="";
		imgPreview.gameObject.SetActive(false);
	}
	public void OnButton(int idx){
		if(idx<levelInfoList.Count) SceneManager.LoadScene(levelInfoList[idx].lvlName);
	}
	
	private int GetButtonIndex(GameObject butObj){
		for(int i=0; i<buttonList.Count; i++){
			if(buttonList[i].rootObj==butObj) return i;
		}
		return 0;
	}
	
	
}
