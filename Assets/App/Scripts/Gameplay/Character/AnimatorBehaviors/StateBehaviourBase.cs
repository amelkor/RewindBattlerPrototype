using UnityEngine;

namespace Game.Gameplay.Character.AnimatorBehaviors
{
    public abstract class StateBehaviourBase : StateMachineBehaviour
    {
        private bool _isInitialized;
        protected PlayerCharacterAnimator characterAnimator;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                characterAnimator = animator.GetComponent<PlayerCharacterAnimator>();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            OnExit();
        }

        protected abstract void OnExit();
    }
}