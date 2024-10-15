using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using System.IO;
using OfficeOpenXml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data.Entity;

namespace BAN_VE_CINE
{
    public partial class Form1 : Form
    {
        private List<Button> lstChonGhe = new List<Button>();

        public Form1()
        {
            InitializeComponent();
            // Set TabIndex
            txtName.TabIndex = 0;    // Focus sẽ chuyển vào đây đầu tiên
            txtSDT.TabIndex = 1;   // Focus sẽ chuyển vào đây khi nhấn Tab từ txtMaKV
            cmbKhuVuc.TabIndex = 2;    // Focus sẽ chuyển vào đây khi nhấn Tab từ txtTenKV
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

            // Thêm cột Ngày Đặt
            dgvKhachHang.Columns.Add("NgayDat", "Ngày Đặt");
            dgvKhachHang.Columns["NgayDat"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"; // Thêm giờ và phút

            // Thêm cột Tổng Tiền
            dgvKhachHang.Columns.Add("TongTien", "Tổng Tiền");
            dgvKhachHang.Columns["TongTien"].DefaultCellStyle.Format = "N0"; // Định dạng số nguyên (20,000)

            using (BanVeCineEntities dbcontext = new BanVeCineEntities())
            {
                var item = from a in dbcontext.KHACHHANG
                           join b in dbcontext.HOADON on a.maKH equals b.maKH
                           join c in dbcontext.CTHD on b.maHD equals c.maHD
                           join kv in dbcontext.KHUVUC on a.maKV equals kv.maKV
                           select new
                           {
                               MaHoaDon = b.maHD,
                               TenKhachHang = a.ten,
                               GioiTinh = a.gioitinh,
                               SoDienThoai = a.sdt,
                               KhuVuc = kv.tenKV,  // Lấy tên khu vực từ bảng KHUVUC
                               NgayDat = b.ngay,
                               TongTien = c.sotien
                           };

                foreach (var c in item.ToList())
                {
                    dgvKhachHang.Rows.Add(c.MaHoaDon, c.TenKhachHang, c.GioiTinh, c.SoDienThoai, c.KhuVuc, c.NgayDat.HasValue ? c.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : "N/A", // Kiểm tra có giá trị không
                                  string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0}", c.TongTien)); // Định dạng tổng tiền kiểu tiền Việt);
                }

                dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                //cmbKhuVuc.Items.AddRange(new string[] { "Quận 9", "Thủ Đức", "Bình Thạnh", "Quận 1", "Quận 5", "Hóc Môn", "Bình Dương" });
                List<KHUVUC> listKhuVuc = dbcontext.KHUVUC.ToList(); //lấy các khuc vuc
                FillKhuVucCombobox(listKhuVuc);
                cmbKhuVuc.SelectedIndex = -1;
                //optNu.Checked = true;
                txtTongTien.Text = "0 VNĐ";
                txtTongTien.ReadOnly = true;
                cmbKhuVuc.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }
        //Hàm binding list có tên hiện thị là tên khoa, giá trị là Mã khoa
        private void FillKhuVucCombobox(List<KHUVUC> listKhuVuc)
        {
            this.cmbKhuVuc.DataSource = listKhuVuc;
            this.cmbKhuVuc.DisplayMember = "tenKV";
            this.cmbKhuVuc.ValueMember = "maKV";
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

            txtTongTien.Text = tongTien == 0 ? "0 VNĐ" : tongTien.ToString("#,##0 VND");
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
                        txtTongTien.Text = tongTien.ToString("#,##0 VND");
                        MessageBox.Show("Thêm Khách hàng thành công!", "Thông báo", MessageBoxButtons.OK);
                        LoadHoaDonData();
                        lstChonGhe.Clear();
                        ResetInput();
                    }
                }
                else
                {
                    MessageBox.Show("Khách hàng đã tồn tại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            txtTongTien.Text = "0 VNĐ";
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void LuuThongTinDonHang(string tenKH, string sdt, string tenKV, string gioitinh, DateTime ngayMua, decimal tongTien, List<CTHD> CTHDList)
        {
            using (var context = new BanVeCineEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Tìm hoặc thêm khu vực mới nếu khu vực không tồn tại
                        var khuVuc = context.KHUVUC.FirstOrDefault(kv => kv.tenKV == tenKV);
                        if (khuVuc == null)
                        {
                            khuVuc = new KHUVUC { tenKV = tenKV };
                            context.KHUVUC.Add(khuVuc);
                            context.SaveChanges(); // Lưu lại để có maKV cho khu vực mới
                        }

                        // Tạo khách hàng mới với thông tin mã khu vực
                        var khachHangMoi = new KHACHHANG
                        {
                            ten = tenKH,
                            sdt = sdt,
                            maKV = khuVuc.maKV,  // Ánh xạ mã khu vực từ bảng KHUVUC
                            gioitinh = gioitinh
                        };
                        context.KHACHHANG.Add(khachHangMoi);
                        context.SaveChanges();

                        // Tạo hóa đơn mới
                        var hoaDonMoi = new HOADON
                        {
                            ngay = ngayMua,  // Sử dụng ngày mua truyền vào
                            maKH = khachHangMoi.maKH,
                            sotien = tongTien
                        };
                        context.HOADON.Add(hoaDonMoi);
                        context.SaveChanges();

                        // Lưu chi tiết hóa đơn
                        foreach (var chitiet in CTHDList)
                        {
                            context.CTHD.Add(new CTHD
                            {
                                maHD = hoaDonMoi.maHD,
                                vitrighe = chitiet.vitrighe,
                                sotien = chitiet.sotien
                            });
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
                                  join kv in context.KHUVUC on kh.maKV equals kv.maKV // Join thêm với bảng KHUVUC để lấy tên khu vực
                                  select new
                                  {
                                      MaHoaDon = hd.maHD,
                                      TenKhachHang = kh.ten,
                                      GioiTinh = kh.gioitinh, // Thêm Giới tính
                                      SoDienThoai = kh.sdt, // Thêm Số Điện Thoại
                                      KhuVuc = kv.tenKV, // Lấy tên khu vực từ bảng KHUVUC
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
                        c.KhuVuc, // Hiển thị tên khu vực
                        c.NgayDat.HasValue ? c.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",  // Kiểm tra có giá trị không
                        string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:C}", c.TongTien) // Định dạng tổng tiền kiểu tiền Việt
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

                    // Tìm khu vực dựa trên tên khu vực từ ComboBox
                    var khuVuc = context.KHUVUC.FirstOrDefault(kv => kv.tenKV == cmbKhuVuc.Text);
                    if (khuVuc == null)
                    {
                        MessageBox.Show("Khu vực không hợp lệ!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Cập nhật thông tin khách hàng
                    khachHang.ten = txtName.Text;
                    khachHang.maKV = khuVuc.maKV; // Cập nhật mã khu vực dựa trên lựa chọn từ ComboBox
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

        // viết hàm để xuất data trong dgvKhachHang thành file excel
        private void ExportFile(string path)
        {
            try
            {
                // Tạo các đối tượng Excel
                Excel.Application oExcel = new Excel.Application();
                Excel.Workbooks oBooks;
                Excel.Sheets oSheets;
                Excel.Workbook oBook;
                Excel.Worksheet oSheet;

                // Tạo mới một Excel WorkBook 
                oExcel.Visible = false;
                oExcel.DisplayAlerts = false;
                oExcel.Application.SheetsInNewWorkbook = 1;
                oBooks = oExcel.Workbooks;
                oBook = (Excel.Workbook)(oExcel.Workbooks.Add(Type.Missing));
                oSheets = oBook.Worksheets;
                oSheet = (Excel.Worksheet)oSheets.get_Item(1);

                // Đặt tên sheet
                oSheet.Name = "Danh sách hóa đơn";

                // Tạo phần Tiêu đề
                Excel.Range head = oSheet.get_Range("A1", "G1");
                head.MergeCells = true;
                head.Value2 = "Danh Sách Hóa Đơn";
                head.Font.Bold = true;
                head.Font.Name = "Times New Roman";
                head.Font.Size = 20;
                head.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Tạo tiêu đề cột từ dgvKhachHang
                for (int i = 0; i < dgvKhachHang.Columns.Count; i++)
                {
                    Excel.Range columnHeader = oSheet.Cells[3, i + 1];  // Dòng 3 là dòng tiêu đề
                    columnHeader.Value2 = dgvKhachHang.Columns[i].HeaderText;  // Lấy tiêu đề từ dgvKhachHang
                    columnHeader.Font.Bold = true;
                    columnHeader.Borders.LineStyle = Excel.Constants.xlSolid;
                    columnHeader.Interior.ColorIndex = 6;  // Màu nền cho tiêu đề
                    columnHeader.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                }

                // Chuyển dữ liệu từ dgvKhachHang sang Excel
                for (int i = 0; i < dgvKhachHang.Rows.Count; i++)
                {
                    for (int j = 0; j < dgvKhachHang.Columns.Count; j++)
                    {
                        oSheet.Cells[i + 4, j + 1] = dgvKhachHang.Rows[i].Cells[j].Value;  // Dữ liệu bắt đầu từ dòng 4
                    }
                }

                // Kẻ viền cho dữ liệu
                int rowStart = 4;
                int rowEnd = rowStart + dgvKhachHang.Rows.Count - 1;
                Excel.Range c1 = (Excel.Range)oSheet.Cells[rowStart, 1];
                Excel.Range c2 = (Excel.Range)oSheet.Cells[rowEnd, dgvKhachHang.Columns.Count];
                Excel.Range range = oSheet.get_Range(c1, c2);

                range.Borders.LineStyle = Excel.Constants.xlSolid;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Định dạng cột "Tổng Tiền" theo tiền Việt Nam
                Excel.Range totalColumn = oSheet.get_Range("G4", $"G{dgvKhachHang.Rows.Count + 3}");  // Dòng 4 là bắt đầu dữ liệu
                totalColumn.NumberFormat = "#,##0 \"VND\"";

                // Lưu file Excel
                oExcel.ActiveWorkbook.SaveCopyAs(path);
                oExcel.ActiveWorkbook.Saved = true;
                oBook.Close();
                //oExcel.Quit();

                // Giải phóng tài nguyên
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oSheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ExportExCel(string path)
        {
            try
            {
                // Tạo các đối tượng Excel
                Excel.Application oExcel = new Excel.Application();
                Excel.Workbooks oBooks;
                Excel.Sheets oSheets;
                Excel.Workbook oBook;
                Excel.Worksheet oSheet;

                // Tạo mới một Excel WorkBook 
                oExcel.Visible = true;
                oExcel.DisplayAlerts = false;
                oExcel.Application.SheetsInNewWorkbook = 1;
                oBooks = oExcel.Workbooks;
                oBook = (Excel.Workbook)(oExcel.Workbooks.Add(Type.Missing));
                oSheets = oBook.Worksheets;
                oSheet = (Excel.Worksheet)oSheets.get_Item(1);

                // Đặt tên sheet
                oSheet.Name = "DANH SÁCH KHÁCH HÀNG";

                // Tạo phần Tiêu đề
                Excel.Range head = oSheet.get_Range("A1", "G1");
                head.MergeCells = true;
                head.Value2 = "DANH SÁCH KHÁCH HÀNG";
                head.Font.Bold = true;
                head.Font.Name = "Times New Roman";
                head.Font.Size = 20;
                head.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Tạo tiêu đề cột từ dgvKhachHang
                for (int i = 0; i < dgvKhachHang.Columns.Count; i++)
                {
                    Excel.Range columnHeader = oSheet.Cells[3, i + 1];  // Dòng 3 là dòng tiêu đề
                    columnHeader.Value2 = dgvKhachHang.Columns[i].HeaderText;  // Lấy tiêu đề từ dgvKhachHang
                    columnHeader.Font.Bold = true;
                    columnHeader.Borders.LineStyle = Excel.Constants.xlSolid;
                    columnHeader.Interior.ColorIndex = 6;  // Màu nền cho tiêu đề
                    columnHeader.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                }

                // Chuyển dữ liệu từ dgvKhachHang sang Excel
                for (int i = 0; i < dgvKhachHang.Rows.Count; i++)
                {
                    for (int j = 0; j < dgvKhachHang.Columns.Count; j++)
                    {
                        oSheet.Cells[i + 4, j + 1] = dgvKhachHang.Rows[i].Cells[j].Value;  // Dữ liệu bắt đầu từ dòng 4
                    }
                }

                // Kẻ viền cho dữ liệu
                int rowStart = 4;
                int rowEnd = rowStart + dgvKhachHang.Rows.Count - 1;
                Excel.Range c1 = (Excel.Range)oSheet.Cells[rowStart, 1];
                Excel.Range c2 = (Excel.Range)oSheet.Cells[rowEnd, dgvKhachHang.Columns.Count];
                Excel.Range range = oSheet.get_Range(c1, c2);

                range.Borders.LineStyle = Excel.Constants.xlSolid;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Lưu file Excel
                oExcel.Columns.AutoFit();
                //oExcel.ActiveWorkbook.SaveCopyAs(path);
                //oExcel.ActiveWorkbook.Saved = true;
                oBook.SaveAs(path);
                //oBook.Close();
                oExcel.Quit();

                // Giải phóng tài nguyên
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oSheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void tsmnXuatFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export Excel";
            saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportExCel(saveFileDialog.FileName);
                    MessageBox.Show("Xuất File Thành Công!","Thông báo",MessageBoxButtons.OK);
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Xuất File Không Thành Công!!\n",ex.Message);
                }
                
            }
        }

        private void btnThemKhuVuc_Click(object sender, EventArgs e)
        {
            FrmQuanLyKhuVuc form2 = new FrmQuanLyKhuVuc();
            form2.FormClosed += new FormClosedEventHandler(form2_FormClosed); 
            form2.ShowDialog();
        }

        private void form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            BanVeCineEntities dbcontext = new BanVeCineEntities();
            List<KHUVUC> listKhuVuc = dbcontext.KHUVUC.ToList(); //lấy các khuc vuc
            FillKhuVucCombobox(listKhuVuc);
            cmbKhuVuc.SelectedIndex = -1;
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu phím Tab được nhấn
            if (e.KeyCode == Keys.Tab)
            {
                // Chuyển focus sang TextBox tiếp theo (txtTenKV)
                txtSDT.Focus();
                e.SuppressKeyPress = true; // Ngăn chặn âm thanh hệ thống khi nhấn Tab
            }
        }

        private void txtSDT_KeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu phím Tab được nhấn
            if (e.KeyCode == Keys.Tab)
            {
                // Bạn có thể chuyển focus sang control tiếp theo hoặc thực hiện hành động khác
                cmbKhuVuc.Focus(); // Ví dụ chuyển focus sang nút Thêm
                e.SuppressKeyPress = true; // Ngăn chặn âm thanh hệ thống khi nhấn Tab
            }
        }
    }
}
