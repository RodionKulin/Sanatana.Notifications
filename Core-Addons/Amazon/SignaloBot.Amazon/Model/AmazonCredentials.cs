using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon
{
    public class AmazonCredentials
    {
        //свойства
        public string AwsAccessKey { get; set; }
        public string AwsSecretKey { get; set; }
        public global::Amazon.RegionEndpoint RegionEndpoint { get; set; }



        //инициализация
        public AmazonCredentials()
        {

        }

        public AmazonCredentials(string regionEndpoint)
        {
            if (string.IsNullOrEmpty(regionEndpoint))
                throw new NullReferenceException("Amazon region endpoint не может быть пустым.");

            RegionEndpoint = global::Amazon.RegionEndpoint.GetBySystemName(regionEndpoint);


            bool isUnknownRegion = AmazonConstansts.UNKNOWN_CREDENTIALS_REGION.Equals(RegionEndpoint.DisplayName
                , StringComparison.InvariantCultureIgnoreCase);

            if (isUnknownRegion)
            {
                string comment = string.Format("Amazon region с именем {0} не обнаружен.", regionEndpoint);
                throw new KeyNotFoundException(comment);
            }
        }

        public AmazonCredentials(string regionEndpoint, string awsAccessKey, string awsSecretKey)
            : this(regionEndpoint)
        {
            AwsAccessKey = awsAccessKey;
            AwsSecretKey = awsSecretKey;
        }
    }
}
