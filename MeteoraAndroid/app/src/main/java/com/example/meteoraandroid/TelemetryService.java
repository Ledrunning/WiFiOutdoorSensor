package com.example.meteoraandroid;

import android.content.Context;
import android.os.Handler;
import android.util.Log;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
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
        this.handler = new Handler();
    }

    public void startTelemetryUpdates() {
        handler.post(telemetryRunnable);
    }

    public void stopTelemetryUpdates() {
        handler.removeCallbacks(telemetryRunnable);
    }

    private void fetchTelemetryData() {
        String[] endpoints = {"/temperature", "/humidity", "/altitude", "/pressure", "/battery_status"};

        for (String endpoint : endpoints) {
            StringRequest request = new StringRequest(Request.Method.GET, baseUrl + endpoint,
                    response -> {
                        telemetryData.put(endpoint, response);
                        Log.d(TAG, "Data from " + endpoint + ": " + response);
                    },
                    new Response.ErrorListener() {
                        @Override
                        public void onErrorResponse(VolleyError error) {
                            Log.e(TAG, "Error fetching " + endpoint + ": " + error.toString());
                        }
                    });

            requestQueue.add(request);
        }
    }

    public Map<String, String> getTelemetryData() {
        return telemetryData;
    }
}

