using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class LevelSelectPanel : MonoBehaviour
{

	public int numRows = 7;
	public int numCols = 2;
	
	public GameObject levelSelectButtonPrefab;
	
	void Start(){
		int numScenes = ReadSceneNames.singleton.scenes.Count();
		
		float colWidth = 1f / numCols;
		float rowWidth = 1f / numRows;
		
		int count = 0;
		for (int x = 0; x < numCols; ++x){
			for (int y = 0; y < numRows; ++y){
				if (count >= numScenes-2) return;
				
				string sceneName = ReadSceneNames.singleton.scenes[count+1];
				GameObject newButton = GameObject.Instantiate(levelSelectButtonPrefab);
				newButton.transform.SetParent(transform);
				RectTransform rectTransform = newButton.GetComponent<RectTransform>();
				rectTransform.anchorMin = new Vector2(x * colWidth, y * rowWidth);
				rectTransform.anchorMax = new Vector2((x+1) * colWidth, (y+1) * rowWidth);
				rectTransform.offsetMin = Vector2.zero;
				rectTransform.offsetMax = Vector2.zero;
				newButton.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = sceneName;
				
				newButton.GetComponent<Button>().onClick.AddListener(delegate{LoadLevel(sceneName);});
				count++;
				
			}
		}
	}
	
	public void LoadLevel(string name){
		Application.LoadLevel(name);
		
	}


}