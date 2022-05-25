// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace TestUtilities
{
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Ex is short for exception.")]
    public static class AssertEx
    {
        public static async Task<TException> ThrowsAsync<TException>(Func<Task> testCode)
            where TException : Exception
        {
            try
            {
                await testCode();
            }
            catch (Exception ex)
            {
                Assert.IsType<TException>(ex);
                return (TException)ex;
            }

            Assert.True(false, string.Format("Expected an exception of type '{0}' but no exception was thrown.", typeof(TException).Name));
            return null;
        }
    }
}
