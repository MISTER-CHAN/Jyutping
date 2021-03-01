using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Jyutping
{

    [Activity(Label = "査字音", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private Button bQuery;
        private EditText etText;
        private LinearLayout llResult;
        private Spinner sSchema, sUrl;

        private void BQuery_Click(object sender, EventArgs e)
        {
            if (etText.Text.Length <= 0)
                return;
            bQuery.Enabled = false;
            bQuery.Text = "加載中";
            llResult.RemoveAllViews();
            new Thread(new ParameterizedThreadStart(LoadPronunciation)).Start(new object[]
            {
                etText.Text,
                sUrl.SelectedItem.ToString(),
                sSchema.SelectedItem.ToString(),
            });
        }

        private void BToIpa(object sender, EventArgs e)
        {
            etText.Text = JyutpingToIpa(etText.Text);
        }

        public string GetHTML(string url)
        {
            return GetHTML(url, "UTF-8");
        }
        public string GetHTML(string url, string encodingName)
        {
            WebClient wc = new WebClient
            {
                Encoding = Encoding.GetEncoding(encodingName)
            };
            try
            {
                return wc.DownloadString(url);
            }
            catch (Exception)
            {
            }
            return "";
        }

        string JyutpingToIpa(string s)
        {
            s = Regex.Replace(s, "(^|[^a-z])(a|e|o|uk|ung)", "$1ʔ$2");
            s = Regex.Replace(s, "eoi", "ɵy̯");
            s = Regex.Replace(s, "eo", "ɵ");
            s = Regex.Replace(s, "oe", "œː");
            s = Regex.Replace(s, "([aeou]i|[aeio]u)", "$1̯");
            s = Regex.Replace(s, "yu", "yː");
            s = Regex.Replace(s, "i([umpnt]?\\d)", "iː$1");
            s = Regex.Replace(s, "u([int]?\\d)", "uː$1");
            s = Regex.Replace(s, "([^a])a([^a])", "$1ɐ$2");
            s = Regex.Replace(s, "aa", "aː");
            s = Regex.Replace(s, "e((u|m|ng|k)|\\d)", "ɛː$1");
            s = Regex.Replace(s, "o((i|m|n|ng|k)|\\d)", "ɔː$1");
            s = Regex.Replace(s, "i(ng|k)", "e$1");
            s = Regex.Replace(s, "u(ng|k)", "o$1");
            s = Regex.Replace(s, "([gk])w", "$1ʷ");
            s = Regex.Replace(s, "(^[ptk])", "$1ʰ");
            s = Regex.Replace(s, "ʰʷ", "ʷʰ");
            s = Regex.Replace(s, "c", "t͡sʰ");
            s = Regex.Replace(s, "([ptk])(\\d)", "$1̚$2");
            s = Regex.Replace(s, "b", "p");
            s = Regex.Replace(s, "d", "t");
            s = Regex.Replace(s, "z", "t͡s");
            s = Regex.Replace(s, "ng", "ŋ");
            s = Regex.Replace(s, "g", "k");
            s = Regex.Replace(s, "([ptk]̚)1", "$1˥");
            s = Regex.Replace(s, "1", "˥˧");
            s = Regex.Replace(s, "2", "˧˥");
            s = Regex.Replace(s, "([ptk]̚)3", "$1˧");
            s = Regex.Replace(s, "3", "˧˧");
            s = Regex.Replace(s, "4", "˨˩");
            s = Regex.Replace(s, "5", "˦˥");
            s = Regex.Replace(s, "([ptk]̚)6", "$1˨");
            s = Regex.Replace(s, "6", "˨˨");
            return "[" + s + "]";
        }

        string JyutpingToLatina(string s)
        {
            s = Regex.Replace(s, "yu", "y");
            s = Regex.Replace(s, "eoi", "eoy");
            s = Regex.Replace(s, "^([aeiouy])", "'$1");
            s = Regex.Replace(s, "^t", "t‘");
            s = Regex.Replace(s, "^d", "t");
            s = Regex.Replace(s, "^p", "p‘");
            s = Regex.Replace(s, "^k", "k‘");
            s = Regex.Replace(s, "^g", "k");
            s = Regex.Replace(s, "^c", "ts‘");
            s = Regex.Replace(s, "^z", "ts");
            s = Regex.Replace(s, "^b", "p");
            return s;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            bQuery = FindViewById<Button>(Resource.Id.b_query);
            etText = FindViewById<EditText>(Resource.Id.et_text);
            llResult = FindViewById<LinearLayout>(Resource.Id.ll_result);
            sUrl = FindViewById<Spinner>(Resource.Id.s_url);
            sSchema = FindViewById<Spinner>(Resource.Id.s_schema);

            bQuery.Click += BQuery_Click;
            
            sUrl.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, new string[]
            {
                 "cuhk.edu.hk", "shyyp.net"
            });
            sSchema.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, new string[]
            {
                 "粵拼", "Latina", "IPA"
            });

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        [Obsolete]
        private void LoadPronunciation(object o)
        {
            object[] os = (object[])o;
            LoadPronunciation(os[0], os[1], os[2]);
        }

        [Obsolete]
        private void LoadPronunciation(object oString, object oUrl, object oSchema)
        {
            string html = "", s = oString.ToString(), url = oUrl.ToString(), schema = oSchema.ToString();
            if (s.Length == 1)
            {
                string[] explains = null, prons = null;
                switch (url)
                {
                    case "cuhk.edu.hk":
                        string c = string.Join("", Encoding.GetEncoding("big5").GetBytes(s).Cast<byte>().Select(b => "%" + b.ToString("X2")));
                        html = GetHTML("https://humanum.arts.cuhk.edu.hk/Lexis/lexi-can/search.php?q=" + c, "big5");
                        prons = Regex.Matches(html, "(?<=\"sound\\.php\\?s=)[a-z1-6]+(?=\")").Cast<Match>().Select(m => m.Value).ToArray();
                        explains = Regex.Matches(html, "(?<=<td><div nowrap>).+?(?=(</div>)?</td>)").Cast<Match>().Select(m => Regex.Replace(Regex.Replace(m.Value, "<.+?>", ""), "\\[\\d+\\.\\.\\]", ", ")).ToArray();
                        break;
                    case "shyyp.net":
                        html = GetHTML("https://shyyp.net/search?q=" + s);
                        prons = Regex.Matches(html, "(?<=<span class=\"PSX  text-xl pl-2 pr-1 py-2 PS_jyutping \">)[a-z1-6]+(?=</span>)").Cast<Match>().Select(m => m.Value).ToArray();
                        explains = Regex.Matches(html, "(?<=<ul class=\"my-2\"><li>).+?(?=</li></ul>)|(?<=\\\"result\\\":{).+?(?=})").Cast<Match>().Select(m => Regex.Replace(Regex.Replace(m.Value, "\"[0-9a-z_]+\":\"?|[01],|null,?|\" \",|\",", ""), "^\"|\"$", "")).ToArray();
                        break;
                }
                if (html == "")
                {
                    prons = new string[] { "無法加載網頁" };
                    explains = new string[] { "" };
                }
                else if (prons.Length <= 0)
                {
                    prons = new string[] { "無結果" };
                    explains = new string[] { "" };
                }
                TextView tvExplain, tvPron;
                RunOnUiThread(() =>
                {
                    switch (schema)
                    {
                        case "粵拼":
                            prons = prons.Cast<string>().Select(s => s + " " + JyutpingToIpa(s)).ToArray();
                            break;
                        case "Latina":
                            prons = prons.Cast<string>().Select(s => JyutpingToLatina(s) + " " + JyutpingToIpa(s)).ToArray();
                            break;
                        case "IPA":
                            prons = prons.Cast<string>().Select(s => JyutpingToIpa(s)).ToArray();
                            break;
                    }
                    for (int i = 0; i < prons.Length; i++)
                    {
                        tvPron = new TextView(this) { Text = "\n" + prons[i] };
                        tvPron.SetTextAppearance(this, Resource.Style.TextAppearance_AppCompat_Large);
                        llResult.AddView(tvPron);
                        tvExplain = new TextView(this) { Text = explains[i] };
                        llResult.AddView(tvExplain);
                    }
                });
            }
            else if (s.Length > 1)
            {
                List<string> pages = new List<string>() { "" };
                int column = 0, character = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    column++;
                    char c = s[i];
                    pages[^1] += c;
                    if ('\u4e00' <= c && c < '\ua000')
                    {
                        character++;
                        if (url == "www.yueyv.cn" && character == 15 ||
                            url == "shyyp.net" && character == 129 ||
                            url == "cuhk.edu.hk" && character == 1)
                        {
                            pages.Add("");
                            character = 0;
                        }
                    }
                    else if (c == '\n')
                    {
                        column = 0;
                    }
                    if (column == 10)
                    {
                        pages[^1] += '\n';
                        column = 0;
                    }
                }
                LinearLayout llLine = null;
                RunOnUiThread(() =>
                {
                    llLine = new LinearLayout(this)
                    {
                        Orientation = Orientation.Horizontal
                    };
                    llLine.SetGravity(GravityFlags.Bottom);
                });
                for (int p = 0; p < pages.Count; p++)
                {
                    string page = pages[p];
                    RunOnUiThread(() =>
                    {
                        bQuery.Text = "加載中...(" + p + "/" + pages.Count + ")";
                    });
                    Array[] prons = null;
                    if (page != "\n")
                    {
                        switch (url)
                        {
                            case "shyyp.net":
                                html = Regex.Match(GetHTML("https://shyyp.net/search?q=" + page), "<div class=\"ml-2 mr-2 mb-0  py-1 bg-green-100\">.+?(?=<div class=\"ml-2 mr-2 mb-0  bg-gray-100 \">)").Value;
                                string[] chars = Regex.Matches(html, "<div class=\"ml-2 mr-2 mb-0  py-1 bg-[a-z]+?-100\">.+?</div>(?=(<div class=\"ml-2 mr-2 mb-0  py-1 bg-[a-z]+?-100\">)|$)").Cast<Match>().Select(m => m.Value).ToArray();
                                prons = new Array[chars.Length];
                                for (character = 0; character < chars.Length; character++)
                                {
                                    prons[character] = Regex.Matches(chars[character], "(?<=<span class=\"PSX  text-xl pl-2 pr-1 py-2 PS_jyutping \">)[a-z1-6]+(?=</span>)").Cast<Match>().Select(m => m.Value).ToArray();
                                }
                                break;
                            case "cuhk.edu.hk":
                                string c = string.Join("", Encoding.GetEncoding("big5").GetBytes(page).Cast<byte>().Select(b => "%" + b.ToString("X2")));
                                html = GetHTML("https://humanum.arts.cuhk.edu.hk/Lexis/lexi-can/search.php?q=" + c, "big5");
                                prons = new Array[1];
                                prons[0] = Regex.Matches(html, "(?<=\"sound\\.php\\?s=)[a-z1-6]+(?=\")").Cast<Match>().Select(m => m.Value).ToArray();
                                break;
                        }
                    }
                    RunOnUiThread(() =>
                    {
                        TextView text;
                        character = 0;
                        for (int i = 0; i < page.Length; i++)
                        {
                            char c = page[i];
                            if (c == '\n')
                            {
                                llResult.AddView(llLine);
                                llLine = new LinearLayout(this)
                                {
                                    Orientation = Orientation.Horizontal
                                };
                                llLine.SetGravity(GravityFlags.Bottom);
                            }
                            LinearLayout layout = new LinearLayout(this)
                            {
                                Orientation = Orientation.Vertical
                            };
                            if ('\u4e00' <= c && c < '\ua000')
                            {
                                if (prons != null)
                                {
                                    if (prons[character].Length > 0)
                                    {
                                        string t = "";
                                        switch (schema)
                                        {
                                            case "粵拼":
                                                t = string.Join("", prons[character].Cast<string>().Select(s => "\n" + s));
                                                break;
                                            case "Latina":
                                                t = string.Join("", prons[character].Cast<string>().Select(s => "\n" + JyutpingToLatina(s)));
                                                break;
                                            case "IPA":
                                                t = string.Join("", prons[character].Cast<string>().Select(s => "\n" + JyutpingToIpa(s)));
                                                break;
                                        }
                                        text = new TextView(this)
                                        {
                                            Gravity = GravityFlags.Center,
                                            Text = t.Substring(1)
                                        };
                                        layout.AddView(text);
                                    }
                                }
                                character++;
                            }
                            text = new TextView(this)
                            {
                                Gravity = GravityFlags.Center,
                                Text = c.ToString()
                            };
                        text.SetTextAppearance(this, Resource.Style.TextAppearance_AppCompat_Large);
                        layout.AddView(text);
                            llLine.AddView(layout);
                        }
                    });
                }
                RunOnUiThread(() =>
                {
                    llResult.AddView(llLine);
                });
            }
            RunOnUiThread(() =>
            {
                bQuery.Text = "査詢";
                bQuery.Enabled = true;
            });
        }
    }
}