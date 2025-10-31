
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
        PM
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
}
