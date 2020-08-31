using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class MathematicsHelper
    {
        public static float GetDistanceTo(this Vector2 a, Vector2 b)
        {
            float xDifference = a.x - b.x;
            float yDifference = a.y - b.y;

            return Mathf.Sqrt(xDifference * xDifference + yDifference * yDifference);
        }
    }
}
