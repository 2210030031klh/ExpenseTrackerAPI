using Microsoft.EntityFrameworkCore;

using ExpenseApi.Models;

namespace ExpenseApi.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Income> Incomes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(

            new Category { CategoryId = 1, CategoryName = "Food", Type = "Expense" },
            new Category { CategoryId = 2, CategoryName = "Transport", Type = "Expense" },
            new Category { CategoryId = 3, CategoryName = "Shopping", Type = "Expense" },
            new Category { CategoryId = 4, CategoryName = "Bills", Type = "Expense" },
            new Category { CategoryId = 5, CategoryName = "Health", Type = "Expense" },
            new Category { CategoryId = 6, CategoryName = "Entertainment", Type = "Expense" },
            new Category { CategoryId = 7, CategoryName = "Loans", Type = "Expense" },

            new Category { CategoryId = 8, CategoryName = "Salary", Type = "Income" },
            new Category { CategoryId = 9, CategoryName = "Freelance", Type = "Income" },
            new Category { CategoryId = 10, CategoryName = "Business", Type = "Income" },
            new Category { CategoryId = 11, CategoryName = "Trading", Type = "Income" },
            new Category { CategoryId = 12, CategoryName = "Investment", Type = "Income" }
        );
    }
    
    
}