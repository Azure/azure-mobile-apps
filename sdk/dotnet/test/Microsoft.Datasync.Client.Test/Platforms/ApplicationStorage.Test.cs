// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Platforms;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Platforms
{
    [ExcludeFromCodeCoverage]
    public class ApplicationStorage_Test : BaseTest
    {
        [Fact]
        public void Storage_CanCreateDefaultContainer()
        {
            var storage = new ApplicationStorage();

            // Storage an item within the storage.
            const string key = "appstorage-test-key1";
            var value = Guid.NewGuid().ToString();

            // Make sure we clean up after a prior run
            storage.RemoveValue(key);
            Assert.False(storage.TryGetValue(key, out string value0));
            Assert.Null(value0);

            storage.SetValue(key, value);
            Assert.True(storage.TryGetValue(key, out string value1));
            Assert.Equal(value, value1);

            // Create a new instance of the storage
            var newStore = new ApplicationStorage();

            Assert.True(newStore.TryGetValue(key, out string value2));
            Assert.Equal(value, value2);

            // Update the key
            var newValue = Guid.NewGuid().ToString();
            newStore.SetValue(key, newValue);

            Assert.True(newStore.TryGetValue(key, out string value3));
            Assert.Equal(newValue, value3);

            // Remove the key
            newStore.RemoveValue(key);
            Assert.False(newStore.TryGetValue(key, out string value4));
            Assert.Null(value4);

            // Create yet another new store and repeat check
            var checkStore = new ApplicationStorage();
            Assert.False(checkStore.TryGetValue(key, out string value5));
            Assert.Null(value5);
        }

        [Fact]
        public void Storage_CanCreateNonDefaultContainer()
        {
            const string containerName = "test2-container-name";
            var storage = new ApplicationStorage(containerName);

            // Storage an item within the storage.
            const string key = "appstorage-test-key2";
            var value = Guid.NewGuid().ToString();

            // Make sure we clean up after a prior run
            storage.RemoveValue(key);
            Assert.False(storage.TryGetValue(key, out string value0));
            Assert.Null(value0);

            storage.SetValue(key, value);
            Assert.True(storage.TryGetValue(key, out string value1));
            Assert.Equal(value, value1);

            // Create a new instance of the storage
            var newStore = new ApplicationStorage(containerName);

            Assert.True(newStore.TryGetValue(key, out string value2));
            Assert.Equal(value, value2);

            // Update the key
            var newValue = Guid.NewGuid().ToString();
            newStore.SetValue(key, newValue);

            Assert.True(newStore.TryGetValue(key, out string value3));
            Assert.Equal(newValue, value3);

            // Remove the key
            newStore.RemoveValue(key);
            Assert.False(newStore.TryGetValue(key, out string value4));
            Assert.Null(value4);

            // Create yet another new store and repeat check
            var checkStore = new ApplicationStorage(containerName);
            Assert.False(checkStore.TryGetValue(key, out string value5));
            Assert.Null(value5);
        }

        [Fact]
        public void Storage_ContainersAreSeparate()
        {
            const string containerName = "test3-container-name";
            var storage = new ApplicationStorage(containerName);

            // Storage an item within the storage.
            const string key = "appstorage-test-key3";
            var value = Guid.NewGuid().ToString();

            // Make sure we clean up after a prior run
            storage.RemoveValue(key);
            Assert.False(storage.TryGetValue(key, out string value0));
            Assert.Null(value0);

            storage.SetValue(key, value);
            Assert.True(storage.TryGetValue(key, out string value1));
            Assert.Equal(value, value1);

            // Create a new instance of the storage
            var newStore = new ApplicationStorage();

            Assert.False(newStore.TryGetValue(key, out string value2));
            Assert.Null(value2);
        }

        [Fact]
        public void Storage_CanClearEmptyStorage()
        {
            const string containerName = "test4-container-name";
            var storage = new ApplicationStorage(containerName);
            storage.ClearValues();
        }

        [Fact]
        public void Storage_CanClearFilledStorage()
        {
            var storage = new ApplicationStorage();

            // Storage an item within the storage.
            const string key = "appstorage-test-key1";
            var value = Guid.NewGuid().ToString();

            // Make sure we clean up after a prior run
            storage.RemoveValue(key);
            Assert.False(storage.TryGetValue(key, out string value0));
            Assert.Null(value0);

            // Make sure we can clear the values.
            storage.ClearValues();
        }
    }
}
