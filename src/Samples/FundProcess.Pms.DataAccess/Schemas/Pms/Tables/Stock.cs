namespace FundProcess.Pms.DataAccess.Schemas.Pms.Tables
{
    public class Stock : Security
    {
        public int? GicsSectorId { get; set; }
        public int? IcbSectorId { get; set; }
    }
}