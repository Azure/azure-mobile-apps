// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using System.Reflection;

namespace Microsoft.Datasync.Client.Test.Query;

[ExcludeFromCodeCoverage]
public class BaseQueryTest : BaseTest
{
    private static IEnumerable<Type> GetBaseTypesAndSelf(Type type)
    {
        Assert.NotNull(type);

        while (type != null)
        {
            yield return type;
            type = type.GetTypeInfo().BaseType;
        }
    }

    protected static IEnumerable<MethodInfo> GetMethods(Type type, string name, Type[] parameterTypes)
        => GetBaseTypesAndSelf(type).SelectMany(t => t.GetRuntimeMethods().Where(m => m.Name == name)).Where(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));

    protected static MethodInfo FindInstanceMethod(Type type, string name, params Type[] parameterTypes)
        => GetMethods(type, name, parameterTypes).SingleOrDefault(m => !m.IsStatic);

    protected static MemberInfo FindInstanceProperty(Type type, string name)
        => GetBaseTypesAndSelf(type)
            .SelectMany(t => t.GetRuntimeProperties().Where(p => p.Name == name && p.CanRead && !p.GetMethod.IsStatic))
            .Cast<MemberInfo>()
            .SingleOrDefault();

    protected static MethodInfo FindStaticMethod(Type type, string name, params Type[] parameterTypes)
        => GetMethods(type, name, parameterTypes).SingleOrDefault(m => m.IsStatic);
}
