using SignaloBot.Client.Resources;
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

namespace SignaloBot.Client.Templates
{
    public class ResourceTemplate : ITemplateProvider
    {
        //поля
        ResourceManager _resourceManager;


        //свойства
        public Type ResourceType { get; set; }
        public List<string> ResourceNames { get; set; }
        public int VariantsCount
        {
            get { return ResourceNames.Count; }
        }


        //инициализация
        public ResourceTemplate(Type resourceType, string resourceName)
        {
            ResourceType = resourceType;
            ResourceNames = new List<string>() { resourceName };
            InitialiseResourseManager();
        }

        public ResourceTemplate(Type resourceType, List<string> resourceNames)
        {
            ResourceType = resourceType;
            ResourceNames = resourceNames;
            InitialiseResourseManager();
        }

        private void InitialiseResourseManager()
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            PropertyInfo resourceManagerProp = ResourceType.GetProperty("ResourceManager", flags);
            _resourceManager = (ResourceManager)resourceManagerProp.GetValue(null);
        }



        //методы
        public string ProvideTemplate(int variant = 0, CultureInfo culture = null)
        {
            string resourceKey = ResourceNames[variant];
            culture = culture ?? Thread.CurrentThread.CurrentCulture;
                        
            ResourceSet set = _resourceManager.GetResourceSet(culture, true, true);
            return set.GetString(resourceKey);
        }
    }

}
