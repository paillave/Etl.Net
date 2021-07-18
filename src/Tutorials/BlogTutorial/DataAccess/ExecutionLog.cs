using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogTutorial.DataAccess
{
    public class ExecutionLog
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public Guid ExecutionId { get; set; }
        public string EventType { get; set; }
        public string Message { get; set; }
    }
    public class ExecutionLogConfiguration : IEntityTypeConfiguration<ExecutionLog>
    {
        public void Configure(EntityTypeBuilder<ExecutionLog> builder)
        {
            builder.ToTable(nameof(ExecutionLog));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseIdentityColumn();
            builder.Property(i => i.EventType).HasMaxLength(250).IsRequired();
            builder.Property(i => i.Message).IsRequired();
        }
    }
}