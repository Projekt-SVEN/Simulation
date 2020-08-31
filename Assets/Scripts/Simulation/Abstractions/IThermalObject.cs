using UnityEngine;

namespace Assets.Scripts.Simulation.Abstractions
{
    public interface IThermalObject
    {
        /// <summary>
        /// Gets if the <see cref="IThermalObject"/> can not change its position.
        /// <see langword="true" /> can not change its position; otherwise <see langword="false"/>.
        /// </summary>
        bool CanNotChangePosition { get; }

        /// <summary>
        /// Gets the absolute (global) Position of the <see cref="IThermalObject"/> in m.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Gets how large the <see cref="IThermalObject"/> is in m (meter).
        /// </summary>
        Vector3 Size { get; }

        /// <summary>
        /// Gets the area of the surface of the <see cref="IThermalObject"/> in m² (square meter).
        /// </summary>
        float ThermalSurfaceArea { get; }

        /// <summary>
        /// Gets the <see cref="ThermalMaterial"/> of the <see cref="IThermalObject"/>.
        /// </summary>
        /// <remarks>
        /// Used to calculate the temperature and the heat transfer from and to the the <see cref="IThermalObject"/>.
        /// </remarks>
        ThermalMaterial ThermalMaterial { get; }

        /// <summary>
        /// Gets the temperature of the <see cref="IThermalObject"/>.
        /// </summary>
        Temperature Temperature { get; }

        /// <summary>
        /// A <see cref="IRoomThermalManager"/> signals the <see cref="IThermalObject"/> that the thermal simulation was started.
        /// </summary>
        /// <param name="roomThermalManager">
        /// The <see cref="IRoomThermalManager"/> that starts the thermal simulation with this <see cref="IThermalObject"/>. 
        /// </param>
        void ThermalStart(IRoomThermalManager roomThermalManager);

        /// <summary>
        /// Is called from the <see cref="IThermalObject"/> once per thermal update.
        /// </summary>
        /// <param name="transferredHeat">
        /// The heat that was transferred to the <see cref="IThermalObject"/> during the thermal update in J (Joule).
        /// </param>
        /// <param name="roomThermalManager">
        /// The <see cref="IRoomThermalManager"/> that does the thermal update.
        /// </param>
        void ThermalUpdate(float transferredHeat, IRoomThermalManager roomThermalManager);
    }
}