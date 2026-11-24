using System.Text;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement;

public partial class BookCopyControl : UserControl
{
    private readonly CatalogManagementService _service = new();
    private int? _filterBookId;

    public BookCopyControl()
    {
        InitializeComponent();
        WireEvents();
        LoadBookCopies();
    }

    public void FilterByBook(int bookId, string bookTitle)
    {
        _filterBookId = bookId;
        lblDescription.Text = $"Danh sách bản sách vật lý thuộc đầu sách: {bookTitle}";
        LoadBookCopies();
    }

    private void WireEvents()
    {
        txtKeyword.TextChanged += (_, _) => LoadBookCopies();
        btnClear.Click += (_, _) =>
        {
            _filterBookId = null;
            lblDescription.Text = "Theo dõi mã vạch, vị trí kệ và tình trạng từng bản sách.";
            txtKeyword.Clear();
            LoadBookCopies();
        };

        btnAdd.Click += (_, _) => OpenCopyDialog(null);
        btnEdit.Click += (_, _) =>
        {
            if (GetSelectedCopy() is { } c) OpenCopyDialog(c);
        };
        btnDetail.Click += (_, _) => ShowCopyDetail();
        btnRepair.Click += (_, _) => ChangeStatus("Maintenance", "Damaged", "Chuyển sang trạng thái bảo trì / sửa chữa");
        btnRepairComplete.Click += (_, _) => ChangeStatus("Available", "Good", "Hoàn tất sửa chữa, đưa trở lại lưu thông");
        btnLost.Click += (_, _) => ChangeStatus("Lost", null, "Báo mất bản sách");
        btnHistory.Click += (_, _) => ShowStatusHistory();
        btnExport.Click += (_, _) => ExportCsv();

        // Bổ sung nút Thanh lý và Xóa vào panelActions sẵn có
        var btnDispose = CreateExtraActionButton("Thanh lý", 991);
        btnDispose.Click += (_, _) => ChangeStatus("Disposed", null, "Thanh lý bản sách không còn đủ điều kiện lưu thông");

        var btnDelete = CreateExtraActionButton("Xóa bản sách", 1082);
        btnDelete.ForeColor = Color.FromArgb(200, 40, 40);
        btnDelete.Click += (_, _) => DeleteSelectedCopy();

        panelActions.Controls.Add(btnDispose);
        panelActions.Controls.Add(btnDelete);
    }

