/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */

/**
 * LocalTableOperationProcessor.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStore;

/**
 * Processes a table operation against a local store.
 */
public class LocalTableOperationProcessor implements TableOperationVisitor<Void> {
    private MobileServiceLocalStore mStore;
    private JsonObject mItem;

    /**
     * Constructor for LocalTableOperationProcessor
     *
     * @param store           the local store
     */
    public LocalTableOperationProcessor(MobileServiceLocalStore store, JsonObject item) {
        this.mStore = store;
        this.mItem = item;
    }

    @Override
    public Void visit(InsertOperation operation) throws Throwable {
        this.mStore.upsert(operation.getTableName(), this.mItem, false);
        return null;
    }

    @Override
    public Void visit(UpdateOperation operation) throws Throwable {
        this.mStore.upsert(operation.getTableName(), this.mItem, false);
        return null;
    }

    @Override
    public Void visit(DeleteOperation operation) throws Throwable {
        if (this.mItem == null)
            operation.setItem(this.mStore.lookup(operation.getTableName(), operation.getItemId()));
        else
            operation.setItem(this.mItem);
        this.mStore.delete(operation.getTableName(), operation.getItemId());
        return null;
    }

    public JsonObject getItem() {
        return this.mItem;
    }

    public void setItem(JsonObject item) {
        this.mItem = item;
    }
}