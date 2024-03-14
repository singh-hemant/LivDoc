using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LivDocApp.Models
{
    public class Employee: IdentityUser
    {

    
        public required string Name { get; set; }
 

        public required string SapId { get; set; }
     

        public required string Location { get; set; }
       

    }
}
