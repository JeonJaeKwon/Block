using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PlayerController))]
public class Player : MonoBehaviour {

	//from external
	PlayerController playerController; //process data on rotate and moveInput  
	private LayerMask layerMask;
	private float playerHalfHeigh;
	private float playerWidth;
	private float blockSize;

	//default
 	private Transform baseBlock; //player's ground
	private Transform[] blocks;	//blocks : [0]->player's ground, [1]-> player's up, [2]->block[0]'s forward, [3]->block[0]'s left, [4]->block[0]'s down, [5]->block's right, [6]->player's' forward, [7]->player's left, [8]->player's down, [9]->player's right, [10]->block[1]'s' forward, [11]->block[1]'s left, [12]->block[1]'s down, [13]->block[1]'s right
	
	
	private bool idle; // when idle
	//walk
	private bool walk; // when walk
	private bool walkToHangAction;



	//grab block
	private bool grabMove;
	private bool grab; //when grab
	private Transform pickedUpBlock; 
	private Vector3 WhenPickedUpBlockPosition;
	private Transform[] blocksForPickedUpBlock; //blocksForPickedUpBlock : [0]->pickedUpBlock, when transform.forward:: [1]->pickedUpBlock's forward, [1]->pickedUpBlock's forward,[2]->pickedUpBlock's left,[3]->pickedUpBlock's back, [4]->pickedUpBlock's right 
	private bool frontArea;
	private bool baseFrontArea;
	private bool backArea;
	private bool baseBackArea;

	private bool grabAction;
	private Vector3 grabNowForward;

	//desend
	private bool desend; // when desend
	//fallOrwalk
	private bool fallOrWalk; //when fall or walk
	//pull
	private bool pull; // when grab, pull
	private bool pullToHangAction;
	//push
	private bool push;// when grab, push
	private bool pushToHangAction;
	//hang
	private bool hang; // when hang
	private int hangRotate;
	[RangeAttribute(0,2)] public float hangDecisionDistance;
	private float blockToPlayerDistance;
	private float maximumDegree; //when hang state, player can move maximum degree from block 
	private float minimumDegree;
	private float standardDegree; // when hang state, block's sufaceLine can move maximum degree from block
	private float  safeDegree;

	private int quadrant;
	private float hangNowDegree;
	private Vector3 hangGoal;
	private int classifyHangingBlockNumber;

	private Vector3 hangNowForward;
	private Vector3 hangNowRight;
	private Transform hangingBlock;

	private bool hangAction;
	private bool hangToCrossThis;
	private bool hangToCrossThisRotationCheck;
	private bool hangToCrossThisFinish;

	private bool hangToCrossNextTo;
	private bool hangToCrossNextToRotationCheck;
	private bool hangToCrossNextToFinish;

	private bool hangToCrossAnother;
	private bool hangToCrossAnotherRotationCheck;
	private bool hangToCrossAnotherFinish;
	public float hangToCrossAnotherDistance;
	private Transform[] blocksForHangingpBlock; //blockForHangingBlock : [0] -> hangingBlock [1] -> HB's up [2] -> HB's left [3] -> HB's right [4] -> HB's up [5] -> player's left [6] -> player's right [7] -> player's down 

	//climb
	private bool climb; //when climb
	[RangeAttribute(0,1)] public float climbDecisionDistance;

	void Start () {
		playerController = GetComponent<PlayerController> ();
		playerHalfHeigh = transform.localScale.y * 0.5f;
		playerWidth = transform.localScale.x;
		blockSize = 1f; //block prefab's size
		layerMask = -1; //default layerMask value;
		blocks = new Transform[14];
		blocksForPickedUpBlock = new Transform[5];
		blocksForHangingpBlock = new Transform[8];

		if(climbDecisionDistance < playerWidth * 0.5f)
			climbDecisionDistance = playerWidth * 0.5f;
		if(climbDecisionDistance > blockSize)
			climbDecisionDistance = blockSize;
		if(hangDecisionDistance < playerHalfHeigh)
			hangDecisionDistance = playerHalfHeigh;
		if(hangDecisionDistance > blockSize + playerHalfHeigh)
			hangDecisionDistance = blockSize + playerHalfHeigh -0.1f;
		idle = true;

		standardDegree = 45;
		blockToPlayerDistance = (blockSize + playerWidth) * 0.5f;
		maximumDegree = Vector2.Angle(new Vector2(0,1), new Vector2(blockSize * 0.5f, blockToPlayerDistance));
		minimumDegree = Vector2.Angle(new Vector2(0,1), new Vector2(blockSize * 0.5f, blockToPlayerDistance + playerWidth * 0.5f));
		safeDegree = (maximumDegree - minimumDegree) * 2;
		Debug.Log("maximumDegree : " + maximumDegree);
		Debug.Log("minimumDegree : " + minimumDegree);

 		hangToCrossAnotherDistance = 0.05f;
	}
	
