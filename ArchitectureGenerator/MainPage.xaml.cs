using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace ArchitectureGenerator
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Dadada();
        }

        public async void Dadada()
        {
            //await CppArchGenerator.GenerateAsync("C:\\Dev\\Repos\\ShadowDriver\\ShadowDriver");
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();
            if(folder != null)
            {
                var layers = await CppArchGenerator.GenerateAsync(folder);

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    MakeRectanges(layers);
                });
            }
        }

        /// <summary>
        /// 用于记录每个按钮所代表的模块在第几层
        /// </summary>
        Dictionary<Button, HashSet<int>> _buttonLayers = new Dictionary<Button, HashSet<int>>();
        public void MakeRectanges(Dictionary<string, HashSet<int>> layers)
        {
            Dictionary<int, int> layerItemCount = new Dictionary<int, int>();
            Dictionary<string, Button> rectangels = new Dictionary<string, Button>();
            
            foreach(var i in layers)
            {
                double width = 30;
                double unitHeight = 40;
                double margin = 10;
                double height = unitHeight * i.Value.Count + margin * (i.Value.Count - 1);

                foreach(var j in i.Value)
                {
                    if(!layerItemCount.ContainsKey(j))
                    {
                        layerItemCount[j] = 0;
                    }
                    layerItemCount[j]++;
                }

                int maxJ = 0;
                foreach(var j in i.Value)
                {
                    maxJ = layerItemCount[j] > maxJ ? layerItemCount[j] : maxJ;
                }


                Button button = new Button()
                {
                    Padding = new Thickness(5,0,5,0),
                    Margin = new Thickness(0),
                    Height = height
                };
                Grid grid = new Grid();
                button.Content = grid;

                Rectangle matchedRectangle = new Rectangle()
                {
                    //Width = width,
                    Height = height,
                    //Margin = new Thickness(5)
                };

                TextBlock textBlock = new TextBlock()
                {
                    Text = i.Key,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0)
                };
                
                grid.Children.Add(matchedRectangle);
                rectangels.Add(i.Key, button);
                grid.Children.Add(textBlock);

                Canvas.SetTop(button, ((i.Value.First() - 1) * (unitHeight + margin)));
                Canvas.SetLeft(button, maxJ * 200);
                button.SizeChanged += Button_SizeChanged;
                ArchitectureCanvas.Children.Add(button);
                _buttonLayers.Add(button, i.Value);
            }
            //Rectangle rectangle = new Rectangle
            //{
            //    Width = 100,
            //    Height = 30,
            //    Fill = new SolidColorBrush(Windows.UI.Colors.Blue),
            //    Margin = new Thickness(5)
            //};
            //Canvas.SetTop(rectangle, 50);
            
        }

        /// <summary>
        /// 记录某个层次目前的按钮已经放到什么位置了。
        /// </summary>
        Dictionary<int, double> _yPositions = new Dictionary<int, double>();
        private void Button_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Button button = (Button)sender;

            var layerSet = _buttonLayers[button];
            if(layerSet != null)
            {
                double maxY = 0;
                foreach(var layer in layerSet)
                {
                    if(!_yPositions.ContainsKey(layer))
                    {
                        _yPositions[layer] = 0;
                    }
                    else
                    {
                        maxY = _yPositions[layer] > maxY ? _yPositions[layer] : maxY;
                    }
                }

                double newY = maxY + 10;
                Canvas.SetLeft(button, newY);
                foreach(var layer in layerSet)
                {
                    _yPositions[layer] = newY + button.ActualWidth;
                }

                ArchitectureCanvas.Width = newY + button.ActualWidth + 10;
            }

            
        }
    }
}
