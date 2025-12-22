
namespace Core.Enums
{
    public enum DeviceStatus
    {
        Pending,
        Active,
        Offline,
        Error,
        Maintenance
    }
    public enum SensorType
    {
        Temperature,
        Humidity,
        Pressure,
        Dust,
        PM2_5,
        PM10
    }
    public enum GroupRole
    {
        Member,
        Admin
    }
    public enum AlertCondition
    {
        GreaterThan,
        LessThan
    }
    public enum ParameterTypes
    {
        Temperature,
        Humidity,
        Pressure,
        PM2_5,
        PM10
    }
}
