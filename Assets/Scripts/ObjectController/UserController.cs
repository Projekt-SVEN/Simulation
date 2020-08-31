using System;
using System.Collections.Generic;
using System.Xml;
using Assets.Scripts;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Remote;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class UserController : MonoBehaviour, IThermalObject
{
    public enum UserRole
    {
        Student,
        Lecturer
    }

    public enum UserState
    {
        Unknown,
        LeavingRoom,
        GoToSeat,
        Listening,
        Lecturing,
        Idle,
        Moving,
        OpeningWindow,
        ClosingWindow,
        TurningUpHeater,
        TurningDownHeater,
    }

    public event Action<UserController> Destroyed;

    private bool _initialized = false;

    private Vertex _lastVertex;
    private Vertex _nextVertex;
    private Path _currentPath = null;

    private Vertex _seat;
    private (Vertex Vertex, RemoteTablet RemoteTablet) _tablet;

    [SerializeField]
    private TextMesh _userStateTextMesh;

    [SerializeField]
    
    private float _normalizedUserSpeed = 0f;
    
    private float _normalizedMaxOkTemperature = 0f;

    private float _normalizedMinOkTemperature = 0f;

    /// <summary>
    /// Gets the movement speed of the user.
    /// </summary>
    public float UserSpeed =>
        Mathf.Lerp(OptionsManager.MinUserSpeed, OptionsManager.MaxUserSpeed, _normalizedUserSpeed);

    /// <summary>
    /// Gets the lowest temperature that is okay for the user. The user will freeze for temperatures below this temperature.
    /// </summary>
    public Temperature MinOkTemperature => Temperature.FromCelsius(Mathf.Lerp(OptionsManager.LowerMinOkUserTemperature,
        OptionsManager.UpperMinOkUserTemperature, _normalizedMinOkTemperature));

    /// <summary>
    /// Gets the highest temperature that is okay for the user. The user will sweat for temperatures above this temperature.
    /// </summary>
    public Temperature MaxOkTemperature => Temperature.FromCelsius(Mathf.Lerp(OptionsManager.LowerMaxOkUserTemperature,
        OptionsManager.UpperMaxOkUserTemperature, _normalizedMaxOkTemperature));

    /// <summary>
    /// Gets if the user is freezing. This indicates that the temperature is too low for the user to be comfortable.
    /// </summary>
    public bool IsFreezing { get; private set; } = false;

    /// <summary>
    /// Gets if the user is sweating. This indicates that the temperature is too high for the user to be comfortable.
    /// </summary>
    public bool IsSweating { get; private set; } = false;

    private IRoomThermalManager RoomThermalManager { get; set; }

    public UserGroupController UserGroupController { get; private set; }

    /// <summary>
    /// Gets the role of the user.
    /// </summary>
    public UserRole Role { get; private set; } = UserRole.Student;

    /// <summary>
    /// Gets the current state of the user.
    /// </summary>
    public UserState State { get; private set; } = UserState.Unknown;

    /// <summary>
    /// Gets if the <see cref="IThermalObject"/> can not change its position.
    /// <see langword="true" /> can not change its position; otherwise <see langword="false"/>.
    /// </summary>
    public bool CanNotChangePosition => false;

    /// <summary>
    /// Gets the absolute (global) Position of the <see cref="IThermalObject"/> in m.
    /// </summary>
    public Vector3 Position => transform.position;

    /// <summary>
    /// Gets how large the <see cref="IThermalObject"/> is in m (meter).
    /// </summary>
    public Vector3 Size => new Vector3(1f, 1f);

    /// <summary>
    /// Gets the area of the surface of the <see cref="IThermalObject"/> in m² (square meter).
    /// </summary>
    public float ThermalSurfaceArea { get; private set; } = 2f;

    /// <summary>
    /// Gets the <see cref="ThermalMaterial"/> of the <see cref="IThermalObject"/>.
    /// </summary>
    /// <remarks>
    /// Used to calculate the temperature and the heat transfer from and to the the <see cref="IThermalObject"/>.
    /// </remarks>
    public ThermalMaterial ThermalMaterial => ThermalMaterial.Human;

    /// <summary>
    /// Gets the temperature of the <see cref="IThermalObject"/>.
    /// </summary>
    public Temperature Temperature => Temperature.FromCelsius(32f);

    public void Initialize(UserGroupController userGroupController, UserRole role, Vertex userSeat)
    {
        if (_initialized)
            throw new InvalidOperationException("User was already initialized!");

        _initialized = true;

        UserGroupController = userGroupController;
        Role = role;

        _seat = userSeat;
    }

    /// <summary>
    /// A <see cref="IRoomThermalManager"/> signals the <see cref="IThermalObject"/> that the thermal simulation was started.
    /// </summary>
    /// <param name="roomThermalManager">
    /// The <see cref="IRoomThermalManager"/> that starts the thermal simulation with this <see cref="IThermalObject"/>. 
    /// </param>
    public void ThermalStart(IRoomThermalManager roomThermalManager)
    {
        RoomThermalManager = roomThermalManager;
    }

    /// <summary>
    /// Is called from the <see cref="IThermalObject"/> once per thermal update.
    /// </summary>
    /// <param name="transferredHeat">
    /// The heat that was transferred to the <see cref="IThermalObject"/> during the thermal update in J (Joule).
    /// </param>
    /// <param name="roomThermalManager">
    /// The <see cref="IRoomThermalManager"/> that does the thermal update.
    /// </param>
    public void ThermalUpdate(float transferredHeat, IRoomThermalManager roomThermalManager)
    {
        //((Skin Surface Area) / (Body Height)) * (Height of Thermal Pixel)
        //((       1m³       ) /     1.8m     ) * (Height of Thermal Pixel) 
        ThermalSurfaceArea = (2f / 1.8f) * roomThermalManager.ThermalPixelSize;
    }

    void Start()
    {
        _normalizedMinOkTemperature = Random.value;
        _normalizedMaxOkTemperature = Random.value;

        Vertex doorVertex = GetRandomDoorVertex();
        gameObject.transform.position = doorVertex.Position;
        _lastVertex = doorVertex;
        _nextVertex = null;
        
        _tablet = RoomThermalManager.Room.RoomGraph.Tablets[0];
    }

    void Update()
    {
        Temperature? temperature = RoomThermalManager?.GetTemperature(transform.position).ToCelsius();

        UpdateTemperatureFeeling(temperature);

        LectureState lectureState = RoomThermalManager.Room.LectureState;

        float routeLength = UserSpeed * Time.deltaTime;
        Vector2 position = transform.position;

        switch (State)
        {
            case UserState.Unknown:
                if (lectureState == LectureState.None)
                {
                    State = UserState.LeavingRoom;
                }
                else if (IsFreezing)
                {
                    State = UserState.TurningUpHeater;
                }
                else if (IsSweating)
                {
                    State = UserState.TurningDownHeater;
                }
                else if (lectureState == LectureState.Lecture)
                {
                    State = Role == UserRole.Lecturer ? UserState.Lecturing : UserState.GoToSeat;
                }
                else if (lectureState == LectureState.Pause)
                {
                    State = Role == UserRole.Lecturer ? UserState.GoToSeat : UserState.Moving;
                }

                State = UserState.Moving;
                break;
            case UserState.Lecturing:
            case UserState.Moving:
                if (lectureState == LectureState.None)
                {
                    State = UserState.LeavingRoom;
                }
                else if (IsFreezing)
                {
                    State = UserState.TurningUpHeater;
                }
                else if (IsSweating)
                {
                    State = UserState.TurningDownHeater;
                }
                else if (lectureState == LectureState.Lecture)
                {
                    State = Role == UserRole.Lecturer ? UserState.Lecturing : UserState.GoToSeat;
                }
                else if (lectureState == LectureState.Pause)
                {
                    State = Role == UserRole.Lecturer ? UserState.GoToSeat : UserState.Moving;
                }

                if (!(State == UserState.Moving || State == UserState.Lecturing))
                    break;

                if (_currentPath == null)
                {
                    if (!SetTarget(GetRandomVertex()))
                    {
                        break;
                    }
                }

                if (_nextVertex == null)
                {
                    if (!_currentPath.TryGetNextVertex(out _nextVertex))
                    {
                        if (!(SetTarget(GetRandomVertex()) &&
                              _currentPath.TryGetNextVertex(out _nextVertex)) )
                        {
                            _nextVertex = null;
                            break;
                        }
                    }
                }

                while (routeLength > 0)
                {
                    Vector2 vectorToNextVertex = _nextVertex.Position - position;
                    float distanceToNextVertex = vectorToNextVertex.sqrMagnitude;

                    if (distanceToNextVertex > routeLength)
                    {
                        position += (vectorToNextVertex.normalized * routeLength);

                        routeLength = 0;
                    }
                    else
                    {
                        position = _nextVertex.Position;
                        _lastVertex = _nextVertex;

                        if (!_currentPath.TryGetNextVertex(out _nextVertex))
                        {
                            if (!(SetTarget(GetRandomVertex()) &&
                                  _currentPath.TryGetNextVertex(out _nextVertex)))
                            {
                                _nextVertex = null;
                                break;
                            }
                        }

                        routeLength -= distanceToNextVertex;
                    }
                }

                transform.position = position;

                break;
            case UserState.LeavingRoom:
                if (_currentPath == null)
                {
                    if (!SetTarget(GetRandomDoorVertex()))
                    {
                        DestroyGameObject();
                    }
                }

                if (_nextVertex == null)
                {
                    if (!_currentPath.TryGetNextVertex(out _nextVertex))
                    {
                        DestroyGameObject();
                    }
                }

                while (routeLength > 0)
                {
                    Vector2 vectorToNextVertex = _nextVertex.Position - position;
                    float distanceToNextVertex = vectorToNextVertex.sqrMagnitude;

                    if (distanceToNextVertex > routeLength)
                    {
                        position += (vectorToNextVertex.normalized * routeLength);

                        routeLength = 0;
                    }
                    else
                    {
                        position = _nextVertex.Position;
                        _lastVertex = _nextVertex;

                        if (!_currentPath.TryGetNextVertex(out _nextVertex))
                        {
                            DestroyGameObject();
                        }

                        routeLength -= distanceToNextVertex;
                    }
                }

                transform.position = position;

                break;
            case UserState.GoToSeat:
                if (_currentPath == null)
                {
                    if (!SetTarget(_seat))
                    {
                        AfterSeatReached();
                    }
                }

                if (_nextVertex == null)
                {
                    if (!_currentPath.TryGetNextVertex(out _nextVertex))
                    {
                        AfterSeatReached();
                    }
                }

                while (routeLength > 0)
                {
                    Vector2 vectorToNextVertex = _nextVertex.Position - position;
                    float distanceToNextVertex = vectorToNextVertex.sqrMagnitude;

                    if (distanceToNextVertex > routeLength)
                    {
                        position += (vectorToNextVertex.normalized * routeLength);

                        routeLength = 0;
                    }
                    else
                    {
                        position = _nextVertex.Position;
                        _lastVertex = _nextVertex;

                        if (!_currentPath.TryGetNextVertex(out _nextVertex))
                        {
                            AfterSeatReached();
                        }

                        routeLength -= distanceToNextVertex;
                    }
                }

                transform.position = position;

                void AfterSeatReached()
                {
                    transform.position = _seat.Position;
                    _lastVertex = _seat;
                    _nextVertex = null;
                    _currentPath = null;

                    
                }

                break;

        }

        /*switch (lectureState)
        {
            case LectureState.Lecture:
                ExecuteLectureStateBehaviour();
                break;
            case LectureState.Pause:
                ExecutePauseStateBehaviour();
                break;
            case LectureState.None:
                ExecuteNothingStateBehaviour();
                break;
            default:
                throw new NotImplementedException();
        }*/

        UpdateUserStateCaption(temperature);
    }

    private void DestroyGameObject()
    {
        GameObject.Destroy(this);

        if (Role == UserRole.Student)
        {
            UserGroupController.UnoccupySeat(_seat);
        }

        Destroyed?.Invoke(this);
    }

    /*
    private void ExecuteLectureStateBehaviour()
    {
        if (State == UserState.Unknown ||
            State == UserState.Idle || 
            State == UserState.Moving)
        {
            GoToSeat();
        }
        else if (State == UserState.GoToSeat)
        {
            if (!FollowPath())
            {
                if (Role == UserRole.Lecturer)
                    State = UserState.Lecturing;
                else if (Role == UserRole.Student)
                    State = UserState.Listening;
            }
        }
        else if (State == UserState.Lecturing)
        {
            if (_currentPath == null)
                SetTarget(GetRandomVertex());

            FollowPath();
        }
    }

    private void ExecutePauseStateBehaviour()
    {
        if (State == UserState.Unknown ||
            State == UserState.Lecturing)
        {
            GoToSeat();
        }
        else if (State == UserState.GoToSeat)
        {
            if (!FollowPath())
            {
                if (Role == UserRole.Lecturer)
                    State = UserState.Idle;
                else if (Role == UserRole.Student)
                    State = UserState.Moving;
            }
        }
        else if (State == UserState.Listening)
        {
            State = UserState.Moving;
        }
        else if (State == UserState.Moving)
        {
            if (_currentPath == null)
                SetTarget(GetRandomVertex());

            FollowPath();
        }
    }

    private void ExecuteNothingStateBehaviour()
    {
        LeaveRoom();

        if (!FollowPath())
        {
            GameObject.Destroy(this);

            if (Role == UserRole.Student)
            {
                UserGroupController.UnoccupySeat(_seat);
            }

            Destroyed?.Invoke(this);
        }
    }*/

    public void LeaveRoom()
    {
        if (State != UserState.LeavingRoom)
        {
            UserGroupController.CancelGoToTabletRequest();
            SetTarget(GetRandomDoorVertex());
            State = UserState.LeavingRoom;
        }
    }

    public void GoToSeat()
    {
        if (State != UserState.GoToSeat)
        {
            SetTarget(_seat);
            State = UserState.GoToSeat;
        }
    }

    private bool SetTarget(Vertex vertex)
    {
        if (Graph.GetPathTo(_nextVertex ?? _lastVertex, vertex, out Path path))
        {
            _currentPath = path;
            return true;
        }
        else
        {
            _currentPath = null;
            return false;
        }
    }

    private void UpdateTemperatureFeeling(Temperature? temperature)
    {
        if (temperature.HasValue)
        {
            if (temperature > MaxOkTemperature)
            {
                IsSweating = true;
                IsFreezing = false;
            }
            else if (temperature < MinOkTemperature)
            {
                IsSweating = false;
                IsFreezing = true;
            }
            else
            {
                IsSweating = false;
                IsFreezing = false;
            }
        }
        else
        {
            IsFreezing = false;
            IsSweating = false;
        }
    }

    private void UpdateUserStateCaption(Temperature? temperature)
    {
        string sweatingText = IsSweating ? "[Sweating]" : string.Empty;
        string freezingText = IsFreezing ? "[Freezing]" : string.Empty;

        _userStateTextMesh.text = $"{State} {sweatingText}{freezingText} ({temperature?.ToString() ?? "No Data"})";
    }

    private Vertex GetRandomDoorVertex()
    {
        IReadOnlyList<Vertex> doors = RoomThermalManager.Room.RoomGraph.Doors;

        return doors[Random.Range(0, doors.Count)];
    }

    private Vertex GetRandomVertex()
    {
        IReadOnlyList<Vertex> vertices = RoomThermalManager.Room.RoomGraph.Vertices;

        return vertices[Random.Range(0, vertices.Count)];
    }
}
