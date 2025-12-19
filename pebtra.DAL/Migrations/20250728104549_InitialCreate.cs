using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pebtra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoryMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: false),
                    MatchText = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryMatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    IsBase = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CurrencyId = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    IsTransit = table.Column<bool>(type: "bit", nullable: false),
                    IsSaving = table.Column<bool>(type: "bit", nullable: false),
                    FormatName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UniqueStatementString = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Currencies",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurrencyRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaseCurrencyId = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    QuoteCurrencyId = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyRates_Currencies",
                        column: x => x.BaseCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CurrencyRates_Currencies1",
                        column: x => x.QuoteCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TransactionTypeId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Categories_TransactionTypes1",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))"),
                    AccountId = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoryId = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    ExtraDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyDetails = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Transactions",
                        column: x => x.ParentId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CurrencyId",
                table: "Accounts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TransactionTypeId",
                table: "Categories",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_BaseCurrencyId",
                table: "CurrencyRates",
                column: "BaseCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_QuoteCurrencyId",
                table: "CurrencyRates",
                column: "QuoteCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ParentId",
                table: "Transactions",
                column: "ParentId");

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW V_BalanceByAccount as
SELECT TOP 10000
	AccountId, 
	Name, 
	Date, 
	Balance, 
	CurrencyId
FROM (
    SELECT
        t.*,
        a.Name,
        a.CurrencyId,
        ROW_NUMBER() OVER (
            PARTITION BY accountId
            ORDER BY Date DESC, t.Id DESC
        ) AS rn
    FROM V_DetailedBalance t
    inner join Accounts a on a.Id = t.AccountId
) x
WHERE rn = 1
ORDER BY CurrencyId, AccountId;


-- dbo.V_BalanceByAccountConverted source

CREATE OR ALTER   VIEW V_BalanceByAccountConverted as
	SELECT 
		AccountId,
		v.Date,		
		Balance,
		accCur.Id as BalanceCurrency,
		CASE 
	        WHEN accCur.IsBase = 1 THEN v.Balance
	        ELSE v.Balance * cr.Rate
	    END AS BalanceInBaseCurrency,
		baseCur.Id as BaseCurrency   
	FROM V_BalanceByAccount v
	JOIN Currencies accCur ON accCur.Id = v.CurrencyId
	JOIN Currencies baseCur on baseCur.IsBase = 1
	LEFT JOIN CurrencyRates cr on cr.Date=v.Date and cr.QuoteCurrencyId = accCur.Id AND cr.BaseCurrencyId = baseCur.Id;


-- dbo.V_BalanceByCurrencyConverted source

CREATE OR ALTER   VIEW V_BalanceByCurrencyConverted as
SELECT 
	BalanceCurrency, 
	SUM(Balance) AS Balance,
	SUM(BalanceInBaseCurrency) AS BalanceInBaseCurrency
FROM V_BalanceByAccountConverted GROUP BY BalanceCurrency;


-- dbo.V_DetailedBalance source

CREATE OR ALTER   view V_DetailedBalance as
select TOP 1000000
	AccountId, 
	Date, 
	Id, 
	CategoryId, 
	Amount, 
	Details, 
	Comment,  
	sum(Amount) over (partition by AccountId order by date, id rows between unbounded preceding and current row) as Balance,
	GroupId
from transactions
order by AccountId, Date, Id;


-- dbo.V_DetectReturnedExpensesNextMonth source

CREATE OR ALTER             VIEW V_DetectReturnedExpensesNextMonth AS
SELECT 
    t1.AccountId AS AccountId1,
    t2.AccountId AS AccountId2,    
    t1.CategoryId AS Category,
    t1.Details AS Details1,
    t2.Details AS Details2,
    t1.Date AS T1_Date,
    t2.Date AS T2_Date,   
    t2.Amount AS TAmount,
    t1.Id AS T1_Id,
	t2.Id AS T2_Id,
	t1.GroupId AS T1_GroupId,
	t2.GroupId AS T2_GroupId
FROM Transactions t1
JOIN Transactions t2 
    ON t2.Date >= t1.Date
    AND MONTH(t1.Date) <>  MONTH(t2.Date)
    AND DATEDIFF(DAY, t1.Date, t2.Date) <= 60
    AND t1.Amount = -t2.Amount
    AND t1.Amount < 0
    AND (t1.CategoryId = t2.CategoryId)
