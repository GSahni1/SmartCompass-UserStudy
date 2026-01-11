package com.PersuasiveComputing.SmartComp; // <-- MAKE SURE THIS MATCHES YOURS!

import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import com.unity3d.player.UnityPlayer;

public class StepDetectorListener implements SensorEventListener {
    private String unityObjectName = "";

    public void setUnityObjectName(String name) {
        this.unityObjectName = name;
    }

    public void register(Object sensorManagerService, Object stepSensor) {
        SensorManager sensorManager = (SensorManager) sensorManagerService;
        Sensor sensor = (Sensor) stepSensor;
        sensorManager.registerListener(this, sensor, SensorManager.SENSOR_DELAY_NORMAL);
    }

    @Override
    public void onSensorChanged(SensorEvent event) {
        if (event.sensor.getType() == Sensor.TYPE_STEP_COUNTER) {
            int totalSteps = (int) event.values[0];
            UnityPlayer.UnitySendMessage(unityObjectName, "OnStepCounterDataReceived", String.valueOf(totalSteps));
        }
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {
        // Required, but not used.
    }
}