using System.Collections.Generic;

namespace DevExpressSortByReflectionProblem.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }

        public HashSet<Book> Books { get; } = new HashSet<Book>();
    }
}
