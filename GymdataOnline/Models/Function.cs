using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Models.Domain
{
    [Table(name:"Functions", Schema ="Event")]
    public class Function
    {
        [Key()]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name:"FunctionId")]
        public int Id { get; set; }

        [Column(TypeName ="nvarchar(200)")]      
        [Required(ErrorMessage ="Function Name is required")]
        public string Name { get; set; }
        public bool? HasLicense { get; set; }
        public Function()
        {
            EventFunctions = new List<Event_Function>();
        }

        public List<Event_Function> EventFunctions { get; set; }
    }
}
