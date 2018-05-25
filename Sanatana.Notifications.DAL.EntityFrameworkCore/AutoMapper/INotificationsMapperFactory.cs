using AutoMapper;
using Sanatana.Notifications.DAL;
using System;
using System.Collections.Generic;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper
{
    public interface INotificationsMapperFactory
    {
        IMapper GetMapper();

    }
}