using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.Notifications;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Shouldly;
using Shouldly.ShouldlyExtensionMethods;


namespace Sanatana.NotificationsTests.DAL.Queries.Temporary
{
    [TestClass()]
    public class TemporaryStorageTests
    {
        //fields
        TemporaryStorage<SignalEvent<long>> _tempStorage;

        //init
        public TemporaryStorageTests()
        {
            var fileRepository = new FileRepository();
            _tempStorage = new TemporaryStorage<SignalEvent<long>>(fileRepository);

            string storageFolder = _tempStorage.GetStorageFolder();
            DirectoryInfo directory = new DirectoryInfo(storageFolder);
            if (directory.Exists)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
            }
        }


        [TestMethod()]
        public void TemporaryStorage_InsertTest()
        {
            var temporaryStorageParameters = new TemporaryStorageParameters()
            {
                QueueType = NotificationsConstants.TS_DISPATCH_QUEUE_KEY,
                EntityVersion = NotificationsConstants.TS_ENTITIES_VERSION
            };
            var signalEvent = new SignalEvent<long>
            {
                DataKeyValues = new Dictionary<string, string>
                {
                    { "customer", "Crabs" }
                },
                PredefinedAddresses = new List<DeliveryAddress>
                {
                    new DeliveryAddress
                    {
                        Address = "address",
                        DeliveryType = 1,
                        Language = "lang"
                    }
                }
            };
            Guid id = Guid.NewGuid();

            _tempStorage.Insert(temporaryStorageParameters, id, signalEvent);

            TemporaryStorage_SelectTest(id, signalEvent);
        }
        
        private void TemporaryStorage_SelectTest(Guid id, SignalEvent<long> expectedSignalEvent)
        {
            var temporaryStorageParameters = new TemporaryStorageParameters()
            {
                QueueType = NotificationsConstants.TS_DISPATCH_QUEUE_KEY,
                EntityVersion = NotificationsConstants.TS_ENTITIES_VERSION
            };

            Dictionary<Guid, SignalEvent<long>> eventsList = _tempStorage
                .Select(temporaryStorageParameters);
            SignalEvent<long> actualEvent = eventsList[id];
            actualEvent.DataKeyValues["customer"].ShouldBe("Crabs");
            actualEvent.PredefinedAddresses[0].Address.ShouldBe("address");
        }
    }
}
