using AutoMapper;
using Datasync.Common.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync.Automapper.Test.Helpers
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<EFMovie, MovieDto>().ReverseMap();
        }
    }
}
