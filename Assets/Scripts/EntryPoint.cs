using System;
using System.Collections.Generic;
using Core.ProjectUpdater;
using UnityEngine;
using Water;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private Joystick _joystick;
    [SerializeField] private WaterManager _waterManager;
    [SerializeField] private Boat _boat;
    
    private ProjectUpdater _projectUpdater;
    private BoatSystem _boatSystem;
    private void Start()
    {
        if (ProjectUpdater.Instance == null)
            _projectUpdater = new GameObject().AddComponent<ProjectUpdater>();
        else
            _projectUpdater = ProjectUpdater.Instance as ProjectUpdater;

        _waterManager.Initialize();
        _boatSystem = new BoatSystem(_boat, _joystick, _waterManager);

    }

    private void OnDestroy()
    {
        _boatSystem.Dispose();
    }
}