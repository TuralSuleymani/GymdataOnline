using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Models.Domain
{
    [Table("Events", Schema = "Event")]
    public class Event :Entity
    {
        public Event()
        {
            EventStages = new HashSet<Event_Stage>();
            EventFunctions = new HashSet<Event_Function>();
            UserEvents = new HashSet<User_Events>();
        }
        public ICollection<User_Events> UserEvents { get; set; }

        [Column(TypeName ="nvarchar(500)",Order =4)]
        [Required(ErrorMessage ="Event Name can't be empty!")]
        public string Name { get; set; }

       
        [Display(Name = "Event status",Order =5)]
        public EventStatus EventStatus { get; set; }


        [Required(ErrorMessage ="Event status can't be empty or invalid")]
        [Display(Name = "Event status", Order = 5)]
        public int EventStatusId { get; set; }


        [Column(TypeName ="smalldatetime",Order =6)]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }


        [Display(Name = "Updated At",Order =7)]
        [Column(TypeName ="smalldatetime")]
        public DateTime UpdatedAt { get; set; }


        [Display(Name = "Currency")]
        public Currency Currency { get; set; }


        [ForeignKey("FK_Event_Currency")]
        [Display(Name="Currency")]
        [Required(ErrorMessage ="Currency can't be empty or invalid")]
        public int CurrencyId { get; set; }


        public ICollection<Event_Stage> EventStages { get; set; }

      
        public ICollection<Event_Function> EventFunctions { get; set; }

        #region Delegation
        [Required(ErrorMessage = "Please fill Delegation's Start Date!")]
        [Column(name: "DelegationStartDate")]
        [Display(Name = "Start Date")]
        public DateTime DelegationStartDate { get; set; }


        [Required(ErrorMessage = "Please fill Delegation's End Date!")]
        [Display(Name = "End Date")]
        public DateTime DelegationEndDate { get; set; }
        #endregion

        #region Accommodation
        [Required(ErrorMessage = "Please fill Accommodation's Start Date!")]
        [Display(Name = "Start Date")]
        public DateTime AccommodationStartDate { get; set; }


        [Display(Name = "End Date")]
        [Required(ErrorMessage = "Please fill Accommodation's End Date!")]
        public DateTime AccommodationEndDate { get; set; }


        [Required(ErrorMessage = "Please fill Accommodation's Arrival Start Date!")]
        [Display(Name = "CheckIn Date")]
        public DateTime AccommodationArrivalStartDate { get; set; }


        [Required(ErrorMessage = "Please fill Accommodation's Departure Date!")]
        [Display(Name = "CheckOut Date")]
        public DateTime AccommodationArrivalEndDate { get; set; }
        #endregion

        #region Meals
        [Required(ErrorMessage = "Please fill Meal's Start Date!")]
        [Display(Name = "Start Date")]
        public DateTime MealsStartDate { get; set; }


        [Required(ErrorMessage = "Please fill Meal's End Date!")]
        [Display(Name = "End Date")]
        public DateTime MealsEndDate { get; set; }

        #endregion

        #region Visa
        [Required(ErrorMessage = "Please fill Visa's Start Date!")]
        [Display(Name = "Start Date", AutoGenerateField = false)]
        public DateTime VisaStartDate { get; set; }


        [Required(ErrorMessage = "Please fill Visa's End Date!")]
        [Display(Name = "End Date")]
        public DateTime VisaEndDate { get; set; }
        #endregion

        #region Rooming
        [Required(ErrorMessage = "Please fill Rooming's Start Date!")]
        [Column(TypeName = "smalldatetime")]
        [Display(Name = "Start Date")]
        public DateTime RoomingStartDate { get; set; }


        [Required(ErrorMessage = "Please fill Rooming's End Date!")]
        [Display(Name = "End Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime RoomingEndDate { get; set; }
        #endregion

        #region Travel
        [Required(ErrorMessage = "Please fill Travel's  Start Date!")]
        [Display(Name = "Start Date")]
        public DateTime TravelStartDate { get; set; }


        [Required(ErrorMessage = "Please fill Travel's End Date!")]
        [Display(Name = "End Date")]
        public DateTime TravelEndDate { get; set; }
        #endregion

        #region Photo
        [Required(ErrorMessage = "Please fill Photo's  Start Date!")]
        [Display(Name = "Start Date")]
        public DateTime PhotoStartDate { get; set; }


        [Required(ErrorMessage = "Please fill Photo's Start Date!")]
        [Display(Name = "End Date")]
        public DateTime PhotoEndDate { get; set; }
        #endregion
        #region Music
        [Display(Name = "Start Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime? MusicStartDate { get; set; }
        [Display(Name = "End Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime? MusicEndDate { get; set; }
        #endregion
        #region Interest
        [Display(Name = "Start Date")]
        [Column(TypeName ="smalldatetime")]
        public DateTime? InterestStartDate { get; set; }
        [Display(Name = "End Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime? InterestEndDate { get; set; }
        #endregion

        #region Provisional
        [Display(Name = "Start Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime? ProvisionalStartDate { get; set; }
        [Display(Name = "End Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime? ProvisionalEndDate { get; set; }
        #endregion

        #region Definitive
        [Column(TypeName = "smalldatetime")]
        [Display(Name = "Start Date")]
        public DateTime? DefinitiveStartDate { get; set; }
        [Display(Name = "End Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime? DefinitiveEndDate { get; set; }
        #endregion

        #region Nominative
        [Display(Name = "Start Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime? NominativeStartDate { get; set; }
        [Display(Name = "End Date")]
        [Column(TypeName = "smalldatetime")]
        public DateTime? NominativeEndDate { get; set; }
        #endregion

        [Required]
        public string EventPlaceInformation { get; set; }

        [DefaultValue("0")]
        public bool? IsDeleted { get; set; }
        public AppUser AppUser { get; set; }
        public string AppUserId { get; set; }

        [Display(Name = "Bank Information")]
        public string BankDescription { get; set; }

        public string ImagePath { get; set; }
       
        [Display(Name ="Mail Template")]
        public string MailTemplate { get; set; }
        [Required]
        public MediaCategoryStandardType MediaCategoryStandardType { get; set; }


    }
}
