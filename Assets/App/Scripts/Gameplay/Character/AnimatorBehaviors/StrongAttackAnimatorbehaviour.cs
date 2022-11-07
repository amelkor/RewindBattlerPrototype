namespace Game.Gameplay.Character.AnimatorBehaviors
{
    public class StrongAttackAnimatorbehaviour : StateBehaviourBase
    {
        protected override void OnExit()
        {
            characterAnimator.TriggerStrongAttackFinished();
        }
    }
}