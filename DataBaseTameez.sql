-- 1. Creating Table User for (Customer or Admin) data
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role NVARCHAR(20) NOT NULL
);

-- 2. Inserting Predetermined Customer & Admin Login data into Table Users
INSERT INTO Users (Username, PasswordHash, Role)
VALUES
    ('customer01', 'customerpass123', 'Customer'),
    ('admin01', 'adminpass123', 'Admin');

-- 3. Creating Table Cart for shopping cart data
CREATE TABLE Cart (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerUsername NVARCHAR(100),
    ProductId NVARCHAR(100),
    Quantity INT
);


-- 4. Retrieving all records from Table Users (to check)
SELECT * FROM Cart;
SELECT * FROM Users;