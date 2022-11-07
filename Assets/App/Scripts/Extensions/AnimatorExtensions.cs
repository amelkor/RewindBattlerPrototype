using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class AnimatorExtensions
    {
        public static void BindAnimationEventToAllClipsWithPrefix(this RuntimeAnimatorController controller, string clipNamePrefix, string callbackMethodName, AnimationClipEventFirePosition firePosition, float fireTime = 0f, object parameter = null)
        {
            var clips = controller.GetAnimationClipsWithPrefix(clipNamePrefix);

            foreach (var clip in clips)
            {
                switch (firePosition)
                {
                    case AnimationClipEventFirePosition.Custom:
                    {
                        if (fireTime > clip.length)
                            fireTime = clip.length;
                        else if (fireTime < 0f)
                            fireTime = 0f;

                        break;
                    }
                    case AnimationClipEventFirePosition.AtStart:
                    {
                        fireTime = 0f;
                        break;
                    }
                    case AnimationClipEventFirePosition.AtEnd:
                    {
                        fireTime = clip.length;
                        break;
                    }
                    case AnimationClipEventFirePosition.AtMiddle:
                    {
                        fireTime = clip.length / 2;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(firePosition), firePosition, null);
                }

                var animationEvent = new AnimationEvent
                {
                    functionName = callbackMethodName,
                    time = fireTime,
                };

                if (parameter != null)
                {
                    switch (parameter)
                    {
                        case int i:
                            animationEvent.intParameter = i;
                            break;
                        case float f:
                            animationEvent.floatParameter = f;
                            break;
                        case string s:
                            animationEvent.stringParameter = s;
                            break;
                        case UnityEngine.Object o:
                            animationEvent.objectReferenceParameter = o;
                            break;
                    }
                }

                clip.AddEvent(animationEvent);
            }
        }

        /// <exception cref="KeyNotFoundException">when animation clip with provided name not found.</exception>
        private static IEnumerable<AnimationClip> GetAnimationClipsWithPrefix(this RuntimeAnimatorController controller, string nameLike)
        {
            var clips = controller.animationClips;

            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i].name.StartsWith(nameLike, StringComparison.OrdinalIgnoreCase))
                    yield return clips[i];
            }
        }
    }
}