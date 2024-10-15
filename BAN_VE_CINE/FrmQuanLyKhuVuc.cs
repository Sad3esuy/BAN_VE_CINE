using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace BAN_VE_CINE
{
    public partial class FrmQuanLyKhuVuc : Form
    {
        public FrmQuanLyKhuVuc()
        {
            InitializeComponent();
            // Set TabIndex
            txtMaKV.TabIndex = 0;    // Focus sẽ chuyển vào đây đầu tiên
            txtTenKV.TabIndex = 1;   // Focus sẽ chuyển vào đây khi nhấn Tab từ txtMaKV
            btnThem.TabIndex = 2;    // Focus sẽ chuyển vào đây khi nhấn Tab từ txtTenKV
        }

        // Thiết lập style cho DataGridView
        public void SetGridViewStyle(DataGridView dgview)
        {
            dgview.BorderStyle = BorderStyle.None;
            dgview.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgview.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgview.BackgroundColor = Color.White;
            dgview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Khi form load, thiết lập cột và load dữ liệu
        private void FrmQuanLyKhuVuc_Load(object sender, EventArgs e)
        {
            SetGridViewStyle(dgvKhuVuc);

            if (dgvKhuVuc.Columns.Count == 0)
            {
                dgvKhuVuc.Columns.Add("MaKhuVuc", "Mã Khu Vực");
                dgvKhuVuc.Columns.Add("TenKhuVuc", "Tên Khu Vực");
            }

            LoadKhuVuc();
        }

        // Nút thêm khu vực mới
        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                if (KiemTraNhapLieu())
                {
                    using (BanVeCineEntities dbcontext = new BanVeCineEntities())
                    {
                        var existingKhuVuc = dbcontext.KHUVUC.FirstOrDefault(s => s.maKV == txtMaKV.Text);
                        if (existingKhuVuc == null)
                        {
                            KHUVUC newKhuVuc = new KHUVUC()
                            {
                                maKV = txtMaKV.Text,
                                tenKV = txtTenKV.Text
                            };

                            dbcontext.KHUVUC.Add(newKhuVuc);
                            dbcontext.SaveChanges();
                            LoadKhuVuc();
                            MessageBox.Show("Thêm mới khu vực thành công!");
                            ResetInput();
                        }
                        else
                        {
                            MessageBox.Show("Mã khu vực đã tồn tại!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load dữ liệu từ database và hiển thị trên DataGridView
        private void LoadKhuVuc()
        {
            using (BanVeCineEntities dbcontext = new BanVeCineEntities())
            {
                dgvKhuVuc.Rows.Clear();
                var items = from kv in dbcontext.KHUVUC
                            select new
                            {
                                MaKhuVuc = kv.maKV,
                                TenKhuVuc = kv.tenKV
                            };

                foreach (var c in items.ToList())
                {
                    dgvKhuVuc.Rows.Add(c.MaKhuVuc, c.TenKhuVuc);
                }
            }
        }

        // Nút xóa khu vực
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtMaKV.Text))
            {
                using (BanVeCineEntities dbcontext = new BanVeCineEntities())
                {
                    var existingKhuVuc = dbcontext.KHUVUC.FirstOrDefault(s => s.maKV == txtMaKV.Text);
                    if (existingKhuVuc != null)
                    {
                        DialogResult result = MessageBox.Show("Bạn có chắc muốn xóa khu vực này?", "Xác nhận", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            dbcontext.KHUVUC.Remove(existingKhuVuc);
                            dbcontext.SaveChanges();
                            LoadKhuVuc();
                            MessageBox.Show("Xóa khu vực thành công!", "Thông báo", MessageBoxButtons.OK);
                            ResetInput();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy mã khu vực cần xoá!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng nhập Mã Khu vực để xóa!");
            }
        }

        // Kiểm tra nhập liệu
        private bool KiemTraNhapLieu()
        {
            errorProvider1.Clear();
            errorProvider2.Clear();

            bool isValid = true;

            // Kiểm tra Mã Khu Vực
            if (string.IsNullOrWhiteSpace(txtMaKV.Text))
            {
                errorProvider1.SetError(txtMaKV, "Vui lòng nhập Mã khu vực!");
                isValid = false;
            }
            else if (!txtMaKV.Text.All(char.IsDigit))
            {
                errorProvider1.SetError(txtMaKV, "Mã khu vực phải là chữ số!");
                isValid = false;
            }
            else if (txtMaKV.Text.Length != 2)
            {
                errorProvider1.SetError(txtMaKV, "Mã khu vực phải có 2 chữ số!");
                isValid = false;
            }

            // Kiểm tra Tên Khu Vực
            if (string.IsNullOrWhiteSpace(txtTenKV.Text))
            {
                errorProvider2.SetError(txtTenKV, "Vui lòng nhập Tên khu vực!");
                isValid = false;
            }

            return isValid;
        }

        // Reset input sau khi thêm hoặc xóa
        private void ResetInput()
        {
            txtMaKV.Clear();
            txtTenKV.Clear();
        }

        // Xử lý khi chọn một hàng trong DataGridView
        private void dgvKhuVuc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvKhuVuc.Rows[e.RowIndex];
                txtMaKV.Text = row.Cells[0].Value?.ToString() ?? string.Empty;
                txtTenKV.Text = row.Cells[1].Value?.ToString() ?? string.Empty;
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Bạn có muốn thoát?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes) {
                this.Close();
            }
        }

        private void txtMaKV_KeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu phím Tab được nhấn
            if (e.KeyCode == Keys.Tab)
            {
                // Chuyển focus sang TextBox tiếp theo (txtTenKV)
                txtTenKV.Focus();
                e.SuppressKeyPress = true; // Ngăn chặn âm thanh hệ thống khi nhấn Tab
            }
        }

        private void txtTenKV_KeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu phím Tab được nhấn
            if (e.KeyCode == Keys.Tab)
            {
                // Bạn có thể chuyển focus sang control tiếp theo hoặc thực hiện hành động khác
                btnThem.Focus(); // Ví dụ chuyển focus sang nút Thêm
                e.SuppressKeyPress = true; // Ngăn chặn âm thanh hệ thống khi nhấn Tab
            }
        }

    }
}
