-- Lumel Sales Analytics Database Schema
-- This script creates the required tables for the sales data analysis system

-- Create database (uncomment if running for the first time)
-- CREATE DATABASE LumelSalesDb;
-- GO
-- USE LumelSalesDb;
-- GO

-- Regions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Regions')
BEGIN
    CREATE TABLE Regions (
        RegionId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL
    );
    
    CREATE UNIQUE INDEX IX_Regions_Name ON Regions(Name);
END
GO

-- Customers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        CustomerId INT PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        Address NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    
    CREATE INDEX IX_Customers_Email ON Customers(Email);
END
GO

-- Products table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        ProductId INT PRIMARY KEY,
        ProductName NVARCHAR(200) NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    
    CREATE INDEX IX_Products_Category ON Products(Category);
END
GO

-- Orders table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        OrderId BIGINT PRIMARY KEY,
        CustomerId INT NOT NULL,
        RegionId INT NOT NULL,
        OrderDate DATE NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_Orders_Regions FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
    );
    
    CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
    CREATE INDEX IX_Orders_RegionId ON Orders(RegionId);
    CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
END
GO

-- OrderItems table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
BEGIN
    CREATE TABLE OrderItems (
        OrderItemId BIGINT IDENTITY(1,1) PRIMARY KEY,
        OrderId BIGINT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
        ShippingCost DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
    );
    
    CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
    CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);
    CREATE INDEX IX_OrderItems_CreatedAt ON OrderItems(CreatedAt);
END
GO

-- DataRefreshLog table for tracking import operations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DataRefreshLog')
BEGIN
    CREATE TABLE DataRefreshLog (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StartedAt DATETIME2 NOT NULL,
        CompletedAt DATETIME2,
        Status NVARCHAR(50) NOT NULL,
        RecordsProcessed INT NOT NULL DEFAULT 0,
        ErrorMessage NVARCHAR(MAX)
    );
END
GO

-- StagingSales table for bulk import staging (optional - used for BULK INSERT operations)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StagingSales')
BEGIN
    CREATE TABLE StagingSales (
        OrderId BIGINT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Discount DECIMAL(18,2) NOT NULL,
        ShippingCost DECIMAL(18,2) NOT NULL
    );
END
GO

PRINT 'Database schema created successfully.';
