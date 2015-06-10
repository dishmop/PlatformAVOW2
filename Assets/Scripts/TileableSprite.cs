using UnityEngine;
using System.Collections;


public class TileableSprite : MonoBehaviour {

	public GameObject tiledSpritePrefab;
	public float tileSize = 1;
	public bool continuousUpdate = false;
	
	void SetupTiles(){
		Vector3 thisScale = transform.localScale;
		Vector3 offset = -thisScale * 0.5f;
		
		for (int i = 0; i < thisScale.x; i++){
			for (int j = 0; j < thisScale.y; j++){
				float xTileSize = tileSize/thisScale.x;
				float yTileSize = tileSize/thisScale.y;
				GameObject newSprite = GameObject.Instantiate(tiledSpritePrefab);
				newSprite.transform.parent = transform;
				newSprite.transform.localPosition = new Vector3((offset.x + i + 0.5f) * xTileSize,  (offset.y + j + 0.5f) * yTileSize);
				newSprite.transform.localScale = new Vector3(xTileSize, yTileSize, 1);
			}
		}
	}
	
	void ClearTiles(){
		foreach (Transform child in transform){
			Destroy (child.gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		GetComponent<SpriteRenderer>().enabled = false;
		SetupTiles();

	
	
	}
	
	// Update is called once per frame
	void Update () {
		if (continuousUpdate){
			ClearTiles();
			SetupTiles();
		}
	
	}
}
