using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace XMLParse
{
    public partial class Form1 : Form
    {
        string version = "XMLParse (v1.0)";
        int searchStrPos = 0;
        string oldSearchStr = string.Empty;

        OpenFileDialog openFileDialog1 = new OpenFileDialog();
        string extension = string.Empty;
        string filename = string.Empty;
        string filenameNoExtension = string.Empty;
        string fileroot = string.Empty;
        string filepath = string.Empty;
        public Form1()
        {
            InitializeComponent();
            this.Text = version;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Filter = "All files (*.*)|*.*";
            lblCount.Visible = false;
            inputFileName.Visible = false;

            //Anker
            groupBox1.Anchor = AnchorStyles.Top |                AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtDataText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            //text area
            txtDataText.SelectAll();
            txtDataText.SelectionFont = new Font("Calibri", 11);
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK)                      // Test result.
            {
                string selectFileName = openFileDialog1.FileName;
                extension = System.IO.Path.GetExtension(selectFileName);
                filename = System.IO.Path.GetFileName(selectFileName);
                filenameNoExtension = System.IO.Path.GetFileNameWithoutExtension(selectFileName);
                fileroot = System.IO.Path.GetPathRoot(selectFileName);
                filepath = System.IO.Path.GetDirectoryName(selectFileName);

                FileInfo info = new FileInfo(selectFileName);
                long filesize = info.Length;
                if (filesize > 2147483647)
                {
                    inputFileName.Text = "Too big file size(Max is 2,147,483,647 bytes)";
                }
                else
                {
                    this.Text = version + " - " + filepath;
                    inputFileName.Text = filename;
                    txtDataText.Text = File.ReadAllText(info.FullName);
                }
                inputFileName.Visible = true;
            }
            else
            {
                inputFileName.Text = string.Empty;
            }

        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtDataText.Text = string.Empty;
            txtSearch.Text = string.Empty;
            searchStrPos = 0;
            lblCount.Visible = false;
            inputFileName.Visible = false;
        }

        private void btnCopyText_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(txtDataText.Text);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            txtSearch.Text = txtSearch.Text.Trim();
            int founded = Regex.Matches(txtDataText.Text, txtSearch.Text).Count;

            if (txtSearch.Text == string.Empty) return;
            if (oldSearchStr != txtSearch.Text)
            {
                oldSearchStr = txtSearch.Text;
                searchStrPos = 0;

                txtDataText.SelectionStart = 0;
                txtDataText.SelectionLength = txtDataText.TextLength;
                txtDataText.SelectionColor = Color.Black;

                lblCount.Visible = true;
                lblCount.Text = "Wait.";
            }

            if (txtDataText.SelectionStart < searchStrPos) 
                searchStrPos = txtDataText.SelectionStart;

            if (searchStrPos < txtDataText.TextLength)
            {
                int count = 0;
                searchStrPos++;
                searchStrPos = Utility.HighlightText(txtDataText, txtSearch.Text, Color.Red, searchStrPos, ref count);
                count = count > 0 ? count : 1;      //if not found, set 1
                lblCount.Text = (founded - count + 1).ToString() + " of " + founded.ToString();
                txtDataText.SelectionStart = searchStrPos;
                txtDataText.Focus();
            }
            else
            {
                searchStrPos = 0;
            }
        }

        private void btnBackward_Click(object sender, EventArgs e)
        {
            txtSearch.Text = txtSearch.Text.Trim();
            int founded = Regex.Matches(txtDataText.Text, txtSearch.Text).Count;

            if (txtSearch.Text == string.Empty) return;
            if (oldSearchStr != txtSearch.Text)
            {
                oldSearchStr = txtSearch.Text;
                searchStrPos = txtDataText.TextLength;

                txtDataText.SelectionStart = 0;
                txtDataText.SelectionLength = txtDataText.TextLength;
                txtDataText.SelectionColor = Color.Black;

                lblCount.Visible = true;
                lblCount.Text = "Wait.";
            }

            //get current curso position and check
            if (txtDataText.SelectionStart > searchStrPos)
                searchStrPos = txtDataText.SelectionStart;

            if (searchStrPos > 0)
            {
                int count = 0;
                searchStrPos--;
                searchStrPos = Utility.Back_HighlightText(txtDataText, txtSearch.Text, Color.Red, searchStrPos, ref count);
                count = count > 0 ? count : 1;      //if not found, set 1
                lblCount.Text = (founded - count + 1).ToString() + " of " + founded.ToString();
                txtDataText.SelectionStart = searchStrPos;
                txtDataText.Focus();
            }
            else
            {
                searchStrPos = 0;
            }
        }

        private void btnParseXml_Click(object sender, EventArgs e)
        {
            if (txtDataText.TextLength < 1) return;

            txtDataText.SelectAll();
            txtDataText.SelectionFont = new Font("Consolas", 10);

            string xmloutput = String.Empty;
            xmloutput = txtDataText.Text;

            System.Xml.XmlDocument oDoc = new System.Xml.XmlDocument();
            try
            {
                //Boolean isXML = oDoc.LoadXml(XMLData);
                oDoc.LoadXml(xmloutput);
                txtDataText.Text = "";
                display_node(oDoc.ChildNodes, -4);
            }
            catch (Exception ex)
            {
                txtDataText.Text = ex.Message;
            }

            txtDataText.SelectAll();
            txtDataText.SelectionFont = new Font("Consolas", 10);

            //https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.send(v=vs.110).aspx
            //SendKeys.Send("{HOME}");
            System.Media.SystemSounds.Beep.Play();
        }

        private void display_node(System.Xml.XmlNodeList Nodes, int Indent)
        {
            Indent += 4;    //Insert space left to make indent

            foreach (System.Xml.XmlNode xNode in Nodes)
            {
                if (xNode.NodeType == XmlNodeType.Element || xNode.NodeType == XmlNodeType.Text)
                {
                    if (xNode.NodeType == XmlNodeType.Element)
                    {
                        txtDataText.Text += System.Environment.NewLine;
                        txtDataText.Text += new String(' ', Indent);
                        txtDataText.Text += xNode.Name + ": ";

                        if (xNode.Attributes.Count > 0)
                        {
                            for (int i = 0; i < xNode.Attributes.Count; i++)
                            {
                                txtDataText.Text += xNode.Attributes[i].Name + ": " + xNode.Attributes[i].Value + " ";
                                if (i != xNode.Attributes.Count - 1)
                                    txtDataText.Text += ", ";
                            }
                        }
                    }
                    txtDataText.Text += string.IsNullOrEmpty(xNode.Value) ? "" : xNode.Value;
                }
                if (xNode.HasChildNodes)
                {
                    display_node(xNode.ChildNodes, Indent);
                }
            }

        }


    }



    static class Utility {
        public static int HighlightText(RichTextBox myRtb, string word, Color color, int startIndex, ref int count)
        {
            bool found = false;
            int forwardPos = startIndex;
            int index;
            count = 0;

            while ((index = myRtb.Text.IndexOf(word, startIndex)) != -1)
            {
                if (forwardPos < index)
                {
                    count++;                    //count all matches
                    if (!found)
                    {
                        forwardPos = index;     //save first found position
                        found = true;
                    }
                }

                myRtb.Select(index, word.Length);
                myRtb.SelectionColor = color;

                startIndex = index + word.Length;
            }
            return forwardPos;
        }
        public static int Back_HighlightText(RichTextBox myRtb, string word, Color color, int startIndex, ref int count)
        {
            bool found = false;
            int pos = startIndex;
            int index;
            count = 0;

            while ((index = myRtb.Text.LastIndexOf(word, startIndex)) != -1)
            {
                count++;
                if (pos > index && !found)
                {
                    pos = index;
                    found = true;
                }

                myRtb.Select(index, word.Length);
                myRtb.SelectionColor = color;

                startIndex = index - word.Length;
            }
            return pos;
        }
    }

}
