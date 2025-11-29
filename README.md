
<img width="2048" height="2048" alt="logo" src="https://github.com/user-attachments/assets/2524914e-7684-439d-bc3f-52051bbea462" />


# 🌤️ SmartWeather API

**SmartWeather** is a robust IoT backend system designed to manage environmental monitoring networks. It provides a comprehensive RESTful API for organizing devices, tracking real-time sensor data, and managing automated alerts based on environmental conditions.

## 🚀 Project Overview

This application serves as the central control unit for smart weather stations. It allows users to group devices by location, monitor specific metrics and receive notifications when critical thresholds are breached.

## ✨ Key Features

### 🔐 Secure Authentication & Authorization
* **User Management:** Full registration and login flows.
* **JWT Security:** Stateless authentication using JSON Web Tokens (Bearer Auth).
* **Role-Based Access:** Granular permissions distinguishing between `Admin` and standard users.

### 📡 Device & Group Management
* **Hierarchical Organization:** Manage devices within logical **Groups**.
* **Device Lifecycle:** Full CRUD capabilities for devices, including metadata like **Serial Number** and **Location**.

### 🌡️ Sensor Metrics & Monitoring
* **Customizable Metrics:** Dynamically add and configure sensors for every device.
* **Supported Parameters:** Designed to handle various environmental data points including:
    * Temperature & Humidity
    * Pressure
    * Air Quality (PM2.5, PM10)
* **Historical Data:** Retrieve historical measurement data via query parameters.

### 🔔 Proactive Alerting System
* **Custom Rules:** Define logic-based alert rules for specific sensor metrics.
* **Threshold Management:** Set precise threshold values and conditions.
* **Audit Logs:** Maintain a searchable history of **Alert Logs** to track when and why alerts were triggered.
