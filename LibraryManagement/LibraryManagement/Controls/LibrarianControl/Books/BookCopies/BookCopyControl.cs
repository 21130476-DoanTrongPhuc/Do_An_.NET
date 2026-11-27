using System.Text;
using LibraryManagement.Enums;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement;

public partial class BookCopyControl : UserControl
{
    private readonly CatalogManagementService _service = new();
    private int? _filterBookId;
    public event Action? CopyDataChanged;

    public BookCopyControl()
    {
        InitializeComponent();
        dgvBookCopies.Columns.Remove(dataGridViewTextBoxColumn4);
        WireEvents();
        LoadBookCopies();
    }

    public void FilterByBook(int bookId, string bookTitle)
    {
        _filterBookId = bookId;
        lblDescription.Text = $"Danh sách bản sách vật lý thuộc đầu sách: {bookTitle}";
        LoadBookCopies();
    }

    // ============================================================
    //  WIRE EVENTS
    // ============================================================
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

        btnRepair.Click += (_, _) => ChangeStatus(
            CirculationStatus.Maintenance,
            BookCondition.Damaged,
            "Chuyển sang trạng thái bảo trì / sửa chữa");

        btnRepairComplete.Click += (_, _) => ChangeStatus(
            CirculationStatus.Available,
            BookCondition.Good,
            "Hoàn tất sửa chữa, đưa trở lại lưu thông");

        btnLost.Click += (_, _) => ChangeStatus(
            CirculationStatus.Maintenance,
            BookCondition.Lost,
            "Báo mất bản sách");

        btnHistory.Click += (_, _) => ShowStatusHistory();
        btnExport.Click += (_, _) => ExportCsv();

        var btnDispose = CreateExtraActionButton("Thanh lý", btnExport.Right + 8);        btnDispose.Click += (_, _) => ChangeStatus(
            CirculationStatus.Disposed,
            null,
            "Thanh lý bản sách không còn đủ điều kiện lưu thông");

        var btnDelete = CreateExtraActionButton("Xóa bản sách", btnDispose.Right + 8);        btnDelete.ForeColor = Color.FromArgb(200, 40, 40);
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

