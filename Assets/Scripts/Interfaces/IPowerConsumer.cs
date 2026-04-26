using UnityEngine;

namespace AstroPioneer.Interfaces
{
    public interface IPowerConsumer
    {
        /// <summary>
        /// Amount of power required per tick/frame to function.
        /// </summary>
        float PowerRequired { get; }

        /// <summary>
        /// Current power status.
        /// </summary>
        bool IsPowered { get; }

        /// <summary>
        /// Called by PowerManager to supply power.
        /// </summary>
        /// <param name="amount">Amount of power supplied.</param>
        void ReceivePower(float amount);
        
        /// <summary>
        /// World position for distance calculations.
        /// </summary>
        Vector3 Position { get; }
    }
}
