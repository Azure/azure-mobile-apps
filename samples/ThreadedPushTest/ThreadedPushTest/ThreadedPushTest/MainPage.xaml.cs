// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.SQLiteStore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ThreadedPushTest
{
    public class LogItem
    {
        public string Message { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            startButton.Clicked += StartButton_Clicked;
        }

        /// <summary>
        /// The enabled state of the controls as a bindable property
        /// </summary>
        private bool _controlsAreEnabled = true;
        public bool ControlsAreEnabled
        {
            get => _controlsAreEnabled;
            set { _controlsAreEnabled = value; OnPropertyChanged(nameof(ControlsAreEnabled)); }
        }

        /// <summary>
        /// The log messages as a bindable property
        /// </summary>
        private string _logMessages = "";
        public string LogMessages
        {
            get => _logMessages;
            set { _logMessages = value; OnPropertyChanged(nameof(LogMessages)); }
        }

        /// <summary>
        /// Helper method to execute something in the context of the UI.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        private void UI(Action func)
            => MainThread.BeginInvokeOnMainThread(func);

        /// <summary>
        /// Adds to the logger.
        /// </summary>
        /// <param name="message">the message to log</param>
        private void AddToLog(string message)
        {
            Console.WriteLine($">>>LOG>>> {message}");
            UI(() => { LogMessages = $"{LogMessages}{message}\r\n"; });
        }

        /// <summary>
        /// Clears the log messages
        /// </summary>
        private void ClearLog()
            => UI(() => { LogMessages = ""; });

        /// <summary>
        /// Enable or disable all the data entry controls.
        /// </summary>
        /// <param name="isEnabled">true to enable, false to disable</param>
        private void EnableAllControls(bool isEnabled)
            => UI(() => { ControlsAreEnabled = isEnabled; });

        /// <summary>
        /// The number of items to create.
        /// </summary>
        public int ItemsCount
        {
            get => int.Parse(itemsEntry.Text);
        }

        /// <summary>
        /// The number of threads to execute the push on.
        /// </summary>
        public int ThreadCount
        {
            get => int.Parse(threadsEntry.Text);
        }

        /// <summary>
        /// Our central event handler.
        /// </summary>
        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            EnableAllControls(false);
            ClearLog();
            AddToLog($"Test run started at {DateTimeOffset.Now}");

            await RunTest();

            AddToLog($"Test run finished at {DateTimeOffset.Now}");
            EnableAllControls(true);
        }

        private async Task RunTest()
        {
            string dbFile = $"{FileSystem.CacheDirectory}/offline.db";
            AddToLog($"Deleting old database file if it exists");
            if (File.Exists(dbFile))
            {
                File.Delete(dbFile);
            }

            AddToLog($"Setting up service client");
            var store = new OfflineSQLiteStore($"file:{dbFile}?mode=rwc");
            store.DefineTable<Entity>();
            var options = new DatasyncClientOptions
            {
                //HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() },
                OfflineStore = store,
                ParallelOperations = ThreadCount
            };
            var client = new DatasyncClient(Constants.Endpoint, options);
            await client.InitializeOfflineStoreAsync();
            var offlineTable = client.GetOfflineTable<Entity>();

            AddToLog($"Adding {ItemsCount} items to the offline store");
            DateTimeOffset startTime = DateTimeOffset.Now;
            List<string> entities = new List<string>();
            for (int itemCounter = 0; itemCounter < ItemsCount; itemCounter++)
            {
                var entity = new Entity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    BoolProp = true,
                    DoubleProp = itemCounter,
                    IntProp = itemCounter,
                    StringProp = $"Item #{itemCounter}",
                    GuidProp = Guid.NewGuid(),
                    TimestampProp = DateTimeOffset.UtcNow
                };
                entities.Add(entity.Id);
                await offlineTable.InsertItemAsync(entity);
            }
            double ms = (DateTimeOffset.Now - startTime).TotalMilliseconds;
            AddToLog($"Adding to offline store took {ms}ms");

            AddToLog($"Pushing changes with {ThreadCount} threads");
            startTime = DateTimeOffset.Now;
            await offlineTable.PushItemsAsync();
            ms = (DateTimeOffset.Now - startTime).TotalMilliseconds;
            AddToLog($"Pushing to remote store took {ms}ms");

            AddToLog($"Removing items from offline store");
            foreach (var entityId in entities)
            {
                var entity = await offlineTable.GetItemAsync(entityId);
                await offlineTable.DeleteItemAsync(entity);
            }
            ms = (DateTimeOffset.Now - startTime).TotalMilliseconds;
            AddToLog($"Deleting from offline store took {ms}ms");

            AddToLog($"Pushing changes with {ThreadCount} threads");
            startTime = DateTimeOffset.Now;
            await offlineTable.PushItemsAsync();
            ms = (DateTimeOffset.Now - startTime).TotalMilliseconds;
            AddToLog($"Pushing to remote store took {ms}ms");
        }
    }
}
