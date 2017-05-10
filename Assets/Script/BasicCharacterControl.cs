using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class BasicCharacterControl : MonoBehaviour {
	private BasicCharacter m_Character; // A reference to the BasicCharacter on the object
	private Transform m_Cam;                  // A reference to the main camera in the scenes transform
	private Vector3 m_CamForward;             // The current forward direction of the camera
	private Vector3 m_Move;
	private bool m_Grab;                      // the world-relative desired move direction, calculated from the camForward and user input.
	private bool m_Up;
	private bool m_Down;
	private bool m_Right;
	private bool m_Left;
//
	public float grabVelocity = 10f;
	public float desendVelocity = 0.02f;
	public float climbVelocity = 3f;
	public float hangVelocity = 10f;
	public float pullVelocity = 0.02f;
	public float pushVelocity = 0.02f;
	public float fallVerocity = 10f;
	public float walkVerocity = 3f;
	public float hangToDesendVerocity = 0.2f;
	public float hangToWalkVerocity = 0.2f;
	public float hangingWalkVerocity = 1f;
	public float hangToCrossThisVelocity = 5f;
	public float hangToCrossNextToVelocity = 3f;
//
	public delegate Transform blockMoveEventHandler(Transform block);
	public delegate Vector3 positionEventHandler(Vector3 position);
	public static event blockMoveEventHandler OnSendBlockMove;
	public static event positionEventHandler OnSendPlayerPosition;
	public static event positionEventHandler OnSendCameraPosition;
	void Start () {
		// myRigidbody = GetComponent<Rigidbody> ();
		CamManager.OnSendCameraPosition += getCameraPosition;
		m_Character = GetComponent<BasicCharacter>();
		if (Camera.main != null)
		{
			m_Cam = Camera.main.transform;
		}
		else
		{
			Debug.LogWarning(
				"Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
			// we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
		}
	}
	void Destoryed(){
		CamManager.OnSendCameraPosition -= getCameraPosition;
	}
	
	private void Update()
	{
		if (!m_Grab)
		{
			m_Grab = CrossPlatformInputManager.GetButtonDown("Jump");
			//m_Grab = CrossPlatformInputManager.GetButtonDown("up");
		}
		if(!m_Up){
			m_Up = CrossPlatformInputManager.GetButtonDown("up");
		}
		if(!m_Down){
			m_Down = CrossPlatformInputManager.GetButtonDown("down");
		}
		if(!m_Right){
			m_Right = CrossPlatformInputManager.GetButtonDown("right");
		}
		if(!m_Left){
			m_Left = CrossPlatformInputManager.GetButtonDown("left");
		}
	}


	// Fixed update is called in sync with physics
	private void FixedUpdate()
	{
		// read inputs
		float h = CrossPlatformInputManager.GetAxis("Horizontal");
		float v = CrossPlatformInputManager.GetAxis("Vertical");

		// m_Move = v*transform.forward + h*transform.right;
		m_Move = MoveInput(v, h);
		// m_Move =MoveInputHanging(transform.forward, v, h);
		
		// calculate move direction to pass to character
		// if (m_Cam != null)
		// {
		// 	// calculate camera relative direction to move:
		// 	m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
		// 	m_Move = v*m_CamForward + h*m_Cam.right;
		// }
		// else
		// {
		// 	// we use world-relative directions in the case of no main camera
		// 	m_Move = v*Vector3.forward + h*Vector3.right;
		// }

		// pass all parameters to the character control script
		m_Character.Move(m_Move, m_Grab, m_Up, m_Down, m_Right, m_Left);
		m_Grab = false;
		m_Up = false;
		m_Down = false;
		m_Right = false;
		m_Left = false;
	}

	public int PlayerRotate(Vector3 nowForward) {
		int direction = -1;
		int[] directions = new int[4];
		if(nowForward == Vector3.forward){
			direction =0;
			directions[0] = 0; //Vector3.forward;
			directions[1] = -90; //Vector3.left;
			directions[2] = 180; //Vector3.back;
			directions[3] = 90; //Vector3.right;
		}
		if(nowForward == Vector3.back){
			direction = 1;
			directions[0] = 0; // Vector3.back;
			directions[1] = -90; //Vector3.right;
			directions[2] = 180; //Vector3.forward;
			directions[3] = 90; //Vector3.left;
		}
		if(nowForward == Vector3.right){
			direction = 2;
			directions[0] = 0; //Vector3.right;
			directions[1] = -90; //Vector3.forward;
			directions[2] = 180; //Vector3.left;
			directions[3] = 90; //Vector3.back;
		}
		if(nowForward == Vector3.left){
			direction = 3;
			directions[0] = 0; //Vector3.left;
			directions[1] = -90; //Vector3.back;
			directions[2] = 180; //Vector3.right;
			directions[3] = 90; //Vector3.forward;
		}


		int result = 0;
		if(Input.GetKey(KeyCode.LeftArrow)){
			if(transform.forward != Vector3.left){
				if(direction == 0)
					result = directions[1];
				if(direction == 1)
					result = directions[3];
				if(direction == 2)
					result = directions[2];
				if(direction == 3)
					result = directions[0];
			}
		}
		else if(Input.GetKey(KeyCode.RightArrow)){
			if(transform.forward != Vector3.right)
				if(direction == 0)
					result = directions[3];
				if(direction == 1)
					result = directions[1];
				if(direction == 2)
					result = directions[0];
				if(direction == 3)
					result = directions[2];
		}
		else if(Input.GetKey(KeyCode.UpArrow)){
			if(transform.forward != Vector3.forward)
				if(direction == 0)
					result = directions[0];
				if(direction == 1)
					result = directions[2];
				if(direction == 2)
					result = directions[1];
				if(direction == 3)
					result = directions[3];
		}
		else if(Input.GetKey(KeyCode.DownArrow)){
			if(transform.forward != Vector3.back)
				if(direction == 0)
					result = directions[2];
				if(direction == 1)
					result = directions[0];
				if(direction == 2)
					result = directions[3];
				if(direction == 3)
					result = directions[1];
		}
	return result;
	}
	private Vector3 MoveInput(float v, float h) {
		bool m_Up = CrossPlatformInputManager.GetButton("up");
		bool m_Down = CrossPlatformInputManager.GetButton("down");
		bool m_Right = CrossPlatformInputManager.GetButton("right");
		bool m_Left = CrossPlatformInputManager.GetButton("left");
		Vector3 moveInput = Vector3.zero;
		if((m_Up || m_Down) && !m_Left && !m_Right){
			moveInput = v*transform.forward;
		}
		else if((m_Left || m_Right) && !m_Up && !m_Down){
			moveInput = h*transform.right;
		}
		return moveInput;
	}
	public Vector3 MoveInputHanging(Vector3 nowForward) {
		bool m_Up = CrossPlatformInputManager.GetButton("up");
		bool m_Down = CrossPlatformInputManager.GetButton("down");
		bool m_Right = CrossPlatformInputManager.GetButton("right");
		bool m_Left = CrossPlatformInputManager.GetButton("left");
		float h = CrossPlatformInputManager.GetAxis("Horizontal");
		float v = CrossPlatformInputManager.GetAxis("Vertical");		
		Vector3 moveInput= Vector3.zero;
		if(nowForward == Vector3.forward){
			moveInput = new Vector3(h, 0, 0);
		}
		if(nowForward == Vector3.back){
			moveInput = new Vector3(-h, 0, 0);
		}
		if(nowForward == Vector3.right){
			moveInput = new Vector3(0, 0, -h);
		}
		if(nowForward == Vector3.left){
			moveInput = new Vector3(0, 0, h);
		}
		
		if(m_Up || m_Down){
			moveInput= Vector3.zero;
		}
		else if(m_Left || m_Right){
			return moveInput;
		}
		return Vector3.zero;
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
	public IEnumerator hangAction(Vector3 playerPosition, Vector3 midGoal ,Vector3 goal, int rotate){
		float percent = 0;
		// Vector3 toGoal = Vector3.zero;
		transform.rotation = transform.rotation * Quaternion.Euler(0,rotate,0);
		while(percent < 1){
			percent += Time.fixedDeltaTime * hangVelocity;
			// if(percent < 1){
				Vector3 toGoal = Vector3.Slerp(playerPosition, goal, percent * percent);
			// }else{
				// toGoal = Vector3.Lerp(midGoal, goal, (percent-1.2f) * (percent-1.2f));
			// }
			transform.position = toGoal;
			yield return null;
		}
		yield return null;
	}
	public IEnumerator hangToCrossThisAction(Vector3 playerPosition, Vector3 goal, int rotate){
		float percent = 0;
		// Vector3 toGoal = transform.position;
		transform.rotation =transform.rotation * Quaternion.Euler(0,rotate,0);
		while(percent < 1){
			percent += Time.fixedDeltaTime * hangToCrossThisVelocity;
			// if(percent < 1){
				 Vector3 toGoal = Vector3.Lerp(playerPosition, goal, percent);
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
	// public IEnumerator walkAction(int rotation, Vector3 vectorInput) {
	// 	//rotation
	// 	Quaternion newRotation = transform.rotation * Quaternion.Euler(0,rotation,0);
	// 	transform.rotation = newRotation;
		
	// 	//move
	// 	transform.position = transform.position + vectorInput.normalized * walkVerocity * Time.fixedDeltaTime;
	// 	yield return null;
	// }
	
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



		
}


