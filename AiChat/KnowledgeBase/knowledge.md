# SmartWeather System Knowledge Base

## 1. System Overview
SmartWeather is a comprehensive platform designed to manage a network of devices that monitor weather conditions and air quality.

The system is organized into a hierarchical structure: Users manage Groups, which contain Devices, and each Device is equipped with specific Sensor Metrics.

The platform allows for real-time monitoring, historical data analysis, and AI-driven forecasting.

## 2. Security and Access
Access to the system is secured and requires user authentication.

**User Account Registration:**
New users must register an account using an email address and a password (minimum 6 characters).

**Authentication Process:**
Users log in with their credentials to receive a secure access token (JWT), which is required to authorize all subsequent interactions with the system.

**Roles & Permissions:**
The system enforces role-based access control. Critical administrative actions—such as removing devices or modifying alert rules—require "Admin" privileges.

**Admin Privileges Rule:**
If You create a group you automatically have "Admin" privileges for that group.

## 3. Group Management
Groups are used to logically organize devices (e.g., "Office", "Home", "Warehouse"). Users can create new groups by providing a name and an optional description.

**Group Overview:**
Users can retrieve a list of all their groups, including details such as creation date and the count of assigned devices.

**Group Deletion Rule:**
A strict business rule applies to deletion: a group can only be deleted if it is empty (contains no devices). This prevents the accidental loss of device data.

## 4. Device Configuration
A Device represents a physical monitoring unit assigned to a specific Group.

**Device Identification:**
Each device is identified by a unique serial number. The serial number must be unique in the system.

**Device Location:**
Devices include location data such as location name, Latitude, and Longitude.

**Device Images:**
During the setup, an image file can be uploaded and associated with the device.

**Device Management:**
Administrators can update device details or remove a device from a group entirely.

## 5. Sensor Metrics
Devices report data through specific channels called Sensor Metrics.

**Supported Parameters:**
The system supports various environmental measurements, including: Temperature, Humidity, Pressure, and Dust (PM2.5 and PM10).

**Metric Configuration:**
Metrics are defined with a specific name, sensor type, and unit of measurement (e.g., Celsius, %, hPa).

## 6. Historical Data & Measurements
The system stores measurement data transmitted by the devices. Users can request historical measurement records for specific devices and parameters.

**Data Retrieval Time Constraint:**
To ensure system performance, data retrieval is limited to a maximum date range of **30 days** between the start date and end date per request.

## 7. AI Forecasts & Predictions
SmartWeather integrates an AI module to generate future environmental predictions using external Machine Learning models.

**Supported AI Models:**
Users can select specific algorithms for the prediction: LSTM (Long Short-Term Memory), BiLSTM (Bidirectional LSTM), RF (Random Forest), and ATTN_LSTM (Attention-based LSTM).

**Forecast Horizon:**
Predictions can be generated for a range of **1 to 72 hours** into the future.

## 8. Alerts & Real-Time Monitoring
The system allows users to actively monitor conditions and receive notifications about anomalies.

**Alert Rules:**
Users can define specific rules for sensor metrics (e.g., "Temperature > 30"). A rule consists of a name, a condition, a threshold value, and an enabled/disabled status.

**Alert Logs:**
When a threshold is breached, the event is recorded in the alert logs for historical review.

**Live Streaming:**
The system supports real-time monitoring via an event stream. This allows clients to maintain an open connection and receive immediate updates regarding alert status changes without needing to manually refresh the data.