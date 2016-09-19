using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class UserCategorySettings<TKey>
        where TKey : struct
    {
        //свойства
        public TKey UserID { get; set; }
        public TKey? GroupID { get; set; }
        public int DeliveryType { get; set; }
        public int CategoryID { get; set; }

        public DateTime? LastSendDateUtc { get; set; }
        public int SendCount { get; set; }
        public bool IsEnabled { get; set; }
        


        //инициализация
        public static UserCategorySettings<TKey> Default(
            TKey userID, int deliveryType, int categoryID, TKey? groupID = null)
        {
            return new UserCategorySettings<TKey>()
            {
                UserID = userID,
                GroupID = groupID,
                DeliveryType = deliveryType,
                CategoryID = categoryID,
                IsEnabled = true,
                LastSendDateUtc = null,
                SendCount = 0
            };
        }

    }
}
