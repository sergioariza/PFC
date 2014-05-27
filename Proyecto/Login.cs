using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Traducciones;

namespace Proyecto
{
    public partial class Login : Form
    {
        public bool login = true;
        public string usuarioLogin;
        public string tipoUsuarioLogin;

        public Login()
        {
            InitializeComponent();
        }

        private void aceptar_Click(object sender, EventArgs e)
        {
            Proyecto.MenPas.WS_EstimacionF WS_EstimacionF = new Proyecto.MenPas.WS_EstimacionF();
            try
            {
                if (WS_EstimacionF.EstaRegistrado(textBoxUsuario.Text, textBoxContraseña.Text) == true)
                {
                    login = true;
                    usuarioLogin = textBoxUsuario.Text;
                    tipoUsuarioLogin = WS_EstimacionF.dame_perfil(usuarioLogin);
                    MessageBox.Show(Cadenas.inicioOk);

                    if (System.IO.File.Exists("pass.dll"))
                    {
                        System.IO.File.Delete("pass.dll");
                    }

                    if (contraseñaCheckBox.Checked == true)
                    {
                        System.IO.StreamWriter sw = new System.IO.StreamWriter("pass.dll", true, System.Text.Encoding.Default);
                        String s = textBoxUsuario.Text + " " + textBoxContraseña.Text;
                        sw.WriteLine(s);
                        sw.Close();
                    }
                    else
                    {
                        System.IO.File.Create("pass.dll");
                    }
                }
                else
                {
                    login = false;
                }
            }
            catch (Exception msg) {
                login = false;
            }
        }

        private void pictureBoxEspañol_Click(object sender, EventArgs e)
        {
            traducir("");
        }

        private void pictureBoxIngles_Click(object sender, EventArgs e)
        {
            traducir("en");
        }

        private void pictureBoxPortugues_Click(object sender, EventArgs e)
        {
            traducir("pt");
        }

        private void traducir(string idioma)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(idioma);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(idioma);
            ComponentResourceManager resources = new ComponentResourceManager(this.GetType());
            resources.ApplyResources(this, "$this");

            foreach (Control c in this.Controls)
            {
                resources.ApplyResources(c, c.Name);
            }
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData.ToString())
            {
                case "Return":
                    aceptar_Click(sender, e);
                    this.DialogResult = DialogResult.OK;
                    break;
            }
        }

        private void textBoxContraseña_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData.ToString())
            {
                case "Return":
                    aceptar_Click(sender, e);
                    this.DialogResult = DialogResult.OK;
                    break;
            }
        }

        private void textBoxUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData.ToString())
            {
                case "Return":
                    aceptar_Click(sender, e);
                    this.DialogResult = DialogResult.OK;
                    break;
            }            
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists("pass.dll"))
            {
                System.IO.File.Create("pass.dll");
            }
            else
            {
                System.IO.StreamReader sr = new System.IO.StreamReader("pass.dll", System.Text.Encoding.Default, true);

                //Si se trata de un fichero no vacio, inicializo antes los listados de funciones
                if (sr.Peek() != -1)
                {
                    // Leer una línea del ficheroFunciones
                    String s = sr.ReadLine();

                    if (String.IsNullOrEmpty(s) == false)
                    {
                        String[] vector = s.Split(' ');
                        String usuario = vector[0];
                        String contraseña = vector[1];
                        textBoxUsuario.Text = usuario;
                        textBoxContraseña.Text = contraseña;
                        contraseñaCheckBox.Checked = true;
                    }
                }

                sr.Close();
            }            
        }
    }
}
