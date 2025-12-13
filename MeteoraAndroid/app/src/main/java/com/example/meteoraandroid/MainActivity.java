package com.example.meteoraandroid;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import java.util.Map;
import com.github.mikephil.charting.charts.LineChart;
import com.github.mikephil.charting.data.Entry;
import com.github.mikephil.charting.data.LineData;
import com.github.mikephil.charting.data.LineDataSet;

public class MainActivity extends AppCompatActivity {

    private TelemetryService telemetryService;
    private final Handler uiHandler = new Handler();

    // Chart area
    private LineChart combinedChart;
    private LineDataSet tempDataSet;
    private LineDataSet humidityDataSet;
    private LineData lineData;

    private int timeIndex = 0;
    private static final int MAX_POINTS = 60;

    // UI elements
    private TextView temperatureView, humidityView, pressureView, altitudeView, batteryLevelView;
    private EditText ipAddressInput;
    private Button connectButton;

    // Settings storage
    private SharedPreferences prefs;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // TextView initialization
        temperatureView = findViewById(R.id.temperature);
        humidityView = findViewById(R.id.humidity);
        pressureView = findViewById(R.id.pressure);
        altitudeView = findViewById(R.id.altitude);
        batteryLevelView = findViewById(R.id.batteryLevel);

        // IP and connect button initialization
        ipAddressInput = findViewById(R.id.ipAddressInput);
        connectButton = findViewById(R.id.connectButton);

        combinedChart = findViewById(R.id.combinedChart);
        setupChart();

        // SharedPreferences to save entered IP
        prefs = getSharedPreferences("settings", MODE_PRIVATE);

        // Load saved IP
        String savedIp = prefs.getString("ip", "192.168.1.111:8080");
        ipAddressInput.setText(savedIp);

        telemetryService = new TelemetryService(this, savedIp);
        telemetryService.startTelemetryUpdates();
        startUiUpdates();

        // Connect button handler
        connectButton.setOnClickListener(v -> {
            String ip = ipAddressInput.getText().toString().trim();

            if (ip.isEmpty()) {
                Toast.makeText(this, "Enter IP:", Toast.LENGTH_SHORT).show();
                return;
            }

            // Stop old service
            telemetryService.stopTelemetryUpdates();

            // Save IP
            prefs.edit().putString("ip", ip).apply();

            // Create new service with the new IP
            telemetryService = new TelemetryService(this, ip);
            telemetryService.startTelemetryUpdates();

            Toast.makeText(this, "Connected to " + ip, Toast.LENGTH_SHORT).show();
        });
    }

    // Chart setup
    private void setupChart() {

        tempDataSet = new LineDataSet(null, "Temperature °C");
        tempDataSet.setColor(0xFFFF5722); // orange color
        tempDataSet.setDrawCircles(false);
        tempDataSet.setLineWidth(2f);

        humidityDataSet = new LineDataSet(null, "Humidity %");
        humidityDataSet.setColor(0xFF03A9F4); // blue color
        humidityDataSet.setDrawCircles(false);
        humidityDataSet.setLineWidth(2f);

        lineData = new LineData();
        lineData.addDataSet(tempDataSet);
        lineData.addDataSet(humidityDataSet);

        combinedChart.setData(lineData);

        combinedChart.getDescription().setEnabled(false);
        combinedChart.getLegend().setEnabled(true);

        combinedChart.getAxisRight().setEnabled(false);
        combinedChart.getXAxis().setDrawLabels(false);

        combinedChart.getAxisLeft().setTextColor(0xFF757575); // gray #757575
    }

    private void addChartEntry(float temperature, float humidity) {

        lineData.addEntry(new Entry(timeIndex, temperature), 0);
        lineData.addEntry(new Entry(timeIndex, humidity), 1);

        if (tempDataSet.getEntryCount() > MAX_POINTS) {
            tempDataSet.removeFirst();
            humidityDataSet.removeFirst();
            timeIndex--;
        }

        lineData.notifyDataChanged();
        combinedChart.notifyDataSetChanged();
        combinedChart.setVisibleXRangeMaximum(MAX_POINTS);
        combinedChart.moveViewToX(timeIndex);

        timeIndex++;
    }

    // Timer for UI updating
    private void startUiUpdates() {
        uiHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                updateUI();
                uiHandler.postDelayed(this, 1000);
            }
        }, 1000);
    }

    // Refresh telemetry data
    private void updateUI() {
        Map<String, String> telemetryData = telemetryService.getTelemetryData();

        String tempStr = telemetryData.get("/temperature");
        String humStr = telemetryData.get("/humidity");

        temperatureView.setText(
                String.format("%s °C", telemetryData.getOrDefault("/temperature", "--.--"))
        );

        humidityView.setText(
                String.format("%s %%", telemetryData.getOrDefault("/humidity", "--"))
        );

        pressureView.setText(
                String.format("%s kPa", telemetryData.getOrDefault("/pressure", "---"))
        );

        altitudeView.setText(
                String.format("%s m", telemetryData.getOrDefault("/altitude", "---"))
        );

        batteryLevelView.setText(
                String.format("%s %%", telemetryData.getOrDefault("/battery_status", "---"))
        );

        if (tempStr != null && humStr != null) {
            try {
                float temp = Float.parseFloat(tempStr);
                float hum = Float.parseFloat(humStr);
                addChartEntry(temp, hum);
            } catch (NumberFormatException ignored) {}
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        telemetryService.stopTelemetryUpdates();
        uiHandler.removeCallbacksAndMessages(null);
    }
}