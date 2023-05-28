using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rope : MonoBehaviour
{
    [SerializeField] AxeThrow axeThrow;

    PlayerInputAction inputAction;

    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] DistanceJoint2D distanceJoint;

    void Awake() 
    {
        inputAction = new PlayerInputAction();
    }

    void OnEnable() 
    {
        inputAction.Enable();
        inputAction.Player.TightenRope.performed += TightenRope;
        inputAction.Player.TightenRope.canceled += LoosenRope;
    }
    void OnDisable() 
    {
        inputAction.Disable();
        inputAction.Player.TightenRope.performed -= TightenRope;
        inputAction.Player.TightenRope.canceled -= LoosenRope;
    }

    void TightenRope(InputAction.CallbackContext context) 
    {
        distanceJoint.enabled = true;
        distanceJoint.connectedAnchor = axeThrow.targetLocation;
    }

    void LoosenRope(InputAction.CallbackContext context) 
    {
        distanceJoint.enabled = false;
    }
}
