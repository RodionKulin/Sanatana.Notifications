using Common.Utility;
using SignaloBot.Client.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.Manager
{
    public class NDRManager
    {
        //свойства
        public SignaloBotContext Context { get; set; }


         //инициализация
        public NDRManager(SignaloBotContext context)
        {
            Context = context; 
        }



        //методы   
        public virtual string CreateUserIDHash(Guid userID, string salt)
        {
            string userIDString = userID.ToString().Replace("-", string.Empty);
            string encodedString = string.Format("{0}{1}", userIDString, salt);

            return Cryptography.EncryptMD5(encodedString).ToLower();
        }

        public virtual bool CheckUserIDHash(string hash, string userID, string salt)
        {
            Guid userIdGuid;
            if(!Guid.TryParse(userID, out userIdGuid))
            {
                return false;
            }

            string expected = CreateUserIDHash(userIdGuid, salt);
            return hash == expected;
        }

        public virtual void ResetNDRCount(Guid userID, int deliveryType, out Exception exception)
        {
            Context.Queries.UserDeliveryTypeSettings.ResetNDRCount(userID, deliveryType, out exception);
        }

        public virtual string GenerateResetNDRCountCode(Guid userID, int deliveryType, out Exception exception)
        {
            string resetCode = Guid.NewGuid().ToString();
            
            Context.Queries.UserDeliveryTypeSettings.UpdateNDRResetCode(userID, deliveryType, resetCode, out exception);

            return resetCode;
        }
    }
}
