using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pebtra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SecondaryCurrencyInAnalyticViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER             VIEW V_Expenses AS
SELECT TOP 10000
    t.Id AS TransactionId,
    t.Date,
    YEAR(t.Date) AS Year,
    MONTH(t.Date) AS Month,
    ISNULL(parent.Name, 'Другое') AS ParentCategory,
    c.Name AS Category,
    ISNULL(c.OrderIndex, 1000000) AS CategoryOrderIndex,
	-t.Amount AS Amount,	
    SUM(t.Amount) OVER (
        PARTITION BY t.AccountId 
        ORDER BY t.Date
        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    ) AS Balance,	
	accCur.Id as Currency,
	CASE 
        WHEN accCur.IsBase = 1 
        THEN -t.Amount
        ELSE -t.Amount * cr1.Rate
    END AS AmountInBaseCurrency,
	baseCur.Id as BaseCurrency,
	CASE 
        WHEN accCur.IsSecondary = 1
        THEN -t.Amount
        ELSE (CASE 
            WHEN accCur.IsBase = 1
            THEN -t.Amount
            ELSE -t.Amount * cr1.Rate
        END) / cr2.Rate
    END AS AmountInSecondaryCurrency,
	secCur.Id as SecondaryCurrency,	
	a.Name as AccountName
	FROM Transactions t
JOIN Categories c ON t.CategoryId = c.Id
LEFT JOIN Categories parent ON c.ParentId = parent.Id
JOIN Accounts a ON t.AccountId = a.Id
JOIN Currencies accCur ON accCur.Id=a.CurrencyId
JOIN Currencies baseCur on baseCur.IsBase = 1
JOIN Currencies secCur on secCur.IsSecondary = 1
LEFT JOIN CurrencyRates cr1 on cr1.Date=t.Date and cr1.QuoteCurrencyId = accCur.Id AND cr1.BaseCurrencyId = baseCur.Id
LEFT JOIN CurrencyRates cr2 on cr2.Date=t.Date and cr2.QuoteCurrencyId = secCur.Id AND cr2.BaseCurrencyId = baseCur.Id
WHERE
    c.TransactionTypeId = -1 -- Expenses
    AND t.IsActive = 1