    private static Button CreateExtraActionButton(string text, int x)
    {
        var b = new Button
        {
            Text = text,
            BackColor = Color.FromArgb(248, 248, 248),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.8F),
            Location = new Point(x, 9),
            Size = new Size(98, 32),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);
        return b;
    }

    public void LoadBookCopies()
    {
        var copies = _service.SearchBookCopies(txtKeyword.Text, _filterBookId);
        dgvBookCopies.Rows.Clear();

        foreach (var c in copies)
        {
            int idx = dgvBookCopies.Rows.Add(
                c.CopyCode,
                c.Book?.BookCode ?? "-",
                c.Book?.Title ?? "-",
                c.CopyCode, // Mã vạch riêng biệt
                c.ShelfLocation ?? "Chưa xếp kệ",
                TranslateCondition(c.PhysicalCondition),
                TranslateStatus(c.CirculationStatus),
                c.ReceivedAt,
                $"{c.PurchasePrice:N0} đ"
            );
            dgvBookCopies.Rows[idx].Tag = c;
        }

        lblRecordCount.Text = $"Tổng số bản sách: {copies.Count}";
    }

    private BookCopies? GetSelectedCopy()
    {
        if (dgvBookCopies.CurrentRow?.Tag is BookCopies c) return c;
        MessageBox.Show("Vui lòng chọn một bản sách trong bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void OpenCopyDialog(BookCopies? copy)
    {
        using var dlg = new BookCopyEditDialog(copy, _filterBookId);
        if (dlg.ShowDialog() == DialogResult.OK)
            LoadBookCopies();
    }

    private void ChangeStatus(string newStatus, string? newCond, string defaultReason)
    {
        var copy = GetSelectedCopy();
        if (copy == null) return;

        try
        {
            _service.ChangeCopyStatus(copy.Id, newStatus, newCond, defaultReason);
            MessageBox.Show($"Đã cập nhật trạng thái bản sách '{copy.CopyCode}' thành {TranslateStatus(newStatus)}.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadBookCopies();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void DeleteSelectedCopy()
    {
        var copy = GetSelectedCopy();
        if (copy == null) return;

        if (MessageBox.Show($"Xác nhận xóa bản sách '{copy.CopyCode}'?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        try
        {
            _service.DeleteBookCopy(copy.Id);
            MessageBox.Show("Đã xóa bản sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadBookCopies();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể xóa bản sách", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowCopyDetail()
    {
        var c = GetSelectedCopy();
        if (c == null) return;

        string info =
            $"Mã bản sách / Mã vạch: {c.CopyCode}\n" +
            $"Đầu sách: {c.Book?.Title} ({c.Book?.BookCode})\n" +
            $"Vị trí kệ: {c.ShelfLocation ?? "Chưa gán"}\n" +
            $"Tình trạng vật lý: {TranslateCondition(c.PhysicalCondition)}\n" +
            $"Trạng thái lưu thông: {TranslateStatus(c.CirculationStatus)}\n" +
            $"Ngày nhập: {c.ReceivedAt}\n" +
            $"Giá nhập: {c.PurchasePrice:N0} đ\n" +
            $"Nguồn nhập: {c.Source ?? "Không rõ"}\n" +
            $"Ghi chú: {c.Notes ?? "Không có"}";

        MessageBox.Show(info, $"Chi tiết bản sách {c.CopyCode}", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowStatusHistory()
    {
        var c = GetSelectedCopy();
        if (c == null) return;

        var histories = _service.GetCopyHistories(c.Id);
        if (histories.Count == 0)
        {
            MessageBox.Show("Bản sách này chưa có lịch sử thay đổi trạng thái.", "Lịch sử trạng thái", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var sb = new StringBuilder();
        foreach (var h in histories)
        {
            sb.AppendLine($"[{h.ChangedAt}] {h.ActionType}");
            if (h.OldStatus != h.NewStatus)
                sb.AppendLine($"   - Trạng thái: {h.OldStatus ?? "N/A"} -> {h.NewStatus}");
            if (h.OldCondition != h.NewCondition)
                sb.AppendLine($"   - Tình trạng: {h.OldCondition ?? "N/A"} -> {h.NewCondition}");
            if (!string.IsNullOrWhiteSpace(h.Note))
                sb.AppendLine($"   - Ghi chú: {h.Note}");
            sb.AppendLine();
        }

        MessageBox.Show(sb.ToString(), $"Lịch sử thay đổi trạng thái - {c.CopyCode}", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ExportCsv()
    {
        using var sfd = new SaveFileDialog { Filter = "CSV File|*.csv", FileName = $"DanhSachBanSach_{DateTime.Now:yyyyMMdd}.csv" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("MaBanSach,MaDauSach,TenSach,ViTriKe,TinhTrangVatLy,TrangThaiLuuThong,NgayNhap,GiaNhap");
        foreach (DataGridViewRow row in dgvBookCopies.Rows)
        {
            var vals = row.Cells.Cast<DataGridViewCell>().Select(c => $"\"{c.Value}\"");
            sb.AppendLine(string.Join(",", vals));
        }
        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string TranslateCondition(string cond) => cond switch
    {
        "New" => "Mới",
        "Good" => "Tốt",
        "Worn" => "Cũ",
        "Damaged" => "Hư hỏng / Cần sửa chữa",
        _ => cond
    };

    private static string TranslateStatus(string st) => st switch
    {
        "Available" => "Có sẵn",
        "Borrowed" => "Đang mượn",
        "Reserved" => "Đang giữ chỗ",
        "Maintenance" => "Đang bảo trì",
        "Lost" => "Bị mất",
        "Disposed" => "Thanh lý",
        _ => st
    };
}

public class BookCopyEditDialog : Form
{
    private readonly CatalogManagementService _service = new();
    private readonly BookCopies _copy;

    private ComboBox _cboBook = null!;
    private TextBox _txtCopyCode = null!;
    private TextBox _txtShelf = null!;
    private DateTimePicker _dtpReceived = null!;
    private NumericUpDown _numPrice = null!;
    private TextBox _txtSource = null!;
    private ComboBox _cboCondition = null!;
    private ComboBox _cboStatus = null!;
    private TextBox _txtNote = null!;

    public BookCopyEditDialog(BookCopies? copy, int? defaultBookId)
    {
        _copy = copy ?? new BookCopies
        {
            IdBook = defaultBookId ?? 0,
            ReceivedAt = DateTime.Today.ToString("yyyy-MM-dd"),
            PhysicalCondition = "New",
            CirculationStatus = "Available"
        };

        Text = copy == null ? "Thêm mới bản sách vật lý" : $"Cập nhật bản sách - {copy.CopyCode}";
        Size = new Size(520, 470);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi();
        BindData();
    }

    private void BuildUi()
    {
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _cboBook = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _txtCopyCode = new TextBox { Dock = DockStyle.Fill };
        _txtShelf = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "VD: Kệ A2 - Tầng 3" };
        _dtpReceived = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        _numPrice = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 100_000_000, Increment = 5000 };
        _txtSource = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "VD: NXB Trẻ / Mua mới / Biếu tặng" };

        _cboCondition = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboCondition.Items.AddRange(["New", "Good", "Worn", "Damaged"]);

        _cboStatus = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboStatus.Items.AddRange(["Available", "Borrowed", "Reserved", "Maintenance", "Lost", "Disposed"]);

        _txtNote = new TextBox { Dock = DockStyle.Fill };

        AddRow(tbl, 0, "Đầu sách (*):", _cboBook);
        AddRow(tbl, 1, "Mã bản sách / Barcode:", _txtCopyCode);
        AddRow(tbl, 2, "Vị trí kệ sách:", _txtShelf);
        AddRow(tbl, 3, "Ngày nhập sách:", _dtpReceived);
        AddRow(tbl, 4, "Giá nhập (VNĐ):", _numPrice);
        AddRow(tbl, 5, "Nguồn nhập:", _txtSource);
        AddRow(tbl, 6, "Tình trạng vật lý:", _cboCondition);
        AddRow(tbl, 7, "Trạng thái lưu thông:", _cboStatus);
        AddRow(tbl, 8, "Ghi chú:", _txtNote);

        var pnlBottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(16, 6, 16, 6) };
        var btnCancel = new Button { Text = "Hủy", Width = 85, Height = 32 };
        var btnSave = new Button { Text = "Lưu bản sách", Width = 115, Height = 32, BackColor = Color.FromArgb(19, 98, 160), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        btnSave.Click += BtnSave_Click;

        pnlBottom.Controls.Add(btnCancel);
        pnlBottom.Controls.Add(btnSave);

        Controls.Add(tbl);
        Controls.Add(pnlBottom);
    }

    private static void AddRow(TableLayoutPanel tbl, int row, string label, Control c)
    {
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
        tbl.Controls.Add(c, 1, row);
    }

    private void BindData()
    {
        var books = _service.SearchBooks(null, null, true);
        _cboBook.DataSource = books;
        _cboBook.DisplayMember = "Title";
        _cboBook.ValueMember = "Id";

        if (_copy.IdBook > 0)
            _cboBook.SelectedValue = _copy.IdBook;

        _cboBook.Enabled = _copy.Id == 0;
        _cboBook.SelectedIndexChanged += (_, _) =>
        {
            if (_copy.Id == 0 && _cboBook.SelectedValue is int bId)
                _txtCopyCode.Text = _service.GenerateNextCopyCode(bId);
        };

        if (_copy.Id == 0 && _cboBook.SelectedValue is int initBookId)
            _txtCopyCode.Text = _service.GenerateNextCopyCode(initBookId);
        else
            _txtCopyCode.Text = _copy.CopyCode;

        _txtShelf.Text = _copy.ShelfLocation;
        if (DateTime.TryParse(_copy.ReceivedAt, out var dt)) _dtpReceived.Value = dt;
        _numPrice.Value = _copy.PurchasePrice;
        _txtSource.Text = _copy.Source;
        _cboCondition.SelectedItem = _copy.PhysicalCondition;
        _cboStatus.SelectedItem = _copy.CirculationStatus;
        _txtNote.Text = _copy.Notes;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            _copy.IdBook = _cboBook.SelectedValue is int id ? id : 0;
            _copy.CopyCode = _txtCopyCode.Text;
            _copy.ShelfLocation = _txtShelf.Text;
            _copy.ReceivedAt = _dtpReceived.Value.ToString("yyyy-MM-dd");
            _copy.PurchasePrice = (int)_numPrice.Value;
            _copy.Source = _txtSource.Text;
            _copy.PhysicalCondition = _cboCondition.SelectedItem?.ToString() ?? "Good";
            _copy.CirculationStatus = _cboStatus.SelectedItem?.ToString() ?? "Available";
            _copy.Notes = _txtNote.Text;

            _service.SaveBookCopy(_copy);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}