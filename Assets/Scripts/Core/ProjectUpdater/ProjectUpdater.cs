using System;
using UnityEngine;

namespace Core.ProjectUpdater
{
    public class ProjectUpdater : MonoBehaviour, IProjectUpdater
    {
        public static IProjectUpdater Instance;
        
        public event Action UpdateCalled;
        public event Action FixedUpdateCalled;
        public event Action LateUpdateCalled;

        private bool _isPaused;

        public bool IsPaused
        {
            get => _isPaused;

            set
            {
                if(_isPaused == value)
                    return;

                UnityEngine.Time.timeScale = value ? 0 : 1;
                _isPaused = value;
            }
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            if(_isPaused)
                return;
            
            UpdateCalled?.Invoke();
        }

        private void FixedUpdate()
        {
            if(_isPaused)
                return;
            
            FixedUpdateCalled?.Invoke();
        }

        private void LateUpdate()
        {
            if(_isPaused)
                return;
            
            LateUpdateCalled?.Invoke();
        }
    }
}