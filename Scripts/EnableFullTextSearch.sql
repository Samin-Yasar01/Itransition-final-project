-- Enable full-text search if not already enabled
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'FormsAppCatalog')
BEGIN
    CREATE FULLTEXT CATALOG FormsAppCatalog AS DEFAULT;
END

-- Create full-text index on Templates table
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Templates'))
BEGIN
    CREATE FULLTEXT INDEX ON Templates(Title, Description)
    KEY INDEX PK_Templates
    ON FormsAppCatalog
    WITH CHANGE_TRACKING AUTO;
END

-- Create full-text index on Questions table
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Questions'))
BEGIN
    CREATE FULLTEXT INDEX ON Questions(Description)
    KEY INDEX PK_Questions
    ON FormsAppCatalog
    WITH CHANGE_TRACKING AUTO;
END

-- Create full-text index on Comments table
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Comments'))
BEGIN
    CREATE FULLTEXT INDEX ON Comments(Content)
    KEY INDEX PK_Comments
    ON FormsAppCatalog
    WITH CHANGE_TRACKING AUTO;
END

-- Populate the full-text indexes
ALTER FULLTEXT INDEX ON Templates START FULL POPULATION;
ALTER FULLTEXT INDEX ON Questions START FULL POPULATION;
ALTER FULLTEXT INDEX ON Comments START FULL POPULATION; 