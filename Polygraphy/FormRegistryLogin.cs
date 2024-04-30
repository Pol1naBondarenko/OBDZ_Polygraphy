using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Polygraphy
{
    public partial class FormRegistryLogin : Form
    {
        public bool Registry;
        private FormMain mainForm;
        DbActions dbActions = new DbActions();
        private bool isDragging = false; private int offsetX, offsetY;

        public FormRegistryLogin(FormMain mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
        }

        private void FormRegistryLogin_Load(object sender, EventArgs e)
        {
            if (Registry)
            {
                labelMode.Text = "Реєстрація";
                labelMode.Location = new Point(144, labelMode.Location.Y);
                this.Text = "Реєстрація";
                labelName.Visible = true;
                labelSurname.Visible = true;
                labelPatronymic.Visible = true;
                textBoxName.Visible = true;
                textBoxSurname.Visible = true;
                textBoxPatronymic.Visible = true;
                labelNameError.Visible = true;
                labelSurnameError.Visible = true;
                labelPatronymicError.Visible = true;

                labelEmail.Location = new Point(86, 306);
                textBoxEmail.Location = new Point(90, 332);
                labelEmailError.Location = new Point(87, 356);
                labelPassword.Location = new Point(86, 376);
                textBoxPassword.Location = new Point(90, 402);
                labelPasswordError.Location = new Point(87, 426);
                checkBoxShowPassword.Location = new Point(215, 446);
                buttonConfirm.Location = new Point(145, 504);
                buttonChangeMode.Location = new Point(163, 567);
                this.Size = new Size(450, 600);
                buttonConfirm.Text = "Зареєструватися";
                buttonChangeMode.Text = "Вже є акаунт?";

            }
            else
            {
                labelMode.Text = "Вхід";
                labelMode.Location = new Point(192, labelMode.Location.Y);
                this.Text = "Вхід";
                labelName.Visible = false;
                labelSurname.Visible = false;
                labelPatronymic.Visible = false;
                textBoxName.Visible = false;
                textBoxSurname.Visible = false;
                textBoxPatronymic.Visible = false;
                labelNameError.Visible = false;
                labelSurnameError.Visible = false;
                labelPatronymicError.Visible = false;

                labelEmail.Location = new Point(labelName.Location.X, labelName.Location.Y);
                textBoxEmail.Location = new Point(textBoxName.Location.X, textBoxName.Location.Y);
                labelEmailError.Location = new Point(labelNameError.Location.X, labelNameError.Location.Y);
                labelPassword.Location = new Point(labelSurname.Location.X, labelSurname.Location.Y);
                textBoxPassword.Location = new Point(textBoxSurname.Location.X, textBoxSurname.Location.Y);
                labelPasswordError.Location = new Point(labelSurnameError.Location.X, labelSurnameError.Location.Y);

                checkBoxShowPassword.Location = new Point(215, labelPasswordError.Location.Y + 20);
                buttonConfirm.Location = new Point(145, checkBoxShowPassword.Location.Y + 58);
                buttonChangeMode.Location = new Point(163, buttonConfirm.Location.Y + 63);
                this.Size = new Size(450, buttonChangeMode.Location.Y + 60);
                buttonConfirm.Text = "Вхід";
                buttonChangeMode.Text = "Створити акаунт";
            }
        }

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowPassword.Checked)
                textBoxPassword.UseSystemPasswordChar = false;
            else
                textBoxPassword.UseSystemPasswordChar = true;
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            if (Registry)
            {
                Validation validation = new Validation();
                if (validation.ValidationUser(textBoxName.Text, textBoxSurname.Text, textBoxPatronymic.Text, textBoxEmail.Text, labelNameError, labelSurnameError, labelPatronymicError, labelEmailError, textBoxPassword.Text, labelPasswordError))
                {
                    if (!dbActions.RegisterUser(textBoxName.Text, textBoxSurname.Text, textBoxPatronymic.Text, textBoxEmail.Text, validation.Hash(textBoxPassword.Text)))
                    {
                        MessageBox.Show("Помилка при реєстрації", "Помилка");
                        return;
                    }                       
                        Registry = false; 
                    buttonConfirm_Click(this, EventArgs.Empty);
                }
            }
            else
            {
                Validation validation = new Validation();
                bool valEmail = validation.ValidationUserEmail(textBoxEmail.Text, labelEmailError, false), valPassword = validation.ValidationUserPassword(textBoxPassword.Text, labelPasswordError);
                if (!valEmail || !valPassword)
                {
                    return;
                }

                object[] data = dbActions.LoginUser(textBoxEmail.Text, validation.Hash(textBoxPassword.Text));

                if (data != null && (string)data[1] != null)
                {                  
                    mainForm.isAutorized = true;
                    mainForm.autorizedUserID = (int)data[0]; mainForm.userNameSurname = $"{data[1]} {data[2]}";
                    if ((string)data[3] == "Адміністратор")
                        mainForm.isAdmin = true;
                    mainForm.Authorization();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Невірна пошта або пароль");
                    return;
                }
            }
        }    

        private void buttonChangeMode_Click(object sender, EventArgs e)
        {
            if (Registry) 
                Registry = false;
            else 
                Registry = true;
            FormRegistryLogin_Load(this, EventArgs.Empty);
        }

        private void FormRegistryLogin_Activated(object sender, EventArgs e)
        {
            FormRegistryLogin_Load(this, EventArgs.Empty);
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

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
