package com.example.meteoraandroid;

import android.os.Bundle;
import android.os.Handler;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import java.util.Map;

public class MainActivity extends AppCompatActivity {

    private TelemetryService telemetryService;
    private final Handler uiHandler = new Handler();
    private TextView temperatureView, humidityView, pressureView, altitudeView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        temperatureView = findViewById(R.id.temperature);
        humidityView = findViewById(R.id.humidity);
        pressureView = findViewById(R.id.pressure);
        altitudeView = findViewById(R.id.altitude);

        telemetryService = new TelemetryService(this, "192.168.1.111:8080");

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

        Toast.makeText(this, telemetryData.toString(), Toast.LENGTH_SHORT).show();

        temperatureView.setText(String.format("%s C", telemetryData.getOrDefault("/temperature", "--.--")));
        humidityView.setText(String.format("%s %%", telemetryData.getOrDefault("/humidity", "--")));
        pressureView.setText(String.format("%s kPa", telemetryData.getOrDefault("/pressure", "---")));
        altitudeView.setText(String.format("%s m", telemetryData.getOrDefault("/altitude", "---")));
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        telemetryService.stopTelemetryUpdates();
        uiHandler.removeCallbacksAndMessages(null);
    }
}