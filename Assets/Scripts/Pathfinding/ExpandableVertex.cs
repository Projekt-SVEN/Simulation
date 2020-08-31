using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public class ExpandableVertex
    {
        public ExpandableVertex(Vertex vertex, float cost, Path currPath)
        {
            this.Vertex = vertex;
            this.Cost = cost;
            this.CurrPath = new Path();
            Vertex v = null;
            // We got to copy our path manually, as it messes up when we try: this.CurrPath = currPath. It's a reference assignment somehow
            currPath.First();
            while (currPath.TryGetNextVertex(out v))
            {
                CurrPath.AddVertex(v);
            }
        }

        public Vertex Vertex { get; private set; }
        public float Cost { get; private set; }

        public Path CurrPath { get; private set; }
    }
}