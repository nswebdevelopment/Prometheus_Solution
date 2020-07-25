using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Dal.Entities
{
    public interface IPrometheusEntities
    {
        DbSet<Account> Account { get; set; }
        DbSet<Adapter> Adapter { get; set; }
        DbSet<AdapterMapping> AdapterMapping { get; set; }
        DbSet<AdapterTypeItem> AdapterTypeItem { get; set; }
        DbSet<AdapterTypeItemProperty> AdapterTypeItemProperty { get; set; }
        DbSet<Block> Block { get; set; }
        DbSet<BlockTransaction> BlockTransaction { get; set; }
        DbSet<BusinessAdapter> BusinessAdapter { get; set; }
        DbSet<BusinessFile> BusinessFile { get; set; }
        DbSet<Company> Company { get; set; }
        DbSet<CryptoAdapter> CryptoAdapter { get; set; }
        DbSet<CryptoAdapterProperty> CryptoAdapterProperty { get; set; }
        DbSet<EnterpriseAdapter> EnterpriseAdapter { get; set; }
        DbSet<EnterpriseAdapterTable> EnterpriseAdapterTable { get; set; }
        DbSet<EnterpriseAdapterTableColumn> EnterpriseAdapterTableColumn { get; set; }
        DbSet<EnterpriseAdapterProperty> EnterpriseAdapterProperty { get; set; }
        DbSet<Exchange> Exchange { get; set; }
        DbSet<JobDefinition> JobDefinition { get; set; }
        DbSet<JobDefinitionAdapterTypeItemProperty> JobDefinitionAdapterTypeItemProperty { get; set; }
        DbSet<JobDefinitionProperty> JobDefinitionProperty { get; set; }
        DbSet<JobHistory> JobHistory { get; set; }
        DbSet<JobStatus> JobStatus { get; set; }
        DbSet<JobTimeline> JobTimeline { get; set; }
        DbSet<Market> Market { get; set; }
        DbSet<Notification> Notification { get; set; }
        DbSet<Property> Property { get; set; }
        DbSet<Schedule> Schedule { get; set; }
        DbSet<Status> Status { get; set; }
        DbSet<Symbol> Symbol { get; set; }
        DbSet<Transaction> Transaction { get; set; }
        DbSet<TransactionStatus> TransactionStatus { get; set; }
        DbSet<TransactionType> TransactionType { get; set; }
        DbSet<TransactionTypeAlias> TransactionTypeAlias { get; set; }
        DbSet<UserProfile> UserProfile { get; set; }
        int SaveChanges();
    }
}
