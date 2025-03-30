/*********
 Outdoor Wi Fi temperature sensor
*********/

// Import required libraries
#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <Hash.h>
#include <ESPAsyncTCP.h>
#include <ESPAsyncWebServer.h>
#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <Adafruit_BMP085.h>

#define SERIAL_BAUDRATE 115200
#define DHTPIN 14 // Digital pin connected to the DHT sensor
#define CONNECTION_DELAY 1000
#define HTTP_STATUS_OK 200

// Uncomment the type of sensor in use:
//#define DHTTYPE    DHT11     // DHT 11
#define DHTTYPE DHT22 // DHT 22 (AM2302)
//#define DHTTYPE    DHT21     // DHT 21 (AM2301)

// Replace with your network credentials
const char *ssid = "TP-LINK_91FA";
const char *password = "68783709";
const uint8_t PORT = 80;
const short BMP_CONNECTION_ATTEMPT = 5;
const double ADC_DELTA = 0.00486;

//Static IP address configuration
IPAddress staticIP(192, 168, 0, 101); //ESP static ip
IPAddress gateway(192, 168, 0, 1);    //IP Address of your WiFi Router (Gateway)
IPAddress subnet(255, 255, 255, 0);   //Subnet mask
IPAddress dns(8, 8, 8, 8);            //DNS

const char *DEVICE_NAME = "weStation";

// Time to sleep (in seconds):
const int SLEEP_TIME_MS = 10;

// Updates DHT readings every 10 seconds
const long DHT_READ_INTERVAL = 10000;

DHT dht(DHTPIN, DHTTYPE);

Adafruit_BMP085 bmp;

// current temperature & humidity, updated in loop()
float temperature = 0.0;
float humidity = 0.0;
float pressure = 0;
float altitude = 0;
float bmpTemperature = 0.0;
int chargeLevel = 0;

// Create AsyncWebServer object on port 80
AsyncWebServer server(PORT);

// Generally, you should use "unsigned long" for variables that hold time
// The value will quickly become too large for an int to store
unsigned long previousMillis = 0; // will store last time DHT was updated

void setup();
void setupBmp180();
void readBmp180(float &altitude, float &pressure);
void readBatteryCharge();
void loop();

float voltageMatrix[22][2] = {
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

const char index_html[] PROGMEM = R"rawliteral(
<!DOCTYPE HTML>
<html>
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css">
  <style>
    body {
      font-family: Arial, sans-serif;
      text-align: center;
      background-color: #f4f4f4;
      margin: 0;
      padding: 20px;
    }
    .container {
      max-width: 400px;
      background: white;
      padding: 20px;
      border-radius: 10px;
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
      margin: auto;
    }
    h2 {
      font-size: 24px;
      color: #333;
    }
    .sensor {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px 0;
      border-bottom: 1px solid #ddd;
      font-size: 18px;
    }
    .sensor:last-child {
      border-bottom: none;
    }
    .icon {
      font-size: 22px;
      color: #4CAF50;
      width: 30px;
    }
    .value {
      font-weight: bold;
      color: #333;
    }
  </style>
</head>
<body>
  <div class="container">
    <h2>Метеора 1.0</h2>
    <div class="sensor"><i class="fas fa-temperature-high icon"></i> Температура <span id="temperature" class="value">%TEMPERATURE% &deg;C</span></div>
    <div class="sensor"><i class="fas fa-tint icon"></i> Влажность <span id="humidity" class="value">%HUMIDITY%%</span></div>
    <div class="sensor"><i class="fas fa-tachometer-alt icon"></i> Давление <span id="pressure" class="value">%PRESSURE% мм рс</span></div>
    <div class="sensor"><i class="fas fa-mountain icon"></i> Высота <span id="altitude" class="value">%ALTITUDE% м</span></div>
    <div class="sensor"><i class="fas fa-thermometer icon"></i> Темп. BMP-180 <span id="bmpTemperature" class="value">%BMPTEMPERATURE% &deg;C</span></div>
    <div class="sensor"><i class="fas fa-battery-full icon"></i> Батарея <span id="chargeLevel" class="value">%BATTERY_STATUS%%</span></div>
  </div>

  <script>
    function updateData(id, endpoint, suffix = "") {
    setInterval(function () {
        var xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function() {
            if (this.readyState == 4 && this.status == 200) {
                document.getElementById(id).innerHTML = this.responseText + suffix;
            }
        };
        xhttp.open("GET", endpoint, true);
        xhttp.send();
     }, 10000);
    }
    updateData("temperature", "/temperature", " &deg;C");
    updateData("humidity", "/humidity", "%");
    updateData("pressure", "/pressure", " мм рс");
    updateData("altitude", "/altitude", " м");
    updateData("bmpTemperature", "/bmpTemperature", " &deg;C");
    updateData("chargeLevel", "/battery_status", "%");

  </script>
</body>
</html>)rawliteral";

