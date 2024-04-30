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
    public partial class FormEdit : Form
    {
        private FormMain mainForm;
        DbActions dbActions = new DbActions();
        Validation validation = new Validation();
        private bool isDragging = false; private int offsetX, offsetY;

        public FormEdit(FormMain mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
        }

        public void EditUser(int id)
        {
            tabControlEdit.SelectedTab = tabPageUser;

            string sql = $"SELECT [Ім'я], Прізвище, [По-батькові], [Електронна пошта], Роль FROM Користувачі WHERE [Код користувача] = {id}";
            object[] data = dbActions.ReadData(sql, 5);
            buttonConfirmUser.Tag = id;
            textBoxName.Text = (string)data[0];
            textBoxSurname.Text = (string)data[1];
            textBoxPatronymic.Text = (string)data[2];
            textBoxEmail.Text = (string)data[3];
            comboBoxRole.SelectedItem = data[4];
            this.Height = 556;
        }

        public void CreateProduct()
        {
            tabControlEdit.SelectedTab = tabPageProduct;
            labelProduct.Text = "Створення товару"; labelProduct.Tag = "Create";
            comboBoxProductType.SelectedIndex = 0;
            this.Height = 499;
        }

        public void CreatePrintingEquipment()
        {
            tabControlEdit.SelectedTab = tabPagePrintingEquipment;
            labelPrintingEquipment.Text = "Створення друкарського устаткування"; labelPrintingEquipment.Tag = "Create";
            comboBoxTechnicalCondition.SelectedIndex = 2;
            this.Height = 564;
        }

        public void EditProduct(int id)
        {
            tabControlEdit.SelectedTab = tabPageProduct;
            labelProduct.Text = "Редагування товару"; labelProduct.Tag = "Edit";
            string sql = $"SELECT [Найменування], [Ціна за одиницю], [Формат], [Вид продукції] FROM Продукція WHERE [Код продукції] = {id}";
            object[] data = dbActions.ReadData(sql, 4);
            buttonConfirmProduct.Tag = id;
            textBoxProductName.Text = (string)data[0];
            numericUpDownUnitPrice.Value = (decimal)data[1];
            textBoxFormat.Text = (string)data[2];
            comboBoxProductType.SelectedItem = data[3];
            this.Height = 499;
        }

        public void EditPrintingEquipment(int id)
        {
            tabControlEdit.SelectedTab = tabPagePrintingEquipment;
            labelPrintingEquipment.Text = "Редагування друкарського устаткування"; labelPrintingEquipment.Tag = "Edit";

            string sql = $"SELECT [Найменування], [Тип], [Рік випуску], [Технічний стан], [Швидкість друку] FROM [Друкарське устаткування] WHERE [Код устаткування] = {id}";
            object[] data = dbActions.ReadData(sql, 5);
            buttonConfirmPrintingEquipment.Tag = id;
            textBoxPrintingEquipmentName.Text = (string)data[0]; 
            textBoxPrintingEquipmentType.Text = (string)data[1]; 
            numericUpDownReleaseYear.Value = (int)data[2];         
            comboBoxTechnicalCondition.SelectedItem = data[3];
            numericUpDownPrintSpeed.Value = (int)data[4];
            this.Height = 564;
        }

        public void EditOrder(int id)
        {
            tabControlEdit.SelectedTab = tabPageOrder;
            buttonConfirmOrder.Tag = id;
            string sql = $"SELECT [Дата замовлення],[Дата виконання],[Статус замовлення] FROM Замовлення WHERE [Код замовлення] = {id}";
            object[] data = dbActions.ReadData(sql, 3);
            maskedTextBoxOrderDate.Text = ((DateTime)data[0]).ToString("dd.MM.yyyy");
            if (data[1] != null) maskedTextBoxExecutionDate.Text = ((DateTime)data[1]).ToString("dd.MM.yyyy");
            comboBoxOrderStatus.SelectedItem = (string)data[2];
            this.Height = 422;
        }

        public void EditManufacturing(int id)
        {
            tabControlEdit.SelectedTab = tabPageManufacturing;
            labelManufacturing.Text = "Редагування виробництва товару"; labelManufacturing.Tag = "Edit";

            string sql = "SELECT [Код продукції],[Код устаткування],[Кількість екземплярів]," +
                "[Дата початку],[Дата закінчення],[Статус виробництва] FROM Виробництво " +
                $"WHERE [Код виробництва] = {id}";
            object[] data = dbActions.ReadData(sql, 6);
            buttonConfirmManufacturing.Tag = id;
          
            sql = "SELECT [Код продукції],Найменування FROM Продукція ORDER BY Найменування";
            dbActions.AddToComboBox(sql, comboBoxProduct, false, "Код продукції", "Найменування");
            comboBoxProduct.SelectedValue = data[0];
            sql = "SELECT [Код устаткування],Найменування FROM [Друкарське устаткування] ORDER BY Найменування";
            dbActions.AddToComboBox(sql,comboBoxPrintingEquipment, true, "Код устаткування", "Найменування");
            if (data[1] == null) comboBoxPrintingEquipment.SelectedValue = 0;
            else comboBoxPrintingEquipment.SelectedValue = data[1];

            numericUpDownNumberOfCopies.Value = (int)data[2];
            if (data[3] != null) maskedTextBoxStartDate.Text = ((DateTime)data[3]).ToString("dd.MM.yyyy");
            if (data[4] != null) maskedTextBoxEndDate.Text = ((DateTime)data[4]).ToString("dd.MM.yyyy");
            comboBoxManufacturingStatus.SelectedItem = data[5];
            this.Height = 609;
        }

        private void buttonConfirmManufacturing_Click(object sender, EventArgs e)
        {
            if (!validation.ValidationOrderManufacturing(maskedTextBoxStartDate.Text, maskedTextBoxEndDate.Text, false, true, labelStartDateError, labelEndDateError))
            {
                return;
            }

            if (Int32.TryParse(buttonConfirmManufacturing.Tag.ToString(), out int id))
            {
                bool costChange = false, statusChange1 = false, statusChange2 = false; object[] dataCost = new object[2];
                string sql = "SELECT [Код продукції],[Код устаткування],[Кількість екземплярів]," +
                "[Дата початку],[Дата закінчення],[Статус виробництва],[Код замовлення],Вартість FROM Виробництво " +
                $"WHERE [Код виробництва] = {id}";
                object[] data = dbActions.ReadData(sql, 8);

                sql = "UPDATE Виробництво SET ";
                List<string> updateFields = new List<string>();

                int productId = (int)data[0];
                if ((int)comboBoxProduct.SelectedValue != productId)
                {
                    updateFields.Add($"[Код продукції] = {comboBoxProduct.SelectedValue}");
                    costChange = true;
                    productId = (int)comboBoxProduct.SelectedValue;
                }

                if ((int)comboBoxPrintingEquipment.SelectedValue == 0 && data[1] != null) 
                    updateFields.Add($"[Код устаткування] = NULL");
                else if ((data[1] == null && (int)comboBoxPrintingEquipment.SelectedValue != 0) || 
                    (data[1] != null && (int)comboBoxPrintingEquipment.SelectedValue != (int)data[1]))
                    updateFields.Add($"[Код устаткування] = {comboBoxPrintingEquipment.SelectedValue}");

                int numberOfCopies = (int)data[2];
                if (numericUpDownNumberOfCopies.Value != numberOfCopies)
                {
                    updateFields.Add($"[Кількість екземплярів] = {numericUpDownNumberOfCopies.Value.ToString(CultureInfo.InvariantCulture)}");
                    numberOfCopies = (int)numericUpDownNumberOfCopies.Value;
                    costChange = true;
                }

                if (costChange)
                {
                    updateFields.Add($"Вартість = (SELECT [Ціна за одиницю] FROM Продукція WHERE [Код продукції] = {productId}) * {numberOfCopies}");
                    string sqlCost = $"SELECT [Код замовлення],Вартість FROM Виробництво WHERE [Код виробництва] = {id}";
                    dataCost = dbActions.ReadData(sqlCost, 2);
                }

                DateTime? date = null;
                if (maskedTextBoxStartDate.Text.Any(char.IsDigit) && DateTime.TryParseExact(maskedTextBoxStartDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    date = parsedDate;
                }
                if (date != data[3] as DateTime?)
                {
                    if (date.HasValue)
                    {
                        updateFields.Add($"[Дата початку] = '{date.Value.ToString("yyyy-MM-dd")}'");
                    }
                    else
                    {
                        updateFields.Add("[Дата початку] = NULL");
                    }
                }

                date = null;
                if (maskedTextBoxEndDate.Text.Any(char.IsDigit) && DateTime.TryParseExact(maskedTextBoxEndDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    date = parsedDate;
                }
                if (date != data[4] as DateTime?)
                {
                    if (date.HasValue)
                    {
                        updateFields.Add($"[Дата закінчення] = '{date.Value.ToString("yyyy-MM-dd")}'");
                    }
                    else
                    {
                        updateFields.Add("[Дата закінчення] = NULL");
                    }
                }

                if ((string)comboBoxManufacturingStatus.SelectedItem != (string)data[5])
                {
                    updateFields.Add($"[Статус виробництва] = '{comboBoxManufacturingStatus.SelectedItem}'");
                    if ((string)comboBoxManufacturingStatus.SelectedItem == "Скасовано")
                    {
                        statusChange1 = true;
                    }
                    else if ((string)data[5] == "Скасовано")
                    {
                        statusChange2 = true;
                    }
                }                       

                if (updateFields.Count == 0)
                {
                    MessageBox.Show("Немає змін");
                    return;
                }

                sql += string.Join(", ", updateFields);
                sql += $" WHERE [Код виробництва] = {id};";
                if (costChange)
                {                    
                    sql += $"UPDATE Замовлення SET [Сума] = [Сума] - {string.Format(CultureInfo.InvariantCulture, "{0}", dataCost[1])} + (SELECT Вартість FROM Виробництво WHERE [Код виробництва] = {id}) WHERE [Код замовлення] = {dataCost[0]};";
                }
                if (statusChange1)
                {
                    sql += $"UPDATE Замовлення SET [Сума] = [Сума] - (SELECT Вартість FROM Виробництво WHERE [Код виробництва] = {id}) WHERE [Код замовлення] = (SELECT [Код замовлення] FROM Виробництво WHERE [Код виробництва] = {id});";
                }
                else if (statusChange2)
                {
                    sql += $"UPDATE Замовлення SET [Сума] = [Сума] + (SELECT Вартість FROM Виробництво WHERE [Код виробництва] = {id}) WHERE [Код замовлення] = (SELECT [Код замовлення] FROM Виробництво WHERE [Код виробництва] = {id});";
                }
                dbActions.ExecuteQuery(sql);
                mainForm.buttonManufacturing_Click(mainForm, EventArgs.Empty);
                this.Close();
            }
        }

        private void buttonConfirmOrder_Click(object sender, EventArgs e)
        {
            if (!validation.ValidationOrderManufacturing(maskedTextBoxOrderDate.Text, maskedTextBoxExecutionDate.Text, true, true, labelOrderDateError, labelExecutionDateError))
            {
                return;
            }

            if (Int32.TryParse(buttonConfirmOrder.Tag.ToString(), out int id))
            {
                string sql = $"SELECT [Дата замовлення],[Дата виконання],[Статус замовлення] FROM Замовлення WHERE [Код замовлення] = {id}";
                object[] data = dbActions.ReadData(sql, 3);

                sql = "UPDATE Замовлення SET ";
                List<string> updateFields = new List<string>();
                DateTime.TryParseExact(maskedTextBoxOrderDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime orderDate);
                if (orderDate != ((DateTime)data[0]))
                    updateFields.Add($"[Дата замовлення] = '{orderDate.ToString("yyyy-MM-dd")}'");

                DateTime? endDate = null;
                if (maskedTextBoxExecutionDate.Text.Any(char.IsDigit) && DateTime.TryParseExact(maskedTextBoxExecutionDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    endDate = parsedDate;
                }
                if (endDate != data[1] as DateTime?)
                {
                    if (endDate.HasValue)
                    {
                        updateFields.Add($"[Дата виконання] = '{endDate.Value.ToString("yyyy-MM-dd")}'");
                    }
                    else
                    {
                        updateFields.Add("[Дата виконання] = NULL");
                    }
                }
                if ((string)comboBoxOrderStatus.SelectedItem != (string)data[2])
                    updateFields.Add($"[Статус замовлення] = '{comboBoxOrderStatus.SelectedItem}'");

                if (updateFields.Count == 0)
                {
                    MessageBox.Show("Немає змін");
                    return;
                }

                sql += string.Join(", ", updateFields);
                sql += $" WHERE [Код замовлення] = {id}";
                dbActions.ExecuteQuery(sql);
                mainForm.buttonOrders_Click(mainForm, EventArgs.Empty);
                this.Close();
            }
        }

        private void buttonConfirmProduct_Click(object sender, EventArgs e)
        {
            if (!validation.ValidationProduct(textBoxProductName.Text, textBoxFormat.Text, labelProductNameError, labelFormatError))
            {
                return;
            }
            switch(labelProduct.Tag)
            {
                case "Edit":
                    {
                        if (Int32.TryParse(buttonConfirmProduct.Tag.ToString(), out int id))
                        {
                            string sql = "UPDATE Продукція SET ";
                            List<string> updateFields = new List<string>();
                            string sqlProduct = $"SELECT [Найменування], [Ціна за одиницю], [Формат], [Вид продукції] FROM Продукція WHERE [Код продукції] = {id}";
                            object[] data = dbActions.ReadData(sqlProduct, 4);

                            if (textBoxProductName.Text != (string)data[0])
                            {
                                if (!dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM Продукція WHERE Найменування = '{textBoxProductName.Text}'"))
                                {
                                    updateFields.Add($"Найменування = '{textBoxProductName.Text}'");
                                }
                                else
                                {
                                    MessageBox.Show("Цей товар вже є в системі!");
                                    textBoxProductName.Text = (string)data[0];
                                    return;
                                }
                            }
                                
                            if (numericUpDownUnitPrice.Value != (decimal)data[1])
                                updateFields.Add($"[Ціна за одиницю] = {numericUpDownUnitPrice.Value.ToString(CultureInfo.InvariantCulture)}");

                            if (textBoxFormat.Text != (string)data[2])
                                updateFields.Add($"Формат = '{textBoxFormat.Text}'");

                            if ((string)comboBoxProductType.SelectedItem != (string)data[3])
                                updateFields.Add($"[Вид продукції] = '{comboBoxProductType.SelectedItem}'");

                            if (updateFields.Count == 0)
                            {
                                MessageBox.Show("Немає змін");
                                return;
                            }

                            sql += string.Join(", ", updateFields);
                            sql += $" WHERE [Код продукції] = {id}";
                            dbActions.ExecuteQuery(sql);
                            mainForm.buttonProducts_Click(mainForm, EventArgs.Empty);
                            this.Close();
                        }
                        break;
                    }
                case "Create":
                    {
                        if (dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM Продукція WHERE Найменування = '{textBoxProductName.Text}'"))
                        {
                            MessageBox.Show("Цей товар вже є в системі!");
                            return;
                        }
                        string sql = "INSERT INTO Продукція (Найменування, [Ціна за одиницю], Формат, [Вид продукції]) " +
                                  $"VALUES ('{textBoxProductName.Text}', {numericUpDownUnitPrice.Value.ToString(CultureInfo.InvariantCulture)}, '{textBoxFormat.Text}', '{comboBoxProductType.SelectedItem}')";
                        dbActions.ExecuteQuery(sql);
                        mainForm.buttonProducts_Click(mainForm, EventArgs.Empty);
                        this.Close();
                        break;
                    }
            }  
        }

        private void buttonConfirmPrintingEquipment_Click(object sender, EventArgs e)
        {
            if (!validation.ValidationPrintingEquipment(textBoxPrintingEquipmentName.Text, textBoxPrintingEquipmentType.Text, labelPrintingEquipmentNameError, labelPrintingEquipmentTypeError))
            {
                return;
            }
            switch (labelPrintingEquipment.Tag)
            {
                case "Edit":
                    {
                        if (Int32.TryParse(buttonConfirmPrintingEquipment.Tag.ToString(), out int id))
                        {
                            string sql = "UPDATE [Друкарське устаткування] SET ";
                            List<string> updateFields = new List<string>();
                            string sqlPrintingEquipment = $"SELECT [Найменування], [Тип], [Рік випуску], [Технічний стан], [Швидкість друку] FROM [Друкарське устаткування] WHERE [Код устаткування] = {id}";
                            object[] data = dbActions.ReadData(sqlPrintingEquipment, 5);

                            if (textBoxPrintingEquipmentName.Text != (string)data[0])
                            {
                                if (!dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM [Друкарське устаткування] WHERE Найменування = '{textBoxPrintingEquipmentName.Text}'"))
                                {
                                    updateFields.Add($"Найменування = '{textBoxPrintingEquipmentName.Text}'");
                                }
                                else
                                {
                                    MessageBox.Show("Це устаткування вже є в системі!");
                                    textBoxPrintingEquipmentName.Text = (string)data[0];
                                    return;
                                }
                            }                         

                            if (textBoxPrintingEquipmentType.Text != (string)data[1])
                                updateFields.Add($"Тип = '{textBoxPrintingEquipmentType.Text}'");

                            if (numericUpDownReleaseYear.Value != (int)data[2])
                                updateFields.Add($"[Рік випуску] = {numericUpDownReleaseYear.Value.ToString(CultureInfo.InvariantCulture)}");

                            if ((string)comboBoxTechnicalCondition.SelectedItem != (string)data[3])
                                updateFields.Add($"[Технічний стан] = '{comboBoxTechnicalCondition.SelectedItem}'");

                            if (numericUpDownPrintSpeed.Value != (int)data[4])
                                updateFields.Add($"[Швидкість друку] = {numericUpDownPrintSpeed.Value.ToString(CultureInfo.InvariantCulture)}");                    

                            if (updateFields.Count == 0)
                            {
                                MessageBox.Show("Немає змін");
                                return;
                            }

                            sql += string.Join(", ", updateFields);
                            sql += $" WHERE [Код устаткування] = {id}";
                            dbActions.ExecuteQuery(sql);
                            mainForm.buttonPrintingEquipment_Click(mainForm, EventArgs.Empty);
                            this.Close();
                        }
                        break;
                    }
                case "Create":
                    {
                        if (dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM [Друкарське устаткування] WHERE Найменування = '{textBoxPrintingEquipmentName.Text}'"))
                        {
                            MessageBox.Show("Це устаткування вже є в системі!");
                            return;
                        }                        
                        string sql = "INSERT INTO [Друкарське устаткування] (Найменування, Тип, [Рік випуску], [Технічний стан], [Швидкість друку]) " +
                                  $"VALUES ('{textBoxPrintingEquipmentName.Text}', '{textBoxPrintingEquipmentType.Text}', {numericUpDownReleaseYear.Value.ToString(CultureInfo.InvariantCulture)}, '{comboBoxTechnicalCondition.SelectedItem}', {numericUpDownPrintSpeed.Value.ToString(CultureInfo.InvariantCulture)})";
                        dbActions.ExecuteQuery(sql);
                        mainForm.buttonPrintingEquipment_Click(mainForm, EventArgs.Empty);
                        this.Close();
                        break;
                    }
            }
        }

        private void buttonConfirmUser_Click(object sender, EventArgs e)
        {
            if (!validation.ValidationUser(textBoxName.Text, textBoxSurname.Text, textBoxPatronymic.Text, textBoxEmail.Text, labelNameError, labelSurnameError, labelPatronymicError, labelEmailError))
            {
                return;
            }

            if (Int32.TryParse(buttonConfirmUser.Tag.ToString(), out int id))
            {
                string sql = "UPDATE Користувачі SET ";
                List<string> updateFields = new List<string>();
                string sqlUser = $"SELECT [Ім'я], Прізвище, [По-батькові], [Електронна пошта], Роль FROM Користувачі WHERE [Код користувача] = {id}";
                object[] data = dbActions.ReadData(sqlUser, 5);

                if (textBoxName.Text != (string)data[0])
                    updateFields.Add($"[Ім'я] = '{textBoxName.Text}'");

                if (textBoxSurname.Text != (string)data[1])
                    updateFields.Add($"Прізвище = '{textBoxSurname.Text}'");

                if (textBoxPatronymic.Text != (string)data[2])
                    updateFields.Add($"[По-батькові] = '{textBoxPatronymic.Text}'");

                if (textBoxEmail.Text != (string)data[3])
                {
                    if (!dbActions.CheckIfExistInDb($"SELECT COUNT(*) FROM Користувачі WHERE [Електронна пошта] = '{textBoxEmail.Text}'"))
                    {
                        updateFields.Add($"[Електронна пошта] = '{textBoxEmail.Text}'");
                    }
                    else
                    {
                        MessageBox.Show("Ця електронна пошта вже є в системі!");
                        textBoxEmail.Text = (string)data[3];
                        return;
                    }                     
                }                    

                if ((string)comboBoxRole.SelectedItem != (string)data[4])
                    updateFields.Add($"Роль = '{comboBoxRole.SelectedItem}'");

                if (updateFields.Count == 0)
                {
                    MessageBox.Show("Немає змін");
                    return;
                }

                sql += string.Join(", ", updateFields);
                sql += $" WHERE [Код користувача] = {id}";
                dbActions.ExecuteQuery(sql);
                mainForm.buttonUsers_Click(mainForm, EventArgs.Empty);
                this.Close();
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

        private void buttonMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
