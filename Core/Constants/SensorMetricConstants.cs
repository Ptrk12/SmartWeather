using Core.Enums;
using System.Collections.Frozen;

namespace Core.Constants
{
    public static class SensorMetricConstants
    {
        public static readonly FrozenDictionary<string, FrozenSet<string>> SensorAndUnits =
            new Dictionary<string, FrozenSet<string>>
        {
            { SensorType.Temperature.ToString(), FrozenSet.ToFrozenSet(new[] { "°C", "°F" }) },
            { SensorType.Humidity.ToString(), FrozenSet.ToFrozenSet(new[] { "%" }) },
            { SensorType.Pressure.ToString(), FrozenSet.ToFrozenSet(new[] { "hPa" }) },
            { SensorType.PM10.ToString(), FrozenSet.ToFrozenSet(new[] { "g/m³" }) },
            { SensorType.PM2_5.ToString(), FrozenSet.ToFrozenSet(new[] { "g/m³" }) }
        }.ToFrozenDictionary();
    }
}
