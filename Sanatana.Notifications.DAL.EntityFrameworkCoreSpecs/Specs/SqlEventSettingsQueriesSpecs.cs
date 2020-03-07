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
using Sanatana.Notifications.DeliveryTypes.Email;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Composing.Templates;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using AutoMapper;
using SpecsFor.StructureMap;
using FluentAssertions;
using SpecsFor.Core;
using Moq;
using StructureMap;
using Sanatana.Notifications.DAL.Interfaces;
using StructureMap.AutoMocking;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.Queries
{
    public class SqlEventSettingsQueriesSpecs
    {

        [TestFixture]
        public class when_event_settings_insert_using_ef
           : SpecsFor<SqlEventSettingsQueries>, INeedDbContext
        {
            private int _categoryId = 1;
            private List<EventSettings<long>> _insertedData;
            public SenderDbContext DbContext { get; set; }



            private List<EventSettings<long>> GetEventSettings()
            {
                return new List<EventSettings<long>>()
                {
                    new EventSettings<long>()
                    {
                        CategoryId = _categoryId,
                        Subscription = new SubscriptionParameters()
                        {
                            CategoryId = _categoryId
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
                    new EventSettings<long>()
                    {
                        CategoryId = _categoryId,
                        Subscription = new SubscriptionParameters()
                        {
                            CategoryId = _categoryId
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
                _insertedData = GetEventSettings();

                SUT.Insert(_insertedData).Wait();
            }


            [Test]
            public void then_event_settings_inserted_are_found_using_ef()
            {
                List<EventSettingsLong> actual = DbContext.EventSettings
                    .Where(x => x.CategoryId == _categoryId)
                    .OrderBy(x => x.EventSettingsId)
                    .ToList();

                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(_insertedData.Count);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    EventSettingsLong actualItem = actual[i];
                    EventSettings<long> expectedItem = _insertedData[i];

                    expectedItem.EventSettingsId = actualItem.EventSettingsId;
                    actualItem.Templates = expectedItem.Templates;

                    actualItem.Should().BeEquivalentTo(expectedItem);
                }
            }

            [Test]
            public void then_dispatch_templates_inserted_are_found_using_ef()
            {
                INotificationsMapperFactory mapperFactory = Mocker.GetServiceInstance<INotificationsMapperFactory>();
                IMapper mapper = mapperFactory.GetMapper();

                List<DispatchTemplateLong> actual = DbContext.DispatchTemplates
                   .OrderBy(x => x.EventSettingsId)
                   .ThenBy(x => x.DispatchTemplateId)
                   .ToList();

                List<DispatchTemplate<long>> expected = _insertedData
                    .SelectMany(x => x.Templates)
                    .ToList();

                actual.Should().NotBeEmpty();
                int expectedCount = _insertedData.Sum(x => x.Templates.Count);
                actual.Count.Should().Be(expectedCount);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    DispatchTemplateLong actualItem = actual[i];
                    DispatchTemplate<long> mappedActualItem = mapper.Map<DispatchTemplate<long>>(actualItem);
                    DispatchTemplate<long> expectedItem = expected[i];

                    expectedItem.EventSettingsId = mappedActualItem.EventSettingsId;
                    expectedItem.DispatchTemplateId = mappedActualItem.DispatchTemplateId;

                    mappedActualItem.Should().BeEquivalentTo(expectedItem);
                }
            }
        }


        [TestFixture]
        public class when_event_settings_update_using_ef
           : SpecsFor<SqlEventSettingsQueries>, INeedDbContext
        {
            private List<EventSettingsLong> _insertedSettings;
            private List<EmailDispatchTemplate<long>> _insertedTemplates;
            private int _categoryId = 2;
            public SenderDbContext DbContext { get; set; }


            private List<EventSettingsLong> GetEventSettings()
            {
                return new List<EventSettingsLong>()
                {
                    new EventSettingsLong()
                    {
                        CategoryId = _categoryId,
                        Subscription = new SubscriptionParameters()
                        {
                            CategoryId = _categoryId
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
                    new EventSettingsLong()
                    {
                        CategoryId = _categoryId,
                        Subscription = new SubscriptionParameters()
                        {
                            CategoryId = _categoryId
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

            protected override void Given()
            {
                _insertedSettings = GetEventSettings();
                _insertedTemplates = _insertedSettings
                    .SelectMany(x => x.Templates)
                    .Cast<EmailDispatchTemplate<long>>()
                    .ToList();

                var insertedItems = _insertedSettings.Cast<EventSettings<long>>().ToList();
                SUT.Insert(insertedItems).Wait();
            }

            protected override void When()
            {
                foreach (EventSettingsLong item in _insertedSettings)
                {
                    item.DisplayName = "Updated name";
                    item.Subscription.CategoryId = 101;
                    item.Templates.ForEach(template =>
                    {
                        EmailDispatchTemplate<long> emailTemplate = (EmailDispatchTemplate<long>)template;
                        emailTemplate.IsBodyHtml = true;
                    });
                }

                var updatedItems = _insertedSettings.Cast<EventSettings<long>>().ToList();
                SUT.Update(updatedItems).Wait();
            }

            [Test]
            public void then_event_settings_updated_are_found_using_ef()
            {
                List<EventSettingsLong> actual = DbContext.EventSettings
                    .Where(x => x.CategoryId == _categoryId)
                    .OrderBy(x => x.EventSettingsId)
                    .ToList();

                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(_insertedSettings.Count);

                for (int i = 0; i < _insertedSettings.Count; i++)
                {
                    EventSettings<long> expectedItem = _insertedSettings[i];
                    EventSettingsLong actualItem = actual[i];

                    expectedItem.EventSettingsId = actualItem.EventSettingsId;
                    expectedItem.Templates = null;

                    actualItem.Should().BeEquivalentTo(expectedItem);
                }
            }

            [Test]
            public void then_dispatch_templates_updated_are_found_using_ef()
            {
                List<long> templateIds = _insertedTemplates.Select(x => x.DispatchTemplateId).ToList();
                List<DispatchTemplateLong> actualList = DbContext.DispatchTemplates
                   .Where(x => templateIds.Contains(x.DispatchTemplateId))
                   .OrderBy(x => x.EventSettingsId)
                   .ThenBy(x => x.DispatchTemplateId)
                   .ToList();

                actualList.Should().NotBeEmpty();
                actualList.Count.Should().Be(_insertedTemplates.Count);

                
                INotificationsMapperFactory mapperFactory = Mocker.GetServiceInstance<INotificationsMapperFactory>();
                IMapper mapper = mapperFactory.GetMapper();

                for (int i = 0; i < _insertedSettings.Count; i++)
                {
                    DispatchTemplateLong actualItem = actualList[i];
                    DispatchTemplate<long> mappedActualItem = mapper.Map<DispatchTemplate<long>>(actualItem);
                    DispatchTemplate<long> expectedItem = _insertedTemplates[i];
                    
                    mappedActualItem.Should().BeEquivalentTo(expectedItem);
                }
            }
        }


        [TestFixture]
        public class when_event_settings_delete_using_ef
           : SpecsFor<SqlEventSettingsQueries>, INeedDbContext
        {
            private List<EventSettingsLong> _insertedSettings;
            private List<EmailDispatchTemplate<long>> _insertedTemplates;
            private int _categoryId = 3;
            public SenderDbContext DbContext { get; set; }


            private List<EventSettingsLong> GetEventSettings()
            {
                return new List<EventSettingsLong>()
                {
                    new EventSettingsLong()
                    {
                        CategoryId = _categoryId,
                        Subscription = new SubscriptionParameters()
                        {
                            CategoryId = _categoryId
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
                    new EventSettingsLong()
                    {
                        CategoryId = _categoryId,
                        Subscription = new SubscriptionParameters()
                        {
                            CategoryId = _categoryId
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

            protected override void Given()
            {
                _insertedSettings = GetEventSettings();
                _insertedTemplates = _insertedSettings
                    .SelectMany(x => x.Templates)
                    .Cast<EmailDispatchTemplate<long>>()
                    .ToList();

                var insertedItems = _insertedSettings.Cast<EventSettings<long>>().ToList();
                SUT.Insert(insertedItems).Wait();
            }

            protected override void When()
            {
                var deletedItems = _insertedSettings.Cast<EventSettings<long>>().ToList();
                SUT.Delete(deletedItems).Wait();
            }

            [Test]
            public void then_event_settings_deleted_are_not_found_using_ef()
            {
                List<EventSettingsLong> actual = DbContext.EventSettings
                    .Where(x => x.CategoryId == _categoryId)
                    .ToList();

                actual.Count.Should().Be(0);
            }

            [Test]
            public void then_dispatch_templates_updated_are_not_found_using_ef()
            {
                List<long> templateIds = _insertedTemplates.Select(x => x.DispatchTemplateId).ToList();
                List<DispatchTemplateLong> actualList = DbContext.DispatchTemplates
                   .Where(x => templateIds.Contains(x.DispatchTemplateId))
                   .ToList();

                actualList.Count.Should().Be(0);
            }
        }

    }
}
