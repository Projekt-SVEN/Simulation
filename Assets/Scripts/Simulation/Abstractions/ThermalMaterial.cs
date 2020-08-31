using System;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Simulation.Abstractions
{
    /// <remarks>
    /// WARNING: The functions are NOT thermodynamically correct implemented.
    /// Many thermodynamical depend on the temperature, pressure and other thermodynamical context.
    /// Because this is a dumb an simple thermodynamical simulation the corresponding inaccuracies will be ignored.
    /// </remarks>
    public abstract class ThermalMaterial
    { 
        public static readonly ThermalMaterial InsideAir = new InsideAirThermalMaterial();
        public static readonly ThermalMaterial OutsideAir = new OutsideAirThermalMaterial();
        public static readonly ThermalMaterial Wall = new WallThermalMaterial();
        public static readonly ThermalMaterial Human = new HumanThermalMaterial();
        public static readonly ThermalMaterial WindowOpen = new WindowOpenThermalMaterial();
        public static readonly ThermalMaterial WindowClosed = new WindowClosedThermalMaterial();
        public static readonly ThermalMaterial Heater = new HeaterThermalMaterial();

        protected abstract uint ID { get; }

        /// <summary>
        /// Density of the <see cref="ThermalMaterial"/> in kg/m³.
        /// </summary>
        /// <remarks>
        /// Mass per unit of volume.
        /// </remarks>
        public abstract float Density { get; }

        /// <summary>
        /// Specific heat capacity of the <see cref="ThermalMaterial"/> in J/(K·kg).
        /// </summary>
        /// <remarks>
        /// Amount of energy that must be added, in the form of heat, to one unit of mass of the substance in order to cause an increase of one unit in its temperature.
        /// </remarks>
        public abstract float SpecificHeatCapacity { get; }

        /// <summary>
        /// Thermal conductivity of the <see cref="ThermalMaterial"/> in W/(m·K). 
        /// </summary>
        /// <remarks>
        /// Measure of its ability to conduct heat.
        /// </remarks>
        public abstract float ThermalConductivity { get; }

        /// <summary>
        /// Gets the heat transfer coefficient from this <see cref="ThermalMaterial"/> to another <see cref="ThermalMaterial"/>.
        /// </summary>
        /// <param name="otherThermalMaterial">
        /// The other <see cref="ThermalMaterial"/> that is involved in the heat transfer to get the heat transfer coefficient for.
        /// </param>
        /// <returns>
        /// The heat transfer coefficient from this <see cref="ThermalMaterial"/> to <paramref name="otherThermalMaterial"/> in W/(m²·K).
        /// </returns>
        public abstract float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial, float temperatureDifference);

        private class InsideAirThermalMaterial : ThermalMaterial
        {
            private static float[] HeatTransferCoefficients;

            static InsideAirThermalMaterial()
            {
                //TODO: find a better solution

                float[] heatTransferCoefficients = new float[7];
                heatTransferCoefficients[0] = 8.1f; //InsideAir
                heatTransferCoefficients[1] = 14f; //OutsideAir
                heatTransferCoefficients[2] = 8.1f; //Wall
                heatTransferCoefficients[3] = 7f; //Human
                heatTransferCoefficients[4] = 8.1f; //WindowClosed
                heatTransferCoefficients[5] = 12f; //WindowOpen
                heatTransferCoefficients[6] = 1000f; //Heater

                HeatTransferCoefficients = heatTransferCoefficients;
            }

            protected override uint ID => 0;

            public override float Density => 1.225f; //at 15°C and 1013.25 hPa

            public override float SpecificHeatCapacity => 1012f; //isobaric mass heat capacity at 20°C and 101,3kPa

            public override float ThermalConductivity => 10f;//0.025f; //at 20°C and 101,3kPa

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial, float temperatureDifference)
            {
                uint id = otherThermalMaterial.ID;

                if (id < HeatTransferCoefficients.Length)
                    return HeatTransferCoefficients[id];
                else
                    return 0f;
            }
        }

        private class OutsideAirThermalMaterial : ThermalMaterial
        {
            protected override uint ID => 1;

            public override float Density => 1.225f; //at 15°C and 1013.25 hPa

            public override float SpecificHeatCapacity => 1012f; //isobaric mass heat capacity at 20°C and 101,3kPa

            public override float ThermalConductivity => 0.025f; //at 20°C and 101,3kPa

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial, float temperatureDifference)
            {
                if (otherThermalMaterial == Wall)
                    return 23f;
                else
                    return 0f;
            }
        }

        private class WallThermalMaterial : ThermalMaterial
        {
            private static readonly float[] HeatTransferCoefficients;

            static WallThermalMaterial()
            {
                HeatTransferCoefficients = new float[2];
                HeatTransferCoefficients[0] = 8.1f; //InsideAir
                HeatTransferCoefficients[1] = 23f; //OutsideAir
            }

            protected override uint ID => 2;

            public override float Density => 2100;

            public override float SpecificHeatCapacity => 900f;

            public override float ThermalConductivity => 1.1f;

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial, float temperatureDifference)
            {
                uint id = otherThermalMaterial.ID;

                if (id < HeatTransferCoefficients.Length)
                    return HeatTransferCoefficients[id];
                else
                    return 0f;
            }
        }

        private class HumanThermalMaterial : ThermalMaterial
        {
            protected override uint ID => 3;

            public override float Density => 998f;

            public override float SpecificHeatCapacity => 0.209f;

            public override float ThermalConductivity => throw new NotSupportedException();

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial, float temperatureDifference)
            {
                if (otherThermalMaterial == InsideAir)
                    return 1.183f * Mathf.Pow(temperatureDifference, 0.347f);

                else
                    return 0f;
            }
        }

        private class WindowOpenThermalMaterial : ThermalMaterial
        {
            protected override uint ID => 4;

            public override float Density => 2579f;

            public override float SpecificHeatCapacity => 840f;

            public override float ThermalConductivity => throw new NotSupportedException();

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial, float temperatureDifference)
            {
                if (otherThermalMaterial == InsideAir)
                    return 8.1f;
                else
                    return 0f;
            }
        }

        private class WindowClosedThermalMaterial : ThermalMaterial
        {
            protected override uint ID => 5;

            public override float Density => 2579f;

            public override float SpecificHeatCapacity => 840;

            public override float ThermalConductivity => throw new NotSupportedException();

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial, float temperatureDifference)
            {
                if (otherThermalMaterial == InsideAir)
                    return 12f;
                else
                    return 0f;
            }
        }

        private class HeaterThermalMaterial : ThermalMaterial
        {
            protected override uint ID => 6;

            public override float Density => 1000f;

            public override float SpecificHeatCapacity => 4190;

            public override float ThermalConductivity => throw new NotSupportedException();

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial, float temperatureDifference)
            {
                if (otherThermalMaterial == InsideAir)
                    return 1500f;
                else
                    return 0f;
            }
        }
    }
}