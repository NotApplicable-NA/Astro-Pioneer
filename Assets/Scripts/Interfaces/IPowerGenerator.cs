using UnityEngine;

namespace AstroPioneer.Interfaces
{
    public interface IPowerGenerator
    {
        /// <summary>
        /// Amount of power produced per tick/frame.
        /// </summary>
        float PowerProduction { get; }

        /// <summary>
        /// Effective range of power distribution (radius).
        /// </summary>
        float PowerRange { get; }
        
        /// <summary>
        /// Whether the generator is currently active/producing.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Callback when power is actually used by the system.
        /// Useful for batteries to deplete charge.
        /// </summary>
        /// <param name="amount">Amount of power consumed from this generator.</param>
        void OnPowerProvided(float amount);
        
        /// <summary>
        /// World position for distance calculations.
        /// </summary>
        Vector3 Position { get; }
    }
}
