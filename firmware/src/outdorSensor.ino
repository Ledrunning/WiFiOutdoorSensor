/*********
 Outdor Wi Fi temperature sensor
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
#define DHTPIN 16 // Digital pin connected to the DHT sensor
#define CONNECTION_DELAY 1000
#define HTTP_STATUS_OK 200

// Uncomment the type of sensor in use:
//#define DHTTYPE    DHT11     // DHT 11
#define DHTTYPE DHT22 // DHT 22 (AM2302)
//#define DHTTYPE    DHT21     // DHT 21 (AM2301)

// Replace with your network credentials
const char *ssid = "TP-Link_AF98";
const char *password = "96767962";
const uint8_t PORT = 80;
const short BMP_CONNECTION_ATTEMPT = 5;

// Updates DHT readings every 10 seconds
const long interval = 10000;

DHT dht(DHTPIN, DHTTYPE);

Adafruit_BMP085 bmp;

// current temperature & humidity, updated in loop()
float temperature = 0.0;
float humidity = 0.0;

int pressure = 0, altitude = 0;

// Create AsyncWebServer object on port 80
AsyncWebServer server(PORT);

// Generally, you should use "unsigned long" for variables that hold time
// The value will quickly become too large for an int to store
unsigned long previousMillis = 0; // will store last time DHT was updated

const char index_html[] PROGMEM = R"rawliteral(
<!DOCTYPE HTML><html>
<head>
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.7.2/css/all.css" integrity="sha384-fnmOCqbTlWIlj8LyTjo7mOUStjsKC4pOpQbqyi7RrhN7udi9RwhKkMHpvLbHG9Sr" crossorigin="anonymous">
  <style>
    html {
     font-family: Arial;
     display: inline-block;
     margin: 0px auto;
     text-align: center;
    }
    h2 { font-size: 3.0rem; }
    p { font-size: 3.0rem; }
    .units { font-size: 1.2rem; }
    .dht-labels{
      font-size: 1.5rem;
      vertical-align:middle;
      padding-bottom: 15px;
    }
    .bmp-labels{
      font-size: 1.5rem;
      vertical-align:middle;
      padding-bottom: 15px;
    }
  </style>
</head>
<body>
  <h2>Outdoor meteo probe</h2>
  <p>
    <i class="fas fa-thermometer-half" style="color:#059e8a;"></i> 
    <span class="dht-labels">Temperature</span> 
    <span id="temperature">23.4</span>
    <sup class="units">&deg;C</sup>
  </p>

  <p>
    <i class="fas fa-tint" style="color:#00add6;"></i> 
    <span class="dht-labels">Humidity</span>
    <span id="humidity">70.2</span>
    <sup class="units">%</sup>
  </p>

   <p>
    <i class="fas fa-weight" style="color:#059e8a;"></i> 
    <span class="bmp-labels">Pressure</span>
    <span id="pressure">760</span>
    <sup class="units">мм рт</sup>
  </p>

   <p>
      <i class="fas fa-arrows-alt-v" style="color:#00add6;"></i>
      <span class="bmp-labels">Altitude</span>
      <span id="altitude">150</span>
      <sup class="units">m</sup>
  </p>
</body>

<script>
setInterval(function ( ) {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("temperature").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/temperature", true);
  xhttp.send();
}, 10000 ) ;

setInterval(function ( ) {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("humidity").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/humidity", true);
  xhttp.send();
}, 10000 ) ;

setInterval(function ( ) {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("pressure").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/pressure", true);
  xhttp.send();
}, 10000 ) ;

 setInterval(function ( ) {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("altitude").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/altitude", true);
  xhttp.send();
}, 10000 ) ;

</script>
</html>)rawliteral";

// Replaces placeholder with DHT values
String processor(const String &var)
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
  return String();
}

void setup()
{
  // Serial port for debugging purposes
  Serial.begin(SERIAL_BAUDRATE);
  dht.begin();

  // Connect to Wi-Fi
  WiFi.begin(ssid, password);
  Serial.println("Connecting to WiFi");
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(CONNECTION_DELAY);
    Serial.println(".");
  }

  // Print ESP8266 Local IP Address
  Serial.println(WiFi.localIP());

  //SetupBmp180();

  // Route for root / web page
  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(HTTP_STATUS_OK, "text/html", index_html, processor);
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
  // Start server
  server.begin();
}

void SetupBmp180()
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
void readPressure(long &altitude, long &pressure)
{
  pressure = bmp.readPressure();
  pressure = pressure / 133.3; // frop Pa to мм рт
  altitude = bmp.readAltitude();
}

void loop()
{

  //readPressure(altitude, pressure);

  unsigned long currentMillis = millis();

  if (currentMillis - previousMillis >= interval)
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
  }
}