using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Models.Domain
{
    [Table(name:"Delegations",Schema ="Event")]
    public partial class Delegation 
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }

        [Column(Order =2,TypeName ="nvarchar(200)")]
        [Required]
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        [Column(Order =3,TypeName ="nvarchar(250)")]
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required]
        [Display(Name = "Email Address")]
        [Column(Order =4,TypeName ="varchar(100)")]
        public string Email { get; set; }

        [Required]
        public bool IsMedia { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        [Column(Order = 4, TypeName = "varchar(30)")]
        public string Phone { get; set; }
        [DataType(DataType.PhoneNumber)]
        [Required]
        [Column(Order = 5, TypeName = "varchar(30)")]
        public string MobilePhone { get; set; }

        [Required]
        [Display(Name = "Federation Name")]
        public string FederationName { get; set; }

        public Event Event { get; set; }

        [Required]
        [ForeignKey("FK_Place_Event")]
        public int EventId { get; set; }

        public AppUser AppUser { get; set; }

        [ForeignKey("FK_Delegation_User_Identify")]
        public string AppUserId { get; set; }

       

    }

    public partial class Delegation 
    {
        public override bool Equals(object obj)
        {
            Delegation Other = obj as Delegation;
            if (Other == null)
                return false;

            if (this.Id == Other.Id && this.FirstName.ToLower() == Other.FirstName.ToLower() && this.LastName == Other.LastName.ToLower() &&
                this.Email.ToLower() == Other.Email.ToLower() && this.Phone == Other.Phone && this.MobilePhone == Other.MobilePhone && this.FederationName.ToLower() == Other.FederationName.ToLower() && this.EventId == Other.EventId && this.AppUserId == Other.AppUserId)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Delegation Clone()
        {
            //shallow copy for comparing..
            return (Delegation)this.MemberwiseClone();
        }

        //overriding this method for logging easily..
        public override string ToString()
        {
            return String.Format("<strong>FirstName</strong> = {0}<br><strong>LastName</strong> = {1}<br><strong>Email</strong> = {2}<br><strong>Phone Number</strong> = {3}<br><strong>Mobile Phone</strong> = {4}<br><strong>Federation Name</strong> = {5}", FirstName,LastName,Email,Phone,MobilePhone,FederationName);
        }

    }
}
