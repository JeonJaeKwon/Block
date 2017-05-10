using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManager : MonoBehaviour {

public delegate Vector3 positionEventHandler(Vector3 position);
public static event positionEventHandler OnSendCameraPosition;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	private void sendCameraPosition(Vector3 position){
		if(OnSendCameraPosition != null){
			OnSendCameraPosition(position);
			print("sendCameraPosition");
		}
	}
}
