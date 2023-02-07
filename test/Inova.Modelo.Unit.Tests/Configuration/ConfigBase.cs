using AutoMapper;
using Inova.Modelo.API.AutoMapper;

namespace Inova.Modelo.Unit.Tests.Configuration
{
    public class ConfigBase
    {
        public readonly IMapper _mapper;

        public ConfigBase()
        {

            if (_mapper == null)
            {
                var mappingConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new MappingProfiles());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;
            }
        }
    }
}
