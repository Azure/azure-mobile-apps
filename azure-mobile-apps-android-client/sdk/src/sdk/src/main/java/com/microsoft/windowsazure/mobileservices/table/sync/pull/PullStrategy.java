package com.microsoft.windowsazure.mobileservices.table.sync.pull;

import com.google.gson.JsonArray;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemColumns;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;

/**
 * Created by marianosanchez on 11/3/14.
 */
public class PullStrategy {

    /**
     * Default number of records.
     *
     * @see Query#top(int)
     */
    static final int defaultTop = 50;

    /**
     * Limit the maximum number of records.
     *
     * @see Query#top(int)
     */
    static final int maxTop = 1000;

    Query query;
    MobileServiceJsonTable table;
    int totalRead; // used to track how many we have read so far since the last delta

    public PullStrategy(Query query, MobileServiceJsonTable table) {

        this.query = query;
        this.table = table;
    }

    public void initialize() {

        query.includeDeleted();
        query.removeInlineCount();
        query.removeProjection();

        if (this.query.getTop() == 0) {
            this.query.top(defaultTop);
        } else {
            this.query.top(Math.min(this.query.getTop(), maxTop));
        }

        if (query.getOrderBy().size() == 0) {
            this.query.orderBy(MobileServiceSystemColumns.Id, QueryOrder.Ascending);
        }

        if (this.query.getSkip() < 0) {
            this.query.skip(0);
        }
    }

    public void onResultsProcessed(JsonArray elements) {
    }

    public boolean moveToNextPage(int lastElementCount) {

        totalRead += lastElementCount;

        if (lastElementCount == 0)
            return false;


        this.query.skip(totalRead);

        return true;
    }

    public Query getLastQuery() {
        return this.query;
    }
}
