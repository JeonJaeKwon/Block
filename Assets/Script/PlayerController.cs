using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent (typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
	public float grabVelocity = 0.2f;
	public float desendVelocity = 0.2f;
	public float climbVelocity = 0.2f;
	public float hangVelocity = 0.2f;
	public float pullVelocity = 5f;
	public float pushVelocity = 5f;
	public float fallVerocity = 5f;
	public float walkVerocity = 5f;
	public float hangToDesendVerocity = 0.2f;
	public float hangToWalkVerocity = 0.2f;
	public float hangingWalkVerocity = 0.2f;
	public float hangToCrossThisVelocity = 0.02f;
	public float hangToCrossNextToVelocity = 0.02f;
	
	

	public delegate Transform blockMoveEventHandler(Transform block);
	public delegate Vector3 positionEventHandler(Vector3 position);
	public static event blockMoveEventHandler OnSendBlockMove;
	public static event positionEventHandler OnSendPlayerPosition;
	public static event positionEventHandler OnSendCameraPosition;
	void Start () {
		// myRigidbody = GetComponent<Rigidbody> ();
		CamManager.OnSendCameraPosition += getCameraPosition;
	}
	void Destoryed(){
		CamManager.OnSendCameraPosition -= getCameraPosition;
	}

	//event
	private void sendBlockMove(Transform block){
		if(OnSendBlockMove != null){
			OnSendBlockMove(block);
			print("sendBlockMove");
		}
	}
	private void sendPlayerPosition(Vector3 position){
		if(OnSendPlayerPosition != null){
			OnSendPlayerPosition(position);
			print("sendPlayerPosition");
		}
	}
	private void sendCameraPosition(Vector3 position){
		if(OnSendCameraPosition != null){
			OnSendCameraPosition(position);
			print("sendCameraPosition");
		}
	}
	private Vector3 getCameraPosition(Vector3 position){
		Debug.Log("getCameraPosition");
		return position;
	}
	//event end
	public IEnumerator grabAction(Vector3 playerPosition, Vector3 goal){
		float percent = 0;
		while(percent < 1){
			percent += Time.fixedDeltaTime * grabVelocity;
			Vector3 toGoal = Vector3.Lerp(playerPosition, goal, percent * percent) ;
			// myRigidbody.MovePosition(toGoal);
			transform.position = toGoal;
			yield return null;
		}
		yield return null;
	}
	public IEnumerator pushAction(Vector3 playerPosition, Vector3 playerGoal, Vector3 blockPosition, Vector3 blockGoal, Transform pickedUpBlock){
		float percent = 0;
		while(percent < 1){
			percent += Time.fixedTime * pushVelocity;
			Vector3 toGoal = Vector3.Lerp(playerPosition, playerGoal, percent * percent) ;
			// myRigidbody.MovePosition(toGoal);
			transform.position = toGoal;
			Vector3 toGoalBlock = Vector3.Lerp(blockPosition, blockGoal, percent * percent);
			pickedUpBlock.position = toGoalBlock;
			yield return null;
		}
		yield return null;
	}
	public IEnumerator pullAction(Vector3 playerPosition, Vector3 playerGoal, Vector3 blockPosition, Vector3 blockGoal, Transform pickedUpBlock){
		float percent = 0;
		while(percent < 1){
			percent += Time.fixedTime * pullVelocity;
			Vector3 toGoal = Vector3.Lerp(playerPosition, playerGoal, percent * percent) ;
			// myRigidbody.MovePosition(toGoal);
			transform.position = toGoal;
			Vector3 toGoalBlock = Vector3.Lerp(blockPosition, blockGoal, percent * percent);
			pickedUpBlock.position = toGoalBlock;
			yield return null;
		}
		yield return null;
	}
	public IEnumerator desendAction(Vector3 playerPosition, Vector3 goal){
		float percent = 0;
		while(percent < 1){
			percent += Time.fixedTime * desendVelocity;
			Vector3 toGoal = Vector3.Lerp(playerPosition, goal, percent * percent) ;
			// myRigidbody.MovePosition(toGoal);
			transform.position = toGoal;
			yield return null;
		}
		yield return null;
	}
	public IEnumerator climbAction(Vector3 playerPosition, Vector3 goal){
		float percent = 0;
		while(percent < 1){
			percent += Time.fixedDeltaTime * climbVelocity;
			Vector3 toGoal = Vector3.Slerp(playerPosition, goal, percent * percent) ;
			// myRigidbody.MovePosition(toGoal);
			transform.position = toGoal;
			yield return null;
		}
			transform.position = goal;
		yield return null;
	}
	public IEnumerator hangAction(Vector3 playerPosition, Vector3 goal, int rotate){
		float percent = 0;
		transform.rotation = transform.rotation * Quaternion.Euler(0,rotate,0);
		while(percent < 1){
			percent += Time.fixedDeltaTime * hangVelocity;
			Vector3 toGoal = Vector3.Slerp(playerPosition, goal, percent * percent);
			transform.position = toGoal;
			// yield return null;
		}
		yield return null;
	}
	public IEnumerator hangToCrossThisAction(Vector3 playerPosition, Vector3 goal, int rotate){
		float percent = 0;
		Vector3 toGoal = transform.position;
		transform.rotation =transform.rotation * Quaternion.Euler(0,rotate,0);
		while(percent < 1){
			percent += Time.fixedDeltaTime * hangToCrossThisVelocity;
			// if(percent < 1){
				toGoal = Vector3.Lerp(playerPosition, goal, percent);
			// }else{
			// 	toGoal = Vector3.Lerp(playerPosition, goal, percent-1);
			// }
			transform.position = toGoal;

			yield return null;
		}
		yield return null;
	}
	public IEnumerator hangToCrossNextToAction(Vector3 playerPosition, Vector3 goal, int rotate){
		float percent = 0;
		Vector3 toGoal = transform.position;
		transform.rotation =transform.rotation * Quaternion.Euler(0,rotate,0);
		while(percent < 1){
			percent += Time.fixedDeltaTime * hangToCrossNextToVelocity;
			// if(percent < 1){
				toGoal = Vector3.Lerp(playerPosition, goal, percent);
			// }else{
			// 	toGoal = Vector3.Lerp(playerPosition, goal, percent-1);
			// }
			transform.position = toGoal;

			yield return null;
		}
		yield return null;
	}
	public IEnumerator hangToCrossAnotherAction(Vector3 playerPosition, Vector3 goal, int rotate){
		float percent = 0;
		Vector3 toGoal = transform.position;
		transform.rotation =transform.rotation * Quaternion.Euler(0,rotate,0);
		while(percent < 1){
			percent += Time.fixedDeltaTime * hangToCrossThisVelocity;
			// if(percent < 1){
				toGoal = Vector3.Lerp(playerPosition, goal, percent);
			// }else{
			// 	toGoal = Vector3.Lerp(playerPosition, goal, percent-1);
			// }
			transform.position = toGoal;

			yield return null;
		}
		yield return null;
	}
	public IEnumerator fallAction(int rotation, Vector3 vectorInput) {
		//rotation
		Quaternion newRotation = transform.rotation * Quaternion.Euler(0,rotation,0);
		transform.rotation = newRotation;
		//move
		transform.position = transform.position + vectorInput.normalized * fallVerocity * Time.fixedDeltaTime;
		yield return null;
	}
	public IEnumerator walkAction(int rotation, Vector3 vectorInput) {
		//rotation
		Quaternion newRotation = transform.rotation * Quaternion.Euler(0,rotation,0);
		transform.rotation = newRotation;
		
		//move
		transform.position = transform.position + vectorInput.normalized * walkVerocity * Time.fixedDeltaTime;
		yield return null;
	}
	
	public IEnumerator fromHangToDesendAction(int rotation, Vector3 vectorInput) {
		//rotation
		Quaternion newRotation = transform.rotation * Quaternion.Euler(0,rotation,0);
		transform.rotation = newRotation;
		//move
		transform.position = transform.position + vectorInput.normalized * hangToDesendVerocity * Time.fixedDeltaTime;
		yield return null;
	}
	public IEnumerator fromHangToWalkAction(int rotation, Vector3 vectorInput) {
		//rotation
		Quaternion newRotation = transform.rotation * Quaternion.Euler(0,rotation,0);
		transform.rotation = newRotation;
		//move
		transform.position = transform.position + vectorInput.normalized * hangToWalkVerocity * Time.fixedDeltaTime;
		yield return null;
	}
	public IEnumerator hangingWalkAction(int rotation, Vector3 vectorInput) {
		//rotation
		Quaternion newRotation = transform.rotation * Quaternion.Euler(0,rotation,0);
		transform.rotation = newRotation;
		//move
		transform.position = transform.position + vectorInput.normalized * hangingWalkVerocity * Time.fixedDeltaTime;
		yield return null;
	}
	
	public void FixedUpdate() {
		//rotation
		// Quaternion newRotation = transform.rotation * Quaternion.Euler(0,rotation,0);
		// rotation = 0;
		// myRigidbody.MoveRotation(newRotation);
		// //move
		// myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
		// transform.position = transform.position + velocity * Time.fixedDeltaTime;
		// velocity = Vector3.zero;
	}

}
