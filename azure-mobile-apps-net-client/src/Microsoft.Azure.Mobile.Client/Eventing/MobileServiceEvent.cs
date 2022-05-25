// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    /// <summary>
    /// Represents a mobile service event.
    /// </summary>
    public class MobileServiceEvent : IMobileServiceEvent
    {
        private readonly string name;

        /// <summary>
        /// Creates an instance of a <see cref="MobileServiceEvent"/>.
        /// </summary>
        /// <param name="name">The event name.</param>
        public MobileServiceEvent(string name)
        {
            Arguments.IsNotNull(name, nameof(name));

            this.name = name;
        }

        /// <summary>
        /// Gets the event name.
        /// </summary>
        public virtual string Name => name;
    }

    /// <summary>
    /// Represents a mobile service event with a strongly typed payload.
    /// </summary>
    /// <typeparam name="T">The type of payload in this event.</typeparam>
    public class MobileServiceEvent<T> : MobileServiceEvent
    {
        /// <summary>
        /// Initializes an instance of the MobileServiceEvent using the specified event name and payload.
        /// </summary>
        /// <param name="name">The event name.</param>
        /// <param name="payload">The payload associated with this event.</param>
        public MobileServiceEvent(string name, T payload) : base(name)
        {
            Payload = payload;
        }

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        public T Payload { get; private set; }
    }
}
