using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace spamInfoAllWebsite
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            textBoxContent.Text = "Content";
            textBoxEmail.Text = "abc@gmail.com";
            textBoxNumberPhone.Text = "0123456789";
        }

        string[] urls;
        List<Thread> threads = new List<Thread>();
        List<string> urlAddHttp = new List<string>();
        private ConcurrentQueue<string> _queue;
        private ConcurrentDictionary<string, DataGridViewRow> _nameAndRow = new ConcurrentDictionary<string, DataGridViewRow>();


        public static string ReadHTMLCode(string URL)
        {
            try
            {
                WebClient webClient = new WebClient();
                byte[] reqHTML = webClient.DownloadData(URL);
                UTF8Encoding objUTF8 = new UTF8Encoding();
                return objUTF8.GetString(reqHTML);
            }
            catch
            {
                return null;
            }
        }

        private string GetUrl()
        {
            _queue.TryDequeue(out string url);
            return url;
        }

        public void Addrows(List<string> urlAddHttp)
        {
            for (int i = 0; i < urlAddHttp.Count; i++)
            {
                int myRowIndex = dGV.Rows.Add();
                DataGridViewRow row = dGV.Rows[myRowIndex];
                row.Cells["Column1"].Value = dGV.Rows.Count;
                row.Cells["Column2"].Value = urlAddHttp[i];
                if (!_nameAndRow.ContainsKey(urlAddHttp[i]))
                {
                    _nameAndRow.TryAdd(urlAddHttp[i], row);
                }
            }
        }

        private void btnLoadDomain_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                urls = File.ReadAllLines(openFileDialog.FileName.Trim());
                foreach (var item in urls)
                {
                    string x = "https://" + item;
                    urlAddHttp.Add(x);
                }
                _queue = new ConcurrentQueue<string>(urlAddHttp);

                Addrows(urlAddHttp);
                MessageBox.Show("Đã load xong", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            dGV.Rows.Clear();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (urls == null)
            {
                MessageBox.Show("Chưa load file domain ...", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            int thread_count = Convert.ToInt32(numericUpDownThread.Value);

            await Task.Run(() =>
            {
                while (_queue.IsEmpty == false)
                {
                    for (int i = 0; i < thread_count; i++)
                    {
                        Thread t = new Thread(() =>
                        {
                            Run();
                        });
                        t.Start();
                        threads.Add(t);
                        Thread.Sleep(500);
                    }
                    foreach (Thread t in threads)
                    {
                        t.Join();
                    }
                }
            });
        }

        public void Run()
        {
            string content = textBoxContent.Text;
            string numberPhone = textBoxNumberPhone.Text;
            string email = textBoxEmail.Text;

            int delayTime = Convert.ToInt32(numericUpDownTime.Value);

            MyChrome myChrome = new MyChrome();

            while (_queue.IsEmpty == false)
            {
                var url = GetUrl();
                if (url == null)
                {
                    //Hết domain, return
                    return;
                }
                else
                {
                    List<string> names = new List<string>();
                    //Tiếp tục xử lý
                    string html = ReadHTMLCode(url);
                    if (html != null)
                    {
                        //Nếu có nút submit
                        if (html.Contains("type=\"submit\"") || html.Contains("type=\"button\""))
                        {
                            string[] splitText = null;
                            // Tìm thẻ input
                            if (html.Contains("<input "))
                            {
                                splitText = Regex.Split(html, "<input");
                            }
                            if (splitText != null)
                            {
                                foreach (string text in splitText)
                                {
                                    bool test = !text.Contains("<!DOCTYPE") && !text.Contains("type=\"hidden\"") && !text.Contains("type=\"search\"") && !text.Contains("name=\"s\"")
                                        && !text.Contains("name=\"username\"") && !text.Contains("name=\"password\"") && !text.Contains("name=\"rememberme\"") && !text.Contains("name=\"Lines\"")
                                        && !text.Contains("name=\"q\"") && !text.Contains("name=\"quantity") && !text.Contains("name=\"viewport\"") && !text.Contains("name=\"keyword\"") && !text.Contains("name=\"query\"")
                                        && !text.Contains("name=\"addtocart\"") && !text.Contains("name=\"search\"") && !text.Contains("name=\"description\"") && !text.Contains("name=\"login_email\"") && !text.Contains("name=\"login_pass\"")
                                        && !text.Contains("name=\"login_button\"") && !text.Contains("name=\"forgot_button\"") && !text.Contains("name=\"login_email\"") && !text.Contains("name=\"key\"")
                                        && !text.Contains("name=\"login\"") && !text.Contains("name=\"gender\"") && !text.Contains("name=\"user_name\"") && !text.Contains("name=\"add-to-cart\"");
                                    //Nếu không chứa những cái name này thì ...
                                    if (test == true)
                                    {
                                        try
                                        {
                                            string name = Regex.Split(Regex.Split(text, "name=\"")[1], "\"")[0];
                                            // Lưu tất cả các classname không phải là những cái ở trên
                                            names.Add(name);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    
                                }
                            }

                        }
                    }
                }
            }
        }
    }
}
