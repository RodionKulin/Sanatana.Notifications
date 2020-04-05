using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public class ResourceTemplate : ITemplateProvider
    {
        //fields
        protected ResourceManager _resourceManager;


        //properties
        public Type ResourceType { get; set; }
        public string ResourceName { get; set; }
        /// <summary>
        /// Default resource culture if Subscriber does not have a valid language specified. Thread.CurrentThread.CurrentCulture by default.
        /// </summary>
        public CultureInfo DefaultCulture { get; set; } = Thread.CurrentThread.CurrentCulture;



        //init
        public ResourceTemplate(Type resourceType, string resourceName)
        {
            ResourceType = resourceType;
            ResourceName = resourceName;
            InitialiseResourseManager();
        }



        //methods
        protected virtual void InitialiseResourseManager()
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            PropertyInfo resourceManagerProp = ResourceType.GetProperty("ResourceManager", flags);
            _resourceManager = (ResourceManager)resourceManagerProp.GetValue(null);
        }

        public virtual string ProvideTemplate(string language = null)
        {
            CultureInfo culture = null;
            try
            {
                culture = string.IsNullOrEmpty(language) ? null : new CultureInfo(language);
            }
            catch (CultureNotFoundException ex)
            {
            }
            culture = culture ?? DefaultCulture;

            ResourceSet set = _resourceManager.GetResourceSet(culture, true, true);
            return set.GetString(ResourceName);
        }
    }

}
