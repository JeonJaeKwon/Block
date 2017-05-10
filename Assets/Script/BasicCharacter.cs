using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BasicCharacterControl))]
public class BasicCharacter : MonoBehaviour {
	public float grabVelocity = 5f;
	public float desendVelocity = 0.02f;
	public float climbVelocity = 3f;
	public float hangVelocity = 3f;
	public float pullVelocity = 0.02f;
	public float pushVelocity = 0.02f;
	public float fallVerocity = 10f;
	public float walkVerocity = 3f;
	public float hangToDesendVerocity = 0.2f;
	public float hangToWalkVerocity = 0.2f;
	public float hangingWalkVerocity = 1f;
	public float hangToCrossThisVelocity = 1f;
	public float hangToCrossNextToVelocity = 1f;

	//from external
	Animator m_Animator;
	Rigidbody m_rigid;
	CapsuleCollider m_Capsule;
	BasicCharacterControl playerController; //process data on rotate and moveInput  
	private LayerMask layerMask;
	private float playerHalfHeigh;
	private float playerWidth;
	private float blockSize;
	//default
	private Transform[] totalBlocks; // totalBlocks : [0~11],[12~23],[24~35] // [1]->push base, [4]->frontBlock's base, [7]->player's base, [10]->pull base, [12]->push block, [14]-> front's left block, [15]->front block, [16]->front's right block, [17]->player left block, [18]->player, [19]->player right block, [21]->pull block, [28]->front's up, [31]->player's up
	//idle
	private bool idle; // when idle
	//walk
	private bool walk; // when walk
	private bool walkToHangAction;
	private float walkDecisionDistance;
	//grab block
	private bool grabMove;
	private bool grab; //when grab
	private Transform pickedUpBlock; 
	private Vector3 WhenPickedUpBlockPosition;
	private bool frontArea;
	private bool baseGrabBlockArea;
	private bool backArea;
	private bool baseBackArea;
	//grab action
	private bool grabActionReady;
	Vector3 grabNow;
	Vector3 grabGoal;
	private bool grabAction;
	private float grabActionAnimTimer;
	private Vector3 grabNowForward;
	
	//desend
	private bool desend; // when desend
	//pull
	private bool pull; // when grab, pull
	private bool pullToHangAction;
	//push
	private bool push;// when grab, push
	private bool pushToHangAction;
	//hang
	private bool hang; // when hang
	private int hangRotate;
	private float hangDecisionDistance;
	private float blockToPlayerDistance;
	private float maximumDegree; //when hang state, player can move maximum degree from block 
	private float minimumDegree;
	private float  safeDegree;

	private int quadrant;
	private float hangNowDegree;
	private Vector3 hangToCrossNow;
	private Vector3 hangMidGoal;
	private Vector3 hangGoal;
	private int classifyHangingBlockNumber;

	private Vector3 hangNowForward;
	private Vector3 hangNowRight;
	private Transform hangingBlock;
	//hang action
	private bool hangActionReady;
	Vector3 hangActionNow;
	Vector3 hangActionMidGoal;
	Vector3 hangActionGoal;
	private bool hangAction;
	private float hangActionAnimTimer;
	private bool hangActionFinish;
	private bool hangToWalkActionReady;
	Vector3 hangToWalkNow;
	Vector3 hangToWalkGoal;
	private float hangToWalkActionReadyAnimTimer;
	private bool hangToWalkActionFinish;
	//hang actions this
	private bool hangToCrossThis;
	private float hangToCrossThisAnimTimer;
	private bool hangToCrossThisSecond;
	private bool hangToCrossThisThird;
	private float hangToCrossThisThirdAnimTimer;
	private bool hangToCrossThisRotationCheck;
	private bool hangToCrossThisFinish;
	//hang actions next to
	private bool hangToCrossNextTo;
	private float hangToCrossNextToAnimTimer;
	private bool hangToCrossNextToFinish;
	//hang actions another
	private bool hangToCrossAnother;
	private bool hangToCrossAnotherSecond;
	private float hangToCrossAnotherSecondAnimTimer;
	private bool hangToCrossAnotherRotationCheck;
	private bool hangToCrossAnotherFinish;
	private float hangToCrossAnotherDistance;
	//climb
	private bool climbReady; //when climb
	private Vector3 climbNow; 
	private Vector3 climbMidGoal;	
	private Vector3 climbGoal;
	private bool climbFirst;
	private float climbFirstAnimTimer;
	private bool climbSecond;
	private float climbSecondAnimTimer;	
	private float climbDecisionDistance;
//end

