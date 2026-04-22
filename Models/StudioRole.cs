using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YogaStudioLRAManagementSystem.Models
{
    [Table("YS_STUDIO_ROLES")]
    public class StudioRole
    {
        [Key]
        [Column("STUDIO_ROLE_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //to ensure db generates the value
        public int StudioRoleId { get; set; }


        [Column("ROLE_NAME")]
        [Required (ErrorMessage = "Role name is required.")]
        [StringLength (15)]
        public string RoleName { get; set; } = null!;

        [Column("REQUIRES_CERT")] 
        public bool RequiresCertification { get; set; }

        // Navigation property
        public ICollection<Employee> Employees { get; set; } = new List<Employee>(); //Studio role has many employees [relationship]


    }
}