package com.example.meteoraandroid;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;

import java.util.HashMap;
import java.util.Map;

public class TelemetryService {

    private static final String TAG = "TelemetryService";

    private static final int UPDATE_INTERVAL_MS = 3000;
    private static final int REQUEST_DELAY_MS = 300;
    private static final int ERROR_DELAY_MS = 200;
    private static final int TIMEOUT_MS = 8000;
    private static final int MIN_SUCCESS_FOR_CONNECTED = 3;

    private final String baseUrl;
    private final RequestQueue requestQueue;
    private final Handler handler = new Handler(Looper.getMainLooper());

    private boolean running = false;
    private int currentEndpointIndex = 0;

    private final Map<String, String> telemetryData = new HashMap<>();

    private int successCount = 0;
    private int errorCount = 0;

    private static final String[] ENDPOINTS = {
            "/temperature",
            "/humidity",
            "/pressure",
            "/altitude",
            //"/chargeLevel"
    };

    public TelemetryService(Context context, String ipPort) {
        this.baseUrl = "http://" + ipPort;
        this.requestQueue = Volley.newRequestQueue(context.getApplicationContext());
        initDefaults();
    }

    private void initDefaults() {
        telemetryData.put("/temperature", "--.--");
        telemetryData.put("/humidity", "--");
        telemetryData.put("/pressure", "---");
        telemetryData.put("/altitude", "---");
        // telemetryData.put("/chargeLevel", "---");
    }

    public void startTelemetryUpdates() {
        if (running) {
            Log.w(TAG, "Already running");
            return;
        }

        running = true;
        successCount = 0;
        errorCount = 0;

        Log.d(TAG, "Telemetry started");
        handler.post(updateRunnable);
    }

    public void stopTelemetryUpdates() {
        running = false;
        handler.removeCallbacks(updateRunnable);
        handler.removeCallbacksAndMessages(null);
        requestQueue.cancelAll(TAG);

        Log.d(TAG, "Telemetry stopped");
    }

    private final Runnable updateRunnable = new Runnable() {
        @Override
        public void run() {
            if (!running) return;

            fetchAllEndpointsSequentially();
            handler.postDelayed(this, UPDATE_INTERVAL_MS);
        }
    };

    private void fetchAllEndpointsSequentially() {
        currentEndpointIndex = 0;
        fetchNextEndpoint();
    }

    private void fetchNextEndpoint() {
        if (!running || currentEndpointIndex >= ENDPOINTS.length) {
            return;
        }

        String endpoint = ENDPOINTS[currentEndpointIndex];
        sendRequest(endpoint);
    }

    private void sendRequest(String endpoint) {
        String url = baseUrl + endpoint;

        StringRequest request = new StringRequest(
                Request.Method.GET,
                url,
                response -> {
                    // If success, save the response
                    telemetryData.put(endpoint, response.trim());
                    successCount++;
                    errorCount = 0;

                    Log.d(TAG, "✓ " + endpoint + " = " + response.trim() +
                            " (success: " + successCount + ")");

                    // next request
                    currentEndpointIndex++;
                    if (running) {
                        handler.postDelayed(this::fetchNextEndpoint, REQUEST_DELAY_MS);
                    }
                },
                error -> {
                    errorCount++;
                    successCount = 0;

                    String errorMsg = "unknown";
                    if (error.networkResponse != null) {
                        errorMsg = "HTTP " + error.networkResponse.statusCode;
                    } else if (error.getCause() != null) {
                        errorMsg = error.getCause().getMessage();
                    } else if (error.getMessage() != null) {
                        errorMsg = error.getMessage();
                    }

                    Log.e(TAG, "✗ " + endpoint + " : " + errorMsg +
                            " (errors: " + errorCount + ")");

                    // Next request even if there is an error (with less delay)
                    currentEndpointIndex++;
                    if (running) {
                        handler.postDelayed(this::fetchNextEndpoint, ERROR_DELAY_MS);
                    }
                }
        ) {
            @Override
            public Map<String, String> getHeaders() {
                Map<String, String> headers = new HashMap<>();
                headers.put("Connection", "close");
                return headers;
            }
        };

        request.setTag(TAG);
        request.setShouldCache(false);
        request.setRetryPolicy(new DefaultRetryPolicy(
                TIMEOUT_MS,
                0,
                1f
        ));

        requestQueue.add(request);
    }

    public Map<String, String> getTelemetryData() {
        return new HashMap<>(telemetryData);
    }

    public boolean isConnected() {
        boolean connected = (successCount >= MIN_SUCCESS_FOR_CONNECTED) && (errorCount == 0);

        Log.d(TAG, "isConnected() = " + connected +
                " (success: " + successCount + ", errors: " + errorCount + ")");

        return connected;
    }

    public int getSuccessCount() {
        return successCount;
    }

    public int getErrorCount() {
        return errorCount;
    }
}