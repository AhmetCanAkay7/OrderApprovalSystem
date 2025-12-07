# Siemens Order Approval System

A comprehensive **Order Management System** designed for internal processes, featuring order creation, hierarchical approval workflows, and inventory management.
Built with **ASP.NET Core MVC** using a **Database First** approach with pure **ADO.NET** (no ORM).

---

## 📑 Table of Contents

1. [Project Overview](#-project-overview)
2. [Project Architecture](#-project-architecture)
3. [Functionality & Data Access](#-functionality--data-access)
4. [Stored Procedures](#-stored-procedures)
5. [Helper Methods (SqlHelper)](#-helper-methods-sqlhelper)
6. [Session Management](#-session-management)
7. [Database Schema](#-database-schema)
8. [Getting Started](#-getting-started)
9. [Project Structure](#-project-structure)

---

## 🚀 Project Overview

The system manages the complete order lifecycle:

1. **Order Creation**: Users create orders by selecting a partner and adding products
2. **Approval Workflow**: Orders pass through 3 approval stages:
   - **Step 0**: Commercial Approval
   - **Step 1**: Technical Approval  
   - **Step 2**: Paraf Approval (Final Signature)
3. **Completion**: When all approvals are complete (`Step 3` → `Status 0`), the order is marked as completed

### Key Features
- 📦 **Order Management**: Create, view, and track orders
- ✅ **Multi-level Approval**: Three-stage hierarchical approval process
- 👥 **Employee Directory**: View and manage employee information
- 📊 **Dashboard**: Real-time statistics and recent order activity
- 🏭 **Inventory Tracking**: Monitor stock levels across warehouses
- 📈 **Sales Reports**: Partner sales performance analytics

---

## 🏗 Project Architecture

The project follows an **N-Tier Architecture** with a clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │   Views     │  │ Controllers │  │      ViewModels         │ │
│  │  (Razor)    │  │   (MVC)     │  │  (Data Transfer)        │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DATA ACCESS LAYER                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    Repositories                          │   │
│  │  OrderRepository │ EmployeeRepository │ ProductRepository│   │
│  │  PartnerRepository │ StockRepository │ ReportRepository  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                              │                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              SqlHelper (Singleton)                       │   │
│  │         Centralized ADO.NET Database Access              │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DATABASE LAYER                             │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                  SQL Server Database                     │   │
│  │     Tables │ Stored Procedures │ Triggers │ Indexes      │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### Design Patterns Used

| Pattern | Implementation | Purpose |
|---------|---------------|---------|
| **Singleton** | `SqlHelper` | Single database connection manager instance |
| **Repository** | `*Repository` classes | Encapsulate data access logic per entity |
| **MVC** | Controllers, Views, Models | Separation of presentation and business logic |
| **Dependency Injection** | `Program.cs` | Loose coupling between components |

---

## ⚙ Functionality & Data Access

### Controllers & Their Responsibilities

| Controller | Description | Key Actions |
|------------|-------------|-------------|
| `DashboardController` | Main dashboard and reports | `Index()`, `PartnerSales()` |
| `OrderController` | Order CRUD and approval workflow | `Index()`, `Create()`, `Details()`, `PendingApprovals()`, `Approve()` |
| `EmployeeController` | Employee directory management | `Index()`, `Details()` |
| `ProductController` | Product catalog and inventory | `Index()`, `Details()`, `Inventory()` |
| `PartnerController` | Partner/customer management | `Index()`, `Details()`, `SalesPerformance()` |
| `AccountController` | Authentication (session-based) | `Login()`, `Logout()` |

### Repositories & Data Access Methods

#### OrderRepository
```csharp
GetDashboard()           // Get all orders with status information
GetPendingApprovals(id)  // Get orders pending approval for an employee
GetOrderDetails(id)      // Get complete order with items and approvers
CreateOrder(...)         // Insert new order
AddOrderItem(...)        // Add product to order
UpdateOrderStep(id)      // Advance approval workflow
CanEmployeeApprove(...)  // Check if employee can approve an order
AssignApprover(...)      // Assign an approver to an order
```

#### EmployeeRepository
```csharp
GetAllEmployees()        // List all employees with department info
GetEmployeeById(id)      // Get single employee details
GetEmployeeByEmail(email)// Find employee by email (for auth)
GetEmployeeDirectory()   // Get formatted employee list for directory
GetAllDepartments()      // List all departments
```

#### ProductRepository
```csharp
GetAllProducts()         // List all products with categories
GetProductById(id)       // Get product details
GetAllCategories()       // List all categories
GetProductsByCategory(id)// Filter products by category
```

#### StockRepository
```csharp
GetInventoryValuation()  // Get stock values across warehouses
GetAllWarehouses()       // List all warehouses
GetStockByProduct(id)    // Stock levels for a specific product
GetStockByWarehouse(id)  // All stock in a warehouse
GetTotalStockForProduct(id) // Total quantity across all warehouses
```

#### ReportRepository
```csharp
GetPartnerSalesPerformance() // Sales analytics per partner
GetDashboardSummary()        // Aggregate statistics for dashboard
```

---

## 🗄 Database Schema

### Core Tables

| Table | Description |
|-------|-------------|
| `DEPARTMENT` | Organizational departments with location info |
| `EMPLOYEE` | User accounts with role assignments |
| `APPROVAL_ROLE` | Approval role definitions (Commercial, Technical, Paraf) |
| `PARTNER` | Customer/partner information |
| `CATEGORY` | Product categories |
| `PRODUCT` | Product catalog with pricing |
| `WAREHOUSE` | Storage locations |
| `STOCK` | Inventory levels per product/warehouse |
| `[ORDER]` | Order headers with approval status |
| `ORDER_ITEM` | Order line items |
| `ORDER_APPROVER` | Order-to-employee approval assignments |


## 📁 Project Structure

```
OrderApprovalSystem/
├── Controllers/
│   ├── AccountController.cs      # Authentication
│   ├── DashboardController.cs    # Main dashboard & reports
│   ├── EmployeeController.cs     # Employee management
│   ├── HomeController.cs         # Home page
│   ├── OrderController.cs        # Order CRUD & approvals
│   ├── PartnerController.cs      # Partner management
│   └── ProductController.cs      # Product & inventory
│
├── DataAccess/
│   ├── SqlHelper.cs              # Singleton ADO.NET helper
│   ├── OrderRepository.cs        # Order data access
│   ├── EmployeeRepository.cs     # Employee data access
│   ├── ProductRepository.cs      # Product data access
│   ├── PartnerRepository.cs      # Partner data access
│   ├── StockRepository.cs        # Inventory data access
│   └── ReportRepository.cs       # Reporting queries
│
├── Models/
│   ├── Order.cs                  # Order entity
│   ├── OrderItem.cs              # Order line item entity
│   ├── OrderApprover.cs          # Approval assignment entity
│   ├── Employee.cs               # Employee entity
│   ├── Product.cs                # Product entity
│   ├── Partner.cs                # Partner entity
│   ├── Department.cs             # Department entity
│   ├── Category.cs               # Category entity
│   ├── Warehouse.cs              # Warehouse entity
│   ├── Stock.cs                  # Stock entity
│   └── ApprovalRole.cs           # Approval role entity
│
├── ViewModels/
│   ├── DashboardViewModel.cs     # Dashboard display model
│   ├── OrderDetailsViewModel.cs  # Order details display
│   ├── CreateOrderViewModel.cs   # Order creation form
│   ├── PendingApprovalViewModel.cs # Pending approvals list
│   ├── ApprovalActionViewModel.cs  # Approval form submission
│   ├── LoginViewModel.cs         # Login form
│   ├── EmployeeDirectoryViewModel.cs # Employee list display
│   ├── InventoryValuationViewModel.cs # Stock valuation
│   └── PartnerSalesViewModel.cs  # Sales report display
│
├── Views/
│   ├── Account/                  # Login views
│   ├── Dashboard/                # Dashboard views
│   ├── Employee/                 # Employee views
│   ├── Order/                    # Order views
│   ├── Partner/                  # Partner views
│   ├── Product/                  # Product views
│   └── Shared/                   # Layout & partials
│
├── wwwroot/                      # Static files (CSS, JS, images)
├── Program.cs                    # Application entry point & DI config
├── appsettings.json              # Configuration
└── OrderApprovalSystem.csproj    # Project file
```


---

## 👨‍💻 Author

**Ahmet Can Akay**

- GitHub: [@AhmetCanAkay7](https://github.com/AhmetCanAkay7)