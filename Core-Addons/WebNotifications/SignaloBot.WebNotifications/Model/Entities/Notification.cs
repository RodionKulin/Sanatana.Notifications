using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SignaloBot.WebNotifications.Entities
{
    public class Notification
    {
        //свойства
        /// <summary>
        /// Идентификатор оповещения.
        /// </summary>
        public Guid NotificationID { get; set; }
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public Guid UserID { get; set; }
        /// <summary>
        /// Идентификатор категории.
        /// </summary>
        public int CategoryID { get; set; }
        /// <summary>
        /// Идентификатор темы.
        /// </summary>
        public int TopicID { get; set; }
        /// <summary>
        /// Текст оповещения.
        /// </summary>
        public string NotifyText { get; set; }
        /// <summary>
        /// Время отправки оповещения в UTC.
        /// </summary>
        public DateTime SendDateUtc { get; set; }
        /// <summary>
        /// Есть ли изменения в данных, о которых упоминается в оповещении. (Имена пользователей, названия статей, и т.д.)
        /// </summary>
        public bool IsDirty { get; set; }
        /// <summary>
        /// Номер варианта текста оповещения.
        /// </summary>
        public int Variant { get; set; }
        /// <summary>
        /// Язык на котором отображается оповещение.
        /// </summary>
        public string Culture { get; set; }        
        /// <summary>
        /// Не обязательное свойство, для хранения дополнительных данных.
        /// </summary>
        public string Tag { get; set; }
        

    }
}