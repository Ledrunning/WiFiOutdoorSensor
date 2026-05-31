# WiFiOutdoorSensor

WiFiOutdoorSensor is a wireless outdoor temperature and humidity monitoring system. It uses ESP8266 for network communication, BMP180 for barometric pressure measurements, and DHT-22 for temperature and humidity data. Powered by a battery, it's designed to be low-power and can be accessed via a web server, an Android application, or a Windows desktop application.

**Full-stack solution:** Hardware -> Firmware -> Web Interface -> Mobile App -> Desktop Application
---

[![.NET Desktop](https://github.com/Ledrunning/WiFiOutdoorSensor/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Ledrunning/WiFiOutdoorSensor/actions/workflows/dotnet-desktop.yml)
[![Android CI](https://github.com/Ledrunning/WiFiOutdoorSensor/actions/workflows/android.yml/badge.svg)](https://github.com/Ledrunning/WiFiOutdoorSensor/actions/workflows/android.yml)
[![Build ESP8266 Firmware](https://github.com/Ledrunning/WiFiOutdoorSensor/actions/workflows/c-cpp.yml/badge.svg)](https://github.com/Ledrunning/WiFiOutdoorSensor/actions/workflows/c-cpp.yml)

## Table of Contents

- [Features](#features)
- [Hardware](#hardware)
- [Components](#components)
- [Quick Start](#quick-start)
- [Setup Guide](#setup-guide)
- [Power Management](#power-management)
- [Interfaces](#interfaces)
  - [Web Interface](#web-interface)
  - [Android Application](#android-application)
  - [Windows Desktop Application](#windows-desktop-application)
- [Project Structure](#project-structure)
- [Roadmap](#roadmap)
- [License](#license)
- [Support](#support)

---

## Features

**Wireless Connectivity**: ESP8266 Wi-Fi module for remote data transmission  
**Multi-Sensor**: Temperature, humidity, and atmospheric pressure measurements  
**Real-time Web Dashboard**: Access sensor data from any web browser  
**Android App**: Monitor data from anywhere within Wi-Fi range  
**Windows Desktop App**: Real-time monitoring on your PC  
**Debug Mode**: Mock data for testing without physical sensors  
**Professional CI/CD**: Automated builds for all platforms  

---

## Hardware

### Specifications

| Component | Model | Function |
|-----------|-------|----------|
| Microcontroller | ESP8266 NodeMCU | Wi-Fi connectivity & data processing |
| Temperature/Humidity | DHT-22 | Environmental monitoring ±0.5°C accuracy |
| Pressure Sensor | BMP180 | Atmospheric pressure & altitude |
| Power Supply | 5V DC | USB or external adapter |
| Feature / TODO | Solar Panel 5V 1W | Indefinite runtime (with TP4056 charger) |

### Schematic

Wire the sensors according to the connections shown below:

```
ESP8266 (NodeMCU)
├─ GPIO14 (D5) → DHT22 Data pin
├─ GPIO4 (D2)  → BMP180 SDA (I2C)
├─ GPIO5 (D1)  → BMP180 SCL (I2C)
├─ A0           → Battery voltage divider (100k+100k)
└─ 5V GND       → Power ground

Optional Solar Setup:
[Solar Panel 5V] → [TP4056 Charger] → [18650 Li-Ion] → [3.3V LDO] → [ESP8266]
```

---

## Components

### Core Sensors

- **ESP8266**: ESP07 Low-cost Wi-Fi microcontroller with built-in TCP/IP stack
- **BMP180**: I2C pressure sensor, accurate and reliable
- **DHT-22**: Stable and accurate temperature/humidity sensor (±0.5°C / ±2% RH)

### Optional Power Components

- **TP4056**: Lithium charger module with protection circuits
- **18650 Li-Ion Battery**: 2500-3500 mAh for portable operation
- **Solar Panel 5V 1W**: 80×55mm polycrystalline panel
- **Low-Iq LDO Regulator**: XC6206 (1μA quiescent) or HT7333 (4μA) for better battery life

---

## Quick Start

### 1. Hardware Assembly

```
1. Connect BMP180 to I2C pins (GPIO4=SDA, GPIO5=SCL)
2. Connect DHT22 data pin to GPIO14
3. Connect 5V power supply
4. Connect the extra antenna to ESP module
```

### 2. Firmware Upload

```bash
# Clone repository
git clone https://github.com/Ledrunning/WiFiOutdoorSensor.git
cd WiFiOutdoorSensor

# Open in VSCode with PlatformIO
code .

# Edit configuration
# - Edit SSID and password in the sketch
# - Set desired IP address (default: 192.168.1.125)
# - Build firmware
# - Upload to ESP8266

# Or using command line:
pio run -e production -t upload
```

### 3. Access the Interface

- **Web Dashboard**: http://192.168.1.125
- **API Status**: http://192.168.1.125/status
- **Individual Endpoints**:
  - `/temperature` - Current temperature (°C)
  - `/humidity` - Current humidity (%)
  - `/pressure` - Atmospheric pressure (mmHg)
  - `/altitude` - Altitude (m)
---

## Setup Guide

### Hardware Setup

1. **Assemble the circuit** according to the schematic in `EspOutdoorStation/circuit.pdf`
2. **Verify all connections** - especially I2C pullups and power connections
3. **Connect 5V power supply** (USB or external adapter)
4. **Test with Serial Monitor** at 115200 baud

### Software Setup

1. **Install Dependencies**
   - VSCode with PlatformIO extension
   - ESP8266 board support in PlatformIO

2. **Configure Firmware**
   ```cpp
   // In the sketch:
   const char *ssid = "Your_WiFi_SSID";
   const char *password = "Your_WiFi_Password";
   IPAddress staticIP(192, 168, 1, 125);
   ```

3. **Choose Compile Mode**
   ```bash
   # Production mode (real sensors)
   pio run -e production -t upload
   
   # Debug mode (mock data for testing)
   pio run -e debug -t upload
   ```

4. **Verify Serial Output**
   ```
   Connected. IP: 192.168.1.125
   Web server started
   ```

---

## Power Management

| State | Current | Duration |
|-------|---------|----------|
| Active (WiFi ON) | 80 mA | ~1 sec/10 sec |
| Light Sleep | 0.9 mA | ~9 sec/10 sec |
| **Average** | **~9 mA** | Continuous |

---

## Interfaces

### Web Interface

![Web Dashboard](docs/screenshots/web-interface.png)

**Features:**
- Real-time data updates
- Responsive design for desktop and mobile browsers
- Automatic sensor status detection
- Visual indication of data availability
- Network-agnostic (HTTP only, no HTTPS required)

**Access:**
```
http://192.168.1.125
```

### Android Application

![Android App](docs/screenshots/android-app.png)

**Features:**
- Real-time sensor monitoring
- Sequential endpoint polling for stability
- Connection status indicator
- Automatic retry logic with exponential backoff
- Detailed error diagnostics in Logcat

**Setup:**
```
1. Open source in Android Studio
2. Edit IP address in TelemetryService.java:
   "192.168.1.125"
3. Ensure cleartext traffic is permitted:
   - AndroidManifest.xml: android:usesCleartextTraffic="true"
   - res/xml/network_security_config.xml configured
4. Build and run on physical device or emulator
```

**Requirements:**
- Android 8.0+ (API 26)
- Internet permission
- Same Wi-Fi network as ESP8266

### Windows Desktop Application

![Windows App](docs/screenshots/windows-app.png)

**Features:**
- .NET Framework demo application
- Real-time data visualization
- Lightweight and responsive UI

**Setup:**
```
1. Open project in Visual Studio
2. Update IP address in config
3. Build solution
4. Run the executable
```

**Technology Stack:**
- C# WinForms
- HttpClient for REST API calls
- .NET Framework 4.8

---

## Project Structure

```
WiFiOutdoorSensor/
├── firmware/
│   ├── src/
│   │   └── outdoorSensor.ino          # Main ESP8266 sketch
│   ├── platformio.ini                 # Build configuration (production/debug modes)
│   └── README.md
├── android/
│   ├── app/
│   │   ├── src/
│   │   │   └── TelemetryService.java  # Network communication
│   │   └── AndroidManifest.xml
│   └── README.md
├── windows/
│   ├── WiFiOutdoorSensor/
│   │   └── MainForm.cs
│   └── WiFiOutdoorSensor.sln
├── docs/
│   ├── circuit_diagram.pdf
│   └── screenshots/
├── .github/
│   └── workflows/                     # CI/CD pipelines
├── LICENSE
└── README.md
```

---

## Roadmap

### Version 1.1 (Current)
- 5V 1A power supply
- DEBUG_MODE for testing without sensors
- Network diagnostics in Serial output

### Version 2.0 (Planned)
- [ ] Deep Sleep + Cloud Backend for 1-2 year battery life
- [ ] ESP32 migration (better power efficiency, more features)
- [ ] LoRa support for long-range outdoor deployment
- [ ] Android: Configurable IP address in UI
- [ ] Android: Local SQLite database for historical data storage
- [ ] Windows: Real-time graphing and data export
- [ ] Improved enclosure design with solar panel integration

### Future Enhancements
- [ ] MQTT support for home automation integration
- [ ] Data cloud sync (optional)
- [ ] Over-the-air (OTA) firmware updates
- [ ] Multiple sensor node support
- [ ] iOS application

---

## Technical Details

### Firmware Features

- **Sensor Validation**: Only displays data from available sensors
- **Debug Mode**: Mock data generation for testing without hardware
- **Comprehensive Logging**: Serial output with detailed diagnostics

### Network Protocol

- **HTTP REST API** - Simple, reliable, no HTTPS overhead
- **Request Format**: Simple GET requests to endpoints
- **Response Format**: Plain text values (temperature, humidity, pressure, etc.)
- **Update Interval**: Configurable (default: 10 seconds)

### Build Configurations

```ini
[env:production]
# Real sensors, optimized for battery

[env:debug]
build_flags = -DDEBUG_MODE
# Mock data, full functionality testing
```

---

## Troubleshooting

### ESP8266 not responding

```bash
# 1. Verify network connection
ping 192.168.1.125

# 2. Check port 80
telnet 192.168.1.125 80
curl http://192.168.1.125

# 3. Monitor Serial output
pio device monitor -b 115200
```

### Android app can't connect

- Confirm `android:usesCleartextTraffic="true"` in AndroidManifest.xml
- Check IP address in TelemetryService.java matches your ESP
- Check Internet permission
- Verify phone is on same Wi-Fi network
- Check firewall isn't blocking port 80

---

## Performance Metrics

### Sensor Accuracy

| Sensor | Accuracy | Response Time |
|--------|----------|----------------|
| DHT-22 | ±0.5°C / ±2% RH | ~2 sec |
| BMP180 | ±1 hPa | ~10 msec |

### Network Performance

- **Response Time**: < 100 ms per endpoint
- **Update Interval**: 10 seconds (configurable)
- **Wi-Fi Range**: Typical home router coverage
- **Concurrent Connections**: Single browser/app supported

### Power Efficiency

- **Active Current**: 80 mA (WiFi transmit)

---

## Contributing

Contributions are welcome! Please feel free to:

1. **Report Issues**: Create a GitHub issue with detailed description
2. **Suggest Features**: Open a discussion for new functionality
3. **Submit Pull Requests**: Fork, develop, and submit PR
4. **Improve Documentation**: Help keep docs up-to-date

---

## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions...

---

## Support

**Having issues or questions?**

1. **Check Troubleshooting Section** - Most common issues covered
2. **Review Serial Monitor Output** - Detailed diagnostics included
3. **Create GitHub Issue** - Provide error logs and hardware details
4. **Check Documentation** - Comprehensive guides in /docs folder

**Contact:**
- GitHub Issues: [WiFiOutdoorSensor/issues](https://github.com/Ledrunning/WiFiOutdoorSensor/issues)
- Project Repository: [Ledrunning/WiFiOutdoorSensor](https://github.com/Ledrunning/WiFiOutdoorSensor)

---

## Acknowledgments

This project combines several open-source libraries:

- **ESP8266 Arduino Core** - Espressif Systems
- **ESPAsyncWebServer** - by me-no-dev
- **Adafruit Sensor Libraries** - Adafruit Industries
- **Android Volley** - Google Android Team
- **.NET Framework** - Microsoft

Thanks to the open-source community for providing excellent tools and documentation!

---

## Project Status

**Active Development** - Actively maintained and improved.

Current focus:
- Optimizing battery consumption
- Expanding platform support
- Improving documentation
- Community feedback implementation

---

## Future Vision

This project demonstrates the possibility of building a **complete IoT solution** from hardware design through cloud integration. It's perfect for:

- Home weather monitoring
- Garden/plant monitoring
- Environmental research
- IoT learning projects
- Industrial monitoring

Whether you're a hobbyist, student, or professional, WiFiOutdoorSensor provides a solid foundation for environmental monitoring applications.

---

**Enjoy building and monitoring!**