	void Start () {
		// m_Animator = GetComponent<Animator>();
		playerController = GetComponent<BasicCharacterControl> ();
		m_Capsule = GetComponent<CapsuleCollider> ();
		playerHalfHeigh = m_Capsule.height * 0.5f;
		playerWidth = m_Capsule.radius;
		blockSize = 1f; //block prefab's size
		layerMask = -1 - ((1 << LayerMask.NameToLayer("Player"))); ; 
		totalBlocks = new Transform[36];

		

		climbDecisionDistance = playerWidth * 0.5f;
		walkDecisionDistance = playerHalfHeigh * 1.5f;
		hangDecisionDistance = playerHalfHeigh * 1.5f;

		idle = true;

		blockToPlayerDistance = (blockSize + playerWidth) * 0.5f;
		maximumDegree = Vector2.Angle(new Vector2(0,1), new Vector2(blockSize * 0.5f, blockToPlayerDistance));
		minimumDegree = Vector2.Angle(new Vector2(0,1), new Vector2(blockSize * 0.5f, blockToPlayerDistance + playerWidth * 0.5f));
		safeDegree = (maximumDegree - minimumDegree) * 2;
		// Debug.Log("maximumDegree : " + maximumDegree);
		// Debug.Log("minimumDegree : " + minimumDegree);

 		hangToCrossAnotherDistance = 0.05f;
 		m_rigid = GetComponent<Rigidbody>();
		m_rigid.isKinematic = false;
		m_rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
	
		
	}
	
