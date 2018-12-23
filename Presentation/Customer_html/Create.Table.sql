create table [dbo].[BonusApp_ActivityLogType] (
    [Id] [int] not null identity,
    [SystemKeyword] [nvarchar](100) not null,
    [Name] [nvarchar](200) not null,
    [Enabled] [bit] not null
    primary key ([Id])
);

create table [dbo].[BonusApp_ActivityLog] (
    [Id] [int] not null identity,
    [ActivityLogTypeId] [int] not null,
    [CustomerId] [int] not null,
    [Comment] [nvarchar](max) not null,
    [CreatedOnUtc] [datetime] not null,
    [IpAddress] [nvarchar](200) null,
    primary key ([Id])
);
create table [dbo].[BonusApp_Customer] (
    [Id] [int] not null identity,
    [CustomerGuid] [uniqueidentifier] not null,
    [Username] [nvarchar](max) null,
    [Password] [nvarchar](max) null,
    [AvatarUrl] [nvarchar](max) null,
    [Nickname] [nvarchar](max) null,
    [PhoneNumber] [nvarchar](max) null,
    [Active] [bit] not null,
    [Deleted] [bit] not null,
    [LastIpAddress] [nvarchar](max) null,
    [CreatedOnUtc] [datetime] not null,
    [LastLoginDateUtc] [datetime] null,
    [LastActivityDateUtc] [datetime] not null,
    [Money] [float] not null,
    primary key ([Id])
);
create table [dbo].[BonusApp_MoneyLog] (
    [Id] [int] not null identity,
    [CustomerId] [int] not null,
    [Comment] [nvarchar](max) not null,
    [CreatedOnUtc] [datetime] not null,
    [IpAddress] [nvarchar](200) null,
    [Money] [float] not null,
    [ReturnMoney] [float] not null,
    [AppMoneyBefore] [float] not null,
    [AppMoneyDelta] [float] not null,
    [AppMoneyAfter] [float] not null,
    [MoneyReturnStatus] [int] not null,
    primary key ([Id])
);
create table [dbo].[BonusApp_Setting] (
    [Id] [int] not null identity,
    [Name] [nvarchar](200) not null,
    [Value] [nvarchar](2000) not null,
    [StoreId] [int] not null,
    primary key ([Id])
);
create table [dbo].[BonusApp_Status] (
    [Id] [int] not null identity,
    [CurrentMoney] [float] not null,
    [WaitingUserCount] [int] not null,
    [CompleteUserCount] [int] not null,
    [MoneyPaied] [float] not null,
    primary key ([Id])
);

alter table [dbo].[BonusApp_ActivityLog] add constraint [BonusApp_ActivityLog_ActivityLogType] foreign key ([ActivityLogTypeId]) references [dbo].[BonusApp_ActivityLogType]([Id]) on delete cascade;
alter table [dbo].[BonusApp_ActivityLog] add constraint [BonusApp_ActivityLog_Customer] foreign key ([CustomerId]) references [dbo].[BonusApp_Customer]([Id]) on delete cascade;
alter table [dbo].[BonusApp_MoneyLog] add constraint [BonusApp_MoneyLog_Customer] foreign key ([CustomerId]) references [dbo].[BonusApp_Customer]([Id]) on delete cascade;
alter table [dbo].[BonusApp_Setting] add constraint [BonusApp_Setting_TypeConstraint_From_Setting_To_BonusApp_Setting] foreign key ([Id]) references [dbo].[Setting]([Id]);