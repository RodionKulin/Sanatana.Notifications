using NUnit.Framework;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.StructureMap;
using System;
using System.Collections.Generic;
using Sanatana.MongoDb;
using MongoDB.Bson;
using Sanatana.Notifications.DAL.Parameters;
using System.Linq;
using FluentAssertions;
using Sanatana.MongoDb.Extensions;
using SpecsFor.Core;
using Moq;
using Sanatana.Notifications.DAL.MongoDb.Queries;
using Sanatana.DataGenerator.Storages;
using Sanatana.Notifications.DAL.Entities;
using MongoDB.Driver;
using Should;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.Specs
{
    public class MongoDbSignalDispatchQueriesSpecs
    {
        [TestFixture]
        public class when_selecting_dispatches_with_set_lock_except_consolidated
           : SpecsFor<MongoDbSignalDispatchQueries>, INeedDispatchesData
        {
            public InMemoryStorage DispatchesGenerated { get; set; }

            List<SignalDispatch<ObjectId>> _dispatches;

            protected override void When()
            {
                List<int> deliveryTypes = new List<int> { 1, 2, 3 };
                Guid lockId = new Guid("36a77316-41e0-4732-9309-2f1bcac9efbf");
                DateTime expirationBoundary = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2));
                _dispatches = SUT.SelectWithSetLock(new DispatchQueryParameters<ObjectId>
                {
                    Count = 10,
                    ActiveDeliveryTypes = deliveryTypes,
                    MaxFailedAttempts = 3,
                    ExcludeIds = new ObjectId[0],
                    ExcludeConsolidated = new ConsolidationLock<ObjectId>[]
                    {
                        new ConsolidationLock<ObjectId>
                        {
                            ReceiverSubscriberId = ObjectId.GenerateNewId(),
                            CategoryId = 3,
                            DeliveryType = 33
                        },
                        new ConsolidationLock<ObjectId>
                        {
                            ReceiverSubscriberId = ObjectId.GenerateNewId(),
                            CategoryId = 4,
                            DeliveryType = 44
                        }
                    }
                }, lockId, expirationBoundary).Result;
            }

            [Test]
            public void then_dispatches_are_returned()
            {
                _dispatches.Should().NotBeNull();
            }
        }

        [TestFixture]
        public class when_updating_dispatches
           : SpecsFor<MongoDbSignalDispatchQueries>, INeedDispatchesData
        {
            public InMemoryStorage DispatchesGenerated { get; set; }

            [Test]
            public void then_dispatches_are_updated()
            {
                var items = DispatchesGenerated.GetList<SignalDispatch<ObjectId>>()
                    .Take(2)
                    .ToList();
                items.Add(new SignalDispatch<ObjectId>());

                SUT.UpdateSendResults(items).Wait();
            }
        }

        [TestFixture]
        public class when_set_lock
           : SpecsFor<MongoDbSignalDispatchQueries>, INeedDispatchesData
        {
            public InMemoryStorage DispatchesGenerated { get; set; }

            [Test]
            public void then_dispatches_are_updated()
            {
                var dispatchIds = DispatchesGenerated.GetList<SignalDispatch<ObjectId>>()
                    .Take(2)
                    .Select(x => x.SignalDispatchId)
                    .ToList(); ;

                DateTime previousLockExpirationTime = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30));
                bool lockSet = SUT.SetLock(dispatchIds, Guid.NewGuid(), DateTime.UtcNow, previousLockExpirationTime).Result;
            }
        }

        [TestFixture]
        public class when_select_with_lock : SpecsFor<MongoDbSignalDispatchQueries>, INeedDispatchesData, INeedDbContext
        {
            public InMemoryStorage DispatchesGenerated { get; set; }
            public SpecsDbContext DbContext { get; set; }

            private List<SignalDispatch<ObjectId>> _dispatches;
            private Guid _lockedBy;
            private DateTime _lockedSinceUtc = DateTime.UtcNow;

            protected override void When()
            {
                var dispatchIds = DispatchesGenerated.GetList<SignalDispatch<ObjectId>>()
                    .Take(2)
                    .Select(x => x.SignalDispatchId)
                    .ToList(); ;

                List<int> deliveryTypes = new List<int> { 1 };
                DateTime previousLockExpirationTime = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30));
                _lockedBy = Guid.NewGuid();
                _lockedSinceUtc = DateTime.UtcNow;

                _dispatches = SUT
                    .SelectWithSetLock(new DispatchQueryParameters<ObjectId>
                    {
                        Count = 10,
                        ActiveDeliveryTypes = deliveryTypes,
                        MaxFailedAttempts = 3,
                        ExcludeIds = new ObjectId[0],
                        ExcludeConsolidated = new ConsolidationLock<ObjectId>[0]
                    }, _lockedBy, previousLockExpirationTime)
                    .Result;
            }

            [Test]
            public void then_dispatches_are_selected_and_locked()
            {
                _dispatches.Should().NotBeNull();

                ObjectId[] dispatchIds = _dispatches.Select(x => x.SignalDispatchId).ToArray();
                List<SignalDispatch<ObjectId>> storedDispatches = DbContext.SignalDispatches
                    .Find(x => dispatchIds.Contains(x.SignalDispatchId))
                    .ToList();

                storedDispatches.Should().AllBeEquivalentTo(new
                {
                    LockedBy = _lockedBy
                });
                storedDispatches.ForEach(
                    d => d.LockedSinceUtc.Should().BeCloseTo(_lockedSinceUtc, 100));
            }
        }

        [TestFixture]
        public class when_select_with_lock_occupied_for_single_dispatch
           : SpecsFor<MongoDbSignalDispatchQueries>, INeedDispatchesData
        {
            //fields
            private int _count = 10;
            private int _maxFailedAttempts = 3;
            private List<int> _deliveryTypes = new List<int> { 1 };
            private ObjectId[] _excludeIds = new ObjectId[0];
            private Guid _lockedBy = Guid.NewGuid();
            private DateTime _lockExpirationDate = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30));
            private DateTime _lockStartTimeUtc = DateTime.UtcNow;
            private List<SignalDispatch<ObjectId>> _dispatchesReturned;

            //properties
            public InMemoryStorage DispatchesGenerated { get; set; }

            protected override void When()
            {
                List<SignalDispatch<ObjectId>> selected = SUT.SelectUnlocked(new DispatchQueryParameters<ObjectId>
                {
                    Count = _count,
                    ActiveDeliveryTypes = _deliveryTypes,
                    MaxFailedAttempts = _maxFailedAttempts,
                    ExcludeIds = _excludeIds,
                    ExcludeConsolidated = new ConsolidationLock<ObjectId>[0]
                }, _lockExpirationDate).Result;
                if (selected.Count == 0)
                {
                    throw new Exception("No unlocked Dispatches found in database");
                }

                //experiment start - lock with another lock during selection
                //to check if other dispatches will be locked
                Guid anotherLock = Guid.NewGuid();
                SUT.SetLock(new List<ObjectId> { selected[0].SignalDispatchId },
                    anotherLock, DateTime.UtcNow, _lockExpirationDate).Wait();
                //experiment end

                List<ObjectId> dispatchIds = selected.Select(x => x.SignalDispatchId).ToList();
                bool lockSetOnAllEntities = SUT.SetLock(dispatchIds, _lockedBy, _lockStartTimeUtc, _lockExpirationDate).Result;
                
                _dispatchesReturned = SUT
                    .SelectLocked(new DispatchQueryParameters<ObjectId>
                    {
                        Count = _count,
                        ActiveDeliveryTypes = _deliveryTypes,
                        MaxFailedAttempts = _maxFailedAttempts,
                        ExcludeIds = _excludeIds,
                        ExcludeConsolidated = new ConsolidationLock<ObjectId>[0]
                    }, _lockedBy, _lockExpirationDate)
                    .Result;
            }

            [Test]
            public void then_dispatches_are_returned_except_occupied()
            {
                _dispatchesReturned.Should().HaveCount(_count - 1);
            }

            [Test]
            public void then_dispatches_are_locked()
            {
                _dispatchesReturned.Should().AllBeEquivalentTo(new
                {
                    LockedBy = _lockedBy
                });
                _dispatchesReturned.ForEach(
                    d => d.LockedSinceUtc.Should().BeCloseTo(_lockStartTimeUtc, 100));
            }
        }
    }
}
