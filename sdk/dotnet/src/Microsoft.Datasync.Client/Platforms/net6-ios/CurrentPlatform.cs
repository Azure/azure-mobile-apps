// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;

namespace Microsoft.Datasync.Client.Platforms
{
    /// <summary>
    /// The Android-specific implementation of the <see cref="IPlatform"/> interface.
    /// </summary>
    internal class CurrentPlatform : IPlatform
    {
        /// <summary>
        /// Lazy initializer for the <see cref="IApplicationStorage"/> implementation.
        /// </summary>
        private readonly Lazy<IApplicationStorage> _applicationStorage = new(() => new ApplicationStorage());

        /// <summary>
        /// Accessor for the platform-specific <see cref="IApplicationStorage"/> implementation.
        /// </summary>
        public IApplicationStorage ApplicationStorage { get => _applicationStorage.Value; }

        /// <summary>
        /// Lazy initializer for the <see cref="IPlatformInformation"/> implementation.
        /// </summary>
        private readonly Lazy<IPlatformInformation> _platformInformation = new(() => new PlatformInformation());

        /// <summary>
        /// Accessor for the platform-specific <see cref="IPlatformInformation"/> implementation.
        /// </summary>
        public IPlatformInformation PlatformInformation { get => _platformInformation.Value; }
    }
}
