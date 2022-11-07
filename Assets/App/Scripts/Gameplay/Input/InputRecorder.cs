using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.Input
{
    /// <summary>
    /// Records and replays Player's input actions.
    /// </summary>
    public class InputRecorder : MonoBehaviour
    {
        private readonly LinkedList<InputRecord> _records = new();
        private LinkedListNode<InputRecord> _currentRecord;
        private double _recordingStartTime;
        private double _recordingStopTime;
        private double _replayStartTime;

        private Action<Guid> _onReplayActionCallback;
        private Action _onReplayFinishedCallback;
        private bool _isRecording;
        private bool _isReplaying;
        private int _replayPosition;

        public bool IsEmpty => _records.Count == 0;
        public bool IsRecording => _isRecording;
        public bool IsReplaying => _isReplaying;
        public double TotalDuration => IsEmpty ? 0f : _records.Last.Value.time;
        public double ReplayTimeLeft => _isReplaying ? (float)TotalDuration - (float)(Time.timeAsDouble - _replayStartTime) : 0f;
        public double ElapsedReplayTimeNormalized => _isReplaying ? Mathf.InverseLerp(0f, (float)TotalDuration, (float)(Time.timeAsDouble - _replayStartTime)) : 0f;

        /// <summary>
        /// Starts or resumes recording of Player's input actions.
        /// </summary>
        public void StartRecording()
        {
            _recordingStartTime += Time.timeAsDouble - _recordingStopTime;
            _isRecording = true;
        }

        /// <summary>
        /// Stops recording.
        /// </summary>
        public void StopRecording()
        {
            _recordingStopTime = Time.timeAsDouble;
            _isRecording = false;
        }

        /// <summary>
        /// Creates texture of a given dimensions which represents the recorded input timeline and places action markers on it.
        /// </summary>
        /// <param name="markerColor">Which color the markers should be.</param>
        /// <param name="width">Timeline texture width.</param>
        /// <param name="height">Timeline texture height.</param>
        /// <param name="markerWidth">Width of markers.</param>
        /// <returns>New texture with input action markers on it.</returns>
        public Texture2D CreateInputTimelineTexture(Color markerColor, int width, int height = 20, int markerWidth = 4)
        {
            var texture = new Texture2D(width, height);

            if (IsEmpty)
                return texture;

            var colors = new Color[height * markerWidth];
            Array.Fill(colors, markerColor);

            var timeEnd = _records.Last.Value.time;

            foreach (var record in _records)
            {
                var x = Mathf.FloorToInt(math.remap(0, (float)timeEnd, 0, width, (float)record.time));
                if (x >= width)
                    x = width - markerWidth - 1;

                texture.SetPixels(x, 0, markerWidth, height, colors);
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// starts replaying the recorded actions according to their timestamps.
        /// </summary>
        /// <param name="onActionCallback">Callback when recorded action fires.</param>
        /// <param name="onFinished">Callback when the replay finishes.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="onActionCallback"/> is null.</exception>
        public void StartReplay(Action<Guid> onActionCallback, Action onFinished = null)
        {
            _onReplayActionCallback = onActionCallback ?? throw new ArgumentNullException(nameof(onActionCallback), "Callback action can not be null");
            _onReplayFinishedCallback = onFinished;
            _replayStartTime = Time.timeAsDouble;
            _currentRecord = _records.First;
            _replayPosition = 0;
            _isReplaying = true;
        }

        /// <summary>
        /// Stops current replay.
        /// </summary>
        public void StopReplay()
        {
            if (_isReplaying)
                OnFinishedReplay();
        }

        /// <summary>
        /// Records the input action.
        /// </summary>
        /// <param name="actionId">Input action id to record.</param>
        /// <param name="time">Game time at what the action has occured.</param>
        public void RecordAction(Guid actionId, double time)
        {
            _records.AddLast(new InputRecord(actionId, time - _recordingStartTime));
            Debug.Log($"recorded action id: {actionId.ToString()}");
        }

        /// <summary>
        /// Checks if the next action queued for replay is the same <paramref name="action"/> provided, happened at the same time relative to recording start time,
        /// and the acceptable time offset is not greater or less than <paramref name="timeErrorOffset"/> value.
        /// </summary>
        /// <param name="action">Expected input action.</param>
        /// <param name="timeErrorOffset">Acceptable time offset from the original timestamp.</param>
        /// <returns>True if action and timing are same as the recorded ones.</returns>
        /// <exception cref="InvalidOperationException">when no replay is active.</exception>
        public bool EvaluateCurrentReplayedAction(InputAction action, float timeErrorOffset)
        {
            if (!_isReplaying)
                throw new InvalidOperationException("The recorder is not replaying");
            
            var relativeTime = Time.timeAsDouble - _replayStartTime;
            var record = _currentRecord.Value;

            return record.actionId == action.id && IsNotExceeds(relativeTime, record.time, timeErrorOffset);
        }

        private void FixedUpdate()
        {
            if (!_isReplaying)
                return;

            if (_replayPosition + 1 > _records.Count)
            {
                OnFinishedReplay();
                return;
            }

            var record = _currentRecord.Value;
            if (Time.timeAsDouble - _replayStartTime >= record.time)
            {
                _replayPosition++;
                _currentRecord = _currentRecord.Next;
                _onReplayActionCallback(record.actionId);
            }
        }

        private void OnFinishedReplay()
        {
            _isReplaying = false;
            var finishedCallback = _onReplayFinishedCallback;
            _onReplayFinishedCallback = default;

            finishedCallback?.Invoke();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool IsNotExceeds(double actual, double value, float error)
        {
            return actual >= value - error && actual <= value + error;
        }
    }
}