package com.example.meteoraandroid;

import android.os.Bundle;
import android.os.Handler;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import java.util.Map;

public class MainActivity extends AppCompatActivity {

    private TelemetryService telemetryService;
    private Handler uiHandler;
    private TextView temperatureView, humidityView, pressureView, altitudeView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        temperatureView = findViewById(R.id.temperature);
        humidityView = findViewById(R.id.humidity);
        pressureView = findViewById(R.id.pressure);
        altitudeView = findViewById(R.id.altitude);

        telemetryService = new TelemetryService(this, "192.168.0.101:8080");
        uiHandler = new Handler();

        telemetryService.startTelemetryUpdates();
        startUiUpdates();
    }

    private void startUiUpdates() {
        uiHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                updateUI();
                uiHandler.postDelayed(this, 1000);
            }
        }, 1000);
    }

    private void updateUI() {
        Map<String, String> telemetryData = telemetryService.getTelemetryData();

        if (telemetryData.containsKey("/temperature")) {
            temperatureView.setText(String.format("%s C", telemetryData.get("/temperature")));
        }
        if (telemetryData.containsKey("/humidity")) {
            humidityView.setText(String.format("%s %%", telemetryData.get("/humidity")));
        }
        if (telemetryData.containsKey("/pressure")) {
            pressureView.setText(String.format("%s kPa", telemetryData.get("/pressure")));
        }
        if (telemetryData.containsKey("/altitude")) {
            altitudeView.setText(String.format("%s m", telemetryData.get("/altitude")));
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        telemetryService.stopTelemetryUpdates();
        uiHandler.removeCallbacksAndMessages(null);
    }
}