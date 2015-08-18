using SignaloBot.Amazon;
using SignaloBot.Sender;
using SignaloBot.Sender.Senders.LimitManager;
using SignaloBot.Sender.Senders.LimitManager.JournalStorage;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon.Sender
{
    public class AmazonLimitManager : PeriodLimitManager
    {
        //поля
        AmazonCredentials _credentials;
        ICommonLogger _logger;
        LimitedPeriod _max24HourSend;
        LimitedPeriod _maxSecondSend;

        DateTime _lastQuotaRequestUtc;
        bool _amazonLimitsReceived;


        
        //инициализация
        public AmazonLimitManager(List<LimitedPeriod> limitedPeriods, IJournalStorage journalStorage
            , AmazonCredentials credentials, ICommonLogger logger)
            : base(limitedPeriods, journalStorage)
        {
            _credentials = credentials;
            _logger = logger;

            _max24HourSend = new LimitedPeriod();
            _maxSecondSend = new LimitedPeriod();
            _limitedPeriods.Add(_max24HourSend);
            _limitedPeriods.Add(_maxSecondSend);
        }


        //методы
        public override int GetLimitCapacity()
        {
            CheckAmazonQuota();

            if (!_amazonLimitsReceived)
                return 0;

            return base.GetLimitCapacity();
        }

        public override DateTime GetLimitsEndTimeUtc()
        {
            CheckAmazonQuota();

            if (!_amazonLimitsReceived)
                return DateTime.UtcNow;

            return base.GetLimitsEndTimeUtc();
        }

        protected virtual void CheckAmazonQuota()
        {
            lock (_journalLock)
            {
                //времени с последнего запроса квоты на рассылку
                TimeSpan fromLastQuotaRequest = DateTime.UtcNow - _lastQuotaRequestUtc;

                //пора обновлять сведения о квоте
                bool isTimeToGetQuotaAgain = fromLastQuotaRequest >= AmazonConstansts.LIMITMANAGER_QUOTA_REQUEST_PERIOD;

                //можно повторить запрос, если предыдущий был неудачным
                bool isTimeToRetryFailedRequest = fromLastQuotaRequest >= AmazonConstansts.LIMITMANAGER_FAILED_QUOTA_REQUEST_RETRY_PERIOD;


                if ((_amazonLimitsReceived && isTimeToGetQuotaAgain)
                    || (!_amazonLimitsReceived && isTimeToRetryFailedRequest))
                {
                    _lastQuotaRequestUtc = DateTime.UtcNow;
                    GetAmazonQuota();
                }
            }
        }

        internal virtual void GetAmazonQuota()
        {
            _amazonLimitsReceived = false;

            //запрос квоты на рассылку тоже увеличивает счётчик отправленных сообщений в амазоне
            InsertTime();

            using (var client = new AmazonSimpleEmailServiceClient(_credentials.AwsAccessKey,
                _credentials.AwsSecretKey, _credentials.RegionEndpoint))
            {
                try
                {
                    GetSendQuotaResponse response = client.GetSendQuota();

                    _max24HourSend.Limit = (int)response.Max24HourSend;
                    _max24HourSend.Period = TimeSpan.FromHours(24);

                    _maxSecondSend.Limit = (int)response.MaxSendRate;
                    _maxSecondSend.Period = TimeSpan.FromSeconds(1);

                    _amazonLimitsReceived = true;
                }
                catch (Exception sesException)
                {
                    if (_logger != null)
                    {
                        _logger.Exception(sesException, "Ошибка при получении квоты Amazon SES");
                    }
                }
            }
        }
    }
}