JOIN Accounts a1 ON t1.AccountId = a1.Id
JOIN Accounts a2 ON t2.AccountId = a2.Id    
WHERE 
	t1.CategoryId LIKE 'E_%'
	AND (t1.AccountId = t2.AccountId OR a1.CurrencyId = a2.CurrencyId)
	AND t1.AccountId NOT LIKE '%Temp%'
	AND t2.AccountId NOT LIKE '%Temp%'
	AND NOT EXISTS (
	    SELECT 1
	    FROM Transactions t3
	    WHERE 
	        t3.CategoryId = t1.CategoryId
	        AND MONTH(t1.Date) <>  MONTH(t3.Date)
	        AND DATEDIFF(DAY, t1.Date, t3.Date) <= 60
	        AND ABS(t3.Amount) = ABS(t1.Amount)
	        AND t3.Id NOT IN (t1.Id, t2.Id)
	);


-- dbo.V_DetectReturnedExpensesSameMonth source

-- dbo.V_DetectReturnedExpensesSameMonth source

CREATE OR ALTER               VIEW V_DetectReturnedExpensesSameMonth AS
SELECT 
    t1.AccountId AS AccountId1,
    t2.AccountId AS AccountId2,    
    t1.CategoryId AS Category,
    t1.Details AS Details1,
    t2.Details AS Details2,
    t1.Date AS T1_Date,
    t2.Date AS T2_Date,   
    t2.Amount AS TAmount,
    t1.Id AS T1_Id,
	t2.Id AS T2_Id,
	t1.GroupId AS T1_GroupId,
	t2.GroupId AS T2_GroupId
FROM Transactions t1
JOIN Transactions t2 
    ON t2.Date >= t1.Date
    AND MONTH(t1.Date) =  MONTH(t2.Date) 
    AND YEAR(t1.Date) =  YEAR(t2.Date) 
    AND t1.Amount = -t2.Amount
    AND t1.Amount < 0
    AND (t1.CategoryId = t2.CategoryId)
JOIN Accounts a1 ON t1.AccountId = a1.Id
JOIN Accounts a2 ON t2.AccountId = a2.Id    
WHERE 
	t1.CategoryId LIKE 'E_%'
	AND (t1.AccountId = t2.AccountId OR a1.CurrencyId = a2.CurrencyId)
	AND t1.AccountId NOT LIKE '%Temp%'
	AND t2.AccountId NOT LIKE '%Temp%'
	AND NOT EXISTS (
	    SELECT 1
	    FROM Transactions t3
	    WHERE 
	        t3.CategoryId = t1.CategoryId
	        AND MONTH(t1.Date) =  MONTH(t3.Date)
	        AND YEAR(t1.Date) =  YEAR(t3.Date)
	        AND ABS(t3.Amount) = ABS(t1.Amount)
	        AND t3.Id NOT IN (t1.Id, t2.Id)
	);


-- dbo.V_DetectUngroupedConversionMatches source

-- dbo.V_DetectUngroupedConversionMatches source

-- dbo.V_DetectUngroupedConversionMatches source

