using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Remote;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Simulation
{
    public enum LectureState
    {
        Pause,
        Lecture,
        None
    }

    public class UserGroupController : MonoBehaviour
    {
        private bool _initialized = false;

        private object _goToTabletLock;
        private bool _doesSomeoneGoToTablet = false;
        
        [SerializeField] 
        private GameObject _userPrefab;

        private List<UserController> _students;

        private List<Vertex> _unoccupiedSeats;
        private IRoomThermalManager _roomThermalManager;

        public IRoomThermalManager RoomThermalManager
        {
            get => _roomThermalManager;
            set
            {
                _goToTabletLock = new object();

                var studentChairs = value.Room.RoomGraph.StudentsChairs;

                _unoccupiedSeats = new List<Vertex>(studentChairs);
                _students = new List<UserController>();

                _roomThermalManager = value;

                _initialized = true;
            }
        }

        public UserController Lecturer { get; set; } = null;

        public IReadOnlyList<UserController> Students => _students;

        private int UserCount => _students.Count + (Lecturer == null ? 0 : 1);

        private bool oneTime = false;

        void Update()
        {
            if (!_initialized)
                return;

            if (oneTime)
                return;

            oneTime = true;

            CreateUser(UserController.UserRole.Student);
            /*
            if (RoomThermalManager.Room.LectureState == LectureState.None)
                return;

            int userCount = UserCount;

            if (userCount < OptionsManager.UserCount)
            {
                if (Lecturer == null)
                {
                    CreateUser(UserController.UserRole.Lecturer);
                    ++userCount;
                }

                for (int i = userCount; i < OptionsManager.UserCount; ++i)
                {
                    CreateUser(UserController.UserRole.Student);
                }
            }
            else if (userCount > OptionsManager.UserCount)
            {
                if (OptionsManager.UserCount == 0 && Lecturer != null)
                {
                    Lecturer.LeaveRoom();
                    --userCount;
                }

                for (int i = 0; userCount > 0 && i < _students.Count; ++i)
                {
                    UserController student = _students[i];

                    if (student.State != UserController.UserState.LeavingRoom)
                    {
                        student.LeaveRoom();
                        --userCount;
                    }
                }
            }
            */
        }

        private void CreateUser(UserController.UserRole role)
        {
            Vertex seat = GetFreeSeat(role);

            if (seat == null)
                return;

            GameObject userObject = Instantiate(_userPrefab);
            userObject.transform.parent = transform;
            UserController userController = userObject.GetComponent<UserController>();
            userController.Initialize(userGroupController: this, role, seat);
            userController.Destroyed += OnUserControllerDestroyed;
            RoomThermalManager.AddThermalObject(userController);

            if (role == UserController.UserRole.Student)
                _students.Add(userController);
            else if (role == UserController.UserRole.Lecturer)
                Lecturer = userController;
            else
                throw new NotImplementedException();
        }

        private Vertex GetFreeSeat(UserController.UserRole role)
        {
            if (role == UserController.UserRole.Student)
            {
                return OccupySeat();
            }
            else if (role == UserController.UserRole.Lecturer)
            {
                IReadOnlyList<Vertex> lecturerChairs = RoomThermalManager.Room.RoomGraph.LecturerChairs;

                if (lecturerChairs.Count == 0)
                    return null;

                return RoomThermalManager.Room.RoomGraph.LecturerChairs[0];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void OnUserControllerDestroyed(UserController destroyedUserController)
        {
            RoomThermalManager.RemoveThermalObject(destroyedUserController);

            if (destroyedUserController == Lecturer)
            {
                Lecturer = null;
            }
            else
            {
                _students.Remove(destroyedUserController);
            }

            destroyedUserController.Destroyed -= OnUserControllerDestroyed;
        }

        private Vertex OccupySeat()
        {
            lock (_unoccupiedSeats)
            {
                if (_unoccupiedSeats.Count == 0)
                    return null;

                int index = Random.Range(0, _unoccupiedSeats.Count);

                Vertex seat = _unoccupiedSeats[index];
                _unoccupiedSeats.RemoveAt(index);

                return seat;
            }
        }

        public void UnoccupySeat(Vertex seat)
        {
            lock (_unoccupiedSeats)
            {
                _unoccupiedSeats.Add(seat);
            }
        }

        public bool RequestToGoToTablet()
        {
            lock (_goToTabletLock)
            {
                if (_doesSomeoneGoToTablet)
                    return false;

                return _doesSomeoneGoToTablet = true;
            }
        }

        public void CancelGoToTabletRequest()
        {
            lock (_goToTabletLock)
            {
                _doesSomeoneGoToTablet = false;
            }
        }
    }
}
