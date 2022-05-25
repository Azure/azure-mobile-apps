// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// This is the non-generic common base class for table controllers. It is strongly recommended instead using
    /// the generic version TableController{T} which provides strongly typed support for the
    /// various table operations.
    /// </summary>
    [TableControllerConfig]
    public abstract class TableController : ApiController
    {
    }
}