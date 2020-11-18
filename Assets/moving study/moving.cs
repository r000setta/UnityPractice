using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moving : MonoBehaviour
{


    [SerializeField,Range(0f,10f)]
    float jumpHeight=2f;

    bool OnGround;
    Rigidbody body;

    [SerializeField,Range(0f,100f)]
    float maxSpeed=10f;

    [SerializeField,Range(0f,100f)]
    float maxAcceleration=10f,maxAirAcceleration=1f;

    [SerializeField,Range(0,5)]
    int maxAirJumps=0;

    int jumpPhase;

    bool desiredJump;

    Vector3 velocity,desiredVelocity;

    [SerializeField]
    Transform playerInputSpace=default;

    Vector3 upAxis;

    // Start is called before the first frame update

    private void Awake() {
        body=GetComponent<Rigidbody>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 playerInput;
        playerInput.x=Input.GetAxis("Horizontal");
        playerInput.y=Input.GetAxis("Vertical");
        playerInput=Vector2.ClampMagnitude(playerInput,1f);
        if(playerInputSpace){
            Vector3 forward=playerInputSpace.forward;
            forward.y=0f;
            forward.Normalize();
            Vector3 right=playerInputSpace.right;
            right.y=0;
            right.Normalize();
            desiredVelocity=(forward*playerInput.y+right*playerInput.x)*maxSpeed;
        }else{
            desiredVelocity=new Vector3(playerInput.x,0f,playerInput.y)*maxSpeed;
        }

        desiredJump|=Input.GetButtonDown("Jump");
    }

    private void FixedUpdate() {
        upAxis=-Physics.gravity.normalized;
        UpdateState();
        float acceleration=OnGround?maxAcceleration:maxAirAcceleration;
        float maxSpeedChange=acceleration*Time.deltaTime;
        velocity.x =
			Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
		velocity.z =
			Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        if(desiredJump){
            desiredJump=false;
            Jump();
        }

        body.velocity=velocity;
        OnGround=false;
    }

    void UpdateState(){
        velocity=body.velocity;
        if(OnGround){
            jumpPhase=0;
        }
    }

    void Jump(){
        if(OnGround||jumpPhase<maxAirJumps){
            jumpPhase++;
            float jumpSpeed=Mathf.Sqrt(-2f*Physics.gravity.y*jumpHeight);
            if(velocity.y>0f){
                jumpSpeed=Mathf.Max(jumpSpeed-velocity.y,0f);
            }
            velocity.y+=jumpSpeed;
        }
    }

    private void OnCollisionEnter(Collision other) {
        EvaluateCollision(other);
    }

    private void OnCollisionStay(Collision other) {
        EvaluateCollision(other);
    }

    void EvaluateCollision(Collision collision){
        for(int i=0;i<collision.contactCount;++i){
            Vector3 normal=collision.GetContact(i).normal;
            OnGround|=normal.y>=0.9f;
        }
    }
}
