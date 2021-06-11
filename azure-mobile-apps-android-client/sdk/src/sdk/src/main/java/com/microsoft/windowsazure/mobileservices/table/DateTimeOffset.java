package com.microsoft.windowsazure.mobileservices.table;

import java.util.Date;

/**
 * Represents a point in time, typically expressed as a date and time of day
 */
public class DateTimeOffset extends Date {

    public DateTimeOffset(Date date) {
        this.setTime(date.getTime());
    }
}
