/***************************************************************
 * FILENAME: outdorSensor.ino
 * DESCRIPTION: This program for showing external
 * temperature, humidity, altitude and pressure data.
 * AUTHOR: Osman Mazinov
 * DATE: 17/05/2026
 * MODIFICATION: Mastermind
 * CHANGES: Fixed line power logic, removed blocking delays and heavy JS polling
 ****************************************************************/

#define DEBUG_MODE // Comment for real using

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
#define CONNECTION_DELAY 500

const char *ssid = "Ego_Entertainment";
const char *password = "36729838";
const uint8_t PORT = 80;
const char *DEVICE_NAME = "weStation";

IPAddress staticIP(192, 168, 1, 125);
IPAddress gateway(192, 168, 1, 1);
IPAddress subnet(255, 255, 255, 0);
IPAddress dns(8, 8, 8, 8);

// The optimal polling interval for the cable is 5 seconds
const long SENSOR_READ_INTERVAL = 5000;

#ifndef DEBUG_MODE
DHT dht(DHTPIN, DHTTYPE);
Adafruit_BMP085 bmp;
#endif

bool dhtAvailable = false;
bool bmpAvailable = false;

float temperature = 0.0, humidity = 0.0, pressure = 0, altitude = 0, bmpTemperature = 0.0;

#ifdef DEBUG_MODE
unsigned long debugCounter = 0;
#endif

AsyncWebServer server(PORT);
unsigned long previousMillis = 0;

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
    .container { max-width: 500px; margin: 0 auto; display: flex; flex-direction: column; }
    .card { display: flex; align-items: center; background: #fff; border-radius: 12px; padding: 16px; margin-bottom: 16px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }
    .card i { font-size: 20px; width: 24px; text-align: center; }
    .card .value { font-size: 24px; font-weight: bold; margin-left: 16px; }
    .status { background: #fff; border-radius: 12px; padding: 12px; margin-bottom: 16px; text-align: center; color: #666; font-size: 14px; }
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
    <div class="card" style="color:#4CAF50;"><i class="fas fa-plug"></i><span id="powerStatus">DC 5V (USB)</span></div>
  </div>

  <script>
    function fetchValue(id, endpoint, suffix) {
      // Refresh every 5 seconds to avoid overloading the controller
      setInterval(() => {
        fetch(endpoint)
          .then(r => r.text())
          .then(val => {
            const el = document.getElementById(id);
            if (val === "N/A") el.innerText = val;
            else el.innerText = val + suffix;
          })
          .catch(err => console.error('Error fetch:', err));
      }, 5000);
    }
    
    fetch('/status').then(r => r.text()).then(t => document.getElementById('statusText').innerText = t);
    
    fetchValue("temperature", "/temperature", " °C");
    fetchValue("humidity", "/humidity", "%");
    fetchValue("pressure", "/pressure", " mmHg");
    fetchValue("altitude", "/altitude", " m");
    fetchValue("bmpTemperature", "/bmpTemperature", " °C"); 
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

/************** Support functions **************/
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

void setup()
{
  Serial.begin(SERIAL_BAUDRATE);

  // forcefully disable any sleep mode; the modem is always in active listening mode
  WiFi.setSleepMode(WIFI_NONE_SLEEP);

  if (!WiFi.config(staticIP, gateway, subnet, dns))
  {
    Serial.println("Failed to configure WiFi!");
  }

  WiFi.begin(ssid, password);
  Serial.println("Connecting to WiFi");

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(CONNECTION_DELAY);
    Serial.print(".");
  }

  WiFi.hostname(DEVICE_NAME);

  Serial.println("\nConnected. IP: " + WiFi.localIP().toString());
  Serial.println("The modem is always on (constant power supply)");

#ifdef DEBUG_MODE
  dhtAvailable = true;
  bmpAvailable = true;
  Serial.println("DEBUG MODE: use mock data");
#else
  dht.begin();
  setupBmp180();
  dhtAvailable = !isnan(dht.readTemperature());
#endif

  // endpoints
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
  server.on("/status", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(HTTP_STATUS_OK, "text/plain", "DHT: " + String(dhtAvailable ? "OK" : "FAIL") + ", BMP: " + String(bmpAvailable ? "OK" : "FAIL")); });

  server.begin();
  Serial.println("Web server started");
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
#endif

    Serial.printf("Update: T=%.1f H=%.1f P=%.1f A=%.1f BmpT=%.1f\n",
                  temperature, humidity, pressure, altitude, bmpTemperature);
  }
}