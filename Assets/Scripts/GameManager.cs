using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GameManager : MonoBehaviour
{

    [SerializeField] private InputActionReference leftSwitch;
    [SerializeField] private InputActionReference rightSwitch;

    public ActionBasedController rightController;
    public ActionBasedController leftController;
    public Rigidbody leftHandRb;
    public Rigidbody rightHandRb;
    public int mode = 0;
    
    private float _rightThrusterValue = 0f;
    private float _leftThrusterValue = 0f;
    public Rigidbody rb;
    public float thrusterConstant = 2.5f;
    public Transform camera;

    public Transform rightEmitter;
    public Transform rightVecPoint;
    public Transform leftEmitter;
    public Transform leftVecPoint;

    private Vector3 _leftHandVelocity;
    private Vector3 _rightHandVelocity;
    private Vector3 _previousLeftHandPosition;
    private Vector3 _previousRightHandPosition;

    public ActionBasedContinuousMoveProvider cmp;
    public ActionBasedContinuousTurnProvider ctp;
    public Transform playerTransform;

    public float liftMultiplier = 2f;
    public float negativeLiftMultiplier = .2f;
    public float maxLiftForce = 20f;
    public float maxUpwardVelocity = 10f;
    public float forwardSpeed = 3f;

    public MeshRenderer leftHandRenderer, rightHandRenderer;
    public Material thrusterMat, impulseMat, jetpackMat, flappyMat;
    void Start()
    {
        _previousLeftHandPosition = leftController.transform.position;
        _previousRightHandPosition = rightController.transform.position;
        

    }
    // Update is called once per frame
    void Update()
    {
        
        leftSwitch.action.performed += decrementMode;
        rightSwitch.action.performed += incrementMode;
        switch (mode)
        {
            case 0:
                Debug.Log("Press triggers and rotate hands!");
                leftHandRenderer.material = thrusterMat;
                rightHandRenderer.material = thrusterMat;
                cmp.enabled = false;
                ctp.enabled = true;
                rotateHandThruster();
                break;
            case 1:
                Debug.Log("Press both grips to shoot in opposite direction of your hands!");
                leftHandRenderer.material = impulseMat;
                rightHandRenderer.material = impulseMat;
                cmp.enabled = false;
                ctp.enabled = true;
                rotateHandThruster();
                handImpulseThruster();
                break;
            case 2:
                Debug.Log("Press both triggers and use left joystick to move!");
                leftHandRenderer.material = jetpackMat;
                rightHandRenderer.material = jetpackMat;
                cmp.enabled = true;
                ctp.enabled = true;
                jetpack();
                break;
            case 3:
                Debug.Log("Flap like a bird!!!");
                leftHandRenderer.material = flappyMat;
                rightHandRenderer.material = flappyMat;
                cmp.enabled = false;
                ctp.enabled = false;

                flappy();
                break;
        }

    }

    private void flappy()
    {
        _leftHandVelocity = (leftController.transform.position - _previousLeftHandPosition) / Time.deltaTime;
        _previousLeftHandPosition = leftController.transform.position;
        _rightHandVelocity = (rightController.transform.position - _previousRightHandPosition) / Time.deltaTime;
        _previousRightHandPosition = rightController.transform.position;
        
        
        float currentUpwardVelocity = rb.velocity.y;

        
        // Calculate the relative velocities of the controllers to the player
        Vector3 relativeLeftHandVelocity = _leftHandVelocity - rb.velocity;
        Vector3 relativeRightHandVelocity = _rightHandVelocity - rb.velocity;

        // Combine the velocities to determine the overall force vector
        Vector3 combinedVelocity = relativeLeftHandVelocity + relativeRightHandVelocity;

        // Calculate the lift force based on the combined velocity
        if (relativeLeftHandVelocity.y < 0 && relativeRightHandVelocity.y < 0 && currentUpwardVelocity < maxUpwardVelocity)
        {
            Vector3 liftForce = combinedVelocity.magnitude * liftMultiplier * Vector3.up;
            
            liftForce *= (1 - Mathf.Clamp01(currentUpwardVelocity / maxUpwardVelocity));

            Vector3 forwardForce = camera.forward;
            forwardForce.y = 0;

            

           
            liftForce = Vector3.ClampMagnitude(liftForce, maxLiftForce);
            
            

            // Apply the lift force
            rb.AddForce(liftForce, ForceMode.Force);
            
            Vector3 forwardVelocity = forwardForce.normalized * forwardSpeed;

            // Set the rigidbody's velocity, maintaining the current y-component (vertical velocity)
            rb.velocity = new Vector3(forwardVelocity.x, rb.velocity.y, forwardVelocity.z);




            //Debug.Log(forwardForce);

        }

        if (relativeLeftHandVelocity.y > 0 && relativeRightHandVelocity.y > 0)
        {
            Vector3 negativeLiftForce = combinedVelocity.magnitude * negativeLiftMultiplier * Vector3.down;
            rb.AddForce(negativeLiftForce, ForceMode.Force);
        }
        

        


    }

    private void jetpack()
    {
        float rightTriggerValue = rightController.activateActionValue.action.ReadValue<float>();
        _rightThrusterValue = rightTriggerValue;
        float leftTriggerValue = leftController.activateActionValue.action.ReadValue<float>();
        _leftThrusterValue = leftTriggerValue;
        
        rb.AddForce((_rightThrusterValue + _leftThrusterValue) * thrusterConstant * Vector3.up);
    }

    private void handImpulseThruster()
    {
        float rightGrip = rightController.selectAction.action.ReadValue<float>();
        float leftGrip = leftController.selectAction.action.ReadValue<float>();
        Vector3 rightHandPosition = rightController.transform.position;
        Vector3 leftHandPosition = leftController.transform.position;
        Vector3 rightImpulseVector = playerTransform.position - rightHandPosition;
        Vector3 leftImpulseVector = playerTransform.position - leftHandPosition;

        if (rightGrip == 1 && leftGrip == 1)
        {
            rb.AddForce((rightImpulseVector + leftImpulseVector).normalized * 0.5f, ForceMode.Impulse);
        }
    }

    private void rotateHandThruster()
    {
        //right hand
        float rightTriggerValue = rightController.activateActionValue.action.ReadValue<float>();
        _rightThrusterValue = rightTriggerValue;
        Vector3 rightForceDirection = (rightVecPoint.position - rightEmitter.position) * -1;
        Vector3 rightForce = thrusterConstant * _rightThrusterValue * rightForceDirection.normalized;
        rb.AddForce(rightForce);
        
        //left hand
        float leftTriggerValue = leftController.activateActionValue.action.ReadValue<float>();
        _leftThrusterValue = leftTriggerValue;
        Vector3 leftForceDirection = (leftVecPoint.position - leftEmitter.position) * -1;
        Vector3 leftForce = thrusterConstant * _leftThrusterValue * leftForceDirection.normalized;
        rb.AddForce(leftForce);
    }

    private void incrementMode(InputAction.CallbackContext obj)
    {
        if (mode == 3)
        {
            
            mode = 0;
            Debug.Log(mode);

        }
        else
        {
            
            mode++;
            Debug.Log(mode);
        }
    }


    private void decrementMode(InputAction.CallbackContext obj)
    {
        if (mode == 0)
        {
            
            mode = 3;
            Debug.Log(mode);
        }
        else
        {
            
            mode--;
            Debug.Log(mode);
        }
    }
    
}
