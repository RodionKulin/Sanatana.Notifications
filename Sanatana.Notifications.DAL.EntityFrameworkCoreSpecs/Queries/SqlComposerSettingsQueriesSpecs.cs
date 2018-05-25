using NUnit.Framework;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.ShouldExtensions;
using Sanatana.Notifications.DeliveryTypes.Email;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Composing.Templates;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using AutoMapper;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.Queries
{
    public class SqlComposerSettingsQueriesSpecs
    {
        [TestFixture]
        public class when_composer_settings_insert_using_ef
           : SpecsFor<SqlComposerSettingsQueries>, INeedDbContext
        {
            private List<ComposerSettings<long>> _insertedData;
            public SenderDbContext DbContext { get; set; }



            private List<ComposerSettings<long>> GetComposerSettings()
            {
                return new List<ComposerSettings<long>>()
                {
                    new ComposerSettings<long>()
                    {
                        CategoryId = 2,
                        Subscription = new SubscriptionParameters()
                        {
                            CategoryId = 2
                        },
                        CompositionHandlerId = null,
                        Templates = new List<DispatchTemplate<long>>()
                        {
                            new EmailDispatchTemplate<long>()
                            {
                                DeliveryType = 2,
                                IsBodyHtml = false,
                                SubjectProvider = (StringTemplate)"subject {key}",
                                SubjectTransformer = new ReplaceTransformer(),
                                BodyProvider = (StringTemplate)"body text {key}",
                                BodyTransformer = new ReplaceTransformer(),
                            },
                            new EmailDispatchTemplate<long>()
                            {
                                DeliveryType = 2,
                                IsBodyHtml = false,
                                SubjectProvider = (StringTemplate)"subject {key}",
                                SubjectTransformer = new ReplaceTransformer(),
                                BodyProvider = (StringTemplate)"body text {key}",
                                BodyTransformer = new ReplaceTransformer(),
                            }
                        },
                        Updates = new UpdateParameters()
                    },
                    new ComposerSettings<long>()
                    {
                        CategoryId = 3,
                        Subscription = new SubscriptionParameters()
                        {
                            CategoryId = 3
                        },
                        Templates = new List<DispatchTemplate<long>>()
                        {
                            new EmailDispatchTemplate<long>()
                            {
                                DeliveryType = 3,
                                IsBodyHtml = true,
                                SubjectProvider = new StringTemplate("Demo event in games category"),
                                SubjectTransformer = null,
                                BodyProvider = new StringTemplate("Games-Body.cshtml"),
                                BodyTransformer = new ReplaceTransformer(),
                            }
                        },
                        Updates = new UpdateParameters()
                    }
                };
            }
            
            protected override void When()
            {
                _insertedData = GetComposerSettings();

                SUT.Insert(_insertedData).Wait();
            }


            [Test]
            public void then_composer_settings_inserted_are_found_using_ef()
            {
                List<ComposerSettingsLong> actual = DbContext.ComposerSettings
                   .OrderBy(x => x.ComposerSettingsId)
                   .ToList();

                actual.ShouldNotBeEmpty();
                actual.Count.ShouldEqual(_insertedData.Count);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    ComposerSettingsLong actualItem = actual[i];
                    ComposerSettings<long> expectedItem = _insertedData[i];

                    expectedItem.ComposerSettingsId = actualItem.ComposerSettingsId;
                    actualItem.Templates = expectedItem.Templates;

                    actualItem.ShouldLookLike(expectedItem);
                }
            }

            [Test]
            public void then_dispatch_templates_inserted_are_found_using_ef()
            {
                INotificationsMapperFactory mapperFactory = MockContainer.GetInstance<INotificationsMapperFactory>();
                IMapper mapper = mapperFactory.GetMapper();

                List<DispatchTemplateLong> actual = DbContext.DispatchTemplates
                   .OrderBy(x => x.ComposerSettingsId)
                   .ThenBy(x => x.DispatchTemplateId)
                   .ToList();

                List<DispatchTemplate<long>> expected = _insertedData
                    .SelectMany(x => x.Templates)
                    .ToList();

                actual.ShouldNotBeEmpty();
                int expectedCount = _insertedData.Sum(x => x.Templates.Count);
                actual.Count.ShouldEqual(expectedCount);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    DispatchTemplateLong actualItem = actual[i];
                    DispatchTemplate<long> mappedActualItem = mapper.Map<DispatchTemplate<long>>(actualItem);
                    DispatchTemplate<long> expectedItem = expected[i];

                    expectedItem.ComposerSettingsId = mappedActualItem.ComposerSettingsId;
                    expectedItem.DispatchTemplateId = mappedActualItem.DispatchTemplateId;

                    mappedActualItem.ShouldLookLike(expectedItem);
                }
            }
        }



    }
}
