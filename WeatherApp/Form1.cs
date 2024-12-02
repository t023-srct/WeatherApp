using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace WeatherApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private async Task LoadWeatherData()
        {
            string apiUrl = "https://data.tmd.go.th/api/Weather3Hours/V2/?uid=api&ukey=api12345"; // URL API
            string userToken = "X3XI70WyafTlKuThWG0XQUQdTZ0VBR9E"; // User Token
            string stationName = textBox1.Text.Trim(); // รับชื่อสถานีจาก TextBox1

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // เพิ่ม Header api-key
                    client.DefaultRequestHeaders.Add("api-key", userToken);

                    // ดึงข้อมูลจาก API
                    var response = await client.GetStringAsync(apiUrl);
                    //XDocument xml = XDocument.Parse(response); // แปลง XML

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(response);

                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Station"); // เพิ่มคอลัมน์ตามโครงสร้าง XML
                    dataTable.Columns.Add("Temperature");
                    dataTable.Columns.Add("Humidity");

                    foreach (XmlNode node in xmlDoc.SelectNodes("//Station"))
                    {
                        DataRow row = dataTable.NewRow();
                        string stationNameThai = node.SelectSingleNode("StationNameThai")?.InnerText;
                        row["Station"] = stationNameThai;

                        if (!string.IsNullOrEmpty(stationName) && !string.Equals(stationNameThai, stationName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue; // ข้ามถ้าไม่ตรงกับชื่อสถานีที่ป้อน
                        }

                        var observationNode = node.SelectSingleNode("Observation");
                        if (observationNode != null)
                        {
                            row["Temperature"] = observationNode.SelectSingleNode("AirTemperature")?.InnerText;
                            row["Humidity"] = observationNode.SelectSingleNode("RelativeHumidity")?.InnerText;
                        }
                        else
                        {
                            row["Temperature"] = "N/A";
                            row["Humidity"] = "N/A";// กรณีไม่มีโหนด Observation
                        }

                        dataTable.Rows.Add(row);
                    }



                    if (dataTable != null)
                    {
                        dataGridView1.DataSource = dataTable; // แสดงเฉพาะคอลัมน์ที่เลือกใน DataGridView
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            await LoadWeatherData();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            timer1.Start();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            date.Text = now.ToString("dd/MM/yyyy");
            hour.Text = now.Hour.ToString("00"); // แสดงชั่วโมง
            minute.Text = now.Minute.ToString("00"); // แสดงนาที
            second.Text = now.Second.ToString("00"); // แสดงวินาที


            if (now.Second <= 30)
            {
                second.ForeColor = Color.Green; // วินาที <= 30 สีเขียว
            }
            else
            {
                second.ForeColor = Color.Red; // วินาที > 30 สีแดง
            }

            if (now.Second == 30 || now.Second == 0)
            {
                 LoadWeatherData();
            }
        }
    }
}
