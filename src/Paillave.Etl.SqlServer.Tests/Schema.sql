create table People (
    Id int identity primary key,
    Firstname nvarchar(100) not null,
    Lastname nvarchar(100) not null,
    LastnamePrefix nvarchar(10) null,
    [Role] nvarchar(100) null,
    InsertedAtUtc datetime2(7) not null default getutcdate(),    
    ValidFromUtc datetime2(7) generated always as row start not null,
    ValidToUtc datetime2(7) generated always as row end not null,
    period for system_time (ValidFromUtc, ValidToUtc)
)
with (system_versioning = on (history_table = dbo.PeopleHistory));

set identity_insert People on;


create table Groups (
    PersonId int not null,
    [Group] nvarchar(10) not null
);