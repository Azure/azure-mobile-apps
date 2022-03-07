// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Operations;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Test.Offline
{
    [ExcludeFromCodeCoverage]
    internal class MockSyncHandler : SyncHandler
    {
        public Func<ITableOperation, Task<JObject>> TableOperationAction { get; set; }

        public Func<PushCompletionResult, Task> PushCompleteAction { get; set; }

        public PushCompletionResult PushCompletionResult { get; set; }

        #region ISyncHandler
        public override Task<JObject> ExecuteTableOperationAsync(TableOperation operation, CancellationToken cancellationToken = default)
            => TableOperationAction?.Invoke(operation) ?? base.ExecuteTableOperationAsync(operation, cancellationToken);

        public override Task OnPushCompleteAsync(PushCompletionResult result, CancellationToken cancellationToken = default)
        {
            PushCompletionResult = result;
            return PushCompleteAction?.Invoke(result) ?? base.OnPushCompleteAsync(result, cancellationToken);
        }
        #endregion
    }
}
