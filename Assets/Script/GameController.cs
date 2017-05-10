using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	List<bool[,]> list;

	public delegate List<bool[,]> mapEventHandler(List<bool[,]> list);
	public delegate Transform blockMoveEventHandler(Transform block);
	public delegate Vector3 positionEventHandler(Vector3 position);
	public static event mapEventHandler OnSendMap;
	public static event blockMoveEventHandler OnSendBlockMove;
	
	// Use this for initialization
	void Start () {
		mapManager.OnSendMap += getMap;
		PlayerController.OnSendBlockMove += getBlockMove;
		PlayerController.OnSendPlayerPosition += getPlayerPosition;
		PlayerController.OnSendCameraPosition += getCameraPosition;
	}
	void Destoryed(){
		mapManager.OnSendMap -= getMap;
		PlayerController.OnSendBlockMove -= getBlockMove;
		PlayerController.OnSendPlayerPosition -= getPlayerPosition;
		PlayerController.OnSendCameraPosition -= getCameraPosition;
	}
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.W))
			sendMap(list);
	}

	private void sendMap(List<bool[,]> list){
		if(OnSendMap != null){
			OnSendMap(list);
			print("sendMap");
		}
	}
	private void sendBlockMove(Transform block){
		if(OnSendBlockMove != null){
			OnSendBlockMove(block);
			print("sendBlockMove");
		}
	}
	private List<bool[,]> getMap(List<bool[,]> list){
		Debug.Log("getMap");
		return list;
	}
	private Transform getBlockMove(Transform block){
		Debug.Log("getBlockMove");
		return block;
	}
	private Vector3 getPlayerPosition(Vector3 position){
		Debug.Log("getPlayerPosition");
		return position;
	}
	private Vector3 getCameraPosition(Vector3 position){
		Debug.Log("getCameraPosition");
		return position;
	}

}
