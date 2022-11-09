using System;
using System.Collections.Generic;
using Game.Gameplay.Character;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Gameplay.Input
{
    /// <note>
    /// For debugging
    /// </note>
    public class DebugInputRecorderCheck: MonoBehaviour
    {
        [SerializeField] private PlayerCharacterAnimator animator;
        [SerializeField] private InputRecorder recorder;
        [SerializeField] private RawImage timelineBg;
        [SerializeField] private Scrollbar timeline;
        [SerializeField] private Text countdown;
        [SerializeField] private float inputValidationTimeErrorOffset = 2f;
        
        private GameInput _input;
        private Dictionary<Guid, Action> _inputActionLookup;

        private bool _isBusy;

        private void Awake()
        {
            _input = new GameInput();
            _input.Enable();
            _input.Character.Enable();
            _input.Character.StrongAttack.performed += StrongAttackOnPerformed;
            _input.Character.Dodge.performed += DodgeOnPerformed;

            _inputActionLookup = new Dictionary<Guid, Action>
            {
                { _input.Character.Dodge.id, OnInputDefenceDodge },
                { _input.Character.StrongAttack.id, OnInputAttackStrong },
            };
        }
        
        private async void OnInputDefenceDodge()
        {
            await animator.TriggerDodgingAsync();
        }
        
        private async void OnInputAttackStrong()
        {
            await animator.TriggerSwiftAttackAsync();
        }
        
        private async void DodgeOnPerformed(InputAction.CallbackContext context)
        {
            if (recorder.IsReplaying)
            {
                if (recorder.EvaluateCurrentReplayedAction(context.action, inputValidationTimeErrorOffset))
                    Debug.Log($"+++ Input Match: {context.action.name}");
                else
                    Debug.Log($"--- Input Missmatch: {context.action.name}");

                return;
            }

            recorder.RecordAction(context.action.id, Time.timeAsDouble);

            Debug.Log($"{context.action.name} performed");
            
            _isBusy = true;
            await animator.TriggerDodgingAsync();
            _isBusy = false;
            
            Debug.Log($"{context.action.name} awaited");
        }

        private async void StrongAttackOnPerformed(InputAction.CallbackContext context)
        {
            if(_isBusy)
                return;
            
            if (recorder.IsReplaying)
            {
                if (recorder.EvaluateCurrentReplayedAction(context.action, inputValidationTimeErrorOffset))
                    Debug.Log($"+++ Input Match: {context.action.name}");
                else
                    Debug.Log($"--- Input Missmatch: {context.action.name}");

                return;
            }

            recorder.RecordAction(context.action.id, Time.timeAsDouble);

            Debug.Log($"{context.action.name} performed");
            
            _isBusy = true;
            await animator.TriggerStrongAttackAsync();
            _isBusy = false;
            
            Debug.Log($"{context.action.name} awaited");
        }

        public void StartRecording()
        {
            animator.InBattle = true;
            recorder.StartRecording();
        }

        public void StopAndReplay()
        {
            recorder.StopRecording();
            timelineBg.texture = recorder.CreateInputTimelineTexture(Color.red, Screen.width);
            timeline.value = 0f;

            recorder.StartReplay(OnActionReplay, () => Debug.Log("Replay finished"));
        }

        private void LateUpdate()
        {
            if (recorder.IsReplaying)
            {
                countdown.text = Mathf.RoundToInt((float)recorder.ReplayTimeLeft).ToString();
                timeline.value = (float)recorder.ElapsedReplayTimeNormalized;
            }
        }

        private void OnActionReplay(Guid id)
        {
            Debug.Log($"REPLAYED action: {id}");

            if (_inputActionLookup.TryGetValue(id, out var callback))
                callback?.Invoke();
        }
    }
}