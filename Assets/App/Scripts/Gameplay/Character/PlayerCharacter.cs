using System;
using System.Collections.Generic;
using Cinemachine;
using Game.Gameplay.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.Character
{
    public class PlayerCharacter : MonoBehaviour, IPlayerControllable, IDamageTarget
    {
        [Header("References")]
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        [SerializeField] private CinemachineVirtualCamera thirdPersonCamera;
        [SerializeField] private PlayerCharacterAnimator characterAnimator;
        [SerializeField] private InputRecorder inputRecorder;

        [Header("Input actions")]
        [SerializeField] private InputActionReference attackSwift;
        [SerializeField] private InputActionReference attackStrong;
        [SerializeField] private InputActionReference defenceBlock;
        [SerializeField] private InputActionReference defenceDodge;

        private bool _isPossessed;
        private bool _isActionInputBlocked;

        private Dictionary<Guid, Action> _inputActionsCallbackLookup;
        private readonly CharacterStateData _stateData = new();
        private IDamageTarget _lockedOnTarget;

        public bool IsPossessed => _isPossessed;
        public InputRecorder InputRecorder => inputRecorder;

        /// <summary>
        /// Fired when character is hit to death.
        /// </summary>
        public event Action<PlayerCharacter> Defeated;

        private void Awake()
        {
            thirdPersonCamera.enabled = false;

            _inputActionsCallbackLookup = new Dictionary<Guid, Action>
            {
                { attackSwift.action.id, OnInputAttackSwift },
                { attackStrong.action.id, OnInputAttackStrong },
                { defenceBlock.action.id, OnInputDefenceBlock },
                { defenceDodge.action.id, OnInputDefenceDodge },
            };
        }

        #region Input callbacks

        private async void OnInputAttackSwift()
        {
            _isActionInputBlocked = true;
            await characterAnimator.TriggerSwiftAttackAsync();

            _lockedOnTarget.TakeHit(AttackType.Swift);

            _isActionInputBlocked = false;
        }

        private async void OnInputAttackStrong()
        {
            _isActionInputBlocked = true;
            await characterAnimator.TriggerStrongAttackAsync();

            _lockedOnTarget.TakeHit(AttackType.Strong);

            _isActionInputBlocked = false;
        }

        private async void OnInputDefenceBlock()
        {
            _stateData.Blocking = true;
            await characterAnimator.TriggerBlockingAsync();

            _stateData.Blocking = false;
            _isActionInputBlocked = false;
        }

        private async void OnInputDefenceDodge()
        {
            _stateData.Dodging = true;
            await characterAnimator.TriggerDodgingAsync();

            _stateData.Dodging = false;
            _isActionInputBlocked = false;
        }

        #endregion

        public void SetBodyColor(Color color)
        {
            if (!bodyRenderer)
            {
                Debug.LogError("Unable to change color, Character has no body renderer assigned", this);
                return;
            }

            var material = bodyRenderer.material;
            material.color = color;

            bodyRenderer.sharedMaterial = material;
        }

        public void LockOnTarget(IDamageTarget target)
        {
            _lockedOnTarget = target;
            characterAnimator.InBattle = true;
        }

        public IPlayerControllable Possess()
        {
            _isPossessed = true;

            // note: should've be done in a separate service, as reaction on possession event but for the demo let it be
            Cursor.lockState = CursorLockMode.Locked;
            thirdPersonCamera.enabled = true;

            return this;
        }

        public IPlayerControllable Release()
        {
            _isPossessed = false;
            Cursor.lockState = CursorLockMode.None;
            thirdPersonCamera.enabled = false;

            return this;
        }

        public void TriggerInputAction(Guid actionId)
        {
            if (!_isActionInputBlocked && _inputActionsCallbackLookup.TryGetValue(actionId, out var callback))
            {
                if (_isPossessed && inputRecorder.IsRecording)
                    inputRecorder.RecordAction(actionId, Time.timeAsDouble);

                callback.Invoke();
            }
        }

        public void TakeHit(AttackType attack)
        {
            bool isHit;
            switch (attack)
            {
                case AttackType.Swift:
                    isHit = !_stateData.Blocking;
                    if (_stateData.Blocking)
                        characterAnimator.TriggerShieldImpact();
                    else if (!_stateData.Dodging)
                        characterAnimator.TriggerHit();
                    break;
                case AttackType.Strong:
                    isHit = !_stateData.Dodging;
                    if (!_stateData.Dodging)
                        characterAnimator.TriggerHit();
                    break;
                default:
                    isHit = false;
                    break;
            }

            if (isHit)
            {
                characterAnimator.TriggerDeath();
                characterAnimator.InBattle = false;

                Defeated?.Invoke(this);
            }
        }

        public void Revive()
        {
            characterAnimator.ResetAnimator();
            characterAnimator.InBattle = true;
            _stateData.Reset();
        }
    }
}