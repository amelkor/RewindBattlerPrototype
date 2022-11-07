namespace Game.Gameplay.Character.AnimatorBehaviors
{
    public class BlockDefenceAnimationBehaviour : StateBehaviourBase
    {
        protected override void OnExit()
        {
            characterAnimator.TriggerBlockingFinished();
        }
    }
}