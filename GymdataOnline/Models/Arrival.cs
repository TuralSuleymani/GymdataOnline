using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Models.Domain
{
    [Table(name:"Arrivals",Schema ="Event")]
    public partial class Arrival
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column(TypeName ="smalldatetime")]
        [Required]
        public DateTime ArrivalDate { get; set; }
        [Required]
        [Column(TypeName ="nvarchar(40)")]
        public string FlightNumber { get; set; }
        [Required]
        [Column(TypeName ="nvarchar(200)")]
        public string From { get; set; }
        [Required]
        public int NumberOfPeople { get; set; }
        public Event Event { get; set; }
        [ForeignKey("FK_Arrival_Event")]
        public int EventId { get; set; }
        public AppUser AppUser { get; set; }
        [ForeignKey("FK_Arrival_User")]
        public string AppUserId { get; set; }
    }

    public partial class Arrival
    {
        public override bool Equals(object obj)
        {
            Arrival Other = obj as Arrival;
            if (Other == null)
                return false;

            if (this.Id == Other.Id && this.AppUserId == Other.AppUserId && this.ArrivalDate == Other.ArrivalDate &&
                this.EventId == Other.EventId && this.FlightNumber == Other.FlightNumber && this.From.ToLower() == Other.From.ToLower() 
                && this.NumberOfPeople == Other.NumberOfPeople)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Arrival Clone()
        {
            //shallow copy for comparing..
            return (Arrival)this.MemberwiseClone();
        }

        //overriding this method for logging easily..
        public override string ToString()
        {
            return String.Format("<strong>Arrival Date</strong> = {0}<br><strong>Flight Number</strong> = {1}<br><strong>From</strong> = {2}<br><strong>Number Of People</strong> = {3}", ArrivalDate,FlightNumber,From,NumberOfPeople);
        }

    }
}
