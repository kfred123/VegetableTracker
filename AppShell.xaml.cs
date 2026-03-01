namespace VegetableTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("SelectVegetablePage", typeof(SelectVegetablePage));
            Routing.RegisterRoute("DetailsPage", typeof(DetailsPage));
            Routing.RegisterRoute("SuggestionsPage", typeof(SuggestionsPage));
        }
    }
}
