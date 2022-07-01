// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using AutoMapper;
using Datasync.Common.Test.Models;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync.Automapper.Test.Helpers
{
    [ExcludeFromCodeCoverage]
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<EFMovie, MovieDto>().ReverseMap();
        }
    }
}
