using System;

namespace Core.ProjectUpdater
{
    public interface IProjectUpdater
    {
        event Action UpdateCalled;
        event Action FixedUpdateCalled;
        event Action LateUpdateCalled;
    }
}