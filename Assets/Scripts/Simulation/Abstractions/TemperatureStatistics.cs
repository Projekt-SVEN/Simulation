using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Simulation.Abstractions
{
    /// <summary>
    /// A read-only data structure that stores some statistical values about a set of <see cref="Temperature"/> values.
    /// </summary>
    public class TemperatureStatistics
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TemperatureStatistics"/>.
        /// </summary>
        /// <param name="minimum">
        /// The lowest <see cref="Temperature"/> of a set of <see cref="Temperature"/> values.
        /// </param>
        /// <param name="maximum">
        /// The highest <see cref="Temperature"/> of a set of <see cref="Temperature"/> values.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="minimum"/> is higher than <paramref name="maximum"/>.
        /// </exception>
        public TemperatureStatistics(
            Temperature minimum,
            Temperature maximum)
        {
            Minimum = minimum;
            Maximum = maximum;

            if (Minimum > Maximum)
                throw new ArgumentException("Minimum is higher than Maximum.");
        }

        /// <summary>
        /// Lowest <see cref="Temperature"/> of the set.
        /// </summary>
        public readonly Temperature Minimum;

        /// <summary>
        /// Mean <see cref="Temperature"/> of the set.
        /// </summary>
        public readonly Temperature Maximum;
    }
}
