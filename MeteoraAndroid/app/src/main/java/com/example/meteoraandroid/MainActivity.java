package com.example.meteoraandroid;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.view.View;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import java.util.Map;
import com.github.mikephil.charting.charts.LineChart;
import com.github.mikephil.charting.data.Entry;
import com.github.mikephil.charting.data.LineData;
import com.github.mikephil.charting.data.LineDataSet;
import com.google.android.material.appbar.MaterialToolbar;
import android.util.Log;

public class MainActivity extends AppCompatActivity {
    private static final String TAG = "MainActivity";

    private TelemetryService telemetryService;
    private final Handler uiHandler = new Handler(Looper.getMainLooper());

    // Chart area
    private LineChart combinedChart;
    private LineDataSet tempDataSet;
    private LineDataSet humidityDataSet;
    private LineData lineData;

    private int timeIndex = 0;
    private static final int MAX_POINTS = 60;
    private static final int UPDATE_UI_MS = 1000;

    // UI elements
    private TextView title, temperatureView, humidityView, pressureView, altitudeView, batteryLevelView;
    private View connectionStatusIcon;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        MaterialToolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        // TextView initialization
        title = findViewById(R.id.title);
        temperatureView = findViewById(R.id.temperature);
        humidityView = findViewById(R.id.humidity);
        pressureView = findViewById(R.id.pressure);
        altitudeView = findViewById(R.id.altitude);
        batteryLevelView = findViewById(R.id.batteryLevel);

        combinedChart = findViewById(R.id.combinedChart);
        setupChart();

        telemetryService = new TelemetryService(this, "192.168.1.125");

        // Delay before the first UI update
        uiHandler.postDelayed(() -> {
            telemetryService.startTelemetryUpdates();
            startUiUpdates();
        }, 500);
    }

    // Chart setup
    private void setupChart() {
        tempDataSet = new LineDataSet(null, "Temperature °C");
        tempDataSet.setColor(0xFFFF5722);
        tempDataSet.setDrawCircles(false);
        tempDataSet.setLineWidth(2f);

        humidityDataSet = new LineDataSet(null, "Humidity %");
        humidityDataSet.setColor(0xFF03A9F4);
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
        combinedChart.getAxisLeft().setTextColor(0xFF757575);
    }

    private void addChartEntry(float temperature, float humidity) {
        lineData.addEntry(new Entry(timeIndex, temperature), 0);
        lineData.addEntry(new Entry(timeIndex, humidity), 1);

        if (tempDataSet.getEntryCount() > MAX_POINTS) {
            tempDataSet.removeFirst();
            humidityDataSet.removeFirst();
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
                if (telemetryService != null) {
                    uiHandler.postDelayed(this, UPDATE_UI_MS);
                }
            }
        }, UPDATE_UI_MS);
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

        // Add String.format("%s %%", telemetryData.getOrDefault("/chargeLevel", "---"))
        // If use battery power
        batteryLevelView.setText(String.format("%s 5V", "DC Power"));

        updateConnectionStatus();

        // Update the graph only if the data is valid
        if (tempStr != null && !tempStr.equals("--.--") &&
                humStr != null && !humStr.equals("--")) {
            try {
                float temp = Float.parseFloat(tempStr);
                float hum = Float.parseFloat(humStr);
                addChartEntry(temp, hum);
            } catch (NumberFormatException e) {
                Log.e(TAG, "Parse error: " + e.getMessage());
            }
        }
    }

    private void updateConnectionStatus() {
        if (title == null) {
            Log.e(TAG, "title TextView is NULL!");
            return;
        }

        boolean isConnected = telemetryService.isConnected();

        if (isConnected) {
            title.setText("Meteora 🟢");
            Log.d(TAG, "🟢 Status: CONNECTED");
        } else {
            title.setText("Meteora 🔴");
            Log.e(TAG, "🔴 Status: DISCONNECTED");
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (telemetryService != null) {
            telemetryService.stopTelemetryUpdates();
        }
        uiHandler.removeCallbacksAndMessages(null);
    }

    @Override
    protected void onPause() {
        super.onPause();
        Log.d(TAG, "Activity paused");
    }

    @Override
    protected void onResume() {
        super.onResume();
        Log.d(TAG, "Activity resumed");
    }
}