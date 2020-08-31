using Assets.Scripts.Simulation.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public class OutsideTemperatureSource :
        ITemperatureSource,
        ITemperatureSource<float>,
        ITemperatureSource<Vector2>,
        ITemperatureSource<Vector3>
    {
        static OutsideTemperatureSource()
        {
            Instance = new OutsideTemperatureSource();
        }

        private OutsideTemperatureSource()
        {
        }

        public static OutsideTemperatureSource Instance { get; }

        public Temperature GetTemperature()
        {
            return new Temperature(OptionsManager.OutsideTemperature, TemperatureUnit.Celsius);
        }

        public Temperature GetTemperature(float parameter)
        {
            return new Temperature(OptionsManager.OutsideTemperature, TemperatureUnit.Celsius);
        }

        public Temperature GetTemperature(Vector2 parameter)
        {
            return new Temperature(OptionsManager.OutsideTemperature, TemperatureUnit.Celsius);
        }

        public Temperature GetTemperature(Vector3 parameter)
        {
            return new Temperature(OptionsManager.OutsideTemperature, TemperatureUnit.Celsius);
        }
    }
}
