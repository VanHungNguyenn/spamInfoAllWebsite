using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace spamInfoAllWebsite
{
    class MyChrome
    {
        public IWebDriver driver;

        public const string CLICK = "CLICK";
        public const string SEND_KEYS = "SEND_KEYS";
        public const string CLEAR = "CLEAR";
        public const string GET_TEXT = "GET_TEXT";
        public const string GET_INNER_HTML = "GET_INNER_HTML";
        public const string GET_OUTER_HTML = "GET_OUTER_HTML";
        public const string SWITCH_FRAME = "SWITCH_FRAME";
        public const string SWITCH_DEFAULT = "SWITCH_DEFAULT";

        public string ElementAction(string action, string xpath = "", int index = 0, string text = "", int waits = 3, int delay = 0, bool is_exception = true)
        {
            if (action.Equals(SWITCH_FRAME))
            {
                driver.SwitchTo().Frame(driver.FindElements(By.XPath(xpath))[index]);
                Sleep(delay);
                return "";
            }
            int wait = 0;
            while (driver.FindElements(By.XPath(xpath)).Count == 0 && wait < waits)
            {
                Sleep(1);
                wait++;
            }
            if (wait == waits)
            {
                if (is_exception)
                {
                    throw new Exception(string.Format("Xpath not found: {0}", xpath));
                }
                else
                {
                    return "";
                }
            }
            if (index < 0)
            {
                index += driver.FindElements(By.XPath(xpath)).Count;
            }
            switch (action)
            {
                case CLICK:
                    try
                    {
                        driver.FindElements(By.XPath(xpath))[index].Click();
                    }
                    catch
                    {
                        IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                        executor.ExecuteScript("arguments[0].click();", driver.FindElements(By.XPath(xpath))[index]);
                    }
                    Sleep(delay);
                    return "";
                case CLEAR:
                    driver.FindElements(By.XPath(xpath))[index].Clear();
                    Sleep(delay);
                    return "";
                case SEND_KEYS:
                    driver.FindElements(By.XPath(xpath))[index].SendKeys(text);
                    Sleep(delay);
                    return "";
                case GET_TEXT:
                    string t = driver.FindElements(By.XPath(xpath))[index].Text;
                    Console.WriteLine("t: " + t);
                    Sleep(delay);
                    return t;
                case GET_INNER_HTML:
                    string innerHTML = driver.FindElements(By.XPath(xpath))[index].GetAttribute("innerHTML");
                    Sleep(delay);
                    return innerHTML;
                case GET_OUTER_HTML:
                    string outerHTML = driver.FindElements(By.XPath(xpath))[index].GetAttribute("outerHTML");
                    Sleep(delay);
                    return outerHTML;
                case SWITCH_FRAME:
                    driver.SwitchTo().Frame(driver.FindElements(By.XPath(xpath))[index]);
                    Sleep(delay);
                    return "";
                case SWITCH_DEFAULT:
                    driver.SwitchTo().DefaultContent();
                    Sleep(delay);
                    return "";
                default: return "";
            }
        }

        private void Sleep(double seconds)
        {
            Thread.Sleep(Convert.ToInt32(seconds * 1000));
        }

        public MyChrome()
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disable-notifications");
            options.AddArgument("--window-size=1300,1000");
            options.AddArguments("--disable-extensions");
            //options.AddArgument("--window-position=-32000,-32000");
            //options.AddArguments("--proxy-server=");
            driver = new ChromeDriver(service, options);
        }

        public void Quit()
        {
            Sleep(1);
            driver.Quit();
        }

        public void Run(MyChrome myChrome, string url, List<string> ten, List<string> sodienthoai, List<string> thudientu, List<string> noidung, List<string> nameTag, string content, string numberPhone, string email, int delay)
        {
            //string name = "Name";

            url = "https://" + url;
            bool check = false;

            driver.Navigate().GoToUrl(url);
            Sleep(2);

            try
            {
                if (sodienthoai.Count > 0)
                {
                    for (int i = 0; i < sodienthoai.Count; i++)
                    {
                        try
                        {
                            ElementAction(SEND_KEYS, $"//input[@name='{sodienthoai[i]}']", i, text: numberPhone);
                            Sleep(1);
                            check = true;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch { }
            try
            {
                if (thudientu.Count > 0)
                {
                    for (int i = 0; i < thudientu.Count; i++)
                    {
                        try
                        {
                            ElementAction(SEND_KEYS, $"//input[@name='{thudientu[i]}']", i, text: email);
                            Sleep(delay);
                        }
                        catch
                        {
                        }
                    }

                }
            }
            catch { }
            try
            {
                if (ten.Count > 0)
                {
                    for (int i = 0; i < ten.Count; i++)
                    {
                        try
                        {
                            if (check)
                            {
                                ElementAction(SEND_KEYS, $"//input[@name='{ten[i]}']", i, text: content);
                                Sleep(delay);
                            }
                            else
                            {
                                ElementAction(SEND_KEYS, $"//input[@name='{ten[i]}']", i, text: numberPhone);
                                Sleep(delay);
                            }

                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch { }

            try
            {
                if (noidung.Count > 0)
                {
                    for (int i = 0; i < noidung.Count; i++)
                    {
                        try
                        {
                            ElementAction(SEND_KEYS, $"//input[@name='{noidung[i]}']", i, text: content);
                            Sleep(delay);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch { }

            try
            {
                int textareaCount = driver.FindElements(By.XPath("//textarea")).Count;
                if (textareaCount > 0)
                {
                    for (int i = 0; i < textareaCount; i++)
                    {
                        try
                        {
                            ElementAction(SEND_KEYS, "//textarea", i, content);
                            Sleep(delay);
                        }
                        catch
                        {
                        }
                    }

                }
            }
            catch
            { }

            try
            {
                foreach (string item in nameTag)
                {
                    ElementAction(SEND_KEYS, $"//input[@name='{item}']", text: content);
                    Sleep(delay);
                }
            }
            catch
            {
            }

            int buttonTypeSubmit = driver.FindElements(By.XPath("//input[@type='submit']")).Count;
            int buttonTypeButton = driver.FindElements(By.XPath("//input[@type='button']")).Count;
            int buttonDivSubmit = driver.FindElements(By.XPath("//div[@data-action='submit']")).Count;
            if (buttonTypeSubmit > 0)
            {
                try
                {
                    for (int i = 0; i < buttonTypeSubmit; i++)
                    {
                        ElementAction(CLICK, $"//input[@type='submit']", i);
                        Sleep(delay);
                    }
                }
                catch
                {
                }
            }
            else if (buttonTypeButton > 0)
            {
                try
                {
                    for (int i = 0; i < buttonTypeButton; i++)
                    {
                        ElementAction(CLICK, $"//input[@type='button']", i);
                        Sleep(delay);
                    }
                }
                catch
                {
                }
            }
            else if (buttonDivSubmit > 0)
            {
                try
                {
                    for (int i = 0; i < buttonDivSubmit; i++)
                    {
                        ElementAction(CLICK, "//div[@data-action='submit']", i);
                        Sleep(delay);
                    }
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    int buttonCount = driver.FindElements(By.XPath("//button")).Count;
                    if (buttonCount > 0)
                    {
                        for (int i = 0; i < buttonCount; i++)
                        {
                            ElementAction(CLICK, "//button", i);
                        }

                    }
                }
                catch
                {
                }
            }
            Sleep(5);
        }
    }
}
