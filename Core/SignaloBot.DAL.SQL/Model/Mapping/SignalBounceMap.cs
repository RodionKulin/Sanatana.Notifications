using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SignaloBot.DAL.SQL
{
    internal class SignalBounceMap : EntityTypeConfiguration<SignalBounceGuid>
    {
        public SignalBounceMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => t.SignalBounceID);

            // Properties
            this.Property(t => t.ReceiverAddress).IsRequired();
            this.Property(t => t.BounceReceiveDateUtc).HasColumnType("datetime2");
            this.Property(t => t.SendDateUtc).HasColumnType("datetime2");
            this.Property(t => t.BounceDetailsXML).HasColumnType("xml");


            // Table & Column Mappings
            this.ToTable(prefix + "SignalBounces");
        }
    }
}
