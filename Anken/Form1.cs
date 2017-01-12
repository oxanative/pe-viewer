using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Anken
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //デフォルトのセルスタイルの設定
            this.defaultCellStyle = new DataGridViewCellStyle();
            //現在のセルのセルスタイルの設定
            this.mouseCellStyle = new DataGridViewCellStyle();
            this.mouseCellStyle.BackColor = Color.LightBlue;
            this.mouseCellStyle.SelectionBackColor = Color.Blue;


            if (File.Exists(dsfile))
            {
                dsAnken.ReadXml(dsfile);
                RefreshRowColor();
            }



        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                dsAnken.Clear();
                foreach (string fileName in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    readFile(fileName);
                }

                RefreshRowColor();
            }
        }

        private const string dsfile = @"anken.xml";

        private void readFile(string filename)
        {
            string line = string.Empty;

            //string all = ReadToEnd(filename);

            using (StreamReader sr = new StreamReader(filename, Encoding.GetEncoding("Shift_JIS")))
            {
                AnkenDataSet.AnkenTblRow r = null;
                StringBuilder sb = new StringBuilder();
                while ((line = sr.ReadLine()) != null)
                {
                    if (HasSplitter(line))
                    {
                        if (r != null)
                        {
                            r.Memo = sb.ToString();
                        }
                        continue;
                    }

                    if (line.Contains("◆担当："))
                    {
                        r = dsAnken.AnkenTbl.NewAnkenTblRow();
                        dsAnken.AnkenTbl.AddAnkenTblRow(r);
                        r.ID = line;
                        sb.Clear();
                    }
                    if (HasStation(line))
                    {
                        r.Station = line;
                    }
                    if (HasTanka(line))
                    {
                        r.Unit = line;
                    }
                    if (!IsIgnore(line))
                    {
                        sb.Append(line).Append("\r\n");
                    }
                }
                if (r != null)
                {
                    r.Memo = sb.ToString();
                }
            }

            dsAnken.WriteXml(dsfile);
        }

        static string[] ignore = {
                                      "案件情報（",
                                      "ページ(",
                                  };
        private bool IsIgnore(string s)
        {
            foreach (var key in ignore)
            {
                if (s.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        static string[] station = {
                                      "【勤務地】",
                                      "最寄り駅：",
                                      "場所",
                                  };

        private bool HasStation(string s)
        {
            foreach (var st in station)
            {
                if (s.Contains(st))
                {
                    return true;
                }
            }
            return false;
        }

        static string[] unit = {
                                      "万円",
                                      //"単価",
                                  };

        private bool HasTanka(string s)
        {
            foreach (var st in unit)
            {
                if (s.Contains(st))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasSplitter(string s)
        {
            return s.Contains("==============================================================================================");
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {

            FilterIt();
        }
        private string ReadToEnd(string input)
        {
            StringBuilder builder = new StringBuilder();

            using (PdfReader reader = new PdfReader(input))
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                //Page
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    string curPage = PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                    builder.Append(curPage);

                    /* もし一行ずつ読みたいならこちらを挿入し、直上のbuilder.Append(curPage);を削除
                    string[] lineSet = curPage.Split('\n');
                    foreach (string line in lineSet) {
                        builder.Append(line);
                    }
                     */
                }
                reader.Close();
            }

            return builder.ToString();
        }
        private string CreateFilter(string col, string val)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(col);
            sb.Append(" LIKE '%");
            sb.Append(val);
            sb.Append("%' ");
            return sb.ToString();
        }

        private void txtFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FilterIt();
            }
        }

        private void FilterIt()
        {
            string s = txtFilter.Text;
            StringBuilder sb = new StringBuilder();
            if (s.Trim().Length != 0)
            {
                sb.Append(CreateFilter("Memo", s));
            }


            if (chkSelected.Checked)
            {
                if (sb.Length != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append("Selected = True");
            }

            bsAnken.Filter = sb.ToString();
            RefreshRowColor();

        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            txtFilter.Text = string.Empty;
            FilterIt();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            dsAnken.WriteXml(dsfile);
        }
        //デフォルトのセルスタイル
        private DataGridViewCellStyle defaultCellStyle;
        //マウスポインタの下にあるセルのセルスタイル
        private DataGridViewCellStyle mouseCellStyle;

        private void RefreshRowColor()
        {
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                DataRowView drv = (DataRowView)r.DataBoundItem;
                AnkenDataSet.AnkenTblRow dr = (AnkenDataSet.AnkenTblRow)drv.Row;
                if (dr.Selected)
                {
                    ChangeStyle(r, mouseCellStyle);
                }
                else
                {
                    ChangeStyle(r, defaultCellStyle);
                }
            }
        }

        private void ChangeStyle(DataGridViewRow r, DataGridViewCellStyle style)
        {
            foreach (DataGridViewCell c in r.Cells)
            {
                c.Style = style;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            RefreshRowColor();
        }

        private void dataGridView1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            RefreshRowColor();

        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            var list = (from d in dsAnken.AnkenTbl
                        where d.Selected == true
                        select d).ToArray();
            StringBuilder sb = new StringBuilder();

            foreach (var r in list)
            {
                sb.Append("==============================================================================================\n");
                sb.Append(r.Memo);
            }
            Clipboard.SetText(sb.ToString());
        }
    }
}
