using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class LevelSelectPanel : MonoBehaviour
{
	public int screenWidth;
	public int screenHeight;
	public int numRows = 7;
	public int numCols = 2;
	
	public GameObject levelSelectButtonPrefab;
	
	void Start(){
		CreateButtons();
		CalcBestFit();
		CalcFixedFontSize();
	}
	
	void CreateButtons(){
		int numScenes = ReadSceneNames.singleton.scenes.Count();
		
		float colWidth = 1f / numCols;
		float rowWidth = 1f / numRows;
		
		int count = 0;
		for (int x = 0; x < numCols; ++x){
			for (int y = numRows-1; y >= 0; --y){
				if (count >= numScenes-2) return;
				
				string sceneName = ReadSceneNames.singleton.scenes[count+1];
				GameObject newButton = GameObject.Instantiate(levelSelectButtonPrefab);
				newButton.transform.SetParent(transform);
				RectTransform rectTransform = newButton.GetComponent<RectTransform>();
				rectTransform.anchorMin = new Vector2(x * colWidth, y * rowWidth);
				rectTransform.anchorMax = new Vector2((x+1) * colWidth, (y+1) * rowWidth);
				rectTransform.offsetMin = new Vector2(2, 2);
				rectTransform.offsetMax = new Vector2(-2, -2);
				GameObject textGO = newButton.transform.GetChild(0).GetChild(0).gameObject;
				textGO.GetComponent<Text>().text = CreateDisplayName (count+1, sceneName);
				
				// Need to make a local variable so that each new deleage gets a new instance
				int levelNumber = count+1;
				newButton.GetComponent<Button>().onClick.AddListener(delegate{LoadLevel(levelNumber);});
				
				count++;
				
			}
		}
	}
	
	string CreateDisplayName(int levelNum, string rawName){
		int realNameIndex = rawName.IndexOf(" ") + 1;
		return levelNum.ToString("D2") + ". " + rawName.Substring(realNameIndex);
	}
	
	public void LoadLevel(int levelNum){
		GoogleAnalytics.Client.SendEventHit("gameFlow", "selectLevel", levelNum.ToString ());
		GoogleAnalytics.Client.SendScreenHit("selectLevel");		
		GameMode.gameStartTime = Time.time;
		
		Application.LoadLevel(levelNum);
		
	}
	
	void CalcBestFit(){
		foreach (Transform child in transform){
			GameObject textGO = child.GetChild(0).GetChild(0).gameObject;
			// If we are not using best fit, then nothing to do
			textGO.GetComponent<Text>().resizeTextForBestFit = true;
			textGO.GetComponent<Text>().cachedTextGenerator.Invalidate();
			Rect rect = textGO.GetComponent<RectTransform>().rect;
			Vector2 extents = new Vector2(rect.width, rect.height);
			TextGenerationSettings settings = textGO.GetComponent<Text>().GetGenerationSettings(extents);
			string text = textGO.GetComponent<Text>().text;
			textGO.GetComponent<Text>().cachedTextGenerator.Populate(text, settings);
		}
	}
	
	void CalcFixedFontSize(){
		int minFontSize = 999;
		foreach (Transform child in transform){
			GameObject textGO = child.GetChild(0).GetChild(0).gameObject;
			
			// Record the size of the text - need to wait till it has rendered before we do this.
			int fontSize = textGO.GetComponent<Text>().cachedTextGenerator.fontSizeUsedForBestFit;
			minFontSize = Mathf.Min (minFontSize, fontSize);
		}
		// Now we have a min font size, we can apply it to all elements
		if (minFontSize != 999){
			foreach (Transform child in transform){
				GameObject textGO = child.GetChild(0).GetChild(0).gameObject;
				textGO.GetComponent<Text>().fontSize = minFontSize;
				textGO.GetComponent<Text>().resizeTextForBestFit = false;
			}
		}
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		
	}
	
	void Update(){
	
		if (Screen.width != screenWidth || Screen.height != screenHeight){
			CalcBestFit();
			CalcFixedFontSize();
		}

		
	}


}