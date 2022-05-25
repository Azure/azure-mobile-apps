// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace TestUtilities
{
    public static class PropertyAssert
    {
        private static MethodInfo isSetMethod = typeof(PropertyAssert).GetMethod("IsSet", BindingFlags.Static | BindingFlags.NonPublic);

        public static PropertyInfo GetPropertyInfo<TInstance, TProperty>(Expression<Func<TInstance, TProperty>> property)
        {
            if (property.Body is MemberExpression)
            {
                return (PropertyInfo)((MemberExpression)property.Body).Member;
            }
            else if (property.Body is UnaryExpression && property.Body.NodeType == ExpressionType.Convert)
            {
                return (PropertyInfo)((MemberExpression)((UnaryExpression)property.Body).Operand).Member;
            }
            else
            {
                throw new InvalidOperationException("Did not find any property to test.");
            }
        }

        private static Func<TInstance, TProperty> GetPropertyGetter<TInstance, TProperty>(PropertyInfo property)
        {
            return (instance) => (TProperty)property.GetValue(instance, index: null);
        }

        private static Action<TInstance, TProperty> GetPropertySetter<TInstance, TProperty>(PropertyInfo property)
        {
            return (instance, value) => property.SetValue(instance, value, index: null);
        }

        private static void TestRoundtrip<TInstance, TProperty>(TInstance instance, Func<TInstance, TProperty> getter, Action<TInstance, TProperty> setter, TProperty roundtripValue)
            where TInstance : class
        {
            TestRoundtrip(instance, getter, setter, setValue: roundtripValue, expectedValue: roundtripValue);
        }

        private static void TestRoundtrip<TInstance, TProperty>(TInstance instance, Func<TInstance, TProperty> getter, Action<TInstance, TProperty> setter, TProperty setValue, TProperty expectedValue)
            where TInstance : class
        {
            setter(instance, setValue);
            TProperty actual = getter(instance);
            Assert.Equal(expectedValue, actual);
        }

        public static void Roundtrips<TInstance, TProperty>(TInstance instance, Expression<Func<TInstance, TProperty>> propertyExpression, PropertySetter propertySetter, TProperty defaultValue = null, TProperty roundtripValue = null)
            where TInstance : class
            where TProperty : class
        {
            Roundtrips(instance, propertyExpression, propertySetter, defaultValue: defaultValue, roundtripValue: roundtripValue, initialValue: defaultValue);
        }

        public static void Roundtrips<TInstance, TProperty>(TInstance instance, Expression<Func<TInstance, TProperty>> propertyExpression, PropertySetter propertySetter, TProperty defaultValue, TProperty roundtripValue, TProperty initialValue)
            where TInstance : class
            where TProperty : class
        {
            PropertyInfo property = GetPropertyInfo(propertyExpression);
            Func<TInstance, TProperty> getter = GetPropertyGetter<TInstance, TProperty>(property);
            Action<TInstance, TProperty> setter = GetPropertySetter<TInstance, TProperty>(property);

            Assert.Equal(initialValue, getter(instance));

            switch (propertySetter)
            {
                case PropertySetter.NullRoundtrips:
                    TestRoundtrip(instance, getter, setter, roundtripValue: (TProperty)null);
                    break;

                case PropertySetter.NullSetsDefault:
                    TestRoundtrip(instance, getter, setter, setValue: (TProperty)null, expectedValue: defaultValue);
                    break;

                case PropertySetter.NullThrows:
                    TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, null));
                    Assert.IsType<ArgumentNullException>(ex.InnerException);
                    ArgumentNullException argumentNullException = ex.InnerException as ArgumentNullException;
                    Assert.Equal("value", argumentNullException.ParamName);
                    break;

                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Invalid '{0}' value", typeof(PropertySetter).Name));
            }

            if (roundtripValue != null)
            {
                TestRoundtrip(instance, getter, setter, roundtripValue: roundtripValue);
            }
        }

        public static void Roundtrips<TInstance, TProperty>(TInstance instance, Expression<Func<TInstance, TProperty?>> propertyExpression, PropertySetter propertySetter, TProperty? defaultValue = null,
            TProperty? minLegalValue = null, TProperty? illegalLowerValue = null,
            TProperty? maxLegalValue = null, TProperty? illegalUpperValue = null,
            TProperty? roundtripValue = null)
            where TInstance : class
            where TProperty : struct
        {
            PropertyInfo property = GetPropertyInfo(propertyExpression);
            Func<TInstance, TProperty?> getter = (obj) => (TProperty?)property.GetValue(obj, index: null);
            Action<TInstance, TProperty?> setter = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(defaultValue, getter(instance));

            switch (propertySetter)
            {
                case PropertySetter.NullRoundtrips:
                    TestRoundtrip(instance, getter, setter, roundtripValue: null);
                    break;

                case PropertySetter.NullSetsDefault:
                    TestRoundtrip(instance, getter, setter, roundtripValue: defaultValue);
                    break;

                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Invalid '{0}' value", typeof(PropertySetter).Name));
            }

            if (roundtripValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, roundtripValue.Value);
            }

            if (minLegalValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, minLegalValue.Value);
            }

            if (maxLegalValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, maxLegalValue.Value);
            }

            if (illegalLowerValue.HasValue)
            {
                TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, illegalLowerValue.Value));
                Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
                ArgumentOutOfRangeException rex = ex.InnerException as ArgumentOutOfRangeException;
                Assert.Equal(illegalLowerValue.Value, rex.ActualValue);
            }

            if (illegalUpperValue.HasValue)
            {
                TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, illegalUpperValue.Value));
                Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
                ArgumentOutOfRangeException rex = ex.InnerException as ArgumentOutOfRangeException;
                Assert.Equal(illegalUpperValue.Value, rex.ActualValue);
            }
        }

        public static void Roundtrips<TInstance, TProperty>(TInstance instance, Expression<Func<TInstance, TProperty>> propertyExpression, TProperty defaultValue = default(TProperty),
        TProperty? minLegalValue = null, TProperty? illegalLowerValue = null,
        TProperty? maxLegalValue = null, TProperty? illegalUpperValue = null,
        TProperty? roundtripValue = null)
            where TInstance : class
            where TProperty : struct
        {
            PropertyInfo property = GetPropertyInfo(propertyExpression);
            Func<TInstance, TProperty> getter = (obj) => (TProperty)property.GetValue(obj, index: null);
            Action<TInstance, TProperty> setter = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(defaultValue, getter(instance));

            if (roundtripValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, roundtripValue.Value);
            }

            if (minLegalValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, minLegalValue.Value);
            }

            if (maxLegalValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, maxLegalValue.Value);
            }

            if (illegalLowerValue.HasValue)
            {
                TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, illegalLowerValue.Value));
                Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
                ArgumentOutOfRangeException rex = ex.InnerException as ArgumentOutOfRangeException;
                Assert.Equal(illegalLowerValue.Value, rex.ActualValue);
            }

            if (illegalUpperValue.HasValue)
            {
                TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, illegalUpperValue.Value));
                Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
                ArgumentOutOfRangeException rex = ex.InnerException as ArgumentOutOfRangeException;
                Assert.Equal(illegalUpperValue.Value, rex.ActualValue);
            }
        }

        /// <summary>
        /// Validates that all public properties have been set on a particular type
        /// and that all public collections have at least one member.
        /// </summary>
        public static void PublicPropertiesAreSet<TInstance>(TInstance instance, IEnumerable<string> excludeProperties = null)
            where TInstance : class
        {
            PropertyInfo[] properties = typeof(TInstance).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                if (excludeProperties != null && excludeProperties.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (p.CanWrite)
                {
                    MethodInfo isSet = isSetMethod.MakeGenericMethod(p.PropertyType);
                    bool result = (bool)isSet.Invoke(instance, new object[] { p.GetValue(instance) });
                    Assert.True(result, string.Format("Parameter '{0}' was not set on type '{1}'", p.Name, typeof(TInstance).Name));
                }
                else if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                {
                    Assert.NotEmpty((IEnumerable)p.GetValue(instance));
                }
            }
        }

        private static bool IsSet<T>(T value)
        {
            return !EqualityComparer<T>.Default.Equals(value, default(T));
        }
    }
}
