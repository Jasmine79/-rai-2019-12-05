using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Golf
{
    public partial class Form1 : Form
    {
        int LastID=0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dateTimePicker_Szuletett.MaxDate = DateTime.Today.AddYears(-18);
            dateTimePicker_Szuletett.Value = DateTime.Today.AddYears(-50); //-átlag életkor

            TagokListajanakFrissitese();

        }

        private void TagokListajanakFrissitese()
        {
            Program.sql.CommandText = "SELECT `kod`, `nev`, `szuletett`, `magassag` FROM `tagok` ORDER BY `nev`;";
            Program.conn.Open();
            using (MySqlDataReader dr = Program.sql.ExecuteReader())
            {
                listBox_Tagok.Items.Clear(); //--törölni kell a listát
                while (dr.Read()) // a datareader 1-1 rekordját olvassa be
                {
                    listBox_Tagok.Items.Add(new GolfTag(dr.GetInt32("kod"), dr.GetString("nev"), dr.GetDateTime("szuletett"), dr.GetInt32("magassag")));
                    if (dr.GetInt32("kod")>LastID)
                    {
                        LastID = dr.GetInt32("kod");
                    }
                }
            }
            //--beviteli mezők kiürítése
            numericUpDown_kod.Value = LastID + 1;
            textBox_Nev.Text = "";
            dateTimePicker_Szuletett.Value = DateTime.Today.AddYears(-50);
            numericUpDown_Magassag.Value = 176;
            Program.conn.Close();
        }

        private void button_Insert_Click(object sender, EventArgs e)
        {
            if (textBox_Nev.Text.Trim().Length<3)
            {
                MessageBox.Show("Kérem töltse ki a név mezőt");
                textBox_Nev.Focus(); //-- A fókuszt ráviszi a textboxra
                textBox_Nev.Select(textBox_Nev.Text.Length, 0);
                //--ott villogjon a kurzor, meghatározni hova tegye a kurzort
                return;
            }
            Program.sql.CommandText = "INSERT INTO `tagok` (`kod`, `nev`, `szuletett`, `magassag`) VALUES (@kod, @nev, @szuletett, @magassag);";
            //--jelöltük, hogy később megadjuk ezeket az értékeket

           
            Program.sql.Parameters.AddWithValue("@kod", LastID+1);
            Program.sql.Parameters.AddWithValue("@nev", textBox_Nev.Text.Trim());
            Program.sql.Parameters.AddWithValue("@szuletett", dateTimePicker_Szuletett.Value.ToString("yyyy-MM-dd")); //--stringként tartalmazza
            Program.sql.Parameters.AddWithValue("@magassag", Convert.ToInt32(numericUpDown_Magassag.Value));
            //--parameters metódusa van
            try //--hibakezelés
            {
                Program.conn.Open();
                Program.sql.ExecuteNonQuery();                
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                
            }
            finally
            {
                Program.sql.Parameters.Clear();
                Program.conn.Close();
            }
            TagokListajanakFrissitese();
        }

        private void listBox_Tagok_SelectedIndexChanged(object sender, EventArgs e)
        {
            GolfTag kivalasztott_golfTag = (GolfTag)listBox_Tagok.SelectedItem;
            //--zárójeles GolfTag itt a kikényszerítéshez kell
            numericUpDown_kod.Value = kivalasztott_golfTag.Kod;
            textBox_Nev.Text = kivalasztott_golfTag.Nev;
            dateTimePicker_Szuletett.Value = kivalasztott_golfTag.Szuletett;
            numericUpDown_Magassag.Value = kivalasztott_golfTag.Magassag;

        }

        private void button_Delete_Click(object sender, EventArgs e)
        {
            if (listBox_Tagok.SelectedIndex==-1)
            {
                return;
            }
            GolfTag kivalasztott_golfTag = (GolfTag)listBox_Tagok.SelectedItem;
            if (MessageBox.Show($"Valóban törölni akarja\n\n{kivalasztott_golfTag.ToString()}","Törlés", MessageBoxButtons.YesNo,MessageBoxIcon.Asterisk)==DialogResult.No)
            {
                return; //--a hívás helyére tér vissza
            } //--ha valójában nem akartunk törölni 

            Program.sql.CommandText="DELETE FROM `tagok` WHERE `tagok`.`kod` = @kod;";
            Program.sql.Parameters.AddWithValue("@kod", kivalasztott_golfTag.Kod);
            try
            {
                Program.conn.Open();
                Program.sql.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Program.sql.Parameters.Clear();
                Program.conn.Close();
            }
            TagokListajanakFrissitese();
        }
    }
}
