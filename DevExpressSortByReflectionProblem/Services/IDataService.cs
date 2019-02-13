using System.Linq;

namespace DevExpressSortByReflectionProblem.Services
{
    public interface IDataService
    {
        IQueryable<BookQueryEntity> GetBooksQuery();
    }
}