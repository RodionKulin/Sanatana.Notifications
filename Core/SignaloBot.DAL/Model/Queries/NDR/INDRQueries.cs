using System;
namespace SignaloBot.DAL.Queries.NDR
{
    public interface INDRQueries
    {
        void BouncedMessage_Insert(global::System.Collections.Generic.List<global::SignaloBot.DAL.Entities.BouncedMessage> messages);
        global::System.Collections.Generic.List<global::SignaloBot.DAL.Entities.Core.UserDeliveryTypeSettings> NDRSettings_Select(int deliveryType, global::System.Collections.Generic.List<string> emails);
        void NDRSettings_Update(global::System.Collections.Generic.List<global::SignaloBot.DAL.Entities.Core.UserDeliveryTypeSettings> settings);
    }
}