CREATE OR ALTER       VIEW V_DetectUngroupedConversionMatches AS
select * from (
SELECT 
    t1.Id AS T1_Id,
	t2.Id AS T2_Id,
	t1.Date AS T1_Date,
    t1.AccountId AS T1_AccountId,		
    t1.Amount AS T1_Amount,    
	a1.CurrencyId as T1_Currency,   
	t2.Date AS T2_Date,
	t2.AccountId AS T2_AccountId,    
    t2.Amount AS T2_Amount,
	a2.CurrencyId as T2_Currency,
	-t1.Amount/t2.Amount as TransactionRate,
	cr.Rate as ActualRate,
	CAST(ABS(cr.Rate/(-t1.Amount/t2.Amount) * 100 - 100) as INT) as DeviationPercent,
	t1.Details as T1_Details,
	t1.Comment as T1_Comment,
	t2.Details as T2_Details,
	t2.Comment as T2_Comment,
	t1.GroupId AS T1_GroupId,
	t2.GroupId AS T2_GroupId
FROM Transactions t1
JOIN Transactions t2 
    ON (t1.Date = t2.Date)/* OR (YEAR(t1.Date) = YEAR(t2.Date) AND MONTH(t1.Date) = MONTH(t2.Date) AND ABS(DAY(t1.Date)-DAY(t2.Date)) <= 3)*/
	AND SIGN(t1.Amount)<>SIGN(t2.Amount)
    AND t1.CategoryId = 'C_CONV'
    AND t2.CategoryId = 'C_CONV'
    AND t1.AccountId <> t2.AccountId	
	AND t1.AccountId NOT LIKE '%Temp%'
	AND t2.AccountId NOT LIKE '%Temp%'
INNER JOIN Accounts a1 on t1.AccountId = a1.Id
INNER JOIN Accounts a2 on t2.AccountId = a2.Id
INNER JOIN Currencies c1 on a1.CurrencyId = c1.Id
INNER JOIN Currencies c2 on a2.CurrencyId = c2.Id
INNER JOIN CurrencyRates cr on cr.BaseCurrencyId = c1.Id and cr.QuoteCurrencyId = c2.Id and cr.Date=t1.Date
WHERE a1.CurrencyId <> a2.CurrencyId
AND c1.IsBase = 1
AND NOT EXISTS (SELECT 1 from Transactions t3 WHERE t3.CategoryId='C_CONV' AND t3.Amount = t1.Amount AND t1.Date = t3.Date AND t3.Id NOT IN (t1.Id, t2.Id))																 
AND NOT EXISTS (SELECT 1 from Transactions t3 WHERE t3.CategoryId='C_CONV' AND t3.Amount = t2.Amount AND t2.Date = t3.Date AND t3.Id NOT IN (t1.Id, t2.Id))
AND t1.GroupId is null 
AND t2.GroupId is null
) q WHERE DeviationPercent < 10;


-- dbo.V_DetectUngroupedTransferMatches source

-- dbo.V_DetectUngroupedTransferMatches source

-- dbo.V_DetectUngroupedTransferMatches source

-- dbo.V_DetectUngroupedTransferMatches source

CREATE OR ALTER                 VIEW V_DetectUngroupedTransferMatches AS
SELECT 
    t1.Id AS T1_Id,
	t2.Id AS T2_Id,
    t1.AccountId AS T1_AccountId,
    t2.AccountId AS T2_AccountId,
    t1.Date AS T1_Date,
    t2.Date AS T2_Date,
    t1.Amount AS T1_Amount,    
    t2.Amount AS T2_Amount,
    t1.CategoryId AS T1_CategoryId,    
    t2.CategoryId AS T2_CategoryId,
	t1.Details AS T1_Details,
	t2.Details AS T2_Details,
	t1.GroupId AS T1_GroupId,
	t2.GroupId AS T2_GroupId	
FROM Transactions t1
JOIN Transactions t2 
    ON t1.Date = t2.Date
    AND t1.Amount = -t2.Amount
    AND ((t1.CategoryId = t2.CategoryId AND t1.CategoryId LIKE 'T_%') 
    	OR (t1.CategoryId IS NULL AND t2.CategoryId LIKE 'T_%')
    	OR (t2.CategoryId IS NULL AND t1.CategoryId LIKE 'T_%')
    	OR (t1.CategoryId IS NULL AND t2.CategoryId IS NULL))
    AND t1.AccountId <> t2.AccountId
	AND t1.AccountId NOT LIKE '%Temp%'
	AND t2.AccountId NOT LIKE '%Temp%'
    AND t1.Id < t2.Id  -- avoid duplicates and self-joins
WHERE 
	t1.GroupId IS NULL
	AND t2.GroupId IS NULL
	AND NOT EXISTS (
	    SELECT 1
	    FROM Transactions t3
	    WHERE 
	        t3.CategoryId LIKE 'T_%'
	        AND t3.Date = t1.Date
	        AND ABS(t3.Amount) = ABS(t1.Amount)
	        AND t3.Id NOT IN (t1.Id, t2.Id)
	        AND t3.GroupID IS NULL
		);


-- dbo.V_Expenses source

-- dbo.V_Expenses source

-- dbo.V_Expenses source

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
ORDER BY t.Date;


