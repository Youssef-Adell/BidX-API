-- This trigger updates the `LastMessageId` field in the `Chat` table
-- for the corresponding `ChatId` in each newly inserted `Message`.

CREATE TRIGGER [dbo].[last_message_trigger]
ON [dbo].[Message]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;     -- Prevent unintended side effects by suppressing row count messages

    UPDATE Chat
    SET Chat.LastMessageId = inserted.Id
    FROM Chat
    INNER JOIN inserted ON Chat.Id = inserted.ChatId;
END;
