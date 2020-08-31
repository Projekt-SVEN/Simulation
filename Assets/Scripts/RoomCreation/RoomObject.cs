using UnityEngine;

namespace Assets.Scripts.RoomCreation
{
    public class RoomObject
    {
        public enum RoomElement
        {
            Chair,
            Table,
            Window,
            Door,
            Heater,
            Closet,
            Tablet,
            Thermometer
        };

        public string Type { get; set; }

        public RoomElement Element { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float Rotation { get; set; }

        public float SizeHeight { get; set; }

        public float SizeWidth { get; set; }

        public double RotationRadians => Rotation * Mathf.PI / 180;

        public string FhemGetName { get; set; }

        public string FhemSetName { get; set; }
    }
}
