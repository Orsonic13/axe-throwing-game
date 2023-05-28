using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public PlayerMovementData data;

    #region Variables
    //Components
    public Rigidbody2D rb;
    [SerializeField] AxeThrow axeThrow;

    PlayerInputAction inputAction;

    //State Parameters
    //Control the various actions that a player can perform at any time
    public bool isFacingRight { get; private set; }
    public bool isJumping { get; private set; }
    
    //Timers 
    public float lastOnGroundtTime { get; private set; }

    //Jump 
    private bool isJumpCut;
    private bool isJumpFalling; 

    //Input Parameters
    private Vector2 moveInput;
    public float lastPressedJumpTime;


    //Check Parameters
    [Header("Checks")] 
	[SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);

    //Layers & Tags
    [Header("Layers & Tags")]
	[SerializeField] private LayerMask groundLayer;
    #endregion

    private void Awake() 
    {
        inputAction = new PlayerInputAction();
    }

    private void Start() 
    {
        isFacingRight = true;
    }

    void OnEnable() 
    {
        inputAction.Enable();
        inputAction.Player.Movement.performed += OnMovementPerformed;
        inputAction.Player.Movement.canceled += OnMovementCanceled;
        inputAction.Player.Jump.performed += OnJumpPerformed;
        inputAction.Player.Jump.canceled += OnJumpCanceled;
    }
    void OnDisable() 
    {
        inputAction.Disable();
        inputAction.Player.Movement.performed -= OnMovementPerformed;
        inputAction.Player.Movement.canceled -= OnMovementCanceled;
        inputAction.Player.Jump.performed -= OnJumpPerformed;
        inputAction.Player.Jump.canceled -= OnJumpCanceled;
    }

    private void Update() 
    {
        #region Timers
        lastOnGroundtTime -= Time.deltaTime;
        lastPressedJumpTime -= Time.deltaTime;
        #endregion

        #region InputHandlers

        if(moveInput.x != 0) 
            CheckDirectionToFace(moveInput.x > 0);
        #endregion
    
    
        #region Collision Checks
        if(!isJumping) 
        {
            //Ground Check
            if(Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer) && !isJumping) //checks if set box overlaps with ground
                lastOnGroundtTime = 0.1f;

            
        }
        #endregion

        #region Jump Checks
        if(isJumping && rb.velocity.y < 0) 
        {
            isJumping = false;
        }
        if(lastOnGroundtTime > 0 && !isJumping) 
        {
            isJumpCut = false;

            if(!isJumping)
                isJumpFalling = false;
        }

        //Jump
        if(CanJump() && lastPressedJumpTime > 0) 
        {
            isJumping = true;
            isJumpCut = false;
            Jump(); 
        }
        #endregion
        
        #region Gravity
        //Higher Gravity if we've released the jump or are falling
        if(rb.velocity.y < 0 && moveInput.y < 0)
        {
            //Much higher gravity is holding down
            SetGravityScale(data.gravityScale * data.fastFallGravityMult);
            //Caps max fall speed, so when falling over large distances we don't accelerate too fast
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -data.maxFastFallSpeed));
        }
        else if(isJumpCut) 
        {
            SetGravityScale(data.gravityScale * data.jumpCutGravityMult);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -data.maxFallSpeed));
        }
        else if (isJumping && Mathf.Abs(rb.velocity.y) < data.jumpHangTimeThreshold)
        {
            SetGravityScale(data.gravityScale * data.jumpHangGravityMult);
        }
        else if(rb.velocity.y < 0) 
        {
            //Higher gravity if falling
            SetGravityScale(data.gravityScale * data.fallGravityMult);
            //Caps max fall speed
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -data.maxFallSpeed));
        }
        else
        {
            //Default gravity if standing on a platform or moving upwards
            SetGravityScale(data.gravityScale);
        }
        #endregion

    }

    private void FixedUpdate() 
    {
        Run(1);
    }

    #region Input Callbacks
    //Methods which handle input detected in Update()
    private void OnMovementPerformed(InputAction.CallbackContext context) 
    {
        moveInput.x = context.ReadValue<float>();
    }
    private void OnMovementCanceled(InputAction.CallbackContext context) 
    {
        moveInput = Vector2.zero;
    }

    public void OnJumpPerformed(InputAction.CallbackContext context) 
    {
        lastPressedJumpTime = data.jumpInputBufferTime;
    }
    public void OnJumpCanceled(InputAction.CallbackContext context) 
    {
        if(CanJumpCut())
            isJumpCut = true;
    }
    #endregion

    #region General Methods
    public void SetGravityScale(float scale) 
    {
        rb.gravityScale = scale;
    }
    #endregion

    //Movement Methods
    #region Run Methods
    private void Run(float lerpAmount) 
    {
        //Calculate the direction we want to go and move in our desired velocity
        float targetSpeed = moveInput.x  * data.runMaxSpeed;
        //We can reduce our air using Lerp(), this smooths chanes to air direction and speed
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

        #region Caluclate AccelRate
        float accelRate;

        //Gets and acceleration value based on if we're accelerating (includes turning) or deccelerating (stop). As well as applying a multiplier if we're air borne
        if(lastOnGroundtTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccelAmount : data.runDeccelAmount;
        else 
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccelAmount * data.accelInAir : data.runDeccelAmount * data.deccelInAir;
        #endregion

        #region Add Bonus Jump Apex Acceleration
        //Incrase air acceleration and maxSpeed when at the apex of a jump, makes the jump feel more natural
        if(isJumping && Mathf.Abs(rb.velocity.y) > data.jumpHangTimeThreshold) 
        {
            accelRate *= data.jumpHangAccelerationMult;
            targetSpeed *= data.jumpHangMaxSpeedMult;
        }
        #endregion

        //Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - rb.velocity.x;
		//Calculate force along x-axis to apply to thr player
		float movement = speedDif * accelRate;

        #region Friction
        //check if we're grounded  and that we're trying to stop (movement = 0)
        if(lastOnGroundtTime > 0 && Mathf.Abs(moveInput.x) < 0.01f) 
        {
            //either use friction amount (~0.2) or velocity
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(data.frictionAmount));
            //set to movement direction
            amount *= Mathf.Sign(rb.velocity.x);
            //applies force against movement direction
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        #endregion 

        //Convert this to a vector and apply to rigidbody
		rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Turn()
	{
		//stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale; 
		scale.x *= -1;
		transform.localScale = scale;

		isFacingRight = !isFacingRight;
        axeThrow.Turn();
	}
    #endregion

    #region Jump Methods
    private void Jump()
    {
        //Ensures we can't call Jump multiple times from one press
        lastPressedJumpTime = 0;
        lastOnGroundtTime = 0;

        #region Perform Jump
        //We increase the force applied if we're falling
        //This means we'll always feel like we jump the same amount
        //(setting the player's & velocity to 0 beforehand will likely work the same, but I have found this more elegant)
        float force = data.jumpForce;
        if(rb.velocity.y < 0) 
            force -= rb.velocity.y;

        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region Check Methods
    public void CheckDirectionToFace(bool isMovingRight) 
    {
        if(isMovingRight != isFacingRight) 
            Turn();
    }

    private bool CanJump() 
    {
        return lastOnGroundtTime > 0 && !isJumping;
    }

    private bool CanJumpCut() 
    {
        return isJumping && rb.velocity.y > 0;
    }
    #endregion

}
