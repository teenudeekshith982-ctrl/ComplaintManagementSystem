using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComplaintManagementSystem.Contexts
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ComplaintManagementSystemContext context)
        {
            // Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new Department { DepartmentId = 1, DepartmentName = "Technical" },
                    new Department { DepartmentId = 2, DepartmentName = "Finance" },
                    new Department { DepartmentId = 3, DepartmentName = "HR" },
                    new Department { DepartmentId = 4, DepartmentName = "Management" }
                };
                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('\"Departments\"', 'DepartmentId'), COALESCE(MAX(\"DepartmentId\"), 1)) FROM \"Departments\";");
            }

            // Seed Complaint Categories
            if (!await context.ComplaintCategories.AnyAsync())
            {
                var categories = new List<ComplaintCategory>
                {
                    new ComplaintCategory { CategoryId = 1, Categoryname = "Technical" },
                    new ComplaintCategory { CategoryId = 2, Categoryname = "Finance" },
                    new ComplaintCategory { CategoryId = 3, Categoryname = "HR" }
                };
                await context.ComplaintCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('\"ComplaintCategories\"', 'CategoryId'), COALESCE(MAX(\"CategoryId\"), 1)) FROM \"ComplaintCategories\";");
            }

            // Seed Complaint Priorities
            if (!await context.ComplaintPriorities.AnyAsync())
            {
                var priorities = new List<ComplaintPriority>
                {
                    new ComplaintPriority { PriorityId = 1, Priority = "Critical" },
                    new ComplaintPriority { PriorityId = 2, Priority = "High" },
                    new ComplaintPriority { PriorityId = 3, Priority = "Medium" },
                    new ComplaintPriority { PriorityId = 4, Priority = "Low" }
                };
                await context.ComplaintPriorities.AddRangeAsync(priorities);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('\"ComplaintPriorities\"', 'PriorityId'), COALESCE(MAX(\"PriorityId\"), 1)) FROM \"ComplaintPriorities\";");
            }

            // Seed Complaint Statuses
            if (!await context.ComplaintStatuses.AnyAsync())
            {
                var statuses = new List<ComplaintStatus>
                {
                    new ComplaintStatus { StatusId = 1, StatusName = "Open" },
                    new ComplaintStatus { StatusId = 2, StatusName = "Assigned" },
                    new ComplaintStatus { StatusId = 3, StatusName = "InProgress" },
                    new ComplaintStatus { StatusId = 4, StatusName = "PendingCustomerResponse" },
                    new ComplaintStatus { StatusId = 5, StatusName = "Resolved" },
                    new ComplaintStatus { StatusId = 6, StatusName = "Closed" },
                    new ComplaintStatus { StatusId = 7, StatusName = "Reopened" },
                    new ComplaintStatus { StatusId = 8, StatusName = "Cancelled" }
                };
                await context.ComplaintStatuses.AddRangeAsync(statuses);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('\"ComplaintStatuses\"', 'StatusId'), COALESCE(MAX(\"StatusId\"), 1)) FROM \"ComplaintStatuses\";");
            }

            // Seed SLAs (Service Level Agreements)
            if (!await context.SLAs.AnyAsync())
            {
                var slas = new List<SLA>
                {
                    new SLA { SlaId = 1, PriorityId = 1, ResolutionHours = 6 },
                    new SLA { SlaId = 2, PriorityId = 2, ResolutionHours = 12 },
                    new SLA { SlaId = 3, PriorityId = 3, ResolutionHours = 24 },
                    new SLA { SlaId = 4, PriorityId = 4, ResolutionHours = 48 }
                };
                await context.SLAs.AddRangeAsync(slas);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('\"SLAs\"', 'SlaId'), COALESCE(MAX(\"SlaId\"), 1)) FROM \"SLAs\";");
            }
        }
    }
}
