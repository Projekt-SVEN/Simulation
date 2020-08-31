using System;
using Assets.Scripts.Remote;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.ObjectController
{
    public class HeaterController : MonoBehaviour, IThermalObject
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

        private DateTime _lastRead = DateTime.MinValue;
        private float _lastReadVentilValue = 0f;

        public RemoteHeater RemoteHeater { get; set; }

        /// <summary>
        /// Gets if the <see cref="IThermalObject"/> can not change its position.
        /// <see langword="true" /> can not change its position; otherwise <see langword="false"/>.
        /// </summary>
        public bool CanNotChangePosition { get; } = true;

        /// <summary>
        /// Gets the absolute (global) Position of the <see cref="IThermalObject"/> in m.
        /// </summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// Gets how large the <see cref="IThermalObject"/> is in m (meter).
        /// </summary>
        public Vector3 Size { get; set; } = new Vector3(1, 1);

        /// <summary>
        /// Gets the area of the surface of the <see cref="IThermalObject"/> in m² (square meter).
        /// </summary>
        public float ThermalSurfaceArea => Size.x * Size.y * 25;

        /// <summary>
        /// Gets the <see cref="IThermalObject.ThermalMaterial"/> of the <see cref="IThermalObject"/>.
        /// </summary>
        /// <remarks>
        /// Used to calculate the temperature and the heat transfer from and to the the <see cref="IThermalObject"/>.
        /// </remarks>
        public ThermalMaterial ThermalMaterial => ThermalMaterial.Heater;

        /// <summary>
        /// Gets the temperature of the <see cref="IThermalObject"/>.
        /// </summary>
        public Temperature Temperature { get; } = Temperature.FromCelsius(50);

        /// <summary>
        /// A <see cref="IRoomThermalManager"/> signals the <see cref="IThermalObject"/> that the thermal simulation was started.
        /// </summary>
        /// <param name="roomThermalManager">
        /// The <see cref="IRoomThermalManager"/> that starts the thermal simulation with this <see cref="IThermalObject"/>. 
        /// </param>
        public void ThermalStart(IRoomThermalManager roomThermalManager)
        {
        }

        /// <summary>
        /// Is called from the <see cref="IThermalObject"/> once per thermal update.
        /// </summary>
        /// <param name="transferredHeat">
        /// The heat that was transferred to the <see cref="IThermalObject"/> during the thermal update in J (Joule).
        /// </param>
        /// <param name="roomThermalManager">
        /// The <see cref="IRoomThermalManager"/> that does the thermal update.
        /// </param>
        public void ThermalUpdate(float transferredHeat, IRoomThermalManager roomThermalManager)
        {
            DateTime now = DateTime.Now;
            TimeSpan durationSinceLastRead = DateTime.Now - _lastRead;

            if (durationSinceLastRead > RefreshInterval)
            {
                _lastRead = now;

                try
                {
                    _lastReadVentilValue = RemoteHeater.GetState();
                }
                catch (Exception exception)
                {
                    Debug.LogError("Failed to get heater State");
                    Debug.LogException(exception);
                }
            }

            //TODO: process ventil value
        }
    }
}
