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
 * InsertOperation.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import com.google.gson.JsonObject;

import java.util.Date;

/**
 * Class representing an insert operation against remote table.
 */
public class InsertOperation extends AbstractTableOperation {

    /**
     * Constructor for InsertOperation
     *
     * @param tableName the table name
     * @param itemId    the item id
     */
    public InsertOperation(String tableName, String itemId) {
        super(tableName, itemId);
    }

    /**
     * Constructor for Insert Operation
     *
     * @param id
     * @param tableName
     * @param itemId
     * @param createdAt
     */
    public InsertOperation(String id, String tableName, String itemId, Date createdAt, MobileServiceTableOperationState state) {
        super(id, tableName, itemId, createdAt, state, null);
    }

    @Override
    public TableOperationKind getKind() {
        return TableOperationKind.Insert;
    }

    @Override
    public <T> T accept(TableOperationVisitor<T> visitor) throws Throwable {
        return visitor.visit(this);
    }
}