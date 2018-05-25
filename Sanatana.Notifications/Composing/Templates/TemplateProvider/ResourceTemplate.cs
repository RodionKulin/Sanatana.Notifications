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

namespace Sanatana.Notifications.Composing.Templates
{
    public class ResourceTemplate : ITemplateProvider
    {
        //fields
        protected ResourceManager _resourceManager;


        //properties
        public Type ResourceType { get; set; }
        public string ResourceName { get; set; }
     


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

        public virtual string ProvideTemplate(CultureInfo culture = null)
        {
            culture = culture ?? Thread.CurrentThread.CurrentCulture;
                        
            ResourceSet set = _resourceManager.GetResourceSet(culture, true, true);
            return set.GetString(ResourceName);
        }
    }

}
