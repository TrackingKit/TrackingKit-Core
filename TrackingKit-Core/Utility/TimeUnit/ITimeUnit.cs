using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;
using Xunit;

namespace TrackingKit_Core
{
    public interface ITimeUnit : IComparable<ITimeUnit>
    {
        string Name { get; }
        string PluralName { get; }

        T ConvertTo<T>() where T : ITimeUnit, new();
    }

    public class TickTimeUnit : ITimeUnit
    {
        private readonly int _tick;

        public TickTimeUnit(int tick)
        {
            if (tick < 0)
                throw new ArgumentException("Tick cannot be negative.", nameof(tick));

            _tick = tick;
        }

        public string Name => "Tick";
        public string PluralName => "Ticks";

        public int CompareTo(ITimeUnit? other)
        {
            throw new NotImplementedException();
        }

        public T ConvertTo<T>() where T : ITimeUnit, new()
        {
            if (typeof(T) == typeof(SecondTimeUnit))
            {
                double convertedValue = _tick / 60.0;  // Assuming 60 ticks per second as an example.
                return (T)Activator.CreateInstance(typeof(T), convertedValue);
            }

            if (typeof(T) == typeof(TickTimeUnit))
            {
                return (T)Activator.CreateInstance(typeof(T), _tick);
            }

            throw new NotSupportedException($"Conversion from {this.GetType().Name} to {typeof(T).Name} is not supported.");
        }

        public override string ToString()
        {
            return $"{_tick} {PluralName}";
        }

    }

    public class SecondTimeUnit : ITimeUnit
    {
        private readonly double _second;

        public SecondTimeUnit(double second)
        {
            if (second < 0)
                throw new ArgumentException("Second cannot be negative.", nameof(second));

            _second = second;
        }

        public string Name => "Second";
        public string PluralName => "Seconds";

        public int CompareTo(ITimeUnit? other)
        {
            throw new NotImplementedException();
        }

        public T ConvertTo<T>() where T : ITimeUnit, new()
        {
            if (typeof(T) == typeof(TickTimeUnit))
            {
                int convertedValue = (int)(_second * 60);  // Assuming 60 ticks per second as an example.
                return (T)Activator.CreateInstance(typeof(T), convertedValue);
            }

            if (typeof(T) == typeof(SecondTimeUnit))
            {
                return (T)Activator.CreateInstance(typeof(T), _second);
            }

            throw new NotSupportedException($"Conversion from {this.GetType().Name} to {typeof(T).Name} is not supported.");
        }

        public override string ToString()
        {
            return $"{_second} {PluralName}";
        }
    }
}
