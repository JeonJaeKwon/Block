using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameController.OnSendMap += getMap;
	}
	void Destoryed(){
		GameController.OnSendMap -= getMap;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	private List<bool[,]> getMap(List<bool[,]> list){
		Debug.Log("getMap");
		return list;
	}
}
