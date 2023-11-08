// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using AutoMapper;

namespace Microsoft.AspNetCore.Datasync.Automapper.Tests;

[ExcludeFromCodeCoverage]
public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<SqliteEntityMovie, MovieDto>().ReverseMap();
    }
}
