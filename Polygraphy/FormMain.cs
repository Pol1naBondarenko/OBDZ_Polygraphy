using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Polygraphy
{
    public partial class FormMain : Form
    {
        DbActions dbActions = new DbActions();
        private List<int> productsIDInCart = new List<int>();
        private List<int> productsQuantityInCart = new List<int>();
        Validation validation = new Validation();

        public bool isAutorized = false, isAdmin = false; public int? autorizedUserID = null; public string userNameSurname = null; private decimal cartTotalAmount = 0;

        private bool isDragging = false; private int offsetX, offsetY;

        public FormMain()
        {
            InitializeComponent();         
            buttonProductsUsers_Click(this, EventArgs.Empty);
        }

        public void Authorization()
        {
            if (isAutorized)
            {
                buttonLogout.Visible = true;              
                buttonLogin.Visible = false;
                buttonRegister.Visible = false;
                textBoxUserDetails.Text = userNameSurname;
                if (isAdmin)
                {
                    buttonProductsUsers.Visible = false;
                    buttonCart.Visible = false;

                    buttonAdminPanel.Visible = true;
                    buttonUsers.Visible = true;
                    buttonOrders.Visible = true;
                    buttonManufacturing.Visible = true;
                    buttonProducts.Visible = true;
                    buttonPrintingEquipment.Visible = true;
                    textBoxUserDetails.Text += "\r\nАдміністратор";
                    buttonOrders_Click(this, EventArgs.Empty);
                    return;
                }
                buttonMyOrders.Visible = true;
            }
            else
            {
                buttonLogout.Visible = false;
                buttonMyOrders.Visible = false;
                buttonLogin.Visible = true;
                buttonRegister.Visible = true;

                buttonProductsUsers.Visible = true;
                buttonCart.Visible = true;

                buttonAdminPanel.Visible = false;
                buttonUsers.Visible = false;
                buttonOrders.Visible = false;
                buttonManufacturing.Visible = false;
                buttonProducts.Visible = false;
                buttonPrintingEquipment.Visible = false;
                textBoxUserDetails.Text = "";
                autorizedUserID = null; userNameSurname = null;
            }
        }

        private void buttonProductsUsers_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedTab = tabPageProductsUsers;
            ProductsUsers(false);
            ProductsUsersClearSearch();
        }

        private void buttonProductsUsersSearch_Click(object sender, EventArgs e)
        {
            ProductsUsers(true);
        }

        private void buttonProductsUsersSearchClear_Click(object sender, EventArgs e)
        {
            ProductsUsersClearSearch();
            buttonProductsUsersSearch_Click(this, EventArgs.Empty);
        }

        private void ProductsUsersClearSearch()
        {
            textBoxProductsUsersSearchName.Text = "";
            textBoxProductsUsersSearchFormat.Text = "";
            comboBoxProductsUsersSearchPrintType.SelectedIndex = -1; 
            numericUpDownProductsUsersSearchCostFrom.Value = (decimal)0.50;
            numericUpDownProductsUsersSearchCostTo.Value = (decimal)1000.00;
        }

        private void ProductsUsers(bool isSearch)
        {
            dataGridViewProductsUsers.DataSource = null;
            dataGridViewProductsUsers.Columns.Clear();

            dataGridViewProductsUsers.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridViewProductsUsers.Columns[0].HeaderText = "Кількість";
            dataGridViewProductsUsers.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewProductsUsers.Columns[0].Width = 100;

            DataGridViewImageColumn iconColumn = new DataGridViewImageColumn();
            iconColumn.Image = Properties.Resources.Cart3;
            iconColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            iconColumn.Width = 40;
            dataGridViewProductsUsers.Columns.Add(iconColumn);
            DataSet ds = new DataSet();
            if (!isSearch)
            {
                string sql = "SELECT * FROM Продукція ORDER BY Найменування";
                ds = dbActions.FillDataSet(sql);
            }
            else
            {
                ds = dbActions.UsersProductSearch(textBoxProductsUsersSearchName.Text, numericUpDownProductsUsersSearchCostFrom.Value.ToString(CultureInfo.InvariantCulture),
                    numericUpDownProductsUsersSearchCostTo.Value.ToString(CultureInfo.InvariantCulture), textBoxProductsUsersSearchFormat.Text, comboBoxProductsUsersSearchPrintType.Text);
            }

            dataGridViewProductsUsers.DataSource = ds.Tables[0];
            
            dataGridViewProductsUsers.Columns[2].Visible = false;

            for (int i = 2; i <= 6; i++)
            {
                dataGridViewProductsUsers.Columns[i].ReadOnly = true;
            }

            dataGridViewProductsUsers.Columns[0].DisplayIndex = 6;
            dataGridViewProductsUsers.Columns[1].DisplayIndex = 6;
        }

        private void dataGridViewProductsUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0 && int.TryParse(dataGridViewProductsUsers.Rows[e.RowIndex].Cells[0].Value?.ToString(), out int quantity))
            {
                if (Int32.TryParse(dataGridViewProductsUsers.Rows[e.RowIndex].Cells[2].Value.ToString(), out int id))
                {
                    if (productsIDInCart.Contains(id))
                    {
                        productsQuantityInCart[productsIDInCart.IndexOf(id)] += quantity;
                        MessageBox.Show("Збільшено кількість товару у кошику!");
                    }
                    else
                    {
                        productsIDInCart.Add(id);
                        productsQuantityInCart.Add(quantity);
                        MessageBox.Show("Товар додано у кошик!");
                    }

                    dataGridViewProductsUsers.CellValueChanged -= dataGridViewProductsUsers_CellValueChanged;
                    dataGridViewProductsUsers.Rows[e.RowIndex].Cells[0].Value = "";
                    dataGridViewProductsUsers.CellValueChanged += dataGridViewProductsUsers_CellValueChanged;
                }              
            }
            else if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                MessageBox.Show("Введіть кількість товару!");
            }
        }

        private void dataGridViewProductsUsers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewProductsUsers.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Value == null || !int.TryParse(cell.Value.ToString(), out int result) || result <= 0)
                {
                    dataGridViewProductsUsers.CellValueChanged -= dataGridViewProductsUsers_CellValueChanged;
                    cell.Value = null;
                    MessageBox.Show("Некоректна кількість товару!");
                    dataGridViewProductsUsers.CellValueChanged += dataGridViewProductsUsers_CellValueChanged;
                }
            }
        }

        private void buttonCart_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedTab = tabPageCart;

            dataGridViewCart.DataSource = null;
            dataGridViewCart.Columns.Clear();

            if (productsIDInCart.Count > 0)
            {
                labelCart.Visible = false;
                buttonCartOrder.Visible = true;
                labelTotalAmount.Visible = true;
                dataGridViewCart.Visible = true;
                string ids = string.Join(",", productsIDInCart.Select(id => id.ToString()));
                string sql = $"SELECT * FROM Продукція WHERE [Код продукції] IN ({ids}) ORDER BY Найменування";

                DataSet ds = dbActions.FillDataSet(sql);
                dataGridViewCart.DataSource = ds.Tables[0];
                cartTotalAmount = 0;

                dataGridViewCart.Columns[0].Visible = false;
                dataGridViewCart.Columns.Add(new DataGridViewTextBoxColumn());
                dataGridViewCart.Columns[5].HeaderText = "Вартість";
                foreach (DataGridViewColumn column in dataGridViewCart.Columns)
                {
                    column.ReadOnly = true;
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                dataGridViewCart.Columns.Add(new DataGridViewTextBoxColumn());
                dataGridViewCart.Columns[6].HeaderText = "Кількість";
                dataGridViewCart.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridViewCart.Columns[6].Width = 100;
                DataGridViewImageColumn iconColumn = new DataGridViewImageColumn();
                iconColumn.Image = Properties.Resources.Delete;
                iconColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                iconColumn.Width = 40;
                dataGridViewCart.Columns.Add(iconColumn);               

                for (int i = 0; i < productsQuantityInCart.Count; i++)
                {
                    int id = Int32.Parse(dataGridViewCart.Rows[i].Cells[0].Value.ToString()); 
                    dataGridViewCart.Rows[i].Cells[5].Value = productsQuantityInCart[productsIDInCart.IndexOf(id)] * (decimal)dataGridViewCart.Rows[i].Cells[2].Value;
                    dataGridViewCart.Rows[i].Cells[6].Value = productsQuantityInCart[productsIDInCart.IndexOf(id)];
                    cartTotalAmount += (decimal)dataGridViewCart.Rows[i].Cells[5].Value;
                }

                labelTotalAmount.Text = $"Сума замовлення {cartTotalAmount} грн.";
            }
            else
            {
                labelCart.Visible = true;
                dataGridViewCart.Visible = false;
                labelTotalAmount.Visible = false;
                buttonCartOrder.Visible = false;
            }
        }

        private void dataGridViewCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 7 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewCart.Rows[e.RowIndex].Cells[0];
                if (int.TryParse(cell.Value.ToString(), out int id))
                {
                    cartTotalAmount -= (decimal)dataGridViewCart.Rows[e.RowIndex].Cells[5].Value;
                    labelTotalAmount.Text = $"Сума замовлення {cartTotalAmount} грн.";
                    int index = productsIDInCart.IndexOf(id);
                    productsIDInCart.RemoveAt(index);
                    productsQuantityInCart.RemoveAt(index);
                    dataGridViewCart.Rows.RemoveAt(e.RowIndex);
                }
                if (dataGridViewCart.RowCount == 0)
                {
                    labelCart.Visible = true;
                    dataGridViewCart.Visible = false;
                    labelTotalAmount.Visible = false;
                    buttonCartOrder.Visible = false;
                }
            }       
        }

        private void dataGridViewCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6 && e.RowIndex >= 0)
            {
                DataGridViewCell cell1 = dataGridViewCart.Rows[e.RowIndex].Cells[e.ColumnIndex];
                DataGridViewCell cell2 = dataGridViewCart.Rows[e.RowIndex].Cells[0];
                int.TryParse(cell2.Value.ToString(), out int id);
                if (cell1.Value != null && int.TryParse(cell1.Value.ToString(), out int quantity) && quantity > 0)
                {
                    cartTotalAmount -= (decimal)dataGridViewCart.Rows[e.RowIndex].Cells[5].Value;
                    productsQuantityInCart[productsIDInCart.IndexOf(id)] = quantity;
                    dataGridViewCart.Rows[e.RowIndex].Cells[5].Value = quantity * (decimal)dataGridViewCart.Rows[e.RowIndex].Cells[2].Value;
                    cartTotalAmount += (decimal)dataGridViewCart.Rows[e.RowIndex].Cells[5].Value;
                    labelTotalAmount.Text = $"Сума замовлення {cartTotalAmount} грн.";
                }
                else
                {                   
                    dataGridViewCart.CellValueChanged -= dataGridViewCart_CellValueChanged;
                    MessageBox.Show("Некоректна кількість!");
                    cell1.Value = productsQuantityInCart[productsIDInCart.IndexOf(id)];                 
                    dataGridViewCart.CellValueChanged += dataGridViewCart_CellValueChanged;
                }
            }
        }

        private void buttonMyOrders_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedTab = tabPageMyOrders;

            dataGridViewMyOrders.DataSource = null;
            dataGridViewMyOrders.Columns.Clear();

            dataGridViewMyOrdersDetails.DataSource = null;
            dataGridViewMyOrdersDetails.Columns.Clear();

            string sql = "SELECT [Код замовлення], [Дата замовлення], [Дата виконання], [Статус замовлення], " +
                "[Сума], (SELECT COUNT(*) FROM Виробництво WHERE Виробництво.[Код замовлення] = Замовлення.[Код замовлення]) AS[Кількість товарів] " +
                "FROM Замовлення INNER JOIN Користувачі ON Замовлення.[Код користувача] = Користувачі.[Код користувача]" +
                $"WHERE Замовлення.[Код користувача] = {autorizedUserID}";

            DataSet ds = dbActions.FillDataSet(sql);
            dataGridViewMyOrders.DataSource = ds.Tables[0];
        }

        private void dataGridViewMyOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewMyOrdersDetails.DataSource = null;
            dataGridViewMyOrdersDetails.Columns.Clear();
            if (e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewMyOrders.Rows[e.RowIndex].Cells[0];
                if (int.TryParse(cell.Value.ToString(), out int id))
                {
                    string sql = "SELECT Продукція.Найменування,Виробництво.[Кількість екземплярів]," +
                        "Виробництво.[Вартість],Виробництво.[Статус виробництва] FROM Виробництво " +
                        $"LEFT JOIN Продукція ON Виробництво.[Код продукції] = Продукція.[Код продукції] WHERE Виробництво.[Код замовлення] = {id}";

                    DataSet ds = dbActions.FillDataSet(sql);
                    dataGridViewMyOrdersDetails.DataSource = ds.Tables[0];
                }            
            }                
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            isAdmin = false;
            isAutorized = false;
            productsIDInCart.Clear();
            productsQuantityInCart.Clear();
            Authorization();
            tabControlAdmin.SelectedTab = tabPageProductsUsers;
        }

        public void buttonUsers_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedTab = tabPageUsers;
            UsersSearchClear();
            Users();
        }

        private void Users(string sqlAdd = null)
        {
            dataGridViewUsers.DataSource = null;
            dataGridViewUsers.Columns.Clear();
            AddEditDeleteColumns(dataGridViewUsers);
            string sql = "SELECT * FROM Користувачі" + sqlAdd;

            DataSet ds = dbActions.FillDataSet(sql);
            dataGridViewUsers.DataSource = ds.Tables[0];
            dataGridViewUsers.Columns[2].Visible = false;
            dataGridViewUsers.Columns[7].Visible = false;

            dataGridViewUsers.Columns[0].DisplayIndex = 8;
            dataGridViewUsers.Columns[1].DisplayIndex = 8;
        }

        private void UsersSearchClear()
        {
            textBoxUsersSearchName.Text = "";
            textBoxUsersSearchSurname.Text = "";
            textBoxUsersSearchPatronymic.Text = "";
            textBoxUsersSearchEmail.Text = "";
            comboBoxUsersSearchRole.SelectedIndex = -1;
        }

        private void buttonUsersSearch_Click(object sender, EventArgs e)
        {
            string sql = $" WHERE [Ім'я] LIKE '%{textBoxUsersSearchName.Text}%' AND " +
                $"[Прізвище] LIKE '%{textBoxUsersSearchSurname.Text}%' AND " +
                $"[По-батькові] LIKE '%{textBoxUsersSearchPatronymic.Text}%' AND " +
                $"[Електронна пошта] LIKE '%{textBoxUsersSearchEmail.Text}%' AND " +             
                $"Роль LIKE '%{comboBoxUsersSearchRole.Text}%'";
            Users(sql);
        }

        private void buttonUsersSearchClear_Click(object sender, EventArgs e)
        {
            UsersSearchClear();
            Users();
        }

        private void AddEditDeleteColumns(DataGridView dataGridView)
        {
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.ReadOnly = true;
            }

            DataGridViewImageColumn iconColumn1 = new DataGridViewImageColumn();
            iconColumn1.Image = Properties.Resources.Edit;
            iconColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            iconColumn1.Width = 40;
            dataGridView.Columns.Add(iconColumn1);
            DataGridViewImageColumn iconColumn2 = new DataGridViewImageColumn();
            iconColumn2.Image = Properties.Resources.Delete;
            iconColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            iconColumn2.Width = 40;
            dataGridView.Columns.Add(iconColumn2);
        }

        private void dataGridViewUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewUsers.Rows[e.RowIndex].Cells[2];
                if (int.TryParse(cell.Value.ToString(), out int id))
                {

                    FormEdit formEdit = (FormEdit)Application.OpenForms["FormRegistryLogin"];
                    formEdit = new FormEdit(this);
                    formEdit.EditUser(id);
                    formEdit.Show();
                }
            }
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                bool selfDelete = false;
                if (int.TryParse(dataGridViewUsers.Rows[e.RowIndex].Cells[2].Value.ToString(), out int id))
                {
                    if (dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM Замовлення WHERE [Код користувача] = {id}"))
                    {
                        MessageBox.Show("Спочатку видаліть всі замовлення користувача!");
                        return;
                    }
                    if ((string)dataGridViewUsers.Rows[e.RowIndex].Cells[8].Value == "Адміністратор")
                    {
                        int adminCount = (int)dbActions.ReadData("SELECT COUNT(*) FROM Користувачі WHERE Роль = 'Адміністратор'", 1)[0];
                        if (adminCount == 1)
                        {
                            MessageBox.Show("Не може бути менше одного адміністратора!");
                            return;
                        }
                    }
                    if (id == autorizedUserID) selfDelete = true;
                }
                if (DeleteRecord(dataGridViewUsers.Rows[e.RowIndex].Cells[2], "Користувачі", "[Код користувача]", $"Видалити користувача {dataGridViewUsers.Rows[e.RowIndex].Cells[3].Value} {dataGridViewUsers.Rows[e.RowIndex].Cells[4].Value}?"))
                {
                    if (selfDelete)
                    {
                        isAutorized = false;
                        Authorization();
                        buttonProductsUsers_Click(this, EventArgs.Empty);
                        return;
                    }
                    buttonUsers_Click(this, EventArgs.Empty);
                }                
            }
        }

        private bool DeleteRecord(DataGridViewCell cellId, string tableName, string codeName, string confirmationMessage)
        {
            if (int.TryParse(cellId.Value.ToString(), out int id))
            {
                DialogResult result = MessageBox.Show(confirmationMessage, "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    string sql = $"DELETE FROM {tableName} WHERE {codeName} = {id}";
                    dbActions.ExecuteQuery(sql);
                    return true;
                }              
            }
            return false;
        }

        public void buttonOrders_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedTab = tabPageOrders;
            OrderSearchClear();
            Orders();
        }

        private void buttonOrdersSearchClear_Click(object sender, EventArgs e)
        {
            OrderSearchClear();
            Orders();
        }

        private void Orders(string sqlAdd = null)
        {
            dataGridViewOrders.DataSource = null;
            dataGridViewOrders.Columns.Clear();
            AddEditDeleteColumns(dataGridViewOrders);
            string sql = "SELECT [Код замовлення], Користувачі.[Електронна пошта], [Дата замовлення], [Дата виконання], [Статус замовлення], " +
                "[Сума], (SELECT COUNT(*) FROM Виробництво WHERE Виробництво.[Код замовлення] = Замовлення.[Код замовлення]) AS [Кількість товарів]" +
                "FROM Замовлення INNER JOIN Користувачі ON Замовлення.[Код користувача] = Користувачі.[Код користувача]" + sqlAdd;
            DataSet ds = dbActions.FillDataSet(sql);
            dataGridViewOrders.DataSource = ds.Tables[0];

            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
            cellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cellStyle.Font = new Font("Candara", 10, FontStyle.Underline);
            dataGridViewOrders.Columns[3].DefaultCellStyle = cellStyle;
            dataGridViewOrders.Columns[8].DefaultCellStyle = cellStyle;
            dataGridViewOrders.Columns[0].DisplayIndex = 8;
            dataGridViewOrders.Columns[1].DisplayIndex = 8;
        }

        private void buttonOrdersReport_Click(object sender, EventArgs e)
        {
            string sql = "SELECT Замовлення.[Код замовлення], Користувачі.[Електронна пошта], CONVERT(varchar, Замовлення.[Дата замовлення], 104) AS [Дата замовлення], CONVERT(varchar, Замовлення.[Дата виконання], 104) AS [Дата виконання], Замовлення.[Статус замовлення], " +
                "Замовлення.[Сума], (SELECT COUNT(*) FROM Виробництво WHERE Виробництво.[Код замовлення] = Замовлення.[Код замовлення]) AS [Кількість товарів]" +
                "FROM Замовлення INNER JOIN Користувачі ON Замовлення.[Код користувача] = Користувачі.[Код користувача]" + OrdersSearch();
            dbActions.Report(sql);        
        }

        private void buttonOrderSearch_Click(object sender, EventArgs e)
        {
            string sqlAdd = OrdersSearch();
            if (sqlAdd != null) Orders(sqlAdd);
        }

        private string OrdersSearch()
        {
            string sql = " WHERE ";
            if (!string.IsNullOrWhiteSpace(textBoxOrderSearchId.Text))
            {
                sql += $"[Код замовлення] = {Int32.Parse(textBoxOrderSearchId.Text)} AND ";
            }
            sql += $"Користувачі.[Електронна пошта] LIKE '%{textBoxOrderSearchEmail.Text}%' AND ";
            if (!validation.ValidationOrderManufacturing(maskedTextBoxOrderSearchOrderDateFrom.Text, maskedTextBoxOrderSearchOrderDateTo.Text, false, false))
            {
                return null;
            }
            if (!validation.ValidationOrderManufacturing(maskedTextBoxOrderSearchExecutionFrom.Text, maskedTextBoxOrderSearchExecutionTo.Text, false, false))
            {
                return null;
            }

            if (maskedTextBoxOrderSearchOrderDateFrom.Text.Any(char.IsDigit))
            {
                DateTime.TryParseExact(maskedTextBoxOrderSearchOrderDateFrom.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date1);
                sql += $"[Дата замовлення] >= '{date1}' AND ";
            }
            if (maskedTextBoxOrderSearchOrderDateTo.Text.Any(char.IsDigit))
            {
                DateTime.TryParseExact(maskedTextBoxOrderSearchOrderDateTo.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date2);
                sql += $"[Дата замовлення] <= '{date2}' AND ";
            }
            if (maskedTextBoxOrderSearchExecutionFrom.Text.Any(char.IsDigit))
            {
                DateTime.TryParseExact(maskedTextBoxOrderSearchExecutionFrom.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date3);
                sql += $"[Дата виконання] >= '{date3}' AND ";
            }
            if (maskedTextBoxOrderSearchExecutionTo.Text.Any(char.IsDigit))
            {
                DateTime.TryParseExact(maskedTextBoxOrderSearchExecutionTo.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date4);
                sql += $"[Дата виконання] <= '{date4}' AND ";
            }
            sql += $"[Статус замовлення] LIKE '%{comboBoxOrderSearchOrderStatus.Text}%' AND " +
                $"[Сума] >= {numericUpDownOrderSearchTotalAmountFrom.Value.ToString(CultureInfo.InvariantCulture)} AND " +
                $"[Сума] <= {numericUpDownOrderSearchTotalAmountTo.Value.ToString(CultureInfo.InvariantCulture)} AND" +
                "(SELECT COUNT(*) FROM Виробництво WHERE Виробництво.[Код замовлення] = Замовлення.[Код замовлення]) " +
                $"BETWEEN {numericUpDownOrderSearchCountProdFrom.Value} AND {numericUpDownOrderSearchCountProdTo.Value}";
            return sql;
        }

        private void OrderSearchClear()
        {
            textBoxOrderSearchId.Text = "";
            textBoxOrderSearchEmail.Text = "";
            maskedTextBoxOrderSearchOrderDateFrom.Text = "";
            maskedTextBoxOrderSearchOrderDateTo.Text = "";
            maskedTextBoxOrderSearchExecutionFrom.Text = "";
            maskedTextBoxOrderSearchExecutionTo.Text = "";
            comboBoxOrderSearchOrderStatus.SelectedIndex = -1;
            numericUpDownOrderSearchTotalAmountFrom.Value = (decimal)0.00;
            numericUpDownOrderSearchTotalAmountTo.Value = (decimal)99999.00;
            numericUpDownOrderSearchCountProdFrom.Value = 0;
            numericUpDownOrderSearchCountProdTo.Value = 100;
        }

        private void textBoxOrderSearchId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void dataGridViewOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                UsersSearchClear();
                textBoxUsersSearchEmail.Text = dataGridViewOrders.Rows[e.RowIndex].Cells[3].Value.ToString();
                buttonUsersSearch_Click(this, EventArgs.Empty);
                tabControlAdmin.SelectedTab = tabPageUsers;
            }
            if (e.ColumnIndex == 8 && e.RowIndex >= 0 && dataGridViewOrders.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() != "0")
            {
                DataGridViewCell cell = dataGridViewOrders.Rows[e.RowIndex].Cells[2];
                if (int.TryParse(cell.Value.ToString(), out int id))
                {                   
                    ManufacturingClearSearch();
                    textBoxManufacturingSearchId.Text = id.ToString();
                    buttonManufacturingSearch_Click(this, EventArgs.Empty);
                    tabControlAdmin.SelectedTab = tabPageManufacturing;
                }
            }
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewOrders.Rows[e.RowIndex].Cells[2];
                if (int.TryParse(cell.Value.ToString(), out int id))
                {

                    FormEdit formEdit = (FormEdit)Application.OpenForms["FormRegistryLogin"];
                    formEdit = new FormEdit(this);
                    formEdit.EditOrder(id);
                    formEdit.Show();
                }
            }
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (int.TryParse(dataGridViewOrders.Rows[e.RowIndex].Cells[2].Value.ToString(), out int id))
                {
                    if (dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM Виробництво WHERE [Код замовлення] = {id}"))
                    {
                        MessageBox.Show("Спочатку видаліть всі товари на виробництві цього замовлення!");
                        return;
                    }
                }
                if (DeleteRecord(dataGridViewOrders.Rows[e.RowIndex].Cells[2], "Замовлення", "[Код замовлення]", $"Видалити замовлення №{dataGridViewOrders.Rows[e.RowIndex].Cells[2].Value}?"))
                buttonOrders_Click(this, EventArgs.Empty);
            }
        }

        public void buttonManufacturing_Click(object sender, EventArgs e)
        {
            ManufacturingClearSearch();
            tabControlAdmin.SelectedTab = tabPageManufacturing;
            Manufacturing();
        }

        private void buttonManufacturingSearchClear_Click(object sender, EventArgs e)
        {
            ManufacturingClearSearch();
            Manufacturing();
        }

        private void Manufacturing(string sqlAdd = null)
        {        
            dataGridViewManufacturing.DataSource = null;
            dataGridViewManufacturing.Columns.Clear();
            AddEditDeleteColumns(dataGridViewManufacturing);

            string sql = "SELECT [Код виробництва],[Код замовлення],Продукція.Найменування,[Друкарське устаткування].Найменування,[Кількість екземплярів]," +
                "[Дата початку],[Дата закінчення],[Вартість],[Статус виробництва] FROM Виробництво " +
                "LEFT JOIN Продукція ON Виробництво.[Код продукції] = Продукція.[Код продукції] " +
                "LEFT JOIN[Друкарське устаткування] ON Виробництво.[Код устаткування]=[Друкарське устаткування].[Код устаткування] " + sqlAdd;
            DataSet ds = dbActions.FillDataSet(sql);
            dataGridViewManufacturing.DataSource = ds.Tables[0];
            dataGridViewManufacturing.Columns[2].Visible = false;
            dataGridViewManufacturing.Columns[4].HeaderText = "Товар";
            dataGridViewManufacturing.Columns[5].HeaderText = "Устаткування";
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
            cellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cellStyle.Font = new Font("Candara", 10, FontStyle.Underline);
            dataGridViewManufacturing.Columns[3].DefaultCellStyle = cellStyle;
            dataGridViewManufacturing.Columns[4].DefaultCellStyle = cellStyle;
            dataGridViewManufacturing.Columns[5].DefaultCellStyle = cellStyle;

            dataGridViewManufacturing.Columns[0].DisplayIndex = 10;
            dataGridViewManufacturing.Columns[1].DisplayIndex = 10;
        }

        private void buttonManufacturingSearch_Click(object sender, EventArgs e)
        {
            string sqlAdd = ManufacturingSearch();
            if (sqlAdd != null) Manufacturing(sqlAdd);
        }

        private string ManufacturingSearch()
        {
            string sql = "WHERE ";
            if (!string.IsNullOrWhiteSpace(textBoxManufacturingSearchId.Text))
            {
                sql += $"[Код замовлення] = {Int32.Parse(textBoxManufacturingSearchId.Text)} AND ";
            }
            sql += $"Продукція.Найменування LIKE '%{textBoxManufacturingSearchProduct.Text}%' AND ";
            if (!string.IsNullOrEmpty(textBoxManufacturingSearchPrintingEquipment.Text))
            {
                sql += $"[Друкарське устаткування].Найменування LIKE '%{textBoxManufacturingSearchPrintingEquipment.Text}%' AND ";
            }
            sql += $"[Кількість екземплярів] >= {numericUpDownManufacturingSearchNumberOfCopiesFrom.Text} AND " +
                $"[Кількість екземплярів] <= {numericUpDownManufacturingSearchNumberOfCopiesTo.Text} AND ";

            if (!validation.ValidationOrderManufacturing(maskedTextBoxManufacturingSearchStartDateFrom.Text, maskedTextBoxManufacturingSearchStartDateTo.Text, false, false))
            {
                return null;
            }
            if (!validation.ValidationOrderManufacturing(maskedTextBoxManufacturingSearchEndDateFrom.Text, maskedTextBoxManufacturingSearchEndDateTo.Text, false, false))
            {
                return null;
            }

            if (maskedTextBoxManufacturingSearchStartDateFrom.Text.Any(char.IsDigit))
            {
                DateTime.TryParseExact(maskedTextBoxManufacturingSearchStartDateFrom.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
                sql += $"[Дата початку] >= '{date}' AND ";
            }
            if (maskedTextBoxManufacturingSearchStartDateTo.Text.Any(char.IsDigit))
            {
                DateTime.TryParseExact(maskedTextBoxManufacturingSearchStartDateTo.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
                sql += $"[Дата початку] <= '{date}' AND ";
            }
            if (maskedTextBoxManufacturingSearchEndDateFrom.Text.Any(char.IsDigit))
            {
                DateTime.TryParseExact(maskedTextBoxManufacturingSearchEndDateFrom.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
                sql += $"[Дата закінчення] >= '{date}' AND ";
            }
            if (maskedTextBoxManufacturingSearchEndDateTo.Text.Any(char.IsDigit))
            {
                DateTime.TryParseExact(maskedTextBoxManufacturingSearchEndDateTo.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
                sql += $"[Дата закінчення] <= '{date}' AND ";
            }
            sql += $"[Вартість] >= {numericUpDownManufacturingSearchCostFrom.Value.ToString(CultureInfo.InvariantCulture)} AND " +
                $"[Вартість] <= {numericUpDownManufacturingSearchCostTo.Value.ToString(CultureInfo.InvariantCulture)} AND " +
                $"[Статус виробництва] LIKE '%{comboBoxManufacturingSearchStatus.Text}%'";
            return sql;
        }

        private void buttonManufacturingReport_Click(object sender, EventArgs e)
        {
            string sql = "SELECT Виробництво.[Код замовлення],Продукція.Найменування AS Товар,[Друкарське устаткування].Найменування AS Устаткування,Виробництво.[Кількість екземплярів]," +
                "CONVERT(varchar, Виробництво.[Дата початку], 104) AS [Дата початку],CONVERT(varchar, Виробництво.[Дата закінчення], 104) AS [Дата закінчення],Виробництво.[Вартість],Виробництво.[Статус виробництва] FROM Виробництво " +
                "LEFT JOIN Продукція ON Виробництво.[Код продукції] = Продукція.[Код продукції] " +
                "LEFT JOIN[Друкарське устаткування] ON Виробництво.[Код устаткування]=[Друкарське устаткування].[Код устаткування] " + ManufacturingSearch();
            dbActions.Report(sql);
        }

        private void ManufacturingClearSearch()
        {
            textBoxManufacturingSearchId.Text = "";
            textBoxManufacturingSearchProduct.Text = "";
            textBoxManufacturingSearchPrintingEquipment.Text = "";
            maskedTextBoxManufacturingSearchStartDateFrom.Text = "";
            maskedTextBoxManufacturingSearchStartDateTo.Text = "";
            maskedTextBoxManufacturingSearchEndDateFrom.Text = "";
            maskedTextBoxManufacturingSearchEndDateTo.Text = "";
            numericUpDownManufacturingSearchNumberOfCopiesFrom.Value = 1;
            numericUpDownManufacturingSearchNumberOfCopiesTo.Value = 100000;
            numericUpDownManufacturingSearchCostFrom.Value = (decimal)0.50;
            numericUpDownManufacturingSearchCostTo.Value = (decimal)10000.00;
            comboBoxManufacturingSearchStatus.SelectedIndex = -1;
        }

        private void dataGridViewManufacturing_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewManufacturing.Rows[e.RowIndex].Cells[3];
                if (int.TryParse(cell.Value.ToString(), out int id))
                {
                    OrderSearchClear();
                    textBoxOrderSearchId.Text = id.ToString();
                    buttonOrderSearch_Click(this, EventArgs.Empty);
                    tabControlAdmin.SelectedTab = tabPageOrders;
                }
            }
            if (e.ColumnIndex == 4 && e.RowIndex >= 0)
            {
                ProductsSearchClear();
                textBoxProductSearchName.Text = dataGridViewManufacturing.Rows[e.RowIndex].Cells[4].Value.ToString();
                buttonProductSearch_Click(this, EventArgs.Empty);
                tabControlAdmin.SelectedTab = tabPageProducts;
            }
            if (e.ColumnIndex == 5 && e.RowIndex >= 0)
            {
                PrintingEquipmentSearchClear();
                textBoxPrintingEquipmentSearchName.Text = dataGridViewManufacturing.Rows[e.RowIndex].Cells[5].Value.ToString();
                buttonPrintingEquipmentSearch_Click(this, EventArgs.Empty);
                tabControlAdmin.SelectedTab = tabPagePrintingEquipment;
            }
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewManufacturing.Rows[e.RowIndex].Cells[2];
                if (int.TryParse(cell.Value.ToString(), out int id))
                {

                    FormEdit formEdit = (FormEdit)Application.OpenForms["FormRegistryLogin"];
                    formEdit = new FormEdit(this);
                    formEdit.EditManufacturing(id);
                    formEdit.Show();
                }
            }
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (DeleteRecord(dataGridViewManufacturing.Rows[e.RowIndex].Cells[2], "Виробництво", "[Код виробництва]", $"Видалити виробництво {dataGridViewManufacturing.Rows[e.RowIndex].Cells[4].Value} замовлення {dataGridViewManufacturing.Rows[e.RowIndex].Cells[3].Value}?"))
                buttonManufacturing_Click(this, EventArgs.Empty);
            }
        }

        public void buttonProducts_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedTab = tabPageProducts;
            ProductsSearchClear();
            Products();
        }

        private void buttonProductsSearchClear_Click(object sender, EventArgs e)
        {
            ProductsSearchClear();
            Products();
        }

        private void Products(string sqlAdd = "ORDER BY Найменування")
        {
            dataGridViewProducts.DataSource = null;
            dataGridViewProducts.Columns.Clear();
            AddEditDeleteColumns(dataGridViewProducts);

            string sql = "SELECT * FROM Продукція " + sqlAdd;
            DataSet ds = dbActions.FillDataSet(sql);
            dataGridViewProducts.DataSource = ds.Tables[0];
            dataGridViewProducts.Columns[2].Visible = false;

            dataGridViewProducts.Columns[0].DisplayIndex = 6;
            dataGridViewProducts.Columns[1].DisplayIndex = 6;
        }

        private void ProductsSearchClear()
        {
            textBoxProductSearchName.Text = "";
            textBoxProductSearchFormat.Text = "";
            comboBoxProductSearchPrintType.SelectedIndex = -1;
            numericUpDownProductSearchCostFrom.Value = (decimal)0.50;
            numericUpDownProductSearchCostTo.Value = (decimal)1000.00;
        }

        private void buttonProductSearch_Click(object sender, EventArgs e)
        {
            string sql = $"WHERE [Найменування] LIKE '%{textBoxProductSearchName.Text}%' AND " +
              $"[Ціна за одиницю] >= {numericUpDownProductSearchCostFrom.Value.ToString(CultureInfo.InvariantCulture)} AND " +
              $"[Ціна за одиницю] <= {numericUpDownProductSearchCostTo.Value.ToString(CultureInfo.InvariantCulture)} AND " +
              $"[Формат] LIKE '%{textBoxProductSearchFormat.Text}%' AND " +
              $"[Вид продукції] LIKE '%{comboBoxProductSearchPrintType.Text}%' " +
              "ORDER BY Найменування";

            Products(sql);
        }

        public void buttonPrintingEquipment_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedTab = tabPagePrintingEquipment;
            PrintingEquipmentSearchClear();
            PrintingEquipment();
        }

        private void PrintingEquipmentSearchClear()
        {
            textBoxPrintingEquipmentSearchName.Text = "";
            textBoxPrintingEquipmentSearchType.Text = "";
            comboBoxPrintingEquipmentSearchTechCondition.SelectedIndex = -1;
            numericUpDownPrintingEquipmentSearchReleaseYearFrom.Value = 2000;
            numericUpDownPrintingEquipmentSearchReleaseYearTo.Value = 2024;
            numericUpDownPrintingEquipmentSearchPrintSpeedFrom.Value = 100;
            numericUpDownPrintingEquipmentSearchPrintSpeedTo.Value = 100000;
        }

        private void PrintingEquipment(string sqlAdd = "ORDER BY Найменування")
        {
            dataGridViewPrintingEquipment.DataSource = null;
            dataGridViewPrintingEquipment.Columns.Clear();
            AddEditDeleteColumns(dataGridViewPrintingEquipment);

            string sql = "SELECT * FROM [Друкарське устаткування] " + sqlAdd; 
            DataSet ds = dbActions.FillDataSet(sql);
            dataGridViewPrintingEquipment.DataSource = ds.Tables[0];
            dataGridViewPrintingEquipment.Columns[2].Visible = false;

            dataGridViewPrintingEquipment.Columns[0].DisplayIndex = 7;
            dataGridViewPrintingEquipment.Columns[1].DisplayIndex = 7;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void dataGridViewProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewProducts.Rows[e.RowIndex].Cells[2];
                if (int.TryParse(cell.Value.ToString(), out int id))
                {
                    FormEdit formEdit = (FormEdit)Application.OpenForms["FormRegistryLogin"];
                    formEdit = new FormEdit(this);
                    formEdit.EditProduct(id);
                    formEdit.Show();
                }
            }
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (int.TryParse(dataGridViewProducts.Rows[e.RowIndex].Cells[2].Value.ToString(), out int id))
                {
                    if (dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM Виробництво WHERE [Код продукції] = {id}"))
                    {
                        MessageBox.Show("Цей товар є на виробництві, неможливо видалити! Спочатку приберіть посилання на цей товар у виробництві");
                        return;
                    }
                }
                if (DeleteRecord(dataGridViewProducts.Rows[e.RowIndex].Cells[2], "Продукція", "[Код продукції]", $"Видалити {dataGridViewProducts.Rows[e.RowIndex].Cells[3].Value}?"))
                buttonProducts_Click(this, EventArgs.Empty);
            }
        }

        private void buttonCreateProduct_Click(object sender, EventArgs e)
        {
            FormEdit formEdit = (FormEdit)Application.OpenForms["FormRegistryLogin"];
            formEdit = new FormEdit(this);
            formEdit.CreateProduct();
            formEdit.Show();
        }

        private void buttonCreatePrintingEquipment_Click(object sender, EventArgs e)
        {
            FormEdit formEdit = (FormEdit)Application.OpenForms["FormRegistryLogin"];
            formEdit = new FormEdit(this);
            formEdit.CreatePrintingEquipment();
            formEdit.Show();
        }

        private void buttonPrintingEquipmentSearch_Click(object sender, EventArgs e)
        {
            string sql = $" WHERE [Найменування] LIKE '%{textBoxPrintingEquipmentSearchName.Text}%' AND " +
                $"Тип LIKE '%{textBoxPrintingEquipmentSearchType.Text}%' AND " +
                $"[Рік випуску] >= {numericUpDownPrintingEquipmentSearchReleaseYearFrom.Value.ToString(CultureInfo.InvariantCulture)} AND " +
                $"[Рік випуску] <= {numericUpDownPrintingEquipmentSearchReleaseYearTo.Value.ToString(CultureInfo.InvariantCulture)} AND " +
                $"[Технічний стан] LIKE '%{comboBoxPrintingEquipmentSearchTechCondition.Text}%' AND " +
                $"[Швидкість друку] >= {numericUpDownPrintingEquipmentSearchPrintSpeedFrom.Value.ToString(CultureInfo.InvariantCulture)} AND " +
                $"[Швидкість друку] <= {numericUpDownPrintingEquipmentSearchPrintSpeedTo.Value.ToString(CultureInfo.InvariantCulture)} " +
                "ORDER BY Найменування";
            PrintingEquipment(sql);
        }

        private void buttonPrintingEquipmentSearchClear_Click(object sender, EventArgs e)
        {
            PrintingEquipmentSearchClear();
            PrintingEquipment();
        }

        private void dataGridViewPrintingEquipment_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = dataGridViewPrintingEquipment.Rows[e.RowIndex].Cells[2]; 
                if (int.TryParse(cell.Value.ToString(), out int id))
                {

                    FormEdit formEdit = (FormEdit)Application.OpenForms["FormRegistryLogin"];
                    formEdit = new FormEdit(this);
                    formEdit.EditPrintingEquipment(id);
                    formEdit.Show();
                }
            }
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (int.TryParse(dataGridViewPrintingEquipment.Rows[e.RowIndex].Cells[2].Value.ToString(), out int id))
                {
                    if (dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM Виробництво WHERE [Код устаткування] = {id}"))
                    {
                        MessageBox.Show("Це устаткування використовується на виробництві, неможливо видалити! Спочатку приберіть посилання на це устаткування у виробництві");
                        return;
                    }
                }
                if (DeleteRecord(dataGridViewPrintingEquipment.Rows[e.RowIndex].Cells[2], "[Друкарське устаткування]", "[Код устаткування]", $"Видалити {dataGridViewPrintingEquipment.Rows[e.RowIndex].Cells[3].Value}?"))
                buttonPrintingEquipment_Click(this, EventArgs.Empty);
            }
        }

        private void buttonCartOrder_Click(object sender, EventArgs e)
        {
            if (!isAutorized)
            {
                MessageBox.Show("Спочатку увійдіть в акаунт!");
                buttonLogin_Click(this, EventArgs.Empty);
                return;
            }

            string sqlOrder = "INSERT INTO Замовлення ([Код користувача], [Дата замовлення], [Статус замовлення], [Сума]) " +
                    $"VALUES ({autorizedUserID}, CONVERT(date, GETDATE()), 'В обробці', {cartTotalAmount.ToString(CultureInfo.InvariantCulture)}); " +
                     "SELECT SCOPE_IDENTITY();";
            if (dbActions.Transaction(sqlOrder, productsIDInCart, productsQuantityInCart))
            {
                MessageBox.Show("Успішно замовлено!");
                productsIDInCart.Clear();
                productsQuantityInCart.Clear();
                buttonMyOrders_Click(this, EventArgs.Empty);
            }
            else
            {
                MessageBox.Show("Помилка, спробуйте знов");
            }
        }

        private void panelTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                offsetX = e.X;
                offsetY = e.Y;
            }
        }

        private void panelTop_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void panelTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - offsetX;
                this.Top += e.Y - offsetY;
            }
        }

        private void OpenRegistryLoginForm(bool isRegister)
        {
            FormRegistryLogin formreg = (FormRegistryLogin)Application.OpenForms["FormRegistryLogin"];
            if (formreg == null)
            {
                formreg = new FormRegistryLogin(this);
                formreg.Registry = isRegister;
                formreg.Show();
            }
            else
            {
                formreg.Registry = isRegister;
                formreg.Activate();
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            OpenRegistryLoginForm(false);
        }

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            OpenRegistryLoginForm(true);
        }

        private void buttonMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
