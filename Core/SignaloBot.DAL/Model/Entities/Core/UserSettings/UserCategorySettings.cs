using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Core
{
    public class UserCategorySettings
    {
        //свойства
        public Guid UserID { get; set; }
        public int DeliveryType { get; set; }
        public int CategoryID { get; set; }

        public bool IsEnabled { get; set; }
        public DateTime? LastSendDateUtc { get; set; }
        public int SendCount { get; set; }


        


        //defaults
        public static UserCategorySettings Default(Guid userID, int deliveryType, int categoryID)
        {
            return new UserCategorySettings()
            {
                UserID = userID,
                DeliveryType = deliveryType,
                CategoryID = categoryID,
                IsEnabled = true,
                LastSendDateUtc = null,
                SendCount = 0
            };
        }

    }
}
