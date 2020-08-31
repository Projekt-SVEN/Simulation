using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public class ConstantTemperatureSource : 
        ITemperatureSource, 
        ITemperatureSource<float>, 
        ITemperatureSource<Vector2>, 
        ITemperatureSource<Vector3>
    {
        private readonly Temperature _constantTemperatureValue;

        public ConstantTemperatureSource(Temperature constantTemperatureValue)
        {
            _constantTemperatureValue = constantTemperatureValue;
        }

        public Temperature GetTemperature()
        {
            return _constantTemperatureValue;
        }

        public Temperature GetTemperature(float position)
        {
            return _constantTemperatureValue;
        }

        public Temperature GetTemperature(Vector2 position)
        {
            return _constantTemperatureValue;
        }

        public Temperature GetTemperature(Vector3 position)
        {
            return _constantTemperatureValue;
        }
    }
}
