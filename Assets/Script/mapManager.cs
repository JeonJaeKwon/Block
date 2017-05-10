using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapManager : MonoBehaviour {

	List<bool[,]> list;


	public delegate List<bool[,]> mapEventHandler(List<bool[,]> list);
	public static event mapEventHandler OnSendMap;

	void Start () {
		GameController.OnSendMap += getMap;
		GameController.OnSendBlockMove += getBlockMove;
	}
	void Destoryed(){
		GameController.OnSendMap -= getMap;
		GameController.OnSendBlockMove -= getBlockMove; 
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	private void sendMap(List<bool[,]> list){
		if(OnSendMap != null){
			OnSendMap(list);
			Debug.Log("sendMap");
		}
	}
	private List<bool[,]> getMap(List<bool[,]> list){
		Debug.Log("getMap");
		sendMap(list);
		return list;
	}
	private Transform getBlockMove(Transform block){
		Debug.Log("getBlockMove");
		return block;
	}
}
