/***************************************************************
 * FILENAME: outdorSensor.ino
 * DESCRIPTION: This program for showing external
 *              temperature, humidity, altitude and pressure data.
 * AUTHOR: Osman Mazinov
 * DATE: 08/02/2026
 * MODIFICATION: Mastermind
 * CHANGES: Debug environment, mock data
 ****************************************************************/

// Outdoor Wi-Fi temperature sensor

#define DEBUG_MODE // Uncomment for fake data

#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <Hash.h>
#include <ESPAsyncTCP.h>
#include <ESPAsyncWebServer.h>
#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <Adafruit_BMP085.h>

#define SERIAL_BAUDRATE 115200
#define DHTPIN 14
#define DHTTYPE DHT22
#define HTTP_STATUS_OK 200
#define CONNECTION_DELAY 1000
#define BATTERY_MATRIX_SIZE 22
#define BATTERY_MATRIX_LENGHT 2

const char *ssid = "Ego_Entertainment";
const char *password = "36729838";
const uint8_t PORT = 80;
const char *DEVICE_NAME = "weStation";

IPAddress staticIP(192, 168, 1, 115);
IPAddress gateway(192, 168, 1, 1);
IPAddress subnet(255, 255, 255, 0);
IPAddress dns(8, 8, 8, 8);

const unsigned long LIGHT_SLEEP_DURATION_US = 9000000;
const long SENSOR_READ_INTERVAL = 10000;
const u_int8_t MAIN_DELAY_MS = 100;
const float DIVIDER_K = 4.3;
const float ADC_REF = 1.0;
const float ADC_MAX = 1023.0;

#ifndef DEBUG_MODE
DHT dht(DHTPIN, DHTTYPE);
Adafruit_BMP085 bmp;
#endif

bool dhtAvailable = false;
bool bmpAvailable = false;

float temperature = 0.0, humidity = 0.0, pressure = 0, altitude = 0, bmpTemperature = 0.0;
int chargeLevel = 0;

#ifdef DEBUG_MODE
unsigned long debugCounter = 0;
#endif

AsyncWebServer server(PORT);
unsigned long previousMillis = 0;

const float VOLTAGE_MATRIX[BATTERY_MATRIX_SIZE][BATTERY_MATRIX_LENGHT] = {
    {4.2, 100},
    {4.15, 95},
    {4.11, 90},
    {4.08, 85},
    {4.02, 80},
    {3.98, 75},
    {3.95, 70},
    {3.91, 65},
    {3.87, 60},
    {3.85, 55},
    {3.84, 50},
    {3.82, 45},
    {3.80, 40},
    {3.79, 35},
    {3.77, 30},
    {3.75, 25},
    {3.73, 20},
    {3.71, 15},
    {3.69, 10},
    {3.61, 5},
    {3.27, 0},
    {0, 0}};

