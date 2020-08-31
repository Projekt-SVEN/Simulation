using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Simulation.Abstractions
{
    /// <summary>
    /// Abstraction for a thermal simulation of a <see cref="IRoom"/>.
    /// </summary>
    public interface IRoomThermalManager : ITemperatureSource<Vector3>
    {
        /// <summary>
        /// Occurs when <see cref="ThermalPixelSize"/> changed.
        /// </summary>
        event EventHandler<float> OnThermalPixelSizeChanged;

        /// <summary>
        /// Occurs when <see cref="ThermalTickDuration"/> changed.
        /// </summary>
        event EventHandler<float> OnThermalTickDurationChanged;

        /// <summary>
        /// Gets the <see cref="IRoom"/> that is thermally simulated.
        /// </summary>
        IRoom Room { get; }

        /// <summary>
        /// Gets or sets the size of the thermal-pixels in meter.
        /// </summary>
        float ThermalPixelSize { get; set; }

        /// <summary>
        /// Gets or sets the duration of an thermal-update in seconds.
        /// </summary>
        float ThermalTickDuration { get; set; }

        /// <summary>
        /// Gets the temperature statistics of all rendered frames.
        /// </summary>
        TemperatureStatistics TotalTemperatureStatistics { get; }

        /// <summary>
        /// Gets the temperature statistics of the captured time frame.
        /// </summary>
        TemperatureStatistics CapturedTemperatureStatistics { get; }

        /// <summary>
        /// Gets the temperature statistics of the current rendered frame.
        /// </summary>
        TemperatureStatistics CurrentTemperatureStatistics { get; }

        /// <summary>
        /// Resets the <see cref="TemperatureStatistics"/> for the <see cref="CapturedTemperatureStatistics"/> property.
        /// </summary>
        void ResetCapturedTemperatureStatistics();

        /// <summary>
        /// Adds a <see cref="IThermalObject"/> to the thermal simulation of the <see cref="Room"/>.
        /// </summary>
        /// <param name="thermalObject">
        /// The <see cref="IThermalObject"/> that should be added.
        /// </param>
        void AddThermalObject(IThermalObject thermalObject);

        /// <summary>
        /// Removes a <see cref="IThermalObject"/> from the thermal simulation of the <see cref="Room"/>.
        /// </summary>
        /// <param name="thermalObject">
        /// The <see cref="IThermalObject"/> that should be removed.
        /// </param>
        void RemoveThermalObject(IThermalObject thermalObject);

        /// <summary>
        /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html">MonoBehaviour.Start</see>
        /// </summary>
        void Start();

        /// <summary>
        /// Called every frame, if the MonoBehaviour is enabled.
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
        /// </summary>
        void Update();
    }
}
