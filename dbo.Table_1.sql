CREATE TABLE [dbo].[Users]
(
	[USERID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [PilotNo] INT NOT NULL, 
    [Username] VARCHAR(50) NOT NULL, 
    [Password] VARCHAR(100) NOT NULL, 
    CONSTRAINT [FK_PilotNo] FOREIGN KEY ([PilotNo]) REFERENCES [Pilots]([PilotNo]) 
)
