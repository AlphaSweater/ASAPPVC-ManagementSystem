
![Logo](https://i.postimg.cc/L6MYhpN8/Logo-With-Title.png)

<h1 align="center">🏭 ASAPPVC Management System</h1>

<p align="center">
  <img src="https://img.shields.io/badge/Built%20with-ASP.NET%20Core%208.0-blue?logo=dotnet">
  <img src="https://img.shields.io/badge/Database-SQLite-lightgrey?logo=sqlite">
  <img src="https://img.shields.io/badge/Architecture-MVC%20%7C%20N--Tier-green">
</p>

---

## 📝 Overview

**ASAPPVC Management System** is a professional **warehouse and inventory management platform** built using **ASP.NET Core MVC** with an **N-Tier architecture**.  
It provides an efficient way to manage **Products**, **Components**, **Orders**, **Customers**, and **Warehouse stock** — with clear separation between layers for scalability and maintainability.

The system is designed for manufacturing environments that require structured stock control and order tracking.

---

## 📚 Table of Contents
- [Features](#-features)
- [Security Features](#-security-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Setup & Prerequisites](#️-setup--prerequisites)
- [How to Run](#-how-to-run)
- [Screenshots](#-screenshots)
- [Contributors](#-contributors)
- [References](#-references)

---

## 🌟 Features

- 🔐 **User Authentication & Authorization**  
  Uses ASP.NET Identity for secure login and role-based access control.

- 🧩 **Component Management**  
  Add, edit, and view stock components. Each component includes attributes such as material type, colour, unit cost, and reorder levels.

- 🧾 **Order Management**  
  Create, track, and manage customer orders with unique automatically generated order codes.

- 📦 **Product Assembly**  
  Combine multiple components into products using the `ProductComponent` bridge model to track quantities and unit costs.

- 🧍 **Customer Management**  
  Store and maintain customer records, linked with order history and interactions.

- ⚙️ **Warehouse & Restock Alerts**  
  Automatically detect low-stock components and display restock warnings in the warehouse module.

- 📊 **Reports & Analytics**  
  Generate warehouse, stock, and order-related reports for management overview.

---

## 🔐 Security Features

- ASP.NET **Identity Framework** for authentication and session management.  
- **HTTPS enforcement** for secure communication.  
- **Anti-forgery tokens** on all forms to protect against CSRF.  
- **Data validation** using attributes like `[Required]`, `[StringLength]`, `[Range]`, and unique field constraints.  
- **Role-based authorization** ensuring sensitive pages are accessible only to authorized users.

---

## 🔧 Tech Stack

### Core
- [ASP.NET Core MVC 8.0](https://dotnet.microsoft.com/en-us/apps/aspnet)
- [C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/)
- [Razor Pages & ViewModels](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/overview)

### Database
- [SQLite](https://www.sqlite.org/index.html) (local development)

### DevOps & Testing
- [GitHub Actions](https://github.com/features/actions) for CI/CD
- [SonarQube](https://www.sonarsource.com/products/sonarqube/) for code analysis

---

## 🏗️ Architecture

<pre>
ASAPPVC.App/
├─ Controllers/
│  ├─ Warehouse/             # Area-specific controllers (e.g., images)
│  ├─ AuthController.cs
│  ├─ CustomerController.cs
│  ├─ EmployeeController.cs
│  ├─ HomeController.cs
│  ├─ OrderController.cs
│  └─ ReportsController.cs
│
├─ Data/
│  └─ (DbContext, seeding, etc.)
│
├─ Migrations/              
│
├─ Models/
│  ├─ Enums/                
│  ├─ General/              
│  ├─ Mappers/             
│  ├─ Validation/        
│  ├─ ApplicationUser.cs     
│  ├─ Component.cs
│  ├─ Customer.cs
│  ├─ Order.cs
│  ├─ OrderProduct.cs
│  ├─ Product.cs
│  └─ ProductComponent.cs   
│
├─ Repositories/    
├─ Services/               
├─ Utils/                  
│
├─ ViewModels/
│  ├─ Auth/
│  ├─ Customer/
│  ├─ Reports/
│  └─ Warehouse/
│  └─ ErrorViewModel.cs
│
├─ Views/
│  ├─ Auth/
│  ├─ Customer/
│  ├─ Employee/
│  ├─ Finance/
│  ├─ Home/
│  ├─ Order/
│  ├─ Reports/
│  └─ Warehouse/
│  ├─ _ViewImports.cshtml
│  └─ _ViewStart.cshtml
│
├─ wwwroot/                
├─ appsettings.json
└─ Program.cs
</pre>

## 🛠️ Prerequisites 

1. Install the .NET 8 SDK
You’ll need the latest .NET SDK to build and run the application.
👉 Download .NET 8 SDK

2. Install Visual Studio 2022 (v17.8 or newer)
Make sure the ASP.NET and web development workload is selected during installation.
👉 Download Visual Studio 2022

## 🚀 How to Compile and Run The Application

1. Download and install Visual Studio 2022 (v17.8 or newer) from the official Microsoft site:
👉 https://visualstudio.microsoft.com/
    When installing, make sure to select the ASP.NET and web development workload.

2. Open Visual Studio on your computer.

3. Get the project files:

- Option 1 – Clone the Repository: Click “Clone a Repository” on the Visual Studio start screen and paste this link: https://github.com/AlphaSweater/ASAPPVC-Management.git

- Option 2 - If you downloaded the repository as a ZIP, extract it to a folder, then open Visual Studio and click “Open a Project or Solution”. Select the ASAPPVC.App.sln file from the extracted folder.

- Wait for NuGet packages to restore automatically.
This ensures all dependencies (like Entity Framework and Identity) are downloaded.
(This may take a few minutes the first time you open the project.)

- Make sure you have the .NET 8 SDK installed.

4. Run the application:

- In Visual Studio, set ASAPPVC.App as the Startup Project.

- Click the green “Run” button at the top, or press F5 to build and launch the app.

- Visual Studio will start the development server and automatically open the app in your browser.

5. Once launched, the app will open automatically


## 📸 Screenshots

<div align="center">
  
| Manage Orders | Manage Components | Manage Products | Customer Page |
|-----------------|------------------------|-------------------|-----------------|
| <img src="https://i.postimg.cc/WtLF75rq/Screenshot-2025-10-31-164545.png" width="700"/> | <img src="https://i.postimg.cc/Dw1QRXw0/Screenshot-2025-10-31-164451.png" width="700"/> | <img src="https://i.postimg.cc/cL3TZzX1/Screenshot-2025-10-31-164400.png" width="700"/> | <img src="https://i.postimg.cc/hPPHDK73/Screenshot-2025-10-31-164212.png" width="700"/> |


## 👥 contributors
<a href="https://github.com/AlphaSweater/BudgetBuddy-Project/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=AlphaSweater/BudgetBuddy-Project" />
</a>

- Chad Fairlie ST10269509
- Dhiren Ruthenavelu ST10256859
- Kayla Ferreira ST10259527
- Nathan Teixeira ST10249266


## 📚 References
- ChatGPT was used to help with the design and planning. As well as assisted with finding and fixing errors in the code.
##
![App Demo](https://i.postimg.cc/HWtLyjr6/kerchoo-kachow.gif)
##
