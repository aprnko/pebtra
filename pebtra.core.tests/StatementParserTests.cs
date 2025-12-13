using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Pebtra.Core.Tests;

[TestFixture]
public class StatementParserTests
{
    private PdfFileReader _pdfReader;
    private StatementFormat _kaspiFormat;
    private StatementFormat _berekeFormat;
    private StatementFormat _homeCreditFormat;
    private StatementFormat _sberFormat;
    private StatementFormat _tinkoffFormat;
    private StatementFormat _kaspiDepositFormat;
    private StatementFormat _homeCreditDepositFormat;
    private StatementFormat _berekeDepositFormat;

    [SetUp]
    public void Setup()
    {
        _pdfReader = new PdfFileReader();
        var formatProvider = new StatementFormatProvider();
        // Store a reference to the formats from the provider
        _kaspiFormat = formatProvider.Get("Kaspi");
        _berekeFormat = formatProvider.Get("Bereke");
        _homeCreditFormat = formatProvider.Get("HomeCredit");
        _sberFormat = formatProvider.Get("Sber");
        _tinkoffFormat = formatProvider.Get("Tinkoff");
        _kaspiDepositFormat = formatProvider.Get("KaspiDeposit");
        _homeCreditDepositFormat = formatProvider.Get("HomeCreditDeposit");
        _berekeDepositFormat = formatProvider.Get("BerekeDeposit");
    }

    #region General Tests

