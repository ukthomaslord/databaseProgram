CREATE TABLE [dbo].[Flights]
(
	[ID] INT NOT NULL PRIMARY KEY, 
    [FlightNo] VARCHAR(6) NOT NULL, 
    [PilotNo] INT NULL, 
    [Destination] VARCHAR(50) NOT NULL, 
    CONSTRAINT [FK2_PilotNo] FOREIGN KEY ([PilotNo]) REFERENCES [Pilots]([PilotNo])
)
