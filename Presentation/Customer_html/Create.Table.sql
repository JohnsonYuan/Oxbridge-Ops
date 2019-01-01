CREATE TABLE [dbo].[BonusApp_ActivityLog]
    (
      [Id] [INT] NOT NULL
                 IDENTITY ,
      [ActivityLogTypeId] [INT] NOT NULL ,
      [CustomerId] [INT] NOT NULL ,
      [Comment] [NVARCHAR](MAX) NOT NULL ,
      [CreatedOnUtc] [DATETIME] NOT NULL ,
      [IpAddress] [NVARCHAR](200) NULL ,
      PRIMARY KEY ( [Id] )
    );
CREATE TABLE [dbo].[BonusApp_ActivityLogType]
    (
      [Id] [INT] NOT NULL
                 IDENTITY ,
      [SystemKeyword] [NVARCHAR](100) NOT NULL ,
      [Name] [NVARCHAR](200) NOT NULL ,
      [Enabled] [BIT] NOT NULL ,
      PRIMARY KEY ( [Id] )
    );
CREATE TABLE [dbo].[BonusApp_WithdrawLog] (
  [Id] int IDENTITY(1,1) NOT NULL,
  [CustomerId] int NOT NULL,
  [Comment] nvarchar(max) COLLATE Chinese_PRC_CI_AS NOT NULL,
  [Amount] [DECIMAL](18, 2) NOT NULL ,
  [IsDone] bit NOT NULL,
  [CreatedOnUtc] datetime NOT NULL,
  [CompleteOnUtc] datetime NULL,
  [IpAddress] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL
)
GO
CREATE TABLE [dbo].[BonusApp_Customer]
    (
      [Id] [INT] NOT NULL
                 IDENTITY ,
      [CustomerGuid] [UNIQUEIDENTIFIER] NOT NULL ,
      [Username] [NVARCHAR](MAX) NULL ,
      [Password] [NVARCHAR](MAX) NULL ,
      [AvatarFileName] [NVARCHAR](MAX) NULL ,
      [Nickname] [NVARCHAR](MAX) NULL ,
      [PhoneNumber] [NVARCHAR](MAX) NULL ,
      [Active] [BIT] NOT NULL default(1),
      [Deleted] [BIT] NOT NULL default(0),
      [LastIpAddress] [NVARCHAR](MAX) NULL ,
      [CreatedOnUtc] [DATETIME] NOT NULL ,
      [LastLoginDateUtc] [DATETIME] NULL ,
      [LastActivityDateUtc] [DATETIME] NOT NULL ,
      [Money] [DECIMAL](18, 2) NOT NULL default(0),
      PRIMARY KEY ( [Id] )
    );
CREATE TABLE [dbo].[BonusApp_CustomerComment]
    (
      [Id] [INT] NOT NULL
                 IDENTITY ,
      [CustomerId] [INT] NOT NULL ,
      [Comment] [NVARCHAR](300) NULL ,
      [Rate] [INT] NOT NULL ,
      [Enabled] [BIT] NOT NULL ,
      [IpAddress] [NVARCHAR](MAX) NULL ,
      [CreatedOnUtc] [DATETIME] NOT NULL ,
      PRIMARY KEY ( [Id] )
    );
CREATE TABLE [dbo].[BonusApp_MoneyLog]
    (
      [Id] [INT] NOT NULL
                 IDENTITY ,
      [CustomerId] [INT] NOT NULL ,
      [Comment] [NVARCHAR](MAX) NOT NULL ,
      [CreatedOnUtc] [DATETIME] NOT NULL ,
      [IpAddress] [NVARCHAR](200) NULL ,
      [Money] [DECIMAL](18, 2) NOT NULL ,
      [ReturnMoney] [DECIMAL](18, 2) NOT NULL ,
      [MoneyReturnStatusId] [INT] NOT NULL ,
      [AppMoneyBefore] [DECIMAL](18, 2) NOT NULL ,
      [AppMoneyDelta] [DECIMAL](18, 2) NOT NULL ,
      [AppMoneyAfter] [DECIMAL](18, 2) NOT NULL ,
      PRIMARY KEY ( [Id] )
    );
CREATE TABLE [dbo].[BonusApp_Setting]
    (
      [Id] [INT] NOT NULL
                 IDENTITY ,
      [Name] [NVARCHAR](MAX) NULL ,
      [Value] [NVARCHAR](MAX) NULL ,
      [StoreId] [INT] NOT NULL ,
      PRIMARY KEY ( [Id] )
    );
CREATE TABLE [dbo].[BonusApp_Status]
    (
      [Id] [INT] NOT NULL
                 IDENTITY ,
      [CurrentMoney] [DECIMAL](18, 2) NOT NULL ,
      [WaitingUserCount] [INT] NOT NULL ,
      [CompleteUserCount] [INT] NOT NULL ,
      [MoneyPaied] [DECIMAL](18, 2) NOT NULL ,
      [AllUserMoney] [DECIMAL](18, 2) NOT NULL ,
      PRIMARY KEY ( [Id] )
    );

ALTER TABLE [dbo].[BonusApp_ActivityLog] ADD CONSTRAINT [BonusApp_ActivityLog_ActivityLogType] FOREIGN KEY ([ActivityLogTypeId]) REFERENCES [dbo].[BonusApp_ActivityLogType]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[BonusApp_ActivityLog] ADD CONSTRAINT [BonusApp_ActivityLog_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[BonusApp_Customer]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[BonusApp_WithdrawLog] ADD CONSTRAINT [BonusApp_WithdrawLog_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[BonusApp_Customer]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[BonusApp_CustomerComment] ADD CONSTRAINT [BonusApp_CustomerComment_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[BonusApp_Customer]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[BonusApp_MoneyLog] ADD CONSTRAINT [BonusApp_MoneyLog_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[BonusApp_Customer]([Id]) ON DELETE CASCADE;
