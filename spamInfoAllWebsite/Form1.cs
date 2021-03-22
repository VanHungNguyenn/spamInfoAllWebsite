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
            Control.CheckForIllegalCrossThreadCalls = false;
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

        public void Run()
        {
            int delayTime = Convert.ToInt32(numericUpDownTime.Value);
            string content = textBoxContent.Text;
            string numberPhone = textBoxNumberPhone.Text;
            string email = textBoxEmail.Text;

            bool hideBrowers = checkBoxHide.Checked;
            bool hideImages = checkBoxImage.Checked;


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
                    this.Invoke(new MethodInvoker(() =>
                    {
                        countTotalWeb++;
                        labelTotalForm.Text = Convert.ToString(countTotalWeb);
                    }));

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

                                Dictionary<string, int> ten = new Dictionary<string, int>();

                                Dictionary<string, int> sodienthoai = new Dictionary<string, int>();
                                Dictionary<string, int> thudientu = new Dictionary<string, int>();
                                Dictionary<string, int> noidung = new Dictionary<string, int>();
                                int count = 0;
                                Dictionary<string, int> nameTag = new Dictionary<string, int>();
                                // Kiếm xem có bao nhiêu trường input để điền.

                                foreach (string name in names)
                                {
                                    string nameLower = name.ToLower();
                                    if (nameLower.Contains("thoai") || nameLower.Contains("phone") || nameLower.Contains("sdt") || nameLower.Contains("tel"))
                                    {
                                        if (!sodienthoai.ContainsKey(name))
                                        {
                                            sodienthoai.Add(name, 1);
                                        }
                                        else
                                        {
                                            sodienthoai[name]++;
                                        }
                                        count++;
                                    }
                                    else if (nameLower.Contains("ten") || nameLower.Contains("name"))
                                    {
                                        if (!ten.ContainsKey(name))
                                        {
                                            ten.Add(name, 1);
                                        }
                                        else
                                        {
                                            ten[name]++;
                                        }
                                        count++;
                                    }
                                    else if (nameLower.Contains("mail"))
                                    {
                                        if (!thudientu.ContainsKey(name))
                                        {
                                            thudientu.Add(name, 1);
                                        }
                                        else
                                        {
                                            thudientu[name]++;
                                        }
                                        count++;
                                    }
                                    else if (nameLower.Contains("message"))
                                    {
                                        if (!noidung.ContainsKey(name))
                                        {
                                            noidung.Add(name, 1);
                                        }
                                        else
                                        {
                                            noidung[name]++;
                                        }
                                        count++;
                                    }
                                    else
                                    {
                                        if (!nameTag.ContainsKey(name))
                                        {
                                            nameTag.Add(name, 1);
                                        }
                                        else
                                        {
                                            nameTag[name]++;
                                        }
                                    }
                                }
                                // Có lớn hơn 2 ko?
                                if (count > 1)
                                {
                                    MyChrome myChrome = new MyChrome(hideBrowers, hideImages);
                                    myChrome.Run(myChrome, url, ten, sodienthoai, thudientu, noidung, nameTag, content, numberPhone, email, delayTime);

                                    myChrome.Quit();

                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        countForm++;
                                        labelForm.Text = Convert.ToString(countForm);
                                    }));
                                    //countForm++;
                                    //labelForm.Text = Convert.ToString(countForm);
                                    if (_nameAndRow.ContainsKey(url))
                                    {
                                        var row = _nameAndRow[url];
                                        this.Invoke(new MethodInvoker(() =>
                                        {
                                            row.Cells["Column3"].Value = "Đã tương tác";
                                            row.Cells["Column3"].Style.BackColor = Color.Green;
                                        }));
                                    }
                                }
                                else
                                {
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        countNoForm++;
                                        labelNoForm.Text = Convert.ToString(countNoForm);
                                    }));
                                    //      countNoForm++;
                                    //      labelNoForm.Text = Convert.ToString(countNoForm);
                                    if (_nameAndRow.ContainsKey(url))
                                    {
                                        var row = _nameAndRow[url];
                                        this.Invoke(new MethodInvoker(() =>
                                        {
                                            row.Cells["Column3"].Value = "Không có form";
                                            row.Cells["Column3"].Style.BackColor = Color.Pink;
                                        }));
                                    }
                                }
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    countNoForm++;
                                    labelNoForm.Text = Convert.ToString(countNoForm);
                                }));
                                if (_nameAndRow.ContainsKey(url))
                                {
                                    var row = _nameAndRow[url];
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        row.Cells["Column3"].Value = "Không có form";
                                        row.Cells["Column3"].Style.BackColor = Color.Pink;
                                    }));
                                }
                            }

                        }
                        else
                        {
                            this.Invoke(new MethodInvoker(() =>
                            {
                                countNoForm++;
                                labelNoForm.Text = Convert.ToString(countNoForm);
                            }));
                            //countNoForm++;
                            //labelNoForm.Text = Convert.ToString(countNoForm);
                            if (_nameAndRow.ContainsKey(url))
                            {
                                var row = _nameAndRow[url];
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    row.Cells["Column3"].Value = "Không có form";
                                    row.Cells["Column3"].Style.BackColor = Color.Pink;
                                }));
                            }
                        }
                    }
                    else
                    {
                        if (_nameAndRow.ContainsKey(url))
                        {
                            this.Invoke(new MethodInvoker(() =>
                            {
                                countNoWeb++;
                                labelNoWeb.Text = Convert.ToString(countNoWeb);
                            }));
                            //countNoWeb++;
                            //labelNoWeb.Text = Convert.ToString(countNoWeb);
                            var row = _nameAndRow[url];
                            this.Invoke(new MethodInvoker(() =>
                            {
                                row.Cells["Column3"].Value = "Web không tồn tại";
                                row.Cells["Column3"].Style.BackColor = Color.Pink;
                            }));
                        }
                    }
                }
            }

            Thread.Sleep(5000);
        }

        private int countTotalWeb, countNoForm, countForm, countNoWeb;

        private async void btnStart_Click_1(object sender, EventArgs e)
        {
            if (urls == null)
            {
                MessageBox.Show("Chưa load file domain ...", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (textBoxNumberPhone.Text == "" || textBoxEmail.Text == "" || textBoxContent.Text == "")
            {
                MessageBox.Show("Chưa điền đủ thông tin ...", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int thread_count = Convert.ToInt32(numericUpDownThread.Value);

            countTotalWeb = 0;
            countNoForm = 0;
            countForm = 0;
            countNoWeb = 0;

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

            MessageBox.Show("Đã tương tác xong...", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            dGV.Rows.Clear();

            urlAddHttp.Clear();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {

        }

        private void btnLoadDomain_Click_1(object sender, EventArgs e)
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

        private void btnStop_Click(object sender, EventArgs e)
        {
            int index_queue = _queue.Count;
            for (int i = 0; i < index_queue; i++)
            {
                _queue.TryDequeue(out string url);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var t in threads)
            {
                t.Abort();
            }
        }
    }
}
