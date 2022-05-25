// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Net.Http.Formatting;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.Mobile.Server.Serialization
{
    /// <summary>
    /// This class implements an <see cref="IContractResolver"/> to provide support for deserialization of the <see cref="Delta{T}"/> type
    /// using JSON.NET. 
    /// </summary>
    /// <remarks>
    /// The contract created for <see cref="Delta{T}"/> will deserialize properties using the types and property names of the 
    /// underlying type. The <see cref="JsonProperty"/> instances are copied from the underlying type's <see cref="JsonContract"/> and 
    /// customized to work with a dynamic object. In particular, a custom <see cref="IValueProvider"/> is used to get and set 
    /// values using the contract of <see cref="DynamicObject"/>, which <see cref="Delta{T}"/> inherits from.
    /// </remarks>
    public class TableContractResolver : ServiceContractResolver
    {
        private ConcurrentDictionary<Type, JsonContract> cache = new ConcurrentDictionary<Type, JsonContract>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TableContractResolver"/> class with a given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">The <see cref="MediaTypeFormatter"/> that this <see cref="TableContractResolver"/> is
        /// associated with.</param>
        public TableContractResolver(MediaTypeFormatter formatter)
            : base(formatter)
        {
        }

        /// <summary>
        /// Gets the contract for a given type. The type <see cref="Delta{T}"/> is treated specially
        /// whereas all other types are handled by the base class. 
        /// </summary>
        /// <param name="objectType">The type to get the contract for.</param>
        /// <returns>A <see cref="JsonContract"/> for the given type.</returns>
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            if (objectType.IsGenericType &&
                objectType.GetGenericTypeDefinition() == typeof(Delta<>) &&
                objectType.GetGenericArguments().Length == 1)
            {
                return this.cache.GetOrAdd(objectType, type => this.GetDeltaContract(type));
            }

            JsonContract contract = base.CreateContract(objectType);

            return contract;
        }

        /// <summary>
        /// Creates a contract for a type of <see cref="Delta{T}"/>.
        /// </summary>
        /// <param name="objectType">The type to provide a contract for.</param>
        /// <returns>A <see cref="JsonContract"/> for the given type.</returns>
        protected virtual JsonContract GetDeltaContract(Type objectType)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            JsonDynamicContract contract = CreateDynamicContract(objectType);
            contract.Properties.Clear();

            JsonObjectContract underlyingContract = CreateObjectContract(objectType.GetGenericArguments()[0]);
            foreach (var property in underlyingContract.Properties)
            {
                property.DeclaringType = objectType;
                property.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
                property.NullValueHandling = NullValueHandling.Include;
                property.ValueProvider = new DynamicObjectValueProvider()
                {
                    PropertyName = property.UnderlyingName,
                };

                contract.Properties.Add(property);
            }

            return contract;
        }

        private class DynamicObjectValueProvider : IValueProvider
        {
            public string PropertyName { get; set; }

            public object GetValue(object target)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }

                DynamicObject d = (DynamicObject)target;
                object result;
                GetMemberBinder binder = CreateGetMemberBinder(target.GetType(), this.PropertyName);
                d.TryGetMember(binder, out result);
                return result;
            }

            public void SetValue(object target, object value)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }

                DynamicObject d = (DynamicObject)target;
                SetMemberBinder binder = CreateSetMemberBinder(target.GetType(), this.PropertyName);
                d.TrySetMember(binder, value);
            }

            private static GetMemberBinder CreateGetMemberBinder(Type type, string memberName)
            {
                return (GetMemberBinder)Binder.GetMember(CSharpBinderFlags.None, memberName, type, new CSharpArgumentInfo[] { });
            }

            private static SetMemberBinder CreateSetMemberBinder(Type type, string memberName)
            {
                return (SetMemberBinder)Binder.SetMember(CSharpBinderFlags.None, memberName, type, new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            }
        }
    }
}
