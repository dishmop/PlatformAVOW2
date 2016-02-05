using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkipScript : MonoBehaviour {

	float timeTillShow = 120;
	float interval = 10;
	float showDuration = 5;
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
	
	
	// Use this for initialization
	void Start () {
		lastTimeStamp = Time.time + timeTillShow - interval;
		transform.FindChild ("Message").GetComponent<Text>().color = new Color(1, 1, 1, 0);
	
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
				transform.FindChild ("Message").GetComponent<Text>().color = Color.Lerp(new Color(1, 1, 1, 0), Color.white,fadeVal);
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
				transform.FindChild ("Message").GetComponent<Text>().color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), fadeVal);
				if (fadeVal > 1){
					lastTimeStamp = Time.time;
					state = State.kWaiting;
				}
				break;
				
			}
		}
		if (Input.GetKeyDown(KeyCode.M) && Time.timeScale != 0){	
			transform.FindChild("ConfirmPanel").gameObject.SetActive(true);
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
		transform.FindChild("ConfirmPanel").gameObject.SetActive(false);
		Cursor.visible = cursorViz;
		Time.timeScale = timeScale;
		
	}
	
	
}
