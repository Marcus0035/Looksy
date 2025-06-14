using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Looksy.Infrastructure.Data.Models
{
    public class Group : BaseModel
    {
        [Required]
        public string Name { get; set; }
        public ICollection<User> Members { get; set; } = new List<User>();
        public ICollection<Photo> Photos { get; set; } = new List<Photo>();


    }
}
