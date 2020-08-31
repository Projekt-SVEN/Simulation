using System;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts
{
    public struct OnSettingChangedEventArgs
    {
        public string SettingName;
        public object OldValue;
    }

    public class OptionsManager : MonoBehaviour
    {
        public event EventHandler<OnSettingChangedEventArgs> OnSettingChanged; 

        [SerializeField]
        private float _roomWidth = 30f;

        [SerializeField]
        private float _roomHeight = 30f;

        [SerializeField]
        private float _wallThickness = 1f;

        [SerializeField]
        private float _outsideTemperature = 10f;

        [SerializeField]
        private float _initialRoomTemperature = 20f;

        [SerializeField]
        private float _thermalPixelSize = 0.1f;

        [SerializeField]
        private float _thermalTickDuration = 0.25f;

        [SerializeField]
        private string _username = string.Empty;

        [SerializeField]
        private string _password = string.Empty;

        [SerializeField]
        private string _serverAddress = string.Empty;

        [SerializeField]
        private bool _requiresAuthentication = false;

        [SerializeField]
        private int _userCount = 1;

        [SerializeField]
        private float _movementSpeed = 5;

        [SerializeField]
        private float _cameraZoomSpeed = 4f;

        [SerializeField]
        private bool _lecture = false;

        [SerializeField]
        private float _minTemperature = 10.0f;

        [SerializeField]
        private float _maxTemperature = 30.0f;

        [SerializeField]
        private bool _dynamicSkalar = false;

        [SerializeField]
        private int _windowMode = 0;

        [SerializeField]
        private int _windowHeight = 0;

        [SerializeField]
        private int _windowWidth = 0;

        [SerializeField]
        private float _maxUserSpeed = 0f;

        [SerializeField]
        private float _minUserSpeed = 0f;

        [SerializeField]
        private float _upperMaxOkUserTemperatur = 0f;

        [SerializeField]
        private float _upperMinOkUserTemperatur = 0f;

        [SerializeField]
        private float _lowerMaxOkUserTemperatur = 0f;

        [SerializeField]
        private float _lowerMinOkUserTemperatur = 0f;

        [SerializeField]
        private float _probailityOfUserStandingUpInPause = 0f;

        [SerializeField]
        private String _roomFile;

        [SerializeField]
        private float _vertexObjectOffSet;

        [SerializeField]
        private float _wallVertexDistance;

        private static OptionsManager _instance;
        private static OptionsManager Instance { get; set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        /// <summary>
        /// Gets or sets the width of the room that should be created when the simulation starts.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float RoomWidth
        {
            get => Instance._roomWidth;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(RoomWidth), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._roomWidth)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_WIDTH,
                        OldValue = Instance._roomWidth
                    };

                    Instance._roomWidth = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of the room that should be created when the simulation starts.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float RoomHeight
        {
            get => Instance._roomHeight;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(RoomHeight), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._roomHeight)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_HEIGHT,
                        OldValue = Instance._roomHeight
                    };

                    Instance._roomHeight = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the thickness of the walls of the room that should be created when the simulation starts.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float WallThickness
        {
            get => Instance._wallThickness;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(WallThickness), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._wallThickness)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.WALL_THICKNESS,
                        OldValue = Instance._wallThickness
                    };

                    Instance._wallThickness = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the temperature outside the room in °C.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value below <see cref="Temperature.AbsoluteZero"/>.
        /// </exception>
        public static float OutsideTemperature
        {
            get => Instance._outsideTemperature;
            set
            {
                if (!IsValidTemperature(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(OutsideTemperature), value, "An attempt was made to set the value below absolute zero.");
                }

                if (value != Instance._outsideTemperature)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.OUTSIDE_TEMPERATURE,
                        OldValue = Instance._outsideTemperature
                    };

                    Instance._outsideTemperature = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the temperature with which the thermal pixels of the room should be initialized.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value below <see cref="Temperature.AbsoluteZero"/>.
        /// </exception>
        public static float InitialRoomTemperature
        {
            get => Instance._initialRoomTemperature;
            set
            {
                if (!IsValidTemperature(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(InitialRoomTemperature), value, "An attempt was made to set the value below absolute zero.");
                    
                }

                if (value != Instance._initialRoomTemperature)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.INITIAL_ROOM_TEMPERATURE,
                        OldValue = Instance._initialRoomTemperature
                    };

                    Instance._initialRoomTemperature = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the thermal pixels.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value below <see cref="Temperature.AbsoluteZero"/>.
        /// </exception>
        public static float ThermalPixelSize
        {
            get => Instance._thermalPixelSize;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "An attempt was made to set the value to zero or an negative value.");
                    
                }

                if (value != Instance._thermalPixelSize)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.THERMAL_PIXEL_SIZE,
                        OldValue = Instance._thermalPixelSize
                    };

                    Instance._thermalPixelSize = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the thermal pixels.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value below <see cref="Temperature.AbsoluteZero"/>.
        /// </exception>
        public static float ThermalTickDuration
        {
            get => Instance._thermalTickDuration;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "An attempt was made to set the value to zero or an negative value.");

                }

                if (value != Instance._thermalTickDuration)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.THERMAL_TICK_DURATION,
                        OldValue = Instance._thermalTickDuration
                    };

                    Instance._thermalTickDuration = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the username to login with to the FHEM server.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// When trying to set the value to <see langword="null"/>.
        /// </exception>
        public static string Username
        {
            get => Instance._username;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(Username), "An attempt was made to set the value to null.");
                }

                if (!value.Equals(Instance._username))
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.USERNAME,
                        OldValue = Instance._username
                    };

                    Instance._username = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the password to login with to the FHEM server.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// When trying to set the value to <see langword="null"/>.
        /// </exception>
        public static string Password
        {
            get => Instance._password;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(Password), "An attempt was made to set the value to null.");
                }

                if (!value.Equals(Instance._password))
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.PASSWORD,
                        OldValue = Instance._password
                    };

                    Instance._password = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the address of the FHEM server.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// When trying to set the value to <see langword="null"/>.
        /// </exception>
        public static string ServerAddress
        {
            get => Instance._serverAddress;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(ServerAddress), "An attempt was made to set the value to null.");
                }

                if (!value.Equals(Instance._serverAddress))
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.SERVER_ADDRESS,
                        OldValue = Instance._serverAddress
                    };

                    Instance._serverAddress = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets if the FHEM server requires the client to authenticate itself.
        /// </summary>
        public static bool RequiresAuthentication
        {
            get => Instance._requiresAuthentication;
            set
            {
                if (value != Instance._requiresAuthentication)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.REQUIRES_AUTHENTICATION,
                        OldValue = Instance._requiresAuthentication
                    };

                    Instance._requiresAuthentication = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }   
        }

        /// <summary>
        /// Gets or sets the amount of users that should be created when the simulation is started.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static int UserCount
        {
            get => Instance._userCount;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(UserCount), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._userCount)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.USER_COUNT,
                        OldValue = Instance._userCount
                    };

                    Instance._userCount = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the movement speed of the camera.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float MovementSpeed
        {
            get => Instance._movementSpeed;
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(MovementSpeed), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._movementSpeed)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.MOVEMENT_SPEED,
                        OldValue = Instance._movementSpeed
                    };

                    Instance._movementSpeed = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the zoom speed of the camera.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float CameraZoomSpeed
        {
            get => Instance._cameraZoomSpeed;
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(CameraZoomSpeed), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._cameraZoomSpeed)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.CAMERA_SPEED,
                        OldValue = Instance._cameraZoomSpeed
                    };

                    Instance._cameraZoomSpeed = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum Temperatur in the room.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float MinTemperatur
        {
            get => Instance._minTemperature;
            set
            {
                Instance._minTemperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximal Temperatur for the room.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float MaxTemperatur
        {
            get => Instance._maxTemperature;
            set
            {
                Instance._maxTemperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximal Temperatur for the room.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static int WindowMode
        {
            get => _instance._windowMode;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(WindowMode), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._windowMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximal Temperatur for the room.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static int WindowHeigth
        {
            get => _instance._windowHeight;
            set
            {
                if (value < 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(WindowHeigth), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._windowHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximal Temperatur for the room.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static int WindowWidth
        {
            get => _instance._windowWidth;
            set
            {
                if (value < 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(WindowWidth), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._windowWidth = value;
            }
        }


        /// <summary>
        /// Gets or sets the maximum user speed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float MaxUserSpeed
        {
            get => Instance._maxUserSpeed;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxUserSpeed), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._maxUserSpeed)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_WIDTH,
                        OldValue = Instance._maxUserSpeed
                    };

                    Instance._maxUserSpeed = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }


        /// <summary>
        /// Gets or sets the minimum user speed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float MinUserSpeed
        {
            get => Instance._minUserSpeed;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(MinUserSpeed), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._minUserSpeed)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_WIDTH,
                        OldValue = Instance._minUserSpeed
                    };

                    Instance._minUserSpeed = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }


        /// <summary>
        /// Gets or sets the upper max OK user temperature
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float UpperMaxOkUserTemperature
        {
            get => Instance._upperMaxOkUserTemperatur;
            set
            {
                if (!IsValidTemperature(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(OutsideTemperature), value, "An attempt was made to set the value below absolute zero.");
                }

                if (value != Instance._upperMaxOkUserTemperatur)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_WIDTH,
                        OldValue = Instance._upperMaxOkUserTemperatur
                    };

                    Instance._upperMaxOkUserTemperatur = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }


        /// <summary>
        /// Gets or sets the lower max OK user temperature
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float LowerMaxOkUserTemperature
        {
            get => Instance._lowerMaxOkUserTemperatur;
            set
            {
                if (!IsValidTemperature(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(OutsideTemperature), value, "An attempt was made to set the value below absolute zero.");
                }

                if (value != Instance._lowerMaxOkUserTemperatur)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_WIDTH,
                        OldValue = Instance._lowerMaxOkUserTemperatur
                    };

                    Instance._lowerMaxOkUserTemperatur = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }


        /// <summary>
        /// Gets or sets the upper min OK user temperature
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float UpperMinOkUserTemperature
        {
            get => Instance._upperMinOkUserTemperatur;
            set
            {
                if (!IsValidTemperature(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(OutsideTemperature), value, "An attempt was made to set the value below absolute zero.");
                }

                if (value != Instance._upperMinOkUserTemperatur)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_WIDTH,
                        OldValue = Instance._upperMinOkUserTemperatur
                    };

                    Instance._upperMinOkUserTemperatur = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }


        /// <summary>
        /// Gets or sets the lower min OK user temperature
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float LowerMinOkUserTemperature
        {
            get => Instance._lowerMinOkUserTemperatur;
            set
            {
                if (!IsValidTemperature(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(OutsideTemperature), value, "An attempt was made to set the value below absolute zero.");
                }

                if (value != Instance._lowerMinOkUserTemperatur)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_WIDTH,
                        OldValue = Instance._lowerMinOkUserTemperatur
                    };

                    Instance._lowerMinOkUserTemperatur = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Probability of a User to standing up in a pause.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float ProbabilityOfUserStandingUpInPause
        {
            get => Instance._probailityOfUserStandingUpInPause;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(ProbabilityOfUserStandingUpInPause), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._probailityOfUserStandingUpInPause)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.ROOM_WIDTH,
                        OldValue = Instance._probailityOfUserStandingUpInPause
                    };

                    Instance._probailityOfUserStandingUpInPause = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the room file for the generation of the simulatat room.
        /// </summary>
        public static string RoomFile
        {
            get => Instance._roomFile;
            set
            {
                Instance._roomFile = value;
            }
        }

        public static bool Lecture
        {
            get => Instance._lecture;
            set
            {
                if (value != Instance._lecture)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.LECTURE,
                        OldValue = Instance._lecture
                    };

                    Instance._lecture = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Offset vor the vertex of Roomobjects
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float VertexObjectOffSet
        {
            get => Instance._vertexObjectOffSet;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(ProbabilityOfUserStandingUpInPause), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._probailityOfUserStandingUpInPause)
                {
                    Instance._vertexObjectOffSet = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the distance between tow vertex on the wall
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float WallVertexDistance
        {
            get => Instance._wallVertexDistance;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(ProbabilityOfUserStandingUpInPause), value, "An attempt was made to set the value to zero or an negative value.");
                }

                if (value != Instance._probailityOfUserStandingUpInPause)
                {
                    Instance._wallVertexDistance = value;
                }
            }
        }

        public static bool DynamicTemperatureScaling
        {
            get => Instance._dynamicSkalar;
            set
            {
                if (value != Instance._dynamicSkalar)
                {
                    OnSettingChangedEventArgs eventArgs = new OnSettingChangedEventArgs()
                    {
                        SettingName = OptionsNames.LECTURE,
                        OldValue = Instance._dynamicSkalar
                    };

                    Instance._dynamicSkalar = value;
                    Instance.OnSettingChanged?.Invoke(Instance, eventArgs);
                }
            }
        }

        public static float ParseFloat(string input)
        {
            return float.TryParse(input, out float value) ? value : float.NaN;
        }

        public static int? ParseInt(string input)
        {
            return int.TryParse(input, out int value) ? new int?(value) : null;
        }

        public static bool IsValidSize(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > 0f;
        }

        public static bool IsValidTemperature(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > Temperature.AbsoluteZero;
        }
    }
}
