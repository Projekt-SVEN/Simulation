using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Simulation.Abstractions
{
    public interface IDateTimeSource
    {
        DateTime Now { get; }
    }
}
