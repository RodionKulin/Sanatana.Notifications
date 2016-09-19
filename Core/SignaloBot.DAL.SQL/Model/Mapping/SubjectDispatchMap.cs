using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    internal class SubjectDispatchMap : EntityTypeConfiguration<SubjectDispatchGuid>
    {
        public SubjectDispatchMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => t.SignalDispatchID);
            this.Property(t => t.SignalDispatchID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Properties
            this.Property(t => t.ReceiverAddress).IsRequired();
            this.Property(t => t.SenderAddress).IsRequired();
            this.Property(t => t.SenderDisplayName).HasMaxLength(4000);

            this.Property(t => t.MessageBody).IsRequired(); 
                       
            this.Property(t => t.SendDateUtc).HasColumnType("datetime2");


            // Table & Column Mappings
            this.ToTable(prefix + "SubjectDispatches");
          
        }
    }
}
