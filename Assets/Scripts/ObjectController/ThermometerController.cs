using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Remote;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.ObjectController
{
    public class ThermometerController : MonoBehaviour
    {
        private bool _initialized = false;

        [SerializeField] 
        private TextMesh _userStateTextMesh;

        private float _lastTransmittedValue = 20f;

        public float TransmissionThreshold { get; set; } = 0.1f;

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public IRoomThermalManager RoomThermalManager { get; private set; }

        public RemoteThermometer RemoteThermometer { get; private set; }

        public void Initialize(IRoomThermalManager roomThermalManager, RemoteThermometer remoteThermometer)
        {
            RoomThermalManager = roomThermalManager;
            RemoteThermometer = remoteThermometer;

            try
            {
                _lastTransmittedValue = RemoteThermometer.GetState();
            }
            catch (Exception exception)
            {
                Debug.Log("Failed to get temperature from remote thermometer.");
                Debug.LogException(exception);
            }

            _initialized = true;
        }

        void Update()
        {
            if (!_initialized)
                return;

            float temperature = RoomThermalManager.GetTemperature(Position).ToCelsius();

            if (Mathf.Abs(_lastTransmittedValue - temperature) > TransmissionThreshold)
            {
                try
                {
                    RemoteThermometer.SetState(temperature);
                    _lastTransmittedValue = temperature;
                }
                catch (Exception exception)
                {
                    Debug.Log("Failed to update remote thermometer.");
                    Debug.LogException(exception);
                }
            }

            _userStateTextMesh.text = RoomThermalManager.GetTemperature(Position).ToCelsius().ToString();
        }
    }

    


}