	public void Move(Vector3 move, bool grab, bool up, bool down, bool right, bool left) {
		Ray forwardRay = new Ray(transform.position, transform.forward); //ray from player
		Ray downRay = new Ray(transform.position, -transform.up); //ray from player
		RaycastHit hit;
		Debug.DrawRay(forwardRay.origin, forwardRay.direction * playerWidth, Color.red);
		Debug.DrawRay(downRay.origin, downRay.direction, Color.red);

		if (move.magnitude > 1f) move.Normalize();
		move = transform.InverseTransformDirection(move);
		// move = Vector3.ProjectOnPlane(move, Vector3.down);
		// print("tt : " + move);

		if(hang || hangActionReady || hangAction || hangActionFinish || hangToCrossAnother || hangToCrossAnotherSecond || hangToCrossThisFinish || hangToWalkActionReady || hangToWalkActionFinish|| hangToCrossThis || hangToCrossThisSecond || hangToCrossThisThird || hangToCrossNextToFinish || hangToCrossNextTo || hangToCrossAnotherFinish || hangToCrossAnother){
			// m_rigid.isKinematic = true;
			Physics.gravity = Vector3.zero;
			m_rigid.velocity = Vector3.zero;
		}else{
			// m_rigid.isKinematic = false;
			Physics.gravity = new Vector3(0, -9.81f, 0);
		}

		if(idle){
			Debug.Log("idle");
			idle = false;
			desend = true;
		}
		if(desend){
			Debug.Log("desend");
			if(Physics.Raycast(downRay, out hit, playerHalfHeigh, layerMask)){
				desend = false;
				walk = true;
			}else{
				transform.position -= Vector3.up * 0.1f;
			}
		}
		if(walk){
            Debug.Log("walk");
			//check to need update blocks
			if(Physics.Raycast(downRay, out hit, walkDecisionDistance, layerMask)){
				updateTotalBlocksForDown(totalBlocks, hit.transform, transform.forward);
#if !anim	
				//when walk player's animation
				transform.position += move * walkVerocity * Time.deltaTime;
				transform.Rotate(0,playerController.PlayerRotate(transform.forward),0);
				//end animation
#endif
			}
			//for climb
			if (Physics.Raycast(forwardRay, out hit, climbDecisionDistance, layerMask)){
				updateTotalBlocksForFront(totalBlocks, hit.transform, transform.forward);			
				if(!totalBlocks[28] && !totalBlocks[31]){
					walk = false;
					climbReady = true;
				}
			}
			// for hang
			else if(!Physics.Raycast(downRay, hangDecisionDistance, layerMask)){
				hangNowForward = -transform.forward;
				hangNowRight = checkDirectionAboutRight(hangNowForward);
				hangingBlock = totalBlocks[7];
				updateTotalBlocksForFront(totalBlocks,hangingBlock,hangNowForward);

				walk = false;
				walkToHangAction = true;
				hangActionReady = true;
			}
			
			//for grab
			else if (Input.GetButton("Fire1")){
				if (!grabActionReady && totalBlocks[16]){
					pickedUpBlock = totalBlocks[16];
					WhenPickedUpBlockPosition = totalBlocks[16].position;
					grabNowForward = transform.forward;

					walk = false;
					grabActionReady = true;
				}
			}
		}
		if(hangActionReady){
			Debug.Log("hangAction");
			hangActionNow = transform.position;
			hangActionMidGoal = hangingBlock.position - hangNowForward * (blockSize + playerWidth) * 0.5f + Vector3.up;
			hangActionGoal = hangingBlock.position - hangNowForward * (blockSize + playerWidth) * 0.5f;
			// print(hangActionNow + " " + hangActionGoal);
#if !rotate
			if(walkToHangAction || pushToHangAction){
				transform.Rotate(0,180,0);
			}else{
				transform.Rotate(0,0,0);
			}
#endif
			walkToHangAction = false;
			pullToHangAction = false;
			pushToHangAction = false;
			hangActionReady = false;
			hangAction = true;
		}
		if(hangAction){
#if !hangAnim
			hangActionAnimTimer += Time.deltaTime * hangVelocity;
			transform.position = Vector3.Lerp(hangActionNow, hangActionGoal, hangActionAnimTimer);
#endif	
			if(hangActionAnimTimer > 1){
				hangActionAnimTimer = 0;
				hangAction = false;
				hangActionFinish = true;
			}
		}
		if(hangActionFinish){
			if(totalBlocks[7]){
				hangActionFinish = false;
				hangToWalkActionReady = true;

// #if !hangToWalkAnim
// 				// transform.position = transform.position + -hangNowForward * Time.deltaTime;
// 				transform.position = transform.position + -hangNowForward * 0.1f;
// #endif
// 				hangActionFinish = false;
// 				walk = true;
			}else{
				hangActionFinish = false;
				hang = true;
			}
		}
	
		if(hangToWalkActionReady){
			print("hangToWahangToWalkActionReadylkGoal");
			hangToWalkNow = transform.position;
			hangToWalkGoal = transform.position - hangNowForward * 0.1f;
			print("hangToWalkGoal : " + hangToWalkGoal);
			hangToWalkActionReady = false;
			hangToWalkActionFinish = true;
		}
		if(hangToWalkActionFinish){
			print("hangToWalkActionFinish");
			hangToWalkActionReadyAnimTimer += hangToWalkVerocity;
#if !hangToCrossThisAnim
			transform.position = Vector3.Lerp(hangToWalkNow, hangToWalkGoal, hangToWalkActionReadyAnimTimer);
#endif	
			if(hangToWalkActionReadyAnimTimer > 1){
				hangToWalkActionReadyAnimTimer = 0;
				hangToWalkActionFinish = false;
				walk = true;
			}
		}
		if(hang){
			Debug.Log("hang");
			hangRotate = 0;
			hangToCrossNow = transform.position;
			hangMidGoal = Vector3.zero;
			hangGoal = Vector3.zero;
			// print("nowForward" + hangNowForward);
			// print("hangNowRight" + hangNowRight);
			//check degree for classify hang to this block or other block 

			classifyHangingBlockNumber = 0;
			quadrant = searchQuadrant(transform.position - hangingBlock.position);
			float hangNowDegreeBuf = Vector2.Angle(new Vector2(-hangNowForward.x, -hangNowForward.z), new Vector2(transform.position.x - hangingBlock.position.x, transform.position.z - hangingBlock.position.z));
			hangNowDegree = correctHangNowDegree(hangNowForward, quadrant, hangNowDegreeBuf);

			//print(hangNowDegree);
			// print(hangNowForward);
			// print(hangingBlock.position);

			//degree is more than maximum degree, check new rotation and position
			if(hangNowForward == Vector3.left && hangNowDegree > maximumDegree - safeDegree && hangNowDegree < 90
			|| hangNowForward == Vector3.left && hangNowDegree < 360 - maximumDegree + safeDegree && hangNowDegree > 270
			|| hangNowForward == Vector3.back && hangNowDegree > 90 + maximumDegree - safeDegree && hangNowDegree < 180
			|| hangNowForward == Vector3.back && hangNowDegree < 90 - maximumDegree + safeDegree && hangNowDegree > 0
			|| hangNowForward == Vector3.right && hangNowDegree > 180 + maximumDegree - safeDegree && hangNowDegree < 270
			|| hangNowForward == Vector3.right && hangNowDegree < 180 - maximumDegree + safeDegree && hangNowDegree > 90
			|| hangNowForward == Vector3.forward && hangNowDegree > 270 + maximumDegree - safeDegree && hangNowDegree < 360
			|| hangNowForward == Vector3.forward && hangNowDegree < 270 - maximumDegree + safeDegree && hangNowDegree > 180
			){
				if(hangNowForward == Vector3.left && hangNowDegree > maximumDegree - safeDegree && hangNowDegree < 90){
					if(totalBlocks[17] == null && totalBlocks[20] == null){
						hangMidGoal = searchHangMidGoal(45, hangingBlock);
						hangGoal = searchHangGoal(90 - maximumDegree + safeDegree * 1.5f, hangingBlock);
						hangRotate = -90;

						hang = false;
						hangToCrossThis = true;
					}
					else if(totalBlocks[17] != null && totalBlocks[20] == null){
						hangGoal = searchHangGoal(360 - maximumDegree + safeDegree * 1.5f, totalBlocks[17]);
						classifyHangingBlockNumber = 17;

						hang = false;
						hangToCrossNextTo = true;
					}
					else if(totalBlocks[20]!= null){
						hangGoal = searchHangGoal(270 - maximumDegree + safeDegree * 1.5f, totalBlocks[20]);
						classifyHangingBlockNumber = 20;
						hangRotate = 90;

						hang = false;
						hangToCrossAnother = true;
					}
				}else if(hangNowForward == Vector3.left && hangNowDegree < 360 - maximumDegree + safeDegree && hangNowDegree > 270){
					if(totalBlocks[15] == null && totalBlocks[18] == null){
						hangMidGoal = searchHangMidGoal(360 - 45, hangingBlock);
						hangGoal = searchHangGoal(270 + maximumDegree - safeDegree * 1.5f, hangingBlock);
						classifyHangingBlockNumber = 15;
						hangRotate = 90;

						hang = false;
						hangToCrossThis = true;
					}
					else if(totalBlocks[15] != null && totalBlocks[18] == null){
						hangGoal = searchHangGoal(0 + maximumDegree - safeDegree * 1.5f, totalBlocks[15]);
						classifyHangingBlockNumber = 15;

						hang = false;
						hangToCrossNextTo = true;
					}
					else if(totalBlocks[18] != null){
						hangGoal = searchHangGoal(90 + maximumDegree - safeDegree * 1.5f, totalBlocks[18]);
						classifyHangingBlockNumber = 18;
						hangRotate = -90;

						hang = false;
						hangToCrossAnother = true;
					}
				}
				if(hangNowForward == Vector3.back && hangNowDegree > 90 + maximumDegree - safeDegree && hangNowDegree < 180){
					if(totalBlocks[17] == null && totalBlocks[20] == null){
						hangMidGoal = searchHangMidGoal(90 + 45, hangingBlock);
						hangGoal = searchHangGoal(180 - maximumDegree + safeDegree * 1.5f, hangingBlock);
						hangRotate = -90;

						hang = false;
						hangToCrossThis = true;
					}
					else if(totalBlocks[17] != null && totalBlocks[20] == null){
						hangGoal = searchHangGoal(90 - maximumDegree + safeDegree * 1.5f, totalBlocks[17]);
						classifyHangingBlockNumber = 17;

						hang = false;
						hangToCrossNextTo = true;
					}
					else if(totalBlocks[20] != null){
						hangGoal = searchHangGoal(360 - maximumDegree + safeDegree * 1.5f, totalBlocks[20]);
						classifyHangingBlockNumber = 20;
						hangRotate = 90;

						hang = false;
						hangToCrossAnother = true;
					}
				}else if(hangNowForward == Vector3.back && hangNowDegree < 90 - maximumDegree + safeDegree && hangNowDegree > 0){
					if(totalBlocks[15] == null && totalBlocks[18] == null){
						hangMidGoal = searchHangMidGoal(90 - 45, hangingBlock);
						hangGoal = searchHangGoal(0 + maximumDegree - safeDegree * 1.5f, hangingBlock);
						classifyHangingBlockNumber = 15;
						hangRotate = 90;

						hang = false;
						hangToCrossThis = true;
					}
					else if(totalBlocks[15] != null && totalBlocks[18] == null){
						hangGoal = searchHangGoal(90 + maximumDegree - safeDegree * 1.5f, totalBlocks[15]);
						classifyHangingBlockNumber = 15;

						hang = false;
						hangToCrossNextTo = true;
					}
					else if(totalBlocks[18] != null){
						hangGoal = searchHangGoal(180 + maximumDegree - safeDegree * 1.5f, totalBlocks[18]);
						classifyHangingBlockNumber = 18;
						hangRotate = -90;

						hang = false;
						hangToCrossAnother = true;
					}
				}
				if(hangNowForward == Vector3.right && hangNowDegree > 180 + maximumDegree - safeDegree && hangNowDegree < 270){
					if(totalBlocks[17] == null && totalBlocks[20] == null){
						hangMidGoal = searchHangMidGoal(180 + 45, hangingBlock);
						hangGoal = searchHangGoal(270 - maximumDegree + safeDegree * 1.5f, hangingBlock);
						hangRotate = -90;

						hang = false;
						hangToCrossThis = true;
					}
					else if(totalBlocks[17] != null && totalBlocks[20] == null){
						hangGoal = searchHangGoal(180 - maximumDegree + safeDegree * 1.5f, totalBlocks[20]);
						classifyHangingBlockNumber = 17;

						hang = false;
						hangToCrossNextTo = true;
					}
					else if(totalBlocks[20] != null){
						hangGoal = searchHangGoal(90 - maximumDegree + safeDegree * 1.5f, totalBlocks[20]);
						classifyHangingBlockNumber = 20;
						hangRotate = 90;

						hang = false;
						hangToCrossAnother = true;
					}
				}else if(hangNowForward == Vector3.right && hangNowDegree < 180 - maximumDegree + safeDegree && hangNowDegree > 90){
					if(totalBlocks[15] == null && totalBlocks[18] == null){
						hangMidGoal = searchHangMidGoal(180 - 45, hangingBlock);
						hangGoal = searchHangGoal(90 + maximumDegree - safeDegree * 1.5f, hangingBlock);
						classifyHangingBlockNumber = 15;
						hangRotate = 90;

						hang = false;
						hangToCrossThis = true;
					}
					else if(totalBlocks[15] != null && totalBlocks[18] == null){
						hangGoal = searchHangGoal(180 + maximumDegree - safeDegree * 1.5f, totalBlocks[15]);
						classifyHangingBlockNumber = 15;

						hang = false;
						hangToCrossNextTo = true;
					}
					else if(totalBlocks[18] != null){
						hangGoal = searchHangGoal(270 + maximumDegree - safeDegree * 1.5f, totalBlocks[18]);
						classifyHangingBlockNumber = 18;
						hangRotate = -90;

						hang = false;
						hangToCrossAnother = true;
					}
				}
				if(hangNowForward == Vector3.forward && hangNowDegree > 270 + maximumDegree - safeDegree && hangNowDegree < 360){
					if(totalBlocks[17] == null && totalBlocks[20] == null){
						hangMidGoal = searchHangMidGoal(270 + 45, hangingBlock);
						hangGoal = searchHangGoal(360 - maximumDegree + safeDegree * 1.5f, hangingBlock);
						hangRotate = -90;

						hang = false;
						hangToCrossThis = true;
					}
					else if(totalBlocks[17] != null && totalBlocks[20] == null){
						hangGoal = searchHangGoal(270 - maximumDegree + safeDegree * 1.5f, totalBlocks[17]);
						classifyHangingBlockNumber = 17;

						hang = false;
						hangToCrossNextTo = true;
					}
					else if(totalBlocks[20] != null){
						hangGoal = searchHangGoal(180 - maximumDegree + safeDegree * 1.5f, totalBlocks[20]);
						classifyHangingBlockNumber = 20;
						hangRotate = 90;

						hang = false;
						hangToCrossAnother = true;
					}
				}else if(hangNowForward == Vector3.forward && hangNowDegree < 270 - maximumDegree + safeDegree && hangNowDegree > 180){
					if(totalBlocks[15] == null && totalBlocks[18] == null){
						hangMidGoal = searchHangMidGoal(270 - 45, hangingBlock);
						hangGoal = searchHangGoal(180 + maximumDegree - safeDegree * 1.5f, hangingBlock);
						classifyHangingBlockNumber = 15;
						hangRotate = 90;

						hang = false;
						hangToCrossThis = true;
					}
					else if(totalBlocks[15] != null && totalBlocks[18] == null){
						hangGoal = searchHangGoal(270 + maximumDegree - safeDegree * 1.5f, totalBlocks[15]);
						classifyHangingBlockNumber = 15;

						hang = false;
						hangToCrossNextTo = true;
					}
					else if(totalBlocks[18] != null){
						hangGoal = searchHangGoal(0 + maximumDegree - safeDegree * 1.5f, totalBlocks[18]);
						classifyHangingBlockNumber = 18;
						hangRotate = -90;

						hang = false;
						hangToCrossAnother = true;
					}
					print("hangToCrossNow : " + hangToCrossNow + " hangMidGoal : " + hangMidGoal + " hangGoal : " + hangGoal);
				// //for climb
				}
			}else if(up){
				if(totalBlocks[28] == null && totalBlocks[31] == null){
					updateTotalBlocksForFront(totalBlocks, hangingBlock, transform.forward);

					hang = false;
					climbReady = true;
				}
			//for fall
			}else if(totalBlocks[7]){
				hang = false;
				hangToWalkActionReady = true;
			}else if(down){
				hang = false;
				desend = true;
			//for hanging walk
			}else
#if !hangingAnim
				transform.position += playerController.MoveInputHanging(hangNowForward) * hangingWalkVerocity * Time.deltaTime;
				transform.Rotate(0, 0 ,0);
#endif
		}
		if(hangToCrossThis){
			hangToCrossThisAnimTimer += Time.deltaTime * hangToCrossThisVelocity;
#if !hangToCrossThisAnim
			transform.position = Vector3.Lerp(hangToCrossNow, hangMidGoal, hangToCrossThisAnimTimer);
#endif	
			if(hangToCrossThisAnimTimer > 1){
				print("hangToCrossThisAnimTimer : " + hangToCrossThisAnimTimer);
				hangToCrossThisAnimTimer = 0;
				hangToCrossThis = false;
				hangToCrossThisSecond = true;
			}
		}
		if(hangToCrossThisSecond){
#if !hangToCrossThisAnim
			transform.Rotate(0,hangRotate,0);
#endif			
			hangToCrossThisSecond = false;
			hangToCrossThisThird = true;
		}
		if(hangToCrossThisThird){
			hangToCrossThisThirdAnimTimer += Time.deltaTime * hangToCrossThisVelocity;
#if !hangToCrossThisAnim
			transform.position = Vector3.Lerp(hangMidGoal, hangGoal, hangToCrossThisThirdAnimTimer);
#endif			
			if(hangToCrossThisThirdAnimTimer > 1){
				hangToCrossThisThirdAnimTimer = 0;
				hangToCrossThisThird = false;
				hangToCrossThisFinish = true;
			}
		}
		if(hangToCrossThisFinish){
			print("hangToCrossThisFinish");
			hangNowForward = transform.forward;
			hangNowRight = checkDirectionAboutRight(hangNowForward);
			hangingBlock = totalBlocks[16];
			updateTotalBlocksForFront(totalBlocks, hangingBlock, hangNowForward);
			// Debug.Log("hangNowForward : " + hangNowForward);
			// Debug.Log("hangNowRight : " + hangNowRight);

			hangToCrossThisFinish = false;
			hang = true;
		}
		if(hangToCrossNextTo){
			Debug.Log("hangToCrossNextTo");
			hangToCrossNextToAnimTimer += Time.deltaTime * hangToCrossNextToVelocity;
#if !hangToCrossNextToAnim
			transform.position = Vector3.Lerp(hangToCrossNow, hangGoal, hangToCrossNextToAnimTimer);
#endif			
			if(hangToCrossNextToAnimTimer > 1){
				hangToCrossNextToAnimTimer = 0;
				hangToCrossNextTo = false;
				hangToCrossNextToFinish = true;
			}
		}
		if(hangToCrossNextToFinish){
			print("hangToCrossNextToFinish");
			hangNowForward = transform.forward;
			hangNowRight = checkDirectionAboutRight(hangNowForward);
			hangingBlock = totalBlocks[classifyHangingBlockNumber];
			updateTotalBlocksForFront(totalBlocks, hangingBlock, hangNowForward);
			// Debug.Log("hangNowForward : " + hangNowForward);
			// Debug.Log("hangNowRight : " + hangNowRight);

			hangToCrossNextToFinish = false;
			hang = true;
		}
		if(hangToCrossAnother){
			Debug.Log("hangToCrossAnother");
#if !hangToCrossAnotherAnim
			transform.Rotate(0,hangRotate,0);
#endif
			hangToCrossAnother = false;
			hangToCrossAnotherSecond = true;
		}
		if(hangToCrossAnotherSecond){
			hangToCrossAnotherSecondAnimTimer += Time.deltaTime * hangToCrossThisVelocity;
#if !hangToCrossAnotherSecondAnim
			transform.position = Vector3.Lerp(hangToCrossNow, hangGoal, hangToCrossAnotherSecondAnimTimer);
#endif			
			if(hangToCrossAnotherSecondAnimTimer > 1){
				hangToCrossAnotherSecondAnimTimer = 0;
				hangToCrossAnotherSecond = false;
				hangToCrossAnotherFinish = true;
			}
		}
		if(hangToCrossAnotherFinish){
			print("hangToCrossOtherFinish");
			hangNowForward = transform.forward;
			hangNowRight = checkDirectionAboutRight(hangNowForward);
			hangingBlock = totalBlocks[classifyHangingBlockNumber];
			updateTotalBlocksForFront(totalBlocks, hangingBlock, hangNowForward);
			// Debug.Log("hangNowForward : " + hangNowForward);
			// Debug.Log("hangNowRight : " + hangNowRight);

			hangToCrossAnotherFinish = false;
			hang = true;
		}
		if(grabActionReady){
			Debug.Log("grabAction");
			grabNow = transform.position;
			grabGoal = WhenPickedUpBlockPosition - grabNowForward * (blockSize + playerWidth) * 0.5f;
			
			grabActionReady = false;
			grabAction = true;
		}
		if(grabAction){
			grabActionAnimTimer += Time.deltaTime * grabVelocity;
#if !grabActionAnim			
			transform.position = Vector3.Lerp(grabNow, grabGoal, grabActionAnimTimer);
#endif			
			if(grabActionAnimTimer > 1){
				grabActionAnimTimer = 0;
				grabAction = false;
				grab = true;
			}
		}
		if(grab){
			Debug.Log("grab");
			if(totalBlocks[22] != null)
				backArea = true;
			if (totalBlocks[10] != null)
				baseBackArea = true;
			if (totalBlocks[13])
				frontArea = true;
			if(totalBlocks[4] != null)
				baseGrabBlockArea = true;
			Debug.Log("backArea : " + backArea + ", baseBackArea : " + baseBackArea + ", frontArea : " + frontArea + ", baseFrontArea : " + baseGrabBlockArea);
			
			grab = false;
			grabMove = true;
		}

		if(grabMove){
			Debug.Log("grabMove");
			if (right){
                if (transform.forward == -Vector3.right){
                    pull = true;
				}
			}
            if (left){
                if (transform.forward == -Vector3.left){
                    pull = true;
				}
			}
            if (up){
                if (transform.forward == -Vector3.forward){
                    pull = true;
				}
			}
            if (down){
                if (transform.forward == -Vector3.back){
                    pull = true;
				}
			}

			if (pickedUpBlock && right)
                if (transform.forward == Vector3.right)
                    push = true;
            if (pickedUpBlock && left)
                if (transform.forward == Vector3.left)
                    push = true;
            if (pickedUpBlock && up)
                if (transform.forward == Vector3.forward)
                    push = true;
            if (pickedUpBlock && down)
                if (transform.forward == Vector3.back)
                    push = true;

			if(right || left || up || down)
			grabMove = false;
		}

		if(pull){
			Debug.Log("pull");
			if(backArea){
				backArea = false;
				baseBackArea = false;
				frontArea = false;
				baseGrabBlockArea = false;

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
					baseGrabBlockArea = false;

					hangNowForward = transform.forward;
					hangNowRight = checkDirectionAboutRight(hangNowForward);
					hangingBlock = totalBlocks[7];
					updateTotalBlocksForFront(totalBlocks, hangingBlock, hangNowForward);
					// updateBlockForHangingBlock(blocksForHangingpBlock, hangingBlock, hangNowForward);
					
					pull = false;
					pullToHangAction = true;
					hangActionReady = true;
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
					baseGrabBlockArea = false;
					
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
				baseGrabBlockArea = false;

				push = false;
				walk = true;
			}else if(!frontArea && !baseGrabBlockArea){
				Vector3 now = transform.position;
				Vector3 goal = WhenPickedUpBlockPosition - Vector3.up - transform.forward * blockSize * 0.2f;
				Vector3 nowBlock = pickedUpBlock.position;
				Vector3 goalBlock = WhenPickedUpBlockPosition + grabNowForward;
				StartCoroutine(playerController.pushAction(now, goal, nowBlock, goalBlock, pickedUpBlock));
				if(pickedUpBlock.position == goalBlock){
					backArea = false;
					baseBackArea = false;
					frontArea = false;
					baseGrabBlockArea = false;

					hangNowForward = -transform.forward;
					hangNowRight = checkDirectionAboutRight(hangNowForward);
					hangingBlock = totalBlocks[7];
					updateTotalBlocksForFront(totalBlocks, hangingBlock, hangNowForward);
					// updateBlockForHangingBlock(blocksForHangingpBlock, hangingBlock, hangNowForward);

					push = false;
					pushToHangAction = true;
					hangActionReady = true;
				}
			}else if(!frontArea && baseGrabBlockArea){
				Vector3 now = transform.position;
				Vector3 goal = WhenPickedUpBlockPosition;
				Vector3 nowBlock = pickedUpBlock.position;
				Vector3 goalBlock = WhenPickedUpBlockPosition + grabNowForward;
				StartCoroutine(playerController.pushAction(now, goal, nowBlock, goalBlock, pickedUpBlock));
				if(pickedUpBlock.position == goalBlock){
					backArea = false;
					baseBackArea = false;
					frontArea = false;
					baseGrabBlockArea = false;

					push = false;
					walk = true;
				}
			}
		}
		if(climbReady){
			Debug.Log("Climb");
			climbNow = transform.position;
			climbMidGoal = totalBlocks[16].position + Vector3.up - transform.forward * (blockSize + playerWidth) * 0.5f;
			climbGoal = totalBlocks[16].position + Vector3.up;

			climbReady = false;
			climbFirst = true;
		}
		if(climbFirst){
			climbFirstAnimTimer += Time.deltaTime * climbVelocity;
#if !climbAnim			
			transform.position = Vector3.Slerp(climbNow, climbMidGoal, climbFirstAnimTimer);
#endif			
			if(climbFirstAnimTimer > 1){
				climbFirstAnimTimer = 0;
				climbFirst = false;
				climbSecond = true;
			}
		}
		if(climbSecond){
			climbSecondAnimTimer += Time.deltaTime * climbVelocity;
#if !climbAnim			
			transform.position = Vector3.Slerp(climbMidGoal, climbGoal, climbSecondAnimTimer);
#endif			
			if(climbSecondAnimTimer > 1){
				climbSecondAnimTimer = 0;
				climbSecond = false;
				walk = true;
			}
		}		
	}

