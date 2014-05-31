using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace VOConverter
{
    public partial class Form1 : Form
    {
        #region constants

        private const string template = "C:\\VisualOutcomes\\Code\\VOConverter\\VOConverter\\ASPX_template.temp";
        private const string root = "C:\\VisualOutcomes\\Code\\VOWeb\\VisualOutcome.Web\\";
        private const string codebehindExt = ".cs";
        private const string designerExt = ".designer.cs";
        private const string projFile = root + "VisualOutcome.Web.csproj";
        
        #endregion
        
        #region private members

        private string ascxPath = "";
        private string ascxName = "";
        private string aspxPath = "";
        private string codebehindName = "";
        private string inheritsValue = "";
        private string newline = "";
        private string newCodebehindName = "";
        private string newInheritsValue = "";
        private string saveFilePath = "";
        private string saveFileName = "";
        
        private StringBuilder controls = new StringBuilder();
        private StringBuilder content = new StringBuilder();
        private StringBuilder line = new StringBuilder();
            
        #endregion

        #region constructor

        public Form1()
        {
            InitializeComponent();
        }

        #endregion

        #region private methos

        private bool convertAscx(string ascx)
        {
            bool retVal = true;

            int cbs = -1;
            int cbe = -1;
            int ins = -1;
            int ine = -1;
            line = new StringBuilder();

            using (StreamReader rwOpenTemplate = new StreamReader(template))
            {
                while (!rwOpenTemplate.EndOfStream)
                {

                    line.Append(rwOpenTemplate.ReadToEnd());
                }
            }

            using (StreamReader rwOpenAscx = new StreamReader(ascx))
            {
                while (!rwOpenAscx.EndOfStream)
                {
                    // get codebehind and inherits
                    newline = rwOpenAscx.ReadLine();
                    if (newline.Contains("<%@ Control"))
                    {
                        cbs = newline.IndexOf("CodeBehind");
                        cbe = newline.IndexOf(".ascx.cs\"") + 9 - cbs;
                        codebehindName = newline.Substring(cbs, cbe);
                        
                        if (newline.Contains("Inherits"))
                        {
                            ins = newline.IndexOf("Inherits");
                            ine = newline.Substring(ins).IndexOf("\" ");
                            inheritsValue = newline.Substring(ins, ine + 1);
                        }
                        else
                        {
                            newline = rwOpenAscx.ReadLine();
                            if (newline.Contains("Inherits"))
                            {
                                ins = newline.IndexOf("Inherits");
                                ine = newline.Substring(ins).IndexOf("\" ");
                                inheritsValue = newline.Substring(ins, ine + 1);
                            }
                        }
                    }
                    else
                    {
                        if (newline.Contains("<%@ Register"))
                        {
                            // get registered controls
                            while (newline.Contains("<%@ Register"))
                            {
                                controls.Append(newline + Environment.NewLine);
                                newline = rwOpenAscx.ReadLine();
                            }
                        }
                        else
                        {
                            content.Append(newline);
                            // get content
                            content.Append(rwOpenAscx.ReadToEnd());
                            break;
                        }
                        content.Append(newline);
                    }
                }
            }

            newCodebehindName = codebehindName.Replace(".ascx", ".aspx");
            if (newCodebehindName.Substring(newCodebehindName.IndexOf("\"") + 1, 2).ToLower() == "uc")
                newCodebehindName = "CodeBehind=\"" + newCodebehindName.Substring(newCodebehindName.IndexOf("\"") + 3);
            
            newInheritsValue = inheritsValue;
            if (newInheritsValue.ToLower().Contains(".uc"))
            {
                newInheritsValue = newInheritsValue.Replace(".Uc", ".");
                newInheritsValue = newInheritsValue.Replace(".uc", ".");
                newInheritsValue = newInheritsValue.Replace(".UC", ".");
            }
            newInheritsValue = newInheritsValue.Replace(".Web.", ".Web.WebPages.");

            saveFilePath = newInheritsValue.Substring(newInheritsValue.IndexOf("\"")).Replace(".", "\\");
            saveFilePath = saveFilePath.Substring(saveFilePath.IndexOf("Web") + 4);
            saveFilePath = root + saveFilePath.Substring(0, saveFilePath.Length - 1) + ".aspx";
            //saveFilePath = saveFilePath.Substring(0, saveFilePath.LastIndexOf("\\"));
            string SaveFileName = saveFilePath.Substring(saveFilePath.LastIndexOf("\\"));
            int i = newCodebehindName.IndexOf("\"") + 1;
            int l = newCodebehindName.Length;
            string cb1 = newCodebehindName.Substring(i, l - i - 1);
            
            //ASPXFileName = cb1.Substring(0,cb1.Length -3);
            //DepUpon = SaveFileName.Substring(1);

            if (!Directory.Exists(@aspxPath))
            {
                Directory.CreateDirectory(@aspxPath);
            }

            FileStream fsSave = File.Create(saveFilePath);

            if (line != null)
            {
                // replace place holders
                line.Replace("CodeBehind", String.Format("{0}", newCodebehindName));
                line.Replace("Inherits", String.Format("{0}", newInheritsValue));
                line.Replace("[RegisterControls]", controls.ToString());
                line.Replace("[PageContent]", content.ToString());
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(fsSave);
                    sw.Write(line);
                }
                catch (Exception ex)
                {
                    retVal = false;
                }
                finally
                {
                    sw.Close();
                }
            }

            return retVal;
        }

        private bool convertCodeBehind()
        {
            bool retVal = true;

            StringBuilder line = new StringBuilder();
            string newline = "";
            int i = codebehindName.IndexOf("\"") + 1;
            int l = codebehindName.Length;
            string cb1 = codebehindName.Substring(i, l - i - 1);
            using (StreamReader rwOpenCB = new StreamReader(ascxPath + ascxName + codebehindExt))
            {
                while (!rwOpenCB.EndOfStream)
                {
                    // get codebehind and inherits
                    newline = rwOpenCB.ReadLine();
                    if (newline.Contains("namespace"))
                    {
                        newline = newline.Replace("Web.", "Web.WebPages.");
                        line.Append(newline + Environment.NewLine);
                        //newline = rwOpenCB.ReadLine();
                    }
                    else
                    {
                        if (newline.Contains("public partial class"))
                        {
                            newline = newline.Replace("Uc", "");
                            newline = newline.Replace("uc", "");
                            newline = newline.Replace("UC", "");
                            newline = newline.Replace("VOUserControlBase", "VOBasePage");
                            line.Append(newline + Environment.NewLine);
                        }
                        else
                        {
                            line.Append(newline + Environment.NewLine);
                        }
                    }
                }
            }

            string SaveFileName = newCodebehindName.Replace("CodeBehind=\"", "");
            SaveFileName = SaveFileName.Substring(0, SaveFileName.Length - 1);
            //NewCBFileName = SaveFileName;

            if (!Directory.Exists(@aspxPath))
            {
                Directory.CreateDirectory(@aspxPath);
            }

            FileStream fsSave = File.Create(saveFilePath + codebehindExt);

            if (line != null)
            {
                line.Replace("HasCurrentClient", "HasClient");
                line.Replace("CurrentClient", "ActiveClient");
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(fsSave);
                    sw.Write(line);
                }
                catch (Exception ex)
                {
                    retVal = false;
                }
                finally
                {
                    sw.Close();
                }
            }

            return retVal;
        }

        private bool convertDesigner()
        {
            bool retVal = true;

            StringBuilder line = new StringBuilder();
            string newline = "";
            int i = codebehindName.IndexOf("\"") + 1;
            int l = codebehindName.Length;
            string cb1 = codebehindName.Substring(i, l - i - 1);
            using (StreamReader rwOpenCB = new StreamReader(ascxPath + ascxName + designerExt))
            {
                while (!rwOpenCB.EndOfStream)
                {
                    // get codebehind and inherits
                    newline = rwOpenCB.ReadLine();
                    if (newline.Contains("namespace"))
                    {
                        newline = newline.Replace("Web.", "Web.WebPages.");
                        line.Append(newline + Environment.NewLine);
                        //newline = rwOpenCB.ReadLine();
                    }
                    else
                    {
                        if (newline.Contains("public partial class"))
                        {
                            newline = newline.Replace("Uc", "");
                            newline = newline.Replace("uc", "");
                            newline = newline.Replace("UC", "");
                            line.Append(newline + Environment.NewLine);
                        }
                        else
                        {
                            line.Append(newline + Environment.NewLine);
                        }
                    }
                }
            }

            string SaveFileName = newCodebehindName.Replace("CodeBehind=\"", "");
            SaveFileName = SaveFileName.Substring(0, SaveFileName.Length - 1);
            SaveFileName = SaveFileName.Replace("aspx.cs", "aspx.designer.cs");

            if (!Directory.Exists(@aspxPath))
            {
                Directory.CreateDirectory(@aspxPath);
            }

            FileStream fsSave = File.Create(saveFilePath + designerExt);

            if (line != null)
            {
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(fsSave);
                    sw.Write(line);
                }
                catch (Exception ex)
                {
                    retVal = false;
                }
                finally
                {
                    sw.Close();
                }
            }

            return retVal;
        }
        
        private bool AddToProj()
        {
            bool retVal = true;

            string ci = saveFilePath.Replace(root, "");
            string addaspx = "<Content Include=\"{0}\" />";
            string openCompile = "<Compile Include=\"{0}\" >";
            string depUpon = "<DependentUpon>{0}</DependentUpon>";
            string subType = "<SubType>ASPXCodeBehind</SubType>";
            string closeCompile = "</Compile>";
            string newline;

            StringBuilder line = new StringBuilder();

            using (StreamReader rwOpenProj = new StreamReader(projFile))
            {
                while (!rwOpenProj.EndOfStream)
                {
                    // get codebehind and inherits
                    newline = rwOpenProj.ReadLine();
                    if (newline.Contains("<addAspx"))
                    {
                        line.Append(newline + Environment.NewLine);
                         
                        line.Append(string.Format(addaspx, ci) + Environment.NewLine);
                    }
                    else
                    {
                        if (newline.Contains("<addCompile"))
                        {
                            line.Append(newline + Environment.NewLine);
                            line.Append(string.Format(openCompile, ci + codebehindExt) + Environment.NewLine);
                            line.Append(string.Format(depUpon, ascxName.Replace(".ascx", ".aspx")) + Environment.NewLine);
                            line.Append(subType + Environment.NewLine);
                            line.Append(closeCompile + Environment.NewLine);
                            line.Append(string.Format(openCompile, ci + designerExt) + Environment.NewLine);
                            line.Append(string.Format(depUpon, ascxName.Replace(".ascx", ".aspx")) + Environment.NewLine);
                            line.Append(closeCompile + Environment.NewLine);
                        }
                        else
                        {
                            line.Append(newline + Environment.NewLine);
                        }
                    }
                }
            }
            File.Delete(projFile + ".old");
            File.Move(projFile, projFile + ".old");
            File.Delete(projFile);
            FileStream fsSave = File.Create(projFile);
            if (line != null)
            {
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(fsSave);
                    sw.Write(line);
                }
                catch (Exception ex)
                {
                    retVal = false;
                }
                finally
                {
                    sw.Close();
                }
            }

            return retVal;
        }
        
        private bool ValidatePath()
        {
            bool retVal = true;

            if (txtUCPath.Text != string.Empty)
            {
                if (!File.Exists(root + txtUCPath.Text))
                    retVal = false;
            }
            else
                retVal = false;
            
            return retVal;
        }

        #endregion

        #region control events

        private void button1_Click(object sender, EventArgs e)
        {
            if (ValidatePath())
            {
                ascxPath = root + txtUCPath.Text.Substring(0,txtUCPath.Text.LastIndexOf("/")) + "\\";
                aspxPath = ascxPath.Replace(".Web\\", ".Web\\WebPages\\");
                ascxName = txtUCPath.Text.Substring(txtUCPath.Text.LastIndexOf("/") + 1);
                if (convertAscx(ascxPath + ascxName))
                    if (convertCodeBehind())
                        if (convertDesigner())
                            if (AddToProj())
                                MessageBox.Show(string.Format("User Control: {0} successfully converted to {1}.", txtUCPath.Text, txtUCPath.Text.Replace(".ascx",".aspx")));
            }
            else
                MessageBox.Show("You must enter a valid control file.");
        }

        #endregion

    }
}
