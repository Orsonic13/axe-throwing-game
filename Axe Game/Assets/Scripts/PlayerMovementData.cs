using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Run Data")] //Create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
public class PlayerMovementData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float gravityStrength;
    [HideInInspector] public float gravityScale;

    [Space(5)]
    public float fallGravityMult;
    public float maxFallSpeed;
    [Space(5)]
    public float fastFallGravityMult;
    public float maxFastFallSpeed;

    [Space(20)]

    [Header("Run")]
    public float runMaxSpeed; //target speed we want the player to reach
    public float runAcceleration; //time (approx) we want it to take the player to go from 0 to max speed
    public float runDecceleration; //time (approx) we want it to take the player to go from max speed to 0
    [HideInInspector] public float runAccelAmount; //the actual force (multplied with speedDif) applied to the player
    [HideInInspector] public float runDeccelAmount; //the actual force (multplied with speedDif) applied to the player
    [Space(5)]
    [Range(0.1f, 1)] public float accelInAir; //multipliers applied when airborne
    [Range(0.1f, 1)] public float deccelInAir;
    
    [Space(20)]

    [Header("Jump")]
    public float jumpHeight;
    public float jumpTimeToApex;
    [HideInInspector] public float jumpForce;

    [Header("Wall Jump")]

    [Header("BothJumps")]
    public float jumpCutGravityMult;
    [Range(0f, 1)] public float jumpHangGravityMult;
    public float jumpHangTimeThreshold;
    [Space(0.5f)]
	public float jumpHangAccelerationMult; 
	public float jumpHangMaxSpeedMult; 

    [Space(20)]
    
    [Header("Assists")]
    [Range(0.01f, 0.5f)] public float coyoteTime;
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;

    public float frictionAmount;


    private void OnValidate() 
    {
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        gravityScale = gravityStrength / Physics2D.gravity.y;

        //Calculate the acceleartion amount using amount = ((1/Time.fixedDeltaTime) * acceleration) / maxSpeed
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region Variable Ranges
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }
}
