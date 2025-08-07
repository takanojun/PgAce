using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App.Views
{
    public class SearchView : DockContent
    {
        private readonly TextBox _input;
        private readonly Button _searchButton;

        public event Action<string> SearchRequested;

        public SearchView()
        {
            Text = "Search";
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            _input = new TextBox { Width = 200 };
            _searchButton = new Button { Text = "Search" };
            _searchButton.Click += (s, e) => SearchRequested?.Invoke(_input.Text);
            panel.Controls.Add(_input);
            panel.Controls.Add(_searchButton);
            Controls.Add(panel);
        }
    }
}