ORDER BY t.Date;");

            migrationBuilder.Sql(@"
CREATE OR ALTER     VIEW V_Income AS
SELECT
    t.Id AS TransactionId,
    t.Date,
    YEAR(t.Date) AS Year,
    MONTH(t.Date) AS Month,
    ISNULL(parent.Name, 'Другое') AS ParentCategory,
    c.Name AS Category,
    ISNULL(c.OrderIndex, 1000000) AS CategoryOrderIndex,
	-t.Amount AS Amount,	
	accCur.Id as Currency,
	CASE 
        WHEN accCur.IsBase = 1 THEN t.Amount
        ELSE t.Amount * cr1.Rate
    END AS AmountInBaseCurrency,    
	baseCur.Id as BaseCurrency,
	CASE 
        WHEN accCur.IsSecondary = 1
        THEN t.Amount
        ELSE (CASE 
            WHEN accCur.IsBase = 1
            THEN t.Amount
            ELSE t.Amount * cr1.Rate
        END) / cr2.Rate
    END AS AmountInSecondaryCurrency,	
	secCur.Id as SecondaryCurrency,    
	a.Name as AccountName
	FROM Transactions t
JOIN Categories c ON t.CategoryId = c.Id
LEFT JOIN Categories parent ON c.ParentId = parent.Id
JOIN Accounts a ON t.AccountId = a.Id
JOIN Currencies accCur ON accCur.Id=a.CurrencyId
JOIN Currencies baseCur on baseCur.IsBase = 1
JOIN Currencies secCur on secCur.IsSecondary = 1
LEFT JOIN CurrencyRates cr1 on cr1.Date=t.Date and cr1.QuoteCurrencyId = accCur.Id AND cr1.BaseCurrencyId = baseCur.Id
LEFT JOIN CurrencyRates cr2 on cr2.Date=t.Date and cr2.QuoteCurrencyId = secCur.Id AND cr2.BaseCurrencyId = baseCur.Id
WHERE
    c.TransactionTypeId = 1 -- Income
    AND t.IsActive = 1;");

            migrationBuilder.Sql(@"
CREATE OR ALTER     VIEW dbo.V_ExpensesAndIncomeByMonth AS
SELECT TOP 1000 e.Y, e.M, e.eamount, i.iamount, i.iamount-e.eamount AS Delta, e.eamount2, i.iamount2
FROM (
	SELECT YEAR(Date) AS Y, Month(Date) AS M, Sum(AmountInBaseCurrency) AS eamount, SUM(AmountInSecondaryCurrency) AS eamount2
	FROM V_Expenses 
	GROUP BY Month(Date), Year(Date)) e
LEFT OUTER JOIN (
	SELECT YEAR(Date) AS Y, Month(Date) AS M, Sum(AmountInBaseCurrency) AS iamount, SUM(AmountInSecondaryCurrency) AS iamount2
	FROM V_Income 
	GROUP BY Month(Date), Year(Date)) i
ON e.Y=i.Y AND e.M=i.M
ORDER BY Y, M;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER             VIEW V_Expenses AS
SELECT TOP 10000
    t.Id AS TransactionId,
    t.Date,
    YEAR(t.Date) AS Year,
    MONTH(t.Date) AS Month,
    ISNULL(parent.Name, 'Другое') AS ParentCategory,
    c.Name AS Category,
    ISNULL(c.OrderIndex, 1000000) AS CategoryOrderIndex,
	-t.Amount AS Amount,	
    SUM(t.Amount) OVER (
        PARTITION BY t.AccountId 
        ORDER BY t.Date
        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    ) AS Balance,	
	accCur.Id as Currency,
	CASE 
        WHEN accCur.IsBase = 1 
        THEN -t.Amount
        ELSE -t.Amount * cr.Rate
    END AS AmountInBaseCurrency,
	baseCur.Id as BaseCurrency,
	a.Name as AccountName
	FROM Transactions t
JOIN Categories c ON t.CategoryId = c.Id
LEFT JOIN Categories parent ON c.ParentId = parent.Id
JOIN Accounts a ON t.AccountId = a.Id
JOIN Currencies accCur ON accCur.Id=a.CurrencyId
JOIN Currencies baseCur on baseCur.IsBase = 1
LEFT JOIN CurrencyRates cr on cr.Date=t.Date and cr.QuoteCurrencyId = accCur.Id AND cr.BaseCurrencyId = baseCur.Id
WHERE
    c.TransactionTypeId = -1 -- Expenses
    AND t.IsActive = 1
ORDER BY t.Date;");
            
            migrationBuilder.Sql(@"
CREATE OR ALTER     VIEW V_Income AS
SELECT
    t.Id AS TransactionId,
    t.Date,
    YEAR(t.Date) AS Year,
    MONTH(t.Date) AS Month,
    ISNULL(parent.Name, 'Другое') AS ParentCategory,
    c.Name AS Category,
    ISNULL(c.OrderIndex, 1000000) AS CategoryOrderIndex,
	-t.Amount AS Amount,	
	accCur.Id as Currency,
	CASE 
        WHEN accCur.IsBase = 1 THEN t.Amount
        ELSE t.Amount * cr1.Rate
    END AS AmountInBaseCurrency,    
	baseCur.Id as BaseCurrency,   
	a.Name as AccountName
	FROM Transactions t
JOIN Categories c ON t.CategoryId = c.Id
LEFT JOIN Categories parent ON c.ParentId = parent.Id
JOIN Accounts a ON t.AccountId = a.Id
JOIN Currencies accCur ON accCur.Id=a.CurrencyId
JOIN Currencies baseCur on baseCur.IsBase = 1
LEFT JOIN CurrencyRates cr1 on cr1.Date=t.Date and cr1.QuoteCurrencyId = accCur.Id AND cr1.BaseCurrencyId = baseCur.Id
WHERE
    c.TransactionTypeId = 1 -- Income
    AND t.IsActive = 1;");
            
            migrationBuilder.Sql(@"
CREATE OR ALTER     VIEW dbo.V_ExpensesAndIncomeByMonth AS
SELECT TOP 1000 e.Y, e.M, e.eamount, i.iamount, i.iamount-e.eamount AS Delta
FROM (
	SELECT YEAR(Date) AS Y, Month(Date) AS M, Sum(AmountInBaseCurrency) AS eamount
	FROM V_Expenses 
	GROUP BY Month(Date), Year(Date)) e
LEFT OUTER JOIN (
	SELECT YEAR(Date) AS Y, Month(Date) AS M, Sum(AmountInBaseCurrency) AS iamount
	FROM V_Income 
	GROUP BY Month(Date), Year(Date)) i
ON e.Y=i.Y AND e.M=i.M
ORDER BY Y, M;");

        }
    }
}
