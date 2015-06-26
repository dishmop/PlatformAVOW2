using UnityEngine;

public class DebugUtils{

	public static void Assert(bool condition){
		if (!condition){
			Debug.LogError("Generic Assert");
		}
	}
	
	public static void Assert(bool condition, Object context){
		if (!condition){
			Debug.LogError("Generic Assert", context);
		}
	}

	public static void Assert(bool condition, string msg){
		if (!condition){
//			 Debug.LogError(msg);
			}
	}

	public static void Assert(bool condition, string msg, Object context){
		if (!condition){
			Debug.LogError(msg, context);
		}
	}
}
