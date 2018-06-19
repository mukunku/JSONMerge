using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace JSONMerge
{
    public partial class MainForm : Form
    {
        private int numberOfRecordsRead = 0;

        public MainForm()
        {
            InitializeComponent();
            this.Icon = JSONMerge.Properties.Resources.jsonicon;
        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            if (this.mainFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.filePathTextBox.Text = this.mainFileDialog.FileName;
            }
        }

        private void mergeButton_Click(object sender, EventArgs e)
        {
            this.mergeButton.Enabled = false;
            this.openFileButton.Enabled = false;
            this.recordSeperatorTextBox.Enabled = false;
            this.filePathTextBox.Enabled = false;
            this.numberOfRecordsRead = 0;
            try
            {
                this.resultTextBox.Text = string.Empty;
                string filePath = this.filePathTextBox.Text;
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        string delimiter = this.recordSeperatorTextBox.Text;
                        if (!string.IsNullOrWhiteSpace(delimiter))
                        {
                            delimiter = delimiter.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");

                            var fileDetailsArg = new FileDetailsArg(filePath, delimiter);
                            this.readFileBackgroundWorker.RunWorkerAsync(fileDetailsArg);
                        }
                        else
                        {
                            MessageBox.Show("Please enter a record seperator (Example: \\r\\n)", "Invalid Seperator");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Selected file doesn't exist", "File Doesn't Exist");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a file first", "No file selected");
                }
            }
            catch (Exception ex)
            {
                this.resultTextBox.Text = string.Concat("Could not start processing the file.", Environment.NewLine, ex.ToString());

                if (!this.readFileBackgroundWorker.IsBusy)
                {
                    this.mergeButton.Enabled = true;
                    this.openFileButton.Enabled = true;
                    this.recordSeperatorTextBox.Enabled = true;
                    this.filePathTextBox.Enabled = true;
                }
            }
        }

        private void MapJObjectToMaster(JObject masterObject, JObject currentObject)
        {
            foreach (KeyValuePair<string, JToken> prop in currentObject)
            {
                switch (prop.Value.Type)
                {
                    case JTokenType.Array:
                        JArray jArray = null;
                        if (masterObject.ContainsKey(prop.Key) && masterObject[prop.Key].Type != JTokenType.Null)
                            jArray = (JArray)masterObject[prop.Key];
                        else
                        {
                            if (masterObject.ContainsKey(prop.Key) && masterObject[prop.Key].Type == JTokenType.Null)
                                masterObject.Remove(prop.Key);

                            jArray = new JArray();
                            masterObject.Add(prop.Key, jArray);
                        }
                        this.MapJArrayToMaster(jArray, (JArray)prop.Value);
                        break;
                    case JTokenType.Object:
                        JObject jObject = null;
                        if (masterObject.ContainsKey(prop.Key) && masterObject[prop.Key].Type != JTokenType.Null)
                            jObject = (JObject)masterObject[prop.Key];
                        else
                        {
                            if (masterObject.ContainsKey(prop.Key) && masterObject[prop.Key].Type == JTokenType.Null)
                                masterObject.Remove(prop.Key);

                            jObject = new JObject();
                            masterObject.Add(prop.Key, jObject);
                        }
                        this.MapJObjectToMaster(jObject, (JObject)prop.Value);
                        break;
                    case JTokenType.Boolean:
                    case JTokenType.Bytes:
                    case JTokenType.Date:
                    case JTokenType.Float:
                    case JTokenType.Guid:
                    case JTokenType.Integer:
                    case JTokenType.String:
                    case JTokenType.Null:
                        if (!masterObject.ContainsKey(prop.Key))
                            masterObject.Add(prop.Key, prop.Value);
                        else if (masterObject[prop.Key].Type == JTokenType.Null && prop.Value.Type != JTokenType.Null)
                            masterObject[prop.Key] = prop.Value;
                        break;
                    default:
                        continue;
                }
            }
        }

        private void MapJArrayToMaster(JArray masterArray, JArray currentArray)
        {
            if (currentArray.Count == 0)
                return;

            this.CheckForHeterogeneousArray(currentArray);

            if (masterArray.Count == 0)
            {
                //First time procesing this array
                {
                    switch (currentArray.First.Type)
                    {
                        case JTokenType.Array:
                            var jArray = new JArray();
                            masterArray.Add(jArray);

                            //What this will do for nested arrays.. your guess is as good as mine.
                            this.MapJArrayToMaster(jArray, (JArray)currentArray.First);

                            break;
                        case JTokenType.Object:
                            var jObject = new JObject();
                            masterArray.Add(jObject);

                            foreach(JObject currentObject in currentArray)
                            {
                                this.MapJObjectToMaster(jObject, currentObject);
                            }

                            break;
                        case JTokenType.Boolean:
                        case JTokenType.Bytes:
                        case JTokenType.Date:
                        case JTokenType.Float:
                        case JTokenType.Guid:
                        case JTokenType.Integer:
                        case JTokenType.String:
                            masterArray.Add(currentArray.First); //Let's have only 1 element max per array for simplicity
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                //We assume 1 element per output array
                if (masterArray[0] is JObject)
                {
                    foreach (JObject currentObject in currentArray)
                    {
                        this.MapJObjectToMaster((JObject)masterArray[0], currentObject);
                    }
                }
                else if (masterArray[0] is JArray)
                {
                    var nestedMasterArray = (JArray)masterArray[0];
                    this.MapJArrayToMaster(nestedMasterArray, currentArray);
                }
                //No need to handle simple value arrays                   
            }
        }

        private void CheckForHeterogeneousArray(JArray jArray)
        {
            JTokenType firstType = JTokenType.None;
            bool isFirst = true;
            foreach (JToken elem in jArray)
            {
                if (isFirst)
                {
                    firstType = elem.Type;
                    isFirst = false;
                }
                else if (firstType != elem.Type)
                    throw new NotSupportedException("Heterogeneous arrays are currently not supported");
            }
        }

        private void readFileBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                var args = (FileDetailsArg)e.Argument;

                JObject masterJObject = new JObject();
                using (StreamReader reader = new StreamReader(args.FilePath))
                {
                    int recordCount = 0;
                    IEnumerable<string> fileEnumerator = reader.ReadUntil(args.Delimiter);
                    foreach (string rawJson in fileEnumerator)
                    {
                        JObject jObject = JObject.Parse(rawJson);
                        this.MapJObjectToMaster(masterJObject, jObject);

                        ((BackgroundWorker)sender).ReportProgress(++recordCount);
                    }
                }

                e.Result = masterJObject;
            }
            catch(Exception ex)
            {
                e.Result = ex;
            }
        }

        private void readFileBackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.numberOfRecordsRead = e.ProgressPercentage;
            this.resultTextBox.Text = string.Format("{0} records processed so far...", e.ProgressPercentage);
        }

        private void readFileBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                this.mergeButton.Enabled = true;
                this.openFileButton.Enabled = true;
                this.recordSeperatorTextBox.Enabled = true;
                this.filePathTextBox.Enabled = true;

                if (e.Result is Exception)
                {
                    throw (Exception)e.Result;
                }
                else
                {
                    var result = (JObject)e.Result;
                    this.resultTextBox.Text = result.ToString();
                }
            }
            catch(Exception ex)
            {
                this.resultTextBox.Text = string.Format("Encountered an error while processing record no: {0}{1}{2}", this.numberOfRecordsRead, Environment.NewLine, ex.ToString());
            }
        }

        private void resultTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                ((TextBox)sender).SelectAll();
        }
    }
}
