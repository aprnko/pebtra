using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pebtra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLinkDirectProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE LinkDirect
    @Id1 INT,
    @Id2 INT,
    @Id3 INT = NULL,
    @Id4 INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
	PRINT '>>> Executing LinkDirect'

    DECLARE @NewGroupId UNIQUEIDENTIFIER = NEWID();

    -- Build table of input IDs
    DECLARE @Ids TABLE (Id INT);
    INSERT INTO @Ids (Id) VALUES (@Id1), (@Id2);
    IF @Id3 IS NOT NULL INSERT INTO @Ids VALUES (@Id3);
    IF @Id4 IS NOT NULL INSERT INTO @Ids VALUES (@Id4);

    -- Select all involved rows
    DECLARE @Selected TABLE (
        Id INT,
        CategoryId CHAR(10),
        Amount DECIMAL(18, 2),
        GroupId UNIQUEIDENTIFIER,
        TransactionTypeId INT,
        Date DATE
    );

    INSERT INTO @Selected (Id, CategoryId, Amount, GroupId, TransactionTypeId, Date)
    SELECT t.Id, t.CategoryId, t.Amount, t.GroupId, c.TransactionTypeId, t.Date
    FROM Transactions t
    INNER JOIN @Ids i ON t.Id = i.Id
    LEFT OUTER JOIN Categories c ON t.CategoryId = c.Id;

    DECLARE @CategoryId NVARCHAR(100) = (SELECT TOP 1 CategoryId FROM @Selected ORDER BY CategoryId DESC);    
    PRINT 'Category is ' + @CategoryId
    
    IF (@CategoryId IS NULL)
    BEGIN
	    PRINT 'No category specified for any of the transactions. Assuming category is T_TRANSFER.'
    	SET @CategoryID = 'T_TRANSFER' -- Default category for matched uncategorized transactions 
    END
   
    -- Validation 1: all records must exist
    IF (SELECT COUNT(*) FROM @Ids) != (SELECT COUNT(*) FROM @Selected)
    BEGIN
        RAISERROR('One or more specified transaction IDs do not exist.', 16, 1);
        RETURN;
    END

    -- Validation 2: all records must have GroupId = NULL
    IF EXISTS (SELECT 1 FROM @Selected WHERE GroupId IS NOT NULL)
    BEGIN
        RAISERROR('One or more specified transactions already have a GroupId.', 16, 1);
        RETURN;
    END

    -- Validation 3: all records must have the same CategoryId
    IF (SELECT COUNT(DISTINCT CategoryId) FROM @Selected WHERE CategoryId IS NOT NULL) > 1
    BEGIN
        RAISERROR('All specified transactions must have the same CategoryId.', 16, 1);
        RETURN;
    END

    -- Validation 4: TransactionTypeId must be 0 (Transfer) or 2 (Conversion)
    DECLARE @TransactionTypeId INT = (SELECT TOP 1 TransactionTypeId FROM @Selected);
    IF @TransactionTypeId NOT IN (-1, 0, 2)
    BEGIN
        RAISERROR('TransactionTypeId must be either Transfer (including Return), Conversion or Expense', 16, 1);
        RETURN;
    END

    -- Validation 5: Amounts sum must be less than 1 for TRANSFER transactions    
    IF @CategoryId = 'T_TRANSFER' AND (SELECT ABS(SUM(Amount)) FROM @Selected) >= 1    
    BEGIN
        RAISERROR('The sum of amounts for the specified transactions must be less than 1 for TRANSFER transactions.', 16, 1);
        RETURN;
    END

    -- Validation 6: For Conversion transactions (TransactionTypeId = 2), there should be exactly 2 records, and amounts must have different signs
    IF @TransactionTypeId = 2
    BEGIN
        DECLARE @Count INT = (SELECT COUNT(*) FROM @Selected);
        IF @Count != 2
        BEGIN
            RAISERROR('For Conversion transactions, exactly 2 transactions are required.', 16, 1);
            RETURN;
        END

        DECLARE @PositiveCount INT = (SELECT COUNT(*) FROM @Selected WHERE Amount > 0);
        DECLARE @NegativeCount INT = (SELECT COUNT(*) FROM @Selected WHERE Amount < 0);

        IF @PositiveCount != 1 OR @NegativeCount != 1
        BEGIN
            RAISERROR('For Conversion transactions, the amounts must have different signs (one positive and one negative).', 16, 1);
            RETURN;
        END
    END
    
    -- Validation 7: Transactions should belong to the same month.
    IF (SELECT COUNT(DISTINCT MONTH(Date)) FROM @Selected) > 1
    BEGIN
        RAISERROR('All specified transactions must belong to the same month. Use LinkTransit if the transactions being linked have been precessed in different months.', 16, 1);
        RETURN;
    END

    PRINT 'All checks passed, updating GroupId.'
    
    -- All checks passed — update
    UPDATE Transactions
    SET GroupId = @NewGroupId
    WHERE Id IN (SELECT Id FROM @Ids);
   
    -- For expense and return transactions, set CategoryId to T_RETURN
    IF @TransactionTypeId = -1
    BEGIN
	    PRINT 'Setting category to T_RETURN for expense and return transactions.'
	    UPDATE Transactions
	    SET CategoryId = 'T_RETURN'
	    WHERE Id IN (SELECT Id FROM @Ids);    	
    END
    
    PRINT 'Setting category for transactions without a category (if any).'
    -- For uncategorized transactions set CategoryId
	UPDATE Transactions
    SET CategoryId = @CategoryId
    WHERE Id IN (SELECT Id FROM @Ids) AND CategoryId IS NULL;   
   
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE LinkDirect
    @Id1 INT,
    @Id2 INT,
    @Id3 INT = NULL,
    @Id4 INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewGroupId UNIQUEIDENTIFIER = NEWID();

    -- Build table of input IDs
    DECLARE @Ids TABLE (Id INT);
    INSERT INTO @Ids (Id) VALUES (@Id1), (@Id2);
    IF @Id3 IS NOT NULL INSERT INTO @Ids VALUES (@Id3);
    IF @Id4 IS NOT NULL INSERT INTO @Ids VALUES (@Id4);

    -- Select all involved rows
    DECLARE @Selected TABLE (
        Id INT,
        CategoryId CHAR(10),
        Amount DECIMAL(18, 2),
        GroupId UNIQUEIDENTIFIER,
        TransactionTypeId INT,
        Date DATE
    );

    INSERT INTO @Selected (Id, CategoryId, Amount, GroupId, TransactionTypeId, Date)
    SELECT t.Id, t.CategoryId, t.Amount, t.GroupId, c.TransactionTypeId, t.Date
    FROM Transactions t
    INNER JOIN @Ids i ON t.Id = i.Id
    INNER JOIN Categories c ON t.CategoryId = c.Id;

    -- Validation 1: all records must exist
    IF (SELECT COUNT(*) FROM @Ids) != (SELECT COUNT(*) FROM @Selected)
    BEGIN
        RAISERROR('One or more specified transaction IDs do not exist.', 16, 1);
        RETURN;
    END

    -- Validation 2: all records must have GroupId = NULL
    IF EXISTS (SELECT 1 FROM @Selected WHERE GroupId IS NOT NULL)
    BEGIN
        RAISERROR('One or more specified transactions already have a GroupId.', 16, 1);
        RETURN;
    END

    -- Validation 3: all records must have the same CategoryId
    IF (SELECT COUNT(DISTINCT CategoryId) FROM @Selected) > 1
    BEGIN
        RAISERROR('All specified transactions must have the same CategoryId.', 16, 1);
        RETURN;
    END

    -- Validation 4: TransactionTypeId must be 0 (Transfer) or 2 (Conversion)
    DECLARE @TransactionTypeId INT = (SELECT TOP 1 TransactionTypeId FROM @Selected);
    IF @TransactionTypeId NOT IN (-1, 0, 2)
    BEGIN
        RAISERROR('TransactionTypeId must be either Transfer (including Return), Conversion or Expense', 16, 1);
        RETURN;
    END

    -- Validation 5: Amounts sum must be less than 1 for TRANSFER transactions    
    DECLARE @CategoryId NVARCHAR  = (SELECT TOP 1 CategoryId FROM @Selected);
    IF @CategoryId = 'T_TRANSFER' AND (SELECT ABS(SUM(Amount)) FROM @Selected) >= 1    
    BEGIN
        RAISERROR('The sum of amounts for the specified transactions must be less than 1 for TRANSFER transactions.', 16, 1);
        RETURN;
    END

    -- Validation 6: For Conversion transactions (TransactionTypeId = 2), there should be exactly 2 records, and amounts must have different signs
    IF @TransactionTypeId = 2
    BEGIN
        DECLARE @Count INT = (SELECT COUNT(*) FROM @Selected);
        IF @Count != 2
        BEGIN
            RAISERROR('For Conversion transactions, exactly 2 transactions are required.', 16, 1);
            RETURN;
        END

        DECLARE @PositiveCount INT = (SELECT COUNT(*) FROM @Selected WHERE Amount > 0);
        DECLARE @NegativeCount INT = (SELECT COUNT(*) FROM @Selected WHERE Amount < 0);

        IF @PositiveCount != 1 OR @NegativeCount != 1
        BEGIN
            RAISERROR('For Conversion transactions, the amounts must have different signs (one positive and one negative).', 16, 1);
            RETURN;
        END
    END
    
    -- Validation 7: Transactions should belong to the same month.
    IF (SELECT COUNT(DISTINCT MONTH(Date)) FROM @Selected) > 1
    BEGIN
        RAISERROR('All specified transactions must belong to the same month. Use LinkTransit if the transactions being linked have been precessed in different months.', 16, 1);
        RETURN;
    END

    -- All checks passed — update
    UPDATE Transactions
    SET GroupId = @NewGroupId
    WHERE Id IN (SELECT Id FROM @Ids);
    
    -- For expense and return transactions, set CategoryId to T_RETURN
    IF @TransactionTypeId = -1
    BEGIN
	    UPDATE Transactions
	    SET CategoryId = 'T_RETURN'
	    WHERE Id IN (SELECT Id FROM @Ids);    	
    END
   
END;");
        }
    }
}
