using System;
using Core.ProjectUpdater;

public class BoatSystem : IDisposable
{
    private Boat _boat;
    private Joystick _joystick;
    
    public BoatSystem(Boat boat,Joystick joystick,WaterManager waterManager)
    {
        _boat = boat;
        _joystick = joystick;
        _boat.Initialize(waterManager);
        
        ProjectUpdater.Instance.UpdateCalled += GetPlayerInput;
        ProjectUpdater.Instance.FixedUpdateCalled += _boat.Move;
    }

    private void GetPlayerInput()
    {
        _boat.UpdateInput(_joystick.Direction);
    }

    public void Dispose()
    {
        ProjectUpdater.Instance.UpdateCalled -= GetPlayerInput;
        ProjectUpdater.Instance.FixedUpdateCalled -= _boat.Move;
    }
}