using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Simulation.Abstractions;

namespace Assets.Scripts.Simulation
{
    public class TemperatureStatisticsBuilder
    {
        private readonly TemperatureUnit _normalTemperatureUnit;

        private float _minimum = float.PositiveInfinity;
        private float _maximum = float.NegativeInfinity;

        public TemperatureStatisticsBuilder(TemperatureUnit normalTemperatureUnit = TemperatureUnit.Celsius)
        {
            _normalTemperatureUnit = normalTemperatureUnit;
        }

        public TemperatureStatisticsBuilder AddTemperatureValue(Temperature value)
        {
            value = value.To(_normalTemperatureUnit);
            float val = value.Value;

            if (val > _maximum)
            {
                _maximum = val;
            }

            if (val < _minimum)
            {
                _minimum = val;
            }

            return this;
        }

        public TemperatureStatistics Build()
        {
            return new TemperatureStatistics(
                minimum: new Temperature(_minimum, _normalTemperatureUnit),
                maximum: new Temperature(_maximum, _normalTemperatureUnit));
        }
    }
}
