using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Globalization;

namespace Polygraphy
{
    public class Validation
    {
        private Regex isUkrainianLanguage = new Regex(@"^[А-Яа-яЁёЇїІіЄєҐґ ']+"); 
        private Regex isUkrainianOrEnglishLanguageNumbers = new Regex(@"^[a-zA-Z0-9А-Яа-яЁёЇїІіЄєҐґ ']+");
        private Regex isEmail = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        private Regex hasNumber = new Regex(@"[0-9]+");
        private Regex hasUpperChar = new Regex(@"[A-Z]+");
        private Regex hasMinMaxCharsPassword = new Regex(@".{6,60}");
        private Regex hasLowerChar = new Regex(@"[a-z]+");
        private Regex isFormat = new Regex(@"^[0-9]+х[0-9]+(х[0-9]+)*$");

        private bool ValidationBase(string check, Label label, string checkName, int length, string valType)
        {
            if (string.IsNullOrWhiteSpace(check))
            {
                label.Text = $"Не заповнено поле {checkName}";
                return false;
            }
            if (check.Length > length)
            {
                label.Text = $"{checkName} має містити від 1 до {length} символів!";
                return false;
            }

            switch (valType)
            {
                case "Ua":
                    if (!isUkrainianLanguage.IsMatch(check))
                    {
                        label.Text = $"{checkName} може містити тільки українські літери!";
                        return false;
                    }
                    break;
                case "UaEnNum":
                    if (!isUkrainianOrEnglishLanguageNumbers.IsMatch(check))
                    {
                        label.Text = $"{checkName} може містити тільки англійські і українські літери та цифри!";
                        return false;
                    }
                    break;
                case "Format":
                    if (!isFormat.IsMatch(check))
                    {
                        label.Text = $"{checkName} може містити тільки цифри і х!";
                        return false;
                    }
                    break;
            }           

            label.Text = "";
            return true;
        }

        public bool ValidationUserEmail(string email, Label label, bool isInDb)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                label.Text = "Не заповнено поле Електрона пошта";
                return false;
            }
            if (!isEmail.IsMatch(email))
            {
                label.Text = "Невірна електрона пошта!";
                return false;
            }
            if (isInDb)
            {
                string sql = $"SELECT COUNT(*) FROM Користувачі WHERE [Електронна пошта] = '{email}'";
                DbActions dbActions = new DbActions();
                bool exist = dbActions.CheckIfExistInDb(sql);
                if (exist)
                {
                    label.Text = "Електрона пошта вже зареєстрована!";
                    return false;
                }        
            }

            label.Text = "";
            return true;
        }

        public bool ValidationUserPassword(string password, Label label)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                label.Text = "Не заповнено поле Пароль";
                return false;
            }
            if (!hasMinMaxCharsPassword.IsMatch(password))
            {
                label.Text = "Пароль має містити від 6 до 60 символів!";
                return false;
            }
            if (!hasNumber.IsMatch(password))
            {
                label.Text = "Пароль має містити хоча б одну цифру!";
                return false;
            }
            if (!hasUpperChar.IsMatch(password))
            {
                label.Text = "Пароль має містити хоча б одну велику англійську літеру!";
                return false;
            }
            if (!hasLowerChar.IsMatch(password))
            {
                label.Text = "Пароль має містити хоча б одну малу англійську літеру!";
                return false;
            }

            label.Text = "";
            return true;
        }

        public bool ValidationUser(string name, string surname, string patronymic, string email, 
            Label labelName, Label labelSurname, Label labelPatronymic, Label labelEmail, string password = null, Label labelPassword = null)
        {
            bool valName = ValidationBase(name, labelName, "Ім'я", 50, "Ua"),
                valSurname = ValidationBase(surname, labelSurname, "Прізвище", 50, "Ua"),
                valPatronymic = ValidationBase(patronymic, labelPatronymic, "По-батькові", 50, "Ua");
            if (password != null && labelPassword != null)
            {
                bool valEmail = ValidationUserEmail(email, labelEmail, true);
                bool valPassword = ValidationUserPassword(password, labelPassword);
                if (valName && valSurname && valPatronymic && valEmail && valPassword)
                {
                    return true;
                }
            }
            else if (valName && valSurname && valPatronymic && ValidationUserEmail(email, labelEmail, false))
            {
                return true;
            }
                
            return false;
        }
        
        public bool ValidationProduct(string name, string format, Label labelName, Label labelFormat)
        {
            bool valName = ValidationBase(name, labelName, "Найменування", 60, "Ua"),
                valFormat = ValidationBase(format, labelFormat, "Формат", 30, "Format");
            if (valName && valFormat)
            {
                return true;
            }
            return false;
        }
        
        public bool ValidationPrintingEquipment(string name, string type, Label labelName, Label labelType)
        {
            bool valName = ValidationBase(name, labelName, "Найменування", 70, "UaEnNum"), 
                valType = ValidationBase(type, labelType, "Тип", 50, "Ua");
            if (valName && valType)
            {
                return true;
            }
            return false;
        }

        private bool ValidationDate (string check, string checkName, bool beNull, Label label = null)
        {
            if (!check.Any(char.IsDigit) && !beNull)
            {
                string text = $"Не заповнено поле {checkName}";
                if (label != null) label.Text = text;
                else MessageBox.Show(text);
                return false;
            }
            if (!DateTime.TryParseExact(check, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) && check.Any(char.IsDigit))
            {
                string text = $"Невірно заповнено поле {checkName}";
                if (label != null) label.Text = text;
                else MessageBox.Show(text);
                return false;
            }

            if (label != null) label.Text = "";
            return true;
        }

        public bool ValidationOrderManufacturing(string dateStart, string dateEnd, bool IsOrder, bool IsLabel, Label labelDateStart = null, Label labelDateEnd = null)
        {
            bool valStart, valEnd;
            if (IsOrder)
            {
                if (IsLabel)
                {
                    valStart = ValidationDate(dateStart, "Дата замовлення", false, labelDateStart);
                    valEnd = ValidationDate(dateEnd, "Дата виконання", true, labelDateEnd);
                }
                else
                {
                    valStart = ValidationDate(dateStart, "для пошуку дати!", false);
                    valEnd = ValidationDate(dateEnd, "для пошуку дати!", true);
                }
            }
            else
            {
                if (IsLabel)
                {
                    valStart = ValidationDate(dateStart, "Дата початку", true, labelDateStart);
                    valEnd = ValidationDate(dateEnd, "Дата закінчення", true, labelDateEnd);
                }
                else
                {
                    valStart = ValidationDate(dateStart, "для пошуку дати!", true);
                    valEnd = ValidationDate(dateEnd, "для пошуку дати!", true);
                }
            }
            
            if (valStart && valEnd)
            {
                return true;
            }
            return false;
        }

        public string Hash(string input)
        {
            using (var sha128 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha128.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
