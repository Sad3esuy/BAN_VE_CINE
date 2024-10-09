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
            LoadHoaDonData();
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
            //dgvKhachHang.Columns["NgayDat"].DefaultCellStyle.Format = "dd/MM/yyyy";

            // Thêm cột Tổng Tiền
            dgvKhachHang.Columns.Add("TongTien", "Tổng Tiền");

            // Định dạng cột Tổng Tiền dưới dạng tiền tệ
           // dgvKhachHang.Columns["TongTien"].DefaultCellStyle.Format = "C2"; // C2 là định dạng tiền tệ với 2 chữ số thập phân

            dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKhachHang.AutoGenerateColumns = true;


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
                lstChonGhe.Remove(btnChonGhe);  // Bỏ ghế khỏi danh sách
            }

            // Cập nhật tổng tiền ngay lập tức sau khi chọn ghế
            TinhTongTien();
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            // Duyệt qua danh sách ghế đã chọn
            foreach (Button item in lstChonGhe.ToList()) // Sử dụng ToList() để tránh thay đổi danh sách trong vòng lặp
            {
                if (item.BackColor == Color.LightBlue)
                {
                    // Đổi màu ghế về màu trắng (màu mặc định của bạn)
                    item.BackColor = Color.White;
                    // Xóa ghế khỏi danh sách đã chọn
                    lstChonGhe.Remove(item);
                }
            }

            // Reset tổng tiền
            txtTongTien.Text = "0 VNĐ";
        }

        // Phương thức tính tổng tiền, cập nhật lại thông tin hiển thị
        private void TinhTongTien()
        {
            // Kiểm tra nếu không có ghế nào được chọn
            if (lstChonGhe.Count == 0)
            {
                txtTongTien.Text = "0 VNĐ";
                return;
            }

            // Tính tổng tiền cho các ghế được chọn
            decimal tongTien = 0;
            foreach (Button item in lstChonGhe)
            {
                if (item.BackColor == Color.LightBlue) // Chỉ tính những ghế có màu LightBlue
                {
                    tongTien += TinhTienGhe(item); // Gọi phương thức tính tiền cho từng ghế
                }
            }

            // Hiển thị tổng tiền
            txtTongTien.Text = tongTien.ToString("N2", new CultureInfo("vi-VN")) + " VNĐ";
        }
        private decimal TinhTienGhe(Button ghe)
        {
            int GheChon = int.Parse(ghe.Text);
            if (GheChon <= 4)
                return 3000;
            else if (GheChon <= 8)
                return 4000;
            else if (GheChon <= 12)
                return 5000;
            else if (GheChon <= 16)
                return 6000;
            else
                return 8000;
        }
        private void btnChon_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra nếu không có ghế nào được chọn
                if (!lstChonGhe.Any(item => item.BackColor == Color.LightBlue))
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một ghế!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Tính tổng tiền và thay đổi màu ghế thành màu vàng
                decimal tongTien = 0;
                foreach (Button item in lstChonGhe)
                {
                    item.BackColor = Color.Yellow; // Đổi màu ghế đã chọn thành màu vàng
                    tongTien += TinhTienGhe(item); // Cộng tiền của từng ghế vào tổng tiền
                }

                // Lấy thông tin giới tính
                string gioiTinh = optNam.Checked ? "Nam" : "Nữ";

                // Tạo danh sách chi tiết hóa đơn
                List<CTHD> ChiTietHD = new List<CTHD>();

                foreach (Button item in lstChonGhe)
                {
                    CTHD cTHD = new CTHD
                    {
                        vitrighe = item.Text, // Lưu vị trí ghế
                        sotien = TinhTienGhe(item) // Lưu số tiền của ghế
                    };
                    ChiTietHD.Add(cTHD);
                }

                // Lưu thông tin đơn hàng và hóa đơn
                LuuThongTinDonHang(txtName.Text, txtSDT.Text, cmbKhuVuc.Text, gioiTinh, DateTime.Now, tongTien, ChiTietHD);

                // Load lại dữ liệu hóa đơn để hiển thị
                LoadHoaDonData();

                // Xóa danh sách ghế đã chọn
                lstChonGhe.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Bạn có muốn thoát?", "Lựa Chọn", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("Bạn có muốn thoát?", "Lựa Chọn", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No)
            {
                e.Cancel = true;
            }
        }


        private void LuuThongTinDonHang(string tenKH, string sdt, string diachi, string gioitinh, DateTime ngayMua, decimal tongTien, List<CTHD> CTHDList)
        {
            using (var context = new BanVeCineEntities()) // Tạo DbContext để kết nối với database
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Tạo mới khách hàng
                        var khachHangMoi = new KHACHHANG
                        {
                            ten = tenKH, // Tên khách hàng
                            sdt = sdt, // Số điện thoại
                            diachi = diachi, // Địa chỉ
                            gioitinh = gioitinh // Giới tính
                        };

                        // Thêm khách hàng mới vào DbSet của KHACHHANG
                        context.KHACHHANG.Add(khachHangMoi);
                        context.SaveChanges(); // Lưu lại để Entity Framework sinh mã khách hàng tự động

                        // 2. Tạo mới hóa đơn cho khách hàng này
                        var hoaDonMoi = new HOADON
                        {
                            ngay = ngayMua, // Ngày lập hóa đơn
                            maKH = khachHangMoi.maKH, // Liên kết mã khách hàng với hóa đơn
                            sotien = tongTien// Tổng số tiền cho tất cả ghế
                        };

                        // Thêm hóa đơn mới vào DbSet của HOADON
                        context.HOADON.Add(hoaDonMoi);
                        context.SaveChanges(); // Lưu lại để Entity Framework sinh mã hóa đơn tự động

                        // 3. Tạo các chi tiết hóa đơn cho hóa đơn này
                        foreach (var chitiet in CTHDList)
                        {
                            var cthdMoi = new CTHD
                            {
                                maHD = hoaDonMoi.maHD, // Liên kết mã hóa đơn với chi tiết hóa đơn
                                vitrighe = chitiet.vitrighe, // Vị trí ghế
                                sotien = chitiet.sotien // Số tiền cho từng ghế
                            };
                            // Thêm chi tiết hóa đơn vào DbSet của CTHD
                            context.CTHD.Add(cthdMoi);
                        }
                        // 4. Lưu tất cả thay đổi vào cơ sở dữ liệu
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message);
                    }
                }
 
            }
        }

        private void LoadHoaDonData()
        {
            using (var context = new BanVeCineEntities())
            {
                var hoaDonData = (from hd in context.HOADON
                                  join kh in context.KHACHHANG on hd.maKH equals kh.maKH
                                  select new
                                  {
                                      MaHoaDon = hd.maHD,
                                      TenKhachHang = kh.ten,
                                      NgayDat = hd.ngay,
                                      TongTien = hd.sotien
                                  });

                    dgvKhachHang.DataSource = hoaDonData.ToList();
            }
        }




        private void LoadDanhSachGheDaBan()
        {
            using (var context = new BanVeCineEntities())
            {
                var gheDaBan = (from ghe in context.CTHD
                                select ghe.vitrighe).ToList();
                foreach (Button ghe in grbViTriGheNgoi.Controls.OfType<Button>())
                {

                }
            }
        }
    }
}
