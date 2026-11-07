namespace Temelie.Database.UnitTests;

/// <summary>Small SQL Server databases covering every scripted object type.</summary>
internal static class Samples
{
    public const string CustomerView = "CREATE VIEW [dbo].[CustomerView] AS SELECT [Id], [Name] FROM [dbo].[Customer]";
    public const string GetCustomers = "CREATE PROCEDURE [dbo].[GetCustomers] AS SELECT [Id] FROM [dbo].[Customer]";
    public const string CustomerTrigger = "CREATE TRIGGER [dbo].[trCustomer] ON [dbo].[Customer] AFTER INSERT AS SELECT 1";

    public static void Customers(DatabaseState db)
    {
        db.Tables.Add(Db.Table("dbo", "Customer", ("Id", "INT"), ("Name", "NVARCHAR")));
        db.Tables.Add(Db.Table("dbo", "Order", ("Id", "INT"), ("CustomerId", "INT")));
        db.Indexes.Add(Db.PrimaryKey("dbo", "Customer", "PK_Customer", "Id"));
        db.Indexes.Add(Db.PrimaryKey("dbo", "Order", "PK_Order", "Id"));
        db.Indexes.Add(Db.Index("dbo", "Customer", "IX_Customer_Name", "Name"));
        db.CheckConstraints.Add(Db.Check("dbo", "Customer", "CK_Customer_Id", "([Id]>(0))"));
        db.Definitions.Add(Db.Procedure("dbo", "GetCustomers", GetCustomers));
        db.Definitions.Add(Db.View("dbo", "CustomerView", CustomerView));
        db.Triggers.Add(Db.Trigger("dbo", "Customer", "trCustomer", CustomerTrigger));
        db.ForeignKeys.Add(Db.ForeignKey("dbo", "Order", "FK_Order_Customer", "CustomerId", "Customer", "Id"));
    }
}
