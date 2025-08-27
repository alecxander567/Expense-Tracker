using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using JsonFormatting = Newtonsoft.Json.Formatting;

namespace Expense_Tracker_App
{
    public partial class Form1 : Form
    {
        private List<Expense> expenses = new List<Expense>();

        public Form1()
        {
            InitializeComponent();
        }

        private string GetGreetingMessage()
        {
            int hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 12)
            {
                return "Good Morning!☀️";
            }
            else if (hour >= 12 && hour < 18)
            {
                return "Good Afternoon!⛅";
            }
            else
            {
                return "Good Evening!🌙";
            }
        }

        private void SaveExpensesToLocal()
        {
            string json = JsonConvert.SerializeObject(expenses, JsonFormatting.Indented);
            File.WriteAllText("expenses.json", json);
        }

        private void LoadExpensesFromLocal()
        {
            if (File.Exists("expenses.json"))
            {
                string json = File.ReadAllText("expenses.json");
                expenses = JsonConvert.DeserializeObject<List<Expense>>(json) ?? new List<Expense>();
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = expenses;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            string fileName = $"Expenses_{DateTime.Now:yyyy_MM}.csv";

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine("Description,Category,Amount,Date");
                foreach (var exp in expenses)
                {
                    writer.WriteLine($"{exp.Description},{exp.Category},{exp.Amount},{exp.Date:yyyy-MM-dd}");
                }
            }

            MessageBox.Show("Expenses exported to " + fileName);

            expenses.Clear();
            dataGridView1.DataSource = null;
            File.Delete("expenses.json"); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblWelcomeMessage.Text = GetGreetingMessage();

            dataGridView1.Columns.Clear();

            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Description",
                DataPropertyName = "Description"
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Category",
                DataPropertyName = "Category"
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Amount",
                DataPropertyName = "Amount"
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Date",
                DataPropertyName = "Date"
            });

            var editButton = new DataGridViewButtonColumn
            {
                HeaderText = "Edit",
                Text = "✍🏾",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None, 
                Width = 60 
            };
            dataGridView1.Columns.Add(editButton);

            var deleteButton = new DataGridViewButtonColumn
            {
                HeaderText = "Delete",
                Text = "🚮",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 60
            };
            dataGridView1.Columns.Add(deleteButton);

            LoadExpensesFromLocal();
            UpdateTotal();
        }

        private void btnAddExpense_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBoxInput.Text) ||
                string.IsNullOrWhiteSpace(txtAmount.Text) ||
                comboCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            Expense expense = new Expense
            {
                Description = txtBoxInput.Text,
                Category = comboCategory.Text,   
                Amount = decimal.Parse(txtAmount.Text),
                Date = dateTimePicker1.Value
            };

            expenses.Add(expense);

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = expenses;

            UpdateTotal();

            SaveExpensesToLocal();

            txtBoxInput.Clear();
            txtAmount.Clear();
            comboCategory.SelectedIndex = -1;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; 

            var expense = expenses[e.RowIndex];

            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Edit")
            {
                string newDescription = Prompt.ShowDialog("Edit Description:", "Edit", expense.Description);
                string newCategory = Prompt.ShowDialog("Edit Category:", "Edit", expense.Category);
                string newAmount = Prompt.ShowDialog("Edit Amount:", "Edit", expense.Amount.ToString());

                if (decimal.TryParse(newAmount, out decimal updatedAmount))
                {
                    expense.Description = newDescription;
                    expense.Category = newCategory;
                    expense.Amount = updatedAmount;

                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = expenses;

                    UpdateTotal();

                    SaveExpensesToLocal();
                }
            }

            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
            {
                var confirm = MessageBox.Show("Are you sure you want to delete this expense?",
                                              "Confirm Delete",
                                              MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    expenses.RemoveAt(e.RowIndex);
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = expenses;

                    UpdateTotal();

                    SaveExpensesToLocal();
                }
            }
        }

        private void btnExport_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.Title = "Export Expenses";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string json = JsonConvert.SerializeObject(expenses, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(saveFileDialog.FileName, json);
                MessageBox.Show("Expenses exported successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (var exp in expenses)
            {
                total += exp.Amount;
            }
            lblTotal.Text = "₱" + total.ToString("N2");
        }

        private void txtBoxInput_TextChanged(object sender, EventArgs e) { }

        private void txtAmount_TextChanged(object sender, EventArgs e) { }

        private void lblWelcomeMessage_Click(object sender, EventArgs e){ }
    }

    public class Expense
    {
        public string Description { get; set; }
        public string Category { get; set; } 
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, Width = 340 };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultValue };
            Button confirmation = new Button() { Text = "Ok", Left = 200, Width = 80, Top = 90, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = 280, Width = 80, Top = 90, DialogResult = DialogResult.Cancel };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { textBox.Text = defaultValue; prompt.Close(); };

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : defaultValue;
        }
    }
}
