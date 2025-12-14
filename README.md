# 🏬 Warehouse Management System (.NET 9)

A backend **Warehouse Management System (WMS)** built with **.NET 9** and **ASP.NET Core Web API**, designed to manage products, categories, stock movements, and warehouse reports with secure authentication and role-based authorization.

This project focuses on clean structure, scalability, and real-world warehouse workflows.

---

## 🚀 Features

### 🔐 Authentication & Authorization
- JWT Authentication
- Role-based access control
- Roles:
  - SystemAdmin
  - WarehouseManager
  - NormalUser
  - Auditor
- Secure login with JWT token
- Register endpoint restricted to **SystemAdmin**
- Prevention of creating multiple SystemAdmin accounts

---

### 🛡 Authorization Policies
- `RequireAdmin`
- `CanManageProducts`
- `CanDoStockMovements`
- `CanViewReports`

Policies are applied using `[Authorize]` and policy-based authorization on controllers and endpoints.

---

### 🧩 Feature Flags
- Feature flag support using `Microsoft.FeatureManagement`
- Enable/disable features without redeploying the application
- Ready for gradual rollout and experimentation

---

### ⚠️ Global Error Handling
- Centralized exception handling middleware
- Consistent error responses across the API
- Improved debugging and maintainability

---

## 📦 Inventory Management

### Categories
- Full CRUD operations
- Protected with `CanManageProducts` policy on sensitive actions

### Products
- CRUD operations
- Product code uniqueness validation
- Category existence validation
- Prevent deletion when related stock movements exist
- Fetch products with category name

---

## 🔄 Stock Movements
- Stock In / Stock Out operations
- Automatic stock calculation
- Prevents negative stock
- Tracks:
  - UserId
  - Timestamp
- Protected with `CanDoStockMovements` policy

---

## 📊 Reports
- Stock summary reports
- Product stock movement history
- Protected with `CanViewReports` policy

---

## 🧱 Architecture

- ASP.NET Core Web API (.NET 9)
- Entity Framework Core
- Repository Pattern
- Unit of Work Pattern
- Dependency Injection (DI & DIP)
- SQLite (can be replaced easily)

---

## 📁 Project Structure (Simplified)

