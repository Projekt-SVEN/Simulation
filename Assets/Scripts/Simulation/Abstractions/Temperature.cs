//#define CHECK_ON_INITIALIZATION_IF_TEMPERATURE_IS_NOT_BELOW_ABSOLUTE_ZERO

using System;
using UnityEngine;

namespace Assets.Scripts.Simulation.Abstractions
{
    public readonly struct Temperature : IEquatable<Temperature>, IEquatable<TemperatureUnit>, IEquatable<float>
    {
        public static readonly Temperature AbsoluteZero = new Temperature(-273.15f, TemperatureUnit.Celsius);

        public readonly float Value;
        public readonly TemperatureUnit Unit;

        public Temperature(float value, TemperatureUnit unit)
        {
            Unit = unit;
            Value = value;

            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Non finite value provided.");
            }

#if CHECK_ON_INITIALIZATION_IF_TEMPERATURE_IS_NOT_BELOW_ABSOLUTE_ZERO

            const string exceptionMessage = "Temperature value can't be below absolute zero.";

            switch (unit)
            {
                case TemperatureUnit.Kelvin:
                    if (value < 0f)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), value, exceptionMessage);
                    }

                    break;
                case TemperatureUnit.Celsius:
                    if (value < AbsoluteZero.Value)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), value, exceptionMessage);
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }
#endif
        }

        public Temperature To(TemperatureUnit unit)
        {
            if (Unit == unit)
            {
                return this;
            }

            switch (unit)
            {
                case TemperatureUnit.Kelvin:
                    return this.ToKelvin();
                case TemperatureUnit.Celsius:
                    return this.ToCelsius();
                default:
                    throw new NotImplementedException();
            }
        }

        public Temperature ToKelvin()
        {
            switch (Unit)
            {
                case TemperatureUnit.Kelvin:
                    return this;
                case TemperatureUnit.Celsius:
                    return new Temperature(Value - AbsoluteZero, TemperatureUnit.Kelvin);
                default:
                    throw new NotImplementedException();
            }
        }

        public Temperature ToCelsius()
        {
            switch (Unit)
            {
                case TemperatureUnit.Celsius:
                    return this;
                case TemperatureUnit.Kelvin:
                    return new Temperature(Value + AbsoluteZero, TemperatureUnit.Celsius);
                default:
                    throw new NotImplementedException();
            }
        }

        public override int GetHashCode()
        {
            if (Unit == TemperatureUnit.Kelvin)
                return Value.GetHashCode();
            else
                return ToKelvin().GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (other is null)
            {
                return Equals(AbsoluteZero);
            }
            else if (other is Temperature temperature)
            {
                return Equals(temperature);
            }
            else if (other is TemperatureUnit unit)
            {
                return Equals(unit);
            }
            else if (other is float value)
            {
                return Equals(value);
            }
            else
            {
                throw new ArgumentException($"Temperature can't be compared against {other.ToString()}.", nameof(other));
            }
        }

        public bool Equals(float value)
        {
            return Value == value;
        }

        public bool Equals(float value, float epsilon)
        {
            if (epsilon < 0f)
                throw new ArgumentOutOfRangeException(nameof(epsilon), epsilon,
                    "Epsilon is not allowed to be negative.");

            return Mathf.Abs(Value - value) < epsilon;
        }

        public bool Equals(Temperature temperature)
        {
            return Value == temperature.Value;
        }

        public bool Equals(TemperatureUnit unit)
        {
            return Unit == unit;
        }

        public static bool operator ==(Temperature a, Temperature b)
        {
            if (a.Unit == b.Unit)
            {
                return a.Value == b.Value;
            }
            else
            {
                return a.Value == b.To(a.Unit).Value;
            }
        }

        public static bool operator !=(Temperature a, Temperature b)
        {
            if (a.Unit == b.Unit)
            {
                return a.Value == b.Value;
            }
            else
            {
                return a.Value == b.To(a.Unit).Value;
            }
        }

        public static bool operator >(Temperature a, Temperature b)
        {
            if (a.Unit == b.Unit)
            {
                return a.Value > b.Value;
            }
            else
            {
                return a.Value > b.To(a.Unit).Value;
            }
        }

        public static bool operator >=(Temperature a, Temperature b)
        {
            if (a.Unit == b.Unit)
            {
                return a.Value >= b.Value;
            }
            else
            {
                return a.Value >= b.To(a.Unit).Value;
            }
        }

        public static bool operator <(Temperature a, Temperature b)
        {
            if (a.Unit == b.Unit)
            {
                return a.Value < b.Value;
            }
            else
            {
                return a.Value < b.To(a.Unit).Value;
            }
        }

        public static bool operator <=(Temperature a, Temperature b)
        {
            if (a.Unit == b.Unit)
            {
                return a.Value <= b.Value;
            }
            else
            {
                return a.Value <= b.To(a.Unit).Value;
            }
        }

        public static Temperature operator +(Temperature a)
        {
            return a;
        }

        public static Temperature operator -(Temperature a)
        {
            return new Temperature(-a.Value, a.Unit);
        }

        public static Temperature operator +(Temperature a, Temperature b)
        {
            float resultingValue;

            if (a.Unit == b.Unit)
            {
                resultingValue = a.Value + b.Value;
            }
            else
            {
                resultingValue = a.Value + b.To(a.Unit).Value;
            }

            return new Temperature(resultingValue, a.Unit);
        }

        public static Temperature operator -(Temperature a, Temperature b)
        {
            float resultingValue;

            if (a.Unit == b.Unit)
            {
                resultingValue = a.Value - b.Value;
            }
            else
            {
                resultingValue = a.Value - b.To(a.Unit).Value;
            }

            return new Temperature(resultingValue, a.Unit);
        }

        public static Temperature FromKelvin(float temperatureValue)
        {
            return new Temperature(temperatureValue, TemperatureUnit.Kelvin);
        }

        public static Temperature FromCelsius(float temperatureValue)
        {
            return new Temperature(temperatureValue, TemperatureUnit.Celsius);
        }

        public static implicit operator float(Temperature temperature)
        {
            return temperature.Value;
        }

        public static implicit operator TemperatureUnit(Temperature temperature)
        {
            return temperature.Unit;
        }

        public override string ToString()
        {
            switch (Unit)
            {
                case TemperatureUnit.Kelvin:
                    return $"{Value} K";
                case TemperatureUnit.Celsius:
                    return $"{Value} °C";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}