/************************ Frontend side *************************/
const char index_html[] PROGMEM = R"rawliteral(
<!DOCTYPE HTML>
<html>
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css">
  <style>
    body { font-family: Arial, sans-serif; background-color: #F5F5F5; margin: 0; padding: 16px; }
    h2 { text-align: center; color: #333; }
    .container {
      max-width: 500px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
    }
    .card { 
      display: flex; 
      align-items: center; 
      background: #fff; 
      border-radius: 12px; 
      padding: 16px; 
      margin-bottom: 16px; 
      box-shadow: 0 4px 8px rgba(0,0,0,0.1); 
    }
    .card i { font-size: 20px; width: 24px; text-align: center; }
    .card .value { font-size: 24px; font-weight: bold; margin-left: 16px; }
    .status { 
      background: #fff; 
      border-radius: 12px; 
      padding: 12px; 
      margin-bottom: 16px; 
      text-align: center; 
      color: #666; 
      font-size: 14px; 
    }
    .debug-badge { background: #ff9800; color: white; padding: 2px 8px; border-radius: 4px; font-size: 12px; vertical-align: middle; }
  </style>
</head>
<body>
  <div class="container">
    <h2>Meteora 1.0 %DEBUG_BADGE%</h2>
    <div class="status">Status: <span id="statusText">Loading...</span></div>
    
    <div class="card" style="color:#FF5722;"><i class="fas fa-temperature-high"></i><span id="temperature" class="value">%TEMPERATURE%</span></div>
    <div class="card" style="color:#03A9F4;"><i class="fas fa-tint"></i><span id="humidity" class="value">%HUMIDITY%</span></div>
    <div class="card" style="color:#4CAF50;"><i class="fas fa-tachometer-alt"></i><span id="pressure" class="value">%PRESSURE%</span></div>
    <div class="card" style="color:#9C27B0;"><i class="fas fa-mountain"></i><span id="altitude" class="value">%ALTITUDE%</span></div>
    <div class="card" style="color:#FF9800;"><i class="fas fa-thermometer"></i><span id="bmpTemperature" class="value">%BMPTEMPERATURE%</span></div>
    <div class="card" style="color:#607D8B;"><i class="fas fa-battery-full"></i><span id="chargeLevel" class="value">%BATTERY_STATUS%</span></div>
  </div>

  <script>
    function fetchValue(id, endpoint, suffix) {
      setInterval(() => {
        fetch(endpoint)
          .then(r => r.text())
          .then(val => {
            const el = document.getElementById(id);
            if (val === "N/A") el.innerText = val;
            else el.innerText = val + suffix;
          })
          .catch(err => console.error('Error fetch:', err));
      }, 10000);
    }
    
    fetch('/status').then(r => r.text()).then(t => document.getElementById('statusText').innerText = t);
    
    // Run updates
    fetchValue("temperature", "/temperature", " °C");
    fetchValue("humidity", "/humidity", "%");
    fetchValue("pressure", "/pressure", " mmHg");
    fetchValue("altitude", "/altitude", " m");
    fetchValue("bmpTemperature", "/bmpTemperature", " °C");
    fetchValue("chargeLevel", "/battery_status", "%");
  </script>
</body>
</html>)rawliteral";

String getStringFromRoutings(const String &var)
{
  if (var == "TEMPERATURE")
    return dhtAvailable ? String(temperature, 1) : "N/A";
  if (var == "HUMIDITY")
    return dhtAvailable ? String(humidity, 0) : "N/A";
  if (var == "PRESSURE")
    return bmpAvailable ? String(pressure, 1) : "N/A";
  if (var == "ALTITUDE")
    return bmpAvailable ? String(altitude, 0) : "N/A";
  if (var == "BMPTEMPERATURE")
    return bmpAvailable ? String(bmpTemperature, 1) : "N/A";
  if (var == "BATTERY_STATUS")
    return String(chargeLevel);
  if (var == "DEBUG_BADGE")
  {
#ifdef DEBUG_MODE
    return "<span class='debug-badge'>DEBUG</span>";
#else
    return "";
#endif
  }
  return String();
}

/************** Support functions (configuration and reading) **************/
void setupBmp180()
{
#ifndef DEBUG_MODE
  if (bmp.begin())
  {
    bmpAvailable = true;
    Serial.println("BMP180 OK");
  }
  else
  {
    bmpAvailable = false;
    Serial.println("BMP180 FAIL");
  }
#endif
}

int getBatteryPercent(float v)
{
  for (int i = 0; i < BATTERY_MATRIX_SIZE; i++)
  {
    if (v >= VOLTAGE_MATRIX[i][0])
    {
      return (int)VOLTAGE_MATRIX[i][1];
    }
  }
  return 0;
}

float readBatteryVoltage()
{
  int raw = analogRead(A0);
  float vadc = raw * (ADC_REF / ADC_MAX); // ADC / 10 bit
  return vadc * DIVIDER_K;                // (33k+10k)/10k
}

void setup()
{
  Serial.begin(SERIAL_BAUDRATE);
  if (!WiFi.config(staticIP, gateway, subnet, dns))
  {
    Serial.println("Failed to configure Wi Fi connection!");
  }

  // Connect to Wi-Fi
  WiFi.begin(ssid, password);
  Serial.println("Connecting to WiFi");

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(CONNECTION_DELAY);
    Serial.println(".");
  }

  WiFi.hostname(DEVICE_NAME);

  Serial.println("\n✓ Connected. IP: " + WiFi.localIP().toString());

#ifdef DEBUG_MODE
  dhtAvailable = true;
  bmpAvailable = true;
#else
  dht.begin();
  setupBmp180();
  dhtAvailable = !isnan(dht.readTemperature());
#endif

  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(HTTP_STATUS_OK, "text/html", index_html, getStringFromRoutings); });

  server.on("/temperature", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(HTTP_STATUS_OK, "text/plain", dhtAvailable ? String(temperature, 1) : "N/A"); });
  server.on("/humidity", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(HTTP_STATUS_OK, "text/plain", dhtAvailable ? String(humidity, 0) : "N/A"); });
  server.on("/pressure", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(HTTP_STATUS_OK, "text/plain", bmpAvailable ? String(pressure, 1) : "N/A"); });
  server.on("/altitude", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(HTTP_STATUS_OK, "text/plain", bmpAvailable ? String(altitude, 0) : "N/A"); });
  server.on("/bmpTemperature", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(HTTP_STATUS_OK, "text/plain", bmpAvailable ? String(bmpTemperature, 1) : "N/A"); });
  server.on("/battery_status", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(HTTP_STATUS_OK, "text/plain", String(chargeLevel)); });
  server.on("/status", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(HTTP_STATUS_OK, "text/plain", "DHT: " + String(dhtAvailable ? "OK" : "FAIL") + ", BMP: " + String(bmpAvailable ? "OK" : "FAIL")); });

  server.begin();
}

void loop()
{
  unsigned long currentMillis = millis();
  if (currentMillis - previousMillis >= SENSOR_READ_INTERVAL)
  {
    previousMillis = currentMillis;

#ifdef DEBUG_MODE
    debugCounter++;
    temperature = 22.0 + sin(debugCounter * 0.1) * 5;
    humidity = 50 + cos(debugCounter * 0.1) * 10;
    altitude = 150.0 + sin(debugCounter * 0.02) * 20;
    bmpTemperature = 21.5 + sin(debugCounter * 0.1) * 5;
    pressure = 750 + sin(debugCounter * 0.05) * 10;
    chargeLevel = 100 - (debugCounter % 20);
#else
    if (dhtAvailable)
    {
      float t = dht.readTemperature();
      float h = dht.readHumidity();
      if (!isnan(t))
        temperature = t;
      if (!isnan(h))
        humidity = h;
    }
    if (bmpAvailable)
    {
      pressure = bmp.readPressure() / 133.3;
      altitude = bmp.readAltitude();
      bmpTemperature = bmp.readTemperature();
    }

    float current_battery_voltage = readBatteryVoltage();
    chargeLevel = getBatteryPercent(current_battery_voltage);

#endif
    Serial.printf("Update: T=%.1f H=%.1f P=%.1f A=%.1f BmpT=%.1f Bat=%d\n", temperature, humidity, pressure, altitude, bmpTemperature, chargeLevel);
  }
  WiFi.setSleepMode(WIFI_LIGHT_SLEEP);
  delay(MAIN_DELAY_MS);
}