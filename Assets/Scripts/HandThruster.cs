using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandThruster : MonoBehaviour
{
    //public XRBaseController controller;
    //[SerializeField] InputActionReference trigger;

    private float _thrusterValue = 0f;
    public Rigidbody rb;
    public float thrusterConstant = 7.5f;

    public Transform emitter;
    public Transform vecPoint;
    public ActionBasedController controller;

    public GameManager GameManager;

    


    public void Update()
    {
        if (GameManager.mode == 0)
        {

            float triggerValue = controller.activateActionValue.action.ReadValue<float>();
            _thrusterValue = triggerValue;
            Vector3 forceDirection = (vecPoint.position - emitter.position) * -1;
            Vector3 force = thrusterConstant * _thrusterValue * forceDirection.normalized;
            rb.AddForce(force);
        }
        //Debug.Log(triggerValue);
    }
    
}
