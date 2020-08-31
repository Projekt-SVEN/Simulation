//#define fixtabelegdes

using Assets.Scripts.RoomCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public class Graph
    {
        private sealed class MutableVertex : Vertex
        {
            private List<Edge> _edges;

            public MutableVertex(Vector2 position) : base(position)
            {
                _edges = new List<Edge>();
            }

            public override IReadOnlyList<Edge> Edges => _edges;

            public void AddEdge(Edge edge)
            {
                _edges.Add(edge);
            }

            public void RemoveEdge(Vertex targe)
            {
                for(int i = 0; i < _edges.Count; i++)
                {
                    if (_edges[i].Target == targe)
                    {
                        _edges.RemoveAt(i);
                        break;
                    }
                }
            }

            public bool Contains(Vertex target)
            {
                foreach(Edge edge in Edges)
                {
                    if(edge.Target == target)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private const float CellLength = 0.5f;
        private const int HashTableLength = 1024;
        private List<Vertex>[] _vertexHashTable;

        private readonly List<Vertex> _vertices;

        private List<Edge> _tabelEdges;

        public Graph()
        {
            _vertices = new List<Vertex>();
            _vertexHashTable = new List<Vertex>[HashTableLength];

			for (int i = 0; i < _vertexHashTable.Length; ++i)
            {
                _vertexHashTable[i] = new List<Vertex>();
            }

            _tabelEdges = new List<Edge>();
        }

        public IReadOnlyList<Vertex> Vertices => _vertices;

        public Vertex AddVertex(Vector2 position)
        {
            MutableVertex vertex = new MutableVertex(position);

            _vertices.Add(vertex);
            _vertexHashTable[CalculateHashIndex(position)].Add(vertex);
            
            return vertex;
        }

        private static uint CalculateHashIndex(Vector2 position)
        {
            const long Prime1 = 73856093L;
            const long Prime2 = 19349663L;

            // "+ 2147483648L" grantees that the factors are not negative
            long factor1 = (long)Mathf.Floor(position.x / CellLength) + 2147483648L;
            long factor2 = (long)Mathf.Floor(position.y / CellLength) + 2147483648L;

            long hashIndex = (factor1 * Prime1 ^ factor2 * Prime2) % HashTableLength;

            return (uint)hashIndex;
        }

        public void AddEdgeBetweenVertices(Vertex endpoint1, Vertex endpoint2)
        {
            if (endpoint1.Equals(endpoint2))
                throw new NotSupportedException("Self-loops are not supported.");

            if (endpoint1.Edges.Select(edge => edge.Target).Contains(endpoint2))
                return;

            if (endpoint1 is MutableVertex mutableEndpoint1 &&
                endpoint2 is MutableVertex mutableEndpoint2)
            {
                float cost = (endpoint1.Position - endpoint2.Position).sqrMagnitude;

                mutableEndpoint1.AddEdge(new Edge(mutableEndpoint2, mutableEndpoint1, cost));
                mutableEndpoint2.AddEdge(new Edge(mutableEndpoint1, mutableEndpoint2, cost));
            }
            else
            {
                throw new NotSupportedException("Vertex Type is not supported.");
            }
        }

        public void AddSqaureObject( RoomObject obj)
        {
            #region Offset zu den Node ermitteln

            float cosinus = Mathf.Cos((float)obj.RotationRadians);
            float sinus = Mathf.Sin((float)obj.RotationRadians);            

#if SinCosDebugging
            Debug.Log("Sin: " + sinus.ToString());
            Debug.Log("Cos: " + cosinus.ToString());
#endif

            //Nodes des Vierecks zwischenspeichern, für alle weitern Schritte
            //Ecke A
            MutableVertex[] squareVertexPositions = new MutableVertex[8];
            squareVertexPositions[0] = new MutableVertex(new Vector2(
                x: obj.PositionX,
                y: obj.PositionY));

            //Nächste Ecke B
            squareVertexPositions[1] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x + cosinus * obj.SizeWidth,
                y: squareVertexPositions[0].Position.y + sinus * obj.SizeWidth));


            //Nächste Ecke D
            squareVertexPositions[3] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x - sinus * obj.SizeHeight,
                y: squareVertexPositions[0].Position.y + cosinus * obj.SizeHeight));


            //Ecke C
            squareVertexPositions[2] = new MutableVertex(new Vector2(
                x: squareVertexPositions[3].Position.x + cosinus * obj.SizeWidth,
                y: squareVertexPositions[3].Position.y + sinus * obj.SizeWidth));


            //Mitte AB
            squareVertexPositions[4] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x + cosinus * obj.SizeWidth / 2,
                y: squareVertexPositions[0].Position.y + sinus * obj.SizeWidth / 2));


            //Mitte DA
            squareVertexPositions[7] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x - sinus * obj.SizeHeight / 2,
                y: squareVertexPositions[0].Position.y + cosinus * obj.SizeHeight / 2));

            //Mitte CD
            squareVertexPositions[6] = new MutableVertex(new Vector2(
                x: squareVertexPositions[3].Position.x + cosinus * obj.SizeWidth / 2,
                y: squareVertexPositions[3].Position.y + sinus * obj.SizeWidth / 2));


            //Mitte BC
            squareVertexPositions[5] = new MutableVertex(new Vector2(
                x: squareVertexPositions[1].Position.x - sinus * obj.SizeHeight / 2,
                y: squareVertexPositions[1].Position.y + cosinus * obj.SizeHeight / 2));

            //Kanten die nicht passiert werden dürfen anlegen
            _tabelEdges.Add(new Edge(squareVertexPositions[0], squareVertexPositions[2], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[3], squareVertexPositions[1], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[4], squareVertexPositions[6], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[5], squareVertexPositions[4], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[3], squareVertexPositions[4], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[7], squareVertexPositions[4], 0));

            //Diagonale des Rechtecks!!
            Vector2 offset = (squareVertexPositions[3].Position - squareVertexPositions[0].Position) + 
                (squareVertexPositions[1].Position - squareVertexPositions[0].Position);
            offset = -offset;   //Direction umdrehen
            offset.Normalize();

            #endregion

            float newWidth = obj.SizeWidth + (2 * OptionsManager.VertexObjectOffSet);
            float newHeigth = obj.SizeHeight + (2 * OptionsManager.VertexObjectOffSet);

            //Ecke A
            squareVertexPositions[0] = new MutableVertex(squareVertexPositions[0].Position + offset * (Mathf.Sqrt(2) * OptionsManager.VertexObjectOffSet));

            //Ecke B
            squareVertexPositions[1] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x + cosinus * newWidth,
                y: squareVertexPositions[0].Position.y + sinus * newWidth));


            //Ecke D
            squareVertexPositions[3] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x - sinus * newHeigth,
                y: squareVertexPositions[0].Position.y + cosinus * newHeigth));
            

            //Ecke C
            squareVertexPositions[2] = new MutableVertex(new Vector2(
                x: squareVertexPositions[3].Position.x + cosinus * newWidth,
                y: squareVertexPositions[3].Position.y + sinus * newWidth));

            //Mitte AB
            squareVertexPositions[4] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x + cosinus * newWidth / 2,
                y: squareVertexPositions[0].Position.y + sinus * newWidth / 2));


            //Mitte DA
            squareVertexPositions[7] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x - sinus * newHeigth / 2,
                y: squareVertexPositions[0].Position.y + cosinus * newHeigth / 2));

            //Mitte CD
            squareVertexPositions[6] = new MutableVertex(new Vector2(
                x: squareVertexPositions[3].Position.x + cosinus * newWidth / 2,
                y: squareVertexPositions[3].Position.y + sinus * newWidth / 2));


            //Mitte BC
            squareVertexPositions[5] = new MutableVertex(new Vector2(
                x: squareVertexPositions[1].Position.x - sinus * newHeigth / 2,
                y: squareVertexPositions[1].Position.y + cosinus * newHeigth / 2));

            foreach (MutableVertex vertex in squareVertexPositions)
            {
                _vertexHashTable[0].Add(vertex);
            }
        }

        public void MeshGraph()
        {
            for (int i = 0; i < _vertexHashTable[0].Count; i++)
            {
                for (int j = i + 1; j < _vertexHashTable[0].Count; j++)
                {
                    MutableVertex vertex = (MutableVertex)_vertexHashTable[0][i];
                    if (!vertex.Contains(_vertexHashTable[0][j]))
                    {
                        bool possible = true;
                        foreach (Edge edge in _tabelEdges)
                        {
                            if (edge.CutEdge(_vertexHashTable[0][i].Position, _vertexHashTable[0][j].Position))
                            {
                                possible = false;
                                break;
                            }
                        }
                        if (possible)
                        {
                            AddEdgeBetweenVertices(_vertexHashTable[0][i], _vertexHashTable[0][j]);
                        }
                    }
                }
            }
            //Nicht genutzet Vertex aufräumen und letzte Fragmente entfernen
            for (int i = 0; i < _vertexHashTable[0].Count; i++)
            {
                if (_vertexHashTable[0][i].Edges.Count < 5)
                {
                    CleanGraph((MutableVertex)_vertexHashTable[0][i]);
                }
            }
        }

        private void CleanGraph(MutableVertex vertex)
        {
            for(int i = 0; i < vertex.Edges.Count; i++)
            {
                MutableVertex nextVertex = (MutableVertex)vertex.Edges[i].Target;
                nextVertex.RemoveEdge(vertex);
                vertex.RemoveEdge(nextVertex);
                //Angrenzende Vertex Kontrollieren
                if(nextVertex.Edges.Count < 5)
                {
                    CleanGraph(nextVertex);
                }
            }
            _vertexHashTable[0].Remove(vertex);
        }
		
		public Vertex GetNearestVertex(Vector2 position, int maxDistance = 20)
        {
            Vertex nearestVertex = null;
            float distanceToNearestVertex = float.MaxValue;

            for (int distance = 0; nearestVertex == null && distance <= maxDistance; ++distance)
            {
                foreach (Vector2 offset in GetOffsets(distance))
                {
                    foreach (Vertex vertex in _vertexHashTable[CalculateHashIndex(position + offset)])
                    {
                        if (nearestVertex == null)
                        {
                            nearestVertex = vertex;
                            distanceToNearestVertex = position.GetDistanceTo(nearestVertex.Position);
                        }

                        float distanceToVertex = position.GetDistanceTo(vertex.Position);
                        
                        if (distanceToVertex < distanceToNearestVertex)
                        {
                            nearestVertex = vertex;
                            distanceToNearestVertex = distanceToVertex;
                        }
                    }
                }
            }

            return nearestVertex;

            IEnumerable<Vector2> GetOffsets(int distance)
            {
                if (distance == 0)
                {
                    yield return new Vector2(0, 0);
                }
                else
                {
                    Vector2 offset = new Vector2(distance * CellLength, distance * CellLength);

                    int limit = 2 * distance;

                    for (int i = 0; i < limit; ++i)
                    {
                        offset += new Vector2(-CellLength, 0);

                        yield return offset;
                    }

                    for (int i = 0; i < limit; ++i)
                    {
                        offset += new Vector2(0, CellLength);

                        yield return offset;
                    }

                    for (int i = 0; i < limit; ++i)
                    {
                        offset += new Vector2(CellLength, 0);

                        yield return offset;
                    }

                    for (int i = 0; i < limit; ++i)
                    {
                        offset += new Vector2(0, -CellLength);

                        yield return offset;
                    }
                }
            }
        }

        public static ExpandableVertex FindLowestCost(List<ExpandableVertex> vertexList)
        {
            float lowestCost = vertexList[0].Cost;
            int indexOfLowestCost = 0;

            for (int i = 1; i < vertexList.Count; i++)
            {
                if (vertexList[i].Cost < lowestCost)
                {
                    indexOfLowestCost = i;
                    lowestCost = vertexList[i].Cost;
                }
            }

            ExpandableVertex bestVertex = vertexList[indexOfLowestCost];
            vertexList.RemoveAt(indexOfLowestCost);
            return bestVertex;
        }

        /// <summary>
        /// The method runs as long as there's no end found or as long as there're vertices to expand.
        /// For any new found vertex, an object of the "ExpandableVertex" is instantiated, where the current path gets saved. That way, there can't be
        /// any conflicts with "jumping in the graph"
        /// The method won't append a vertex that has been expanded already
        /// And to increase performance, whe method "InsertBetterVertex" was written
        /// </summary>
        /// <param name="start"></param> Start Vertex
        /// <param name="end"></param> End Vertex
        /// <param name="p"></param> The way we pass the result
        /// <returns></returns>
        public static bool GetPathTo(Vertex start, Vertex end, out Path p)
        {
            p = new Path();
            // Whether the user was so funny to give the same start and end coordinate
            if (start.Equals(end))
            {
                p.AddVertex(start);
                return true;
            }
            List<ExpandableVertex> toExpandVertices = new List<ExpandableVertex>();
            List<Vertex> listOfAlreadyExpandedVertices = new List<Vertex>();

            bool endFound = false;
            toExpandVertices.Add(new ExpandableVertex(start, 0, p));

            //As long as we haven't found an end and there're still possibilities
            while (!endFound && toExpandVertices.Count > 0)
            {
                ExpandableVertex vertexWrapper = FindLowestCost(toExpandVertices);
                p = vertexWrapper.CurrPath;
                p.AddVertex(vertexWrapper.Vertex);

                // look for all connections
                foreach (Edge edge in vertexWrapper.Vertex.Edges)
                {
                    if (edge.Target.Equals(end))
                    {
                        p.AddVertex(end);
                        endFound = true;
                        break;
                    }
                    else if (!listOfAlreadyExpandedVertices.Contains(edge.Target))
                    {
                        InsertBetterVertex(toExpandVertices, new ExpandableVertex(edge.Target, vertexWrapper.Cost + edge.Cost, p));
                    }
                    //look if the end was found

                }
                listOfAlreadyExpandedVertices.Add(vertexWrapper.Vertex);
            }
            return endFound;
        }

        /// <summary>
        /// for instance: We have the graph:
        ///     a 2---2 b
        ///     5\  c  /3
        /// Where a finds a new ExpandableVertex "c" with the cost of 5 and "b" with the cost of 2
        /// As b also finds the ExpandableVertex "c", but with lower cost than "a" does, the ExpandableVertex "a" found get's kicked out
        /// </summary>
        /// <param name="list"></param>
        /// <param name="v"></param>
        private static void InsertBetterVertex(List<ExpandableVertex> list, ExpandableVertex v)
        {
            bool found = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Vertex == v.Vertex)
                {
                    found = true;
                    if (v.Cost < list[i].Cost)
                    {
                        list[i] = v;
                    }
                    break;
                }
            }
            if (!found)
            {
                list.Add(v);
            }
        }
        public void PrintGraph()
        {
            int[,] adjazensmatrix = new int[_vertexHashTable[0].Count, _vertexHashTable[0].Count]; 
            StringBuilder graphmatrix = new StringBuilder();
            StringBuilder vertexPositions = new StringBuilder();

            for(int j = 0; j < _vertexHashTable[0].Count; j++)
            {
                vertexPositions.Append(_vertexHashTable[0][j].Position.x.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + " " + _vertexHashTable[0][j].Position.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + "; ");
                foreach(Edge edge in _vertexHashTable[0][j].Edges)
                {
                    for(int i = 0; i < _vertexHashTable[0].Count; i++)
                    {
                        if(edge.Target.Equals(_vertexHashTable[0][i]))
                        {
                            adjazensmatrix[i, j] = 1;
                            adjazensmatrix[j, i] = 1;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < adjazensmatrix.GetLength(0); i++)
            {
                for(int j = 0; j < adjazensmatrix.GetLength(0); j++)
                {
                    graphmatrix.Append(adjazensmatrix[i, j] + " ");
                }
                graphmatrix.Append(";\r\n");
            }

            Debug.Log(graphmatrix.ToString());
            Debug.Log(vertexPositions.ToString());
        }
    }
}
