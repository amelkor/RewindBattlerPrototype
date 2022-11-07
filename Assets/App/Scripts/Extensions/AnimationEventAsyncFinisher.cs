using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// Allows to manually set when awaiter should be finished. This relies on AnimationEvents.
    /// </summary>
    /// <example>
    /// <code>
    /// // This example allows to await a Unity animation event.
    /// AnimationEventAsyncFinisher _asyncAnimation = new AnimationEventAsyncFinisher();
    /// 
    /// public async UniTask Reload()
    /// {
    ///    // play the animation
    ///    animator.SetTrigger("SomeTrigger");
    ///    await _asyncAnimation;
    /// }
    ///
    /// //Animation event callback from an animation clip
    /// private void OnAnimationDone()
    /// {
    ///    _asyncAnimation.Finish();
    /// }
    /// </code>
    /// </example>
    public class AnimationEventAsyncFinisher
    {
        public readonly struct AnimationEventAsyncFinisherAwaiter : INotifyCompletion
        {
            private readonly AnimationEventAsyncFinisher _animationEventAsyncFinisher;

            // ReSharper disable once MemberCanBeMadeStatic.Global
            public AnimationEventAsyncFinisherAwaiter(AnimationEventAsyncFinisher e)
            {
                _animationEventAsyncFinisher = e;
            }

            // ReSharper disable once MemberCanBeMadeStatic.Global
            [DebuggerHidden]
            public bool IsCompleted => false;

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // ReSharper disable once MemberCanBeMadeStatic.Global
            public void GetResult()
            {
            }

            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action continuation)
            {
                _animationEventAsyncFinisher._continuation = continuation;
            }
        }

        private AnimationEventAsyncFinisherAwaiter _awaiter;
        private Action _continuation;

        public async UniTask GetTask() => await this;
        public bool IsAwaiting { get; private set; }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AnimationEventAsyncFinisherAwaiter GetAwaiter()
        {
            _awaiter = new AnimationEventAsyncFinisherAwaiter(this);
            IsAwaiting = true;
            return _awaiter;
        }

        /// <summary>
        /// Call this to invoke awaiter continuation callback.
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Finish()
        {
            if (IsAwaiting)
            {
                IsAwaiting = false;
                _continuation();
            }
        }
    }
}