using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    public class MapperUtility
    {
        //поля
        private static IMapper _mapper;
        private static object _mapperLock = new object();


        //свойства
        public static IMapper Mapper
        {
            get
            {
                if (_mapper == null)
                {
                    lock (_mapperLock)
                    {
                        if (_mapper == null)
                        {
                            var config = new MapperConfiguration(cfg => {
                                Configure(cfg);
                            });
                            Mapper = config.CreateMapper();
                        }
                    }
                }
                return _mapper;
            }
            set
            {
                _mapper = value;
            }
        }


        //методы
        public static void Configure(IMapperConfiguration configuration)
        {
            configuration.CreateMap<SignalBounce<Guid>, SignalBounceGuid>();
            configuration.CreateMap<SignalDispatchBase<Guid>, SignalDispatchBaseGuid>();
            configuration.CreateMap<SubjectDispatch<Guid>, SubjectDispatchGuid>();
            configuration.CreateMap<UserCategorySettings<Guid>, UserCategorySettingsGuid>();
            configuration.CreateMap<UserDeliveryTypeSettings<Guid>, UserDeliveryTypeSettingsGuid>();
            configuration.CreateMap<UserReceivePeriod<Guid>, UserReceivePeriodGuid>();
            configuration.CreateMap<UserTopicSettings<Guid>, UserTopicSettingsGuid>();
        }
    }
}
