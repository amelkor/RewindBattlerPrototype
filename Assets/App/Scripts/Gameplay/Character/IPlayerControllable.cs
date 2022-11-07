using System;
using Game.Gameplay.Input;

namespace Game.Gameplay.Character
{
    public interface IPlayerControllable
    {
        bool IsPossessed { get;}
        InputRecorder InputRecorder { get; }
        
        public IPlayerControllable Possess();
        public IPlayerControllable Release();
        public void Revive();
        void TriggerInputAction(Guid actionId);
    }
}