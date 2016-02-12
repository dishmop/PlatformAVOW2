
// This is a modification of a file posted by "Bunny83" - available here:
// http://answers.unity3d.com/questions/33263/how-to-get-names-of-all-available-levels.html
// 
// However, this version works even in non editor builds (but you must have run it in the editor at some point first). 
// 
// Usage
// ------
// Place this component in one game object somewhere in your scene.
// When it starts up in the editor, it will fill the "scenes" array with the names of all your scenes in the build settings
// it will also save them as a binary file in your Resources folder in assets
//
// When you start this up in non editor mode it will load in the file from your resources folder (this uses the resource managemnet system
// Rather than the raw file IO so should also work on web player builds).., and fills the scene array that way
//
// You can force these functions to be called from the editor by right clicking the component in the inspector
//
// To access the filenames from anywhere in your code, use somethign like
// string sceneName = ReadSceneNames.singleton.scenes[2];



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


public class ReadSceneNames : MonoBehaviour
{
	public static ReadSceneNames singleton = null;
	public string sceneFilename = "LevelNames";
	public string[] scenes;
	
	
#if UNITY_EDITOR

	void Start(){
		Reset ();
	}

#else

	void Start(){
		LoadLevelNames();
	}
	
#endif

	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton - Have you got two components of this type in the scene?");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}	

	
	string BuildLoadFilename(){
		return "LevelNames/" + sceneFilename;
	}
	
	
	void LoadLevelNames(){
		TextAsset levelNamesAsset = Resources.Load(BuildLoadFilename()) as TextAsset;
		if (levelNamesAsset != null){
			Stream s = new MemoryStream(levelNamesAsset.bytes);
			BinaryReader br = new BinaryReader(s);
			int numLevels = br.ReadInt32();
			if (numLevels > 0){
				scenes = new string[numLevels];
				for (int i = 0; i < numLevels; ++i){
					scenes[i] = br.ReadString();
				}
			}
		}
	}
		
	
#if UNITY_EDITOR	
	private static string[] ReadNames()
	{
		List<string> temp = new List<string>();
		foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
		{
			if (S.enabled)
			{
				string name = S.path.Substring(S.path.LastIndexOf('/')+1);
				name = name.Substring(0,name.Length-6);
				temp.Add(name);
			}
		}
		return temp.ToArray();
	}
	
	[UnityEditor.MenuItem("CONTEXT/ReadSceneNames/Update Scene Names")]
	private static void UpdateNames(UnityEditor.MenuCommand command)
	{
		ReadSceneNames context = (ReadSceneNames)command.context;
		context.scenes = ReadNames();
		context.SaveNameFile();
		
	}
	
	[UnityEditor.MenuItem("CONTEXT/ReadSceneNames/Force Reload")]
	private static void ForceReload(UnityEditor.MenuCommand command)
	{
		ReadSceneNames context = (ReadSceneNames)command.context;
		context.LoadLevelNames();
		
	}
	
	
	private void Reset()
	{
		scenes = ReadNames();
		SaveNameFile();
	}

	void CreateResourceDirs(){
		if (!System.IO.Directory.Exists(Application.dataPath + "/Resources")){
			System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources");
		}
		if (!System.IO.Directory.Exists(Application.dataPath + "/Resources/LevelNames")){
			System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/LevelNames");
		}
	}
	
	string BuildSaveFilename(){
		return Application.dataPath + "/Resources/LevelNames/" + sceneFilename + ".bytes";
	}
		
	void SaveNameFile(){
		CreateResourceDirs();
		Debug.Log ("Saving level names..." + BuildSaveFilename());
		
		FileStream file = File.Create(BuildSaveFilename());
		BinaryWriter bw = new BinaryWriter(file);
		
		bw.Write(scenes.Count ());
		for (int i = 0; i < scenes.Count(); ++i){
			bw.Write(scenes[i]);
		}
		
		bw.Close();
		file.Close();
		
		// Ensure the assets are all realoaded and the cache cleared.
		UnityEditor.AssetDatabase.Refresh();
	}
	
	
	#endif
}