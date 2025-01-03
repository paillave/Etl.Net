namespace Paillave.EntityFrameworkCoreExtension.Core
{
    public interface IMultiTenantEntity
    {
        int TenantId { get; set; }
    }
}
