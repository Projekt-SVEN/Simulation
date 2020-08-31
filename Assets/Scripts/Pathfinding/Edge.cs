using System;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public sealed class Edge
    {
        public Edge(Vertex target, Vertex start,  float cost)
        {
            if (cost < 0)
                throw new ArgumentOutOfRangeException(nameof(cost), cost, "Negative edge costs are not allowed.");

            Target = target;
            Cost = cost;
            _directionVector = start.Position - target.Position;

            if(start.Position.x - target.Position.x != 0)
            {
                _increase = (start.Position.y - target.Position.y) / (start.Position.x - target.Position.x);
                _offsetYAxes = target.Position.y - target.Position.x * _increase;
            }
            else
            {
                //darstellen vor Linearen Funktionen der Form z.B. x = 1;
                _increase = start.Position.x;
                _offsetYAxes = float.PositiveInfinity;
            }

        }

        public Vertex Target { get; }

        public float Cost { get; }

        private Vector2 _directionVector;

        private float _increase;

        private float _offsetYAxes;

        public bool CutEdge(Vector2 startPoint, Vector2 endPoint)
        {
            //Vorbereitung Position
            Vector2 directionVectorScondEdge = endPoint - startPoint;


            float t1; // from x = p0 + t * a (x p0 and a are vectoren)
            float t2;

            float increaseScondEdge;

            float offsetYAxesScondEdge;

            if (endPoint.x - startPoint.x != 0)
            {
                increaseScondEdge = (endPoint.y - startPoint.y) / (endPoint.x - startPoint.x);
                offsetYAxesScondEdge = startPoint.y - startPoint.x * increaseScondEdge;
            }
            else
            {
                //darstellen vor Linearen Funktionen der Form z.B. x = 1;
                increaseScondEdge = startPoint.x;
                offsetYAxesScondEdge = float.PositiveInfinity;
            }


            Vector2 intersection = new Vector2();

            //IF Intersection bestimmen
            //kontrolle auf paralelität
            if ( Mathf.Abs(increaseScondEdge) == Mathf.Abs(_increase))
            {
                if(offsetYAxesScondEdge == float.PositiveInfinity && _offsetYAxes == float.PositiveInfinity)
                {
                    if(_increase != increaseScondEdge)
                    {
                        return false;
                    }
                }
                if(startPoint.y != startPoint.x * _increase + _offsetYAxes)
                {
                    return false;
                }
                return ControlCutOfParrallelCuts(startPoint, endPoint);
            }
            else if(offsetYAxesScondEdge != float.PositiveInfinity && _offsetYAxes == float.PositiveInfinity)
            {
                intersection.x = _increase;
                intersection.y = intersection.x * increaseScondEdge + offsetYAxesScondEdge;
            }
            else if(offsetYAxesScondEdge == float.PositiveInfinity && _offsetYAxes != float.PositiveInfinity)
            {
                intersection.x = increaseScondEdge;
                intersection.y = intersection.x * _increase + _offsetYAxes;
            }
            else if (offsetYAxesScondEdge == float.PositiveInfinity && _offsetYAxes == float.PositiveInfinity)
            {
                if (_increase != increaseScondEdge)
                {
                    return false;
                }
                return ControlCutOfParrallelCuts(startPoint, endPoint);
            }
            else
            {
                /* Der Punkt S1 ist der Schnittpunkt der Geraden f(x1) und f(x2), daraus folgt:
                 * m1 * x + n1 = m2 * x + n2 |-(m2 * x) - n1
                 * m1 * x - m2 * x = n2 - n1
                 * x * (m1 - m2) = n2 - n1  |/(m1 -m2)
                 * x = (n2 - n1)/(m1 - m2)
                 */
                intersection.x = (offsetYAxesScondEdge - _offsetYAxes) / (_increase - increaseScondEdge);

                intersection.y = intersection.x * _increase + _offsetYAxes;
            }//end if Intersectionberechnung

            if (_directionVector.x != 0)
            {
                t1 = (intersection.x - Target.Position.x) / _directionVector.x;
            }
            else
            {
                if (_directionVector.y == 0)
                {
                    throw new Exception("Direction Vektor is null Vektor!!");
                }
                t1 = (intersection.y - Target.Position.y) / _directionVector.y;
            }

            if (directionVectorScondEdge.x != 0)
            {
                t2 = (intersection.x - startPoint.x) / directionVectorScondEdge.x;
            }
            else
            {
                if (directionVectorScondEdge.y == 0)
                {
                    throw new Exception("Direction Vektor is null Vektor!!");
                }
                t2 = (intersection.y - startPoint.y) / directionVectorScondEdge.y;
            }

            if (t1 > 1 || t1 < 0 || t2 > 1 || t2 < 0)
            {
                return false;
            }
            return true;
        }

        private bool ControlCutOfParrallelCuts(Vector2 startPoint, Vector2 endPoint)
        {
            float t1; // aus x = p0 + t * a (x p0 und a sind Vektoren)
            float t2;

            if (_directionVector.x != 0)
            {
                t1 = (startPoint.x - Target.Position.x) / _directionVector.x;
                t2 = (endPoint.x - Target.Position.x) / _directionVector.x;
            }
            else
            {
                if (Mathf.Abs(_directionVector.y) < 0.01)
                {
                    throw new Exception("Direction vector is null vevtor!!");
}
                t1 = (startPoint.y - Target.Position.y) / _directionVector.y;
                t2 = (endPoint.y - Target.Position.y) / _directionVector.y;
            }

            if ((t1 < 0 && t2 < 0) || (t2 > 1 && t1 > 1))
            {
                return false;
            }
            return true;
        }
    }
}