    // ============================================================
    //  LOAD DATA
    // ============================================================
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
                // c.Barcode ?? "-",                        // ✅ mã vạch riêng, không lặp CopyCode
                c.ShelfLocation ?? "Chưa xếp kệ",
                TranslateCondition(c.PhysicalCondition)!, // ✅ enum
                TranslateStatus(c.CirculationStatus)!,    // ✅ enum
                c.ReceivedAt,
                $"{c.PurchasePriceVnd:N0} đ"
            );
            dgvBookCopies.Rows[idx].Tag = c;
        }

        lblRecordCount.Text = $"Tổng số bản sách: {copies.Count}";
    }

    private BookCopies? GetSelectedCopy()
    {
        if (dgvBookCopies.CurrentRow?.Tag is BookCopies c) return c;
        MessageBox.Show("Vui lòng chọn một bản sách trong bảng.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void OpenCopyDialog(BookCopies? copy)
    {
        using var dlg = new BookCopyEditDialog(copy, _filterBookId);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
        LoadBookCopies();
        CopyDataChanged?.Invoke();
    }
}

    // ============================================================
    //  ĐỔI TRẠNG THÁI
    // ============================================================
    private void ChangeStatus(CirculationStatus newStatus, BookCondition? newCond, string defaultReason)
    {
        var copy = GetSelectedCopy();
        if (copy == null) return;

        // Chặn thanh lý khi sách đang được mượn
        if (newStatus == CirculationStatus.Disposed
            && copy.CirculationStatus == CirculationStatus.Borrowed)
        {
            MessageBox.Show("Không thể thanh lý bản sách đang được mượn.", "Không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _service.ChangeCopyStatus(copy.Id, newStatus, newCond, defaultReason);
            string updatedStatus = newCond == BookCondition.Lost
                ? TranslateCondition(newCond)!
                : TranslateStatus(newStatus)!;
            MessageBox.Show(
                $"Đã cập nhật trạng thái bản sách '{copy.CopyCode}' thành {updatedStatus}.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadBookCopies();
            CopyDataChanged?.Invoke();
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
            MessageBox.Show("Đã xóa bản sách.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadBookCopies();
            CopyDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể xóa bản sách",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ============================================================
    //  CHI TIẾT / LỊCH SỬ
    // ============================================================
    private void ShowCopyDetail()
    {
        var c = GetSelectedCopy();
        if (c == null) return;

        string info =
            $"Mã bản sách: {c.CopyCode}\n" +
            // $"Mã vạch: {c.Barcode ?? "Chưa gán"}\n" +
            $"Đầu sách: {c.Book?.Title} ({c.Book?.BookCode})\n" +
            $"Vị trí kệ: {c.ShelfLocation ?? "Chưa gán"}\n" +
            $"Tình trạng vật lý: {TranslateCondition(c.PhysicalCondition)}\n" +
            $"Trạng thái lưu thông: {TranslateStatus(c.CirculationStatus)}\n" +
            $"Ngày nhập: {c.ReceivedAt}\n" +
            $"Giá nhập: {c.PurchasePriceVnd:N0} đ\n" +
            $"Nguồn nhập: {c.Source ?? "Không rõ"}\n" +
            $"Ghi chú: {c.Notes ?? "Không có"}";

        MessageBox.Show(info, $"Chi tiết bản sách {c.CopyCode}",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowStatusHistory()
    {
        var c = GetSelectedCopy();
        if (c == null) return;

        var histories = _service.GetCopyHistories(c.Id);
        if (histories.Count == 0)
        {
            MessageBox.Show("Bản sách này chưa có lịch sử thay đổi trạng thái.",
                "Lịch sử trạng thái", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var sb = new StringBuilder();
        foreach (var h in histories)
        {
            sb.AppendLine($"[{h.ChangedAt}] {h.ActionType}");

            // Nếu OldStatus/NewStatus là enum không nullable:
            if (h.OldStatus != h.NewStatus)
                sb.AppendLine($"   - Trạng thái: {TranslateStatus(h.OldStatus)} -> {TranslateStatus(h.NewStatus)}");

            if (h.OldCondition != h.NewCondition)
                sb.AppendLine($"   - Tình trạng: {TranslateCondition(h.OldCondition)} -> {TranslateCondition(h.NewCondition)}");

            if (!string.IsNullOrWhiteSpace(h.Note))
                sb.AppendLine($"   - Ghi chú: {h.Note}");
            sb.AppendLine();
        }

        MessageBox.Show(sb.ToString(), $"Lịch sử thay đổi trạng thái - {c.CopyCode}",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ============================================================
    //  XUẤT CSV — lấy dữ liệu thô từ entity, không phụ thuộc hiển thị
    // ============================================================
    private void ExportCsv()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV File|*.csv",
            FileName = $"DanhSachBanSach_{DateTime.Now:yyyyMMdd}.csv"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("MaBanSach,MaDauSach,TenSach,ViTriKe,TinhTrangVatLy,TrangThaiLuuThong,NgayNhap,GiaNhap");

        foreach (DataGridViewRow row in dgvBookCopies.Rows)
        {
            if (row.Tag is not BookCopies c) continue;

            string[] cells =
            [
                c.CopyCode,
                // c.Barcode ?? "",
                c.Book?.BookCode ?? "",
                c.Book?.Title ?? "",
                c.ShelfLocation ?? "",
                TranslateCondition(c.PhysicalCondition),
                TranslateStatus(c.CirculationStatus),
                c.ReceivedAt,
                c.PurchasePriceVnd.ToString("N0")
            ];

            sb.AppendLine(string.Join(",", cells.Select(EscapeCsv)));
        }

        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show("Xuất file CSV thành công!", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string EscapeCsv(string value) =>
        $"\"{value.Replace("\"", "\"\"")}\"";

    // ============================================================
    //  TRANSLATE — nhận enum, không nhận string
    // ============================================================
    private static string? TranslateCondition(BookCondition? cond) => cond switch
    {
        BookCondition.New     => "Mới",
        BookCondition.Good    => "Tốt",
        BookCondition.Worn    => "Cũ",
        BookCondition.Damaged => "Hư hỏng / Cần sửa chữa",
        BookCondition.Lost    => "Bị mất",
        _                     => cond.ToString()
    };

    private static string? TranslateStatus(CirculationStatus? st) => st switch
    {
        CirculationStatus.Available   => "Có sẵn",
        CirculationStatus.Borrowed    => "Đang mượn",
        CirculationStatus.Reserved    => "Đang giữ chỗ",
        CirculationStatus.Maintenance => "Đang bảo trì",
        // CirculationStatus.Lost        => "Bị mất",
        CirculationStatus.Disposed    => "Thanh lý",
        _                             => st.ToString()
    };
}

// ================================================================
//  DIALOG THÊM / SỬA BẢN SÁCH — dùng enum xuyên suốt
// ================================================================
public class BookCopyEditDialog : Form
{
    private readonly CatalogManagementService _service = new();
    private readonly BookCopies _copy;

    private ComboBox _cboBook = null!;
    private TextBox _txtCopyCode = null!;
    private TextBox _txtBarcode = null!;
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
            PhysicalCondition = BookCondition.New,          // ✅ enum
            CirculationStatus = CirculationStatus.Available // ✅ enum
        };

        Text = copy == null ? "Thêm mới bản sách vật lý" : $"Cập nhật bản sách - {copy.CopyCode}";
        Size = new Size(520, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi();
        BindData();
    }

    private void BuildUi()
    {
        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16)
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _cboBook      = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _txtCopyCode  = new TextBox  { Dock = DockStyle.Fill };
        _txtBarcode   = new TextBox  { Dock = DockStyle.Fill, PlaceholderText = "Quét hoặc nhập mã vạch" };
        _txtShelf     = new TextBox  { Dock = DockStyle.Fill, PlaceholderText = "VD: Kệ A2 - Tầng 3" };
        _dtpReceived  = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        _numPrice     = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 100_000_000, Increment = 5000 };
        _txtSource    = new TextBox  { Dock = DockStyle.Fill, PlaceholderText = "VD: NXB Trẻ / Mua mới / Biếu tặng" };
        _txtNote      = new TextBox  { Dock = DockStyle.Fill };

        // ✅ ComboBox chứa enum trực tiếp
        _cboCondition = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboCondition.Items.AddRange(new object[]
        {
            BookCondition.New, BookCondition.Good, BookCondition.Worn,
            BookCondition.Damaged, BookCondition.Lost
        });

        _cboStatus = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboStatus.Items.AddRange(new object[]
        {
            CirculationStatus.Available, 
            CirculationStatus.Borrowed,
            CirculationStatus.Reserved, 
            CirculationStatus.Maintenance,
            // CirculationStatus.Lost, 
            CirculationStatus.Disposed
        });

        AddRow(tbl, 0, "Đầu sách (*):", _cboBook);
        AddRow(tbl, 1, "Mã bản sách:", _txtCopyCode);
        AddRow(tbl, 2, "Mã vạch / Barcode:", _txtBarcode);
        AddRow(tbl, 3, "Vị trí kệ sách:", _txtShelf);
        AddRow(tbl, 4, "Ngày nhập sách:", _dtpReceived);
        AddRow(tbl, 5, "Giá nhập (VNĐ):", _numPrice);
        AddRow(tbl, 6, "Nguồn nhập:", _txtSource);
        AddRow(tbl, 7, "Tình trạng vật lý:", _cboCondition);
        AddRow(tbl, 8, "Trạng thái lưu thông:", _cboStatus);
        AddRow(tbl, 9, "Ghi chú:", _txtNote);

        var pnlBottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 46,
            FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(16, 6, 16, 6)
        };
        var btnCancel = new Button { Text = "Hủy", Width = 85, Height = 32 };
        var btnSave = new Button
        {
            Text = "Lưu bản sách", Width = 115, Height = 32,
            BackColor = Color.FromArgb(19, 98, 160), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
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
        tbl.Controls.Add(new Label
        {
            Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill
        }, 0, row);
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

        // _txtBarcode.Text = _copy.Barcode;
        _txtShelf.Text = _copy.ShelfLocation;
        if (DateTime.TryParse(_copy.ReceivedAt, out var dt)) _dtpReceived.Value = dt;
        _numPrice.Value = _copy.PurchasePriceVnd;
        _txtSource.Text = _copy.Source;

        // ✅ SelectedItem là enum, gán trực tiếp
        _cboCondition.SelectedItem = _copy.PhysicalCondition;
        _cboStatus.SelectedItem = _copy.CirculationStatus;
        _txtNote.Text = _copy.Notes;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            _copy.IdBook = _cboBook.SelectedValue is int id ? id : 0;
            _copy.CopyCode = _txtCopyCode.Text.Trim();
            // _copy.Barcode = string.IsNullOrWhiteSpace(_txtBarcode.Text) ? null : _txtBarcode.Text.Trim();
            _copy.ShelfLocation = _txtShelf.Text;
            _copy.ReceivedAt = _dtpReceived.Value.ToString("yyyy-MM-dd");
            _copy.PurchasePriceVnd = (int)_numPrice.Value;
            _copy.Source = _txtSource.Text;

            // ✅ Cast enum an toàn, fallback khi SelectedItem null
            _copy.PhysicalCondition = _cboCondition.SelectedItem is BookCondition cond
                ? cond : BookCondition.Good;
            _copy.CirculationStatus = _cboStatus.SelectedItem is CirculationStatus st
                ? st : CirculationStatus.Available;

            _copy.Notes = _txtNote.Text;

            _service.SaveBookCopy(_copy);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi nhập liệu",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}