	void Update () {
		Ray forwardRay = new Ray(transform.position, transform.forward); //ray from player
		Ray downRay = new Ray(transform.position, -transform.up); //ray from player
		RaycastHit hit;
		Debug.DrawRay(forwardRay.origin, forwardRay.direction * playerWidth, Color.red);
		Debug.DrawRay(downRay.origin, downRay.direction, Color.red);

		if(idle){
			Debug.Log("idle");
			idle = false;
			fallOrWalk = true;
		}
		if(desend){
			Debug.Log("desend");
			Vector3 now = transform.position;
			Vector3 goal = blocks[0].position + transform.forward;
			StartCoroutine(playerController.desendAction(now, goal));
			if(Vector3.Distance(transform.position, goal) < 0.1f){
				desend = false;
				fallOrWalk = true;
			}
		}
		if(fallOrWalk){
			Debug.Log("fall or Walk");
			if(Physics.Raycast(downRay, out hit, playerHalfHeigh, layerMask)){
				baseBlock = hit.transform;
				updateBlocks(blocks, baseBlock, transform.forward);
				
				fallOrWalk = false;
				walk = true;
			}else{
				StartCoroutine(playerController.fallAction(0, Vector3.down));
			}
		}
		if(walk){
            Debug.Log("walk");
			//check to need update blocks
			if(Physics.Raycast(downRay, out hit, 1f, layerMask)){
				if(baseBlock.position != hit.transform.position){
					baseBlock = hit.transform;
					updateBlocks(blocks, baseBlock, transform.forward);
				}				
			}
			//for climb
			if (Physics.Raycast(forwardRay, out hit, climbDecisionDistance, layerMask)){
				// baseBlock = hit.transform;
				updateBlocks(blocks, hit.transform, transform.forward);
				if(blocks[8] == null && !Physics.Raycast(blocks[0].position, Vector3.up, blockSize, layerMask)){
					walk = false;
					climb = true;
				}else{
					updateBlocks(blocks,baseBlock, transform.forward);
					StartCoroutine(playerController.walkAction(PlayerRotate(transform.forward), MoveInputCantFront(0, transform.forward)));
					walk = false;
					fallOrWalk = true;
				}
			}
			//for hang
			else if(!Physics.Raycast(downRay, hangDecisionDistance, layerMask)){
				hangNowForward = -transform.forward;
				hangNowRight = checkDirectionAboutRight(hangNowForward);
				hangingBlock = blocks[0];
				updateBlockForHangingBlock(blocksForHangingpBlock, hangingBlock, hangNowForward);

				walk = false;
				walkToHangAction = true;
				hangAction = true;
			}
			
			//for grab
			else if (Input.GetButton("Fire1")){
				if (!grabAction && Physics.Raycast(forwardRay, out hit, blockSize, layerMask)){
					pickedUpBlock = hit.transform;
					WhenPickedUpBlockPosition = hit.transform.position;
					grabNowForward = transform.forward;
					updateBlocks(blocks, baseBlock, transform.forward);
					updateBlockForPickedUpBlock(blocksForPickedUpBlock, pickedUpBlock);
					
					walk = false;
					grabAction = true;
				}
			}else{
				StartCoroutine(playerController.walkAction(PlayerRotate(transform.forward), MoveInput(0)));
			}
		}
		if(hangAction){
			Debug.Log("hangAction");
			Vector3 now = transform.position;
			Vector3 goal = hangingBlock.position - (hangNowForward * blockSize + hangNowForward * playerWidth) * 0.5f;
			if(walkToHangAction || pushToHangAction){
				StartCoroutine(playerController.hangAction(now, goal, 180));
			}else if(pullToHangAction){
				StartCoroutine(playerController.hangAction(now, goal, 0));
			}
			walkToHangAction = false;
			pullToHangAction = false;
			pushToHangAction = false;
			if(Vector3.Distance(transform.position, goal) < 0.1f && transform.forward == hangNowForward){
				if(blocksForHangingpBlock[7] != null){
					updateBlocks(blocks, blocksForHangingpBlock[7], transform.forward);
					StartCoroutine(playerController.fromHangToWalkAction(180, -hangNowForward));
					hangAction = false;
					walk = true;
				}else{
					hangAction = false;
					hang = true;
				}
			}
		}
		if(hang){
			Debug.Log("hang");
			hangRotate = 0;
			hangGoal = Vector3.zero;
			// print("nowForward" + hangNowForward);
			// print("hangNowRight" + hangNowRight);
			//check degree for classify hang to this block or other block 

			classifyHangingBlockNumber = 0;
			hangGoal = Vector3.zero;
			quadrant = searchQuadrant(transform.position - hangingBlock.position);
			float hangNowDegreeBuf = Vector2.Angle(new Vector2(-hangNowForward.x, -hangNowForward.z), new Vector2(transform.position.x - hangingBlock.position.x, transform.position.z - hangingBlock.position.z));
			hangNowDegree = correctHangNowDegree(hangNowForward, quadrant, hangNowDegreeBuf);

			//print(hangNowDegree);
			// print(hangNowForward);
			// print(hangingBlock.position);

			//degree is more than maximum degree, check new rotation and position
			if(hangNowForward == Vector3.left && hangNowDegree > maximumDegree - safeDegree && hangNowDegree < 90){	
				if(blocksForHangingpBlock[3] == null && blocksForHangingpBlock[6] == null){
					hangGoal = searchHangGoal(90 - maximumDegree + safeDegree * 1.5f, hangingBlock);
					hangRotate = -90;
					print(hangGoal);

					hang = false;
					hangToCrossThis = true;
				}
				else if(blocksForHangingpBlock[3] != null && blocksForHangingpBlock[6] == null){
					hangGoal = searchHangGoal(360 - maximumDegree + safeDegree * 1.5f, blocksForHangingpBlock[3]);
					classifyHangingBlockNumber = 3;
					print(hangGoal);

					hang = false;
					hangToCrossNextTo = true;
				}
				else if(blocksForHangingpBlock[6] != null){
					print("Test");
					hangGoal = searchHangGoal(270 - maximumDegree + safeDegree * 1.5f, blocksForHangingpBlock[6]);
					classifyHangingBlockNumber = 6;
					hangRotate = 90;
					print(hangGoal);

					hang = false;
					hangToCrossAnother = true;
				}
			}else if(hangNowForward == Vector3.left && hangNowDegree < 360 - maximumDegree + safeDegree && hangNowDegree > 270){
				if(blocksForHangingpBlock[2] == null && blocksForHangingpBlock[5] == null){
					hangGoal = searchHangGoal(270 + maximumDegree - safeDegree * 1.5f, hangingBlock);
					classifyHangingBlockNumber = 2;
					hangRotate = 90;
					print(hangGoal);

					hang = false;
					hangToCrossThis = true;
				}
				else if(blocksForHangingpBlock[2] != null && blocksForHangingpBlock[5] == null){
					hangGoal = searchHangGoal(0 + maximumDegree - safeDegree * 1.5f, blocksForHangingpBlock[2]);
					classifyHangingBlockNumber = 2;
					print(hangGoal);

					hang = false;
					hangToCrossNextTo = true;
				}
				else if(blocksForHangingpBlock[5] != null){
					hangGoal = searchHangGoal(90 + maximumDegree - safeDegree * 1.5f, blocksForHangingpBlock[5]);
					classifyHangingBlockNumber = 5;
					hangRotate = -90;
					print(hangGoal);

					hang = false;
					hangToCrossAnother = true;
				}
			}
			if(hangNowForward == Vector3.back && hangNowDegree > 90 + maximumDegree - safeDegree && hangNowDegree < 180){
				if(blocksForHangingpBlock[3] == null && blocksForHangingpBlock[6] == null){
					hangGoal = searchHangGoal(180 - maximumDegree + safeDegree * 1.5f, hangingBlock);
					hangRotate = -90;
					print(hangGoal);

					hang = false;
					hangToCrossThis = true;
				}
				else if(blocksForHangingpBlock[3] != null && blocksForHangingpBlock[6] == null){
					hangGoal = searchHangGoal(90 - maximumDegree + safeDegree * 1.5f, blocksForHangingpBlock[3]);
					classifyHangingBlockNumber = 3;
					print(hangGoal);

					hang = false;
					hangToCrossNextTo = true;
				}
				else if(blocksForHangingpBlock[6] != null){
					print("Test");
					hangGoal = searchHangGoal(360 - maximumDegree + safeDegree * 1.5f, blocksForHangingpBlock[6]);
					classifyHangingBlockNumber = 6;
					hangRotate = 90;
					print(hangGoal);

					hang = false;
					hangToCrossAnother = true;
				}
			}else if(hangNowForward == Vector3.back && hangNowDegree < 90 - maximumDegree + safeDegree && hangNowDegree > 0){
				if(blocksForHangingpBlock[2] == null && blocksForHangingpBlock[5] == null){
					hangGoal = searchHangGoal(0 + maximumDegree - safeDegree * 1.5f, hangingBlock);
					classifyHangingBlockNumber = 2;
					hangRotate = 90;
					print(hangGoal);

					hang = false;
					hangToCrossThis = true;
				}
				else if(blocksForHangingpBlock[2] != null && blocksForHangingpBlock[5] == null){
					hangGoal = searchHangGoal(90 + maximumDegree - safeDegree * 1.5f, blocksForHangingpBlock[2]);
					classifyHangingBlockNumber = 2;
					print(hangGoal);

					hang = false;
					hangToCrossNextTo = true;
				}
				else if(blocksForHangingpBlock[5] != null){
					hangGoal = searchHangGoal(180 + maximumDegree - safeDegree * 1.5f, blocksForHangingpBlock[5]);
					classifyHangingBlockNumber = 5;
					hangRotate = -90;
					print(hangGoal);

					hang = false;
					hangToCrossAnother = true;
				}
			}
			if(hangNowForward == Vector3.right && hangNowDegree > 180 + maximumDegree - safeDegree && hangNowDegree < 270){
				if(blocksForHangingpBlock[3] == null && blocksForHangingpBlock[6] == null){
					hangGoal = searchHangGoal(270 - maximumDegree + safeDegree * 1.5f, hangingBlock);
					hangRotate = -90;
					print(hangGoal);

					hang = false;
					hangToCrossThis = true;
				}
				else if(blocksForHangingpBlock[3] != null && blocksForHangingpBlock[6] == null){
					hangGoal = searchHangGoal(180 - maximumDegree + safeDegree * 1.5f, blocksForHangingpBlock[3]);
					classifyHangingBlockNumber = 3;
					print(hangGoal);

					hang = false;
					hangToCrossNextTo = true;
				}
				else if(blocksForHangingpBlock[6] != null){
					hangGoal = searchHangGoal(90 - maximumDegree + safeDegree * 1.5f, blocksForHangingpBlock[6]);
					classifyHangingBlockNumber = 6;
					hangRotate = 90;
					print(hangGoal);

					hang = false;
					hangToCrossAnother = true;
				}
			}else if(hangNowForward == Vector3.right && hangNowDegree < 180 - maximumDegree + safeDegree && hangNowDegree > 90){
				if(blocksForHangingpBlock[2] == null && blocksForHangingpBlock[5] == null){
					hangGoal = searchHangGoal(90 + maximumDegree - safeDegree * 1.5f, hangingBlock);
					classifyHangingBlockNumber = 2;
					hangRotate = 90;
					print(hangGoal);

					hang = false;
					hangToCrossThis = true;
				}
				else if(blocksForHangingpBlock[2] != null && blocksForHangingpBlock[5] == null){
					hangGoal = searchHangGoal(180 + maximumDegree - safeDegree * 1.5f, blocksForHangingpBlock[2]);
					classifyHangingBlockNumber = 2;
					print(hangGoal);

					hang = false;
					hangToCrossNextTo = true;
				}
				else if(blocksForHangingpBlock[5] != null){
					hangGoal = searchHangGoal(270 + maximumDegree - safeDegree * 1.5f, blocksForHangingpBlock[5]);
					classifyHangingBlockNumber = 5;
					hangRotate = -90;
					print(hangGoal);

					hang = false;
					hangToCrossAnother = true;
				}
			}
			if(hangNowForward == Vector3.forward && hangNowDegree > 270 + maximumDegree - safeDegree && hangNowDegree < 360){
				if(blocksForHangingpBlock[3] == null && blocksForHangingpBlock[6] == null){
					hangGoal = searchHangGoal(360 - maximumDegree + safeDegree * 1.5f, hangingBlock);
					hangRotate = -90;
					print(hangGoal);

					hang = false;
					hangToCrossThis = true;
				}
				else if(blocksForHangingpBlock[3] != null && blocksForHangingpBlock[6] == null){
					hangGoal = searchHangGoal(270 - maximumDegree + safeDegree * 1.5f, blocksForHangingpBlock[3]);
					classifyHangingBlockNumber = 3;
					print(hangGoal);

					hang = false;
					hangToCrossNextTo = true;
				}
				else if(blocksForHangingpBlock[6] != null){
					print("Test");
					hangGoal = searchHangGoal(180 - maximumDegree + safeDegree * 1.5f, blocksForHangingpBlock[6]);
					classifyHangingBlockNumber = 6;
					hangRotate = 90;
					print(hangGoal);

					hang = false;
					hangToCrossAnother = true;
				}
			}else if(hangNowForward == Vector3.forward && hangNowDegree < 270 - maximumDegree + safeDegree && hangNowDegree > 180){
				if(blocksForHangingpBlock[2] == null && blocksForHangingpBlock[5] == null){
					hangGoal = searchHangGoal(180 + maximumDegree - safeDegree * 1.5f, hangingBlock);
					classifyHangingBlockNumber = 2;
					hangRotate = 90;
					print(hangGoal);

					hang = false;
					hangToCrossThis = true;
				}
				else if(blocksForHangingpBlock[2] != null && blocksForHangingpBlock[5] == null){
					hangGoal = searchHangGoal(270 + maximumDegree - safeDegree * 1.5f, blocksForHangingpBlock[2]);
					classifyHangingBlockNumber = 2;
					print(hangGoal);

					hang = false;
					hangToCrossNextTo = true;
				}
				else if(blocksForHangingpBlock[5] != null){
					hangGoal = searchHangGoal(0 + maximumDegree - safeDegree * 1.5f, blocksForHangingpBlock[5]);
					classifyHangingBlockNumber = 5;
					hangRotate = -90;
					print(hangGoal);

					hang = false;
					hangToCrossAnother = true;
				}
			// //for climb
			}else if(Input.GetKeyDown(KeyCode.UpArrow)){
				if(blocksForHangingpBlock[1] == null && blocksForHangingpBlock[4] == null){
				updateBlocks(blocks, hangingBlock, transform.forward);

				hang = false;
				climb = true;
				}
			//for fall
			}else if(Physics.Raycast(downRay, blockSize, layerMask)){
				StartCoroutine(playerController.fromHangToWalkAction(180, Vector3.zero));
				
				hang = false;
				walk = true;
			}else if(Input.GetKeyDown(KeyCode.DownArrow)){
				updateBlocks(blocks, hangingBlock, transform.forward);
				StartCoroutine(playerController.fromHangToDesendAction(180, Vector3.zero));
				if(transform.forward == -hangNowForward){
					hang = false;
					desend = true;
				}
			//for hanging walk
			}else
				StartCoroutine(playerController.hangingWalkAction(0, MoveInputHanging(hangNowForward, 0)));
		}
		if(hangToCrossThis){
			Debug.Log("hangToCrossThis");

			Vector3 now = transform.position;
			if(!hangToCrossThisRotationCheck){
				StartCoroutine(playerController.hangToCrossThisAction(now, hangGoal, hangRotate));
				hangToCrossThisRotationCheck = true;
			}else{
				StartCoroutine(playerController.hangToCrossThisAction(now, hangGoal, 0));
			}
			if(Vector3.Distance(transform.position, hangGoal) < 0.1f){
				hangToCrossThisRotationCheck = false;
				hangToCrossThis = false;
				hangToCrossThisFinish = true;
			}
		}
		if(hangToCrossThisFinish){
			print("hangToCrossThisFinish");
			hangNowForward = transform.forward;
			hangNowRight = checkDirectionAboutRight(hangNowForward);
			hangingBlock = blocksForHangingpBlock[0];
			updateBlockForHangingBlock(blocksForHangingpBlock, hangingBlock, hangNowForward);
			// Debug.Log("hangNowForward : " + hangNowForward);
			// Debug.Log("hangNowRight : " + hangNowRight);
			// Debug.Log("blocksForHangingpBlock?" + blocksForHangingpBlock[0].position);

			hangToCrossThisFinish = false;
			hang = true;
		}
		if(hangToCrossNextTo){
			Debug.Log("hangToCrossNextTo");
			Vector3 now = transform.position;
				StartCoroutine(playerController.hangToCrossNextToAction(now, hangGoal, 0));

			// transform.position = hangGoal;
			if(Vector3.Distance(transform.position, hangGoal) < 0.1f){
				hangToCrossNextTo = false;
				hangToCrossNextToFinish = true;
			}
		}
		if(hangToCrossNextToFinish){
			print("hangToCrossNextToFinish");
			hangNowForward = transform.forward;
			hangNowRight = checkDirectionAboutRight(hangNowForward);
			hangingBlock = blocksForHangingpBlock[classifyHangingBlockNumber];
			updateBlockForHangingBlock(blocksForHangingpBlock, hangingBlock, hangNowForward);
			// Debug.Log("hangNowForward : " + hangNowForward);
			// Debug.Log("hangNowRight : " + hangNowRight);
			// Debug.Log("blocksForHangingpBlock?" + blocksForHangingpBlock[0].position);

			hangToCrossNextToFinish = false;
			hang = true;
		}
		if(hangToCrossAnother){
			Debug.Log("hangToCrossAnother");
			// transform.Rotate(0,hangRotate,0);
			// transform.position = hangGoal;
			Vector3 now = transform.position;
			if(!hangToCrossAnotherRotationCheck){
				StartCoroutine(playerController.hangToCrossAnotherAction(now, hangGoal, hangRotate));
				hangToCrossAnotherRotationCheck = true;
			}else{
				StartCoroutine(playerController.hangToCrossAnotherAction(now, hangGoal, 0));
			}
			if(Vector3.Distance(transform.position, hangGoal) < hangToCrossAnotherDistance){
				hangToCrossAnotherRotationCheck = false;
				hangToCrossAnother = false;
				hangToCrossAnotherFinish = true;
			}
		}
		if(hangToCrossAnotherFinish){
			print("hangToCrossOtherFinish");
			hangNowForward = transform.forward;
			hangNowRight = checkDirectionAboutRight(hangNowForward);
			hangingBlock = blocksForHangingpBlock[classifyHangingBlockNumber];
			updateBlockForHangingBlock(blocksForHangingpBlock, hangingBlock, hangNowForward);
			// Debug.Log("hangNowForward : " + hangNowForward);
			// Debug.Log("hangNowRight : " + hangNowRight);
			// Debug.Log("blocksForHangingpBlock?" + blocksForHangingpBlock[0].position);

			hangToCrossAnotherFinish = false;
			hang = true;
		}
		if(grabAction){
			Debug.Log("grabAction");
			Vector3 now = transform.position;
			Vector3 goal = WhenPickedUpBlockPosition - grabNowForward * (blockSize + playerWidth) * 0.5f;
			StartCoroutine(playerController.grabAction(now, goal));
			if(Vector3.Distance(transform.position, goal) < 0.1f){
				grabAction = false;
				grab = true;
			}
		}
		if(grab){
			Debug.Log("grab");
			if(blocks[8] != null)
				backArea = true;
			if (blocks[4] != null)
				baseBackArea = true;
			if(blocks[2] != null)
				baseFrontArea = true;
			if (Physics.Raycast(WhenPickedUpBlockPosition, grabNowForward, blockSize, layerMask))
				frontArea = true;
			Debug.Log("backArea : " + backArea + ", baseBackArea : " + baseBackArea + ", frontArea : " + frontArea + ", baseFrontArea : " + baseFrontArea);
			
			grab = false;
			grabMove = true;
		}

		if(grabMove){
			Debug.Log("grabMove");
			if (Input.GetKeyDown(KeyCode.RightArrow)){
                if (transform.forward == -Vector3.right){
                    pull = true;
				}
			}
            if (Input.GetKeyDown(KeyCode.LeftArrow)){
                if (transform.forward == -Vector3.left){
                    pull = true;
				}
			}
            if (Input.GetKeyDown(KeyCode.UpArrow)){
                if (transform.forward == -Vector3.forward){
                    pull = true;
				}
			}
            if (Input.GetKeyDown(KeyCode.DownArrow)){
                if (transform.forward == -Vector3.back){
                    pull = true;
				}
			}

			if (pickedUpBlock && Input.GetKeyDown(KeyCode.RightArrow))
                if (transform.forward == Vector3.right)
                    push = true;
            if (pickedUpBlock && Input.GetKeyDown(KeyCode.LeftArrow))
                if (transform.forward == Vector3.left)
                    push = true;
            if (pickedUpBlock && Input.GetKeyDown(KeyCode.UpArrow))
                if (transform.forward == Vector3.forward)
                    push = true;
            if (pickedUpBlock && Input.GetKeyDown(KeyCode.DownArrow))
                if (transform.forward == Vector3.back)
                    push = true;

			if(Input.GetKeyDown(KeyCode.RightArrow)
			||Input.GetKeyDown(KeyCode.LeftArrow)
			||Input.GetKeyDown(KeyCode.UpArrow)
			||Input.GetKeyDown(KeyCode.DownArrow))
			grabMove = false;
		}

		if(pull){
			Debug.Log("pull");
			if(backArea){
				backArea = false;
				baseBackArea = false;
				frontArea = false;
				baseFrontArea = false;

				pull = false;
				walk = true;
			}else if(!backArea && !baseBackArea){
				Vector3 now = transform.position;
				Vector3 goal = WhenPickedUpBlockPosition - Vector3.up - grabNowForward - grabNowForward;
				Vector3 nowBlock = pickedUpBlock.position;
				Vector3 goalBlock = WhenPickedUpBlockPosition - grabNowForward;
				StartCoroutine(playerController.pullAction(now, goal, nowBlock, goalBlock, pickedUpBlock));
				if(pickedUpBlock.position == goalBlock){
					backArea = false;
					baseBackArea = false;
					frontArea = false;
					baseFrontArea = false;

					hangNowForward = transform.forward;
					hangNowRight = checkDirectionAboutRight(hangNowForward);
					hangingBlock = blocks[0];
					updateBlockForHangingBlock(blocksForHangingpBlock, hangingBlock, hangNowForward);
					
					pull = false;
					pullToHangAction = true;
					hangAction = true;
				}
			}else if(!backArea && baseBackArea){
				Vector3 now = transform.position;
				Vector3 goal = WhenPickedUpBlockPosition - grabNowForward - grabNowForward;
				Vector3 nowBlock = pickedUpBlock.position;
				Vector3 goalBlock = WhenPickedUpBlockPosition - grabNowForward;
				StartCoroutine(playerController.pullAction(now, goal, nowBlock, goalBlock, pickedUpBlock));
				if(pickedUpBlock.position == goalBlock){
					backArea = false;
					baseBackArea = false;
					frontArea = false;
					baseFrontArea = false;
					
					pull = false;
					walk = true;
				}
			}
		}

		
		if(push){
			Debug.Log("push");
			if(frontArea){
				backArea = false;
				baseBackArea = false;
				frontArea = false;
				baseFrontArea = false;

				push = false;
				walk = true;
			}else if(!frontArea && !baseFrontArea){
				Vector3 now = transform.position;
				Vector3 goal = WhenPickedUpBlockPosition - Vector3.up;
				Vector3 nowBlock = pickedUpBlock.position;
				Vector3 goalBlock = WhenPickedUpBlockPosition + grabNowForward;
				StartCoroutine(playerController.pushAction(now, goal, nowBlock, goalBlock, pickedUpBlock));
				if(pickedUpBlock.position == goalBlock){
					backArea = false;
					baseBackArea = false;
					frontArea = false;
					baseFrontArea = false;

					hangNowForward = -transform.forward;
					hangNowRight = checkDirectionAboutRight(hangNowForward);
					hangingBlock = blocks[0];
					updateBlockForHangingBlock(blocksForHangingpBlock, hangingBlock, hangNowForward);

					push = false;
					pushToHangAction = true;
					hangAction = true;
				}
			}else if(!frontArea && baseFrontArea){
				Vector3 now = transform.position;
				Vector3 goal = WhenPickedUpBlockPosition;
				Vector3 nowBlock = pickedUpBlock.position;
				Vector3 goalBlock = WhenPickedUpBlockPosition + grabNowForward;
				StartCoroutine(playerController.pushAction(now, goal, nowBlock, goalBlock, pickedUpBlock));
				if(pickedUpBlock.position == goalBlock){
					backArea = false;
					baseBackArea = false;
					frontArea = false;
					baseFrontArea = false;

					push = false;
					walk = true;
				}
			}
		}
		if(climb){
			Debug.Log("Climb");
			Vector3 now = transform.position;
			Vector3 goal = blocks[0].position + Vector3.up;
			StartCoroutine(playerController.climbAction(now, goal));
			// if(transform.position == goal){
				climb = false;
				idle = true;
				
			// }
		}

		// reflection(rotate, moveInput);
	}

