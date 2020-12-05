using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;
using System.ServiceModel.Syndication;
using System.Web;
using System.Text.RegularExpressions;

namespace SearchDoi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Form1(string title)
        {
            InitializeComponent();
            this.textBox1.Text = title;
        }

        public List<Item> articles = new List<Item>();

        private void button1_ClickAsync(object sender, EventArgs e)
        {
            if (textBox1.Text=="")
            {
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            this.Text = "Searching...";
            switchControls(this.panel1.Controls, false);

            var url = "https://api.crossref.org/works?query=" + textBox1.Text + "&select=DOI,title&mailto=soufsilence@163.com";
            string jsonString = DownloadFormUrl(url);

            this.Text = "SearchDoI";
            this.Cursor = Cursors.Default;
            switchControls(this.panel1.Controls, true);

            var jsondata = JsonConvert.DeserializeObject<RootObject>(jsonString);

            articles.Clear();
            foreach (var item in jsondata.message.items)
            {
                articles.Add(item);
            }
            SetTextbox(articles);
        }

        public string DownloadFormUrl(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            Stream httpStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(httpStream);
            return sr.ReadToEnd();
        }

        public void switchControls(Control.ControlCollection controlCollection,bool isEnable)
        {
            foreach (var control in controlCollection)
            {
                if (control.GetType().Name == "TextBox")
                {
                    TextBox tb = (TextBox)control;
                    tb.Enabled = isEnable;
                }
                else
                {
                    Button bt = (Button)control;
                    bt.Enabled = isEnable;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (articles.Count >= 3)
            {
                Clipboard.SetText(articles[2].DOI);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (articles.Count>=1)
            {
                Clipboard.SetText(articles[0].DOI);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (articles.Count >= 2)
            {
                Clipboard.SetText(articles[1].DOI);
            }
        }

        private void button5_ClickAsync(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            this.Text = "Searching...";
            switchControls(this.panel1.Controls, false);

            //HttpUtility.UrlEncode(HttpUtility.UrlDecode(textBox1.Text))
            var url = "http://export.arxiv.org/api/query?search_query=ti:" + Regex.Replace(textBox1.Text,"[^a-zA-Z0-9 ]","") + "&start=0&max_results=3";

            XmlReader reader = XmlReader.Create(url);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();

            this.Text = "SearchDoI";
            this.Cursor = Cursors.Default;
            switchControls(this.panel1.Controls, true);

            articles.Clear();

            foreach (var item in feed.Items)
            {
                string doi = "";
                foreach (var link in item.Links)
                {
                    if (link.Title=="doi")
                    {
                        doi = link.Uri.AbsolutePath.TrimStart('/');
                        break;
                    }
                }
                if (doi!="")
                {
                    doi = "DoI:" + doi + " arXivID:" + item.Id.Substring(21);
                }
                else
                {
                    doi= item.Id.Substring(21);
                }
                articles.Add(new Item(doi, item.Title.Text));
            }

            SetTextbox(articles);
        }

        public void SetTextbox(List<Item> items)
        {
            switch (items.Count)
            {
                case 0:
                    textBox2.Text = "Not found!!!";
                    textBox3.Text = "Not found!!!";
                    textBox4.Text = "Not found!!!";
                    break;
                case 1:
                    textBox2.Text = articles[0].title[0];
                    textBox3.Text = "Not found!!!";
                    textBox4.Text = "Not found!!!";
                    break;
                case 2:
                    textBox2.Text = articles[0].title[0];
                    textBox3.Text = articles[1].title[0];
                    textBox4.Text = "Not found!!!";
                    break;
                default:
                    textBox2.Text = articles[0].title[0];
                    textBox3.Text = articles[1].title[0];
                    textBox4.Text = articles[2].title[0];
                    break;
            }
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class RootObject
    {
        public string status { get; set; }
        public string message_type { get; set; }
        public string message_version { get; set; }
        public MessageObject message { get; set; }
    }

    public class MessageObject
    {
        public Facets facets { get; set; }
        public int total_results { get; set; }
        public List<Item> items { get; set; }
    }

    public class Facets { }

    public class Item
    {
        public string DOI { get; set; }
        public List<string> title { get; set; }

        public Item(string doi, string ti)
        {
            DOI = doi;
            List<string> a = new List<string>();
            a.Add(ti);
            title = a;
        }
    }
}
