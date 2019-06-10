CREATE TABLE AccountConfiguration (
    Lock CHAR(1) PRIMARY KEY CHECK(Lock = 'X') NOT NULL,
    BaseBalance INTEGER NOT NULL
);
GO

INSERT INTO AccountConfiguration (Lock, BaseBalance) VALUES ('X', 1000);
GO
  
CREATE VIEW View_AccountConfiguration
WITH SCHEMABINDING
AS
    SELECT BaseBalance
    FROM [dbo].[AccountConfiguration]
GO

--Set the options to support indexed views.  
SET NUMERIC_ROUNDABORT OFF;  
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT,  
    QUOTED_IDENTIFIER, ANSI_NULLS ON;  
GO

DROP VIEW IF EXISTS View_BetRecord;
GO

CREATE VIEW View_BetRecord
WITH SCHEMABINDING
AS
    SELECT 
        UserId, 
        SUM(CASE 
            WHEN m.IsFinished = 1 AND s.LosersScore = m.LosersScore AND s.WinningSide = m.WinningSide
            THEN b.Amount * s.Ratio 
            ELSE -b.Amount
            END) 
         AS Balance,
         COUNT_BIG(*) AS "Count"
    FROM [dbo].[Bet] AS b
    JOIN [dbo].[Stake] AS s ON s.Id = b.StakeId
    JOIN [dbo].[Match] AS m ON s.MatchId = m.Id
    GROUP BY b.UserId
GO

CREATE UNIQUE CLUSTERED INDEX IDX_View_BetRecord
    ON View_BetRecord (UserId);
GO

DROP VIEW IF EXISTS View_Account;
GO

CREATE VIEW View_Account
WITH SCHEMABINDING
AS
    SELECT UserId, Balance + c.BaseBalance AS Balance
	FROM [dbo].[View_BetRecord], [dbo].[AccountConfiguration] AS c
GO