using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Polygraphy
{
    public class DbActions
    {
        private string connectionString = "Server=localhost;Database=PolygraphyDb;Trusted_Connection=True;";

        public void Report(string query)
        {
            string filename = "report.docx";
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    using (var file = System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None))
                    {
                    }
                }
                catch (System.IO.IOException)
                {
                    MessageBox.Show("Файл report вже використовується, неможливо зробити звіт!");
                    return;
                }
            }
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                using (WordprocessingDocument doc = WordprocessingDocument.Create(filename, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    Table table = new Table();

                    TableRow headerRow = new TableRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        TableCellProperties cellProperties = new TableCellProperties(
                            new TableCellBorders(
                                new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" }
                            )
                        );
                        TableCell cell = new TableCell(new Paragraph(new Run(new Text(reader.GetName(i)))));
                        cell.Append(cellProperties);
                        headerRow.Append(cell);
                    }
                    table.Append(headerRow);

                    while (reader.Read())
                    {
                        TableRow dataRow = new TableRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            TableCellProperties cellProperties = new TableCellProperties(
                                new TableCellBorders(
                                    new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                    new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                    new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                    new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                    new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" },
                                    new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Color = "000000" }
                                )
                            );
                            TableCell cell = new TableCell(new Paragraph(new Run(new Text(reader[i].ToString()))));
                            cell.Append(cellProperties);
                            dataRow.Append(cell);
                        }
                        table.Append(dataRow);
                    }
                    body.Append(table);
                }
            }
            System.Diagnostics.Process.Start(filename);
        }

        public void AddToComboBox(string sql, ComboBox comboBox, bool addNull, string valueMember, string displayMember)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                var dataTable = new DataTable();
                dataTable.Load(reader);

                if (addNull)
                {
                    DataRow row = dataTable.NewRow();
                    row[valueMember] = 0;
                    row[displayMember] = "Не обрано";
                    dataTable.Rows.InsertAt(row, 0);
                }

                comboBox.DataSource = dataTable;
                comboBox.ValueMember = valueMember;
                comboBox.DisplayMember = displayMember;

                reader.Close();
            }
        }

        public bool Transaction(string sqlOrder, List<int> productsIDInCart, List<int> productsQuantityInCart)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;

                    command.CommandText = sqlOrder;
                    int orderId = Convert.ToInt32(command.ExecuteScalar());

                    for (int i = 0; i < productsIDInCart.Count; i++)
                    {
                        string sqlManufacturing = "INSERT INTO Виробництво ([Код замовлення],[Код продукції],[Кількість екземплярів],[Вартість],[Статус виробництва]) " +
                            $"VALUES ({orderId}, {productsIDInCart[i]}, {productsQuantityInCart[i]}, (SELECT [Ціна за одиницю] * {productsQuantityInCart[i]} FROM Продукція WHERE [Код продукції] = {productsIDInCart[i]}), 'В черзі')";
                        command.CommandText = sqlManufacturing;
                        command.ExecuteNonQuery();                                           
                    }
                    transaction.Commit();
                    Console.WriteLine("Транзакцію успішно завершено");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка під час виконання транзакції: " + ex.Message);
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Помилка відкату транзакції: " + ex2.Message);
                    }
                    return false;
                }
            }
        }

        public DataSet FillDataSet(string sql)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        public bool RegisterUser(string name, string surname, string patronymic, string email, string password)
        {
            string sql = "INSERT INTO Користувачі ([Ім'я], [Прізвище], [По-батькові], [Електронна пошта], [Пароль], [Роль]) " +
            "VALUES (@name, @surname, @patronymic, @email, @password, 'Користувач')";
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@surname", surname);
                    command.Parameters.AddWithValue("@patronymic", patronymic);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", password);
                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Помилка: " + ex.Message);
                        return false;
                    }
                } 
            } 
        }

        public DataSet UsersProductSearch(string name, string costFrom, string costTo, string format, string printType)
        {
            string sql = "SELECT * FROM Продукція WHERE [Найменування] LIKE @name AND " +
                         "[Ціна за одиницю] >= @costFrom AND [Ціна за одиницю] <= @costTo AND " +
                         "[Формат] LIKE @format AND [Вид продукції] LIKE @printType ORDER BY Найменування";

            DataSet ds = new DataSet();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", "%" + name + "%");
                    command.Parameters.AddWithValue("@costFrom", costFrom);
                    command.Parameters.AddWithValue("@costTo", costTo);
                    command.Parameters.AddWithValue("@format", "%" + format + "%");
                    command.Parameters.AddWithValue("@printType", "%" + printType + "%");

                    try
                    {
                        connection.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(ds);
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Помилка: " + ex.Message);
                    }
                }
            }
            return ds;
        }

        public object[] LoginUser(string email, string password)
        {
            string sql = "SELECT [Код користувача], [Ім'я], [Прізвище], [Роль] FROM Користувачі WHERE [Електронна пошта] = @email and [Пароль] = @password";
            object[] results = new object[4];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", password);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                results[0] = reader.GetInt32(0);
                                results[1] = reader.GetString(1);
                                results[2] = reader.GetString(2);
                                results[3] = reader.GetString(3);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Помилка: " + ex.Message);
                    }
                }
            }
            return results;
        }


        public void ExecuteQuery(string sql)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlDataAdapter SDA = new SqlDataAdapter(sql, connection);
            SDA.SelectCommand.ExecuteNonQuery();
        }

        public bool CheckIfExistInDb(string sql)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        public object[] ReadData(string sql, int count)
        {
            object[] data = new object[count];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (!reader.IsDBNull(i))
                        {
                            data[i] = reader.GetValue(i);
                        }
                        else
                        {
                            data[i] = null;
                        }
                    }
                }
            }
            return data;
        }
    }
}
