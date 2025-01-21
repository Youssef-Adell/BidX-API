-- This trigger updates the `AverageRating` field in the `User` table
-- for the corresponding `RevieweeId` in each inserted, Updated, Deleted `Review`.

CREATE TRIGGER [dbo].[average_rating_trigger]
ON [dbo].[Review]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [security].[User]
    SET AverageRating = (
        SELECT COALESCE(AVG(CAST(Rating AS DECIMAL(5, 2))), 0)
        FROM Review
        WHERE Review.RevieweeId = [security].[User].Id
    )
    WHERE [security].[User].Id IN (
        SELECT RevieweeId FROM inserted
        UNION
        SELECT RevieweeId FROM deleted
    );
END;

