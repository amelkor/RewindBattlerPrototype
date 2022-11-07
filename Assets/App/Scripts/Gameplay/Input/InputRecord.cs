using System;

namespace Game.Gameplay.Input
{
    /// <summary>
    /// Represents Player's input at a moment of time.
    /// </summary>
    public readonly struct InputRecord
    {
        /// <summary>
        /// The input action id that was performed.
        /// </summary>
        public readonly Guid actionId;
        
        /// <summary>
        /// The time the action occured since the recording started.
        /// </summary>
        public readonly double time;

        public InputRecord(Guid actionId, double time)
        {
            this.actionId = actionId;
            this.time = time;
        }
    }
}