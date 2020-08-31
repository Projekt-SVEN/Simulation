using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Simulation
{
    public class NoisyTemperatureSource :
        ITemperatureSource,
        ITemperatureSource<float>,
        ITemperatureSource<Vector2>,
        ITemperatureSource<Vector3>
    {

        private readonly float _minTemperatureValue;
        private readonly float _maxTemperatureValue;

        public NoisyTemperatureSource(Temperature baseTemperatureValue, Temperature noiseOffset)
        {
            const float AbsoluteZeroInKelvin = 0f;

            float noiseOffsetInKelvin = noiseOffset.ToKelvin();
            float baseTemperatureInKelvin = baseTemperatureValue.ToKelvin();

            _minTemperatureValue = baseTemperatureInKelvin - noiseOffsetInKelvin;
            _maxTemperatureValue = baseTemperatureInKelvin + noiseOffsetInKelvin;



            if (_maxTemperatureValue < AbsoluteZeroInKelvin)
            {
                Debug.LogWarning("Max Temperature of NoisyTemperatureSource is below absolute zero.");

                _maxTemperatureValue = AbsoluteZeroInKelvin;
                _minTemperatureValue = AbsoluteZeroInKelvin;
            }
            else if (_minTemperatureValue < AbsoluteZeroInKelvin)
            {
                Debug.LogWarning("Min Temperature of NoisyTemperatureSource is below absolute zero.");

                _minTemperatureValue = AbsoluteZeroInKelvin;
            }
        }

        public Temperature GetTemperature()
        {
            return Temperature.FromKelvin(Random.Range(_minTemperatureValue, _maxTemperatureValue));
        }

        public Temperature GetTemperature(float position) => GetTemperature();

        public Temperature GetTemperature(Vector2 position) => GetTemperature();

        public Temperature GetTemperature(Vector3 position) => GetTemperature();
    }
}