using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BAN_VE_CINE
{
    public partial class Form1 : Form
    {
        // Danh sách các ghế được chọn
        private List<Button> lstChonGhe = new List<Button>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CaiDatThongTin();
        }

        private void CaiDatThongTin()
        {
            // Thêm cột Mã Hóa Đơn
            dgvKhachHang.Columns.Add("MaHoaDon", "Mã Hóa Đơn");

            // Thêm cột Tên Khách Hàng
            dgvKhachHang.Columns.Add("TenKhachHang", "Tên Khách Hàng");

            // Thêm cột Ngày Đặt
            dgvKhachHang.Columns.Add("NgayDat", "Ngày Đặt");

            // Định dạng cột Ngày Đặt dưới dạng ngày tháng
            dgvKhachHang.Columns["NgayDat"].DefaultCellStyle.Format = "dd/MM/yyyy";

            // Thêm cột Tổng Tiền
            dgvKhachHang.Columns.Add("TongTien", "Tổng Tiền");

            // Định dạng cột Tổng Tiền dưới dạng tiền tệ
            dgvKhachHang.Columns["TongTien"].DefaultCellStyle.Format = "C2"; // C2 là định dạng tiền tệ với 2 chữ số thập phân

            dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Thêm các mục mới vào ComboBox
            cmbKhuVuc.Items.AddRange(new string[] { "Quận 9", "Thủ Đức", "Bình Thạnh", "Quận 1", "Quận 5", "Hóc Môn", "Bình Dương" });
            cmbKhuVuc.SelectedIndex = 0;

            optNu.Checked = true;

            txtTongTien.Text = "0 VNĐ";
            txtTongTien.ReadOnly = true;
        }

        private void btnChonGhe_Click(object sender, EventArgs e)
        {
            Button btnChonGhe = (Button)sender;

            // Kiểm tra xem ghế đã được mua hay chưa (nếu có trạng thái khác màu trắng và xanh dương)
            if (btnChonGhe.BackColor != Color.White && btnChonGhe.BackColor != Color.LightBlue)
            {
                MessageBox.Show("Ghế đã được mua!!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Thay đổi màu ghế khi chọn hoặc bỏ chọn
            if (btnChonGhe.BackColor == Color.White)
            {
                btnChonGhe.BackColor = Color.LightBlue;
                lstChonGhe.Add(btnChonGhe);  // Thêm ghế vào danh sách
            }
            else if (btnChonGhe.BackColor == Color.LightBlue)
            {
                btnChonGhe.BackColor = Color.White;
                lstChonGhe.Remove(btnChonGhe);  // Bỏ ghế ra khỏi danh sách
            }
        }

        private void btnChon_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra nếu không có ghế nào được chọn
                if (lstChonGhe.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một ghế!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Tính tổng tiền những ghế được chọn
                double tongTien = 0;
                foreach (Button item in lstChonGhe)
                {
                    int GheChon = int.Parse(item.Text);
                    if (GheChon <= 4)
                        tongTien += 3000;
                    else if (GheChon <= 8)
                        tongTien += 4000;
                    else if (GheChon <= 12)
                        tongTien += 5000;
                    else if (GheChon <= 16)
                        tongTien += 6000;
                    else
                        tongTien += 8000;
                }

                // Hiển thị tổng tiền
                txtTongTien.Text = tongTien.ToString("N0") + " VNĐ";

                // Thêm thông tin vào DataGridView (nếu cần)
                // dgvKhachHang.Rows.Add(...);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
