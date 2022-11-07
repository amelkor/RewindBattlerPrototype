using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Character
{
    [RequireComponent(typeof(Animator))]
    public class PlayerCharacterAnimator : MonoBehaviour
    {
        #region Auxiliary classes

        [Serializable]
        private class AnimatorParameters
        {
            public string attackAnimationsPrefix = "AN_Knight_Attack";
            public string inBattle = "In_Battle";
            public string blocking = "Blocking";
            public string dodge = "Dodge";
            public string shieldImpact = "Shield_Impact";
            public string victory = "Victory";

            public string[] swiftAttacks =
            {
                "Attack_Sword_01",
                "Attack_Sword_02"
            };

            public string[] strongAttacks =
            {
                "Attack_Casting",
                "Attack_Kick",
            };

            public string[] hits =
            {
                "Hit_01",
                "Hit_02",
            };

            public string[] deaths =
            {
                "Death_01",
                "Death_02",
            };
        }

        #endregion

        [SerializeField] private Animator animator;
        [SerializeField] private AnimatorParameters animatorParameters;

        private int _inBattle;
        private int _blocking;
        private int _dodge;
        private int _shieldImpact;
        private int _victory;
        private int[] _swiftAttacks;
        private int[] _strongAttacks;
        private int[] _hits;
        private int[] _deaths;

        private readonly AnimationEventAsyncFinisher _swiftAttackAnimationFinisher = new();
        private readonly AnimationEventAsyncFinisher _strongAttackAnimationFinisher = new();
        private readonly AnimationEventAsyncFinisher _blockAnimationFinisher = new();
        private readonly AnimationEventAsyncFinisher _dodgeAnimationFinisher = new();

        private void Awake()
        {
            if (!animator && !TryGetComponent(out animator))
                throw new ArgumentNullException(nameof(animator), "Character Animator component is missing");

            if (!animator.runtimeAnimatorController)
                throw new ArgumentNullException(nameof(animator), "Character Animator Controller is not set");

            HashAnimatorParameters();
        }

        private void HashAnimatorParameters()
        {
            _inBattle = Animator.StringToHash(animatorParameters.inBattle);
            _blocking = Animator.StringToHash(animatorParameters.blocking);
            _dodge = Animator.StringToHash(animatorParameters.dodge);
            _shieldImpact = Animator.StringToHash(animatorParameters.shieldImpact);
            _victory = Animator.StringToHash(animatorParameters.victory);

            _swiftAttacks = new int[animatorParameters.swiftAttacks.Length];
            for (var i = 0; i < animatorParameters.swiftAttacks.Length; i++)
            {
                _swiftAttacks[i] = Animator.StringToHash(animatorParameters.swiftAttacks[i]);
            }

            _strongAttacks = new int[animatorParameters.strongAttacks.Length];
            for (var i = 0; i < animatorParameters.strongAttacks.Length; i++)
            {
                _strongAttacks[i] = Animator.StringToHash(animatorParameters.strongAttacks[i]);
            }

            _hits = new int[animatorParameters.hits.Length];
            for (var i = 0; i < animatorParameters.hits.Length; i++)
            {
                _hits[i] = Animator.StringToHash(animatorParameters.hits[i]);
            }

            _deaths = new int[animatorParameters.deaths.Length];
            for (var i = 0; i < animatorParameters.deaths.Length; i++)
            {
                _deaths[i] = Animator.StringToHash(animatorParameters.deaths[i]);
            }
        }

        public void TriggerSwiftAttackFinished()
        {
            _swiftAttackAnimationFinisher.Finish();
        }

        public void TriggerStrongAttackFinished()
        {
            _strongAttackAnimationFinisher.Finish();
        }

        public void TriggerBlockingFinished()
        {
            _blockAnimationFinisher.Finish();
        }

        public void TriggerDodgeFinished()
        {
            _dodgeAnimationFinisher.Finish();
        }

        public bool InBattle { set => animator.SetBool(_inBattle, value); }
        public void TriggerShieldImpact() => animator.SetTrigger(_shieldImpact);
        public void TriggerVictory() => animator.SetTrigger(_victory);
        public void TriggerDeath() => animator.SetTrigger(_deaths.GetRandom());

        // todo: temp, replace with properly triggering animations instead
        public void ResetAnimator() => animator.Rebind();

        public async UniTask TriggerSwiftAttackAsync()
        {
            if (_swiftAttackAnimationFinisher.IsAwaiting)
                return;

            animator.SetTrigger(_swiftAttacks.GetRandom());
            await _swiftAttackAnimationFinisher;
        }

        public async UniTask TriggerStrongAttackAsync()
        {
            if (_strongAttackAnimationFinisher.IsAwaiting)
                return;

            animator.SetTrigger(_strongAttacks.GetRandom());
            await _strongAttackAnimationFinisher;
        }

        public async UniTask TriggerBlockingAsync()
        {
            if (_blockAnimationFinisher.IsAwaiting)
                return;

            animator.SetTrigger(_blocking);
            await _blockAnimationFinisher;
        }

        public async UniTask TriggerDodgingAsync()
        {
            if (_dodgeAnimationFinisher.IsAwaiting)
                return;

            animator.SetTrigger(_dodge);
            await _dodgeAnimationFinisher;
        }

        public void TriggerHit() => animator.SetTrigger(_hits.GetRandom());

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!animator)
                TryGetComponent(out animator);
        }
#endif
    }
}