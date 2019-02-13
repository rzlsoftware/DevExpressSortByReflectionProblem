namespace DevExpressSortByReflectionProblem.Services
{
    public class AuthorQueryEntity
    {
        public int AuthorId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
    }

    public class BookQueryEntity : AuthorQueryEntity
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Isbn { get; set; }
    }
}