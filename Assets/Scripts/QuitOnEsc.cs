using UnityEngine;

public class QuitOnEsc : MonoBehaviour {

	public string OnQuitLevelName;
	public string finalQuitURL = "http://google.com";
	
	
	
	// Update is called once per frame
	void Update () {

		
		// Test for exit
		if (UnityEngine.Input.GetKeyDown (KeyCode.Escape)) {
			if (OnQuitLevelName != null && OnQuitLevelName != ""){
				Application.LoadLevel(OnQuitLevelName);
			}
			else{
				Quit();
			}
		}
	}
	
	//#if UNITY_WEBPLAYER
	//#endif
	public void Quit()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_WEBPLAYER
		if (finalQuitURL != ""){
			Application.OpenURL(finalQuitURL);
		}
		#else
		if (finalQuitURL != ""){
			Application.OpenURL(finalQuitURL);
		}
		Application.Quit();
		#endif
	}
}
