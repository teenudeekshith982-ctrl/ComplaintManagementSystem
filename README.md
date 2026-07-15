# Complaint Management System (CMS)

A professional, full-stack enterprise platform designed to streamline customer support, technician workload allocation, SLA compliance tracking, and hierarchical ticket escalations. 

This platform allows customers to file and track complaints, aligns technical issues with specialized department queues, provides technicians with structured workboards, and empowers administrators with real-time analytics reports.

---

## 🏗️ Project Architecture & Design Patterns

The platform is built using a decoupled client-server architecture:
- **Backend (Web API)**: Developed with ASP.NET Core 10.0 and Entity Framework Core using a **Repository & Service Pattern** to enforce separation of concerns, transactional safety, and high testability.
- **Database**: PostgreSQL database stores relational entities, optimized for tracking historical changes, user profile states, and file attachments.
- **Frontend (SPA)**: Built using Angular 21.2.0, utilizing **Angular Signals** for robust reactive state management, clean custom styling, and optimized load times.
- **Real-Time Layer**: SignalR Hubs push live, on-screen notifications to technicians and users during ticket assignments or status updates.

---

## 💾 Relational Database Schema & Domain Models

The database models are designed around a strict hierarchical relational schema:
- **User**: Base model for authentication (Administrators, Employees, and Customers).
- **Employee**: Extends user profiles for technicians, mapping them to specific departments and designating escalation levels.
- **Department**: Organizational units (`Technical`, `Finance`, `HR`) that categorize incoming tickets.
- **Complaint**: The central entity representing a ticket. Contains title, description, category, priority, status, created/due/resolution timestamps, customer ID, and assigned technician ID.
- **ComplaintCategory, ComplaintPriority, ComplaintStatus**: Master tables containing pre-configured lookup data (e.g. status ranges from `Open` to `Closed`).
- **Comment**: Chronological feedback messages appended to tickets by customers, technicians, or administrators.
- **ComplaintAttachment**: Metadata for uploaded screenshots and files (stored securely on disk).
- **ComplaintHistory**: Audit logs tracking every single transition (assignee changes, status updates, edits) with time-stamped records.
- **EscalatedComplaint**: Relational mappings representing tickets escalated past the technician tier, linked to an `EscalatedLevel` designating TL, Manager, or Director levels.
- **Notification**: Stores in-app alerts for users and technicians, tracking read/unread status.
- **SLA**: Service Level Agreements mapping ticket priorities to targeted resolution deadlines.

---

## 👥 Roles & Permissions System

The system implements Role-Based Access Control (RBAC) via JWT claims:

### 1. Customer (User) Role
- **Raise Tickets**: File new support requests, assign categories, and drag-and-drop screenshots/files.
- **Track Status**: A customizable dashboard to view, search, and filter raised tickets.
- **Interactive Comments**: Add feedback or read comments from the assigned technician.
- **Profile Controls**: View account profile details, change phone number/name, or update passwords.

### 2. Technician (Employee) Role
- **My Workboard**: View statistics on assigned tickets, resolved counts, escalations, and overdue SLAs.
- **Assigned Queue**: Search, sort, and paginate active tickets assigned to them.
- **Progress Tickets**: Move ticket status from *Assigned* to *In Progress* to *Resolved*.
- **Submit Escalations**: Send complex or blocked tickets to the Admin queue with detailed justifications.

### 3. System Administrator (Admin) Role
- **Central Repository**: Complete directory listing of all system tickets with filter controls.
- **User Account Management**: Full directory of accounts with the ability to activate/deactivate users or register new technician employees.
- **Escalation Resolver**: Panel to review escalated tickets, view justifications, and assign them back to specialized technicians.
- **Live Analytics Reports**: Real-time KPI charts and distributions:
  - **SLA Breach Rate**: Percentage of resolved tickets that missed their target due dates.
  - **Average Resolution Time**: Time metrics in hours to resolve complaints.
  - **Escalations Queue**: Active unresolved escalation count.
  - **Unassigned Workload**: Number of open tickets awaiting allocation.
  - **Visualizations**: Interactive bar charts for status distributions, category divisions, and monthly creation trends.

---

## 🛠️ Tech Stack & Dependencies

### Backend
- **Framework**: .NET 10.0 (ASP.NET Core Web API)
- **ORM**: Entity Framework Core 10.0
- **Database**: PostgreSQL Database Driver (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Authentication**: JWT Bearer Tokens (`System.IdentityModel.Tokens.Jwt`)
- **Websockets**: ASP.NET Core SignalR

### Frontend
- **Framework**: Angular 21.2.0
- **Build tool**: Vite / Angular CLI Compiler
- **Icons**: Google Material Symbols Outlined (imported dynamically via Google Fonts)
- **Styling**: Premium custom CSS variables, responsive grid structures, and glassmorphic panels

---

## 🚀 Setup & Installation Guide

### Prerequisites
- Install **.NET 10.0 SDK**
- Install **Node.js** (v18+) and npm
- Install and run **PostgreSQL database**

---

### Step 1: Configure & Seed the Database

1. Navigate to the backend configuration file: [appsettings.json](file:///d:/ComplaintManagementSystem/backend/appsettings.json).
2. Configure your PostgreSQL connection string:
   ```json
   "ConnectionStrings": {
     "Default" : "Host=localhost;Port=5432;Username=postgres;Database=ComplaintManagementSystem;Password=your_postgres_password"
   }
   ```
3. Open your terminal in the `/backend` folder and apply Entity Framework database migrations:
   ```bash
   dotnet ef database update
   ```
   *This creates all schemas, tables, index triggers, and seeds the master lookup data (Statuses, Categories, Priorities, and Admin accounts).*

---

### Step 2: Running the Backend Web API

1. In your terminal, navigate to the `/backend` folder.
2. Build and run the server:
   ```bash
   cd backend
   dotnet run
   ```
3. The Web API will launch locally at `http://localhost:5048`. You can inspect endpoints or test queries directly.

---

### Step 3: Running the Frontend Application

1. Open a new terminal and navigate to the `/frontend` folder:
   ```bash
   cd frontend
   ```
2. Install the application dependencies:
   ```bash
   npm install
   ```
3. Start the Angular local development server:
   ```bash
   ng serve
   ```
4. Once compiled, open your browser and navigate to:
   ```url
   http://localhost:4200
   ```
5. The application is set up with hot-reloading; changes made to frontend templates or styles will automatically refresh the browser.

---

### Step 4: Running Unit Tests
To verify service components and validate repository logic, navigate to the root directory and execute:
```bash
dotnet test
```
