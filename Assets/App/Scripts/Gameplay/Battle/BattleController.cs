using System.Collections;
using System.Collections.Generic;
using Game.Gameplay.Character;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Gameplay.Battle
{
    /// <note>
    /// Quickly assembled service to control characters. Assuming there could be only two characters, roughly implementing stuff exactly for two.
    /// </note>
    public class BattleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform leftSpawnPoint;
        [SerializeField] private Transform rightSpawnPoint;
        [SerializeField] private Scrollbar timeline;
        [SerializeField] private RawImage timelineBackground;
        [SerializeField] private Text timelineCounter;
        [SerializeField] private Text nextTurnCounter;

        [Header("Settings")]
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private Color leftPlayerColor = Color.blue;
        [SerializeField] private Color rightPlayerColor = Color.red;
        [SerializeField, Tooltip("How many times rewind occurs before the battle ends")] private int maxTurns = 3;
        [SerializeField] private int nextTurnDelay = 5;
        [SerializeField] private Color timelineMarkersColor = Color.red;

        private PlayerCharacter _leftCharacter;
        private PlayerCharacter _rightCharacter;
        private IPlayerControllable _possessedCharacter;
        private readonly Queue<IPlayerControllable> _charactersQueue = new(2);

        private GameInput _input;
        private int _turnsCount;
        private float _currentNextTurnDelay;
        private WaitForSeconds _nextTurnWaitDelay;

        private void Awake()
        {
            _input = new GameInput();
            _nextTurnWaitDelay = new WaitForSeconds(nextTurnDelay);
        }

        private void OnEnable()
        {
            var left = characterPrefab.Instantiate("CharacterLeft", leftSpawnPoint);
            var right = left.Instantiate("CharacterRight", rightSpawnPoint);

            _leftCharacter = left.GetComponent<PlayerCharacter>();
            _leftCharacter.SetBodyColor(leftPlayerColor);
            _leftCharacter.Defeated += OnCharacterDefeat;

            _rightCharacter = right.GetComponent<PlayerCharacter>();
            _rightCharacter.SetBodyColor(rightPlayerColor);
            _rightCharacter.Defeated += OnCharacterDefeat;

            _input.Character.SwiftAttack.performed += SwiftAttackOnPerformed;
            _input.Character.StrongAttack.performed += StrongAttackOnPerformed;
            _input.Character.Block.performed += BlockOnPerformed;
            _input.Character.Dodge.performed += DodgeOnPerformed;
        }

        private void OnDisable()
        {
            if (_leftCharacter)
            {
                _leftCharacter.Defeated -= OnCharacterDefeat;
                Destroy(_leftCharacter.gameObject);
            }

            if (_rightCharacter)
            {
                _rightCharacter.Defeated -= OnCharacterDefeat;
                Destroy(_rightCharacter.gameObject);
            }

            _input.Character.SwiftAttack.performed -= SwiftAttackOnPerformed;
            _input.Character.StrongAttack.performed -= StrongAttackOnPerformed;
            _input.Character.Block.performed -= BlockOnPerformed;
            _input.Character.Dodge.performed -= DodgeOnPerformed;
        }

        private void LateUpdate()
        {
            if (_possessedCharacter == null)
                return;

            var recorder = _possessedCharacter.InputRecorder;
            if (recorder.IsReplaying)
            {
                timelineCounter.text = Mathf.RoundToInt((float)recorder.ReplayTimeLeft).ToString();
                timeline.value = (float)recorder.ElapsedReplayTimeNormalized;
            }
        }

        #region Input actions callbacks

        private void SwiftAttackOnPerformed(InputAction.CallbackContext ctx)
        {
            _possessedCharacter.TriggerInputAction(ctx.action.id);
        }

        private void StrongAttackOnPerformed(InputAction.CallbackContext ctx)
        {
            _possessedCharacter.TriggerInputAction(ctx.action.id);
        }

        private void BlockOnPerformed(InputAction.CallbackContext ctx)
        {
            _possessedCharacter.TriggerInputAction(ctx.action.id);
        }

        private void DodgeOnPerformed(InputAction.CallbackContext ctx)
        {
            _possessedCharacter.TriggerInputAction(ctx.action.id);
        }

        #endregion

        public void StartBattle()
        {
            _leftCharacter.LockOnTarget(_rightCharacter);
            _rightCharacter.LockOnTarget(_leftCharacter);

            _charactersQueue.Enqueue(_leftCharacter);
            _charactersQueue.Enqueue(_rightCharacter);

            _possessedCharacter = _charactersQueue.Dequeue().Possess();
            _possessedCharacter.InputRecorder.StartRecording();

            _input.Character.Enable();
        }

        private void OnCharacterDefeat(PlayerCharacter character)
        {
            StartCoroutine(SwitchTurnsCoroutine());
        }

        // todo: rework this
        private IEnumerator SwitchTurnsCoroutine()
        {
            if (_turnsCount++ <= maxTurns)
            {
                Debug.Log($"Next turn");
                
                _possessedCharacter.InputRecorder.StopRecording();
                _input.Character.Disable();
                
                var victoriousCharacter = _possessedCharacter.Release();
                _possessedCharacter = null;
                _charactersQueue.Enqueue(victoriousCharacter);

                var defeatedCharacter = _charactersQueue.Dequeue();
                timelineBackground.texture = defeatedCharacter.InputRecorder.CreateInputTimelineTexture(timelineMarkersColor, Screen.width);

                // todo: start next turn timer
                Debug.Log($"Next turn in {nextTurnDelay.ToString()} seconds");

                yield return _nextTurnWaitDelay;

                _possessedCharacter = defeatedCharacter.Possess();
                defeatedCharacter.Revive();

                _input.Character.Enable();
                
                victoriousCharacter.InputRecorder.StartReplay(victoriousCharacter.TriggerInputAction, () => defeatedCharacter.InputRecorder.StartRecording());
                // todo: defeatedCharacter.InputRecorder.StartValidation()
            }
            else
            {
                _input.Character.Disable();
                ReplayBattle();
            }
        }

        private void ReplayBattle()
        {
            Debug.Log("Replaying battle");

            Cursor.lockState = CursorLockMode.None;

            _leftCharacter.InputRecorder.StartReplay(_ => { }, () => { Debug.Log("Left Character replay finished"); });
            _rightCharacter.InputRecorder.StartReplay(_ => { }, () => { Debug.Log("Right Character replay finished"); });
        }
    }
}