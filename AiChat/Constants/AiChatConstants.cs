namespace AiChat.Constants
{
    internal static class AiChatConstants
    {
        public static readonly string[] AllowedTopics =
        [
             "SmartWeather system overview and hierarchical structure (Groups, Devices, Sensors)",
             "User authentication, registration, login, and JWT token management",
             "Role-based access control and permissions",
             "Group management rules, including creation and restrictions (e.g., deleting only empty groups)",            
             "Listing, creating, and browsing the user's personal groups and their details",
             "Listing, searching, and checking the status/location of devices belonging to the user",
             "General assistance with navigating and managing the user's weather station network",
             "Reading current and historical sensor measurements (temperature, humidity, pressure, pm2_5, pm10) for specific devices",
             "Analyzing measurement history within the allowed 30-day time range",         
             "Requesting weather predictions and AI-based forecasts using supported models (LSTM,BiLSTM,   RF,ATTN_LSTM)",
             "Management of alert rules, setting thresholds, and checking alert logs",
             "Real-time monitoring via event streams (SSE)"
        ];
    }
}
