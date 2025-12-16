package com.example.meteoraandroid;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import java.util.HashMap;
import java.util.Map;

public class TelemetryService {
    private static final String TAG = "TelemetryService";
    private static final int READING_INTERVAL_MS = 1000;
    private final String baseUrl;
    private final Handler handler;
    private final RequestQueue requestQueue;

    private final Map<String, String> telemetryData = new HashMap<>();

    private final Runnable telemetryRunnable = new Runnable() {
        @Override
        public void run() {
            fetchTelemetryData();
            handler.postDelayed(this, READING_INTERVAL_MS);
        }
    };

    public TelemetryService(Context context, String ipAddress) {
        this.baseUrl = "http://" + ipAddress;
        this.requestQueue = Volley.newRequestQueue(context);
        this.handler = new Handler(Looper.getMainLooper());
    }

    public void startTelemetryUpdates() {
        handler.post(telemetryRunnable);
    }

    public void stopTelemetryUpdates() {
        handler.removeCallbacks(telemetryRunnable);
    }

    private void fetchTelemetryData() {
        String[] endpoints = {"/temperature", "/humidity", "/altitude", "/pressure", "/chargeLevel"};

        for (String endpoint : endpoints) {
            sendRequest(endpoint, 0); // Starting from scratch
        }
    }

    private void sendRequest(String endpoint, int attempt) {
        StringRequest request = new StringRequest(Request.Method.GET, baseUrl + endpoint,
                response -> {
                    telemetryData.put(endpoint, response);
                    Log.d(TAG, "Data from " + endpoint + ": " + response);
                },
                error -> {
                    Log.e(TAG, "Error fetching " + endpoint + ": " + error.toString());
                    // Retry in case of an error, if there are no more than 3 attempts.
                    if (attempt < 3) {
                        Log.d(TAG, "Retrying " + endpoint + "...");
                        sendRequest(endpoint, attempt + 1);
                    }
                });

        requestQueue.add(request);
    }

    public Map<String, String> getTelemetryData() {
        return telemetryData;
    }
}

