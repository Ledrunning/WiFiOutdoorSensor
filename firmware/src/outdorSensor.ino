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
const long DHT_READ_INTERVAL = 2000;

DHT dht(DHTPIN, DHTTYPE);

Adafruit_BMP085 bmp;

// current temperature & humidity, updated in loop()
float temperature = 0.0;
float humidity = 0.0;
float pressure = 0;
float altitude = 0;
int chargeLevel = 0;

// Create AsyncWebServer object on port 80
AsyncWebServer server(PORT);

// Generally, you should use "unsigned long" for variables that hold time
// The value will quickly become too large for an int to store
unsigned long previousMillis = 0; // will store last time DHT was updated

float fVoltageMatrix[22][2] = {
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
<!DOCTYPE HTML><html>
<head>
  <meta charset="utf-8">
  <style>
    html {
     font-family: Arial;
     display: inline-block;
     margin: 0px auto;
     text-align: center;
	 background-color: white;
    }
    h2 { 
		font-size: 3.0rem; 
		color: #696969;
	}
	
    p { font-size: 2.0rem; }
	
    .units { 
		font-size: 1.2rem; 
		color: green;
	}
    .all-labels{
      font-size: 1.5rem;
      vertical-align:middle;
      padding-bottom: 15px;
	  color: #696969;
    }
	.data-labels {
		color: green;
	}
  </style>
</head>
<body>
  <h2>Метеора 1.0 ╔═(███)═╗</h2>
  <hr />
  <p>
    <span class="all-labels">Температура</span> 
    <span id="temperature" class="data-labels">%TEMPERATURE%</span>
    <sup class="units">&deg;C</sup>
  </p>
  <p>
    <span class="all-labels">Влажность</span>
    <span id="humidity" class="data-labels">%HUMIDITY%</span>
    <sup class="units">%</sup>
  </p>
  <p>
    <span class="all-labels">Давление</span>
    <span id="pressure" class="data-labels">%PRESSURE%</span>
    <sup class="units">мм рс</sup>
  </p>
  <p>
      <span class="all-labels">Высота</span>
      <span id="altitude" class="data-labels">%ALTITUDE%</span>
      <sup class="units">м</sup>
  </p>
  <p>
      <span class="all-labels">Уровень батареи</span>
      <span id="chargeLevel" class="data-labels">%BATTERY_STATUS%</span>
      <sup class="units">%</sup>
  </p>
</body>
<script>
setInterval(function () {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("temperature").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/temperature", true);
  xhttp.send();
}, 10000 ) ;

setInterval(function () {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("humidity").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/humidity", true);
  xhttp.send();
}, 10000 ) ;

setInterval(function () {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("pressure").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/pressure", true);
  xhttp.send();
}, 10000 );

 setInterval(function () {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("altitude").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/altitude", true);
  xhttp.send();
}, 10000 );

 setInterval(function () {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("chargeLevel").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/battery_status", true);
  xhttp.send();
}, 10000 );
</script>
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

  // Connect to Wi-Fi
  WiFi.begin(ssid, password);
  Serial.println("Connecting to WiFi");
  while (WiFi.status() != WL_CONNECTED)
  {
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

  server.on("/battery_status", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/plain", String(chargeLevel).c_str());
  });

  // Start server
  server.begin();

  // 10 sec in sleep mode
  // NOTICE! Need to connect D0(WAKE)-GPIO16 Pin and RESET both!
  //ESP.deepSleep(sleepTimeS * 1000000);
}

void setupBmp180()
{
  int count = 0;
  // Start reading bmp 180 sensor
  while (!bmp.begin())
  {
    count++;

    if (count >= BMP_CONNECTION_ATTEMPT)
    {
      Serial.println("Could not find a valid BMP085 sensor, check wiring!");
      count = 0;
      break;
    }
  }
  count = 0;
}

// Read data from bmp 180 sensor
void readBmp180(float &altitude, float &pressure)
{
  pressure = bmp.readPressure();
  pressure = pressure / 133.3; // from Pa to мм рт
  altitude = bmp.readAltitude();
}

void readBatteryCharge()
{

  int nVoltageRaw = analogRead(A0);
  float fVoltage = (float)nVoltageRaw * ADC_DELTA;

  chargeLevel = 100;

  for (int i = 20; i >= 0; i--)
  {
    if (fVoltageMatrix[i][0] >= fVoltage)
    {
      chargeLevel = fVoltageMatrix[i + 1][1];
      break;
    }
  }
}

void loop()
{

  unsigned long currentMillis = millis();

  if (currentMillis - previousMillis >= DHT_READ_INTERVAL)
  {
    // save the last time you updated the DHT values
    previousMillis = currentMillis; 

    // Read temperature as Celsius (the default)
    float newTemperature = dht.readTemperature();

    // Read temperature as Fahrenheit (isFahrenheit = true)
    //float newT = dht.readTemperature(true);

    // if temperature read failed, don't change t value
    if (isnan(newTemperature))
    {
      Serial.println("Failed to read from DHT sensor!");
    }
    else
    {
      temperature = newTemperature;
      Serial.println(temperature);
    }

    // Read Humidity
    float newHumidity = dht.readHumidity();

    // if humidity read failed, don't change h value
    if (isnan(newHumidity))
    {
      Serial.println("Failed to read from DHT sensor!");
    }
    else
    {
      humidity = newHumidity;
      Serial.println(humidity);
    }

     readBmp180(altitude, pressure);
    Serial.println("Altitude:");
    Serial.println(altitude);
    Serial.println("Pressure:");
    Serial.println(pressure);

    readBatteryCharge();
    Serial.println("Battery level:");
    Serial.println(chargeLevel);
  }
}