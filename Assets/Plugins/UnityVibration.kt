package com.example.unityvibration

import android.content.Context
import android.os.Vibrator
import android.os.VibrationEffect
import android.os.Build
import com.unity3d.player.UnityPlayer

class UnityVibration {
    fun vibration(len: Long, amplitude: Int) {
        val context = UnityPlayer.currentActivity
        val vibrator = context.getSystemService(Context.VIBRATOR_SERVICE) as Vibrator

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val effect = VibrationEffect.createOneShot(len, amplitude)
            vibrator.vibrate(effect)
        } else {
            // Android 8.0 이전 버전에서는 세기 조절 불가
            vibrator.vibrate(len)
        }
    }
}
