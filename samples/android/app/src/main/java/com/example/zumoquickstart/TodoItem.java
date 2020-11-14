package com.example.zumoquickstart;

import com.google.gson.annotations.SerializedName;

/**
 * Model representing an item in a Todo List.
 */
public class TodoItem {
    @SerializedName("text") private String mText;
    @SerializedName("id") private String mId;
    @SerializedName("complete") private boolean mComplete;

    public TodoItem() {

    }

    public TodoItem(String text, String id) {
        this.setText(text);
        this.setId(id);
    }

    @Override
    public String toString() {
        return getText();
    }

    @Override
    public boolean equals(Object o) {
        return o instanceof TodoItem && ((TodoItem) o).mId == mId;
    }

    public String getText() {
        return mText;
    }

    public void setText(String mText) {
        this.mText = mText;
    }

    public String getId() {
        return mId;
    }

    public void setId(String mId) {
        this.mId = mId;
    }

    public boolean isComplete() {
        return mComplete;
    }

    public void setComplete(boolean mComplete) {
        this.mComplete = mComplete;
    }
}