-- dbo.V_ExpensesAndIncomeByMonth source

-- dbo.V_ExpensesAndIncomeByMonth source

-- dbo.V_ExpensesAndIncomeByMonth source

CREATE OR ALTER       VIEW dbo.V_ExpensesAndIncomeByMonth AS
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


-- dbo.V_ExpensesByMonthDetailed source

-- dbo.V_ExpensesByMonthDetailed source

CREATE OR ALTER     VIEW V_ExpensesByMonthDetailed AS
SELECT TOP 10000 YearMonth, q.Category, q.CategoryOrderIndex, SUM(q.Amount) AS Amount FROM (
  SELECT FORMAT(Date, 'yyyy-MM') AS YearMonth, -AmountInBaseCurrency AS Amount, Category, CategoryOrderIndex FROM V_Expenses   
) q 
GROUP BY  YearMonth, Category, CategoryOrderIndex 
ORDER BY  YearMonth, CategoryOrderIndex;


-- dbo.V_GroupedConversions source

CREATE OR ALTER       VIEW V_GroupedConversions AS
		SELECT 				
			sell.GroupId, 
			sell.TransactionId AS Id1,
			buy.TransactionId AS Id2,
			sell.Date AS Date1,
			buy.Date AS Date2,			
			sell.AccountId AS Acc1,
			buy.AccountId AS Acc2,			
			sell.Amount AS Amt1,
			sell.CurrencyId AS Cur1,		
			buy.Amount AS Amt2,
			buy.CurrencyId AS Cur2,			
			sell.AmountInBaseCurrency AS SellAmtBase,
			buy.AmountInBaseCurrency AS BuyAmtBase,					
			-buy.AmountInBaseCurrency/sell.AmountInBaseCurrency AS RateDifference 
		FROM (
			SELECT * FROM (
				SELECT
				    t.Id AS TransactionId,
				    t.GroupId AS GroupId,
				    t.Amount,
				    t.Date,
				    t.AccountId,
				    a.CurrencyId,
				    cr.Rate,
					CASE WHEN accCur.IsBase = 1 THEN t.Amount ELSE t.Amount * cr.Rate END AS AmountInBaseCurrency					
					FROM Transactions t
				JOIN Accounts a ON t.AccountId = a.Id
				JOIN Currencies accCur ON accCur.Id=a.CurrencyId
				JOIN Currencies baseCur on baseCur.IsBase = 1
				LEFT JOIN CurrencyRates cr on cr.Date=t.Date and cr.QuoteCurrencyId = accCur.Id AND cr.BaseCurrencyId = baseCur.Id
				WHERE
				    t.CategoryId LIKE 'C_CONV'
				    AND t.IsActive = 1) q1
			WHERE AmountInBaseCurrency < 0) sell
		INNER JOIN (
			SELECT * FROM (
				SELECT
				    t.Id AS TransactionId,
				    t.GroupId AS GroupId,
				    t.Amount,
				    t.Date,
				    t.AccountId,	
				    a.CurrencyId,
				    cr.Rate,
					CASE WHEN accCur.IsBase = 1 THEN t.Amount ELSE t.Amount * cr.Rate END AS AmountInBaseCurrency
					FROM Transactions t
				JOIN Accounts a ON t.AccountId = a.Id
				JOIN Currencies accCur ON accCur.Id=a.CurrencyId
				JOIN Currencies baseCur on baseCur.IsBase = 1
				LEFT JOIN CurrencyRates cr on cr.Date=t.Date and cr.QuoteCurrencyId = accCur.Id AND cr.BaseCurrencyId = baseCur.Id
				WHERE
				    t.CategoryId LIKE 'C_CONV'
				    AND t.IsActive = 1) q2
		    WHERE AmountInBaseCurrency > 0) buy
		ON sell.GroupId = buy.GroupId;


-- dbo.V_GroupedTransfers source

CREATE OR ALTER       VIEW V_GroupedTransfers AS
SELECT 
	tout.GroupId, 
	tout.Id AS Id1, 
	tout.Date AS Date1,	
	tout.AccountId AS Acc1,	
	tout.Amount AS Amt1,	
	tout.Details AS Det1,	
	tin.Id AS Id2,  
	tin.Date AS Date2,  
	tin.AccountId AS Acc2,  
	tin.Amount AS Amt2,  
	tin.Details AS Det2
