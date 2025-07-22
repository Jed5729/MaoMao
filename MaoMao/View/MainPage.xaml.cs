namespace MaoMao
{
    public partial class MainPage : ContentPage
    {
        private int timesClicked = 0;

        public MainPage()
        {
            InitializeComponent();
        }

		void OnButtonClicked(object sender, EventArgs e)
		{
			timesClicked++;
			label.Text = $"Clicked {timesClicked} times!";
		}
	}
}