	private void cleanBlock(Transform[] array) {
		for(int i=0; i<array.Length; i++)
		array[i] = null;
	}
	private void updateTotalBlocksForFront(Transform[] array, Transform baseBlock, Vector3 nowForward){
		// Debug.Log("updateBlocks");
		cleanBlock(array);
		SearchTotalBlockForFront(baseBlock, array, nowForward);
		// for(int i=0; i<array.Length; i++){
		// 	if(array[i] !=null)
		// 	Debug.Log(i + " - blocks number : " + array[i]);
		// }
	}
	private void SearchTotalBlockForFront(Transform baseBlock, Transform[] array, Vector3 nowForward) {
		//blocks[0] = baseBlock;

		Vector3[] standard = new Vector3[3];

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
		Vector3 standardPosition = baseBlock.position - directions[0];
		standard[0] = new Vector3(standardPosition.x, standardPosition.y - 1, standardPosition.z);
		standard[1] = new Vector3(standardPosition.x, standardPosition.y, standardPosition.z);
		standard[2] = new Vector3(standardPosition.x, standardPosition.y + 1, standardPosition.z);

		// print(standardPosition);
		RaycastHit hit;

		// if(Physics.Raycast(standard[1], Vector3.up, out hit, blockSize, layerMask))
		// 	array[1] = hit.transform;
		for(int k=0; k<3; k++){
			for(int i=0; i<4; i++){
				for(int j=0; j<3; j++){
					if(Physics.Raycast(standard[k]+directions[1] + directions[0] + directions[2] * (float)i + directions[3] * (float)j, nowForward, out hit, blockSize, layerMask)){
					array[(12 * k) + (3 * i) + j] = hit.transform;
					// print((12 * k) + (3 * i) + j + " : " + hit.transform.position);
					}
				}
			}
		}
	}

