ALTER TABLE dbo.Customer
ALTER COLUMN [Username] [nvarchar] (1000) 
COLLATE Chinese_PRC_CS_AS NULL
GO

ALTER TABLE dbo.Customer
ALTER COLUMN [Email] [nvarchar] (1000) 
COLLATE Chinese_PRC_CS_AS NULL
GO