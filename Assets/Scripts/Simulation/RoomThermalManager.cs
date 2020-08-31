using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Assets.Scripts.Simulation.Abstractions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Simulation
{
    partial class RoomThermalManagerBuilder
    {
        //TODO: Documentation
        private class RoomThermalManager : IRoomThermalManager
        {
            private struct SurfaceDescriptor
            {
                public readonly Vector2Int LeftBottomPixel;
                public readonly Vector2Int RightUpperPixel;

                public SurfaceDescriptor(Vector2Int leftBottomPixel, Vector2Int rightUpperPixel)
                {
                    LeftBottomPixel = leftBottomPixel;
                    RightUpperPixel = rightUpperPixel;
                }
            }

            private struct ThermalPixel
            {
                public readonly ThermalMaterial ThermalMaterial;
                public readonly Temperature Temperature;
                public readonly IThermalObject Reference;

                public List<IThermalObject> MovableThermalObjects;

                public ThermalPixel(
                    ThermalMaterial thermalMaterial, 
                    Temperature temperature,
                    IThermalObject reference = null)
                {
                    ThermalMaterial = thermalMaterial;
                    Temperature = temperature;
                    Reference = reference;
                    MovableThermalObjects = null;
                }

                public ThermalPixel(
                    ThermalPixel thermalPixel,
                    Temperature temperature)
                {
                    ThermalMaterial = thermalPixel.ThermalMaterial;
                    Temperature = temperature;
                    Reference = thermalPixel.Reference;
                    MovableThermalObjects = null;
                }
            }

            /// <summary>
            /// Occurs when <see cref="ThermalPixelSize"/> changed.
            /// </summary>
            public event EventHandler<float> OnThermalPixelSizeChanged;

            /// <summary>
            /// Occurs when <see cref="ThermalTickDuration"/> changed.
            /// </summary>
            public event EventHandler<float> OnThermalTickDurationChanged;

            private readonly object _thermalUpdateLock;

            private float _thermalPixelSize;
            private float _thermalPixelSizeSquared;
            private float _thermalPixelSizeCubed;

            private float _thermalTickDuration;

            private float _remainingTime = 0f;

            private ThermalPixel[,] _thermalPixels;

            private Temperature _outsideTemperature;
            private readonly ITemperatureSource _outsideTemperatureSource;

            private readonly List<IThermalObject> _movableThermalObjects;
            private readonly List<IThermalObject> _stationaryThermalObjects;

            private readonly Dictionary<IThermalObject, float> _surfaceAreaPerThermalPixelOfThermalObject;

            private readonly Dictionary<IThermalObject, (Vector2 Position, SurfaceDescriptor? Surface)> _movableThermalObjectsPositionCache;
            
            private Vector2Int _leftBottomRoomPixel;

            private readonly TemperatureStatisticsBuilder _totalTemperatureStatisticsBuilder;
            private TemperatureStatisticsBuilder _capturedTemperatureStatisticsBuilder;

            public RoomThermalManager(
                IRoom room, 
                float initialThermalPixelSize,
                float initialThermalTickDuration,
                ITemperatureSource outsideTemperatureSource,
                ITemperatureSource<Vector3> initialRoomTemperatureSource,
                IEnumerable<IThermalObject> initialThermalObjects = null)
            {
                #region Thermal Pixel Size Validation

                if (float.IsNaN(initialThermalPixelSize))
                    throw new ArgumentOutOfRangeException(nameof(initialThermalPixelSize), initialThermalPixelSize, "NaN is an invalid size value.");

                if (float.IsPositiveInfinity(initialThermalPixelSize))
                    throw new ArgumentOutOfRangeException(nameof(initialThermalPixelSize), initialThermalPixelSize, "Positive infinity is an invalid size value.");

                if (float.IsNegativeInfinity(initialThermalPixelSize))
                    throw new ArgumentOutOfRangeException(nameof(initialThermalPixelSize), initialThermalPixelSize, "Negative infinity is an invalid size value.");

                if (initialThermalPixelSize <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(initialThermalPixelSize), initialThermalPixelSize, "Zero or negative values are invalid size values.");
                
                #endregion

                #region Thermal Tick Duration Validation

                if (float.IsNaN(initialThermalTickDuration))
                    throw new ArgumentOutOfRangeException(nameof(initialThermalTickDuration), initialThermalTickDuration, "NaN is an invalid duration value.");

                if (float.IsPositiveInfinity(initialThermalTickDuration))
                    throw new ArgumentOutOfRangeException(nameof(initialThermalTickDuration), initialThermalTickDuration, "Positive infinity is an invalid duration value.");

                if (float.IsNegativeInfinity(initialThermalTickDuration))
                    throw new ArgumentOutOfRangeException(nameof(initialThermalTickDuration), initialThermalTickDuration, "Negative infinity is an invalid duration value.");

                if (initialThermalTickDuration <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(initialThermalTickDuration), initialThermalTickDuration, "Zero or negative values are invalid duration values.");

                #endregion

                _thermalUpdateLock = new object();

                Room = room ?? throw new ArgumentNullException(nameof(room));

                _thermalPixelSize = initialThermalPixelSize;
                _thermalPixelSizeSquared = _thermalPixelSize * _thermalPixelSize;
                _thermalPixelSizeCubed = _thermalPixelSizeSquared * _thermalPixelSize;

                _thermalTickDuration = initialThermalTickDuration;
                _outsideTemperatureSource = outsideTemperatureSource ?? throw new ArgumentNullException(nameof(outsideTemperatureSource));
                _outsideTemperature = outsideTemperatureSource.GetTemperature();

                _stationaryThermalObjects = new List<IThermalObject>();
                _movableThermalObjects = new List<IThermalObject>();
                _movableThermalObjectsPositionCache = new Dictionary<IThermalObject, (Vector2 Position, SurfaceDescriptor? Surface)>();
                
                if (initialThermalObjects != null)
                {
                    foreach (IThermalObject thermalObject in initialThermalObjects)
                    {
                        if (thermalObject.CanNotChangePosition)
                        {
                            _stationaryThermalObjects.Add(thermalObject);
                        }
                        else
                        {
                            _movableThermalObjects.Add(thermalObject);
                        }
                    }
                }

                _totalTemperatureStatisticsBuilder = new TemperatureStatisticsBuilder();
                _capturedTemperatureStatisticsBuilder = new TemperatureStatisticsBuilder();

                _surfaceAreaPerThermalPixelOfThermalObject = new Dictionary<IThermalObject, float>();

                CalculateThermalPixels(initialThermalPixelSize, initialRoomTemperatureSource);
            }

            /// <summary>
            /// Gets the <see cref="IRoom"/> that is thermally simulated.
            /// </summary>
            public IRoom Room { get; }

            /// <summary>
            /// Gets or sets the size of the thermal-pixels in meter.
            /// </summary>
            public float ThermalPixelSize
            {
                get => _thermalPixelSize;
                set
                {
                    if (float.IsNaN(value))
                        throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "NoN is an invalid size value.");

                    if (float.IsPositiveInfinity(value))
                        throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "Positive infinity is an invalid size value.");

                    if (float.IsNegativeInfinity(value))
                        throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "Negative infinity is an invalid size value.");

                    if (value <= 0f)
                        throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "Zero or negative values are invalid size values.");

                    //dynamic neighbourhood "https://en.wikipedia.org/wiki/Neighbourhood_(mathematics)"
                    //change of pixel size has to be grater than 1% of the value tried to set or 10cm.
                    if (Mathf.Abs(_thermalPixelSize - value) > Mathf.Min(value * 0.01f, 0.1f))
                        return;

                    lock (_thermalUpdateLock)
                    {
                        CalculateThermalPixels(value, this);
                        _thermalPixelSize = value;
                        _thermalPixelSizeSquared = value * value;
                        _thermalPixelSizeCubed = _thermalPixelSizeSquared * value;

                        OnThermalPixelSizeChanged?.Invoke(this, value);
                    }
                }
            }

            /// <summary>
            /// Gets or sets the duration of an thermal-update in seconds.
            /// </summary>
            public float ThermalTickDuration
            {
                get => _thermalTickDuration;
                set
                {
                    if (float.IsNaN(value))
                        throw new ArgumentOutOfRangeException(nameof(ThermalTickDuration), value, "NoN is an invalid duration value.");

                    if (float.IsPositiveInfinity(value))
                        throw new ArgumentOutOfRangeException(nameof(ThermalTickDuration), value, "Positive infinity is an invalid duration value.");

                    if (float.IsNegativeInfinity(value))
                        throw new ArgumentOutOfRangeException(nameof(ThermalTickDuration), value, "Negative infinity is an invalid duration value.");

                    if (value <= 0f)
                        throw new ArgumentOutOfRangeException(nameof(ThermalTickDuration), value, "Zero or negative values are invalid duration values.");

                    //dynamic neighbourhood "https://en.wikipedia.org/wiki/Neighbourhood_(mathematics)"
                    //change of pixel size has to be grater than 1% of the value tried to set or 1ms.
                    if (Mathf.Abs(_thermalTickDuration - value) > Mathf.Min(value * 0.01f, 0.001f))
                        return;

                    lock (_thermalUpdateLock)
                    {
                        _thermalTickDuration = value;

                        OnThermalTickDurationChanged?.Invoke(this, value);
                    }
                }
            }

            /// <summary>
            /// Gets the temperature statistics of all rendered frames.
            /// </summary>
            public TemperatureStatistics TotalTemperatureStatistics { get; private set; } = null;

            /// <summary>
            /// Gets the temperature statistics of the captured time frame.
            /// </summary>
            public TemperatureStatistics CapturedTemperatureStatistics { get; private set; } = null;

            /// <summary>
            /// Gets the temperature statistics of the current rendered frame.
            /// </summary>
            public TemperatureStatistics CurrentTemperatureStatistics { get; private set; } = null;

            /// <summary>
            /// Resets the <see cref="TemperatureStatistics"/> for the <see cref="CapturedTemperatureStatistics"/> property.
            /// </summary>
            public void ResetCapturedTemperatureStatistics()
            {
                lock (_thermalUpdateLock)
                {
                    _capturedTemperatureStatisticsBuilder = new TemperatureStatisticsBuilder();
                    CapturedTemperatureStatistics = null;
                }
            }

            /// <summary>
            /// Adds a <see cref="IThermalObject"/> to the thermal simulation of the <see cref="Room"/>.
            /// </summary>
            /// <param name="thermalObject">
            /// The <see cref="IThermalObject"/> that should be added.
            /// </param>
            public void AddThermalObject(IThermalObject thermalObject)
            {
                if (thermalObject == null) 
                    return;

                lock (_thermalUpdateLock)
                {
                    if (thermalObject.CanNotChangePosition)
                    {
                        if (_stationaryThermalObjects.Contains(thermalObject))
                            return;

                        _stationaryThermalObjects.Add(thermalObject);
                        CalculateThermalPixels(_thermalPixelSize, this);
                    }
                    else
                    {
                        if (_movableThermalObjects.Contains(thermalObject))
                            return;

                        _movableThermalObjects.Add(thermalObject);
                        _surfaceAreaPerThermalPixelOfThermalObject.Add(
                            thermalObject,
                            CalculateSurfaceAreaPerThermalPixelOfThermalObject(thermalObject));
                    }

                    thermalObject.ThermalStart(this);
                }
            }

            /// <summary>
            /// Removes a <see cref="IThermalObject"/> from the thermal simulation of the <see cref="Room"/>.
            /// </summary>
            /// <param name="thermalObject">
            /// The <see cref="IThermalObject"/> that should be removed.
            /// </param>
            public void RemoveThermalObject(IThermalObject thermalObject)
            {
                if (thermalObject == null)
                    return;

                lock (_thermalUpdateLock)
                {
                    if (thermalObject.CanNotChangePosition)
                    {
                        if (!_stationaryThermalObjects.Contains(thermalObject))
                            return;

                        _stationaryThermalObjects.Remove(thermalObject);
                        CalculateThermalPixels(_thermalPixelSize, this);
                    }
                    else
                    {
                        if (!_movableThermalObjects.Contains(thermalObject))
                            return;

                        _movableThermalObjects.Remove(thermalObject);
                        _surfaceAreaPerThermalPixelOfThermalObject.Remove(thermalObject);
                    }
                }
            }

            /// <summary>
            /// Gets the <see cref="Temperature"/> at a certain <paramref name="position"/> in the room.
            /// </summary>
            /// <param name="position">
            /// Absolute (global) position where the <see cref="Temperature"/> should be returned.
            /// </param>
            /// <returns>
            /// The <see cref="Temperature"/> at the specified <paramref name="position"/>.
            /// </returns>
            public Temperature GetTemperature(Vector3 position)
            {
                Vector2 relativePosition = position - Room.RoomPosition;

                Vector2Int thermalPixelIndex = GetThermalPixelIndexOfPosition(relativePosition, _thermalPixelSize, _leftBottomRoomPixel);

                if (thermalPixelIndex.x < 0 ||
                    thermalPixelIndex.y < 0 ||
                    thermalPixelIndex.x >= _thermalPixels.GetLength(0) ||
                    thermalPixelIndex.y >= _thermalPixels.GetLength(1))
                {
                    return _outsideTemperature;
                }
                else
                {
                    return _thermalPixels[thermalPixelIndex.x, thermalPixelIndex.y].Temperature;
                }
            }

            /// <summary>
            /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
            /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html">MonoBehaviour.Start</see>
            /// </summary>
            public void Start()
            {
                foreach (var stationaryThermalObject in _stationaryThermalObjects)
                {
                    stationaryThermalObject.ThermalStart(this);
                }

                foreach (var movableThermalObject in _movableThermalObjects)
                {
                    movableThermalObject.ThermalStart(this);
                }
            }

            /// <summary>
            /// Called every frame, if the MonoBehaviour is enabled.
            /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
            /// </summary>
            public void Update()
            {
                float passedTime = _remainingTime + Time.deltaTime;
                int thermalTicks = Mathf.FloorToInt(passedTime / _thermalTickDuration);

                _remainingTime = passedTime - thermalTicks * _thermalTickDuration;

                TemperatureStatisticsBuilder currentTemperatureStatisticsBuilder = null;

                for (int remainingThermalTicks = thermalTicks; remainingThermalTicks > 0; --remainingThermalTicks)
                {
                    #region Thermal Update

                    if (remainingThermalTicks == 1)
                    {
                        currentTemperatureStatisticsBuilder = new TemperatureStatisticsBuilder();
                    }

                    lock (_thermalUpdateLock)
                    {
                        //Debug.Log("Thermal Tick");

                        _outsideTemperature = _outsideTemperatureSource.GetTemperature();

                        Vector2Int discreteRoomSize = new Vector2Int(
                            x: _thermalPixels.GetLength(0),
                            y: _thermalPixels.GetLength(1));

                        ThermalPixel[,] thermalPixels = new ThermalPixel[discreteRoomSize.x,discreteRoomSize.y];
                        Dictionary<IThermalObject, float> heatFlowToThermalObjects = new Dictionary<IThermalObject, float>();

                        foreach (IThermalObject movableThermalObject in _movableThermalObjects)
                        {
                            Vector2 objectPosition = movableThermalObject.Position;
                            SurfaceDescriptor? surfaceDescriptor = null;

                            if (_movableThermalObjectsPositionCache.TryGetValue(movableThermalObject, out var cacheEntry))
                            {
                                if (objectPosition.GetDistanceTo(cacheEntry.Position) < (_thermalPixelSize / 4))
                                {
                                    objectPosition = cacheEntry.Position;
                                    surfaceDescriptor = cacheEntry.Surface;

                                    goto AddThermalObjectToPixels;
                                }

                                _movableThermalObjectsPositionCache.Remove(movableThermalObject);
                            }

                            if (TryGetThermalPixelsOfSurface(
                                objectPosition,
                                movableThermalObject.Size,
                                _thermalPixels,
                                _thermalPixelSize,
                                _leftBottomRoomPixel,
                                out var surface))
                            {
                                surfaceDescriptor = surface;
                            }
                            else
                            {
                                surfaceDescriptor = null;
                            }

                            _movableThermalObjectsPositionCache.Add(movableThermalObject, (objectPosition, surfaceDescriptor));

                            AddThermalObjectToPixels:
                            
                            if (surfaceDescriptor.HasValue)
                            {
                                surface = surfaceDescriptor.Value;

                                for (int x = surface.LeftBottomPixel.x; x <= surface.RightUpperPixel.x; ++x)
                                {
                                    for (int y = surface.LeftBottomPixel.y; y <= surface.RightUpperPixel.y; ++y)
                                    {
                                        List<IThermalObject> movableThermalObjectsList = _thermalPixels[x, y].MovableThermalObjects;

                                        if (movableThermalObjectsList == null)
                                        {
                                            movableThermalObjectsList = new List<IThermalObject>();
                                            _thermalPixels[x, y].MovableThermalObjects = movableThermalObjectsList;
                                        }

                                        movableThermalObjectsList.Add(movableThermalObject);
                                    }
                                }
                            }
                        }

                        int mostRightX = discreteRoomSize.x - 1;
                        int mostUpY = discreteRoomSize.y - 1;

                        for (int x = 0; x < discreteRoomSize.x; ++x)
                        {
                            for (int y = 0; y < discreteRoomSize.y; ++y)
                            {
                                int outsidePixelNeighbours = 0;

                                float heatFlow = 0f;
                                float partialHeatFlow;
                                float temperatureDifference;

                                ThermalPixel neighbourPixel;
                                ThermalPixel thermalPixel = _thermalPixels[x, y];

                                float maxTemperature = thermalPixel.Temperature;

                                if (x > 0) //Thermal pixel has a left neighbour pixel.
                                {
                                    neighbourPixel = _thermalPixels[x - 1, y];
                                    heatFlow += CalculateHeatFlow(thermalPixel, neighbourPixel);
                                    maxTemperature = Mathf.Max(maxTemperature, neighbourPixel.Temperature);
                                }
                                else 
                                {
                                    ++outsidePixelNeighbours;
                                }

                                if (y > 0) //Thermal pixel has a bottom neighbour pixel.
                                {
                                    neighbourPixel = _thermalPixels[x, y - 1];
                                    heatFlow += CalculateHeatFlow(thermalPixel, neighbourPixel);
                                    maxTemperature = Mathf.Max(maxTemperature, neighbourPixel.Temperature);
                                }
                                else 
                                {
                                    ++outsidePixelNeighbours;
                                }

                                if (x < mostRightX) //Thermal pixel has a right neighbour pixel.
                                {
                                    neighbourPixel = _thermalPixels[x + 1, y];
                                    heatFlow += CalculateHeatFlow(thermalPixel, neighbourPixel);
                                    maxTemperature = Mathf.Max(maxTemperature, neighbourPixel.Temperature);
                                }
                                else 
                                {
                                    ++outsidePixelNeighbours;
                                }

                                if (y < mostUpY) //Thermal pixel has a bottom neighbour pixel.
                                {
                                    neighbourPixel = _thermalPixels[x, y + 1];
                                    heatFlow += CalculateHeatFlow(thermalPixel, neighbourPixel);
                                    maxTemperature = Mathf.Max(maxTemperature, neighbourPixel.Temperature);
                                }
                                else 
                                {
                                    ++outsidePixelNeighbours;
                                }

                                if (thermalPixel.Reference != null)
                                {
                                    maxTemperature = Mathf.Max(maxTemperature, thermalPixel.Reference.Temperature.ToKelvin());
                                    
                                    partialHeatFlow = CalculateHeatFlow(thermalPixel, thermalPixel.Reference);
                                    heatFlow += partialHeatFlow;

                                    if (heatFlowToThermalObjects.ContainsKey(thermalPixel.Reference))
                                    {
                                        heatFlowToThermalObjects[thermalPixel.Reference] -= partialHeatFlow;
                                    }
                                    else
                                    {
                                        heatFlowToThermalObjects.Add(thermalPixel.Reference, -partialHeatFlow);
                                    }
                                }

                                if (thermalPixel.MovableThermalObjects != null)
                                {
                                    foreach (var movableThermalObject in thermalPixel.MovableThermalObjects)
                                    {
                                        partialHeatFlow = CalculateHeatFlow(thermalPixel, movableThermalObject);
                                        heatFlow += partialHeatFlow;
                                        maxTemperature = Mathf.Max(maxTemperature, movableThermalObject.Temperature.ToKelvin());

                                        if (heatFlowToThermalObjects.ContainsKey(movableThermalObject))
                                        {
                                            heatFlowToThermalObjects[movableThermalObject] -= partialHeatFlow;
                                        }
                                        else
                                        {
                                            heatFlowToThermalObjects.Add(movableThermalObject, -partialHeatFlow);
                                        }
                                    }
                                }

                                if (outsidePixelNeighbours > 0)
                                {
                                    temperatureDifference =
                                        _outsideTemperature.ToKelvin().Value -
                                        thermalPixel.Temperature.ToKelvin().Value;

                                    heatFlow += _thermalPixelSizeSquared *
                                                       thermalPixel.ThermalMaterial.GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial.OutsideAir, temperatureDifference) *
                                                       temperatureDifference * outsidePixelNeighbours;
                                    maxTemperature = Mathf.Max(maxTemperature, _outsideTemperature.ToKelvin());
                                }

                                float newTemperatureValue = thermalPixel.Temperature.ToKelvin().Value +
                                                            (heatFlow * _thermalTickDuration) /
                                                            (thermalPixel.ThermalMaterial.Density *
                                                             _thermalPixelSizeCubed * thermalPixel.ThermalMaterial
                                                                 .SpecificHeatCapacity);

                                if (newTemperatureValue > maxTemperature)
                                {
                                    newTemperatureValue = maxTemperature;
                                }

                                Temperature newTemperature = Temperature.FromKelvin(newTemperatureValue);
                                
                                currentTemperatureStatisticsBuilder?.AddTemperatureValue(newTemperature);
                                _capturedTemperatureStatisticsBuilder.AddTemperatureValue(newTemperature);
                                _totalTemperatureStatisticsBuilder.AddTemperatureValue(newTemperature);

                                thermalPixels[x,y] = new ThermalPixel(thermalPixel, newTemperature);
                            }
                        }

                        foreach (var heatFlowToThermalObject in heatFlowToThermalObjects)
                        {
                            //Key = Thermal object.
                            //Value = Heat that was transferred to the thermal object during the thermal update.

                            heatFlowToThermalObject.Key.ThermalUpdate(
                                transferredHeat: heatFlowToThermalObject.Value * ThermalTickDuration,
                                roomThermalManager: this);    
                        }

                        _thermalPixels = thermalPixels;
                    }

                    #endregion
                }

                if (thermalTicks > 0)
                {
                    //currentTemperatureStatisticsBuilder is guaranteed to be not null
                    CurrentTemperatureStatistics = currentTemperatureStatisticsBuilder.Build();
                }

                CapturedTemperatureStatistics = _capturedTemperatureStatisticsBuilder.Build();
                TotalTemperatureStatistics = _totalTemperatureStatisticsBuilder.Build();
            }

            private Vector2Int GetThermalPixelIndexOfPosition(
                Vector2 relativePosition,
                float thermalPixelSize,
                Vector2Int leftBottomRoomPixel)
            {
                return new Vector2Int(
                    x: Mathf.FloorToInt(relativePosition.x / thermalPixelSize) + leftBottomRoomPixel.x,
                    y: Mathf.FloorToInt(relativePosition.y / thermalPixelSize) + leftBottomRoomPixel.y);
            }

            private bool TryGetThermalPixelsOfSurface(
                Vector2 position, 
                Vector2 size, 
                ThermalPixel[,] thermalPixels, 
                float thermalPixelSize,
                Vector2Int leftBottomRoomPixel,
                out SurfaceDescriptor surfaceDescriptor)
            {
                Vector2 relativePosition = position - (Vector2)Room.RoomPosition;

                Vector2Int leftBottomPixelIndex = GetThermalPixelIndexOfPosition(relativePosition, thermalPixelSize, leftBottomRoomPixel);
                Vector2Int rightUpperPixelIndex = GetThermalPixelIndexOfPosition(relativePosition + size, thermalPixelSize, leftBottomRoomPixel);

                
                if (rightUpperPixelIndex.x < 0 ||
                    rightUpperPixelIndex.y < 0 ||
                    leftBottomPixelIndex.x >= thermalPixels.GetLength(0) ||
                    leftBottomPixelIndex.y >= thermalPixels.GetLength(1))
                {
                    surfaceDescriptor = default;
                    return false;
                }

                if (leftBottomPixelIndex.x < 0)
                {
                    leftBottomPixelIndex.x = 0;
                }

                if (leftBottomPixelIndex.y < 0)
                {
                    leftBottomPixelIndex.y = 0;
                }

                if (rightUpperPixelIndex.x >= thermalPixels.GetLength(0))
                {
                    rightUpperPixelIndex.x = thermalPixels.GetLength(0) - 1;
                }

                if (rightUpperPixelIndex.y >= thermalPixels.GetLength(1))
                {
                    rightUpperPixelIndex.y = thermalPixels.GetLength(1) - 1;
                }

                surfaceDescriptor = new SurfaceDescriptor(leftBottomPixelIndex, rightUpperPixelIndex);

                return true;
            }

            private void CalculateThermalPixels(float thermalPixelSize, ITemperatureSource<Vector3> temperatureSource)
            {
                //Calculate Room Size
                int wallThermalPixelCount = Mathf.RoundToInt(Room.WallThickness / thermalPixelSize);
                
                Vector2Int roomThermalPixelCount = new Vector2Int(
                    x: Mathf.RoundToInt(Room.RoomSize.x / thermalPixelSize),
                    y: Mathf.RoundToInt(Room.RoomSize.y / thermalPixelSize));

                ThermalPixel[,] thermalPixels = new ThermalPixel[
                    roomThermalPixelCount.x + 2 * wallThermalPixelCount,  
                    roomThermalPixelCount.y + 2 * wallThermalPixelCount];

                Vector2Int leftBottomRoomPixel = new Vector2Int(
                    x: wallThermalPixelCount,
                    y: wallThermalPixelCount);

                Vector2Int rightUpperRoomPixel = new Vector2Int(
                    x: thermalPixels.GetLength(0) - 1 - wallThermalPixelCount,
                    y: thermalPixels.GetLength(1) - 1 - wallThermalPixelCount);

                TemperatureStatisticsBuilder currentTemperatureStatisticsBuilder = new TemperatureStatisticsBuilder();

                for (int x = 0; x < thermalPixels.GetLength(0); ++x)
                {
                    for (int y = 0; y < thermalPixels.GetLength(1); ++y)
                    {
                        Temperature temperature = temperatureSource.GetTemperature(new Vector3(
                            x: 0.5f * thermalPixelSize + x - wallThermalPixelCount + Room.RoomPosition.x,
                            y: 0.5f * thermalPixelSize + y - wallThermalPixelCount + Room.RoomPosition.y));

                        currentTemperatureStatisticsBuilder.AddTemperatureValue(temperature);
                        _capturedTemperatureStatisticsBuilder.AddTemperatureValue(temperature);
                        _totalTemperatureStatisticsBuilder.AddTemperatureValue(temperature);

                        if (x >= leftBottomRoomPixel.x &&
                            x <= rightUpperRoomPixel.x &&
                            y >= leftBottomRoomPixel.y &&
                            y <= rightUpperRoomPixel.y)
                        {
                            thermalPixels[x,y] = new ThermalPixel(
                                ThermalMaterial.InsideAir, 
                                temperature);
                        }
                        else
                        {
                            thermalPixels[x, y] = new ThermalPixel(
                                ThermalMaterial.Wall,
                                temperature);
                        }
                    }
                }

                CurrentTemperatureStatistics = currentTemperatureStatisticsBuilder.Build();
                CapturedTemperatureStatistics = _capturedTemperatureStatisticsBuilder.Build();
                TotalTemperatureStatistics = _totalTemperatureStatisticsBuilder.Build();

                _surfaceAreaPerThermalPixelOfThermalObject.Clear();

                foreach (IThermalObject stationaryThermalObject in _stationaryThermalObjects)
                {
                    if (TryGetThermalPixelsOfSurface(
                        stationaryThermalObject.Position,
                        stationaryThermalObject.Size,
                        thermalPixels,
                        thermalPixelSize,
                        leftBottomRoomPixel,
                        out var surfaceDescriptor))
                    {
                        for (int x = surfaceDescriptor.LeftBottomPixel.x; x <= surfaceDescriptor.RightUpperPixel.x; ++x)
                        {
                            for (int y = surfaceDescriptor.LeftBottomPixel.y; y <= surfaceDescriptor.RightUpperPixel.y; ++y)
                            {
                                ThermalPixel thermalPixel = thermalPixels[x, y];

                                if (thermalPixel.Reference != null)
                                    throw new Exception("Overlapping Stationary Thermal Objects!");

                                thermalPixels[x, y] = new ThermalPixel(thermalPixel.ThermalMaterial, thermalPixel.Temperature, stationaryThermalObject);
                            }
                        }
                    }

                    _surfaceAreaPerThermalPixelOfThermalObject.Add(
                        stationaryThermalObject,
                        CalculateSurfaceAreaPerThermalPixelOfThermalObject(stationaryThermalObject));
                }

                foreach (IThermalObject movableThermalObject in _movableThermalObjects)
                {
                    _surfaceAreaPerThermalPixelOfThermalObject.Add(
                        movableThermalObject, 
                        CalculateSurfaceAreaPerThermalPixelOfThermalObject(movableThermalObject));
                }

                _thermalPixels = thermalPixels;
                _leftBottomRoomPixel = leftBottomRoomPixel;
                _movableThermalObjectsPositionCache.Clear();
            }

            private float CalculateSurfaceAreaPerThermalPixelOfThermalObject(IThermalObject thermalObject)
            {
                int thermalPixelCount = Mathf.RoundToInt(thermalObject.Size.x / _thermalPixelSize) *
                                        Mathf.RoundToInt(thermalObject.Size.y / _thermalPixelSize);

                return thermalObject.ThermalSurfaceArea / thermalPixelCount;
            }

            private float CalculateHeatFlow(ThermalPixel from, IThermalObject to)
            {
                float surfaceArea = _surfaceAreaPerThermalPixelOfThermalObject[to];
                float temperatureDifference = (to.Temperature - from.Temperature).ToKelvin();

                return surfaceArea *
                       from.ThermalMaterial.GetHeatTransferCoefficientToOtherThermalMaterial(to.ThermalMaterial, temperatureDifference) *
                       temperatureDifference;
            }

            private float CalculateHeatFlow(ThermalPixel from, ThermalPixel to)
            {
                float temperatureDifference = (to.Temperature - from.Temperature).ToKelvin();

                if (from.ThermalMaterial == to.ThermalMaterial)
                {
                    return _thermalPixelSize * from.ThermalMaterial.ThermalConductivity * temperatureDifference;
                }
                else
                {
                    return _thermalPixelSizeSquared *
                           from.ThermalMaterial.GetHeatTransferCoefficientToOtherThermalMaterial(to.ThermalMaterial, temperatureDifference) *
                           temperatureDifference;
                }
            }
        }
    }
}