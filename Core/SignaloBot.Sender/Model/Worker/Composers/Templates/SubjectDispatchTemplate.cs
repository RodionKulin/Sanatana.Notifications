using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using SignaloBot.DAL;

namespace SignaloBot.Sender.Composers.Templates
{
    public class SubjectDispatchTemplate<TKey> : SignalTemplateBase<TKey>
        where TKey : struct
    {
        //свойства  
        public virtual ITemplateProvider SubjectProvider { get; set; }
        public virtual ITemplateTransformer SubjectTransformer { get; set; }
        public virtual ITemplateProvider BodyProvider { get; set; }
        public virtual ITemplateTransformer BodyTransformer { get; set; }
        public override int DeliveryType { get; set; }
        public virtual int CategoryID { get; set; }
        public virtual string TopicID { get; set; }
        public virtual string SenderAddress { get; set; }
        public virtual string SenderDisplayName { get; set; }
        public virtual bool IsBodyHtml { get; set; }



        //методы
        public override List<SignalDispatchBase<TKey>> Build(
            List<Subscriber<TKey>> subscribers, Dictionary<string, string> data)
        {
            TemplateData bodyData = new TemplateData(data);
            TemplateData subjectData = new TemplateData(data);
            return Build(subscribers, bodyData, subjectData);
        }

        public virtual List<SignalDispatchBase<TKey>> Build(List<Subscriber<TKey>> subscribers
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

            var list = new List<SignalDispatchBase<TKey>>();
            
            for (int i = 0; i < bodyData.Count; i++)
            {
                list.Add(new SubjectDispatch<TKey>()
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
                });
            }

            return list;
        }

        public virtual List<SignalDispatchBase<TKey>> Build(List<Subscriber<TKey>> subscribers
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

            return Build(subscribers.ToList(), bodyData, subjectDataList);
        }

        public virtual List<SignalDispatchBase<TKey>> Build(List<Subscriber<TKey>> subscribers
            , TemplateData bodyData, TemplateData subjectData = null)
        {
            if(subscribers.Count() == 0)
            {
                return new List<SignalDispatchBase<TKey>>();
            }

            Subscriber<TKey> firstSubscriber = subscribers.First();
            SubjectDispatch<TKey> item = (SubjectDispatch<TKey>)Build(firstSubscriber, bodyData, subjectData);

            var list = new List<SignalDispatchBase<TKey>>();

            foreach (Subscriber<TKey> subscriber in subscribers)
            {
                list.Add(new SubjectDispatch<TKey>()
                {
                    ReceiverUserID = subscriber.UserID,
                    ReceiverAddress = subscriber.Address,

                    DeliveryType = item.DeliveryType,
                    CategoryID = item.CategoryID,
                    TopicID = item.TopicID,
                    SenderAddress = item.SenderAddress,
                    SenderDisplayName = item.SenderDisplayName,
                    MessageSubject = item.MessageSubject,
                    MessageBody = item.MessageBody,
                    IsBodyHtml = item.IsBodyHtml,
                    SendDateUtc = item.SendDateUtc,
                    IsDelayed = item.IsDelayed,
                    FailedAttempts = item.FailedAttempts
                });
            }

            return list;
        }

        public virtual SignalDispatchBase<TKey> Build(Subscriber<TKey> subscriber
            , TemplateData bodyData, TemplateData subjectData = null)
        {
            var subscribersList = new List<Subscriber<TKey>>() { subscriber };
            var bodyDataList = new List<TemplateData>(1) { bodyData };
            var subjectDataList = subjectData == null
                ? null
                : new List<TemplateData>(1) { subjectData };

            List<SignalDispatchBase<TKey>> list = Build(subscribersList, bodyDataList, subjectDataList);
            return list[0];
        }
        
    }
}
