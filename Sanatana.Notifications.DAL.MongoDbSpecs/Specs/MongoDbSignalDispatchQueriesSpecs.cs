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

namespace Sanatana.Notifications.DAL.MongoDbSpecs.Specs
{
    public class MongoDbSignalDispatchQueriesSpecs
    {

        [TestFixture]
        public class when_selecting_dispatches_with_lock
           : SpecsFor<MongoDbSignalDispatchQueries>, INeedDispatchesData
        {
            public InMemoryStorage DispatchesGenerated { get; set; }

            [Test]
            public void then_dispatches_are_returned()
            {
                List<int> deliveryTypes = new List<int> { 1, 2, 3 };
                Guid lockId = new Guid("36a77316-41e0-4732-9309-2f1bcac9efbf");
                DateTime expirationBoundary = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2));
                var lockedDispatches = SUT.SelectWithSetLock(10, deliveryTypes, 3, new ObjectId[0], lockId, expirationBoundary).Result;

                lockedDispatches.Should().NotBeNull();
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
        public class when_select_with_lock
           : SpecsFor<MongoDbSignalDispatchQueries>, INeedDispatchesData
        {
            public InMemoryStorage DispatchesGenerated { get; set; }

            [Test]
            public void then_dispatches_are_selected_updated()
            {
                var dispatchIds = DispatchesGenerated.GetList<SignalDispatch<ObjectId>>()
                    .Take(2)
                    .Select(x => x.SignalDispatchId)
                    .ToList(); ;

                List<int> deliveryTypes = new List<int> { 1 };
                DateTime previousLockExpirationTime = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30));
                Guid lockId = Guid.NewGuid();

                List<SignalDispatch<ObjectId>> dispatches = SUT
                    .SelectWithSetLock(10, deliveryTypes, 3, new ObjectId[0], lockId, previousLockExpirationTime)
                    .Result;
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
            private Guid _lockId = Guid.NewGuid();
            private DateTime _lockExpirationDate = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30));
            private DateTime _lockStartTimeUtc = DateTime.UtcNow;
            private List<SignalDispatch<ObjectId>> _dispatchesReturned;

            //properties
            public InMemoryStorage DispatchesGenerated { get; set; }

            protected override void When()
            {
                List<SignalDispatch<ObjectId>> selected = SUT.SelectUnlocked(_count, _deliveryTypes, _maxFailedAttempts, _excludeIds, _lockExpirationDate).Result;
                if (selected.Count == 0)
                {
                    throw new Exception("No unlocked Dispatches found in database");
                }

                //experiment start - lock with another lock
                //to check if other dispatches will be locked
                Guid anotherLock = Guid.NewGuid();
                SUT.SetLock(new List<ObjectId> { selected[0].SignalDispatchId },
                    anotherLock, DateTime.UtcNow, _lockExpirationDate).Wait();
                //experiment end

                List<ObjectId> dispatchIds = selected.Select(x => x.SignalDispatchId).ToList();
                bool lockSetOnAllEntities = SUT.SetLock(dispatchIds, _lockId, _lockStartTimeUtc, _lockExpirationDate).Result;
                
                _dispatchesReturned = SUT
                    .SelectLocked(_count, _deliveryTypes, _maxFailedAttempts, _excludeIds, _lockId, _lockExpirationDate)
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
                    LockedBy = _lockId
                });
                _dispatchesReturned.ForEach(d => d.LockedDateUtc.Should().BeCloseTo(_lockStartTimeUtc, 100));
            }
        }
    }
}
