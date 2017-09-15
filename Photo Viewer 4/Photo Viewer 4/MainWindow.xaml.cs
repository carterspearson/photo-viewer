using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Photo_Viewer_4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PhotoCollection photos = new PhotoCollection();
        PhotoCollection originalPhotos = new PhotoCollection();
        public static bool clickable = true;

        public MainWindow()
        {
            InitializeComponent();
            photos = new PhotoCollection(new DirectoryInfo("Start-up"));
            listBox1.ItemsSource = photos;
        }

        public class SearchFunctions
        {
            public static List<FileInfo> GetFilesByExtensions(DirectoryInfo dir, string[] extensions)
            {
                try
                {
                    if (extensions == null)
                        extensions = new string[] { "*" };
                    IEnumerable<FileInfo> files = Enumerable.Empty<FileInfo>();
                    foreach (string ext in extensions)
                    {
                        try
                        {
                            files = files.Concat(dir.GetFiles(ext));
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                    }
                    return files.ToList();
                }
                catch (ArgumentException)
                {
                    System.Windows.MessageBox.Show("Invalid File Extension");
                    return new List<FileInfo>();
                }
            }

            public static List<FileInfo> FilterNameDate(List<FileInfo> list, string text)
            {
                try
                {
                    foreach (FileInfo f in list.ToList())
                    {
                        if (getDate(true) != null && getDate(false) != null)
                        {
                            DateTime fileCreationTime = new DateTime(f.CreationTime.Year, f.CreationTime.Month, f.CreationTime.Day, 0, 0, 0);
                            DateTime minTime = new DateTime(getDate(true).Value.Year, getDate(true).Value.Month, getDate(true).Value.Day, 0, 0, 0);
                            DateTime maxTime = new DateTime(getDate(false).Value.Year, getDate(false).Value.Month, getDate(false).Value.Day, 0, 0, 0);
                            if (DateTime.Compare(fileCreationTime, minTime) < 0 || DateTime.Compare(fileCreationTime, maxTime) > 0)
                                list.Remove(f);
                        }
                        if (!(f.Name.ToLower().Contains(text.ToLower())) && text != "")
                            list.Remove(f);
                    }
                    return list;
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Name and Date Filter Error");
                    return null;
                }
                    
            }
            
            public static string[] getExtensionArray(string text)
            {
                try
                {
                    char[] separatingChars = new char[] { ' ', ',', ';' };
                    if (text == "")
                        return new string[] { "*" };

                    string[] extensionArray = text.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < extensionArray.Count(); i++)
                    {
                        if (!(extensionArray[i].StartsWith(".")))
                            extensionArray[i] = "." + extensionArray[i];
                        extensionArray[i] = "*" + extensionArray[i];
                    }

                    return extensionArray;
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Error Interpreting Extension Information");
                    return null;
                }
            }
        }

        public class PhotoCollection : ObservableCollection<Photo>
        {
            public PhotoCollection() { }

            public PhotoCollection(string path) : this(new DirectoryInfo(path)) { }

            public PhotoCollection(DirectoryInfo _directory)
            {
                directory = _directory;
                Update();
            }

            public string Path
            {
                set
                {
                    directory = new DirectoryInfo(value);
                    Update();
                }
                get { return directory.FullName; }
            }

            public DirectoryInfo Directory
            {
                set
                {
                    directory = value;
                    Update();
                }
                get { return directory; }
            }

            public void Update()
            {
                this.Clear();
                try
                {
                    if (directory.Name != "Start-up")
                    {
                        int totalCount = 0;
                        int count = 0;
                        clickable = false;

                        string[] extensionArray = SearchFunctions.getExtensionArray(((MainWindow)System.Windows.Application.Current.MainWindow).extensionText.Text);
                        var fileArrayExtensions = SearchFunctions.GetFilesByExtensions(directory, extensionArray);
                        var fileArray = SearchFunctions.FilterNameDate(fileArrayExtensions, ((MainWindow)System.Windows.Application.Current.MainWindow).nameTextBox.Text);

                        ((MainWindow)System.Windows.Application.Current.MainWindow).fileLoadedText.Text = "Loading File: " + directory.FullName;

                        foreach (FileInfo f in fileArray)
                        {
                            AddToPhotoList(f, ref count, fileArray.Count(), ref totalCount);
                        }

                        //foreach (DirectoryInfo d in directory.GetDirectories())
                        for (int i = directory.GetDirectories().Count() - 1; i >= 0; i--)
                        {
                            DirectoryInfo d = directory.GetDirectories()[i];
                            count = 0;
                            ((MainWindow)System.Windows.Application.Current.MainWindow).fileLoadedText.Text = "Loading File: " + d.FullName;

                            fileArrayExtensions = SearchFunctions.GetFilesByExtensions(d, extensionArray);
                            fileArray = SearchFunctions.FilterNameDate(fileArrayExtensions, ((MainWindow)System.Windows.Application.Current.MainWindow).nameTextBox.Text);

                            foreach (FileInfo f in fileArray)
                            {
                                AddToPhotoList(f, ref count, fileArray.Count(), ref totalCount);
                            }
                        }

                        ((MainWindow)System.Windows.Application.Current.MainWindow).fileLoadedText.Text = "";
                        ((MainWindow)System.Windows.Application.Current.MainWindow).progressText.Text = "";
                        clickable = true;
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    System.Windows.MessageBox.Show("No Such Directory");
                }
            }
            
            /*public void Filter(PhotoCollection originalCollection, string [] extensionArray)
            {
                foreach (Photo p in originalCollection)
                {
                    if (p.date < getDate(true) || p.date > getDate(false))
                    {
                        if (this.Contains(p))
                            Remove(p);
                    }
                    else
                    {
                        if (!this.Contains(p))
                            Add(p);
                    }
                    if (!extensionArray.Contains(p.extension))
                    {
                        if (this.Contains(p))
                            Remove(p);
                    }
                    else
                    {
                        if (!this.Contains(p))
                            Add(p);
                    }
                }
            }*/

            public void AddToPhotoList(FileInfo file, ref int count, int fileCount, ref int totalCount)
            {
                try
                {
                    int max = fileCount + totalCount - count;
                    if (max > Int32.Parse(((MainWindow)System.Windows.Application.Current.MainWindow).numberPhotosBox.Text))
                        max = Int32.Parse(((MainWindow)System.Windows.Application.Current.MainWindow).numberPhotosBox.Text);

                    if (totalCount < max)
                    {
                        Photo tempPhoto = new Photo(file.FullName);
                        count++;
                        totalCount++;
                        Add(tempPhoto);
                        if (count % 2 == 0)
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).progressText.Text = "Progress: " + (count * 100) / (max - totalCount + count) + "%";
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                }
                catch (NotSupportedException)
                {
                }
            }

            public DirectoryInfo directory;
        }

        public class Photo
        {
            public Photo(string _path)
            {
                try
                {
                    path = _path;
                    source = new Uri(_path);
                    BitmapImage sourceImage = new BitmapImage();

                    sourceImage.BeginInit();
                    sourceImage.UriSource = source;
                    sourceImage.DecodePixelHeight = 65;
                    sourceImage.EndInit();

                    date = new FileInfo(_path).CreationTime;
                    extension = new FileInfo(_path).Extension;

                    image = sourceImage;
                }
                catch (IOException)
                {
                    System.Windows.MessageBox.Show("Unable to Read File");
                }
                catch (UnauthorizedAccessException)
                {
                    System.Windows.MessageBox.Show("Unable to Access File");
                }
            }

            /*public void loadImage()
            {
                BitmapImage sourceImage = new BitmapImage();

                sourceImage.BeginInit();
                sourceImage.UriSource = source;
                sourceImage.DecodePixelHeight = 65;
                sourceImage.EndInit();

                date = new FileInfo(path).CreationTime;
                extension = new FileInfo(path).Extension;

                image = sourceImage;
            }*/

            public string path;
            public DateTime date;
            public string extension;

            public Uri source;
            public string Source { get { return path; } }

            public string SourceName
            {
                get
                {
                    string[] words = path.Split('\\');
                    return words.Last();
                }
            }

            public BitmapSource image;
            public BitmapSource Image 
            { 
                get 
                    { return image; } 
                set 
                    { image = value; } 
            }
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clickable)
                {
                    int n = 0;
                    if (!int.TryParse(numberPhotosBox.Text, out n))
                        System.Windows.MessageBox.Show("Invalid Number of Files");
                    else
                    {
                        if (int.Parse(numberPhotosBox.Text) > 2000)
                            numberPhotosBox.Text = "1000"; 
                        System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            extensionText.Text = "";
                            nameTextBox.Text = "";
                            photos.Directory = new DirectoryInfo(dlg.SelectedPath);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
            }

        }

        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                BitmapImage sourceImage = new BitmapImage();
                BitmapImage reference = new BitmapImage(photos[listBox1.SelectedIndex].source);

                sourceImage.BeginInit();
                sourceImage.UriSource = photos[listBox1.SelectedIndex].source;
                if (reference.Width > reference.Height)
                    sourceImage.DecodePixelWidth = 1024;
                else
                    sourceImage.DecodePixelHeight = 768;
                sourceImage.EndInit();


                inspectedImage.Source = sourceImage;
                inspectorTab.Header = "Inspector (" + photos[listBox1.SelectedIndex].SourceName + ")";
                tabControl1.SelectedIndex = 1;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("No Image Available");
            }
        }

        public static DateTime? getDate(bool isStart)
        {
            try
            {
                if (isStart)
                    return ((MainWindow)System.Windows.Application.Current.MainWindow).startDatePicker.SelectedDate;
                else
                    return ((MainWindow)System.Windows.Application.Current.MainWindow).endDatePicker.SelectedDate;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error Accessing Date Information");
                return null;
            }
        }

        private void clearDateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clickable)
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).startDatePicker.SelectedDate = null;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).endDatePicker.SelectedDate = null;
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Clear Date Button Error");
            }
        }

        private void todayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clickable)
                {
                    startDatePicker.SelectedDate = DateTime.Today;
                    endDatePicker.SelectedDate = DateTime.Today;
                    photos.Update();
                }
            }
            catch (NullReferenceException)
            {
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Today Button Error");
            }
        }

        private void weekButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clickable)
                {
                    startDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
                    endDatePicker.SelectedDate = DateTime.Today;
                    photos.Update();
                }
            }
            catch (NullReferenceException)
            {
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Last Week Button Error");
            }
        }

        private void monthButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clickable)
                {
                    startDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
                    endDatePicker.SelectedDate = DateTime.Today;
                    photos.Update();
                }
            }
            catch (NullReferenceException)
            {
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Last Month Button Error");
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (clickable)
            {
                if ((getDate(true) == null && getDate(false) != null) || (getDate(true) != null && getDate(false) == null) || getDate(true) > getDate(false))
                {
                    System.Windows.MessageBox.Show("Please Input Valid Date Range");
                    startDatePicker.SelectedDate = null;
                    endDatePicker.SelectedDate = null;
                }
                else 
                    //if (startDatePicker.SelectedDate != null || endDatePicker.SelectedDate != null || nameTextBox.Text != "" || extensionText.Text != "")
                {
                    photos.Update();
                    //photos.Filter(originalPhotos, SearchFunctions.getExtensionArray(extensionText.Text));
                }
            }
        }
    }
}