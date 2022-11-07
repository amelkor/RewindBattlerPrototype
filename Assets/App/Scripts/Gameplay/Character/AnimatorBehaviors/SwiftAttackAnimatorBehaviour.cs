namespace Game.Gameplay.Character.AnimatorBehaviors
{
    public class SwiftAttackAnimatorBehaviour : StateBehaviourBase
    {
        protected override void OnExit()
        {
            characterAnimator.TriggerSwiftAttackFinished();
        }
    }
}