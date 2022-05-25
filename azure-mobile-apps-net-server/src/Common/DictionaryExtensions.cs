// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.ComponentModel;
using System.Globalization;

namespace System.Collections.Generic
{
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance where both <c>TKey</c> and <c>TValue</c> are of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value with the specified key if found; otherwise the default value.</param>
        /// <returns><c>true</c> if key was found and value is of type <typeparamref name="TValue"/> and non-null; otherwise false.</returns>
        public static bool TryGetValue<TValue>(this IDictionary<object, object> dictionary, object key, out TValue value)
        {
            if (dictionary != null)
            {
                object valueAsObj;
                if (dictionary.TryGetValue(key, out valueAsObj))
                {
                    if (valueAsObj is TValue)
                    {
                        value = (TValue)valueAsObj;
                        return true;
                    }
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance where both <c>TKey</c> and <c>TValue</c> are of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>the value with the specified key if found; otherwise the default value.</returns>
        public static TValue GetValueOrDefault<TValue>(this IDictionary<object, object> dictionary, object key)
        {
            TValue value;
            TryGetValue(dictionary, key, out value);
            return value;
        }

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <see cref="string"/> and <c>TValue</c> of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value with the specified key if found; otherwise the default value.</param>
        /// <returns><c>true</c> if key was found and value is of type <typeparamref name="TValue"/> and non-null; otherwise false.</returns>
        public static bool TryGetValue<TValue>(this IDictionary<string, object> dictionary, string key, out TValue value)
        {
            if (dictionary != null)
            {
                object valueAsObj;
                if (dictionary.TryGetValue(key, out valueAsObj))
                {
                    if (valueAsObj is TValue)
                    {
                        value = (TValue)valueAsObj;
                        return true;
                    }
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <see cref="string"/> and <c>TValue</c> of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>the value with the specified key if found; otherwise the default value.</returns>
        public static TValue GetValueOrDefault<TValue>(this IDictionary<string, object> dictionary, string key)
        {
            TValue value;
            TryGetValue(dictionary, key, out value);
            return value;
        }

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <see cref="Type"/> and <c>TValue</c> of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value with the specified key if found; otherwise the default value.</param>
        /// <returns><c>true</c> if key was found and value is of type <typeparamref name="TValue"/> and non-null; otherwise false.</returns>
        public static bool TryGetValue<TValue>(this IDictionary<Type, object> dictionary, Type key, out TValue value)
        {
            if (dictionary != null)
            {
                object valueAsObj;
                if (dictionary.TryGetValue(key, out valueAsObj))
                {
                    if (valueAsObj is TValue)
                    {
                        value = (TValue)valueAsObj;
                        return true;
                    }
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <see cref="Type"/> and <c>TValue</c> of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>the value with the specified key if found; otherwise the default value.</returns>
        public static TValue GetValueOrDefault<TValue>(this IDictionary<Type, object> dictionary, Type key)
        {
            TValue value;
            TryGetValue(dictionary, key, out value);
            return value;
        }

        /// <summary>
        /// Sets the entry with the given key to the given value. If value is the default value
        /// then the entry is removed.
        /// </summary>
        /// <typeparam name="T">Type of value to be set or cleared.</typeparam>
        /// <param name="dictionary">The dictionary to insert of clear a value from.</param>
        /// <param name="key">The key of the entry.</param>
        /// <param name="value">The value (or default value).</param>
        public static void SetOrClearValue<T>(this IDictionary<string, object> dictionary, string key, T value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                dictionary.Remove(key);
            }
            else
            {
                dictionary[key] = value;
            }
        }

        /// <summary>
        /// Gets the value with the given key, or null if the key is not present. 
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <c>string</c >and <c>TValue</c> of type <c>string</c>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>the value with the specified key if found; otherwise null.</returns>
        public static string GetValueOrDefault(this IDictionary<string, string> dictionary, string key)
        {
            string value;
            return dictionary.TryGetValue(key, out value) ? value : null;
        }

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <c>string</c >and <c>TValue</c> of type <c>string</c>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>the value with the specified key if found; otherwise null.</returns>
        public static TValue GetValueOrDefault<TValue>(this IDictionary<string, string> dictionary, string key) where TValue : IConvertible
        {
            TValue value;
            TryGetValue(dictionary, key, out value);
            return value;
        }

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <see cref="string"/> and <c>TValue</c> of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value with the specified key if found; otherwise the default value.</param>
        /// <returns><c>true</c> if key was found and value is of type <typeparamref name="TValue"/>; otherwise false.</returns>
        public static bool TryGetValue<TValue>(this IDictionary<string, string> dictionary, string key, out TValue value) where TValue : IConvertible
        {
            if (dictionary != null)
            {
                string valueAsString;
                if (dictionary.TryGetValue(key, out valueAsString))
                {
                    try
                    {
                        value = (TValue)Convert.ChangeType(valueAsString, typeof(TValue), CultureInfo.InvariantCulture);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is FormatException || ex is OverflowException || ex is InvalidCastException))
                        {
                            throw;
                        }                        
                    }                    
                }
            }

            value = default(TValue);
            return false;
        }
    }
}
