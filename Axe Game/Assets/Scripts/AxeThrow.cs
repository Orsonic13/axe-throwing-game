using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AxeThrow : MonoBehaviour
{
    [SerializeField] Transform trans;
    [SerializeField] PlayerMovement playerMovement;

    PlayerInputAction inputAction;

    [SerializeField] GameObject axePref;
    public GameObject axeObj;
    Rigidbody2D axeRB;
    Axe axe; 
    Vector3 axeOffset = new Vector3(-0.5f, 0, 0);
    float axeOffsetValue = 0.5f;
    float axeSpeed = 100;
    float withdrawSpeed = 25;

    Vector3 offset = new Vector3(1, 0, 0);
    Vector3 playerDirection = new Vector3(1, 0, 0);
    Vector3 movementDirection;
    Vector3 velocity;
    [HideInInspector] public Vector3 targetLocation;

    [SerializeField] LineRenderer lineRenderer;

    public string axeState = "held"; //"held, attached, withdraw 

    bool isFacingRight;

    void Start() 
    {
        isFacingRight = true;
        offset = Vector3.right;
    }

    private void Awake() 
    {
        inputAction = new PlayerInputAction();
    }

    void OnEnable() 
    {
        inputAction.Enable();
        inputAction.Player.Axe.performed += UseAxe;
        inputAction.Player.LookUp.performed += LookUpPerformed;
        inputAction.Player.LookUp.canceled += LookUpCanceled;
    }

    void OnDisable() 
    {
        inputAction.Disable();
        inputAction.Player.Axe.performed -= UseAxe;
        inputAction.Player.LookUp.performed -= LookUpPerformed;
        inputAction.Player.LookUp.canceled -= LookUpCanceled;
    }

    void Update() 
    {
        if(lineRenderer.enabled == true) 
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, targetLocation);
        }
    }

/*
    void FixedUpdate() 
    {
        if(axeObj != null) 
        {
            if(axeState == "held") 
            {
                if(Mathf.Abs(axeRB.position.x) < Mathf.Abs(targetLocation.x)) 
                {
                    axeRB.velocity = movementDirection * axeSpeed;
                }
                else
                {
                    axeRB.velocity = Vector3.zero;
                    axeState = "attached";
                    axeObj.transform.position = targetLocation;
                }
            } 
        }
    }
    */

    void UseAxe(InputAction.CallbackContext context) 
    {
        if(context.performed) 
        {
            if(axeState == "held")
            {
                SpawnAxe();
            } 
            else if(axeState == "attached")
            {
                WithdrawAxe();
            }
        }
    }

    private void SpawnAxe() 
    {
        if(axeObj != null) 
        {
            Destroy(axeObj);
        }
        RaycastHit2D hit = Physics2D.Raycast(trans.position + offset, offset, 20);
        if(hit.collider != null) 
        {
            axeObj = Instantiate(axePref, trans.position + offset, Quaternion.identity);
            axeRB = axeObj.GetComponent<Rigidbody2D>();
            axe = axeObj.GetComponent<Axe>();
            movementDirection = playerDirection;

            if(hit.transform.tag == "attachable surface") 
            {
                targetLocation = new Vector3(hit.point.x + axeOffset.x, hit.point.y, 0);
                axeObj.transform.position = targetLocation;
                axeState = "attached";

                lineRenderer.enabled = true;
            }
            else if(hit.transform.tag == "nonattachable surface") 
            {
                WithdrawAxe();
            }
        }
    }

    void WithdrawAxe() 
    {
        Destroy(axeObj);
        lineRenderer.enabled = false;
        axeState = "held";
    }

    public void Turn() 
    {
        isFacingRight = playerMovement.isFacingRight;

        if(isFacingRight) 
        {
            axeOffset = new Vector3(-0.5f, 0, 0);
            offset = new Vector3(1, 0, 0);
            playerDirection = new Vector3(Mathf.Abs(playerDirection.x), playerDirection.y, playerDirection.z);
        }
        else  
        {
            axeOffset = new Vector3(0.5f, 0, 0);
            offset = new Vector3(-1, 0, 0);
            playerDirection = new Vector3(-playerDirection.x, playerDirection.y, playerDirection.z);
        }
    }

    private void LookUpPerformed(InputAction.CallbackContext context) 
    {
        axeOffset = new Vector3(0, -0.5f, 0);
        offset = new Vector3(0, 1, 0);
        Debug.Log("looking up");
    }
    private void LookUpCanceled(InputAction.CallbackContext context) 
    {
        axeOffset = playerDirection * axeOffsetValue;
        offset = playerDirection;
        Debug.Log("looking down");
    }
}
