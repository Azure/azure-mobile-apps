// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    internal interface IEventObserver<out T> : IObserver<IMobileServiceEvent> { }

    internal sealed class MobileServiceEventObserver<T> : IEventObserver<T> where T : class, IMobileServiceEvent
    {
        private readonly Action<T> next;

        public MobileServiceEventObserver(Action<T> nextHandler)
        {
            Arguments.IsNotNull(nextHandler, nameof(nextHandler));
            next = nextHandler;
        }

        public void OnNext(IMobileServiceEvent value)
        {
            next((T)value);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public IObserver<T> Observer { get { return this; } }

        public void OnNext(T value)
        {
            throw new NotImplementedException();
        }
    }
}
