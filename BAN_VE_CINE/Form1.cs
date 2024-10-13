using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace BAN_VE_CINE
{
    public partial class Form1 : Form
    {
        private List<Button> lstChonGhe = new List<Button>();

        public Form1()
        {
            InitializeComponent();
        }

        public void SetGridViewStyle(DataGridView dgview)
        {
            dgview.BorderStyle = BorderStyle.None;
            dgview.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgview.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgview.BackgroundColor = Color.White;
            dgview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetGridViewStyle(dgvKhachHang);
            CaiDatThongTin();
            LoadHoaDonData();
            LoadGheDaBan();
        }

        private void CaiDatThongTin()
        {
            dgvKhachHang.Columns.Add("MaHoaDon", "Mã Hóa Đơn");
            dgvKhachHang.Columns.Add("TenKhachHang", "Tên Khách Hàng");
            // Thêm cột Giới tính
            dgvKhachHang.Columns.Add("GioiTinh", "Giới Tính");
            // Thêm cột Số Điện Thoại
            dgvKhachHang.Columns.Add("SoDienThoai", "Số Điện Thoại");

            // Thêm cột Khu Vực
            dgvKhachHang.Columns.Add("KhuVuc", "Khu Vực");

            dgvKhachHang.Columns.Add("NgayDat", "Ngày Đặt");
            dgvKhachHang.Columns["NgayDat"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvKhachHang.Columns.Add("TongTien", "Tổng Tiền");

            // Đặt định dạng cho cột Tổng Tiền
            dgvKhachHang.Columns["TongTien"].DefaultCellStyle.Format = "N0"; // Định dạng số nguyên (20,000)


            using (BanVeCineEntities dbcontext = new BanVeCineEntities())
            {
                var item = from a in dbcontext.KHACHHANG
                           join b in dbcontext.HOADON on a.maKH equals b.maKH
                           join c in dbcontext.CTHD on b.maHD equals c.maHD
                           select new
                           {
                               MaHoaDon = b.maHD,
                               TenKhachHang = a.ten,
                               GioiTinh = a.gioitinh, // Giả sử có thuộc tính 'gioitinh' trong KHACHHANG
                               SoDienThoai = a.sdt, // Giả sử có thuộc tính 'sdt' trong KHACHHANG
                               KhuVuc = a.diachi, // Giả sử có thuộc tính 'diachi' trong KHACHHANG
                               NgayDat = b.ngay,
                               TongTien = c.sotien
                           };

                foreach (var c in item.ToList())
                {
                    dgvKhachHang.Rows.Add(c.MaHoaDon, c.TenKhachHang, c.NgayDat, c.TongTien, c.GioiTinh, c.KhuVuc, c.SoDienThoai);
                }

                dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                cmbKhuVuc.Items.AddRange(new string[] { "Quận 9", "Thủ Đức", "Bình Thạnh", "Quận 1", "Quận 5", "Hóc Môn", "Bình Dương" });
                cmbKhuVuc.SelectedIndex = -1;
                //optNu.Checked = true;
                txtTongTien.Text = "0 VNĐ";
                txtTongTien.ReadOnly = true;
            }
        }


        private void btnChonGhe_Click(object sender, EventArgs e)
        {
            Button btnChonGhe = (Button)sender;

            if (btnChonGhe.BackColor == Color.Yellow)
            {
                MessageBox.Show("Ghế đã được mua!!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (btnChonGhe.BackColor == Color.White)
            {
                btnChonGhe.BackColor = Color.LightBlue;
                lstChonGhe.Add(btnChonGhe);
            }
            else if (btnChonGhe.BackColor == Color.LightBlue)
            {
                btnChonGhe.BackColor = Color.White;
                lstChonGhe.Remove(btnChonGhe);
            }

            TinhTongTien();
        }


        private void btnHuy_Click(object sender, EventArgs e)
        {
            foreach (Button item in lstChonGhe.ToList())
            {
                if (item.BackColor == Color.LightBlue)
                {
                    item.BackColor = Color.White;
                    lstChonGhe.Remove(item);
                }
            }

            txtTongTien.Text = "0 VNĐ";
        }

        private void TinhTongTien()
        {
            decimal tongTien = lstChonGhe.Where(item => item.BackColor == Color.LightBlue).Sum(item => TinhTienGhe(item));

            txtTongTien.Text = tongTien == 0 ? "0 VNĐ" : tongTien.ToString("N2", new CultureInfo("vi-VN")) + " VNĐ";
        }

        private decimal TinhTienGhe(Button ghe)
        {
            int GheChon = int.Parse(ghe.Text);
            if (GheChon <= 4) return 3000;
            else if (GheChon <= 8) return 4000;
            else if (GheChon <= 12) return 5000;
            else if (GheChon <= 16) return 6000;
            else return 8000;
        }

        private void btnChon_Click(object sender, EventArgs e)
        {
            try
            {
                if (KiemTraSDTTonTaiTrongDGV() == false)
                {
                    if (KiemTraNhapLieu())
                    {
                        if (!lstChonGhe.Any(item => item.BackColor == Color.LightBlue))
                        {
                            MessageBox.Show("Vui lòng chọn ít nhất một ghế!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        decimal tongTien = 0;
                        foreach (Button item in lstChonGhe)
                        {
                            item.BackColor = Color.Yellow;
                            tongTien += TinhTienGhe(item);
                        }

                        string gioiTinh = optNam.Checked ? "Nam" : "Nữ";
                        List<CTHD> ChiTietHD = lstChonGhe.Where(item => item.BackColor == Color.Yellow).Select(item => new CTHD
                        {
                            vitrighe = item.Text,
                            sotien = TinhTienGhe(item)
                        }).ToList();

                        LuuThongTinDonHang(txtName.Text, txtSDT.Text, cmbKhuVuc.Text, gioiTinh, DateTime.Now, tongTien, ChiTietHD);
                        txtTongTien.Text = tongTien.ToString("N2", new CultureInfo("vi-VN")) + " VNĐ";
                        MessageBox.Show("Thêm Khách hàng thành công!", "Thông báo", MessageBoxButtons.OK);
                        LoadHoaDonData();
                        lstChonGhe.Clear();
                        ResetInput();
                    }
                }
                else 
                {
                        MessageBox.Show("Khách hàng đã tồn tại", "Thông báo", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Hàm kiểm tra nhập liệu
        private bool KiemTraNhapLieu()
        {
            errorProvider1.Clear();
            errorProvider2.Clear();
            errorProvider3.Clear();
            errorProvider4.Clear();  // Đảm bảo xóa sạch lỗi trước

            bool isValid = true;

            // Kiểm tra tên
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider1.SetError(txtName, "Vui lòng nhập tên!");
                isValid = false;
            }

            // Kiểm tra số điện thoại
            if (string.IsNullOrWhiteSpace(txtSDT.Text))
            {
                errorProvider2.SetError(txtSDT, "Vui lòng nhập số điện thoại!");
                isValid = false;
            }
            else if (!txtSDT.Text.All(char.IsDigit))
            {
                errorProvider2.SetError(txtSDT, "Số điện thoại phải là chữ số!");
            }
            else if (txtSDT.Text.Length != 10)
            {
                errorProvider2.SetError(txtSDT, "Số điện thoại phải có 10 chữ số!");
                isValid = false;
            }

            // Kiểm tra khu vực
            if (cmbKhuVuc.SelectedIndex == -1)
            {
                errorProvider3.SetError(cmbKhuVuc, "Vui lòng chọn khu vực!");
                isValid = false;
            }

            // Kiểm tra giới tính
            if (!optNam.Checked && !optNu.Checked)
            {
                errorProvider4.SetError(optNu, "Vui lòng chọn giới tính!");
                isValid = false;
            }

            return isValid;
        }
        private bool KiemTraSDTTonTaiTrongDGV()
        {
            // Duyệt qua tất cả các hàng trong dgvKhachHang
            foreach (DataGridViewRow row in dgvKhachHang.Rows)
            {
                // Kiểm tra nếu cột Số Điện Thoại (giả sử cột số 3) có giá trị trùng với txtSDT.Text
                if (row.Cells["SoDienThoai"].Value != null && row.Cells["SoDienThoai"].Value.ToString() == txtSDT.Text)
                {
                    return true; // Nếu trùng thì trả về true
                }
            }
            return false; // Không tìm thấy thì trả về false
        }

        // Hàm reset input
        private void ResetInput()
        {
            txtName.Clear();
            txtSDT.Clear();
            optNu.Checked = false;
            optNam.Checked = false;
            cmbKhuVuc.SelectedIndex = -1;
        }
        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát?", "Lựa Chọn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát?", "Lựa Chọn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void LuuThongTinDonHang(string tenKH, string sdt, string diachi, string gioitinh, DateTime ngayMua, decimal tongTien, List<CTHD> CTHDList)
        {
            using (var context = new BanVeCineEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var khachHangMoi = new KHACHHANG { ten = tenKH, sdt = sdt, diachi = diachi, gioitinh = gioitinh };
                        context.KHACHHANG.Add(khachHangMoi);
                        context.SaveChanges();

                        var hoaDonMoi = new HOADON { ngay = DateTime.Now, maKH = khachHangMoi.maKH, sotien = tongTien };
                        context.HOADON.Add(hoaDonMoi);
                        context.SaveChanges();

                        foreach (var chitiet in CTHDList)
                        {
                            context.CTHD.Add(new CTHD { maHD = hoaDonMoi.maHD, vitrighe = chitiet.vitrighe, sotien = chitiet.sotien });
                        }

                        context.SaveChanges();
                        transaction.Commit();  // Commit transaction
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();  // Rollback transaction on error
                        MessageBox.Show("Có lỗi xảy ra khi lưu thông tin đơn hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void LoadHoaDonData()
        {
            dgvKhachHang.Rows.Clear(); // Clear the existing rows before loading new data

            using (var context = new BanVeCineEntities())
            {
                var hoaDonData = (from hd in context.HOADON
                                  join kh in context.KHACHHANG on hd.maKH equals kh.maKH
                                  select new
                                  {
                                      MaHoaDon = hd.maHD,
                                      TenKhachHang = kh.ten,
                                      GioiTinh = kh.gioitinh, // Thêm Giới tính
                                      SoDienThoai = kh.sdt, // Thêm Số Điện Thoại
                                      KhuVuc = kh.diachi, // Thêm Khu Vực
                                      NgayDat = hd.ngay,
                                      TongTien = hd.sotien
                                  }).ToList();

                foreach (var c in hoaDonData)
                {
                    dgvKhachHang.Rows.Add(
                        c.MaHoaDon,
                        c.TenKhachHang,
                        c.GioiTinh,
                        c.SoDienThoai,
                        c.KhuVuc,
                        c.NgayDat?.ToString("dd/MM/yyyy HH:mm") ?? "N/A",  // Hiển thị Ngày và giờ
                        c.TongTien
                    );
                }
            }
        }

        private void LoadGheDaBan()
        {
            using (var context = new BanVeCineEntities())
            {
                // Lấy danh sách ghế đã bán từ CTHD
                var gheDaBan = (from ghe in context.CTHD
                               select ghe.vitrighe).ToList();

                // Duyệt qua danh sách ghế đã bán và cập nhật màu sắc cho các nút ghế
                foreach (Button btnGhe in grbViTriGheNgoi.Controls.OfType<Button>())
                {
                    if (gheDaBan.Contains(btnGhe.Text))
                    {
                        btnGhe.Enabled = false; // Không cho phép ch?n gh? ?? b?n
                        btnGhe.BackColor = Color.Yellow; // Đặt màu vàng cho ghế đã bán
                    }
                    else
                    {
                        btnGhe.BackColor = Color.White;    // Đổi màu ghế đã bị xóa thành màu trắng
                        btnGhe.Enabled = true;
                    }
                }
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            try
            {
                string sdt = txtSDT.Text.Trim();

                if (string.IsNullOrEmpty(sdt))
                {
                    MessageBox.Show("Vui lòng nhập số điện thoại khách hàng để xóa!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var context = new BanVeCineEntities())
                {
                    // Tìm khách hàng dựa trên số điện thoại
                    var khachHang = context.KHACHHANG.FirstOrDefault(kh => kh.sdt == sdt);

                    if (khachHang == null)
                    {
                        MessageBox.Show("Không tìm thấy khách hàng với số điện thoại này!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show("Bạn có chắc muốn xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            // Tìm các hóa đơn liên quan đến khách hàng đó
                            var hoaDons = context.HOADON.Where(hd => hd.maKH == khachHang.maKH).ToList();

                            // Xóa các chi tiết hóa đơn
                            foreach (var hoaDon in hoaDons)
                            {
                                var chiTietHDs = context.CTHD.Where(ct => ct.maHD == hoaDon.maHD).ToList();
                                context.CTHD.RemoveRange(chiTietHDs);
                            }

                            // Xóa các hóa đơn
                            context.HOADON.RemoveRange(hoaDons);

                            // Xóa khách hàng
                            context.KHACHHANG.Remove(khachHang);

                            // Lưu thay đổi vào cơ sở dữ liệu
                            context.SaveChanges();

                            MessageBox.Show("Xóa thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Cập nhật lại DataGridView
                            LoadHoaDonData();
                            LoadGheDaBan();
                            ResetInput();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            try
            {
                string sdt = txtSDT.Text.Trim();

                if (string.IsNullOrEmpty(sdt))
                {
                    MessageBox.Show("Vui lòng nhập số điện thoại khách hàng để sửa!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var context = new BanVeCineEntities())
                {
                    // Tìm khách hàng dựa trên số điện thoại
                    var khachHang = context.KHACHHANG.FirstOrDefault(kh => kh.sdt == sdt);

                    if (khachHang == null)
                    {
                        MessageBox.Show("Không tìm thấy khách hàng với số điện thoại này!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Cập nhật thông tin khách hàng
                    khachHang.ten = txtName.Text;
                    khachHang.diachi = cmbKhuVuc.Text;
                    khachHang.gioitinh = optNam.Checked ? "Nam" : "Nữ";

                    // Lưu thay đổi vào cơ sở dữ liệu
                    context.SaveChanges();

                    MessageBox.Show("Sửa thông tin khách hàng thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Cập nhật lại DataGridView
                    LoadHoaDonData();
                    ResetInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kiểm tra chỉ số hàng và cột có hợp lệ hay không
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow row = dgvKhachHang.Rows[e.RowIndex];

                // Gán giá trị từ DataGridView vào các TextBox, kiểm tra giá trị null trước khi gán
                txtName.Text = row.Cells[1].Value != null ? row.Cells[1].Value.ToString() : string.Empty;

                // Kiểm tra và gán giá trị cho các RadioButton dựa trên giới tính
                string gender = row.Cells[2].Value != null ? row.Cells[2].Value.ToString() : string.Empty;
                if (gender == "Nữ")
                {
                    optNu.Checked = true;
                }
                else if (gender == "Nam")
                {
                    optNam.Checked = true;
                }

                // Gán số điện thoại và khu vực
                txtSDT.Text = row.Cells[3].Value != null ? row.Cells[3].Value.ToString() : string.Empty;
                cmbKhuVuc.Text = row.Cells[4].Value != null ? row.Cells[4].Value.ToString() : string.Empty;
            }
        }


    }
}
