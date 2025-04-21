using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public class PilotAnimator
{
    [SerializeField] private Transform _animationPoint;
    [SerializeField] private float _angleModX;
    [SerializeField] private float _angleModY;
    [SerializeField] private float _angleModZ;
    [SerializeField] private float _angleModSpeed;

    public void KinematicAnimation(Transform boatTransform,float turnInput)
    {
        Quaternion scooterRotation = boatTransform.rotation;
        Vector3 euler = scooterRotation.eulerAngles;
        float adjustedX;
        float adjustedY;
        float adjustedZ;
        
        if (Mathf.Abs(turnInput) > 0.1f)
        {
            adjustedX = Mathf.Clamp((Mathf.DeltaAngle(0, euler.x) * _angleModX),-45f,45);
            adjustedY = Mathf.Clamp((Mathf.DeltaAngle(0, euler.y) * _angleModY),-45f,45);
        }
        else
        {
            adjustedY = 0;
            adjustedX = 0;
        }
        
        adjustedZ = Mathf.Clamp((Mathf.DeltaAngle(0, euler.z) * _angleModZ),-45f,45);
        
        Quaternion targetRotation = Quaternion.Euler(adjustedX, adjustedY, adjustedZ);
        
        _animationPoint.localRotation = Quaternion.Slerp(
            _animationPoint.localRotation, 
            targetRotation, 
            Time.deltaTime * _angleModSpeed
        );
                
    }
        
}