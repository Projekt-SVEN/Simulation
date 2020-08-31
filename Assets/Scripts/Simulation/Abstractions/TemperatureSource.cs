using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Simulation.Abstractions
{
    public interface ITemperatureSource
    {
        Temperature GetTemperature();
    }

    public interface ITemperatureSource<TParameter>
    {
        Temperature GetTemperature(TParameter parameter);
    }
}