FROM (
	SELECT GroupId, Id, Date, AccountId, Amount, Details, ExtraDetails FROM TRANSACTIONS
	WHERE GroupID IN (
		SELECT GroupId 
		FROM (
			SELECT COUNT(1) AS cnt, GroupId
			FROM Transactions 
			WHERE CategoryId LIKE 'T_%' AND Amount < 0
			GROUP BY GroupId) g1
		WHERE cnt = 1)
	AND Amount < 0		
	UNION
	SELECT GroupId, NULL, MAX(Date), NULL, SUM(amount), CONCAT(COUNT(amount), ' transactions'), NULL FROM TRANSACTIONS
	WHERE GroupID IN (
		SELECT GroupId 
		FROM (
			SELECT COUNT(1) AS cnt, GroupId
			FROM Transactions 
			WHERE CategoryId LIKE 'T_%' AND Amount < 0
			GROUP BY GroupId) g1
		WHERE cnt > 1)
	AND Amount < 0	
	GROUP BY GroupId) tout
INNER JOIN (
	SELECT GroupId, Id, Date, AccountId, Amount, Details, ExtraDetails FROM TRANSACTIONS
	WHERE GroupID IN (
		SELECT GroupId 
		FROM (
			SELECT COUNT(1) AS cnt, GroupId
			FROM Transactions 
			WHERE CategoryId LIKE 'T_%' AND Amount > 0
			GROUP BY GroupId) g1
		WHERE cnt = 1)
	AND Amount > 0		
	UNION
	SELECT GroupId, NULL, MAX(date), NULL, SUM(amount), CONCAT(COUNT(amount), ' transactions'), NULL FROM TRANSACTIONS
	WHERE GroupID IN (
		SELECT GroupId 
		FROM (
			SELECT COUNT(1) AS cnt, GroupId
			FROM Transactions 
			WHERE CategoryId LIKE 'T_%' AND Amount > 0
			GROUP BY GroupId) g1
		WHERE cnt > 1)
	AND Amount > 0	
	GROUP BY GroupId) tin
ON tin.GroupId = tout.GroupId;


-- dbo.V_Income source

-- dbo.V_Income source

-- dbo.V_Income source

CREATE OR ALTER       VIEW V_Income AS
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
    AND t.IsActive = 1;


-- dbo.V_IncomeByMonthDetailed source

-- dbo.V_IncomeByMonthDetailed source

CREATE OR ALTER     VIEW V_IncomeByMonthDetailed AS
SELECT TOP 10000 YearMonth, q.Category, q.CategoryOrderIndex, SUM(q.Amount) AS Amount FROM (
  SELECT FORMAT(Date, 'yyyy-MM') AS YearMonth, AmountInBaseCurrency AS Amount, Category, CategoryOrderIndex FROM V_Income
) q 
GROUP BY  YearMonth, Category, CategoryOrderIndex 
ORDER BY  YearMonth, CategoryOrderIndex;


-- dbo.V_LastTransactionsByAccount source

CREATE OR ALTER VIEW V_LastTransactionsByAccount AS
SELECT TOP 1000 * FROM (SELECT max(date) AS LastDate, AccountId FROM Transactions t GROUP BY AccountId) q ORDER BY LastDate;


-- dbo.V_ValidateTransactions source

-- dbo.V_ValidateTransactions source

-- dbo.V_ValidateTransactions source

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
ORDER BY Date;


-- dbo.v_AccountDailyBalances source

CREATE OR ALTER VIEW v_AccountDailyBalances AS
SELECT TOP 100000
    t.AccountId,
    t.Date,
    SUM(SUM(t.Amount)) OVER (
        PARTITION BY t.AccountId
        ORDER BY t.Date 
        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    ) AS Balance
FROM Transactions t
GROUP BY t.AccountId, t.Date
ORDER BY t.Date DESC, t.AccountId;");

            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[Categorize]
