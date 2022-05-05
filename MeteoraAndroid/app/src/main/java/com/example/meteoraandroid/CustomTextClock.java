package com.example.meteoraandroid;

import android.content.Context;
import android.os.Build;
import android.util.AttributeSet;
import android.widget.TextClock;

import androidx.annotation.RequiresApi;

public class CustomTextClock extends TextClock {

    public CustomTextClock(Context context) {
        super(context);
        //setLocaleDateFormat();
        this.setDesigningText();
    }

    public CustomTextClock(Context context, AttributeSet attrs) {
        super(context, attrs);
        //setLocaleDateFormat();
        this.setDesigningText();
    }

    public CustomTextClock(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        //setLocaleDateFormat();
        this.setDesigningText();
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public CustomTextClock(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        super(context, attrs, defStyleAttr, defStyleRes);
        //setLocaleDateFormat();
        this.setDesigningText();
    }

    private void setDesigningText() {
        // The default text is displayed when designing the interface.
        this.setText("11:30:00");
    }

    @Override
    protected void onAttachedToWindow() {
        try {
            super.onAttachedToWindow();
        } catch(Exception e)  {
        }
    }

    
}