	private Vector3 MoveInput(float y) {
		Vector3 moveInput = Vector3.zero;
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)){
			moveInput = new Vector3(0, y, Input.GetAxisRaw("Vertical"));
		}
		else if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)){
			moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), y, 0);
		}
		else{
			moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), y, Input.GetAxisRaw("Vertical"));
		}
		return moveInput;
	}
	private Vector3 MoveInputCantFront(float y, Vector3 nowForward) {
		Vector3 moveInput = Vector3.zero;
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)){
			moveInput = new Vector3(0, y, Input.GetAxisRaw("Vertical"));
		}
		else if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)){
			moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), y, 0);
		}
		else{
			moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), y, Input.GetAxisRaw("Vertical"));
		}

		if(nowForward == Vector3.forward){
			if(moveInput.z > 0)
				moveInput.z = 0;
		}else if(nowForward == Vector3.back){
			if(moveInput.z < 0)
				moveInput.z = 0;
		}else if(nowForward == Vector3.right){
			if(moveInput.x > 0)
				moveInput.x = 0;
		}else if(nowForward == Vector3.left){
			if(moveInput.x < 0)
				moveInput.x = 0;
		}
		return moveInput;
	}
	private Vector3 MoveInputHanging(Vector3 nowForward, float y) {
		Vector3 moveInput= Vector3.zero;
		if(nowForward == Vector3.forward){
			moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);
		}
		if(nowForward == Vector3.back){
			moveInput = new Vector3(-Input.GetAxisRaw("Horizontal"), 0, 0);
		}
		if(nowForward == Vector3.right){
			moveInput = new Vector3(0, 0, -Input.GetAxisRaw("Horizontal"));
		}
		if(nowForward == Vector3.left){
			moveInput = new Vector3(0, 0, Input.GetAxisRaw("Horizontal"));
		}
		
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)){
			moveInput= Vector3.zero;
		}
		else if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)){
			return moveInput;
		}
		return Vector3.zero;
	}

	private int PlayerRotate(Vector3 nowForward) {
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
	private void cleanBlock(Transform[] array) {
		for(int i=0; i<array.Length; i++)
		array[i] = null;
	}

	private void updateBlocks(Transform[] array, Transform baseBlock, Vector3 nowForward){
		Debug.Log("updateBlocks");
		cleanBlock(array);
		SearchBlock(baseBlock, array, nowForward);
		// for(int i=0; i<array.Length; i++){
		// 	if(array[i] !=null)
		// 	Debug.Log(i + " - blocks number : " + array[i]);
		// }
	}
	private void SearchBlock(Transform baseBlock, Transform[] array, Vector3 nowForward) {
		blocks[0] = baseBlock;

		Vector3[] standard = new Vector3[3];
		standard[0] = new Vector3(baseBlock.position.x, baseBlock.position.y, baseBlock.position.z);
		standard[1] = new Vector3(baseBlock.position.x, baseBlock.position.y + 1, baseBlock.position.z);
		standard[2] = new Vector3(baseBlock.position.x, baseBlock.position.y + 2, baseBlock.position.z);

		Vector3[] directions = new Vector3[4];

		if(nowForward == Vector3.forward){
			directions[0] = Vector3.forward;
			directions[1] = Vector3.left;
			directions[2] = Vector3.back;
			directions[3] = Vector3.right;
		}
		if(nowForward == Vector3.back){
			directions[0] = Vector3.back;
			directions[1] = Vector3.right;
			directions[2] = Vector3.forward;
			directions[3] = Vector3.left;
		}
		if(nowForward == Vector3.right){
			directions[0] = Vector3.right;
			directions[1] = Vector3.forward;
			directions[2] = Vector3.left;
			directions[3] = Vector3.back;
		}
		if(nowForward == Vector3.left){
			directions[0] = Vector3.left;
			directions[1] = Vector3.back;
			directions[2] = Vector3.right;
			directions[3] = Vector3.forward;
		}

		RaycastHit hit;

		if(Physics.Raycast(standard[1], Vector3.up, out hit, blockSize, layerMask))
			array[1] = hit.transform;

		for(int i=0; i<3; i++){
			for(int j=0; j<4; j++){
				if(Physics.Raycast(standard[i], directions[j], out hit, blockSize, layerMask)){
					array[2+4*i+j] = hit.transform;
					//print(2+4*i+j + " j : " + j);
				}
			}
		}
	}
	private void updateBlockForPickedUpBlock(Transform[] array, Transform baseBlock){
		Debug.Log("updateBlockForPickedUpBlock");
		cleanBlock(array);
		SearchBlockForPickedUpBlock(baseBlock, array);
		// for(int i=0; i<array.Length; i++){
		// 	if(array[i] !=null)
		// 	Debug.Log(i + " - blocks number : " + array[i]);
		// }
	}
	private void SearchBlockForPickedUpBlock(Transform baseBlock, Transform[] array) {
		
		Vector3 standard = new Vector3(baseBlock.position.x, baseBlock.position.y + 1, baseBlock.position.z);

		Vector3[] directions = new Vector3[4];

		directions[0] = Vector3.forward;
		directions[1] = Vector3.left;
		directions[2] = Vector3.back;
		directions[3] = Vector3.right;

		RaycastHit hit;

		if(Physics.Raycast(baseBlock.position, Vector3.up, out hit, blockSize, layerMask))
			array[0] = hit.transform;

		for(int i=0; i<4; i++){
			if(Physics.Raycast(standard, directions[i], out hit, blockSize, layerMask)){
				array[1+i] = hit.transform;
			}
		}
	}
	private void updateBlockForHangingBlock(Transform[] array, Transform baseBlock, Vector3 nowForward){
		Debug.Log("updateBlockForHangingBlock");
		cleanBlock(array);
		SearchBlockForHangingBlock(baseBlock, array, nowForward);
		// for(int i=0; i<array.Length; i++){
		// 	if(array[i] !=null)
		// 	Debug.Log(i + " - blocks number : " + array[i]);
		// }
	}

	private void SearchBlockForHangingBlock(Transform baseBlock, Transform[] array, Vector3 nowForward) {
		array[0] = baseBlock;

		Vector3[] standard = new Vector3[2];
		standard[0] = baseBlock.position;
		standard[1] = baseBlock.position - nowForward;

		Vector3[] directions = new Vector3[4];
		directions[3] = Vector3.down;

		if(nowForward == Vector3.forward){
			directions[0] = Vector3.up;
			directions[1] = Vector3.left;
			directions[2] = Vector3.right;
		}
		if(nowForward == Vector3.back){
			directions[0] = Vector3.up;
			directions[1] = Vector3.right;
			directions[2] = Vector3.left;
		}
		if(nowForward == Vector3.right){
			directions[0] = Vector3.up;
			directions[1] = Vector3.forward;
			directions[2] = Vector3.back;
		}
		if(nowForward == Vector3.left){
			directions[0] = Vector3.up;
			directions[1] = Vector3.back;
			directions[2] = Vector3.forward;
		}

		RaycastHit hit;

		if(Physics.Raycast(standard[1], directions[3], out hit, blockSize, layerMask)){
			array[7] = hit.transform;
		}

		for(int i=0; i<2; i++){
			for(int j=0; j<3; j++){
				if(Physics.Raycast(standard[i], directions[j], out hit, blockSize, layerMask)){
					array[1+3*i+j] = hit.transform;
					//print(1+3*i+j + " j : " + j);
				}
			}
		}
	}


	private Vector3 checkDirectionAboutRight(Vector3 nowForward){
		Vector3 buf = Vector3.zero;
		if(nowForward == Vector3.forward){
			buf = Vector3.right;
		}else if(nowForward == Vector3.back){
			buf = Vector3.left;
		}else if(nowForward == Vector3.right){
			buf = Vector3.back;
		}else if(nowForward == Vector3.left){
			buf = Vector3.forward;
		}
		return buf;
	}

	private int searchQuadrant(Vector3 playerToHangingBlockPosition){
		Vector3 buf = playerToHangingBlockPosition;
		int quadrant = 0;
		if(buf.x > 0 && buf.z > 0)
			quadrant = 1;
		if(buf.x < 0 && buf.z > 0)
			quadrant = 2;
		if(buf.x < 0 && buf.z < 0)
			quadrant = 3;
		if(buf.x > 0 && buf.z < 0)
			quadrant = 4;	
		return quadrant;
	}

	private float correctHangNowDegree(Vector3 hangNowForward, int quadrant, float hangNowDegreeBuf){
		float hangNowDegree = -1;
		if(hangNowForward == Vector3.left){
			if(quadrant == 1){
				hangNowDegree = hangNowDegreeBuf;
			}
			if(quadrant == 4){
				hangNowDegree = 360 - hangNowDegreeBuf;
			}
		}
		if(hangNowForward == Vector3.back){
			if(quadrant == 1){
				hangNowDegree = 90 - hangNowDegreeBuf;
			}
			if(quadrant == 2){
				hangNowDegree = 90 + hangNowDegreeBuf;
			}
		}
		if(hangNowForward == Vector3.right){
			if(quadrant == 2){
				hangNowDegree = 180 - hangNowDegreeBuf;
			}
			if(quadrant == 3){
				hangNowDegree = 180 + hangNowDegreeBuf;
			}
		}
		if(hangNowForward == Vector3.forward){
			if(quadrant == 3){
				hangNowDegree = 270 - hangNowDegreeBuf;
			}
			if(quadrant == 4){
				hangNowDegree = 270 + hangNowDegreeBuf;
			}
		}
		return hangNowDegree;
	}
	private Vector3 searchHangGoal(float degree, Transform hangingBlock){
		return new Vector3(Mathf.Cos((degree)* Mathf.Deg2Rad) * blockToPlayerDistance + hangingBlock.position.x, hangingBlock.position.y, Mathf.Sin((degree)* Mathf.Deg2Rad) * blockToPlayerDistance + hangingBlock.position.z);
	}
}
