/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 *   Build command:
 *   devenv ./DependencyAnalyzer.sln /rebuild debug
 *   
 *   Maintenance History:
 *   Ver 1.0  Nov. 14  2014 created by Shikai Jin 
 */

/*
 *   public interfaces 
 *  public void DrawDependency(Dictionary<string,List<string>> deps,bool ispackage=true) //draw the deps diagrams to the screen
 *  public void SendTypeDepQueryForProject(string name)//send type query 
 *  public void SendPackDepQueryForProject(string name)//send package query
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DependencyAnalyzer;
using Util;
using System.Xml.Linq;
using System.IO;
namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance = null;


        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        string ServerUrl = "";
        public MainWindow()
        {
            InitializeComponent();
            ServerUrl = ConfigManager.Instance.LocalUrl;


            Instance = this;
            MessageProcessor.Instance.AddHandler(new REC_ANALYSIS());
            SendProjectQuery();

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;

            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;

            slider.ValueChanged += OnSliderValueChanged;

            //DrawRectangle();

            //DrawPackage("Apple", 100, 100);
        }
        /// <summary>
        /// draw arrow
        /// </summary>
        /// <param name="p"></param>
        void DrawArrow(Point p)
        { 

            double size = 5;
            Line left = MakeLine(p, new Point(p.X-size,p.Y-size));
            Line right = MakeLine(p, new Point(p.X + size, p.Y - size));
            DrawLine(right);
            DrawLine(left);
        }
        /// <summary>
        /// generate a line 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        static Line MakeLine(Point p1, Point p2)
        {
            Line l = new Line();
            l.X1 = p1.X;
            l.Y1 = p1.Y;
            l.X2 = p2.X;
            l.Y2 = p2.Y;
            l.StrokeThickness = 1;
            l.Stroke = System.Windows.Media.Brushes.Black;
            return l;
        }
        /// <summary>
        /// make a test line
        /// </summary>
        /// <returns></returns>
        static Line MakeSampleLine()
        {
            Line l = new Line();
            l.X1 = 120;
            l.Y1 = 120;
            l.X2 = 10;
            l.Y2 = 10;
            l.StrokeThickness = 1;
            l.Stroke = System.Windows.Media.Brushes.Black;
            return l;     
        }
        /// <summary>
        /// draw a line
        /// </summary>
        /// <param name="l"></param>
        void DrawLine(Line l)
        {
            DiagramCanvas.Children.Add(l);
        }
        /// <summary>
        /// draw a line from p1 to p2
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        void DrawLine(Point p1, Point p2)
        {
            Line l = MakeLine(p1, p2);
            DiagramCanvas.Children.Add(l);
        }
        /// <summary>
        /// clone a point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        Point Clone(Point p)
        {
            Point ret = new Point();
            ret.X = p.X;
            ret.Y = p.Y;
            return ret;
        }
        /// <summary>
        /// draw diagram direction line 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        void DrawArrowLine(Point p1,Point p2)
        {
            Point top = p1.Y < p2.Y ? p1 : p2;
            
            Point bot = p1.Y > p2.Y ? p1 : p2;

            double middle = (top.Y + bot.Y) / 2.0;
            Point midt = Clone(top);
            midt.Y=middle;
            Point midb = Clone(bot);
            midb.Y=middle;

            DrawLine(top,midt);
            DrawLine(midt,midb);
            DrawLine(midb,bot);
            DrawArrow(bot);
        }
        delegate Rect DrawFunc(string s,double x,double y);
        /// <summary>
        /// draw all dependency
        /// </summary>
        /// <param name="deps"></param>
        /// <param name="ispackage"></param>
        public void DrawDependency(Dictionary<string,List<string>> deps,bool ispackage=true)
        {
            DiagramCanvas.Children.Clear();
            double mw = 0;
            double mh = DiagramCanvas.ActualHeight;
            double orginhori = 30;
            double tmpx = 10;
            double tmpy = 30;
            double space = 50;
            DrawFunc dpack = DrawPackage;
            DrawFunc dtype = DrawType;
            DrawFunc DFunc = ispackage ? (dpack) : (dtype);
            foreach (var p in deps)
            {
                tmpx = orginhori;
                Rect r = DFunc(p.Key, tmpx, tmpy) ;
                Point arrowbegin = new Point((r.Left+r.Right)/2.0,r.Bottom);
                tmpy += (r.Height + space);
                tmpx = orginhori;
                List<string> sl = p.Value;
                foreach (var used in sl)
                {
                    Rect usedrect = DFunc(used, tmpx, tmpy);
                    Point arrowend = new Point((usedrect.Left + usedrect.Right) / 2.0,usedrect.Top);
                    DrawArrowLine(arrowbegin,arrowend);
                    tmpx += (usedrect.Width + space);
                    if (tmpx > mw)
                        mw = tmpx;
                }
                tmpy += (r.Height + space);
            }
            mh = tmpy;
            DiagramCanvas.Width = mw;
            DiagramCanvas.Height = mh;
            scrollViewer.ScrollToHorizontalOffset(0);
            scrollViewer.ScrollToVerticalOffset(0); 
        }
        /// <summary>
        /// draw text in the position
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        TextBlock DrawText(string name, double x, double y)
        {
            TextBlock tb = new TextBlock();
            tb.Text = name;
            tb.Measure(new Size(0, 0));
            tb.Arrange(new Rect(0, 0, 0, 0));
            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            DiagramCanvas.Children.Add(tb);
            return tb;
        }
        /// <summary>
        /// draw a pckage
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Rect DrawPackage(string name, double x, double y)
        {
            return DrawObject(name, x, y, 10,true);
        }
        /// <summary>
        /// draw atype
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Rect DrawType(string name, double x, double y)
        {
            return DrawObject(name, x, y, 10, false);
        }
        /// <summary>
        /// draw a small rectangle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void DrawLittleCornerRect(double x, double y)
        {
            Rectangle rect = new Rectangle();
            rect.Stroke = System.Windows.Media.Brushes.Black;
            rect.Fill = System.Windows.Media.Brushes.SkyBlue;
            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.VerticalAlignment = VerticalAlignment.Center;
            rect.Width = 20;
            rect.Height = 10;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y - 10);
            DiagramCanvas.Children.Add(rect);
        }
        /// <summary>
        /// draw a rect representing a object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="margin"></param>
        /// <param name="ispackage"></param>
        /// <returns></returns>
        Rect DrawObject(string name, double x, double y,int margin,bool ispackage=false)
        {
            Rectangle rect = new Rectangle();
            rect.Stroke = System.Windows.Media.Brushes.Black;
            rect.Fill = System.Windows.Media.Brushes.SkyBlue;
            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.VerticalAlignment = VerticalAlignment.Center;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            DiagramCanvas.Children.Add(rect);


            TextBlock tb = DrawText(name,x+margin,y+margin);

            rect.Width = tb.ActualWidth+2*margin;
            rect.Height = tb.ActualHeight+2*margin;

            if (ispackage)
                DrawLittleCornerRect(x, y);
            return new Rect(x, y, rect.Width, rect.Height);
        }

        /// <summary>
        /// send project query
        /// </summary>
        public void SendProjectQuery()
        {
            SendQuery(Query.Make(Query.QType.LIST,""));
        }
        /// <summary>
        /// send type query
        /// </summary>
        /// <param name="name"></param>
        public void SendTypeDepQueryForProject(string name)
        {
            SendQuery(Query.Make(Query.QType.TYPE_DEPENDENCY, name));
        }
        /// <summary>
        /// send pcks query
        /// </summary>
        /// <param name="name"></param>
        public void SendPackDepQueryForProject(string name)
        {
            SendQuery(Query.Make(Query.QType.PACKAGE_DEPENDENCY, name));
        }
        /// <summary>
        /// send generic query
        /// </summary>
        /// <param name="q"></param>
        async void SendQuery(Query q)
        {
            bool suc = false;
            try
            {
                Message m = Message.MakeQuery(ServerUrl, q.ToXML().ToString());
                suc=await m.SendAsyc();
                if (suc == true)
                    return;
            }
            catch (Exception e){ DebugLog.Instance.Write(e);}
            try
            {
                foreach (var svr in ConfigManager.Instance.OtherServers)
                {
                    Message m = Message.MakeQuery(svr.Value, q.ToXML().ToString());
                    suc=await m.SendAsyc();
                    if (suc == true)
                    {
                        ServerUrl = svr.Value;
                        return;
                    }
                }
            }
            catch (Exception e) { DebugLog.Instance.Write(e); }
            if(suc==false)
                MessageBox.Show("Serves are inaccessible!");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        { 
            
        }

        /// <summary>
        /// handle double click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleClick(object sender, MouseButtonEventArgs e)
        {
            SendQueryByRadioBtn();
        }
        /// <summary>
        /// handle mouse move event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }
        /// <summary>
        /// handle left button down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }
        /// <summary>
        /// handle mouse wheel event inside the scrollview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(grid);

            if (e.Delta > 0)
            {
                slider.Value += 1;
            }
            if (e.Delta < 0)
            {
                slider.Value -= 1;
            }

            e.Handled = true;
        }
        /// <summary>
        /// left button up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }
        /// <summary>
        /// slider event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);
        }
        /// <summary>
        /// scroll view event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;
                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, grid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(grid);

                    lastMousePositionOnTarget = null;
                }
                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / grid.Width;
                    double multiplicatorY = e.ExtentHeight / grid.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
        /// <summary>
        /// query type
        /// </summary>
        void SendQueryByRadioBtn()
        {
            if (ProjectList.SelectedItem == null)
            {
                MessageBox.Show("Please select one project!");
                return;
            }
            if ((PackDepRadBtn.IsChecked.HasValue) ?
               PackDepRadBtn.IsChecked.Value : false)
            {
                SendPackDepQueryForProject(((string)ProjectList.SelectedItem));
            }
            else
            {
                SendTypeDepQueryForProject(((string)ProjectList.SelectedItem));
            }       
        }
        /// <summary>
        /// query click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Query_Click(object sender, RoutedEventArgs e)
        {
            SendQueryByRadioBtn();
        }
        /// <summary>
        /// refresh project list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            SendProjectQuery();
        }
        /// <summary>
        /// handle linq query event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Linq_Run_Click(object sender, RoutedEventArgs e)
        {
            string str=XMLTextBox.Text;
       
            XDocument doc=XDocument.Parse(str);
            var result =from n in doc.Root.Elements("dependency")
                        where n.Element("name").Value == XMLQueryEdit.Text
                         select n;
            var names = from name in result.Descendants() where name.Name == "using" select name.Value;
            XMLResultBox.Text = String.Join(", ",names);
        }
        /// <summary>
        /// handle save as event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; 
            dlg.DefaultExt = ".xml"; 
            dlg.Filter = "Text documents (.xml)|*.xml"; 

           
            Nullable<bool> result = dlg.ShowDialog();

            
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    using(StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(XMLTextBox.Text);
                    }
                }
            }
        }
    }

}
