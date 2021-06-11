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
 * MobileServiceLocalStoreException.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.localstore;

public class MobileServiceLocalStoreException extends Exception {
    /**
     * UID used for serialization
     */
    private static final long serialVersionUID = 8665204682638151717L;

    /**
     * Creates a new MobileServiceLocalStoreException with a detail message
     *
     * @param detailMessage The detail message
     */
    public MobileServiceLocalStoreException(String detailMessage) {
        super(detailMessage);
    }

    /**
     * Creates a new MobileServiceLocalStoreException with a cause
     *
     * @param throwable The exception cause
     */
    public MobileServiceLocalStoreException(Throwable throwable) {
        super(throwable);
    }

    /**
     * Creates a new MobileServiceLocalStoreException with a detail message and
     * a cause
     *
     * @param detailMessage    The detail message
     * @param throwable The exception cause
     */
    public MobileServiceLocalStoreException(String detailMessage, Throwable throwable) {
        super(detailMessage, throwable);
    }
}