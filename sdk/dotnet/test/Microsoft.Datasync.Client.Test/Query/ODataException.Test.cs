// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Test.Query;

[ExcludeFromCodeCoverage]
public class ODataException_Tests
{
    [Fact]
    public void Ctor_Message_Works()
    {
        var ex = new ODataException("message", "expression", 42);
        Assert.Equal("message", ex.Message);
        Assert.Equal("expression", ex.Expression);
        Assert.Equal(42, ex.ErrorPosition);
    }

    [Fact]
    public void Ctor_MessageInner_Works()
    {
        var innerException = new ApplicationException();
        var ex = new ODataException("message", innerException, "expression", 42);
        Assert.Equal("message", ex.Message);
        Assert.Same(innerException, ex.InnerException);
        Assert.Equal("expression", ex.Expression);
        Assert.Equal(42, ex.ErrorPosition);
    }
}