AS
BEGIN
    SET NOCOUNT ON;

    -- Update all transactions with NULL CategoryId where Details contains MatchText
    UPDATE T
    SET T.CategoryId = CM.CategoryId
    FROM [dbo].[Transactions] T
    INNER JOIN [dbo].[CategoryMatches] CM
        ON (T.CategoryId IS NULL or T.CategoryId = 'X_ND' or T.CategoryId like 'E_UNKN%' or T.CategoryId like 'I_UNKN%')
        AND T.Details LIKE '%' + CM.MatchText + '%';
END;

CREATE     PROCEDURE CreateTransitTransaction
    @TransactionId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Date DATE,
        @Amount DECIMAL(18,2),
        @Details NVARCHAR(1000),
        @CategoryId CHAR(10),
        @OriginalAccountId CHAR(10),
        @CurrencyId CHAR(10),
        @TransitAccountId CHAR(10),
        @NewTransactionId INT,
        @GroupId UNIQUEIDENTIFIER;

    -- Fetch original transaction details
    SELECT 
        @Date = [Date],
        @Amount = [Amount],
        @Details = [Details],
        @CategoryId = [CategoryId],
        @OriginalAccountId = [AccountId],
        @GroupId = [GroupId]
    FROM Transactions
    WHERE Id = @TransactionId;

    IF @Date IS NULL
    BEGIN
        RAISERROR('Transaction with the specified ID does not exist.', 16, 1);
        RETURN;
    END

    IF @CategoryId <> 'T_TRANSFER'
    BEGIN
        RAISERROR('Transaction CategoryId must be T_TRANSFER.', 16, 1);
        RETURN;
    END

    IF @GroupId IS NOT NULL
    BEGIN
        RAISERROR('Transaction is already part of a group.', 16, 1);
        RETURN;
    END

    -- Get CurrencyId of the original account
    SELECT @CurrencyId = CurrencyId
    FROM Accounts
    WHERE Id = @OriginalAccountId;

    IF @CurrencyId IS NULL
    BEGIN
        RAISERROR('CurrencyId not found for the original account.', 16, 1);
        RETURN;
    END

    -- Find the transit account with the same currency
    SELECT TOP 1 @TransitAccountId = Id
    FROM Accounts
    WHERE CurrencyId = @CurrencyId AND IsTransit = 1;

    IF @TransitAccountId IS NULL
    BEGIN
        RAISERROR('No transit account found for the currency.', 16, 1);
        RETURN;
    END

    -- Insert the transit transaction
    INSERT INTO Transactions (
        IsActive,
        AccountId,
        [Date],
        Amount,
        CategoryId,
        Details,
        Comment,
        GroupId,
        ParentId
    )
    VALUES (
        1,                      -- IsActive
        @TransitAccountId,
        @Date,
        -1 * @Amount,
        'T_TRANSFER',
        @Details,
        NULL,                   -- Comment
        NULL,                   -- GroupId
        NULL                    -- ParentId
    );

    -- Get ID of the newly inserted transaction
    SET @NewTransactionId = SCOPE_IDENTITY();

    -- Link the original and new transactions
    EXEC LinkDirect @TransactionId, @NewTransactionId;
END;

CREATE           PROCEDURE LinkDirect
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
   
END;

