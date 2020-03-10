using Sanatana.Notifications;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DispatchHandling.Limits;
using Sanatana.Notifications.DispatchHandling.Limits.JournalStorage;
using Microsoft.Extensions.Logging;

namespace Sanatana.Notifications.Dispatchers.AWS_SES
{
    public class AmazonLimitManager : PeriodLimitCounter
    {
        //fields
        protected AmazonCredentials _credentials;
        protected ILogger _logger;
        protected LimitedPeriod _max24HourSend;
        protected LimitedPeriod _maxSecondSend;

        protected DateTime _lastQuotaRequestUtc;
        protected bool _amazonLimitsReceived;


        
        //init
        public AmazonLimitManager(List<LimitedPeriod> limitedPeriods, IJournalStorage journalStorage
            , AmazonCredentials credentials, ILogger logger)
            : base(limitedPeriods, journalStorage)
        {
            _credentials = credentials;
            _logger = logger;

            _max24HourSend = new LimitedPeriod();
            _maxSecondSend = new LimitedPeriod();
            _limitedPeriods.Add(_max24HourSend);
            _limitedPeriods.Add(_maxSecondSend);
        }


        //methods
        public override int GetLimitCapacity()
        {
            CheckAmazonQuota();

            if (!_amazonLimitsReceived)
                return 0;

            return base.GetLimitCapacity();
        }

        public override DateTime? GetLimitsEndTimeUtc()
        {
            CheckAmazonQuota();

            if (!_amazonLimitsReceived)
                return null;

            return base.GetLimitsEndTimeUtc();
        }

        protected virtual void CheckAmazonQuota()
        {
            lock (_journalLock)
            {
                TimeSpan fromLastQuotaRequest = DateTime.UtcNow - _lastQuotaRequestUtc;
                
                bool isTimeToGetQuotaAgain = fromLastQuotaRequest >= AmazonConstansts.LIMITMANAGER_QUOTA_REQUEST_PERIOD;
                
                bool isTimeToRetryFailedRequest = fromLastQuotaRequest >= AmazonConstansts.LIMITMANAGER_FAILED_QUOTA_REQUEST_RETRY_PERIOD;


                if ((_amazonLimitsReceived && isTimeToGetQuotaAgain)
                    || (!_amazonLimitsReceived && isTimeToRetryFailedRequest))
                {
                    _lastQuotaRequestUtc = DateTime.UtcNow;
                    GetAmazonQuota();
                }
            }
        }

        protected virtual void GetAmazonQuota()
        {
            _amazonLimitsReceived = false;

            //quota request also increments messages sent counter
            InsertTime();
            
            using (var client = new AmazonSimpleEmailServiceClient(_credentials.AwsAccessKey,
                _credentials.AwsSecretKey, _credentials.RegionEndpoint))
            {
                try
                {
                    GetSendQuotaResponse response = client.GetSendQuotaAsync().Result;

                    _max24HourSend.Limit = (int)response.Max24HourSend;
                    _max24HourSend.Period = TimeSpan.FromHours(24);

                    _maxSecondSend.Limit = (int)response.MaxSendRate;
                    _maxSecondSend.Period = TimeSpan.FromSeconds(1);

                    _amazonLimitsReceived = true;
                }
                catch (Exception sesException)
                {
                    _logger.LogError(sesException, "Exceptions while AWS SES quota request");
                }
            }
        }
    }
}
