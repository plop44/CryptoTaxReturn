using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CapitalGainGenerator;
using OxyPlot;
using OxyPlot.Wpf;

namespace BackTester.Loggers.WindowLoggers
{
    public class WindowHelper
    {
        private readonly Dictionary<object, TabControl> _tabControls = new Dictionary<object, TabControl>();
        private Application? _app;

        private readonly CapitalGainReportSaver _capitalGainReportSaver;

        public WindowHelper(CapitalGainReportSaver capitalGainReportSaver)
        {
            _capitalGainReportSaver = capitalGainReportSaver;
        }

        public void ShowSeparately(Func<TabItem> tabItem, object key)
        {
            if (_app == null)
            {
                var manualResetEvent = new ManualResetEvent(false);

                void Start()
                {
                    _app = new Application();
                    _app.Startup += (sender, args) => { manualResetEvent.Set(); };
                    _app.Run();
                }

                var thread = new Thread(Start) {IsBackground = false};
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                manualResetEvent.WaitOne();
            }

            _app.Dispatcher.Invoke(() =>
            {
                if (!_tabControls.ContainsKey(key))
                {
                    var tabControl = new TabControl();

                    var windowToShow = new Window
                    {
                        Title = key.ToString(),
                        Content = tabControl,
                        Top = 0,
                        Left = 0
                    };

                    windowToShow.Show();

                    _tabControls.Add(key, tabControl);
                }

                _tabControls[key].Items.Add(tabItem.Invoke());
                _tabControls[key].SelectedIndex = 0;
            });
        }

        public void ExportToFiles()
        {
            _app.Dispatcher.Invoke(() =>
            {

                foreach (var keyValuePair in _tabControls)
                {
                    var tabControl = keyValuePair.Value;

                    var tabItems = tabControl.Items.OfType<TabItem>();
                    foreach (var tabItem in tabItems)
                        switch (tabItem.Content)
                        {
                            case PlotView plotView:
                            {
                                var fileName = $"{tabItem.Header as string}" + ".pdf";

                                using var stream = new FileStream(Path.Combine(_capitalGainReportSaver.SavingFolder, fileName), FileMode.Append, FileAccess.Write, FileShare.None);
                                var pdfExporter = new PdfExporter {Width = tabControl.ActualWidth, Height = tabControl.ActualHeight};
                                pdfExporter.Export(plotView.Model, stream);
                                break;
                            }
                        }
                }
            });
        }
    }
}