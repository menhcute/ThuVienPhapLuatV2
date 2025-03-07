﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

class Program
{
    static void Main()
    {
        ChromeOptions options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        IWebDriver driver = new ChromeDriver(options);

        driver.Navigate().GoToUrl("https://thuvienphapluat.vn/page/tim-van-ban.aspx?keyword=&type=3&match=True&area=0");
        Thread.Sleep(100);

        int count = int.Parse(driver.FindElement(By.Id("lbTotal")).Text);
        int countTxt = Directory.GetFiles("C:\\My_Project\\OJT_SAVE_WORK\\OJT_2\\Storage", "*.txt").Length;
        int countCurrent = count - countTxt;
        int pageCurrent = (int)Math.Ceiling((double)countCurrent / 20);

        driver.Navigate().GoToUrl($"https://thuvienphapluat.vn/page/tim-van-ban.aspx?keyword=&area=0&match=True&type=3&status=0&signer=0&sort=1&lan=1&scan=0&org=0&fields=&page={pageCurrent}");
        Thread.Sleep(100);

        HashSet<string> uniqueHrefs = new HashSet<string>();

        while (countCurrent > 0)
        {
            var contentDivs = driver.FindElements(By.CssSelector("div[class^='content-']"))
                                   .Where(div => div.FindElements(By.CssSelector("div.number"))
                                                   .Any(n => n.Text.Trim() == countCurrent.ToString()));

            foreach (var div in contentDivs)
            {
                try
                {
                    var linkElement = div.FindElement(By.CssSelector("a[onclick='Doc_CT(MemberGA)']"));
                    string href = linkElement.GetAttribute("href");
                    string fileName = Regex.Replace(linkElement.Text.Trim().Replace("–", "-"), "[\\\\/:*?\"<>|]", "_");
                    fileName = fileName.Length > 150 ? fileName.Substring(0, 150) : fileName;

                    if (!string.IsNullOrEmpty(href) && uniqueHrefs.Add(href))
                    {
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("window.open(arguments[0]);", href);
                        Thread.Sleep(100);

                        var tabs = driver.WindowHandles;
                        driver.SwitchTo().Window(tabs.Last());

                        var contentDiv = driver.FindElement(By.CssSelector(".content1"));
                        List<string> contentTexts = new List<string> { "Href: " + href };
                        var elements = contentDiv.FindElements(By.XPath(".//*"));
                        HashSet<string> paragraphsInTables = new HashSet<string>();

                        foreach (var element in elements)
                        {
                            if (element.TagName == "table")
                            {
                                var tableParagraphs = element.FindElements(By.XPath(".//p"));
                                foreach (var p in tableParagraphs)
                                {
                                    paragraphsInTables.Add(p.Text.Trim());
                                }
                            }
                        }

                        foreach (var element in elements)
                        {
                            if (element.TagName == "table")
                            {
                                string width = element.GetAttribute("width");
                                List<string> tableTexts = new List<string>();
                                var rows = element.FindElements(By.TagName("tr"));
                                bool shouldSkipTable = false;
                                HashSet<IWebElement> tdsToRemove = new HashSet<IWebElement>();

                                foreach (var row in rows)
                                {
                                    var cells = row.FindElements(By.TagName("td"));
                                    foreach (var cell in cells)
                                    {
                                        string cellText = cell.Text.Trim().ToLower();
                                        if (cellText.Contains("kính gửi"))
                                        {
                                            shouldSkipTable = true;
                                            break;
                                        }
                                        if (cellText.Contains("nơi nhận"))
                                        {
                                            tdsToRemove.Add(cell);
                                        }
                                    }
                                    if (shouldSkipTable) break;
                                }

                                if (!shouldSkipTable)
                                {
                                    foreach (var row in rows)
                                    {
                                        var cells = row.FindElements(By.TagName("td"));
                                        List<string> rowTexts = new List<string>();

                                        foreach (var cell in cells)
                                        {
                                            if (!tdsToRemove.Contains(cell))
                                            {
                                                rowTexts.Add(cell.Text.Trim());
                                            }
                                        }

                                        if (width == "100%" && rows.Count > 1)
                                        {
                                            tableTexts.Add(string.Join(" || ", rowTexts));
                                        }
                                        else
                                        {
                                            tableTexts.AddRange(rowTexts);
                                        }
                                    }
                                    contentTexts.AddRange(tableTexts);
                                }
                            }
                            else if (element.TagName == "p" && !paragraphsInTables.Contains(element.Text.Trim()))
                            {
                                contentTexts.Add(element.Text.Trim());
                            }
                        }

                        string filePath = $"C:\\My_Project\\OJT_SAVE_WORK\\OJT_2\\Storage\\{fileName}.txt";
                        File.WriteAllLines(filePath, contentTexts);

                        driver.Close();
                        driver.SwitchTo().Window(tabs.First());
                        countCurrent--;
                    }
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Không tìm thấy liên kết phù hợp trong phần tử này.");
                }
            }

            if (countCurrent > 0 && countCurrent % 20 == 0)
            {
                try
                {
                    var prevPageButton = driver.FindElements(By.CssSelector("a[rel='nofollow']"))
                                              .FirstOrDefault(a => a.Text.Trim() == "Trang trước");
                    prevPageButton?.Click();
                    Thread.Sleep(100);
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Không tìm thấy nút chuyển trang.");
                    break;
                }
            }
        }
        driver.Quit();
    }
}
