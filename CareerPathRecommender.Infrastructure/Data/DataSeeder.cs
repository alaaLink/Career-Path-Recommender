using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(
        IEmployeeRepository employeeRepository,
        ICourseRepository courseRepository,
        IProjectRepository projectRepository,
        ISkillRepository skillRepository,
        ApplicationDbContext context)
    {
        // Add sample skills
        var skills = new List<Skill>
        {
            new() { Name = "C#", Category = "Programming", Description = "C# programming language" },
            new() { Name = "JavaScript", Category = "Programming", Description = "JavaScript programming" },
            new() { Name = "React", Category = "Frontend", Description = "React framework" },
            new() { Name = "ASP.NET Core", Category = "Backend", Description = "ASP.NET Core framework" },
            new() { Name = "SQL Server", Category = "Database", Description = "SQL Server database" },
            new() { Name = "Azure", Category = "Cloud", Description = "Microsoft Azure cloud platform" },
            new() { Name = "Leadership", Category = "Soft Skills", Description = "Team leadership abilities" },
            new() { Name = "Project Management", Category = "Management", Description = "Project management skills" },
            new() { Name = "Docker", Category = "DevOps", Description = "Containerization with Docker" },
            new() { Name = "Kubernetes", Category = "DevOps", Description = "Container orchestration" }
        };

        var savedSkills = new List<Skill>();
        foreach (var skill in skills)
        {
            var savedSkill = await skillRepository.AddAsync(skill);
            savedSkills.Add(savedSkill);
        }

        // Add comprehensive sample courses (20+ courses across different categories)
        var courses = new List<Course>
        {
        // Programming Courses
        new() { Title = "Advanced C# Programming", Provider = "Udemy", Category = "Programming", DurationHours = 40, Rating = 4.6m, Price = 89.99m, Url = "https://udemy.com/course/advanced-csharp", Description = "Master advanced C# concepts and design patterns" },
        new() { Title = "JavaScript Fundamentals", Provider = "freeCodeCamp", Category = "Programming", DurationHours = 30, Rating = 4.7m, Price = 0m, Url = "https://freecodecamp.org/javascript", Description = "Complete JavaScript course for beginners" },
        new() { Title = "Python for Data Science", Provider = "Coursera", Category = "Programming", DurationHours = 45, Rating = 4.5m, Price = 79.99m, Url = "https://coursera.org/python-data", Description = "Learn Python for data analysis and machine learning" },
        new() { Title = "Java Enterprise Development", Provider = "Udemy", Category = "Programming", DurationHours = 50, Rating = 4.4m, Price = 99.99m, Url = "https://udemy.com/java-enterprise", Description = "Build enterprise applications with Java and Spring" },
        
        // Frontend Development
        new() { Title = "React for Beginners", Provider = "freeCodeCamp", Category = "Frontend", DurationHours = 25, Rating = 4.4m, Price = 0m, Url = "https://freecodecamp.org/react", Description = "Learn React fundamentals for free" },
        new() { Title = "Vue.js Complete Course", Provider = "Udemy", Category = "Frontend", DurationHours = 35, Rating = 4.6m, Price = 69.99m, Url = "https://udemy.com/vuejs-complete", Description = "Master Vue.js for modern web development" },
        new() { Title = "Angular Advanced Patterns", Provider = "Pluralsight", Category = "Frontend", DurationHours = 28, Rating = 4.5m, Price = 199.99m, Url = "https://pluralsight.com/angular", Description = "Advanced Angular development patterns and best practices" },
        
        // Cloud & DevOps
        new() { Title = "Azure Fundamentals", Provider = "Microsoft Learn", Category = "Cloud", DurationHours = 15, Rating = 4.7m, Price = 0m, Url = "https://docs.microsoft.com/learn/azure", Description = "Free Azure certification path" },
        new() { Title = "AWS Solutions Architect", Provider = "A Cloud Guru", Category = "Cloud", DurationHours = 60, Rating = 4.8m, Price = 299.99m, Url = "https://acloudguru.com/aws-sa", Description = "Prepare for AWS Solutions Architect certification" },
        new() { Title = "Docker Mastery", Provider = "YouTube", Category = "DevOps", DurationHours = 8, Rating = 4.3m, Price = 0m, Url = "https://youtube.com/docker", Description = "Free Docker tutorial series" },
        new() { Title = "Kubernetes in Production", Provider = "Udemy", Category = "DevOps", DurationHours = 42, Rating = 4.7m, Price = 119.99m, Url = "https://udemy.com/kubernetes-prod", Description = "Deploy and manage Kubernetes clusters in production" },
        
        // Database & Backend
        new() { Title = "SQL Server Performance Tuning", Provider = "Pluralsight", Category = "Database", DurationHours = 32, Rating = 4.6m, Price = 199.99m, Url = "https://pluralsight.com/sql-performance", Description = "Optimize SQL Server queries and database performance" },
        new() { Title = "MongoDB University", Provider = "MongoDB", Category = "Database", DurationHours = 24, Rating = 4.4m, Price = 0m, Url = "https://university.mongodb.com", Description = "Free MongoDB certification courses" },
        new() { Title = "ASP.NET Core Web API", Provider = "Udemy", Category = "Backend", DurationHours = 38, Rating = 4.7m, Price = 94.99m, Url = "https://udemy.com/aspnet-webapi", Description = "Build scalable web APIs with ASP.NET Core" },
        
        // Leadership & Management
        new() { Title = "Leadership Excellence", Provider = "Coursera", Category = "Leadership", DurationHours = 20, Rating = 4.5m, Price = 49.99m, Url = "https://coursera.org/leadership", Description = "Develop leadership and management skills" },
        new() { Title = "Agile Project Management", Provider = "edX", Category = "Management", DurationHours = 16, Rating = 4.3m, Price = 149.99m, Url = "https://edx.org/agile-pm", Description = "Master Agile and Scrum methodologies" },
        new() { Title = "Technical Team Leadership", Provider = "Pluralsight", Category = "Leadership", DurationHours = 18, Rating = 4.6m, Price = 199.99m, Url = "https://pluralsight.com/tech-leadership", Description = "Lead technical teams effectively" },
        
        // Data & Analytics
        new() { Title = "Data Analysis with Excel", Provider = "Coursera", Category = "Analytics", DurationHours = 22, Rating = 4.2m, Price = 39.99m, Url = "https://coursera.org/excel-data", Description = "Advanced data analysis techniques in Excel" },
        new() { Title = "Machine Learning Foundations", Provider = "edX", Category = "Analytics", DurationHours = 48, Rating = 4.7m, Price = 249.99m, Url = "https://edx.org/ml-foundations", Description = "Introduction to machine learning concepts and algorithms" },
        
        // UI/UX Design
        new() { Title = "UI/UX Design Fundamentals", Provider = "Coursera", Category = "Design", DurationHours = 26, Rating = 4.5m, Price = 59.99m, Url = "https://coursera.org/ux-fundamentals", Description = "Learn user experience design principles and practices" },
        new() { Title = "Advanced Figma Techniques", Provider = "YouTube", Category = "Design", DurationHours = 12, Rating = 4.4m, Price = 0m, Url = "https://youtube.com/figma-advanced", Description = "Master advanced Figma features for professional design" }
         };

        var savedCourses = new List<Course>();
        foreach (var course in courses)
        {
            var savedCourse = await courseRepository.AddAsync(course);
            savedCourses.Add(savedCourse);
        }

        // Add comprehensive sample employees (50 across all experience levels)
        var employees = new List<Employee>
         {
        // Growing Professionals (0-4 years) - 20 employees
        new() { FirstName = "Alex", LastName = "Johnson", Email = "alex.johnson@company.com", Position = "Junior Developer", Department = "Engineering", YearsOfExperience = 1 },
        new() { FirstName = "Emma", LastName = "Thompson", Email = "emma.thompson@company.com", Position = "Frontend Developer", Department = "Engineering", YearsOfExperience = 2 },
        new() { FirstName = "Ryan", LastName = "Brown", Email = "ryan.brown@company.com", Position = "QA Engineer", Department = "Quality Assurance", YearsOfExperience = 1 },
        new() { FirstName = "Sophie", LastName = "Wilson", Email = "sophie.wilson@company.com", Position = "UX Designer", Department = "Design", YearsOfExperience = 3 },
        new() { FirstName = "Jake", LastName = "Davis", Email = "jake.davis@company.com", Position = "Backend Developer", Department = "Engineering", YearsOfExperience = 2 },
        new() { FirstName = "Maya", LastName = "Patel", Email = "maya.patel@company.com", Position = "Data Analyst", Department = "Analytics", YearsOfExperience = 1 },
        new() { FirstName = "Lucas", LastName = "Miller", Email = "lucas.miller@company.com", Position = "Mobile Developer", Department = "Engineering", YearsOfExperience = 3 },
        new() { FirstName = "Olivia", LastName = "Garcia", Email = "olivia.garcia@company.com", Position = "Marketing Coordinator", Department = "Marketing", YearsOfExperience = 2 },
        new() { FirstName = "Noah", LastName = "Anderson", Email = "noah.anderson@company.com", Position = "Systems Administrator", Department = "IT Operations", YearsOfExperience = 4 },
        new() { FirstName = "Ava", LastName = "Martinez", Email = "ava.martinez@company.com", Position = "Business Analyst", Department = "Business", YearsOfExperience = 3 },
        new() { FirstName = "Ethan", LastName = "Taylor", Email = "ethan.taylor@company.com", Position = "Software Developer", Department = "Engineering", YearsOfExperience = 2 },
        new() { FirstName = "Isabella", LastName = "Lee", Email = "isabella.lee@company.com", Position = "Content Writer", Department = "Marketing", YearsOfExperience = 1 },
        new() { FirstName = "Mason", LastName = "White", Email = "mason.white@company.com", Position = "Database Developer", Department = "Engineering", YearsOfExperience = 4 },
        new() { FirstName = "Sophia", LastName = "Harris", Email = "sophia.harris@company.com", Position = "HR Coordinator", Department = "Human Resources", YearsOfExperience = 2 },
        new() { FirstName = "Logan", LastName = "Clark", Email = "logan.clark@company.com", Position = "Technical Writer", Department = "Documentation", YearsOfExperience = 3 },
        new() { FirstName = "Charlotte", LastName = "Lewis", Email = "charlotte.lewis@company.com", Position = "Sales Associate", Department = "Sales", YearsOfExperience = 1 },
        new() { FirstName = "Jackson", LastName = "Walker", Email = "jackson.walker@company.com", Position = "Support Engineer", Department = "Customer Support", YearsOfExperience = 2 },
        new() { FirstName = "Amelia", LastName = "Hall", Email = "amelia.hall@company.com", Position = "Product Owner", Department = "Product", YearsOfExperience = 4 },
        new() { FirstName = "Liam", LastName = "Allen", Email = "liam.allen@company.com", Position = "Security Analyst", Department = "Security", YearsOfExperience = 3 },
        new() { FirstName = "Mia", LastName = "Young", Email = "mia.young@company.com", Position = "UI Designer", Department = "Design", YearsOfExperience = 2 },

        // Senior Professionals (5-9 years) - 20 employees
        new() { FirstName = "Benjamin", LastName = "King", Email = "benjamin.king@company.com", Position = "Senior Developer", Department = "Engineering", YearsOfExperience = 6 },
        new() { FirstName = "Harper", LastName = "Wright", Email = "harper.wright@company.com", Position = "Senior Data Scientist", Department = "Analytics", YearsOfExperience = 7 },
        new() { FirstName = "Elijah", LastName = "Lopez", Email = "elijah.lopez@company.com", Position = "Senior Frontend Developer", Department = "Engineering", YearsOfExperience = 8 },
        new() { FirstName = "Evelyn", LastName = "Hill", Email = "evelyn.hill@company.com", Position = "Senior UX Designer", Department = "Design", YearsOfExperience = 9 },
        new() { FirstName = "William", LastName = "Scott", Email = "william.scott@company.com", Position = "DevOps Engineer", Department = "Engineering", YearsOfExperience = 6 },
        new() { FirstName = "Abigail", LastName = "Green", Email = "abigail.green@company.com", Position = "Senior QA Engineer", Department = "Quality Assurance", YearsOfExperience = 7 },
        new() { FirstName = "James", LastName = "Adams", Email = "james.adams@company.com", Position = "Tech Lead", Department = "Engineering", YearsOfExperience = 8 },
        new() { FirstName = "Emily", LastName = "Baker", Email = "emily.baker@company.com", Position = "Senior Product Manager", Department = "Product", YearsOfExperience = 9 },
        new() { FirstName = "Henry", LastName = "Gonzalez", Email = "henry.gonzalez@company.com", Position = "Senior Backend Developer", Department = "Engineering", YearsOfExperience = 6 },
        new() { FirstName = "Elizabeth", LastName = "Nelson", Email = "elizabeth.nelson@company.com", Position = "Marketing Manager", Department = "Marketing", YearsOfExperience = 7 },
        new() { FirstName = "Alexander", LastName = "Carter", Email = "alexander.carter@company.com", Position = "Senior Systems Architect", Department = "Engineering", YearsOfExperience = 8 },
        new() { FirstName = "Victoria", LastName = "Mitchell", Email = "victoria.mitchell@company.com", Position = "Senior Business Analyst", Department = "Business", YearsOfExperience = 9 },
        new() { FirstName = "Daniel", LastName = "Perez", Email = "daniel.perez@company.com", Position = "Senior Mobile Developer", Department = "Engineering", YearsOfExperience = 6 },
        new() { FirstName = "Grace", LastName = "Roberts", Email = "grace.roberts@company.com", Position = "Senior HR Manager", Department = "Human Resources", YearsOfExperience = 7 },
        new() { FirstName = "Matthew", LastName = "Turner", Email = "matthew.turner@company.com", Position = "Senior Security Engineer", Department = "Security", YearsOfExperience = 8 },
        new() { FirstName = "Chloe", LastName = "Phillips", Email = "chloe.phillips@company.com", Position = "Senior Sales Manager", Department = "Sales", YearsOfExperience = 9 },
        new() { FirstName = "Samuel", LastName = "Campbell", Email = "samuel.campbell@company.com", Position = "Senior Database Administrator", Department = "Engineering", YearsOfExperience = 6 },
        new() { FirstName = "Natalie", LastName = "Parker", Email = "natalie.parker@company.com", Position = "Senior Content Strategist", Department = "Marketing", YearsOfExperience = 7 },
        new() { FirstName = "David", LastName = "Evans", Email = "david.evans@company.com", Position = "Senior Cloud Engineer", Department = "Engineering", YearsOfExperience = 8 },
        new() { FirstName = "Lily", LastName = "Edwards", Email = "lily.edwards@company.com", Position = "Senior Project Manager", Department = "Management", YearsOfExperience = 9 },

        // Expert Professionals (10+ years) - 10 employees
        new() { FirstName = "Michael", LastName = "Collins", Email = "michael.collins@company.com", Position = "Principal Engineer", Department = "Engineering", YearsOfExperience = 12 },
        new() { FirstName = "Sarah", LastName = "Stewart", Email = "sarah.stewart@company.com", Position = "VP of Engineering", Department = "Engineering", YearsOfExperience = 15 },
        new() { FirstName = "Christopher", LastName = "Sanchez", Email = "christopher.sanchez@company.com", Position = "Chief Technology Officer", Department = "Executive", YearsOfExperience = 18 },
        new() { FirstName = "Jessica", LastName = "Morris", Email = "jessica.morris@company.com", Position = "Director of Product", Department = "Product", YearsOfExperience = 14 },
        new() { FirstName = "Anthony", LastName = "Rogers", Email = "anthony.rogers@company.com", Position = "Chief Architect", Department = "Engineering", YearsOfExperience = 16 },
        new() { FirstName = "Ashley", LastName = "Reed", Email = "ashley.reed@company.com", Position = "VP of Marketing", Department = "Marketing", YearsOfExperience = 13 },
        new() { FirstName = "Joshua", LastName = "Cook", Email = "joshua.cook@company.com", Position = "Engineering Manager", Department = "Engineering", YearsOfExperience = 11 },
        new() { FirstName = "Amanda", LastName = "Bailey", Email = "amanda.bailey@company.com", Position = "Director of Analytics", Department = "Analytics", YearsOfExperience = 10 },
        new() { FirstName = "Andrew", LastName = "Rivera", Email = "andrew.rivera@company.com", Position = "VP of Operations", Department = "Operations", YearsOfExperience = 17 },
        new() { FirstName = "Stephanie", LastName = "Cooper", Email = "stephanie.cooper@company.com", Position = "Chief Security Officer", Department = "Security", YearsOfExperience = 14 }
        };

        var savedEmployees = new List<Employee>();
        foreach (var employee in employees)
        {
            var savedEmployee = await employeeRepository.AddAsync(employee);
            savedEmployees.Add(savedEmployee);
        }

        // Skip adding EmployeeSkills for now to avoid DateTime timezone issues
        // The issue appears to be with BaseEntity's CreatedAt property
        Console.WriteLine("Skipping EmployeeSkills seeding temporarily to avoid DateTime timezone issues");

        // Add comprehensive sample projects (15+ projects across different departments)
        var projects = new List<Project>
    {
        // Engineering Projects
        new() { Name = "E-commerce Platform", Description = "Build next-gen e-commerce platform using modern web technologies", StartDate = DateTime.UtcNow.AddDays(30), Status = ProjectStatus.Planning, Department = "Engineering", MaxTeamSize = 5 },
        new() { Name = "Mobile App Redesign", Description = "Redesign company mobile application with React Native", StartDate = DateTime.UtcNow.AddDays(15), Status = ProjectStatus.Active, Department = "Engineering", MaxTeamSize = 3 },
        new() { Name = "Cloud Migration", Description = "Migrate legacy systems to Azure cloud platform", StartDate = DateTime.UtcNow.AddDays(45), Status = ProjectStatus.Planning, Department = "Engineering", MaxTeamSize = 4 },
        new() { Name = "Microservices Architecture", Description = "Decompose monolithic application into microservices", StartDate = DateTime.UtcNow.AddDays(60), Status = ProjectStatus.Planning, Department = "Engineering", MaxTeamSize = 6 },
        new() { Name = "AI Chatbot Implementation", Description = "Develop intelligent customer service chatbot", StartDate = DateTime.UtcNow.AddDays(20), Status = ProjectStatus.Active, Department = "Engineering", MaxTeamSize = 4 },
        new() { Name = "Performance Optimization", Description = "Optimize application performance and reduce load times", StartDate = DateTime.UtcNow.AddDays(-10), Status = ProjectStatus.Active, Department = "Engineering", MaxTeamSize = 3 },
        
        // Data & Analytics Projects
        new() { Name = "Customer Analytics Dashboard", Description = "Build comprehensive analytics dashboard for customer insights", StartDate = DateTime.UtcNow.AddDays(25), Status = ProjectStatus.Planning, Department = "Analytics", MaxTeamSize = 3 },
        new() { Name = "Data Warehouse Migration", Description = "Migrate data warehouse to modern cloud-based solution", StartDate = DateTime.UtcNow.AddDays(40), Status = ProjectStatus.Planning, Department = "Analytics", MaxTeamSize = 4 },
        new() { Name = "Machine Learning Pipeline", Description = "Implement ML pipeline for predictive analytics", StartDate = DateTime.UtcNow.AddDays(50), Status = ProjectStatus.Planning, Department = "Analytics", MaxTeamSize = 5 },
        
        // Marketing Projects
        new() { Name = "Marketing Automation System", Description = "Implement automated marketing campaign system", StartDate = DateTime.UtcNow.AddDays(35), Status = ProjectStatus.Planning, Department = "Marketing", MaxTeamSize = 2 },
        new() { Name = "Website Redesign", Description = "Complete redesign of company website", StartDate = DateTime.UtcNow.AddDays(-5), Status = ProjectStatus.Active, Department = "Marketing", MaxTeamSize = 4 },
        
        // IT Operations Projects
        new() { Name = "Security Infrastructure Upgrade", Description = "Upgrade cybersecurity infrastructure and monitoring", StartDate = DateTime.UtcNow.AddDays(15), Status = ProjectStatus.Active, Department = "IT Operations", MaxTeamSize = 3 },
        new() { Name = "Backup System Modernization", Description = "Implement modern backup and disaster recovery system", StartDate = DateTime.UtcNow.AddDays(30), Status = ProjectStatus.Planning, Department = "IT Operations", MaxTeamSize = 2 },
        
        // Quality Assurance Projects  
        new() { Name = "Test Automation Framework", Description = "Build comprehensive automated testing framework", StartDate = DateTime.UtcNow.AddDays(20), Status = ProjectStatus.Active, Department = "Quality Assurance", MaxTeamSize = 3 },
        new() { Name = "Performance Testing Suite", Description = "Develop performance and load testing capabilities", StartDate = DateTime.UtcNow.AddDays(40), Status = ProjectStatus.Planning, Department = "Quality Assurance", MaxTeamSize = 2 },
        
        // Human Resources Projects
        new() { Name = "Employee Portal Enhancement", Description = "Enhance internal employee self-service portal", StartDate = DateTime.UtcNow.AddDays(25), Status = ProjectStatus.Planning, Department = "Human Resources", MaxTeamSize = 2 },
        
        // Business Projects
        new() { Name = "Digital Transformation Initiative", Description = "Lead company-wide digital transformation program", StartDate = DateTime.UtcNow.AddDays(90), Status = ProjectStatus.Planning, Department = "Business", MaxTeamSize = 8 },
        new() { Name = "Process Optimization Study", Description = "Analyze and optimize key business processes", StartDate = DateTime.UtcNow.AddDays(10), Status = ProjectStatus.Active, Department = "Business", MaxTeamSize = 3 }
    };

        var savedProjects = new List<Project>();
        foreach (var project in projects)
        {
            var savedProject = await projectRepository.AddAsync(project);
            savedProjects.Add(savedProject);
        }

        // Add project skills using actual saved IDs - mapping skills to multiple projects
        var projectSkills = new List<ProjectSkill>
    {
        // E-commerce Platform requirements (Project 0)
        new() { ProjectId = savedProjects[0].Id, SkillId = savedSkills[0].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // C#
        new() { ProjectId = savedProjects[0].Id, SkillId = savedSkills[2].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = true }, // React
        new() { ProjectId = savedProjects[0].Id, SkillId = savedSkills[4].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = false }, // SQL Server
        
        // Mobile App Redesign requirements (Project 1)
        new() { ProjectId = savedProjects[1].Id, SkillId = savedSkills[1].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // JavaScript
        new() { ProjectId = savedProjects[1].Id, SkillId = savedSkills[2].Id, RequiredLevel = SkillLevel.Expert, IsRequired = true }, // React
        
        // Cloud Migration requirements (Project 2)
        new() { ProjectId = savedProjects[2].Id, SkillId = savedSkills[5].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // Azure
        new() { ProjectId = savedProjects[2].Id, SkillId = savedSkills[8].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = false }, // Docker
        
        // Microservices Architecture requirements (Project 3)
        new() { ProjectId = savedProjects[3].Id, SkillId = savedSkills[0].Id, RequiredLevel = SkillLevel.Expert, IsRequired = true }, // C#
        new() { ProjectId = savedProjects[3].Id, SkillId = savedSkills[8].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // Docker
        new() { ProjectId = savedProjects[3].Id, SkillId = savedSkills[9].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = false }, // Kubernetes
        
        // AI Chatbot Implementation requirements (Project 4)
        new() { ProjectId = savedProjects[4].Id, SkillId = savedSkills[1].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // JavaScript
        new() { ProjectId = savedProjects[4].Id, SkillId = savedSkills[3].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // ASP.NET Core
        
        // Performance Optimization requirements (Project 5)
        new() { ProjectId = savedProjects[5].Id, SkillId = savedSkills[0].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // C#
        new() { ProjectId = savedProjects[5].Id, SkillId = savedSkills[4].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // SQL Server
        
        // Customer Analytics Dashboard requirements (Project 6)
        new() { ProjectId = savedProjects[6].Id, SkillId = savedSkills[1].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = true }, // JavaScript
        new() { ProjectId = savedProjects[6].Id, SkillId = savedSkills[4].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // SQL Server
        
        // Data Warehouse Migration requirements (Project 7)
        new() { ProjectId = savedProjects[7].Id, SkillId = savedSkills[5].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // Azure
        new() { ProjectId = savedProjects[7].Id, SkillId = savedSkills[4].Id, RequiredLevel = SkillLevel.Expert, IsRequired = true }, // SQL Server
        
        // Machine Learning Pipeline requirements (Project 8)
        new() { ProjectId = savedProjects[8].Id, SkillId = savedSkills[1].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // JavaScript
        new() { ProjectId = savedProjects[8].Id, SkillId = savedSkills[5].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = false }, // Azure
        
        // Marketing Automation System requirements (Project 9)
        new() { ProjectId = savedProjects[9].Id, SkillId = savedSkills[1].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = true }, // JavaScript
        new() { ProjectId = savedProjects[9].Id, SkillId = savedSkills[7].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // Project Management
        
        // Website Redesign requirements (Project 10)
        new() { ProjectId = savedProjects[10].Id, SkillId = savedSkills[1].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // JavaScript
        new() { ProjectId = savedProjects[10].Id, SkillId = savedSkills[2].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // React
        
        // Security Infrastructure Upgrade requirements (Project 11)
        new() { ProjectId = savedProjects[11].Id, SkillId = savedSkills[5].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // Azure
        new() { ProjectId = savedProjects[11].Id, SkillId = savedSkills[6].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // Leadership
        
        // Test Automation Framework requirements (Project 13)
        new() { ProjectId = savedProjects[13].Id, SkillId = savedSkills[0].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = true }, // C#
        new() { ProjectId = savedProjects[13].Id, SkillId = savedSkills[1].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = false }, // JavaScript
        
        // Digital Transformation Initiative requirements (Project 16)
        new() { ProjectId = savedProjects[16].Id, SkillId = savedSkills[6].Id, RequiredLevel = SkillLevel.Expert, IsRequired = true }, // Leadership
        new() { ProjectId = savedProjects[16].Id, SkillId = savedSkills[7].Id, RequiredLevel = SkillLevel.Expert, IsRequired = true } // Project Management
    };

        // ProjectSkills is also a junction table - using context directly
        context.ProjectSkills.AddRange(projectSkills);
        await context.SaveChangesAsync();
    }

}
