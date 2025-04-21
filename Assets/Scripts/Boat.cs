using System;
using Core.ProjectUpdater;
using UnityEngine;
using UnityEngine.Serialization;
using Water;

public class Boat : BuoyancyObject
{
    [Header("Movement Settings")]
    [SerializeField] private PilotAnimator _pilotAnimator;
    
    [Header("Movement Settings")]
    [SerializeField] private float _motorForce = 16000f;
    [SerializeField] private float _turnTorque = 2000f;
    [SerializeField] private float _maxSpeed = 500f;
    [SerializeField] private float _dragInWater = 0.98f;
    [SerializeField] private float _turnDampening = 0.95f;
    
    [Header("Tilt Settings")]
    [SerializeField] private float _rollTorqueX = 500f;
    [SerializeField] private float _rollTorqueZ = 1000f;
    [SerializeField] private float _rollStabilizeSpeed = 1f;

    private float _forwardInput;
    private float _turnInput;

    public void UpdateInput(Vector2 direction)
    {
        _forwardInput = direction.y == 0 ? 0 : (direction.y > 0 ? 1 : -1);
        _turnInput = direction.x;
    }

    public void Move()
    {
        float force = Underwater ? _motorForce : _motorForce / 10;
        Vector3 rawRight = transform.right;
        Vector3 forward = new Vector3(rawRight.x, 0f, rawRight.z).normalized;
        Vector3 directionForce = _forwardInput > 0 ? forward * _forwardInput * force : forward * _forwardInput * force/10;
        float directionTurnTorque = _forwardInput > 0 ? _turnInput * _turnTorque : _turnInput * _turnTorque / 3;
        if (Rigidbody.velocity.magnitude < _maxSpeed)
        {
            Rigidbody.AddForce(directionForce * Time.fixedDeltaTime);
        }
        
        if (Mathf.Abs(_forwardInput) > 0.1f)
        {
            Rigidbody.AddTorque(Vector3.up *directionTurnTorque * Time.fixedDeltaTime);
        }
        
        ApplyRollTorque();
        StabilizeRoll();
        _pilotAnimator.KinematicAnimation(transform,_turnInput);
        
        Rigidbody.velocity *= _dragInWater;
        Rigidbody.angularVelocity *= _turnDampening;
    }

    private void ApplyRollTorque()
    {
        Vector3 localTorque = new Vector3(-_turnInput * _rollTorqueX, 0f, _forwardInput * _rollTorqueZ);
        Vector3 worldTorque = transform.TransformDirection(localTorque);

        Rigidbody.AddTorque(worldTorque * Time.fixedDeltaTime);
    }

    private void StabilizeRoll()
    {
        Vector3 localAngularVelocity = transform.InverseTransformDirection(Rigidbody.angularVelocity);
        
        localAngularVelocity.x *= 1f - _rollStabilizeSpeed * Time.fixedDeltaTime;
        localAngularVelocity.z *= 1f - _rollStabilizeSpeed * Time.fixedDeltaTime;
        
        Rigidbody.angularVelocity = transform.TransformDirection(localAngularVelocity);
    }
}