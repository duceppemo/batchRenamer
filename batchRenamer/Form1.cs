using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace batchRenamer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //For debug
            //textBox1.Text = @"C:\Users\duceppem\Desktop\testRename.txt";
            openFileDialog1.Filter = "Tab-separated Text File (*.txt, *.tsv)|*.txt;*.tsv|All Files (*.*)|*.*";
            
            textBox1.AllowDrop = true;
            textBox1.DragEnter += new DragEventHandler(textBox1_DragEnter);
            textBox1.DragDrop += new DragEventHandler(textBox1_DragDrop);
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //Display file name in textbox
            textBox1.Text = openFileDialog1.FileName;

            //Validate button now available
            buttonValidate.Enabled = true;
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length != 0)
            {
                textBox1.Text = files[0];
                clearDataGridView();
            }

            //Validate button now available
            buttonValidate.Enabled = true;
        }

        private void clearDataGridView()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
        }

        //Remove leading and trailing double quotes is present (illegal character)
        private string trimBeginEnd(Char t, string s)
        {
            if (s.StartsWith(t.ToString()) == true && s.EndsWith(t.ToString()) == true)
            {
                //s = s.TrimStart(new Char[] { t }).TrimEnd(new Char[] { t });
                return s.TrimStart(new Char[] { t }).TrimEnd(new Char[] { t });
            }
            else
            {
                return s;
            }
        }

        private bool isDirectory(string s)
        {
            bool isIt = false;
            if (System.IO.Directory.Exists(s))
            {
                isIt = true;
                MessageBox.Show("The following entry from the translation table is a directory:" + '\n' + '\n' + s,
                    "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return isIt;
        }

        private void buttonValidate_Click(object sender, EventArgs e)
        {
            //Make sure button2 label and datagridview reverts back to original in case multiple renaming actions occur in one session
            buttonRename.Text = "Rename";
            clearDataGridView();

            //Show content of translation table in the dataGridView
            string[] lineArray = new string[2];

            try
            {

                //Check is translation table file is empty
                if (new System.IO.FileInfo(textBox1.Text).Length == 0)
                {
                    MessageBox.Show("Translation table file is empty!",
                                        "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                //Open file stream
                System.IO.Stream fileStream = System.IO.File.OpenRead(textBox1.Text);

                int lineNumber = 0;
                string line;
                if (fileStream != null)
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(fileStream))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            lineNumber += 1;

                            //string line = sr.ReadLine();
                            if (!string.IsNullOrEmpty(line)) //if line not empty
                            {
                                //Split 
                                string[] fields = line.Split('\t');

                                //Check if is there is only two fields separated by a tab
                                if (fields.Length != 2)
                                {
                                    MessageBox.Show("Translation table must be a tab-separated file with 2 columns." + "\n"
                                        + "Problem found at line " + lineNumber,
                                        "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                    clearDataGridView();
                                    return; //Stop further execution
                                }

                                string input = fields[0];
                                string output = fields[1];

                                //Remove leading and trailing double quotes if present (illegal character)
                                input = trimBeginEnd('"', input);
                                output = trimBeginEnd('"', output);

                                
                                //Check if translation table format is valid


                                

                                //Check if input is a directory
                                if (isDirectory(input) == true || isDirectory(output) == true)
                                {
                                    clearDataGridView();
                                    return;
                                }

                                //Check if input file exists
                                string inputDirectoryName;
                                string inputFileName;

                                if (System.IO.File.Exists(input) == false)
                                {
                                    MessageBox.Show("The following file at line " + lineNumber + " from the translation table does not exist:" + '\n' + '\n' + input,
                                        "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                    //Clear datagridview
                                    clearDataGridView();
                                    return; //Stop further execution
                                }
                                else
                                {
                                    inputDirectoryName = System.IO.Path.GetDirectoryName(input);
                                    inputFileName = System.IO.Path.GetFileName(input);
                                }

                                //Check for illegal characters
                                string outputDirectoryName;
                                string outputFileName;

                                try
                                {
                                    outputDirectoryName = System.IO.Path.GetDirectoryName(output);
                                }
                                catch
                                {
                                    MessageBox.Show("The following ouput path at line " + lineNumber + " has illegal characters:" + '\n' + '\n' + output,
                                        "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    
                                    clearDataGridView();
                                    return;
                                }
                                
                                //File name
                                outputFileName = output.Substring(outputDirectoryName.Length + 1, output.Length - outputDirectoryName.Length - 1);
                                
                                char[] invalidPathChars = System.IO.Path.GetInvalidPathChars();
                                char[] invalidFileChars = System.IO.Path.GetInvalidFileNameChars();

                                foreach(char c in invalidFileChars)
                                {
                                    if (outputFileName.Contains(c))
                                    {
                                        MessageBox.Show("The following output file name at line " + lineNumber + " has illegal charaters:" + '\n' + '\n' + output,
                                       "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                        clearDataGridView();
                                        return;
                                    }
                                }

                                //Check if path of original and renamed file is the same
                                if (inputDirectoryName != outputDirectoryName)
                                {
                                    MessageBox.Show("Different input and output paths at line " + lineNumber + ":" + '\n' + '\n' + line + '\n' + '\n' +
                                    "This application can only rename files in place.",
                                       "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                    clearDataGridView();
                                    return;
                                }


                                //Check that original file and renamed file names are different
                                if (outputFileName == inputFileName)
                                {
                                    MessageBox.Show("Input and output file name identical at line " + lineNumber + ":" + '\n' + '\n' + line,
                                   "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                    clearDataGridView();
                                    return;
                                }
                               

                                //Check if the output file has non-alphanumerical characters
                                Regex rgx = new Regex("[^a-zA-Z0-9-_.]");

                                if (rgx.IsMatch(outputFileName) == true)
                                {
                                    MessageBox.Show("Please only use alphanumerical characters in output name at line " + lineNumber + ":" + '\n' + '\n' + outputFileName,
                                       "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                    clearDataGridView();
                                    return;
                                }
                                
                                //Populate the datagridview
                                dataGridView1.Rows.Add(input, output);

                                //For debug
                                // Read the first line from the file and write it the textbox.
                                //richTextBox1.Text = line;
                            }
                        }
                    }
                }

                //Close file stream
                fileStream.Close();

                //Check if duplicates in both input and output names
                for (int currentRow = 0; currentRow < dataGridView1.Rows.Count - 1; currentRow++)
                {
                    DataGridViewRow rowToCompare = dataGridView1.Rows[currentRow];

                    for (int otherRow = currentRow + 1; otherRow < dataGridView1.Rows.Count; otherRow++)
                    {
                        DataGridViewRow row = dataGridView1.Rows[otherRow];

                        bool duplicateRow = true;

                        for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
                        {
                            if (!rowToCompare.Cells[cellIndex].Value.Equals(row.Cells[cellIndex].Value))
                            {
                                duplicateRow = false;
                                break;
                            }
                        }

                        if (duplicateRow == true)
                        {
                            MessageBox.Show("Line " + lineNumber + " contains a duplicate:" + '\n' + '\n' + 
                                dataGridView1.Rows[currentRow].Cells[0].Value.ToString() + '\t' +
                                dataGridView1.Rows[currentRow].Cells[1].Value.ToString(),
                                "Invalid Translation Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            //Clear datagridview
                            clearDataGridView();

                            return; //Stop further execution
                        }
                    }
                }

                //Enable the Rename button and show validation message
                labelStatus.Visible = true;
                buttonRename.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    //Get the old and new file names from the datagriview table
                    string oldName = dataGridView1.Rows[i].Cells[0].Value.ToString();
                    string newName = dataGridView1.Rows[i].Cells[1].Value.ToString();

                    //Replace file names
                    System.IO.File.Move(oldName, newName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            //Show complete message and diable the rename button to prevent error on re-click
            //Check if last file to rename exists and is not locked
            string lastFile = dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value.ToString();

            while (System.IO.File.Exists(lastFile) == false)
            {
                //wait
            }

            //Completion message
            labelStatus.Text = "File renaming completed!";
            buttonRename.Enabled = false;
        }

        private void textBox1_MouseHover(object sender, EventArgs e)
        {
            //Build tip message
            string tipText1 = "A tab-separated text file with two columns:" + '\n' + '\n' +
                @"C:\Users\username\Desktop\file_to_rename.txt" + '\t' + @"C:\Users\username\Desktop\renamed_file.txt" + '\n' + '\n' +
            "Note that paths must be identical. Just the file names are changing.";
            //Assign tip message to textbox
            toolTip1.SetToolTip(textBox1, tipText1);
        }
    }
}
