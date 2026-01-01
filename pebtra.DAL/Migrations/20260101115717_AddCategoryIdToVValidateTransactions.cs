using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pebtra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToVValidateTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW V_ValidateTransactions AS 
SELECT TOP 10000 * FROM (
	SELECT 'Transfer amounts differ by more than 1.0' AS msg, t.GroupId, t.Id, t.Date, YEAR(t.Date) AS TYear, MONTH(t.Date) AS TMonth, t.AccountId, t.Amount, t.CategoryId, t.Details, t.Comment, t.ExtraDetails, t.CurrencyDetails 
	FROM Transactions t
	WHERE GroupId in (
		select GroupId from (
			select GroupId, Sum(Amount) as balance
			from Transactions t
			where t.CategoryId = 'T_TRANSFER' AND t.IsActive = 1
			group by GroupId) q 
		where balance > 1
	)
	UNION
SELECT 'Returned amount differs from the original amount by more than 10%' AS msg, t.GroupId, t.Id, t.Date, YEAR(t.Date) AS TYear, MONTH(t.Date) AS TMonth, t.AccountId, t.Amount, t.CategoryId, t.Details, t.Comment, t.ExtraDetails, t.CurrencyDetails
FROM Transactions t
	WHERE GroupId in (
		SELECT pos.GroupId 
		FROM (
			SELECT GroupId, Sum(Amount) as balance
			FROM Transactions t
			WHERE t.CategoryId = 'T_RETURN' AND t.Amount > 0 AND t.IsActive = 1
			GROUP BY GroupId) AS pos
		INNER JOIN (
		SELECT GroupId, Sum(Amount) as balance
			FROM Transactions t
			WHERE t.CategoryId = 'T_RETURN' AND t.Amount < 0 AND t.IsActive = 1
			GROUP BY GroupId
		) AS neg 
		ON pos.GroupId = neg.GroupId
		WHERE ABS(1 - ABS(pos.balance / neg.balance)) >= 0.1
	)
	UNION	
	SELECT 'Conversion rate differs from the market rate by more than 10%' AS msg, t.GroupId, t.Id, t.Date, YEAR(t.Date) AS TYear, MONTH(t.Date) AS TMonth, t.AccountId, t.Amount, t.CategoryId, t.Details, t.Comment, t.ExtraDetails, t.CurrencyDetails
	FROM Transactions t
	WHERE GroupId IN (
		SELECT GroupId FROM V_GroupedConversions q
		WHERE q.RateDifference > 1.1 OR q.RateDifference < 0.9
	)
	UNION
	SELECT 'Ungrouped transfer or conversion' AS msg, t.GroupId, t.Id, t.Date, YEAR(t.Date) AS TYear, MONTH(t.Date) AS TMonth, t.AccountId, t.Amount, t.CategoryId, t.Details, t.Comment, t.ExtraDetails, t.CurrencyDetails
	FROM Transactions t
	WHERE GroupId IS NULL 
	AND (t.CategoryId LIKE 'T_%' OR t.CategoryId = 'C_CONV')
	UNION
	SELECT 'Sum of transfer transactions for a month is not zero' AS msg, NULL, NULL, NULL, TYear, TMonth, NULL, AmountSum, NULL, CurrencyId, NULL, NULL, NULL
	FROM (
		SELECT YEAR(Date) AS TYear, MONTH (Date) AS TMonth, Sum(Amount) AS AmountSum, CurrencyId
		FROM Transactions t
		INNER JOIN Accounts a on a.Id = t.AccountId
		WHERE t.CategoryId LIKE 'T_%'
		GROUP BY YEAR(Date), MONTH (Date),  CurrencyId, t.CategoryId) q
	WHERE ABS(AmountSum) > 1) q
ORDER BY Date;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER           VIEW V_ValidateTransactions AS 
SELECT TOP 10000 * FROM (
	SELECT 'Transfer amounts differ by more than 1.0' AS msg, t.GroupId, t.Id, t.Date, YEAR(t.Date) AS TYear, MONTH(t.Date) AS TMonth, t.AccountId, t.Amount, t.Details, t.Comment, t.ExtraDetails, t.CurrencyDetails 
	FROM Transactions t
	WHERE GroupId in (
		select GroupId from (
			select GroupId, Sum(Amount) as balance
			from Transactions t
			where t.CategoryId = 'T_TRANSFER' AND t.IsActive = 1
			group by GroupId) q 
		where balance > 1
	)
	UNION
SELECT 'Returned amount differs from the original amount by more than 10%' AS msg, t.GroupId, t.Id, t.Date, YEAR(t.Date) AS TYear, MONTH(t.Date) AS TMonth, t.AccountId, t.Amount, t.Details, t.Comment, t.ExtraDetails, t.CurrencyDetails
FROM Transactions t
	WHERE GroupId in (
		SELECT pos.GroupId 
		FROM (
			SELECT GroupId, Sum(Amount) as balance
			FROM Transactions t
			WHERE t.CategoryId = 'T_RETURN' AND t.Amount > 0 AND t.IsActive = 1
			GROUP BY GroupId) AS pos
		INNER JOIN (
		SELECT GroupId, Sum(Amount) as balance
			FROM Transactions t
			WHERE t.CategoryId = 'T_RETURN' AND t.Amount < 0 AND t.IsActive = 1
			GROUP BY GroupId
		) AS neg 
		ON pos.GroupId = neg.GroupId
		WHERE ABS(1 - ABS(pos.balance / neg.balance)) >= 0.1
	)
	UNION	
	SELECT 'Conversion rate differs from the market rate by more than 10%' AS msg, t.GroupId, t.Id, t.Date, YEAR(t.Date) AS TYear, MONTH(t.Date) AS TMonth, t.AccountId, t.Amount, t.Details, t.Comment, t.ExtraDetails, t.CurrencyDetails
	FROM Transactions t
	WHERE GroupId IN (
		SELECT GroupId FROM V_GroupedConversions q
		WHERE q.RateDifference > 1.1 OR q.RateDifference < 0.9
	)
	UNION
	SELECT 'Ungrouped transfer or conversion' AS msg, t.GroupId, t.Id, t.Date, YEAR(t.Date) AS TYear, MONTH(t.Date) AS TMonth, t.AccountId, t.Amount, t.Details, t.Comment, t.ExtraDetails, t.CurrencyDetails
	FROM Transactions t
	WHERE GroupId IS NULL 
	AND (t.CategoryId LIKE 'T_%' OR t.CategoryId = 'C_CONV')
	UNION
	SELECT 'Sum of transfer transactions for a month is not zero' AS msg, NULL, NULL, NULL, TYear, TMonth, NULL, AmountSum, CurrencyId, NULL, NULL, NULL
	FROM (
		SELECT YEAR(Date) AS TYear, MONTH (Date) AS TMonth, Sum(Amount) AS AmountSum, CurrencyId
		FROM Transactions t
		INNER JOIN Accounts a on a.Id = t.AccountId
		WHERE t.CategoryId LIKE 'T_%'
		GROUP BY YEAR(Date), MONTH (Date),  CurrencyId, t.CategoryId) q
	WHERE ABS(AmountSum) > 1) q
ORDER BY Date;");
        }
    }
}