    [Test]
    public void Parse_EmptyLines_ReturnsEmptyList()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiFormat
        };
        var lines = new string[0];

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Parse_IsReverseOrderTrue_ReturnsTransactionsInReversedOrder()
    {
        // Arrange
        var modifiedFormat = new StatementFormat
        {
            LineFormats = _kaspiFormat.LineFormats,
            DateFormat = _kaspiFormat.DateFormat,
            NumberFormat = _kaspiFormat.NumberFormat,
            NumberCulture = _kaspiFormat.NumberCulture,
            IsReverseOrder = true // Override to test reverse order
        };
        
        var parser = new StatementParser
        {
            Format = modifiedFormat
        };
        var lines = new[]
        {
            "23.02.24 + 1 234,56 ₸ CREDIT First transaction",
            "24.02.24 - 500,00 ₸ DEBIT Second transaction",
            "25.02.24 + 2 000,00 ₸ CREDIT Third transaction"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        
        // Verify that the transactions are in reverse order (newest first)
        var firstTransaction = result[0];
        Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2024, 2, 25)));
        Assert.That(firstTransaction.Details, Is.EqualTo("Third transaction"));
        
        var secondTransaction = result[1];
        Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2024, 2, 24)));
        Assert.That(secondTransaction.Details, Is.EqualTo("Second transaction"));
        
        var thirdTransaction = result[2];
        Assert.That(thirdTransaction.Date, Is.EqualTo(new DateOnly(2024, 2, 23)));
        Assert.That(thirdTransaction.Details, Is.EqualTo("First transaction"));
    }

    #endregion

    #region Kaspi Format Tests

    [Test]
    public void Parse_Kaspi_ValidTransactionWithCurrencyDetails_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiFormat
        };
        var lines = new[]
        {
            "23.02.24 + 1 234,56 ₸ CREDIT Card payment",
            "(+ 2 345,67 USD)"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2024, 2, 23)));
            Assert.That(transaction.Amount, Is.EqualTo(1234.56m));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("CREDIT"));
            Assert.That(transaction.Details, Is.EqualTo("Card payment"));
            Assert.That(transaction.CurrencyDetails, Is.EqualTo("+ 2 345,67 USD"));
        });
    }

    [Test]
    public void Parse_Kaspi_ValidTransactionWithoutCurrencyDetails_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiFormat
        };
        var lines = new[]
        {
            "23.02.24 + 1 234,56 ₸ CREDIT Card payment"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2024, 2, 23)));
            Assert.That(transaction.Amount, Is.EqualTo(1234.56m));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("CREDIT"));
            Assert.That(transaction.Details, Is.EqualTo("Card payment"));
            Assert.That(transaction.CurrencyDetails, Is.Null);
        });
    }

    [Test]
    public void Parse_Kaspi_MultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiFormat
        };
        var lines = new[]
        {
            "24.02.24 - 500,00 ₸ DEBIT ATM withdrawal",
            "(- 500,00 USD)",
            "23.02.24 + 1 234,56 ₸ CREDIT Card payment",
            "(+ 1 234,56 USD)"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2024, 2, 23)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(1234.56m));
            Assert.That(firstTransaction.ExtraDetails, Is.EqualTo("CREDIT"));
            Assert.That(firstTransaction.Details, Is.EqualTo("Card payment"));
            Assert.That(firstTransaction.CurrencyDetails, Is.EqualTo("+ 1 234,56 USD"));
        });

        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2024, 2, 24)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(-500.00m));
            Assert.That(secondTransaction.ExtraDetails, Is.EqualTo("DEBIT"));
            Assert.That(secondTransaction.Details, Is.EqualTo("ATM withdrawal"));
            Assert.That(secondTransaction.CurrencyDetails, Is.EqualTo("- 500,00 USD"));
        });
    }

    [Test]
    public void Parse_Kaspi_InvalidLine_IgnoresLine()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiFormat
        };
        var lines = new[]
        {
            "Invalid line",
            "23.02.24 + 1 234,56 ₸ CREDIT Card payment",
            "(+ 1 234,56 USD)"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2024, 2, 23)));
            Assert.That(transaction.Amount, Is.EqualTo(1234.56m));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("CREDIT"));
            Assert.That(transaction.Details, Is.EqualTo("Card payment"));
            Assert.That(transaction.CurrencyDetails, Is.EqualTo("+ 1 234,56 USD"));
        });
    }

    [Test]
    public void Parse_Kaspi_ThreeTransactionsOnlyMiddleHasCurrencyDetails_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiFormat
        };
        var lines = new[]
        {
            "25.02.24 + 2 000,00 ₸ CREDIT Salary",
            "24.02.24 - 500,00 ₸ DEBIT ATM withdrawal",
            "(- 500,00 USD)",
            "23.02.24 + 1 234,56 ₸ CREDIT Card payment",
            "(+ 1 234,56 USD)"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2024, 2, 23)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(1234.56m));
            Assert.That(firstTransaction.ExtraDetails, Is.EqualTo("CREDIT"));
            Assert.That(firstTransaction.Details, Is.EqualTo("Card payment"));
            Assert.That(firstTransaction.CurrencyDetails, Is.EqualTo("+ 1 234,56 USD"));
        });

        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2024, 2, 24)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(-500.00m));
            Assert.That(secondTransaction.ExtraDetails, Is.EqualTo("DEBIT"));
            Assert.That(secondTransaction.Details, Is.EqualTo("ATM withdrawal"));
            Assert.That(secondTransaction.CurrencyDetails, Is.EqualTo("- 500,00 USD"));
        });

        var thirdTransaction = result[2];
        Assert.Multiple(() =>
        {
            Assert.That(thirdTransaction.Date, Is.EqualTo(new DateOnly(2024, 2, 25)));
            Assert.That(thirdTransaction.Amount, Is.EqualTo(2000.00m));
            Assert.That(thirdTransaction.ExtraDetails, Is.EqualTo("CREDIT"));
            Assert.That(thirdTransaction.Details, Is.EqualTo("Salary"));
            Assert.That(thirdTransaction.CurrencyDetails, Is.Null);
        });
    }

    #endregion

    #region Bereke Format Tests

    [Test]
    public void Parse_Bereke_SingleTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _berekeFormat
        };
        var lines = new[]
        {
            "29.01.2025 Оплата товаров и услуг JetPay* Dodopizza.kz -3 990,00 KZT **** 2967",
            "-3 990,00"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 1, 29)));
            Assert.That(transaction.Amount, Is.EqualTo(-3990.00m));
            Assert.That(transaction.Details, Is.EqualTo("Оплата товаров и услуг JetPay* Dodopizza.kz"));
            Assert.That(transaction.CurrencyDetails, Is.EqualTo("-3 990,00 KZT"));
            Assert.That(transaction.ExtraDetails, Is.Null);
        });
    }

    [Test]
    public void Parse_Bereke_MultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _berekeFormat
        };
        var lines = new[]
        {
            "30.01.2025 Оплата товаров и услуг aliexpress -17,10 USD **** 2967",
            "-8 958,69",
            "30.01.2025 Оплата товаров и услуг JetPay*athletex -25 000,00 KZT **** 2967",
            "-25 000,00",
            "29.01.2025 Оплата товаров и услуг JetPay* Dodopizza.kz -3 990,00 KZT **** 2967",
            "-3 990,00"

        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2025, 1, 29)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(-3990.00m));
            Assert.That(firstTransaction.Details, Is.EqualTo("Оплата товаров и услуг JetPay* Dodopizza.kz"));
            Assert.That(firstTransaction.CurrencyDetails, Is.EqualTo("-3 990,00 KZT"));
        });

        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2025, 1, 30)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(-25000.00m));
            Assert.That(secondTransaction.Details, Is.EqualTo("Оплата товаров и услуг JetPay*athletex"));
            Assert.That(secondTransaction.CurrencyDetails, Is.EqualTo("-25 000,00 KZT"));
        });

        var thirdTransaction = result[2];
        Assert.Multiple(() =>
        {
            Assert.That(thirdTransaction.Date, Is.EqualTo(new DateOnly(2025, 1, 30)));
            Assert.That(thirdTransaction.Amount, Is.EqualTo(-8958.69m));
            Assert.That(thirdTransaction.Details, Is.EqualTo("Оплата товаров и услуг aliexpress"));
            Assert.That(thirdTransaction.CurrencyDetails, Is.EqualTo("-17,10 USD"));
        });
    }

    [Test]
    public void Parse_Bereke_DifferentCurrencies_ParsesCorrectly()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _berekeFormat
        };
        var lines = new[]
        {
            "30.01.2025 Оплата товаров и услуг aliexpress -17,10 USD **** 2967",
            "-8 958,69"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 1, 30)));
            Assert.That(transaction.Amount, Is.EqualTo(-8958.69m));
            Assert.That(transaction.Details, Is.EqualTo("Оплата товаров и услуг aliexpress"));
            Assert.That(transaction.CurrencyDetails, Is.EqualTo("-17,10 USD"));
        });
    }

    #endregion

    #region HomeCredit Format Tests

    [Test]
    public void Parse_HomeCredit_SingleTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _homeCreditFormat
        };
        var lines = new[]
        {
            "12.05.2025 TAP-TATTI - 618,00 ₸ + 6,00 Б",
            "5814",
            "19:47",
            "Покупки и траты Fast Food Restaurants"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 5, 12)));
            Assert.That(transaction.Amount, Is.EqualTo(-618.00m));
            Assert.That(transaction.Details, Is.EqualTo("TAP-TATTI"));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("Покупки и траты Fast Food Restaurants (MCC: 5814)"));
        });
    }

    [Test]
    public void Parse_HomeCredit_MultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _homeCreditFormat
        };
        var lines = new[]
        {
            "12.05.2025 APTEKA ARNA - 300,00 ₸ + 3,00 Б",
            "5912",
            "19:43",
            "Покупки и траты Drug Stores, Pharmacies",
            "12.05.2025 TAP-TATTI - 4 708,00 ₸ + 47,00 Б",
            "5814",
            "19:46",
            "Покупки и траты Fast Food Restaurants",
            "12.05.2025 TAP-TATTI - 618,00 ₸ + 6,00 Б",
            "5814",
            "19:47",
            "Покупки и траты Fast Food Restaurants"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2025, 5, 12)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(-618.00m));
            Assert.That(firstTransaction.Details, Is.EqualTo("TAP-TATTI"));
            Assert.That(firstTransaction.ExtraDetails, Is.EqualTo("Покупки и траты Fast Food Restaurants (MCC: 5814)"));
        });

        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2025, 5, 12)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(-4708.00m));
            Assert.That(secondTransaction.Details, Is.EqualTo("TAP-TATTI"));
            Assert.That(secondTransaction.ExtraDetails, Is.EqualTo("Покупки и траты Fast Food Restaurants (MCC: 5814)"));
        });

        var thirdTransaction = result[2];
        Assert.Multiple(() =>
        {
            Assert.That(thirdTransaction.Date, Is.EqualTo(new DateOnly(2025, 5, 12)));
            Assert.That(thirdTransaction.Amount, Is.EqualTo(-300.00m));
            Assert.That(thirdTransaction.Details, Is.EqualTo("APTEKA ARNA"));
            Assert.That(thirdTransaction.ExtraDetails, Is.EqualTo("Покупки и траты Drug Stores, Pharmacies (MCC: 5912)"));
        });
    }

    [Test]
    public void Parse_HomeCredit_PositiveAmount_ParsesCorrectly()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _homeCreditFormat
        };
        var lines = new[]
        {
            "12.05.2025 Salary + 150 000,00 ₸ + 1500,00 Б",
            "5814",
            "12:00",
            "Пополнения Salary"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 5, 12)));
            Assert.That(transaction.Amount, Is.EqualTo(150000.00m));
            Assert.That(transaction.Details, Is.EqualTo("Salary"));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("Пополнения Salary (MCC: 5814)"));
        });
    }

    #endregion

    #region Sber Format Tests

    [Test]
    public void Parse_Sber_SingleTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _sberFormat
        };
        var lines = new[]
        {
            "24.02.2025 07:27 138292 Перевод с карты 20 000,00 21 575,98",
            "24.02.2025 SBOL перевод на платежный счет **3527 К.",
            "ВАЛЕНТИНА ВАЛЕРЬЕВНА. Операция по карте",
            "****0074"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 2, 24)));
            Assert.That(transaction.Amount, Is.EqualTo(-20000.00m));
            Assert.That(transaction.Details, Is.EqualTo("SBOL перевод на платежный счет **3527 К. ВАЛЕНТИНА ВАЛЕРЬЕВНА. Операция по карте ****0074"));
        });
    }

    [Test]
    public void Parse_Sber_PositiveAmountTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _sberFormat
        };
        var lines = new[]
        {
            "12.02.2025 14:20 005911 Прочие операции +14 147,41 56 575,98",
            "12.02.2025 Зачисление. Операция по карте ****0074"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 2, 12)));
            Assert.That(transaction.Amount, Is.EqualTo(14147.41m));
            Assert.That(transaction.Details, Is.EqualTo("Зачисление. Операция по карте ****0074"));
        });
    }

    [Test]
    public void Parse_Sber_MultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _sberFormat
        };
        var lines = new[]
        {
            "24.02.2025 07:27 138292 Перевод с карты 20 000,00 21 575,98",
            "24.02.2025 SBOL перевод на платежный счет **3527 К.",
            "ВАЛЕНТИНА ВАЛЕРЬЕВНА. Операция по карте",
            "****0074",
            "12.02.2025 14:20 005911 Прочие операции +14 147,41 56 575,98",
            "12.02.2025 Зачисление. Операция по карте ****0074"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2025, 2, 24)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(-20000.00m));
            Assert.That(firstTransaction.Details, Is.EqualTo("SBOL перевод на платежный счет **3527 К. ВАЛЕНТИНА ВАЛЕРЬЕВНА. Операция по карте ****0074"));
        });
        
        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2025, 2, 12)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(14147.41m));
            Assert.That(secondTransaction.Details, Is.EqualTo("Зачисление. Операция по карте ****0074"));
        });
    }

    #endregion

    #region Tinkoff Format Tests

    [Test]
    public void Parse_Tinkoff_SingleTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _tinkoffFormat
        };
        var lines = new[]
        {
            "04.03.2023 04.03.2023 -1 000.00 ₽ -1 000.00 ₽ Оплата в 3240",
            "06:59 07:14 EDU.PRODAMUS.ONLINE"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2023, 3, 4)));
            Assert.That(transaction.Amount, Is.EqualTo(-1000.00m));
            Assert.That(transaction.Details, Is.EqualTo("Оплата в EDU.PRODAMUS.ONLINE"));
            Assert.That(transaction.CurrencyDetails, Is.EqualTo("-1 000.00 ₽"));
        });
    }

    [Test]
    public void Parse_Tinkoff_LongerTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _tinkoffFormat
        };
        var lines = new[]
        {
            "03.03.2023 03.03.2023 -20 000.00 ₽ -20 000.00 ₽ Внешний банковский 3846",
            "16:51 17:30 перевод счёт",
            "30111810000000000079,",
            "АО \"РАЙФФАЙЗЕНБАНК\""
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2023, 3, 3)));
            Assert.That(transaction.Amount, Is.EqualTo(-20000.00m));
            Assert.That(transaction.Details, Is.EqualTo("Внешний банковский перевод счёт 30111810000000000079, АО \"РАЙФФАЙЗЕНБАНК\""));
            Assert.That(transaction.CurrencyDetails, Is.EqualTo("-20 000.00 ₽"));
        });
    }

    [Test]
    public void Parse_Tinkoff_PositiveAmountTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _tinkoffFormat
        };
        var lines = new[]
        {
            "01.03.2023 01.03.2023 +20 000.00 ₽ +20 000.00 ₽ Пополнение. Сбербанк 3846",
            "12:01 12:23 Онлайн"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2023, 3, 1)));
            Assert.That(transaction.Amount, Is.EqualTo(20000.00m));
            Assert.That(transaction.Details, Is.EqualTo("Пополнение. Сбербанк Онлайн"));
            Assert.That(transaction.CurrencyDetails, Is.EqualTo("+20 000.00 ₽"));
        });
    }

    [Test]
    public void Parse_Tinkoff_DifferentCurrencyTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _tinkoffFormat
        };
        var lines = new[]
        {
            "21.02.2023 21.02.2023 -780.00 ₸ -136.44 ₽ Оплата в 3846",
            "11:50 13:58 YANDEX*4121*TAXI",
            "MOSKVA RUS"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2023, 2, 21)));
            Assert.That(transaction.Amount, Is.EqualTo(-136.44m));
            Assert.That(transaction.Details, Is.EqualTo("Оплата в YANDEX*4121*TAXI MOSKVA RUS"));
            Assert.That(transaction.CurrencyDetails, Is.EqualTo("-780.00 ₸"));
        });
    }

    [Test]
    public void Parse_Tinkoff_MultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _tinkoffFormat
        };
        var lines = new[]
        {
            "04.03.2023 04.03.2023 -1 000.00 ₽ -1 000.00 ₽ Оплата в 3240",
            "06:59 07:14 EDU.PRODAMUS.ONLINE",
            "03.03.2023 03.03.2023 -20 000.00 ₽ -20 000.00 ₽ Внешний банковский 3846",
            "16:51 17:30 перевод счёт",
            "30111810000000000079,",
            "АО \"РАЙФФАЙЗЕНБАНК\""
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2023, 3, 3)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(-20000.00m));
            Assert.That(firstTransaction.Details, Is.EqualTo("Внешний банковский перевод счёт 30111810000000000079, АО \"РАЙФФАЙЗЕНБАНК\""));
            Assert.That(firstTransaction.CurrencyDetails, Is.EqualTo("-20 000.00 ₽"));
        });
        
        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2023, 3, 4)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(-1000.00m));
            Assert.That(secondTransaction.Details, Is.EqualTo("Оплата в EDU.PRODAMUS.ONLINE"));
            Assert.That(secondTransaction.CurrencyDetails, Is.EqualTo("-1 000.00 ₽"));
        });
    }

    #endregion

    #region KaspiDeposit Format Tests

    [Test]
    public void Parse_KaspiDeposit_SingleTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiDepositFormat
        };
        var lines = new[]
        {
            "01.03.25 +45 465,96 ₸ Вознаграждение Проценты по Kaspi 4 295 465,96 ₸",
            "Депозиту за вычетом",
            "налога 8 023,41 т"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 3, 1)));
            Assert.That(transaction.Amount, Is.EqualTo(45465.96m));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("Вознаграждение"));
            Assert.That(transaction.Details, Is.EqualTo("Проценты по Kaspi Депозиту за вычетом налога 8 023,41 т"));
        });
    }
    
    [Test]
    public void Parse_KaspiDeposit_NegativeAmount_ParsesCorrectly()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiDepositFormat
        };
        var lines = new[]
        {
            "05.03.25 -250 000,00 ₸ Перевод Перевод на карту на 4 045 465,96 ₸",
            "kaspi.kz"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 3, 5)));
            Assert.That(transaction.Amount, Is.EqualTo(-250000.00m));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("Перевод"));
            Assert.That(transaction.Details, Is.EqualTo("Перевод на карту на kaspi.kz"));
        });
    }
    
    [Test]
    public void Parse_KaspiDeposit_MultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _kaspiDepositFormat
        };
        var lines = new[]
        {
            "01.03.25 +45 465,96 ₸ Вознаграждение Проценты по Kaspi 4 295 465,96 ₸",
            "Депозиту за вычетом",
            "налога 8 023,41 т",
            "05.03.25 -250 000,00 ₸ Перевод Перевод на карту на 4 045 465,96 ₸",
            "kaspi.kz"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2025, 3, 5)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(-250000.00m));
            Assert.That(firstTransaction.ExtraDetails, Is.EqualTo("Перевод"));
            Assert.That(firstTransaction.Details, Is.EqualTo("Перевод на карту на kaspi.kz"));
        });
        
        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2025, 3, 1)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(45465.96m));
            Assert.That(secondTransaction.ExtraDetails, Is.EqualTo("Вознаграждение"));
            Assert.That(secondTransaction.Details, Is.EqualTo("Проценты по Kaspi Депозиту за вычетом налога 8 023,41 т"));
        });
    }

    #endregion

    #region HomeCreditDeposit Format Tests

    [Test]
    public void Parse_HomeCreditDeposit_IncomeTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _homeCreditDepositFormat
        };
        var lines = new[]
        {
            "01.03.2025 Капитализация по вкладу 0 1,562.58 43508.57 4055875.14"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 3, 1)));
            Assert.That(transaction.Amount, Is.EqualTo(1562.58m));
            Assert.That(transaction.Details, Is.EqualTo("Капитализация по вкладу"));
        });
    }

    [Test]
    public void Parse_HomeCreditDeposit_ExpenseTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _homeCreditDepositFormat
        };
        var lines = new[]
        {
            "05.03.2025 Снятие средств 10,000.00 0 0 4045875.14"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 3, 5)));
            Assert.That(transaction.Amount, Is.EqualTo(-10000.00m));
            Assert.That(transaction.Details, Is.EqualTo("Снятие средств"));
        });
    }

    [Test]
    public void Parse_HomeCreditDeposit_MultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _homeCreditDepositFormat
        };
        var lines = new[]
        {
            "01.03.2025 Капитализация по вкладу 0 1,562.58 43508.57 4055875.14",
            "02.03.2025 Капитализация по вкладу 0 1,563.18 0 0",
            "05.03.2025 Снятие средств 10,000.00 0 0 4045875.14"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2025, 3, 5)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(-10000.00m));
            Assert.That(firstTransaction.Details, Is.EqualTo("Снятие средств"));
        });
        
        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2025, 3, 2)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(1563.18m));
            Assert.That(secondTransaction.Details, Is.EqualTo("Капитализация по вкладу"));
        });

        var thirdTransaction = result[2];
        Assert.Multiple(() =>
        {
            Assert.That(thirdTransaction.Date, Is.EqualTo(new DateOnly(2025, 3, 1)));
            Assert.That(thirdTransaction.Amount, Is.EqualTo(1562.58m));
            Assert.That(thirdTransaction.Details, Is.EqualTo("Капитализация по вкладу"));
        });
    }

    #endregion

    #region BerekeDeposit Format Tests

    [Test]
    public void Parse_BerekeDeposit_SingleLineTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _berekeDepositFormat
        };
        var lines = new[]
        {
            "12.04.2024 Перенос начисленных процентов на счет до Зачисление 406,72"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2024, 4, 12)));
            Assert.That(transaction.Amount, Is.EqualTo(406.72m));
            Assert.That(transaction.Details, Is.EqualTo("Перенос начисленных процентов на счет до"));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("Зачисление"));
        });
    }

    [Test]
    public void Parse_BerekeDeposit_MultilineTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _berekeDepositFormat
        };
        var lines = new[]
        {
            "12.04.2024 Перенос начисленных процентов на счет до Зачисление 406,72",
            "востребования согласно",
            "условий договора.   KZ12345678901AB234CD",
            "ИВАНОВ ИВАН ИВАНОВИЧ"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2024, 4, 12)));
            Assert.That(transaction.Amount, Is.EqualTo(406.72m));
            Assert.That(transaction.Details, Is.EqualTo("Перенос начисленных процентов на счет до востребования согласно условий договора.   KZ12345678901AB234CD ИВАНОВ ИВАН ИВАНОВИЧ"));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("Зачисление"));
        });
    }

    [Test]
    public void Parse_BerekeDeposit_NegativeAmount_ParsesCorrectly()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _berekeDepositFormat
        };
        var lines = new[]
        {
            "19.04.2024 Перевод между вашими счетами Списание -406,72",
            "KZ98765432109AB876CD и",
            "KZ12345678901AB234CD через B-Bank"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2024, 4, 19)));
            Assert.That(transaction.Amount, Is.EqualTo(-406.72m));
            Assert.That(transaction.Details, Is.EqualTo("Перевод между вашими счетами KZ98765432109AB876CD и KZ12345678901AB234CD через B-Bank"));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("Списание"));
        });
    }

    [Test]
    public void Parse_BerekeDeposit_MultilineComplexTransaction_ParsesCorrectly()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _berekeDepositFormat
        };
        var lines = new[]
        {
            "31.01.2025 Выдача клиенту средств при закрытии Зачисление 3 890,33",
            "вклада. Вклад KZ12345678901AB234CD",
            "ИВАНОВ ИВАН ИВАНОВИЧ ИНН",
            "123456789012. Согласно документу от",
            "31/01/2025 Иностранный паспорт №",
            "AB1234567 выдан МВД РОССИИ 12345",
            "01/01/2020."
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2025, 1, 31)));
            Assert.That(transaction.Amount, Is.EqualTo(3890.33m));
            Assert.That(transaction.ExtraDetails, Is.EqualTo("Зачисление"));
            Assert.That(transaction.Details, Is.EqualTo("Выдача клиенту средств при закрытии вклада. Вклад KZ12345678901AB234CD ИВАНОВ ИВАН ИВАНОВИЧ ИНН 123456789012. Согласно документу от 31/01/2025 Иностранный паспорт № AB1234567 выдан МВД РОССИИ 12345 01/01/2020."));
        });
    }

    [Test]
    public void Parse_BerekeDeposit_MultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = _berekeDepositFormat
        };
        var lines = new[]
        {
            "12.04.2024 Перенос начисленных процентов на счет до Зачисление 406,72",
            "востребования согласно",
            "условий договора.   KZ12345678901AB234CD",
            "ИВАНОВ ИВАН ИВАНОВИЧ",
            "19.04.2024 Перевод между вашими счетами Списание -406,72",
            "KZ98765432109AB876CD и",
            "KZ12345678901AB234CD через B-Bank"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2024, 4, 12)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(406.72m));
            Assert.That(firstTransaction.Details, Is.EqualTo("Перенос начисленных процентов на счет до востребования согласно условий договора.   KZ12345678901AB234CD ИВАНОВ ИВАН ИВАНОВИЧ"));
            Assert.That(firstTransaction.ExtraDetails, Is.EqualTo("Зачисление"));
        });
        
        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2024, 4, 19)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(-406.72m));
            Assert.That(secondTransaction.Details, Is.EqualTo("Перевод между вашими счетами KZ98765432109AB876CD и KZ12345678901AB234CD через B-Bank"));
            Assert.That(secondTransaction.ExtraDetails, Is.EqualTo("Списание"));
        });
    }

    #endregion

    #region BCC Format Tests

    [Test]
    public void Parse_BCC_SingleExpenseTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = new StatementFormatProvider().Get("BCC")
        };
        var lines = new[]
        {
            "1|14.05.2024 10:15:30|3|4|5|6|7|5000.00|0.00|10|11|PAYMENT FOR SERVICES|13|14"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2024, 5, 14)));
            Assert.That(transaction.Amount, Is.EqualTo(-5000.00m));
            Assert.That(transaction.Details, Is.EqualTo("PAYMENT FOR SERVICES"));
        });
    }

    [Test]
    public void Parse_BCC_SingleIncomeTransaction_ReturnsCorrectTransaction()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = new StatementFormatProvider().Get("BCC")
        };
        var lines = new[]
        {
            "1|15.05.2024 11:30:45|3|4|5|6|7|0.00|10000.50|10|11|SALARY PAYMENT|13|14"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var transaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Date, Is.EqualTo(new DateOnly(2024, 5, 15)));
            Assert.That(transaction.Amount, Is.EqualTo(10000.50m));
            Assert.That(transaction.Details, Is.EqualTo("SALARY PAYMENT"));
        });
    }

    [Test]
    public void Parse_BCC_MultipleTransactions_ReturnsAllTransactionsInCorrectOrder()
    {
        // Arrange
        var parser = new StatementParser
        {
            Format = new StatementFormatProvider().Get("BCC")
        };
        var lines = new[]
        {
            "1|10.05.2024 09:15:30|3|4|5|6|7|1500.75|0.00|10|11|GROCERY STORE PURCHASE|13|14",
            "1|12.05.2024 14:25:10|3|4|5|6|7|0.00|5000.00|10|11|TRANSFER FROM ACCOUNT|13|14",
            "1|15.05.2024 17:45:20|3|4|5|6|7|350.25|0.00|10|11|RESTAURANT PAYMENT|13|14"
        };

        // Act
        var result = parser.Parse(lines).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        
        var firstTransaction = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstTransaction.Date, Is.EqualTo(new DateOnly(2024, 5, 10)));
            Assert.That(firstTransaction.Amount, Is.EqualTo(-1500.75m));
            Assert.That(firstTransaction.Details, Is.EqualTo("GROCERY STORE PURCHASE"));
        });
        
        var secondTransaction = result[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondTransaction.Date, Is.EqualTo(new DateOnly(2024, 5, 12)));
            Assert.That(secondTransaction.Amount, Is.EqualTo(5000.00m));
            Assert.That(secondTransaction.Details, Is.EqualTo("TRANSFER FROM ACCOUNT"));
        });
        
        var thirdTransaction = result[2];
        Assert.Multiple(() =>
        {
            Assert.That(thirdTransaction.Date, Is.EqualTo(new DateOnly(2024, 5, 15)));
            Assert.That(thirdTransaction.Amount, Is.EqualTo(-350.25m));
            Assert.That(thirdTransaction.Details, Is.EqualTo("RESTAURANT PAYMENT"));
        });
    }

    #endregion
} 