using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Lab2
{
    public partial class Form1 : Form
    {
        private ListView leftPanelListView;
        private WebBrowser rightPanelBrowser;
        private TextBox rightPanelTextBox;
        private TextBox topTextBox;
        private string currentDirectory;

        private ImageList imageList;

        public Form1()
        {
            InitializeComponent();
            InitializeImageList();
            InitializeTopTextBox();
            InitializeLeftPanel();
            InitializeRightPanel();
        }

        private void InitializeTopTextBox()
        {
            topTextBox = new TextBox();
            topTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right; // Встановлюємо Anchor
            topTextBox.Width = this.Width;
            topTextBox.Multiline = true;
            topTextBox.ReadOnly = true;
            topTextBox.Height = 20;
            topTextBox.Text = Directory.GetCurrentDirectory();

            this.Controls.Add(topTextBox);
        }

        private void InitializeLeftPanel()
        {
            leftPanelListView = new ListView();
            leftPanelListView.Dock = DockStyle.Left;
            leftPanelListView.Width = this.Width / 2;
            leftPanelListView.View = View.Details;
            leftPanelListView.Columns.Add("Name");
            leftPanelListView.DoubleClick += leftPanelListView_DoubleClick;
            leftPanelListView.SmallImageList = imageList;
            leftPanelListView.Top = topTextBox.Bottom;

            PopulateLeftPanel(leftPanelListView, @"C:\Users\user\Desktop\DesktopTest");

            this.Controls.Add(leftPanelListView);
        }

        private void InitializeRightPanel()
        {
            int topMargin = topTextBox.Bottom;

            rightPanelBrowser = new WebBrowser();
            rightPanelBrowser.Dock = DockStyle.Right;
            rightPanelBrowser.Width = this.Width / 2;
            rightPanelBrowser.Top = topMargin;

            rightPanelTextBox = new TextBox();
            rightPanelTextBox.Dock = DockStyle.Right;
            rightPanelTextBox.Width = this.Width / 2;
            rightPanelTextBox.Multiline = true;
            rightPanelTextBox.ReadOnly = true;
            rightPanelTextBox.Top = topMargin + 20;

            this.Controls.Add(rightPanelBrowser);
            this.Controls.Add(rightPanelTextBox);
        }

        private void InitializeImageList()
        {
            imageList = new ImageList();
            imageList.Images.Add("dockIcon", Properties.Resources.dockIcon);
            imageList.Images.Add("folderIcon", Properties.Resources.folderIcon);
            imageList.Images.Add("htmlIcon", Properties.Resources.htmlIcon);
            imageList.Images.Add("pdfIcon", Properties.Resources.pdfIcon);
            imageList.Images.Add("txtIcon", Properties.Resources.txtIcon);
            imageList.Images.Add("upFolderIcon", Properties.Resources.upFolderIcon);
            imageList.Images.Add("xmlIcon", Properties.Resources.xmlIcon);
        }

        private void PopulateLeftPanel(ListView listView, string path)
        {
            leftPanelListView.Items.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            if (directoryInfo.Parent != null)
            {
                ListViewItem item = new ListViewItem("...");
                item.Tag = directoryInfo.Parent.FullName;
                item.ImageKey = "upFolderIcon";
                listView.Items.Add(item);

                PopulateTopTextView(path);
            }

            foreach (var directory in directoryInfo.GetDirectories())
            {
                ListViewItem item = new ListViewItem(directory.Name);
                item.Tag = directory.FullName;
                item.ImageKey = "folderIcon";
                listView.Items.Add(item);
            }

            foreach (var file in directoryInfo.GetFiles())
            {
                ListViewItem item = new ListViewItem(file.Name);
                item.Tag = file.FullName;
                if (file.Extension == ".html")
                {
                    item.ImageKey = "htmlIcon";
                }
                else if (file.Extension == ".docx")
                {
                    item.ImageKey = "dockIcon";
                } 
                else if (file.Extension == ".pdf")
                {
                    item.ImageKey = "pdfIcon";
                }
                else if (file.Extension == ".txt")
                {
                    item.ImageKey = "txtIcon";
                }
                else if (file.Extension == ".xml")
                {
                    item.ImageKey = "xmlIcon";
                }
                listView.Items.Add(item);
            }

            currentDirectory = path;

            // Очистити праву панель
            if(rightPanelBrowser != null) 
            {
                rightPanelBrowser.DocumentText = string.Empty;
            }
            if(rightPanelTextBox != null)
            {
                rightPanelTextBox.Text = string.Empty;
            }
        }

        private void PopulateTopTextView(string path)
        {
            if(Directory.Exists(path) || File.Exists(path))
            {
                topTextBox.Text = path;
            }
        }

        private void PopulateRightPanel(string path)
        {
            if (Directory.Exists(path))
            {
                rightPanelBrowser.Visible = false;
                rightPanelTextBox.Visible = true;

                // Обрана папка - відображаємо кількість файлів у ній
                int txtCount = CountFilesInDirectory(path, "*.txt");
                int pdfCount = CountFilesInDirectory(path, "*.pdf");
                int xmlCount = CountFilesInDirectory(path, "*.xml");
                int docxCount = CountFilesInDirectory(path, "*.docx");

                rightPanelTextBox.Text = Environment.NewLine + 
                                         $"Кількість TXT файлів: {txtCount}" + Environment.NewLine +
                                         $"Кількість PDF файлів: {pdfCount}" + Environment.NewLine +
                                         $"Кількість XML файлів: {xmlCount}" + Environment.NewLine +
                                         $"Кількість DOCX файлів: {docxCount}";
            }

            string selectedItem = leftPanelListView.SelectedItems.Count > 0 ? leftPanelListView.SelectedItems[0].Text : null;

            if (selectedItem != null)
            {
                string selectedPath = Path.Combine(path, selectedItem);

                if(File.Exists(selectedPath))
                {
                    // Обраний файл - перевіряємо його тип
                    string fileExtention = Path.GetExtension(selectedPath).ToLower();

                    if(fileExtention == ".html")
                    {
                        // HTML файл - відображаємо його вміст у WebBrowser
                        rightPanelTextBox.Visible = false;
                        rightPanelBrowser.Visible = true;
                        rightPanelBrowser.Navigate(selectedPath);
                    }
                    else
                    {
                        // Інші файли - відображаємо назву та дату створення
                        rightPanelBrowser.Visible = false;
                        rightPanelTextBox.Visible = true;
                        string fileName = Path.GetFileName(selectedPath);
                        DateTime creationDate = File.GetCreationTime(selectedPath);
                        string fileInfo = Environment.NewLine + $"Iм'я файлу: {fileName}" + Environment.NewLine + $"Дата створення: {creationDate}";
                        rightPanelTextBox.Text = fileInfo;
                    }
                }
            }
        }

        private int CountFilesInDirectory(string directoryPath, string searchPattern)
        {
            return Directory.GetFiles(directoryPath, searchPattern).Length;
        }

        private void leftPanelListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateRightPanel(Directory.GetCurrentDirectory());
        }

        private void leftPanelListView_DoubleClick(object sender, EventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItems.Count > 0)
            {
                string path = (string)listView.SelectedItems[0].Tag;
                if (Directory.Exists(path))
                {
                    PopulateLeftPanel(leftPanelListView, path);
                    PopulateRightPanel(path);
                }
                else if (Path.GetExtension(path).ToLower() == ".html")
                {
                    PopulateRightPanel(Path.GetDirectoryName(path));
                }
                else
                {
                    PopulateRightPanel(Path.GetDirectoryName(path));
                }
            }
        }
    }
}
