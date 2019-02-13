using DevExpressSortByReflectionProblem.Services;

namespace DevExpressSortByReflectionProblem
{
    public sealed class ViewModelLocator
    {
        public MainViewModel MainViewModel { get; }

        public ViewModelLocator()
        {
            var dataService = new DataService(new SampleDbContext());
            dataService.GenerateSampleDatabase();

            MainViewModel = new MainViewModel(dataService);
        }
    }
}