	private void updateTotalBlocksForDown(Transform[] array, Transform baseBlock, Vector3 nowForward){
		// Debug.Log("updateBlocks");
		cleanBlock(array);
		SearchTotalBlockForDown(baseBlock, array, nowForward);
		// for(int i=0; i<array.Length; i++){
		// 	if(array[i] !=null)
		// 	Debug.Log(i + " - blocks number : " + array[i]);
		// }
	}
	private void SearchTotalBlockForDown(Transform baseBlock, Transform[] array, Vector3 nowForward) {
		//blocks[0] = baseBlock;

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
		// print(baseBlock.position);
		
		RaycastHit hit;

		// if(Physics.Raycast(standard[1], Vector3.up, out hit, blockSize, layerMask))
		// 	array[1] = hit.transform;
		for(int k=0; k<3; k++){
			for(int i=0; i<4; i++){
				for(int j=0; j<3; j++){
					if(Physics.Raycast(standard[k]+directions[1] + directions[0] + directions[2] * (float)i + directions[3] * (float)j, nowForward, out hit, blockSize, layerMask)){
						array[(12 * k) + (3 * i) + j] = hit.transform;
						// print((12 * k) + (3 * i) + j + " : " + hit.transform.position);
						// print((11 * k) + (3 * i) + j + " : " +  (directions[1] + directions[0] * (float)3 + directions[2] * (float)i + directions[3] * (float)j));
					}
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
	private Vector3 searchHangMidGoal(float degree, Transform hangingBlock){
		return new Vector3(Mathf.Cos((degree)* Mathf.Deg2Rad) * (blockToPlayerDistance + playerWidth * 0.5f) + hangingBlock.position.x, hangingBlock.position.y, Mathf.Sin((degree)* Mathf.Deg2Rad) * (blockToPlayerDistance + playerWidth * 0.5f) + hangingBlock.position.z);
	}
}
