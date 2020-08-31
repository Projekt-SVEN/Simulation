using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public partial class RoomThermalManagerBuilder
    {
        private float _thermalPixelSize = 0.1f; //meter
        private float _thermalTickDuration = 0.25f; //herz
        private readonly List<IThermalObject> _thermalObjects;
        
        public RoomThermalManagerBuilder()
        {
            _thermalObjects = new List<IThermalObject>();
        }

        public IRoom Room { get; set; } = null;

        public float ThermalPixelSize
        {
            get => _thermalPixelSize;
            set
            {
                #region Validation

                if (float.IsNaN(value))
                    throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "NaN is an invalid size value.");

                if (float.IsPositiveInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "Positive infinity is an invalid size value.");

                if (float.IsNegativeInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "Negative infinity is an invalid size value.");

                if (value <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "Zero or negative values are invalid size values.");

                #endregion

                _thermalPixelSize = value;
            }
        }

        public float ThermalTickDuration
        {
            get => _thermalTickDuration;
            set
            {
                #region Validation

                if (float.IsNaN(value))
                    throw new ArgumentOutOfRangeException(nameof(ThermalTickDuration), value, "NaN is an invalid duration value.");

                if (float.IsPositiveInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(ThermalTickDuration), value, "Positive infinity is an invalid duration value.");

                if (float.IsNegativeInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(ThermalTickDuration), value, "Negative infinity is an invalid duration value.");

                if (value <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(ThermalTickDuration), value, "Zero or negative values are invalid duration values.");

                #endregion
                
                _thermalPixelSize = value;
            }
        }

        public ITemperatureSource OutsideTemperature { get; set; }

        public ITemperatureSource<Vector3> InitialRoomTemperature { get; set; }

        public RoomThermalManagerBuilder AddThermalObject(IThermalObject thermalObject)
        {
            _thermalObjects.Add(thermalObject);
            return this;
        }

        public IRoomThermalManager Build()
        {
            if (Room == null)
                throw new InvalidOperationException($"No value for the property {nameof(Room)} was set.");

            return new RoomThermalManager(
                Room, 
                _thermalPixelSize, 
                _thermalTickDuration,
                OutsideTemperature ?? new ConstantTemperatureSource(Temperature.FromCelsius(10f)), 
                InitialRoomTemperature ?? new ConstantTemperatureSource(Temperature.FromCelsius(22f)), 
                _thermalObjects);
        }
    }
}
