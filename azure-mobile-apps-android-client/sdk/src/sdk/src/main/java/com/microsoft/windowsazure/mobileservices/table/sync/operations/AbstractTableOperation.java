package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import com.google.gson.JsonObject;

import java.util.Date;
import java.util.UUID;

public abstract class AbstractTableOperation implements TableOperation {
    private String mId;
    private String mTableName;
    private String mItemId;
    private Date mCreatedAt;
    private MobileServiceTableOperationState mOperationState;
    private JsonObject mItem;

    /**
     * Constructor for AbstractTableOperation
     *
     * @param id        the operation id
     * @param tableName the table name
     * @param itemId    the item id
     * @param createdAt the creation time
     */
    public AbstractTableOperation(String id, String tableName, String itemId, Date createdAt, MobileServiceTableOperationState state, JsonObject item) {
        this.mId = id;
        this.mTableName = tableName;
        this.mItemId = itemId;
        this.mCreatedAt = createdAt;
        this.mOperationState = state;
        this.mItem = item;
    }

    /**
     * Constructor for AbstractTableOperation
     *
     * @param tableName the table name
     * @param itemId    the item id
     */
    public AbstractTableOperation(String tableName, String itemId) {
        this(UUID.randomUUID().toString(), tableName, itemId, new Date(), MobileServiceTableOperationState.Pending, null);
    }

    @Override
    public String getId() {
        return this.mId;
    }

    @Override
    public String getTableName() {
        return this.mTableName;
    }

    @Override
    public String getItemId() {
        return this.mItemId;
    }

    @Override
    public Date getCreatedAt() {
        return this.mCreatedAt;
    }

    /**
     * Gets the operation state
     *
     * @return The operation state
     */
    @Override
    public MobileServiceTableOperationState getOperationState() {
        return this.mOperationState;
    }

    /**
     * Sets the operation state
     *
     * @param operationState the Operation State
     */
    @Override
    public void setOperationState(MobileServiceTableOperationState operationState) {
        this.mOperationState = operationState;
    }

    @Override
    public JsonObject getItem() {
        return this.mItem;
    }

    @Override
    public void setItem(JsonObject item) { this.mItem = item; }

    @Override
    public String getTableItemId() { return this.getTableName() + '/' + this.getItemId(); }
}