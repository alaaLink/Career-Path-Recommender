using CareerPathRecommender.Domain.Enums;
using System.Collections.Generic;

namespace CareerPathRecommender.Application.Constants
{
    public static class RecommendationConsts
    {
        public static readonly Dictionary<string, string[]> CareerTracks = new Dictionary<string, string[]>
        {
            // Engineering Track
            ["Junior Developer"] = new[] { "Developer", "Software Developer", "Senior Developer", "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering", "Chief Technology Officer", "Chief Architect" },
            ["Developer"] = new[] { "Software Developer", "Senior Developer", "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Software Developer"] = new[] { "Senior Developer", "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Frontend Developer"] = new[] { "Senior Frontend Developer", "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Backend Developer"] = new[] { "Senior Backend Developer", "DevOps Engineer", "Senior Systems Architect", "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Mobile Developer"] = new[] { "Senior Mobile Developer", "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Database Developer"] = new[] { "Senior Database Administrator", "Senior Systems Architect", "Tech Lead", "Principal Engineer" },
            ["Senior Developer"] = new[] { "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Senior Frontend Developer"] = new[] { "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Senior Backend Developer"] = new[] { "DevOps Engineer", "Senior Systems Architect", "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Senior Mobile Developer"] = new[] { "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["DevOps Engineer"] = new[] { "Senior Cloud Engineer", "Senior Systems Architect", "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Tech Lead"] = new[] { "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Principal Engineer"] = new[] { "Chief Architect", "Engineering Manager", "VP of Engineering", "Chief Technology Officer" },
            ["Engineering Manager"] = new[] { "VP of Engineering", "Chief Technology Officer" },

            // Quality Assurance Track
            ["QA Engineer"] = new[] { "Senior QA Engineer", "QA Manager" },
            ["Senior QA Engineer"] = new[] { "QA Manager" },

            // Data & Analytics Track
            ["Data Analyst"] = new[] { "Senior Data Scientist", "Director of Analytics" },
            ["Senior Data Scientist"] = new[] { "Director of Analytics" },
            ["Business Analyst"] = new[] { "Senior Business Analyst", "Senior Product Manager", "Director of Product" },
            ["Senior Business Analyst"] = new[] { "Senior Product Manager", "Director of Product" },

            // Design Track
            ["UX Designer"] = new[] { "Senior UX Designer" },
            ["UI Designer"] = new[] { "Senior UX Designer" },
            ["Senior UX Designer"] = new[] { "Director of Product" },

            // Management Track
            ["Product Owner"] = new[] { "Senior Product Manager", "Director of Product" },
            ["Senior Product Manager"] = new[] { "Director of Product" },

            // Marketing Track
            ["Marketing Coordinator"] = new[] { "Marketing Manager", "VP of Marketing" },
            ["Content Writer"] = new[] { "Senior Content Strategist", "Marketing Manager" },
            ["Marketing Manager"] = new[] { "VP of Marketing" },
            ["Senior Content Strategist"] = new[] { "Marketing Manager", "VP of Marketing" },

            // Operations & Support Track
            ["Systems Administrator"] = new[] { "Senior Database Administrator", "Senior Systems Architect", "VP of Operations" },
            ["Support Engineer"] = new[] { "Senior Systems Architect", "Engineering Manager" },
            ["Security Analyst"] = new[] { "Senior Security Engineer", "Chief Security Officer" },
            ["Senior Security Engineer"] = new[] { "Chief Security Officer" },

            // HR Track
            ["HR Coordinator"] = new[] { "Senior HR Manager" },
            ["Senior HR Manager"] = new[] { "VP of Operations" },

            // Sales Track
            ["Sales Associate"] = new[] { "Senior Sales Manager" },
            ["Senior Sales Manager"] = new[] { "VP of Marketing" }
        };

        public static readonly string[] HighDemandKeywords = new[]
        {
            "AI", "Machine Learning", "Cloud", "Kubernetes", "Docker",
            "React", "Angular", "Vue", "Python", "JavaScript", "TypeScript",
            "DevOps", "Microservices", "Blockchain", "Cybersecurity"
        };

        public static readonly Dictionary<string, string[]> RelatedDepts = new Dictionary <string, string[]>
        {
            ["Engineering"] = new[] { "IT Operations", "Quality Assurance", "Security", "Analytics" },
            ["Product"] = new[] { "Engineering", "Marketing", "Design" },
            ["Marketing"] = new[] { "Product", "Sales", "Design" },
            ["Analytics"] = new[] { "Engineering", "Business", "Marketing" }
        };

        public static readonly string[] SeniorityKeywords = new[]
        {
           "Senior", "Principal", "Lead", "Manager", "Director", "VP", "Chief" 
        };

        public static readonly Dictionary<string, SkillLevel> SeniorRequirements = new Dictionary<string, SkillLevel>
        {
            ["C#"] = SkillLevel.Advanced,
            ["JavaScript"] = SkillLevel.Advanced,
            ["SQL"] = SkillLevel.Advanced,
            ["Cloud Computing"] = SkillLevel.Intermediate,
            ["System Design"] = SkillLevel.Advanced,
            ["Leadership"] = SkillLevel.Intermediate,
            ["Mentoring"] = SkillLevel.Intermediate,
            ["Problem Solving"] = SkillLevel.Advanced
        };

        public static readonly Dictionary<string, SkillLevel> LeadRequirements = new Dictionary<string, SkillLevel>
        {
            ["Leadership"] = SkillLevel.Advanced,
            ["Project Management"] = SkillLevel.Advanced,
            ["Communication"] = SkillLevel.Advanced,
            ["Mentoring"] = SkillLevel.Advanced,
            ["Technical Architecture"] = SkillLevel.Advanced,
            ["C#"] = SkillLevel.Expert,
            ["System Design"] = SkillLevel.Expert
        };

        public static readonly Dictionary<string, SkillLevel> ManagerRequirements = new Dictionary<string, SkillLevel>
        {
            ["Leadership"] = SkillLevel.Expert,
            ["Project Management"] = SkillLevel.Expert,
            ["Team Management"] = SkillLevel.Expert,
            ["Strategic Planning"] = SkillLevel.Advanced,
            ["Budgeting"] = SkillLevel.Intermediate,
            ["Communication"] = SkillLevel.Expert,
            ["Performance Management"] = SkillLevel.Advanced
        };

        public static readonly Dictionary<string, SkillLevel> ArchitectRequirements = new Dictionary<string, SkillLevel>
        {
            ["System Design"] = SkillLevel.Expert,
            ["Technical Architecture"] = SkillLevel.Expert,
            ["Cloud Computing"] = SkillLevel.Expert,
            ["Microservices"] = SkillLevel.Advanced,
            ["Database Design"] = SkillLevel.Advanced,
            ["Security"] = SkillLevel.Advanced,
            ["Performance Optimization"] = SkillLevel.Advanced
        };

        public static readonly Dictionary<string, SkillLevel> FullstackRequirements = new Dictionary<string, SkillLevel>
        {
            ["C#"] = SkillLevel.Advanced,
            ["JavaScript"] = SkillLevel.Advanced,
            ["React"] = SkillLevel.Advanced,
            ["SQL"] = SkillLevel.Advanced,
            ["HTML/CSS"] = SkillLevel.Advanced,
            ["API Development"] = SkillLevel.Advanced,
            ["Database Design"] = SkillLevel.Intermediate
        };

        public static readonly Dictionary<string, SkillLevel> DefaultRequirements = new Dictionary<string, SkillLevel>
        {
            ["C#"] = SkillLevel.Intermediate,
            ["JavaScript"] = SkillLevel.Intermediate,
            ["SQL"] = SkillLevel.Intermediate,
            ["Problem Solving"] = SkillLevel.Intermediate,
            ["Communication"] = SkillLevel.Intermediate
        };
    }
}
