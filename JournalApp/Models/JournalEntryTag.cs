using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace JournalApp.Models
{
    public class JournalEntryTag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int JournalEntryId { get; set; }

        public int TagId { get; set; }
    }
}
