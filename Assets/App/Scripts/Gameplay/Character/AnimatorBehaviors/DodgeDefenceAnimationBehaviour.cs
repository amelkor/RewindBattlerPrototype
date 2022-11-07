namespace Game.Gameplay.Character.AnimatorBehaviors
{
    public class DodgeDefenceAnimationBehaviour : StateBehaviourBase
    {
        protected override void OnExit()
        {
            characterAnimator.TriggerDodgeFinished();
        }
    }
}