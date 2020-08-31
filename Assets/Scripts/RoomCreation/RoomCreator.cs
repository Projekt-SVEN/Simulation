using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.ObjectController;
using Assets.Scripts.Remote;
using Assets.Scripts.Remote.Abstractions;
using Assets.Scripts.RoomCreation;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class RoomCreator : MonoBehaviour, IRoom
{
    [SerializeField]
    private int _roomWidth;

    [SerializeField] 
    private int _roomHeight;

    [SerializeField]
    private float _wallThickness = 0.02f;

    [SerializeField]
    private GameObject _airPrefab;

    [SerializeField]
    private GameObject _wallPrefab;

    [SerializeField]
    private GameObject _thermometerPrefab;

    [SerializeField]
    private GameObject _userGroupControllerPrefab;

    [SerializeField]
    private GameObject _windowPrefab;

    [SerializeField]
    private GameObject _doorPrefab;

    [SerializeField]
    private GameObject _furniture;

    [SerializeField]
    private GameObject _tablePrefab;

    [SerializeField]
    private GameObject _chairPrefab;

    [SerializeField]
    private GameObject _heaterPrefab;

    [SerializeField]
    private GameObject _closetPrefab;

    private (GameObject gameObject, bool isWall)[,] _roomObjects;

    private RoomGraph _roomGraph;

    private IRoomThermalManager _roomThermalManager;

    private LsfInfoSchnittstelle _lsfInfoSchnittstelle;

    private static readonly TimeSpan LsfReadInterval = TimeSpan.FromMinutes(1);
    private DateTime _lastLsfRead = DateTime.MinValue;

    #region Properties

    public GameObject AirPrefab => _airPrefab;

    public GameObject WallPrefab => _wallPrefab;

    /// <summary>
    /// Gets the dimensional extent of the <see cref="IRoom"/> in meter (without the wall).
    /// </summary>
    /// <remarks>
    /// This value is not allowed to change.
    /// It's calculated by this formula: (WallThickness + ThermalPixelSize) / 2 + i * AirPrefab.transform.lossyScale.x
    /// Unfortunately, the variable i is unknown at this point. But 'i' represents: _roomObjects.GetLength(0) - 1,
    /// _roomObjects.GetLength(0) is calculated via: roomThermalPixelCount.x + 2 * wallThermalPixelCount
    /// 
    /// 
    /// roomThermalPixelCount:
    /// we get the roomThermalPixelCount with: _roomWidth / _roomThermalManager.ThermalPixelSize (_roomHeight respectively)
    /// We can't use the ThermalPixelSize from _roomThermalManager, but we can use the OptionsManager one (thermalManagerBuilder.ThermalPixelSize = OptionsManager.ThermalPixelSize)
    /// 
    /// wallThermalPixelCount:
    /// is calculated with the formula: _wallThickness / _roomThermalManager.ThermalPixelSize
    /// 
    /// Putting it all together, we get the formula below
    /// </remarks>
    public Vector3 RoomSize => new Vector3(
        x: (WallThickness + OptionsManager.ThermalPixelSize) / 2 + (((_roomWidth / OptionsManager.ThermalPixelSize) + 2 * (_wallThickness / OptionsManager.ThermalPixelSize)) - 1) * OptionsManager.ThermalPixelSize, 
        y: (WallThickness + OptionsManager.ThermalPixelSize) / 2 + (((_roomHeight / OptionsManager.ThermalPixelSize) + 2 * (_wallThickness / OptionsManager.ThermalPixelSize)) - 1) * OptionsManager.ThermalPixelSize);

    /// <summary>
    /// Gets the global position of the <see cref="IRoom"/>.
    /// </summary>
    /// <remarks>
    /// This value is not allowed to change.
    /// </remarks>
    public Vector3 RoomPosition => new Vector3(WallThickness / 2, WallThickness / 2, 0);

    /// <summary>
    /// Gets the thickness of the walls of the <see cref="IRoom"/>.
    /// </summary>
    /// <remarks>
    /// This value is not allowed to change.
    /// </remarks>
    public float WallThickness => _wallThickness;

    public float ThermalPixelSize => _roomThermalManager?.ThermalPixelSize ?? OptionsManager.ThermalPixelSize;

    public RoomGraph RoomGraph => _roomGraph;

    public LectureState LectureState { get; private set; } = LectureState.None;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        RoomReader roomreader = new RoomReader(OptionsManager.RoomFile);
        RoomObject[] roomObjects = roomreader.ReadRoom();

        #region PreperationsGraph

        _roomGraph = new RoomGraph();

        #endregion

        #region Load Options

        _roomHeight = Mathf.RoundToInt(OptionsManager.RoomHeight);
        _roomWidth = Mathf.RoundToInt(OptionsManager.RoomWidth);
        _wallThickness = OptionsManager.WallThickness;

        #endregion

        IServerConnection serverConnection = ServerConnectionFactory.CreateServerConnection(OptionsManager.Username, OptionsManager.Password, OptionsManager.ServerAddress, OptionsManager.RequiresAuthentication);

        _lsfInfoSchnittstelle = new LsfInfoSchnittstelle(serverConnection, "LSF_Info_Sim");

        #region ThermalManager

        RoomThermalManagerBuilder thermalManagerBuilder = new RoomThermalManagerBuilder();
        thermalManagerBuilder.Room = this;
        thermalManagerBuilder.ThermalPixelSize = new Temperature(OptionsManager.InitialRoomTemperature, TemperatureUnit.Celsius);
        thermalManagerBuilder.OutsideTemperature = OutsideTemperatureSource.Instance;
        thermalManagerBuilder.InitialRoomTemperature = new NoisyTemperatureSource(
                baseTemperatureValue: Temperature.FromCelsius(OptionsManager.InitialRoomTemperature), 
                noiseOffset: Temperature.FromKelvin(0.5f));
        thermalManagerBuilder.ThermalPixelSize = OptionsManager.ThermalPixelSize;
        
        //Calculate Room Size
        int wallThermalPixelCount = Mathf.RoundToInt(_wallThickness / thermalManagerBuilder.ThermalPixelSize);

        Vector2Int roomThermalPixelCount = new Vector2Int(
            x: Mathf.RoundToInt(_roomWidth / thermalManagerBuilder.ThermalPixelSize),
            y: Mathf.RoundToInt(_roomHeight / thermalManagerBuilder.ThermalPixelSize));

        #endregion

        #region Room Creator

        WallPrefab.transform.localScale = new Vector3(
            x: thermalManagerBuilder.ThermalPixelSize,
            y: thermalManagerBuilder.ThermalPixelSize,
            z: thermalManagerBuilder.ThermalPixelSize);

        AirPrefab.transform.localScale = new Vector3(
            x: thermalManagerBuilder.ThermalPixelSize,
            y: thermalManagerBuilder.ThermalPixelSize,
            z: thermalManagerBuilder.ThermalPixelSize);

        _roomObjects = new (GameObject, bool)[
                    roomThermalPixelCount.x + 2 * wallThermalPixelCount,
                    roomThermalPixelCount.y + 2 * wallThermalPixelCount];

        float aktWallVertexDistance = OptionsManager.WallVertexDistance;

        for (int i = 0; i < _roomObjects.GetLength(0); i++)
        {
            for (int j = 0; j < _roomObjects.GetLength(1); j++)
            {
                if (i < wallThermalPixelCount || j < wallThermalPixelCount || 
                    j > _roomObjects.GetLength(1) - wallThermalPixelCount - 1 || i > _roomObjects.GetLength(0) - wallThermalPixelCount - 1)
                {
                    _roomObjects[i, j] = (InstantiateWallObject(i, j), true);
                }
                else
                {
                    GameObject airObject = Instantiate(
                            AirPrefab, //the GameObject that will be instantiated
                            position: new Vector3(
                                x: (WallThickness + ThermalPixelSize) / 2 + i * AirPrefab.transform.lossyScale.x, //WallThickness - (WallThickness - 1) * 0.5f = w/2 + 0.5
                                y: (WallThickness + ThermalPixelSize) / 2 + j * AirPrefab.transform.lossyScale.y),
                            rotation: AirPrefab.transform.rotation);


                airObject.name = "Air_" + i + "." + j;
                airObject.transform.parent = gameObject.transform;
                airObject.GetComponent<TemperatureController>().Position = new Vector2Int(i, j);

                    _roomObjects[i, j] = (airObject, false);
                }
            }
        }
        #endregion

        #region InsertWallVertex

        for(float i = wallThermalPixelCount + OptionsManager.WallVertexDistance; i < _roomObjects.GetLength(0) - wallThermalPixelCount - OptionsManager.WallVertexDistance;
            i += OptionsManager.WallVertexDistance)
        {
            _roomGraph.AddVertex(new Vector2(
                x: i,
                y: wallThermalPixelCount + OptionsManager.VertexObjectOffSet));
            _roomGraph.AddVertex(new Vector2(
                x: i,
                y: _roomObjects.GetLength(1) - wallThermalPixelCount - OptionsManager.VertexObjectOffSet));
        }

        // *2 becaus double Corners are a Problem
        for (float i = wallThermalPixelCount + OptionsManager.WallVertexDistance * 2; i < _roomObjects.GetLength(1) - wallThermalPixelCount - OptionsManager.WallVertexDistance * 2;
           i += OptionsManager.WallVertexDistance)
        {
            _roomGraph.AddVertex(new Vector2(
                x: wallThermalPixelCount + OptionsManager.VertexObjectOffSet,
                y: i));
            _roomGraph.AddVertex(new Vector2(
                x: _roomObjects.GetLength(0) - wallThermalPixelCount - OptionsManager.VertexObjectOffSet,
                y: i));
        }

        #endregion

        #region RoomObjects Creator

        List<RoomObject> thermometers = new List<RoomObject>();

        foreach (RoomObject roomObject in roomObjects)
        {
            if (roomObject.Element == RoomObject.RoomElement.Thermometer)
                thermometers.Add(roomObject);
            else
                InstatiateRoomGameObject(roomObject, serverConnection, thermalManagerBuilder);
        }

        _roomGraph.MeshGraph();
        #endregion

        //Build and start Thermal Manager
        _roomThermalManager = thermalManagerBuilder.Build();


        GameObject userGroupControllerObject = Instantiate(_userGroupControllerPrefab);
        UserGroupController userGroupController = userGroupControllerObject.GetComponent<UserGroupController>();
        userGroupController.RoomThermalManager = _roomThermalManager;

        foreach (RoomObject thermometer in thermometers)
        {
            InstantiateThermometerGameObject(thermometer, serverConnection);
        }

        _roomThermalManager.Start();
    }

    private void InstatiateRoomGameObject(RoomObject roomObject, IServerConnection serverConnection,
        RoomThermalManagerBuilder thermalManagerBuilder)
    {
        switch (roomObject.Element)
        {
            case RoomObject.RoomElement.Chair:
                InstantiateChairGameObject(roomObject);
                break;
            case RoomObject.RoomElement.Table:
                InstantiateTableGameObject(roomObject);
                break;
            case RoomObject.RoomElement.Heater:
                InstantiateHeaterGameObject(roomObject, thermalManagerBuilder, serverConnection);
                break;
            case RoomObject.RoomElement.Closet:
                InstantiateClosetGameObject(roomObject);
                break;
            case RoomObject.RoomElement.Door:
                InstantiateDoorGameObject(roomObject);
                break;
            case RoomObject.RoomElement.Window:
                InstantiateWindowGameObject(roomObject, thermalManagerBuilder, serverConnection);
                break;
            case RoomObject.RoomElement.Tablet:
                InstantiateTabletGameObject(roomObject, serverConnection);
                break;
        }
    }

    private void InstantiateChairGameObject(RoomObject roomObject)
    {
        GameObject chair = Instantiate(
            _chairPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _chairPrefab.transform.rotation);

        Vector2 centerOfChair = CalculateCenterOfRoomObject(
            position: chair.transform.position,
            roomObject: roomObject);

        if (roomObject.Type.Equals("Dozent"))
        {
            _roomGraph.AddLecturerChair(centerOfChair);
        }
        else
        {
            _roomGraph.AddStudentsChair(centerOfChair);
        }
        Transform(chair, roomObject);
    }

    private static Vector2 CalculateCenterOfRoomObject(Vector2 position, RoomObject roomObject)
    {
        float cosinus = Mathf.Cos((float)roomObject.RotationRadians);
        float sinus = Mathf.Sin((float)roomObject.RotationRadians);

        return new Vector2
        (
            x: position.x + (cosinus * roomObject.SizeWidth) - (sinus * roomObject.SizeHeight),
            y: position.y + (sinus * roomObject.SizeWidth) + (cosinus * roomObject.SizeHeight)
        );
    }

    private void InstantiateTableGameObject(RoomObject roomObject)
    {
        GameObject table = Instantiate(
            _tablePrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        table.GetComponent<TableController>().setSprite(roomObject.Type);

        _roomGraph.AddSqaureObject(roomObject);

        Transform(table, roomObject);
    }

    private void InstantiateHeaterGameObject(RoomObject roomObject, RoomThermalManagerBuilder thermalManagerBuilder, IServerConnection serverConnection)
    {
        RemoteHeater remoteHeater = new RemoteHeater(serverConnection, roomObject.FhemGetName, roomObject.FhemSetName);

        GameObject heater = Instantiate(
            _heaterPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        HeaterController heaterController = heater.GetComponent<HeaterController>();
        heaterController.RemoteHeater = remoteHeater;
        heaterController.Size = new Vector3(
            x: roomObject.SizeWidth,
            y: roomObject.SizeHeight);

        thermalManagerBuilder.AddThermalObject(heaterController);

        _roomGraph.AddVertex(heater.transform.position);

        Transform(heater, roomObject);
    }

    private void InstantiateClosetGameObject(RoomObject roomObject)
    {
        GameObject closet = Instantiate(
            _closetPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        Transform(closet, roomObject);
    }

    private void InstantiateDoorGameObject(RoomObject roomObject)
    {
        GameObject door = Instantiate(
            _doorPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        _roomGraph.AddDoor(door.transform.position);

        Transform(door, roomObject);
    }

    private void InstantiateWindowGameObject(RoomObject roomObject, RoomThermalManagerBuilder thermalManagerBuilder,
        IServerConnection serverConnection)
    {
        RemoteWindow remoteWindow = new RemoteWindow(serverConnection, roomObject.FhemGetName, roomObject.FhemSetName);

        GameObject window = Instantiate(
            _windowPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        WindowController windowController = window.GetComponent<WindowController>();
        windowController.Initialize(remoteWindow, 
            thermalSize: new Vector3(
            x: roomObject.SizeWidth,
            y: roomObject.SizeHeight));

        thermalManagerBuilder.AddThermalObject(windowController);

        Vector2 windowVertexPosition = window.transform.position;
        
        _roomGraph.AddVertex(windowVertexPosition);
        
        Transform(window, roomObject);
    }

    private void InstantiateTabletGameObject(RoomObject roomObject, IServerConnection serverConnection)
    {
        RemoteTargetTemperature remoteTargetTemperature = new RemoteTargetTemperature(serverConnection, roomObject.FhemGetName, roomObject.FhemSetName);
        RemoteWindow remoteWindow = new RemoteWindow(serverConnection, "Sven_FensterManager_Sim", "Fensterkontakt_Sim");

        RemoteTablet remoteTablet = new RemoteTablet(remoteTargetTemperature, remoteWindow);

        _roomGraph.AddTablet(
            position: new Vector2(roomObject.PositionX, roomObject.PositionY),
            remoteTablet: remoteTablet);
    }

    private void InstantiateThermometerGameObject(RoomObject roomObject, IServerConnection serverConnection)
    {
        RemoteThermometer remoteThermometer = new RemoteThermometer(serverConnection, roomObject.FhemGetName, roomObject.FhemSetName);

        GameObject thermometerObject = Instantiate(
            _thermometerPrefab,
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _thermometerPrefab.transform.rotation);

        thermometerObject.transform.parent = gameObject.transform;
        ThermometerController thermometerController = thermometerObject.GetComponent<ThermometerController>();
        thermometerController.Initialize(_roomThermalManager, remoteThermometer);
        
    }

    private void Transform(GameObject gameObject, RoomObject roomObject)
    {
        gameObject.transform.Rotate(0, 0, roomObject.Rotation);
        gameObject.transform.parent = _furniture.transform;
        
        Vector3 objectSize = gameObject.transform.localScale;
        objectSize.x *= roomObject.SizeWidth;
        objectSize.y *= roomObject.SizeHeight;
        
        gameObject.transform.localScale = objectSize;
    }

    private GameObject InstantiateWallObject(int i, int j)
    {
        GameObject wallObject = Instantiate(
                                WallPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + ThermalPixelSize) / 2 + i * _wallPrefab.transform.lossyScale.x,
                                    y: (WallThickness + ThermalPixelSize) / 2 + j * _wallPrefab.transform.lossyScale.x),
                                rotation: WallPrefab.transform.rotation);

        wallObject.transform.parent = gameObject.transform;

        wallObject.name = "Wall_" + i + ":" + j;
        SetWallSprite(i, j, wallObject);

        return wallObject;
    }

    private void SetWallSprite(int i, int j, GameObject wallObject)
    {
        bool[] neighbours = new bool[4];

        if (i == 0)
        {
            neighbours[1] = _roomObjects[i + 1, j].isWall;
            neighbours[3] = false;
        }
        else if (i == _roomObjects.GetLength(0) - 1)
        {
            neighbours[1] = false;
            neighbours[3] = _roomObjects[i - 1, j].isWall;
        }
        else
        {
            neighbours[1] = _roomObjects[i + 1, j].isWall;
            neighbours[3] = _roomObjects[i - 1, j].isWall;
        }

        if (j == 0)
        {
            neighbours[0] = _roomObjects[i, j + 1].isWall;
            neighbours[2] = false;
        }
        else if (j == _roomObjects.GetLength(1) - 1)
        {
            neighbours[0] = false;
            neighbours[2] = _roomObjects[i, j - 1].isWall;
        }
        else
        {
            neighbours[0] = _roomObjects[i, j + 1].isWall;
            neighbours[2] = _roomObjects[i, j - 1].isWall;
        }

        wallObject.GetComponent<WallController>().SetWalls(neighbours);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLectureState();

        _roomThermalManager.Update();
        
        SetAirTemperatureColors();
        SetWallTemperatureColors();
    }

    private void UpdateLectureState()
    {
        DateTime now = DateTime.Now;
        TimeSpan durationSinceLastRead = now - _lastLsfRead;

        if (durationSinceLastRead > LsfReadInterval)
        {
            _lastLsfRead = now;

            try
            {
                _lsfInfoSchnittstelle.GetStates(out bool lecture, out bool isBreak, out DateTime? nextBreak);

                if (lecture)
                {
                    LectureState = LectureState.Lecture;
                }
                else if (isBreak)
                {
                    LectureState = LectureState.Pause;
                }
                else
                {
                    LectureState = LectureState.None;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Failed to read LSF lecture state.");
                Debug.LogException(exception);
            }
        }
    }

    private void SetWallTemperatureColors()
    {

    }

    /// <summary>
    /// Sets the color of all air game-objects relative to the temperature of a air game-object
    /// to the lowest and highest temperature of all air game-objects.
    /// </summary>
    private void SetAirTemperatureColors()
    {
        float lowestTemperature;
        float highestTemperature;
        float temperatureStep;

        SetTemperatures();
        lowestTemperature = _roomThermalManager.CurrentTemperatureStatistics.Minimum;
        highestTemperature = _roomThermalManager.CurrentTemperatureStatistics.Maximum;

        if (OptionsManager.DynamicTemperatureScaling)
        { 
            OptionsManager.MinTemperatur = lowestTemperature;
            OptionsManager.MaxTemperatur = highestTemperature;
        }
        else
        {
            lowestTemperature = OptionsManager.MinTemperatur;
            highestTemperature = OptionsManager.MaxTemperatur;
        }

        //Length -1, cuz otherwise we'd get values between 0 and 16
        float temperatureDifference = highestTemperature - lowestTemperature;
        
        if (temperatureDifference != 0)
        {
            temperatureStep = temperatureDifference / (AirColors.ColorArray.GetLength(0) - 1);

            for (int x = 0; x < _roomObjects.GetLength(0); x++)
            {
                for (int y = 0; y < _roomObjects.GetLength(1); y++)
                {
                    float localColorDiff = _roomObjects[x, y].gameObject.GetComponent<TemperatureController>().Temperature - lowestTemperature;

                    _roomObjects[x, y].gameObject.GetComponent<TemperatureController>().SetColor(InterpolateColor(localColorDiff, temperatureStep));
                }
            }
        }
    }

    private Color32 InterpolateColor(float localColorDiff, float temperatureStep)
    {
        // Just for static scaling possible
        if (localColorDiff < 0f)
        {
            return AirColors.ColorArray[0];
        }

        float colorSection = localColorDiff / temperatureStep;
        int currentColorIndex = (int)Math.Round(colorSection, 0);
        float minimumTemperatureValueForNextColorindex = (currentColorIndex + 1) * temperatureStep;

        if (-1 < currentColorIndex && currentColorIndex < AirColors.ColorArray.GetLength(0) - 1)
        {
            byte r = (byte)Mathf.Lerp(AirColors.ColorArray[currentColorIndex].r, AirColors.ColorArray[currentColorIndex + 1].r, minimumTemperatureValueForNextColorindex - localColorDiff);
            byte g = (byte)Mathf.Lerp(AirColors.ColorArray[currentColorIndex].g, AirColors.ColorArray[currentColorIndex + 1].g, minimumTemperatureValueForNextColorindex - localColorDiff);
            byte b = (byte)Mathf.Lerp(AirColors.ColorArray[currentColorIndex].b, AirColors.ColorArray[currentColorIndex + 1].b, minimumTemperatureValueForNextColorindex - localColorDiff);
            return new Color32(r, g, b, 255);
        }
        else
        {
            return AirColors.ColorArray[AirColors.ColorArray.GetLength(0) - 1];
        }
    }

    private void SetTemperatures()
    {

        for (int x = 0; x < _roomObjects.GetLength(0); x++)
        {
            for (int y = 0; y < _roomObjects.GetLength(1); y++)
            {
                float temperature = _roomThermalManager.GetTemperature(_roomObjects[x,y].gameObject.transform.position).ToCelsius().Value;

                _roomObjects[x, y].gameObject.GetComponent<TemperatureController>().Temperature = temperature;
            }
        }
    }
}
