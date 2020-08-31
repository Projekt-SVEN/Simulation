using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Pathfinding
{
    public class Path
    {

        int listIndex = -1;
        List<Vertex> VertexPath = new List<Vertex>();
        public Path()
        {
        }

        public void Next()
        {
            listIndex++;
        }

        public bool HasActVertex() => listIndex < VertexPath.Count;

        public void AddVertex(Vertex v)
        {
            VertexPath.Add(v);
        }

        public bool HasNextVertex => listIndex + 1 < VertexPath.Count;

        public void First()
        {
            listIndex = -1;
        }

        public bool TryGetNextVertex(out Vertex nextVertex)
        {
            if ((++listIndex) < VertexPath.Count)
            {
                nextVertex = VertexPath[listIndex];
                return true;
            }
            nextVertex = null;
            return false;
        }
        public bool TryToConfirmPath()
        {
            Vertex vertex = VertexPath[0];
            Vertex backup;
            bool errorInside = false;
            for (int i = 1; !errorInside && i < VertexPath.Count - 1; i++)
            {
                backup = vertex;
                foreach (Edge edge in vertex.Edges)
                {
                    if (edge.Target == VertexPath[i])
                    {
                        vertex = edge.Target;
                    }
                }
                //if vertex would still equal to backup, then we would've an error as vertex did not move to the next vertex (as the next in "path" wasn't connected to any edge of "vertex")
                errorInside = vertex.Equals(backup);
            }
            return !errorInside;
        }

    }
}
