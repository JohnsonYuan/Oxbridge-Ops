CREATE TABLE [dbo].[MoneyLog]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[ActivityLogTypeId] [int] NOT NULL,
[CustomerId] [int] NOT NULL,
[Comment] [nvarchar] (max) COLLATE Chinese_PRC_CI_AS NOT NULL,
[CreatedOnUtc] [datetime] NOT NULL,
[IpAddress] [nvarchar] (200) COLLATE Chinese_PRC_CI_AS NULL,
[MoneyBefore] [bigint] NOT NULL,
[MoneyDelta] [bigint] NOT NULL,
[MoneyAfter] [bigint] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[MoneyLog] ADD CONSTRAINT [PK__MoneyLog__3214EC07D7C6533B] PRIMARY KEY CLUSTERED ([Id]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MoneyLog] ADD CONSTRAINT [ActivityLog_ActivityLogType1] FOREIGN KEY ([ActivityLogTypeId]) REFERENCES [dbo].[ActivityLogType] ([Id]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MoneyLog] ADD CONSTRAINT [ActivityLog_Customer1] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]) ON DELETE CASCADE
GO

--把金钱相关log放到moneylog中
INSERT INTO dbo.MoneyLog
        (  
          ActivityLogTypeId ,
          CustomerId ,
          Comment ,
          CreatedOnUtc ,
          IpAddress ,
          MoneyBefore ,
          MoneyDelta ,
          MoneyAfter
        )
SELECT ActivityLogTypeId ,
          CustomerId ,
          Comment ,
          CreatedOnUtc ,
          IpAddress, 0 AS MoneyBefore,
		  0 AS MoneyDelta,
		  0 AS MoneyAfter FROM	dbo.ActivityLog
 
 WHERE ActivityLogTypeId IN (
  SELECT Id FROM dbo.ActivityLogType WHERE CHARINDEX('ZhiXiao', SystemKeyword) > 0
 )