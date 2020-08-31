using System.Collections.Generic;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Remote;
using UnityEngine;

namespace Assets.Scripts.RoomCreation
{
    public class RoomGraph : Graph
    {
        private List<Vertex> _doors;

        private List<Vertex> _studentsChairs;

        private List<Vertex> _lecturerChairs;

        private List<(Vertex Vertex, RemoteTablet RemoteTablet)> _tablets;

        public RoomGraph()
        {
            _doors = new List<Vertex>();
            _studentsChairs = new List<Vertex>();
            _lecturerChairs = new List<Vertex>();
            _tablets = new List<(Vertex Vertex, RemoteTablet RemoteTablet)>();
        }

        public IReadOnlyList<Vertex> Doors => _doors;

        public IReadOnlyList<Vertex> StudentsChairs => _studentsChairs;

        public IReadOnlyList<Vertex> LecturerChairs => _lecturerChairs;

        public IReadOnlyList<(Vertex Vertex, RemoteTablet RemoteTablet)> Tablets => _tablets;

        public Vertex AddTablet(Vector2 position, RemoteTablet remoteTablet)
        {
            Vertex tabletVertex = AddVertex(position);

            _tablets.Add((tabletVertex, remoteTablet));

            return tabletVertex;
        }

        public Vertex AddDoor(Vector2 position)
        {
            Vertex doorVertex = AddVertex(position);

            _doors.Add(doorVertex);

            return doorVertex;
        }

        public Vertex AddStudentsChair(Vector2 position)
        {
            Vertex studentChair = AddVertex(position);

            _studentsChairs.Add(studentChair);

            return studentChair;
        }

        public Vertex AddLecturerChair(Vector2 position)
        {
            Vertex lecturerChair = AddVertex(position);

            _lecturerChairs.Add(lecturerChair);

            return lecturerChair;
        }
    }
}
