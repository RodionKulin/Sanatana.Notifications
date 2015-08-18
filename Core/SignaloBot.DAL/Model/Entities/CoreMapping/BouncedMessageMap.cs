using SignaloBot.DAL.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SignaloBot.DAL.Entities.CoreMapping
{
    internal class BouncedMessageMap : EntityTypeConfiguration<BouncedMessage>
    {
        public BouncedMessageMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => t.BouncedMessageID);

            // Properties
            this.Property(t => t.ReceiverAddress).IsRequired();
            this.Property(t => t.BounceReceiveDateUtc).HasColumnType("datetime2");
            this.Property(t => t.SendDateUtc).HasColumnType("datetime2");
            this.Property(t => t.BounceDetailsXML).HasColumnType("xml");


            // Table & Column Mappings
            this.ToTable(prefix + "BouncedMessages");
        }
    }
}
