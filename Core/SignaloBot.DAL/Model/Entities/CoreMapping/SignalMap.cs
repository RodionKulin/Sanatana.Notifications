using SignaloBot.DAL.Entities.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.CoreMapping
{
    internal class SignalMap: EntityTypeConfiguration<Signal>
    {
        public SignalMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => t.SignalID);
            this.Property(t => t.SignalID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Properties
            this.Property(t => t.ReceiverAddress).IsRequired();
            this.Property(t => t.SenderAddress).IsRequired();
            this.Property(t => t.SenderDisplayName);    
        
            this.Property(t => t.MessageBody).IsRequired(); 
                       
            this.Property(t => t.SendDateUtc).HasColumnType("datetime2");


            // Table & Column Mappings
            this.ToTable(prefix + "Signals");
          
        }
    }
}
