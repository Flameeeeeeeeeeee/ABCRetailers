# 🛒 ABCRetailers E-commerce Platform

## overview

ABCRetailers is a comprehensive e-commerce web application built to demonstrate a modern, decoupled cloud architecture. The system is comprised of two distinct projects: a front-end **ASP.NET Core MVC application** and a serverless **Azure Functions backend API**.

This project fulfills all requirements from its three-part development, including:

  * Full admin-facing CRUD (Create, Read, Update, Delete) operations for products, customers, and orders.
  * A complete, custom-built manual authentication system with distinct **Admin** and **Customer** roles.
  * A persistent, user-specific shopping cart stored in **Azure SQL**.
  * A hybrid database model, using **Azure SQL** for user identity and **Azure Table Storage** for business data.
  * Asynchronous order processing using **Azure Queue Storage**.

## 🏛️ Core Architecture

The application uses a sophisticated, decoupled architecture to separate concerns:

1.  **`ABCRetailers` (The MVC Frontend):** This is the user-facing application hosted on **Azure App Service**. It is responsible for all user interaction, rendering views, and managing user state. It contains the `LoginController` for manual authentication and the `CartController` for the shopping cart. This project connects directly to an **Azure SQL Database** to manage its `Users` and `Cart` tables.

2.  **`ABCRetailersFunctions` (The Functions Backend):** This is the stateless, serverless backend API hosted on an **Azure Function App**. It acts as the "engine" for all core business logic, managing the permanent `Product`, `Customer`, and `Order` data in **Azure Table Storage**.

3.  **`FunctionsApiClient` (The "Bridge"):** The MVC frontend communicates with the backend *exclusively* through this `FunctionsApiClient` service. This client calls the various HTTP-triggered functions (e.g., `Products_Create`, `Orders_GetByCustomerIdAsync`), ensuring the frontend and backend are completely decoupled.

4.  **Data Synchronization:** To bridge the two database systems, the `LoginController` (on the MVC app) calls the `Customers_Create` function (in the Functions app) during registration. This function has been modified to accept the new SQL `User.Id` and use it as the `RowKey` for the new `CustomerEntity` in Table Storage. This ensures a 1:1 link between a user's login and their business data, allowing the `CartController` to successfully validate a user and create an order.

## ✨ Core Features

  * **Role-Based Authentication:** A complete, custom-built login and registration system. Users are assigned "Admin" or "Customer" roles stored in Azure SQL.
  * **Role-Specific Dashboards:** After login, users are redirected to a custom dashboard:
      * **Admin Dashboard:** Provides links to manage all customers, products, and orders.
      * **Customer Dashboard:** Provides links to browse products, view the cart, and see personal order history.
  * **Product Management (Admin):** Full CRUD functionality for products, including image uploads to **Azure Blob Storage**.
  * **Customer Management (Admin):** Full CRUD functionality for customer records in **Azure Table Storage**.
  * **Order Management (Admin):** Admins can view *all* orders from *all* customers and update their status (e.g., "Submitted" to "Processing").
  * **Persistent Shopping Cart (Customer):** A fully functional shopping cart. Customers can add items, update quantities, and remove items. The cart is stored in the **Azure SQL `Cart` table**, so it persists between logins.
  * **Customer Checkout:** The cart's `Checkout` action validates the user, calls the `Orders_Create` function for each item, and clears the SQL cart.
  * **Customer Order History (My Orders):** A "Your Orders" page that calls a custom Azure Function (`Orders_GetByCustomerIdAsync`) to retrieve and display *only* the orders for the logged-in customer.
  * **Asynchronous Processing:** Order creation and status updates automatically drop messages on an **Azure Queue**, allowing for decoupled background processing (like email notifications).

## 💻 Technology Stack

  * **Frontend:** ASP.NET Core MVC (`.NET 9`)
  * **Backend:** Azure Functions (Serverless API, `.NET 9`)
  * **Authentication Database:** Azure SQL Database (for `Users` and `Cart` tables)
  * **Business Database:** Azure Table Storage (for `Customers`, `Products`, and `Orders`)
  * **File Storage:** Azure Blob Storage (for product images) & Azure File Share (for payment proofs)
  * **Messaging:** Azure Queue Storage (for asynchronous order notifications)
  * **Hosting:** Azure App Service (for MVC app) & Azure Function App

## 🏗️ Project Structure

```
/ABCRetailers (The MVC Frontend)
    /Controllers    (LoginController, CartController, OrderController, ProductController, etc.)
    /Views
        /Home       (AdminDashboard.cshtml, CustomerDashboard.cshtml)
        /Cart       (Index.cshtml, Confirmation.cshtml)
        /Login      (Index.cshtml, Register.cshtml, AccessDenied.cshtml)
        /Order      (Manage.cshtml, MyOrders.cshtml)
    /Models         (User.cs, Cart.cs etc.)
    /ViewModels     (LoginViewModel.cs, RegisterViewModel.cs, CartPageViewModel.cs)
    /Data           (AuthDbContext.cs)
    /Services       (IFunctionsApi.cs, FunctionsApiClient.cs)
    appsettings.json

/ABCRetailersFunctions (The Serverless Backend)
    /Functions      (CustomersFunctions.cs, OrdersFunctions.cs, ProductsFunctions.cs, etc.)
    /Entities       (CustomerEntity.cs, OrderEntity.cs, ProductEntity.cs)
    /Helpers
    local.settings.json

database-setup.sql
```

## 🚀 Setup & Usage

To run this project locally, a multi setup is required.

1.  **Setup the SQL Database:**

      * You must have an Azure SQL Database.
      * Open the `database-setup.sql` script in SSMS and execute it against your database. This will create the `Users` and `Cart` tables and seed the initial `admin01` account.

2.  **Configure the `ABCRetailersFunctions` (Backend):**

      * Open `local.settings.json`.
      * Update `STORAGE_CONNECTION` with your connection string from your Azure Storage Account.
      * Run this project. It will start the backend API, typically on `http://localhost:7251`.

3.  **Configure the `ABCRetailers` (Frontend):**

      * Open `appsettings.Development.json`.
      * Update `ConnectionStrings:DefaultConnection` to point to your Azure SQL database.
      * Update `ConnectionStrings:AzureStorage` with your Storage Account string.
      * Update `Functions:BaseUrl` to the correct local URL of your running Functions project (e.g., `http://localhost:7251`).

4.  **Run the Project:**

      * Set a new start up profile and add both of the functions and mvc on the startup project and run it.
      * You can now log in as `admin02` or register a new customer or admin to test the full data synchronization and checkout flow.

 5. **login as admin or Custumer**
      * Admin Login Details: The database-setup.sql script automatically creates a default administrator account. You can log in using the following credentials:
      * Username: admin01
      * Password: adminpass123
      * Role: Admin

      * **or create a customer for example**
      * Username: JD66
      * Password: Password45-1
      * Role: Customer

 You can now test functionality freely     

## repo link
https://github.com/Flameeeeeeeeeeee/ABCRetailers.git


### Author
Tameez