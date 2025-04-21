using System;
using Core.ProjectUpdater;
using UnityEngine;
using UnityEngine.Serialization;

namespace Water
{
    [RequireComponent(typeof(Rigidbody))]
    public class BuoyancyObject : MonoBehaviour
    {
        [SerializeField] private Transform[] _floaters;
        [SerializeField] private float _floatingPower = 60f;
        
        private const float underWaterDrag = 3f;
        private const float underWaterAngularDrag = 1f;
        private const float airDrag = 0f;
        private const float airAngularDrag = 0.05f;
        private int _floatersUnderWater;
        
        private WaterManager _waterManager;
        protected Rigidbody Rigidbody;
        
        protected bool Underwater;

        public virtual void Initialize(WaterManager waterManager)
        {
            Rigidbody = GetComponent<Rigidbody>();
            _waterManager = waterManager;
            ProjectUpdater.Instance.FixedUpdateCalled += StayAfloat;
        }

        private void StayAfloat()
        {
            _floatersUnderWater = 0;
            for (int i = 0; i < _floaters.Length; i++)
            {
                float diff = _floaters[i].position.y - _waterManager.GetWaveHeight(_floaters[i].position);
                if (diff < 0)
                {
                    Rigidbody.AddForceAtPosition(Vector3.up * _floatingPower * Mathf.Abs(diff),_floaters[i].position,ForceMode.Force);
                    _floatersUnderWater++;
                    if (!Underwater)
                    {
                        Underwater = true;
                        SwitchState(Underwater);
                    }
                }
            }
        
            if (Underwater && _floatersUnderWater == 0)
            {
                Underwater = false;
                SwitchState(Underwater);
            }
        }

        private void SwitchState(bool isUnderwater)
        {
            if (isUnderwater)
            {
                Rigidbody.drag = underWaterDrag;
                Rigidbody.angularDrag = underWaterAngularDrag;
            }
            else
            {
                Rigidbody.drag = airDrag;
                Rigidbody.angularDrag = airAngularDrag;
            }
        }

        private void OnDestroy()
        {
            ProjectUpdater.Instance.FixedUpdateCalled -= StayAfloat;
        }
    }
}
