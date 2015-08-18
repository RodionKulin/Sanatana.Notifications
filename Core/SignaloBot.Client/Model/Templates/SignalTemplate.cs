using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using SignaloBot.Client.Manager;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Entities.Results;

namespace SignaloBot.Client.Templates
{
    public class SignalTemplate
    {
        //свойства  
        public ITemplateProvider SubjectProvider { get; set; }
        public ITemplateTransformer SubjectTransformer { get; set; }
        public ITemplateProvider BodyProvider { get; set; }
        public ITemplateTransformer BodyTransformer { get; set; }
        public int DeliveryType { get; set; }
        public int CategoryID { get; set; }
        public int? TopicID { get; set; }
        public string SenderAddress { get; set; }
        public string SenderDisplayName { get; set; }
        public bool IsBodyHtml { get; set; }

        

        //методы
        public virtual List<Signal> Build(List<Subscriber> subscribers
            , List<TemplateData> bodyData, List<TemplateData> subjectData)
        {
            List<string> bodyList = null;
            if (BodyProvider != null)
            {
                ITemplateTransformer transformer = BodyTransformer ?? new ReplaceTransformer();
                bodyList = transformer.TransformList(BodyProvider, bodyData);
            }

            List<string> subjectList = null;
            if (SubjectProvider != null)
            {
                ITemplateTransformer transformer = SubjectTransformer ?? new ReplaceTransformer();
                List<TemplateData> data = subjectData ?? bodyData.Select(p => TemplateData.Empty()).ToList();
                subjectList = transformer.TransformList(SubjectProvider, data);             
            }

            List<Signal> messageList = new List<Signal>();

            for (int i = 0; i < bodyData.Count; i++)
            {
                var message = new Signal()
                {
                    DeliveryType = DeliveryType,
                    CategoryID = CategoryID,
                    TopicID = TopicID,

                    ReceiverUserID = subscribers[i].UserID,
                    ReceiverAddress = subscribers[i].Address,

                    SenderAddress = SenderAddress,
                    SenderDisplayName = SenderDisplayName,

                    MessageSubject = subjectList == null
                        ? null
                        : subjectList[i],
                    MessageBody = bodyList == null
                        ? null
                        : bodyList[i],
                    IsBodyHtml = IsBodyHtml,

                    SendDateUtc = DateTime.UtcNow,
                    IsDelayed = false,
                    FailedAttempts = 0
                };

                messageList.Add(message);
            }

            return messageList;
        }

        public virtual List<Signal> Build(List<Subscriber> subscribers
            , List<TemplateData> bodyData, TemplateData subjectData = null)
        {
            List<TemplateData> subjectDataList = null;
            if (subjectData != null)
            {
                subjectDataList = new List<TemplateData>();
                for (int i = 0; i < bodyData.Count; i++)
                {
                    subjectDataList.Add(subjectData);
                }
            }

            return Build(subscribers, bodyData, subjectDataList);
        }

        public virtual List<Signal> Build(List<Subscriber> subscribers
            , TemplateData bodyData, TemplateData subjectData = null)
        {
            if(subscribers.Count == 0)
            {
                return new List<Signal>();
            }

            Subscriber subscriber = subscribers.First();
            Signal message = Build(subscriber, bodyData, subjectData);

            List<Signal> resultList = new List<Signal>();
            for (int i = 0; i < subscribers.Count; i++)
            {
                resultList.Add(new Signal()
                {
                    ReceiverUserID = subscribers[i].UserID,
                    ReceiverAddress = subscribers[i].Address,

                    DeliveryType = message.DeliveryType,
                    CategoryID = message.CategoryID,
                    TopicID = message.TopicID,
                    SenderAddress = message.SenderAddress,
                    SenderDisplayName = message.SenderDisplayName,
                    MessageSubject = message.MessageSubject,
                    MessageBody = message.MessageBody,
                    IsBodyHtml = message.IsBodyHtml,
                    SendDateUtc = message.SendDateUtc,
                    IsDelayed = message. IsDelayed,
                    FailedAttempts = message.FailedAttempts
                });
            }

            return resultList;
        }

        public virtual Signal Build(Subscriber subscriber
            , TemplateData bodyData, TemplateData subjectData = null)
        {
            var subscribersList = new List<Subscriber>() { subscriber };
            var bodyDataList = new List<TemplateData>(1) { bodyData };
            var subjectDataList = subjectData == null
                ? null
                : new List<TemplateData>(1) { subjectData };

            List<Signal> messageList = Build(subscribersList, bodyDataList, subjectDataList);
            return messageList[0];
        }
        
    }
}
