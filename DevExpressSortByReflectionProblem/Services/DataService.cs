using DevExpressSortByReflectionProblem.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Tynamix.ObjectFiller;

namespace DevExpressSortByReflectionProblem.Services
{
    public class DataService : IDataService
    {
        private readonly SampleDbContext context;

        public DataService(SampleDbContext context)
        {
            this.context = context;
        }

        public IQueryable<BookQueryEntity> GetBooksQuery()
            => context.Books
            .Include(b => b.Author)
            .Select(b => new BookQueryEntity
            {
                BookId = b.Id,
                Title = b.Title,
                Description = b.Description,
                Isbn = b.ISBN,
                AuthorId = b.Author.Id,
                Firstname = b.Author.Firstname,
                Lastname = b.Author.Lastname,
                Email = b.Author.Email
            });

        internal void GenerateSampleDatabase()
        {
            if (!context.Database.EnsureCreated())
                return;

            var bookFiller = new Filler<Book>();
            bookFiller.Setup()
                .OnProperty(b => b.Id).IgnoreIt()
                .OnProperty(b => b.AuthorId).IgnoreIt()
                .OnProperty(b => b.Author).IgnoreIt()
                .OnProperty(b => b.Title).Use(new RandomListItem<string>(
                    "Kochen für Einsteiger",
                    "C# in Depth",
                    "Harry Potter",
                    "Concurrency in C# Cookbook",
                    "Programming in F#"))
                .OnProperty(b => b.ISBN).Use(new PatternGenerator("ISBN {C:100}-{0}-{10000}-{100}-{0}"));

            var authorFiller = new Filler<Author>();
            authorFiller.Setup()
                .OnProperty(b => b.Id).IgnoreIt()
                .OnProperty(b => b.Books).IgnoreIt()
                .OnProperty(b => b.Firstname).Use(new RealNames(NameStyle.FirstName))
                .OnProperty(b => b.Lastname).Use(new RealNames(NameStyle.LastName))
                .OnProperty(b => b.Email).Use(new EmailAddresses());

            var random = new Random();
            var authors = authorFiller.Create(500000);
            foreach (var a in authors)
                foreach (var b in bookFiller.Create(random.Next(1, 5)))
                    a.Books.Add(b);

            context.Authors.AddRange(authors);
            context.SaveChanges();
        }
    }
}
