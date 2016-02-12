using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkipScript : MonoBehaviour {

	float timeTillShow = 60 * 5;
	float interval = 60;
	float showDuration = 10;
	float lastTimeStamp = 0;
	float fadeTime = 1;
	
	bool cursorViz;
	float timeScale;
	
	
	enum State{
		kWaiting,
		kFadeIn,
		kShow,
		kFadeOut
	};
	State state = State.kWaiting;
	GameObject confirmPanelGO;
	GameObject messageGO;
	GameObject messagePanelGO;
	
	
	// Use this for initialization
	void Start () {
		lastTimeStamp = Time.time + timeTillShow - interval;
		messagePanelGO = transform.FindChild ("Panel").gameObject;
		messageGO = messagePanelGO.transform.FindChild ("Message").gameObject;
		
		SetSkipMessageAlpha(0);
		
		confirmPanelGO = transform.FindChild("ConfirmPanel").gameObject;
		confirmPanelGO.SetActive(false);
	}
	
	void SetSkipMessageAlpha(float alpha){
		messagePanelGO.GetComponent<Image>().color = SetAlpha(messagePanelGO.GetComponent<Image>().color, alpha);
		messageGO.GetComponent<Text>().color = SetAlpha(messageGO.GetComponent<Text>().color, alpha);
	}
	
	Color SetAlpha (Color col, float alpha){
		col.a = alpha;
		return col;
	}
	
	// Update is called once per frame
	void Update () {
		switch (state){
			case State.kWaiting:
			{
				if (Time.time > lastTimeStamp + interval){
					lastTimeStamp = Time.time;
					state = State.kFadeIn;
				}
				break;
			}
			case State.kFadeIn:
			{
				float fadeVal = (Time.time - lastTimeStamp) / fadeTime;
				SetSkipMessageAlpha(Mathf.Lerp(0, 0.75f, fadeVal));
				if (fadeVal > 1){
					lastTimeStamp = Time.time;
					state = State.kShow;
				}
				break;
				
			}
			case State.kShow:
			{
				if (Time.time  > lastTimeStamp + showDuration){
					lastTimeStamp = Time.time;
					state = State.kFadeOut;
				}
				break;
			}
			case State.kFadeOut:
			{
				float fadeVal = (Time.time - lastTimeStamp) / fadeTime;
				SetSkipMessageAlpha(Mathf.Lerp(0.75f, 0, fadeVal));
				if (fadeVal > 1){
					lastTimeStamp = Time.time;
					state = State.kWaiting;
				}
				break;
				
			}
		}
		if (Input.GetKeyDown(KeyCode.M) && Time.timeScale != 0){	
			confirmPanelGO.SetActive(true);
			cursorViz = Cursor.visible;
			timeScale = Time.timeScale;
			Cursor.visible = true;
			Time.timeScale = 0;
			
			
		}
	}
	
	public void DoSkip(){
		Application.LoadLevel(Application.loadedLevel + 1);
		Cursor.visible = cursorViz;
		Time.timeScale = timeScale;
		
	}
	
	public void DontSkip(){
		confirmPanelGO.gameObject.SetActive(false);
		Cursor.visible = cursorViz;
		Time.timeScale = timeScale;
		
	}
	
	
}
