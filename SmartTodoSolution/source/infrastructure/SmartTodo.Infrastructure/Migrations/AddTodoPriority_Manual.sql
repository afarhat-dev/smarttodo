-- Manual migration script for AddTodoPriority
-- This adds the Priority column to the TodoItems table
-- Run this if 'dotnet ef database update' is not working

-- Add Priority column with default value
ALTER TABLE "TodoItems"
ADD COLUMN "Priority" text NOT NULL DEFAULT 'Medium';

-- Create index on Priority column for better query performance
CREATE INDEX "IX_TodoItems_Priority" ON "TodoItems" ("Priority");

-- Verify the changes
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'TodoItems'
ORDER BY ordinal_position;

-- Show indexes
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'TodoItems';

-- IMPORTANT: Record this migration as applied in EF Core's migration history
-- This prevents EF Core from trying to apply it again
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260121132554_AddTodoPriority', '10.0.1');
