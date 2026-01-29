namespace AiChat.Constants
{
    internal static class AiChatConstants
    {
        public static readonly string[] AllowedTopics =
            [
              "SmartWeather system overview and hierarchical structure (Groups, Devices, Sensors)",
              "User authentication, registration, login, and JWT token management",
              "Role-based access control and permissions",
              "Group management rules, including creation and the restriction on deleting only empty groups",
              "Device administration, including handling serial numbers, location data (latitude/longitude), and device images",
              "Sensor metric definitions (Temperature, Humidity, Pressure, PM2.5, PM10) and units",
              "Retrieving historical measurement data with time range constraints (max 30 days)",
              "AI-based forecasting using specific models (LSTM, BiLSTM, RF, ATTN_LSTM) and prediction horizons (1-72 hours)",
              "Management of alert rules, conditions, and threshold values",
              "Real-time monitoring via event streams (SSE) and retrieving alert history logs",
                "Listing and browsing the user's personal groups and their details",
            "Listing, searching, and checking the status of devices belonging to the user",
            "General assistance with navigating and managing the user's weather station network"
            ];

    }
}
