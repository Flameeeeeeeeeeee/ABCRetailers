# ABCRetailers

## Overview

   ABCRetailers is a web application demonstrating CRUD operations integrated with Microsoft Azure services. The application supports Customer, Product, and Order management, along with file uploads, showcasing cloud-based storage and processing.

## Features
   Customer Management

   Create, read, update, and delete customer records.

   Data stored in Azure Table Storage.

## Product Management

   Add and manage product details with basic price validation.

## Order Management

 Create and track customer orders linked to stored customers and products.

## File Uploads

   Upload proof-of-payment or contract files.

   Files stored securely in Azure Blob Storage or Azure File Share.

## Azure Services Used

   Azure Blob Storage – Stores uploaded files.

   Azure File Share – Provides shared access to files.

   Azure Table Storage – Manages structured entity data.

   Azure Queue Storage – Enables background task handling (if applicable).

## Tech Stack

   Backend: ASP.NET Core MVC

   Frontend: Razor Pages (CSHTML)

   Storage: Azure Storage Services (Tables, Blobs, File Share, Queues)

   Database: NoSQL via Azure Tables

## Setup & Usage Prerequisites

   Azure subscription with Storage Account configured.

   Visual Studio or VS Code with .NET SDK installed.

## project structure
   /Controllers     # MVC Controllers for CRUD operations
   /Models          # Data Models (Customer, Product, Order)
   /Views           # Razor views for UI
   /Uploads         # Dummy files for testing

   Author
   Tameez


