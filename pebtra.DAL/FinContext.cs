using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace pebtra.DAL
{
    public partial class FinContext : DbContext
    {
        public FinContext()
        {
        }

        public FinContext(DbContextOptions<FinContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryMatch> CategoryMatches { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<CurrencyRate> CurrencyRates { get; set; }
        public virtual DbSet<ImportSession> ImportSessions { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransactionType> TransactionTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default connection string if none provided
                optionsBuilder.UseSqlServer("Data Source=(local);Initial Catalog=fin;Integrated Security=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CurrencyId)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Name).HasMaxLength(40);
                entity.Property(e => e.UniqueStatementString).HasMaxLength(100);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Accounts_Currencies");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Name).HasMaxLength(40);

                entity.Property(e => e.ParentId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Categories_Categories");

                entity.HasOne(d => d.TransactionType)
                    .WithMany(p => p.Categories)
                    .HasForeignKey(d => d.TransactionTypeId)
                    .HasConstraintName("FK_Categories_TransactionTypes1");
            });

            modelBuilder.Entity<CategoryMatch>(entity =>
            {
                entity.ToTable("CategoryMatches");

                entity.Property(e => e.CategoryId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.MatchText)
                    .IsRequired()
                    .HasMaxLength(40);
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("Currencies");

                entity.Property(e => e.Id)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<CurrencyRate>(entity =>
            {
                entity.ToTable("CurrencyRates");

                entity.Property(e => e.BaseCurrencyId)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.QuoteCurrencyId)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.BaseCurrency)
                    .WithMany(p => p.CurrencyRateBaseCurrencies)
                    .HasForeignKey(d => d.BaseCurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CurrencyRates_Currencies");

                entity.HasOne(d => d.QuoteCurrency)
                    .WithMany(p => p.CurrencyRateQuoteCurrencies)
                    .HasForeignKey(d => d.QuoteCurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CurrencyRates_Currencies1");
            });

            modelBuilder.Entity<ImportSession>(entity =>
            {
                entity.ToTable("ImportSessions");

                entity.Property(e => e.AccountId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Filename)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.StartDate).HasColumnType("date");
                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.HasOne(d => d.Account)
                    .WithMany()
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ImportSessions_Accounts");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");

                entity.Property(e => e.AccountId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CategoryId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Comment).HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transactions_Accounts");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_Transactions_Categories");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Transactions_Transactions");

                entity.HasOne(d => d.ImportSession)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.ImportSessionId)
                    .HasConstraintName("FK_Transactions_ImportSessions");
            });

            modelBuilder.Entity<TransactionType>(entity =>
            {
                entity.ToTable("TransactionTypes");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
} 