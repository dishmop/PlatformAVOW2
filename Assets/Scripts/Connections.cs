using UnityEngine;
using System.Collections;

public class Connections : MonoBehaviour {


	[System.Serializable]
	public struct ConnectionData{
		public GameObject wire;
		public Vector3 pos;
		public int dir;
		
	};
	
	public ConnectionData[] connectionData;
	
	public void GetConnectionData(GameObject wire, out int dir, out Vector3 pos){
		foreach (ConnectionData data in connectionData){
			if (data.wire == wire){
				dir = data.dir;
				pos = data.pos + transform.position;
				return;
			}
		}
		dir = Directions.kNull;
		pos = Vector3.zero;
		
	}
	

}