// Replaces placeholder with sensor values
String getStringFromRoutings(const String &var)
{
  //Serial.println(var);
  if (var == "TEMPERATURE")
  {
    return String(temperature);
  }
  else if (var == "HUMIDITY")
  {
    return String(humidity);
  }
  else if (var == "PRESSURE")
  {
    return String(pressure);
  }
  else if (var == "ALTITUDE")
  {
    return String(altitude);
  }
  else if (var == "BMPTEMPERATURE") 
  {
    return String(bmpTemperature);
  }
  else if (var == "BATTERY_STATUS")
  {
    return String(chargeLevel);
  }
  return String();
}

void setup()
{
  // Serial port for debugging purposes
  Serial.begin(SERIAL_BAUDRATE);
  
  if(!WiFi.config(staticIP, gateway, subnet, dns)){
    Serial.println("STA Failed to configure");
  }

  // Connect to Wi-Fi
  WiFi.begin(ssid, password);
  Serial.println("Connecting to WiFi");

  while (WiFi.status() != WL_CONNECTED) {
    delay(CONNECTION_DELAY);
    Serial.println(".");
  }

  WiFi.hostname(DEVICE_NAME);

  dht.begin();

  // Print ESP8266 Local IP Address
  Serial.println(WiFi.localIP());
  Serial.println(WiFi.hostname());

  setupBmp180();

  // Route for root / web page
  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/html", index_html, getStringFromRoutings);
  });
  server.on("/temperature", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/plain", String(temperature).c_str());
  });
  server.on("/humidity", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/plain", String(humidity).c_str());
  });
  server.on("/pressure", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/plain", String(pressure).c_str());
  });
  server.on("/altitude", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/plain", String(altitude).c_str());
  });
  server.on("/bmpTemperature", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/plain", String(bmpTemperature).c_str());
  });
  server.on("/battery_status", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/plain", String(chargeLevel).c_str());
  });

  // Start server
  server.begin();

  // 10 sec in sleep mode
  // NOTICE! Need to connect D0(WAKE)-GPIO16 Pin and RESET both!
  //ESP.deepSleep(SLEEP_TIME_MS * 1000000);
}

void setupBmp180(){
  int count = 0;
  // Start reading bmp 180 sensor
  while (!bmp.begin()) {
    count++;

    if (count >= BMP_CONNECTION_ATTEMPT) {
      Serial.println("Could not find a valid BMP085 sensor, check wiring!");
      count = 0;
      break;
    }
  }
  count = 0;
}

// Read data from bmp 180 sensor
void readBmp180(float &altitude, float &pressure, float &bmpTemperature){
  pressure = bmp.readPressure();
  pressure = pressure / 133.3; // from Pa to мм рт
  altitude = bmp.readAltitude();
  bmpTemperature = bmp.readTemperature();
}

void readBatteryCharge(){

  int rawVoltage = analogRead(A0);
  float currentVoltage = (float)rawVoltage * ADC_DELTA;

  //chargeLevel = 100;

  for (int i = 20; i >= 0; i--){
    if (voltageMatrix[i][0] >= currentVoltage){
      chargeLevel = voltageMatrix[i + 1][1];
      break;
    }
  }
}

void loop(){

  unsigned long currentMillis = millis();

  if (currentMillis - previousMillis >= DHT_READ_INTERVAL){
    // save the last time you updated the DHT values
    previousMillis = currentMillis; 

    // Read temperature as Celsius (the default)
    float newTemperature = dht.readTemperature();

    // Read temperature as Fahrenheit (isFahrenheit = true)
    //float newT = dht.readTemperature(true);

    // if temperature read failed, don't change t value
    if (isnan(newTemperature)){
      Serial.println("Failed to read from DHT sensor!");
    }
    else{
      temperature = newTemperature;
      Serial.println(temperature);
    }

    // Read Humidity
    float newHumidity = dht.readHumidity();

    // if humidity read failed, don't change h value
    if (isnan(newHumidity)){
      Serial.println("Failed to read from DHT sensor!");
    }
    else{
      humidity = newHumidity;
      Serial.println(humidity);
    }

    readBmp180(altitude, pressure, bmpTemperature);
    Serial.println("Altitude:");
    Serial.println(altitude);
    Serial.println("Pressure:");
    Serial.println(pressure);
    Serial.println("Temperature from BMP:");
    Serial.println(bmpTemperature);

    readBatteryCharge();
    Serial.println("Battery level:");
    Serial.println(chargeLevel);
  }
}