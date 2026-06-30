using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a project in the system.
/// </summary>
namespace Pulse.Core.Entities
{
    public class Project : BaseEntity<string>
    {
        public Project()
        {
            AuditTrails = new HashSet<ProjectAuditTrail>();
            Members = new HashSet<ProjectMember>();
            Milestones = new HashSet<ProjectMilestone>();
            Products = new HashSet<ProjectProduct>();
            StatusChanges = new HashSet<ProjectStatusChange>();
            Tasks = new HashSet<ProjectTask>();
            Annotations = new HashSet<Annotation>();
            Fields = new HashSet<ProjectField>();
        }

        [Key]
        [Required]
        public string ProjectNo { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 3)]
        public string ProjectName { get; set; }
        [StringLength(200)]
        public string ProjectDescription { get; set; }
        public string ProjectIcon { get; set; }
        public string ProjectIconColor { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductDivisionCode { get; set; }
        public string PlantCode { get; set; }
        public string CategoryCode { get; set; }
        public string ProjectOwnerUserName { get; set; }
        public string ProjectOwnerId { get; set; }
        public string ProjectMaturityCode { get; set; }
        public string MilestoneSysId { get; set; }
        public string RoadmapMilestoneSysId { get; set; }
        public string Status { get; set; }
        public int? TargetStartYear { get; set; }
        [StringLength(2)]
        public string TargetStartWorkWeek { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public string TargetStartedBy { get; set; }
        public int? TargetCompletionYear { get; set; }
        [StringLength(2)]
        public string TargetCompletionWorkWeek { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public string TargetCompletedBy { get; set; }
        
        public DateTime? ActualStartDate { get; set; }
        public string ActualStartedBy { get; set; }

        public DateTime? ActualCompletionDate { get; set; }
        public string ActualCompletedBy { get; set; }
        public string RoadmapSysId { get; set; }
        public string PlantRoadmapLinkSysId { get; set; }

        //[StringLength(10)]
        //public string ProductLine { get; set; }
        //[StringLength(200)]
        //public string ProductLineDescription { get; set; }
        //[StringLength(10)]
        //public string PlantType { get; set; }
        //[StringLength(200)]
        //public string PlantTypeDescription { get; set; }
        //[StringLength(10)]
        //public string PackageFamily { get; set; }
        //[StringLength(200)]
        //public string PackageFamilyDescription { get; set; }
        //public string MacroPackage { get; set; }
        //[StringLength(200)]
        //public string MacroPackageDescription { get; set; }
        //[StringLength(10)]
        //public string PackageCode { get; set; }
        //[StringLength(200)]
        //public string PackageCodeDescription { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public User UserModified { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public ProductGroup ProductGroup { get; set; }
        public ProductDivision ProductDivision { get; set; }
        public Plant Plant { get; set; }
        public Category Category { get; set; }
        public User ProjectOwner { get; set; }
        public MaturityLevel ProjectMaturity { get; set; }
        public User Owner { get; set; }
        public User CreatedByUser { get; set; }
        public User ModifiedByUser { get; set; }
        public ICollection<ProjectAuditTrail> AuditTrails { get; set; }
        public ICollection<ProjectMember> Members { get; set; }
        public ICollection<ProjectMilestone> Milestones { get; set; }
        public ICollection<ProjectProduct> Products { get; set; }
        public ICollection<ProjectStatusChange> StatusChanges { get; set; }
        public ICollection<ProjectTask> Tasks { get; set; }
        public ICollection<Annotation> Annotations { get; set; }
        public ICollection<ProjectField> Fields { get; set; }

    }

    public class ProjectSearch : Project
    {
        public string KeyString { get; set; }
    }
}

