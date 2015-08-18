using SignaloBot.Client.Templates;
using SignaloBot.DAL.Entities;
using SignaloBot.WebNotifications.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SignaloBot.WebNotifications.Manager
{
    public class NotificationSettings
    {
        bool _isUnique = false;
        bool _upsertSameTopic = false;
        bool _saveMeta = true;


        //свойства
        /// <summary>
        /// Номер категории оповещений
        /// </summary>
        internal int CategoryID { get; set; }
        /// <summary>
        /// Является уникальным оповещением в категории. Оповещение с этим флагом можно отправить только один раз.
        /// </summary>
        public bool IsUnique
        {
            get { return _isUnique; }
            set { _isUnique = value; }
        }
        /// <summary>
        /// Обновлять существующее оповещение в той же категории categoryID и той же теме topicID или добавлять новое.
        /// </summary>
        public bool UpsertSameTopic 
        {
            get { return _upsertSameTopic; }
            set { _upsertSameTopic = value; }
        }
        /// <summary>
        /// Сохранять мета данные оповещения.
        /// </summary>
        public bool SaveMeta 
        {
            get { return _saveMeta; }
            set { _saveMeta = value; }
        }
        /// <summary>
        /// Шаблон текста оповещения.
        /// </summary>
        public ITemplateProvider TemplateProvider { get; set; }
        /// <summary>
        /// Преобразователь шаблона с данными в сообщение.
        /// </summary>
        public ITemplateTransformer TemplateTransformer { get; set; }



        
        //инициализация
        public NotificationSettings(ITemplateProvider templateProvider, ITemplateTransformer templateTransformer)
        {
            TemplateProvider = templateProvider;
            TemplateTransformer = templateTransformer;
        }


        //методы
        public virtual string BuildNotificationText(List<NotificationMeta> metaStrings
            , int variantIndex = 0, CultureInfo culture = null)
        {
            Dictionary<string, string> replaceModel = new Dictionary<string, string>();
            foreach (NotificationMeta item in metaStrings)
	        {
                replaceModel.Add(item.MetaType, item.MetaValue);
	        }

            return BuildNotificationText(replaceModel, variantIndex, culture);
        }

        public virtual string BuildNotificationText(Dictionary<string, string> replaceModel
            , int variantIndex = 0, CultureInfo culture = null)
        {
            string template = TemplateProvider.ProvideTemplate(variantIndex, culture);

            return TemplateTransformer.Transform(template, replaceModel);
        }

    }
}
