// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    internal sealed class MobileServiceEventManager : IMobileServiceEventManager, IDisposable
    {
        private readonly List<ISubscription> subscriptions;
        private readonly ReaderWriterLockSlim subscriptionLock = new ReaderWriterLockSlim();
        private static readonly Type observerTypeDefinition;

        static MobileServiceEventManager()
        {
            observerTypeDefinition = typeof(IEventObserver<IMobileServiceEvent>).GetTypeInfo().GetGenericTypeDefinition();
        }

        public MobileServiceEventManager()
        {
            this.subscriptions = new List<ISubscription>();
        }

        ~MobileServiceEventManager()
        {
            Dispose(false);
        }

        public Task PublishAsync(IMobileServiceEvent mobileServiceEvent)
        {
            Arguments.IsNotNull(mobileServiceEvent, nameof(mobileServiceEvent));

            return Task.Run(() =>
            {
                TypeInfo messageType = mobileServiceEvent.GetType().GetTypeInfo();
                subscriptionLock.EnterReadLock();
                IList<ISubscription> subscriptionMatches = null;
                try
                {
                    subscriptionMatches = subscriptions.Where(s => s.TargetMessageType.GetTypeInfo().IsAssignableFrom(messageType)).ToList();
                }
                finally
                {
                    subscriptionLock.ExitReadLock();
                }

                foreach (var subscription in subscriptionMatches)
                {
                    subscription.OnNext(mobileServiceEvent);
                }
            });
        }

        public IDisposable Subscribe(IObserver<IMobileServiceEvent> observer)
        {
            Arguments.IsNotNull(observer, nameof(observer));

            Type messageType = GetMessageType(observer);
            var subscription = new Subscription<IMobileServiceEvent>(this, observer, messageType);
            return Subscribe(subscription);
        }

        public IDisposable Subscribe<T>(Action<T> next) where T : class, IMobileServiceEvent
        {
            Arguments.IsNotNull(next, nameof(next));

            var observer = new MobileServiceEventObserver<T>(next);
            var subscription = new Subscription<T>(this, observer);
            return Subscribe(subscription);
        }

        private IDisposable Subscribe(ISubscription subscription)
        {
            subscriptionLock.EnterWriteLock();
            try
            {
                subscriptions.Add(subscription);
            }
            finally
            {
                subscriptionLock.ExitWriteLock();
            }

            return subscription;
        }

        private void Unsubscribe(ISubscription subscription)
        {
            Task.Run(() =>
            {
                subscriptionLock.EnterWriteLock();
                try
                {
                    subscriptions.Remove(subscription);
                }
                finally
                {
                    subscriptionLock.ExitWriteLock();
                }
            });
        }

        private Type GetMessageType(IObserver<IMobileServiceEvent> observer)
        {
            TypeInfo typeInfo = observer.GetType().GetTypeInfo();
            return typeInfo.ImplementedInterfaces.Select(i => i.GetTypeInfo())
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == observerTypeDefinition)
                .Select(i => i.GenericTypeArguments[0]).First();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                subscriptionLock.Dispose();
            }
        }

        private sealed class Subscription<T> : IDisposable, ISubscription where T : class, IMobileServiceEvent
        {
            private readonly MobileServiceEventManager service;
            private readonly IObserver<T> observer;
            private readonly Type targetMessageType;

            public Subscription(MobileServiceEventManager service, IObserver<T> observer, Type targetMessageType = null)
            {
                Arguments.IsNotNull(service, nameof(service));
                Arguments.IsNotNull(observer, nameof(observer));

                this.service = service;
                this.observer = observer;

                this.targetMessageType = targetMessageType ?? typeof(T);
            }

            public Type TargetMessageType => targetMessageType;

            public void Dispose()
            {
                service.Unsubscribe(this);
            }


            public void OnNext(IMobileServiceEvent mobileServiceEvent)
            {
                observer.OnNext((T)mobileServiceEvent);
            }
        }
    }
}