CREATE     PROCEDURE [dbo].[LinkTransit]
    @Id1 INT,
    @Id2 INT,
    @Id3 INT = NULL,
    @Id4 INT = NULL,
    @Id5 INT = NULL,
    @Id6 INT = NULL,
    @Id7 INT = NULL,
    @Id8 INT = NULL,
    @Id9 INT = NULL,
    @Id10 INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TransactionIds TABLE (Id INT);

    INSERT INTO @TransactionIds (Id)
    SELECT Id FROM (VALUES (@Id1), (@Id2), (@Id3), (@Id4), (@Id5),
                            (@Id6), (@Id7), (@Id8), (@Id9), (@Id10)) AS t(Id)
    WHERE Id IS NOT NULL;

    IF (SELECT COUNT(*) FROM @TransactionIds) < 2
    BEGIN
        RAISERROR('At least two transaction IDs must be provided.', 16, 1);
        RETURN;
    END

    -- Fetch all relevant transactions
    DECLARE @Tx TABLE (
        Id INT,
        AccountId CHAR(10),
        Amount DECIMAL(18,2),
        CategoryId CHAR(10),
        [Date] DATE,
        GroupId UNIQUEIDENTIFIER,
        CurrencyId CHAR(10)
    );

    INSERT INTO @Tx
    SELECT t.Id, t.AccountId, t.Amount, t.CategoryId, t.[Date], t.GroupId, a.CurrencyId
    FROM Transactions t
    JOIN Accounts a ON t.AccountId = a.Id
    WHERE t.Id IN (SELECT Id FROM @TransactionIds);

    IF (SELECT COUNT(*) FROM @Tx) <> (SELECT COUNT(*) FROM @TransactionIds)
    BEGIN
        RAISERROR('One or more transactions not found.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM @Tx WHERE (CategoryId NOT LIKE 'T_%') AND (CategoryId NOT LIKE 'E_%'))
    BEGIN
        RAISERROR('All transactions must have either Transfer (including Return) or Expense category.', 16, 1);
        RETURN;
    END
    
    IF (SELECT COUNT(DISTINCT CategoryId) FROM @Tx) > 1
    BEGIN
        RAISERROR('All specified transactions must have the same CategoryId.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM @Tx WHERE GroupId IS NOT NULL)
    BEGIN
        RAISERROR('All transactions must be ungrouped.', 16, 1);
        RETURN;
    END

    DECLARE @SourceAcc CHAR(10), @DestAcc CHAR(10), @Currency CHAR(10);

    SELECT TOP 1 @SourceAcc = AccountId FROM @Tx WHERE Amount < 0;
    SELECT TOP 1 @DestAcc = AccountId FROM @Tx WHERE Amount > 0;

    IF EXISTS (
        SELECT 1 FROM @Tx WHERE Amount < 0 AND AccountId <> @SourceAcc
    ) OR EXISTS (
        SELECT 1 FROM @Tx WHERE Amount > 0 AND AccountId <> @DestAcc
    )
    BEGIN
        RAISERROR('All negative or positive transactions must belong to the same account.', 16, 1);
        RETURN;
    END

    SELECT TOP 1 @Currency = CurrencyId FROM @Tx;

    IF EXISTS (SELECT 1 FROM @Tx WHERE CurrencyId <> @Currency)
    BEGIN
        RAISERROR('All transactions must have the same currency.', 16, 1);
        RETURN;
    END

    IF ABS((SELECT SUM(Amount) FROM @Tx)) >= 1
    BEGIN
        RAISERROR('Sum of all amounts must be close to zero.', 16, 1);
        RETURN;
    END

    DECLARE @TransitAccountId CHAR(10);
    SELECT TOP 1 @TransitAccountId = Id
    FROM Accounts
    WHERE CurrencyId = @Currency AND IsTransit = 1;

    IF @TransitAccountId IS NULL
    BEGIN
        RAISERROR('No transit account found for the currency.', 16, 1);
        RETURN;
    END

    DECLARE @NegSum DECIMAL(18,2), @PosSum DECIMAL(18,2);
    DECLARE @NegDetails NVARCHAR(1000), @PosDetails NVARCHAR(1000);
    DECLARE @NegDate DATE, @PosDate DATE;
    DECLARE @Group1 UNIQUEIDENTIFIER = NEWID(), @Group2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @CategoryId NVARCHAR(10);

    SELECT 
        @NegSum = -1 * SUM(Amount),
        @NegDetails = STRING_AGG(CAST(-Amount AS NVARCHAR), '+'),
        @NegDate = MAX([Date])        
    FROM @Tx WHERE Amount < 0;

    SELECT 
        @PosSum = -1 * SUM(Amount),
        @PosDetails = STRING_AGG(CAST(Amount AS NVARCHAR), '+'),
        @PosDate = MIN([Date])
    FROM @Tx WHERE Amount > 0;
    
    -- If CategoryId is an expense category, change it to T_RETURN
    SELECT TOP 1 @CategoryId = CASE WHEN CategoryId LIKE 'E_%' THEN 'T_RETURN' ELSE CategoryId END FROM @Tx

    DECLARE @NegTxId INT, @PosTxId INT;

    INSERT INTO Transactions (IsActive, AccountId, [Date], Amount, CategoryId, Details, Comment, GroupId)
    VALUES (1, @TransitAccountId, @NegDate, @NegSum, @CategoryId, @NegDetails, @SourceAcc, @Group1);

    SET @NegTxId = SCOPE_IDENTITY();

    INSERT INTO Transactions (IsActive, AccountId, [Date], Amount, CategoryId, Details, Comment, GroupId)
    VALUES (1, @TransitAccountId, @PosDate, @PosSum, @CategoryId, @PosDetails, @DestAcc, @Group2);

    SET @PosTxId = SCOPE_IDENTITY();

    -- Update GroupId for transactions
    UPDATE t
    SET GroupId = @Group1, CategoryId = @CategoryId
    FROM Transactions t
    WHERE t.Id IN (SELECT Id FROM @Tx WHERE Amount < 0);

    UPDATE t
    SET GroupId = @Group2, CategoryId = @CategoryId
    FROM Transactions t
    WHERE t.Id IN (SELECT Id FROM @Tx WHERE Amount > 0);

    PRINT 'Created transit transaction for negative group: ' + CAST(@NegTxId AS NVARCHAR);
    PRINT 'Created transit transaction for positive group: ' + CAST(@PosTxId AS NVARCHAR);
END;

CREATE   PROCEDURE Unlink
    @Id1 INT,
    @Id2 INT,
    @Id3 INT = NULL,
    @Id4 INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Build table of input IDs
    DECLARE @Ids TABLE (Id INT);
    INSERT INTO @Ids (Id) VALUES (@Id1), (@Id2);
    IF @Id3 IS NOT NULL INSERT INTO @Ids VALUES (@Id3);
    IF @Id4 IS NOT NULL INSERT INTO @Ids VALUES (@Id4);

    -- Select involved transactions
    DECLARE @Selected TABLE (
        Id INT,
        GroupId UNIQUEIDENTIFIER
    );

    INSERT INTO @Selected (Id, GroupId)
    SELECT t.Id, t.GroupId
    FROM Transactions t
    INNER JOIN @Ids i ON t.Id = i.Id;

    -- Validation 1: all records must exist
    IF (SELECT COUNT(*) FROM @Ids) != (SELECT COUNT(*) FROM @Selected)
    BEGIN
        RAISERROR('One or more specified transaction IDs do not exist.', 16, 1);
        RETURN;
    END

    -- Validation 2: all records must have the same non-null GroupId
    IF EXISTS (
        SELECT 1
        FROM @Selected
        WHERE GroupId IS NULL
    )
    BEGIN
        RAISERROR('One or more specified transactions have no GroupId.', 16, 1);
        RETURN;
    END

    IF (SELECT COUNT(DISTINCT GroupId) FROM @Selected) > 1
    BEGIN
        RAISERROR('Specified transactions do not share the same GroupId.', 16, 1);
        RETURN;
    END

    -- All checks passed — unlink
    UPDATE Transactions
    SET GroupId = NULL
    WHERE Id IN (SELECT Id FROM @Ids);
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE Categorize
DROP PROCEDURE CreateTransitTransaction
DROP PROCEDURE LinkDirect
DROP PROCEDURE LinkTransit
DROP PROCEDURE Unlink

DROP VIEW V_BalanceByAccount
DROP VIEW V_BalanceByAccountConverted
DROP VIEW V_BalanceByCurrencyConverted
DROP VIEW V_DetailedBalance
DROP VIEW V_DetectReturnedExpensesNextMonth
DROP VIEW V_DetectReturnedExpensesSameMonth
DROP VIEW V_DetectUngroupedConversionMatches
DROP VIEW V_DetectUngroupedTransferMatches
DROP VIEW V_Expenses
DROP VIEW V_ExpensesAndIncomeByMonth
DROP VIEW V_ExpensesByMonthDetailed
DROP VIEW V_GroupedConversions
DROP VIEW V_GroupedTransfers
DROP VIEW V_Income
DROP VIEW V_IncomeByMonthDetailed
DROP VIEW V_LastTransactionsByAccount
DROP VIEW V_ValidateTransactions
DROP VIEW v_AccountDailyBalances
");

            migrationBuilder.DropTable(
                name: "CategoryMatches");

            migrationBuilder.DropTable(
                name: "CurrencyRates");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "TransactionTypes");
        }
    }
}
