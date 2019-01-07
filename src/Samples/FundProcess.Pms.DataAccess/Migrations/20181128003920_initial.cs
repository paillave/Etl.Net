using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Entity");

            migrationBuilder.EnsureSchema(
                name: "Fee");

            migrationBuilder.EnsureSchema(
                name: "Pms");

            migrationBuilder.EnsureSchema(
                name: "UserAccounts");

            migrationBuilder.CreateTable(
                name: "EntityGroup",
                schema: "Entity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvestorRelationship",
                schema: "Entity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorRelationship", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProviderSecurity",
                schema: "Pms",
                columns: table => new
                {
                    SecurityId = table.Column<int>(nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    DataProvider = table.Column<string>(maxLength: 50, nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProviderSecurity", x => new { x.SecurityId, x.DataProvider });
                });

            migrationBuilder.CreateTable(
                name: "SecurityGroup",
                schema: "Pms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityBase",
                schema: "Entity",
                columns: table => new
                {
                    BelongsToEntityGroupId = table.Column<int>(nullable: true),
                    BelongsToEntityId = table.Column<int>(nullable: true),
                    EntityGroupId = table.Column<int>(nullable: true),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    Email = table.Column<string>(maxLength: 250, nullable: true),
                    Address = table.Column<string>(nullable: true),
                    CountryCode = table.Column<string>(maxLength: 2, nullable: true),
                    PhoneNumber = table.Column<string>(maxLength: 50, nullable: true),
                    ConnectionString = table.Column<string>(maxLength: 512, nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    ContactId = table.Column<int>(nullable: true),
                    VAT = table.Column<decimal>(nullable: true),
                    Url = table.Column<string>(maxLength: 500, nullable: true),
                    RegistrationNumber = table.Column<string>(maxLength: 50, nullable: true),
                    Regulated = table.Column<bool>(nullable: true),
                    CssfEquivalentSupervision = table.Column<bool>(nullable: true),
                    Type = table.Column<int>(nullable: true),
                    CollectiveManagement = table.Column<bool>(nullable: true),
                    DiscretionaryManagement = table.Column<bool>(nullable: true),
                    Aifm = table.Column<bool>(nullable: true),
                    FirstName = table.Column<string>(maxLength: 50, nullable: true),
                    LastName = table.Column<string>(maxLength: 50, nullable: true),
                    MobileNumber = table.Column<string>(maxLength: 50, nullable: true),
                    IdCardNumber = table.Column<string>(maxLength: 50, nullable: true),
                    PassportNumber = table.Column<string>(maxLength: 50, nullable: true),
                    Culture = table.Column<string>(maxLength: 5, nullable: true, defaultValue: "en-GB")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityBase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityBase_EntityBase_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityBase_EntityGroup_EntityGroupId",
                        column: x => x.EntityGroupId,
                        principalSchema: "Entity",
                        principalTable: "EntityGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Investor",
                schema: "Entity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EntityId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    IntermediaryId = table.Column<int>(nullable: true),
                    InternalResponsibleId = table.Column<int>(nullable: true),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Investor_EntityBase_EntityId",
                        column: x => x.EntityId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Investor_EntityBase_IntermediaryId",
                        column: x => x.IntermediaryId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Investor_EntityBase_InternalResponsibleId",
                        column: x => x.InternalResponsibleId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonToCompany",
                schema: "Entity",
                columns: table => new
                {
                    PersonId = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonToCompany", x => new { x.CompanyId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_PersonToCompany_EntityBase_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonToCompany_EntityBase_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sicav",
                schema: "Pms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SicavName = table.Column<string>(maxLength: 255, nullable: true),
                    ProspectusId = table.Column<int>(nullable: true),
                    LegalStructure = table.Column<string>(nullable: true),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sicav", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sicav_EntityBase_BelongsToEntityId",
                        column: x => x.BelongsToEntityId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserEntityRole",
                schema: "UserAccounts",
                columns: table => new
                {
                    CompanyId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ApplicationRole = table.Column<string>(maxLength: 50, nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEntityRole", x => new { x.UserId, x.CompanyId, x.ApplicationRole });
                    table.ForeignKey(
                        name: "FK_UserEntityRole_EntityBase_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserEntityRole_EntityBase_UserId",
                        column: x => x.UserId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogin",
                schema: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Subject = table.Column<string>(maxLength: 255, nullable: false),
                    IdentityProvider = table.Column<string>(maxLength: 255, nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLogin_EntityBase_UserId",
                        column: x => x.UserId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestorRelationshipItem",
                schema: "Entity",
                columns: table => new
                {
                    InvestorRelationshipId = table.Column<int>(nullable: false),
                    InvestorId = table.Column<int>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorRelationshipItem", x => new { x.InvestorId, x.InvestorRelationshipId });
                    table.ForeignKey(
                        name: "FK_InvestorRelationshipItem_Investor_InvestorId",
                        column: x => x.InvestorId,
                        principalSchema: "Entity",
                        principalTable: "Investor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestorRelationshipItem_InvestorRelationship_InvestorRelationshipId",
                        column: x => x.InvestorRelationshipId,
                        principalSchema: "Entity",
                        principalTable: "InvestorRelationship",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegisterAccount",
                schema: "Pms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Number = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    SortName = table.Column<string>(maxLength: 250, nullable: false),
                    DealerTaCode = table.Column<string>(maxLength: 250, nullable: false),
                    ShareHolderId = table.Column<int>(nullable: false),
                    DistributorId = table.Column<int>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisterAccount_EntityBase_DistributorId",
                        column: x => x.DistributorId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegisterAccount_Investor_ShareHolderId",
                        column: x => x.ShareHolderId,
                        principalSchema: "Entity",
                        principalTable: "Investor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Security",
                schema: "Pms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InternalCode = table.Column<string>(maxLength: 50, nullable: false),
                    Isin = table.Column<string>(maxLength: 12, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    BenchmarkId = table.Column<int>(nullable: false),
                    CurrencyIso = table.Column<string>(maxLength: 3, nullable: true),
                    CountryCode = table.Column<string>(maxLength: 2, nullable: true),
                    PricingFrequency = table.Column<int>(nullable: false),
                    ClassificationStrategy = table.Column<string>(nullable: false),
                    GicsSectorId = table.Column<int>(nullable: false),
                    IcbSectorId = table.Column<int>(nullable: false),
                    MarketPlaceId = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    LastModifiedDate = table.Column<DateTime>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    CouponType = table.Column<string>(maxLength: 3, nullable: true),
                    CouponRate = table.Column<decimal>(nullable: true),
                    FaceValue = table.Column<decimal>(nullable: true),
                    Notional = table.Column<decimal>(nullable: true),
                    MaturityDate = table.Column<DateTime>(nullable: true),
                    IsPerpetual = table.Column<bool>(nullable: true),
                    FirstPaymentDate = table.Column<DateTime>(nullable: true),
                    NextCouponDate = table.Column<DateTime>(nullable: true),
                    PaymentFrequency = table.Column<int>(nullable: true),
                    IssueAmount = table.Column<decimal>(nullable: true),
                    IsOtc = table.Column<bool>(nullable: true),
                    CounterpartyId = table.Column<int>(nullable: true),
                    ContractSize = table.Column<decimal>(nullable: true),
                    StrikePrice = table.Column<decimal>(nullable: true),
                    SubFundId = table.Column<int>(nullable: true),
                    IsPrimary = table.Column<bool>(nullable: true),
                    DistributionType = table.Column<string>(maxLength: 50, nullable: true),
                    InvestorType = table.Column<int>(nullable: true),
                    InceptionDate = table.Column<DateTime>(nullable: true),
                    ClosingDate = table.Column<DateTime>(nullable: true),
                    MinimumInvestment = table.Column<decimal>(nullable: true),
                    EntryFee = table.Column<decimal>(nullable: true),
                    ExitFee = table.Column<decimal>(nullable: true),
                    ManagementFee = table.Column<decimal>(nullable: true),
                    PerformanceFee = table.Column<decimal>(nullable: true),
                    DividendPeriodicity = table.Column<int>(nullable: true),
                    IsOpenForInvestment = table.Column<bool>(nullable: true),
                    SicavId = table.Column<int>(nullable: true),
                    FundAdminId = table.Column<int>(nullable: true),
                    CustodianId = table.Column<int>(nullable: true),
                    TransferAgentId = table.Column<int>(nullable: true),
                    Url = table.Column<string>(maxLength: 500, nullable: true),
                    DomicileIso = table.Column<string>(maxLength: 2, nullable: true),
                    SubscriptionContactId = table.Column<int>(nullable: true),
                    RecommendedTimeHorizon = table.Column<decimal>(nullable: true),
                    SettlementNbDays = table.Column<int>(nullable: true),
                    NavFrequency = table.Column<int>(nullable: true),
                    IsLiquidated = table.Column<bool>(nullable: true),
                    LiquidationDate = table.Column<DateTime>(nullable: true),
                    InvestmentProcess = table.Column<int>(nullable: true),
                    ShortExposure = table.Column<bool>(nullable: true),
                    Leverage = table.Column<bool>(nullable: true),
                    ClosedEnded = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Security_EntityBase_CounterpartyId",
                        column: x => x.CounterpartyId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_Security_BenchmarkId",
                        column: x => x.BenchmarkId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_Security_SubFundId",
                        column: x => x.SubFundId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_EntityBase_CustodianId",
                        column: x => x.CustodianId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_EntityBase_FundAdminId",
                        column: x => x.FundAdminId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_Sicav_SicavId",
                        column: x => x.SicavId,
                        principalSchema: "Pms",
                        principalTable: "Sicav",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_EntityBase_SubscriptionContactId",
                        column: x => x.SubscriptionContactId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_EntityBase_TransferAgentId",
                        column: x => x.TransferAgentId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeeDefinition",
                schema: "Fee",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PortfolioId = table.Column<int>(nullable: true),
                    ShareClassId = table.Column<int>(nullable: true),
                    SicavId = table.Column<int>(nullable: true),
                    AnnualRate = table.Column<decimal>(nullable: true),
                    RegisterAccountId = table.Column<int>(nullable: true),
                    ThirdPartyId = table.Column<int>(nullable: false),
                    AssetPart = table.Column<int>(nullable: false),
                    ManCoSecuritiesId = table.Column<int>(nullable: true),
                    IncludeCashInAum = table.Column<bool>(nullable: false),
                    ValidityFrom = table.Column<DateTime>(nullable: true),
                    ValidityTo = table.Column<DateTime>(nullable: true),
                    FeeType = table.Column<int>(nullable: false),
                    IsVatApplicable = table.Column<bool>(nullable: false),
                    VatValue = table.Column<bool>(nullable: true),
                    PaymentFrequency = table.Column<int>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeDefinition_SecurityGroup_ManCoSecuritiesId",
                        column: x => x.ManCoSecuritiesId,
                        principalSchema: "Pms",
                        principalTable: "SecurityGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeeDefinition_Security_PortfolioId",
                        column: x => x.PortfolioId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeeDefinition_RegisterAccount_RegisterAccountId",
                        column: x => x.RegisterAccountId,
                        principalSchema: "Pms",
                        principalTable: "RegisterAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeeDefinition_Security_ShareClassId",
                        column: x => x.ShareClassId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeeDefinition_Sicav_SicavId",
                        column: x => x.SicavId,
                        principalSchema: "Pms",
                        principalTable: "Sicav",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeeDefinition_EntityBase_ThirdPartyId",
                        column: x => x.ThirdPartyId,
                        principalSchema: "Entity",
                        principalTable: "EntityBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoricalValue",
                schema: "Pms",
                columns: table => new
                {
                    SecurityId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 3, nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Value = table.Column<decimal>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalValue", x => new { x.Date, x.SecurityId, x.Type });
                    table.ForeignKey(
                        name: "FK_HistoricalValue_Security_SecurityId",
                        column: x => x.SecurityId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioComposition",
                schema: "Pms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PortfolioId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: true),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioComposition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioComposition_Security_PortfolioId",
                        column: x => x.PortfolioId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityGroupItem",
                schema: "Pms",
                columns: table => new
                {
                    SecurityId = table.Column<int>(nullable: false),
                    SecurityGroupId = table.Column<int>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityGroupItem", x => new { x.SecurityId, x.SecurityGroupId });
                    table.ForeignKey(
                        name: "FK_SecurityGroupItem_SecurityGroup_SecurityGroupId",
                        column: x => x.SecurityGroupId,
                        principalSchema: "Pms",
                        principalTable: "SecurityGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecurityGroupItem_Security_SecurityId",
                        column: x => x.SecurityId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityTransaction",
                schema: "Pms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PortfolioId = table.Column<int>(nullable: false),
                    SecurityId = table.Column<int>(nullable: false),
                    StatusCode = table.Column<string>(maxLength: 50, nullable: false),
                    Type = table.Column<string>(maxLength: 20, nullable: false),
                    TradeDate = table.Column<DateTime>(nullable: false),
                    ValueDate = table.Column<DateTime>(nullable: false),
                    NavDate = table.Column<DateTime>(nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 3, nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    GrossAmountInSecurityCcy = table.Column<decimal>(nullable: false),
                    NetAmountInSecurityCcy = table.Column<decimal>(nullable: false),
                    NetAmountInFundCcy = table.Column<decimal>(nullable: false),
                    PriceInSecurityCcy = table.Column<decimal>(nullable: false),
                    PriceInFundCcy = table.Column<decimal>(nullable: false),
                    DealDescription = table.Column<string>(nullable: false),
                    TotalGainLoss = table.Column<decimal>(nullable: false),
                    FeesInSecurityCcy = table.Column<decimal>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityTransaction_Security_PortfolioId",
                        column: x => x.PortfolioId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecurityTransaction_Security_SecurityId",
                        column: x => x.SecurityId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fee",
                schema: "Fee",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    FeeDefinitionId = table.Column<int>(nullable: false),
                    AumAssetPart = table.Column<decimal>(nullable: false),
                    AumSubFund = table.Column<decimal>(nullable: false),
                    AumSicav = table.Column<decimal>(nullable: false),
                    FeeAmount = table.Column<decimal>(nullable: false),
                    FeeAmountVatIncluded = table.Column<decimal>(nullable: true),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fee", x => x.Id);
                    table.UniqueConstraint("AK_Fee_FeeDefinitionId_Date", x => new { x.FeeDefinitionId, x.Date });
                    table.ForeignKey(
                        name: "FK_Fee_FeeDefinition_FeeDefinitionId",
                        column: x => x.FeeDefinitionId,
                        principalSchema: "Fee",
                        principalTable: "FeeDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                schema: "Pms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PortfolioCompositionId = table.Column<int>(nullable: false),
                    SecurityId = table.Column<int>(nullable: false),
                    Value = table.Column<decimal>(nullable: false),
                    Weight = table.Column<decimal>(nullable: true),
                    MarketValueInSecurityCcy = table.Column<decimal>(nullable: true),
                    MarketValueInPortfolioCcy = table.Column<decimal>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Position_PortfolioComposition_PortfolioCompositionId",
                        column: x => x.PortfolioCompositionId,
                        principalSchema: "Pms",
                        principalTable: "PortfolioComposition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Position_Security_SecurityId",
                        column: x => x.SecurityId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AumThreshold",
                schema: "Fee",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FeeId = table.Column<int>(nullable: false),
                    AumFromIncluded = table.Column<decimal>(nullable: false),
                    AumToExcluded = table.Column<decimal>(nullable: false),
                    AnnualRate = table.Column<decimal>(nullable: true),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AumThreshold", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AumThreshold_Fee_FeeId",
                        column: x => x.FeeId,
                        principalSchema: "Fee",
                        principalTable: "Fee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityBase_ContactId",
                schema: "Entity",
                table: "EntityBase",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityBase_EntityGroupId",
                schema: "Entity",
                table: "EntityBase",
                column: "EntityGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Investor_EntityId",
                schema: "Entity",
                table: "Investor",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Investor_IntermediaryId",
                schema: "Entity",
                table: "Investor",
                column: "IntermediaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Investor_InternalResponsibleId",
                schema: "Entity",
                table: "Investor",
                column: "InternalResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestorRelationshipItem_InvestorRelationshipId",
                schema: "Entity",
                table: "InvestorRelationshipItem",
                column: "InvestorRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonToCompany_PersonId",
                schema: "Entity",
                table: "PersonToCompany",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AumThreshold_FeeId",
                schema: "Fee",
                table: "AumThreshold",
                column: "FeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDefinition_ManCoSecuritiesId",
                schema: "Fee",
                table: "FeeDefinition",
                column: "ManCoSecuritiesId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDefinition_PortfolioId",
                schema: "Fee",
                table: "FeeDefinition",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDefinition_RegisterAccountId",
                schema: "Fee",
                table: "FeeDefinition",
                column: "RegisterAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDefinition_ShareClassId",
                schema: "Fee",
                table: "FeeDefinition",
                column: "ShareClassId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDefinition_SicavId",
                schema: "Fee",
                table: "FeeDefinition",
                column: "SicavId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDefinition_ThirdPartyId",
                schema: "Fee",
                table: "FeeDefinition",
                column: "ThirdPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalValue_SecurityId",
                schema: "Pms",
                table: "HistoricalValue",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioComposition_PortfolioId",
                schema: "Pms",
                table: "PortfolioComposition",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Position_PortfolioCompositionId",
                schema: "Pms",
                table: "Position",
                column: "PortfolioCompositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Position_SecurityId",
                schema: "Pms",
                table: "Position",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterAccount_DistributorId",
                schema: "Pms",
                table: "RegisterAccount",
                column: "DistributorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterAccount_ShareHolderId",
                schema: "Pms",
                table: "RegisterAccount",
                column: "ShareHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_CounterpartyId",
                schema: "Pms",
                table: "Security",
                column: "CounterpartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_BenchmarkId",
                schema: "Pms",
                table: "Security",
                column: "BenchmarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_SubFundId",
                schema: "Pms",
                table: "Security",
                column: "SubFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_CustodianId",
                schema: "Pms",
                table: "Security",
                column: "CustodianId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_FundAdminId",
                schema: "Pms",
                table: "Security",
                column: "FundAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_SicavId",
                schema: "Pms",
                table: "Security",
                column: "SicavId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_SubscriptionContactId",
                schema: "Pms",
                table: "Security",
                column: "SubscriptionContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_TransferAgentId",
                schema: "Pms",
                table: "Security",
                column: "TransferAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityGroupItem_SecurityGroupId",
                schema: "Pms",
                table: "SecurityGroupItem",
                column: "SecurityGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityTransaction_PortfolioId",
                schema: "Pms",
                table: "SecurityTransaction",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityTransaction_SecurityId",
                schema: "Pms",
                table: "SecurityTransaction",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sicav_BelongsToEntityId",
                schema: "Pms",
                table: "Sicav",
                column: "BelongsToEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntityRole_CompanyId",
                schema: "UserAccounts",
                table: "UserEntityRole",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_UserId",
                schema: "UserAccounts",
                table: "UserLogin",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestorRelationshipItem",
                schema: "Entity");

            migrationBuilder.DropTable(
                name: "PersonToCompany",
                schema: "Entity");

            migrationBuilder.DropTable(
                name: "AumThreshold",
                schema: "Fee");

            migrationBuilder.DropTable(
                name: "DataProviderSecurity",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "HistoricalValue",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "Position",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "SecurityGroupItem",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "SecurityTransaction",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "UserEntityRole",
                schema: "UserAccounts");

            migrationBuilder.DropTable(
                name: "UserLogin",
                schema: "UserAccounts");

            migrationBuilder.DropTable(
                name: "InvestorRelationship",
                schema: "Entity");

            migrationBuilder.DropTable(
                name: "Fee",
                schema: "Fee");

            migrationBuilder.DropTable(
                name: "PortfolioComposition",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "FeeDefinition",
                schema: "Fee");

            migrationBuilder.DropTable(
                name: "SecurityGroup",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "Security",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "RegisterAccount",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "Sicav",
                schema: "Pms");

            migrationBuilder.DropTable(
                name: "Investor",
                schema: "Entity");

            migrationBuilder.DropTable(
                name: "EntityBase",
                schema: "Entity");

            migrationBuilder.DropTable(
                name: "EntityGroup",
                schema: "Entity");
        }
    }
}
