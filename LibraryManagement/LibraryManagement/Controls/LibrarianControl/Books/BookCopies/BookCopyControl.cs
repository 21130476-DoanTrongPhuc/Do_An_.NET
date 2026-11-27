using System.Drawing.Drawing2D;
using System.Text;
using LibraryManagement.Enums;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement;

public class BookCopyControl : UserControl
{
    // ====== Bảng màu & Font đồng bộ hệ thống ======
    private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
    private static readonly Color BgGray     = Color.FromArgb(240, 242, 245);
    private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
    private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
    private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
    private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
    private static readonly Color Green      = Color.FromArgb(34, 160, 90);
    private static readonly Color Red        = Color.FromArgb(200, 40, 40);
    private const string IconFont = "Segoe MDL2 Assets";

    // Glyph icons
    private const string GAdd      = "\uE710";
    private const string GEdit     = "\uE70F";
    private const string GInfo     = "\uE946";
    private const string GRepair   = "\uE90F";
    private const string GCheck    = "\uE73E";
    private const string GWarning  = "\uE7BA";
    private const string GHistory  = "\uE81C";
    private const string GExport   = "\uEDE1";
    private const string GDispose  = "\uE74D";
    private const string GSearch   = "\uE721";
    private const string GRefresh  = "\uE72C";

    private readonly CatalogManagementService _service = new();
    private int? _filterBookId;
    public event Action? CopyDataChanged;

    // ====== UI Controls ======
    private Label _lblDescription = null!;
    private TextBox _txtKeyword = null!;
    private DataGridView _dgvBookCopies = null!;

    public BookCopyControl()
    {
        BackColor = BgGray;
        Font = new Font("Segoe UI", 9F);
        BuildUi();
        LoadBookCopies();
    }

    public void FilterByBook(int bookId, string bookTitle)
    {
        _filterBookId = bookId;
        _lblDescription.Text = $"Đang lọc theo đầu sách: {bookTitle}";
        LoadBookCopies();
    }

    // ============================================================
    //  DỰNG GIAO DIỆN THUẦN CODE (KHÔNG CẦN DESIGNER)
    // ============================================================
    private void BuildUi()
    {
        SuspendLayout();

        // 1. Vùng Top: Header + Thanh công cụ nghiệp vụ + Thanh tìm kiếm
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 160, BackColor = BgGray };
        pnlTop.Controls.Add(BuildFilterBar());
        pnlTop.Controls.Add(BuildActionToolbar());
        pnlTop.Controls.Add(BuildHeader());

        // 2. Vùng Main: Bảng danh sách bản sách
        var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
        var grpList = new GroupBox
        {
            Text = "Danh sách bản sách vật lý trong kho",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 8, 10, 10)
        };

        _dgvBookCopies = BuildGrid();
        grpList.Controls.Add(_dgvBookCopies);
        pnlMain.Controls.Add(grpList);

        Controls.Add(pnlMain);
        Controls.Add(pnlTop);

        ResumeLayout(true);
    }

    private Panel BuildHeader()
    {
        var pnl = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = SoftBlue };
        pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ SÁCH VẬT LÝ & BẢN SAO TRONG KHO",
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Location = new Point(14, 8)
        };
        pnl.Controls.Add(lblTitle);

        _lblDescription = new Label
        {
            Text = "Theo dõi mã bản sách, vị trí kệ và tình trạng lưu thông của từng bản sách.",
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(16, 32)
        };
        pnl.Controls.Add(_lblDescription);

        // Dùng FlowLayoutPanel neo bên phải để các nút tự co giãn theo đúng độ dài chữ
        var flowActions = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var btnRefresh = CreateActionButton("Làm mới", GRefresh, Color.White, Color.Black);
        btnRefresh.Click += (_, _) => LoadBookCopies();

        var btnAdd = CreateActionButton("Thêm bản sách", GAdd, Primary, Color.White);
        btnAdd.Click += (_, _) => OpenCopyDialog(null);

        flowActions.Controls.AddRange(new Control[] { btnRefresh, btnAdd });
        pnl.Controls.Add(flowActions);

        void LayoutHeader()
        {
            flowActions.Location = new Point(
                Math.Max(lblTitle.Right + 16, pnl.ClientSize.Width - flowActions.Width - 12),
                (pnl.ClientSize.Height - flowActions.Height) / 2
            );
        }

        pnl.Resize += (_, _) => LayoutHeader();
        LayoutHeader();

        return pnl;
    }

    private Panel BuildActionToolbar()
    {
        var pnl = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.White,
            Padding = new Padding(10, 7, 10, 7)
        };
        pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var btnEdit = CreateCompactBtn("Sửa", GEdit, Color.White, Color.Black);
        btnEdit.Click += (_, _) => { if (GetSelectedCopy() is { } c) OpenCopyDialog(c); };

        var btnDetail = CreateCompactBtn("Chi tiết", GInfo, SoftBlue, Primary);
        btnDetail.Click += (_, _) => ShowCopyDetail();

        var btnRepair = CreateCompactBtn("Ghi nhận sửa chữa", GRepair, Color.FromArgb(255, 248, 235), Orange);
        btnRepair.Click += (_, _) => ChangeStatus(CirculationStatus.Maintenance, BookCondition.Damaged, "Chuyển sang trạng thái bảo trì / sửa chữa");

        var btnRepairComplete = CreateCompactBtn("Hoàn tất sửa", GCheck, Color.FromArgb(236, 250, 242), Green);
        btnRepairComplete.Click += (_, _) => ChangeStatus(CirculationStatus.Available, BookCondition.Good, "Hoàn tất sửa chữa, đưa trở lại lưu thông");

        var btnLost = CreateCompactBtn("Báo mất", GWarning, Color.FromArgb(253, 238, 238), Red);
        btnLost.Click += (_, _) => ChangeStatus(CirculationStatus.Maintenance, BookCondition.Lost, "Báo mất bản sách");

        var btnHistory = CreateCompactBtn("Lịch sử", GHistory, Color.White, Color.Black);
        btnHistory.Click += (_, _) => ShowStatusHistory();

        var btnDispose = CreateCompactBtn("Thanh lý", GDispose, Color.White, TextGray);
        btnDispose.Click += (_, _) => ChangeStatus(CirculationStatus.Disposed, null, "Thanh lý bản sách không còn đủ điều kiện lưu thông");

        var btnDelete = CreateCompactBtn("Xóa bản sách", GDispose, Color.White, Red);
        btnDelete.Click += (_, _) => DeleteSelectedCopy();

        var btnExport = CreateCompactBtn("Xuất CSV", GExport, SoftBlue, Primary);
        btnExport.Click += (_, _) => ExportCsv();

        flow.Controls.AddRange(new Control[]
        {
            btnEdit, btnDetail, btnRepair, btnRepairComplete, btnLost, btnHistory, btnDispose, btnDelete, btnExport
        });

        pnl.Controls.Add(flow);
        return pnl;
    }

    private Panel BuildFilterBar()
    {
        var pnl = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            BackColor = Color.White,
            Padding = new Padding(12, 8, 12, 8)
        };
        pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

        var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, ForeColor = TextGray, Location = new Point(14, 16) };

        _txtKeyword = new TextBox
        {
            Location = new Point(82, 12),
            Width = 340,
            PlaceholderText = "Mã bản sách, mã đầu sách, tên sách hoặc vị trí kệ..."
        };
        _txtKeyword.TextChanged += (_, _) => LoadBookCopies();

        var btnClear = CreateActionButton("Xóa lọc", GSearch, SoftBlue, Primary);
        btnClear.MinimumSize = new Size(0, 31);
        btnClear.MaximumSize = new Size(0, 31);
        btnClear.Click += (_, _) =>
        {
            _filterBookId = null;
            _lblDescription.Text = "Theo dõi mã bản sách, vị trí kệ và tình trạng lưu thông của từng bản sách.";
            _txtKeyword.Clear();
            LoadBookCopies();
        };

        pnl.Controls.AddRange(new Control[] { lblSearch, _txtKeyword, btnClear });
        pnl.Resize += (_, _) =>
        {
            btnClear.Left = pnl.ClientSize.Width - btnClear.Width - 12;
            btnClear.Top = 9;
            _txtKeyword.Left = lblSearch.Right + 10;
            _txtKeyword.Width = Math.Max(200, btnClear.Left - _txtKeyword.Left - 14);
            lblSearch.Top = (pnl.ClientSize.Height - lblSearch.Height) / 2;
        };

        return pnl;
    }

    private DataGridView BuildGrid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(225, 228, 233),
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 40 },
            ColumnHeadersHeight = 36,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 9F)
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;

        g.Columns.Add("colCopyCode",  "Mã bản sách");
        g.Columns.Add("colBookCode",  "Mã đầu sách");
        g.Columns.Add("colBookTitle", "Tên đầu sách");
        g.Columns.Add("colShelf",     "Vị trí kệ");
        g.Columns.Add("colCondition", "Tình trạng vật lý");
        g.Columns.Add("colStatus",    "Trạng thái lưu thông");
        g.Columns.Add("colReceived",  "Ngày nhập");
        g.Columns.Add("colPrice",     "Giá nhập");

        g.Columns["colCopyCode"]!.DefaultCellStyle.ForeColor = Primary;
        g.Columns["colCopyCode"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        g.Columns["colCopyCode"]!.FillWeight  = 85;
        g.Columns["colBookCode"]!.FillWeight  = 80;
        g.Columns["colBookTitle"]!.FillWeight = 155;
        g.Columns["colShelf"]!.FillWeight     = 90;
        g.Columns["colCondition"]!.FillWeight = 105;
        g.Columns["colStatus"]!.FillWeight    = 110;
        g.Columns["colReceived"]!.FillWeight  = 80;
        g.Columns["colPrice"]!.FillWeight     = 80;

        g.CellPainting += DgvBookCopies_CellPainting;
        g.CellDoubleClick += (_, _) => { if (GetSelectedCopy() is { } c) OpenCopyDialog(c); };
        return g;
    }

    private void DgvBookCopies_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 5 || e.Graphics == null) return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
        string text = e.Value?.ToString() ?? "";
        Color c = text switch
        {
            "Có sẵn"        => Green,
            "Đang mượn"     => Primary,
            "Đang giữ chỗ"  => Orange,
            "Đang bảo trì"  => Red,
            "Thanh lý"      => TextGray,
            _               => TextGray
        };

        var font = new Font("Segoe UI", 8.5F);
        string label = "●  " + text;
        var size = TextRenderer.MeasureText(label, font);
        var rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 25) / 2, size.Width + 8, 25);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var path = RoundedRect(rect, 12))
        using (var fill = new SolidBrush(Color.FromArgb(22, c)))
        using (var pen  = new Pen(Color.FromArgb(150, c)))
        {
            e.Graphics.FillPath(fill, path);
            e.Graphics.DrawPath(pen, path);
        }
        TextRenderer.DrawText(e.Graphics, label, font, rect, c,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        e.Handled = true;
    }

    // ============================================================
    //  LOAD DATA & NGHIỆP VỤ
    // ============================================================
    public void LoadBookCopies()
    {
        var copies = _service.SearchBookCopies(_txtKeyword.Text, _filterBookId);
        _dgvBookCopies.Rows.Clear();

        foreach (var c in copies)
        {
            int idx = _dgvBookCopies.Rows.Add(
                c.CopyCode,
                c.Book?.BookCode ?? "-",
                c.Book?.Title ?? "-",
                c.ShelfLocation ?? "Chưa xếp kệ",
                TranslateCondition(c.PhysicalCondition)!,
                TranslateStatus(c.CirculationStatus)!,
                c.ReceivedAt,
                $"{c.PurchasePriceVnd:N0} đ"
            );
            _dgvBookCopies.Rows[idx].Tag = c;
        }

        if (_filterBookId == null)
            _lblDescription.Text = $"Tổng số bản sách trong kho: {copies.Count:N0} bản  ·  Theo dõi mã bản sách, vị trí kệ và tình trạng lưu thông.";
    }

    private BookCopies? GetSelectedCopy()
    {
        if (_dgvBookCopies.CurrentRow?.Tag is BookCopies c) return c;
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

    private void ChangeStatus(CirculationStatus newStatus, BookCondition? newCond, string defaultReason)
    {
        var copy = GetSelectedCopy();
        if (copy == null) return;

        if (newStatus == CirculationStatus.Disposed && copy.CirculationStatus == CirculationStatus.Borrowed)
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

    private void ShowCopyDetail()
    {
        var c = GetSelectedCopy();
        if (c == null) return;

        string info =
            $"Mã bản sách: {c.CopyCode}\n" +
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

        foreach (DataGridViewRow row in _dgvBookCopies.Rows)
        {
            if (row.Tag is not BookCopies c) continue;
            string[] cells =
            [
                c.CopyCode,
                c.Book?.BookCode ?? "",
                c.Book?.Title ?? "",
                c.ShelfLocation ?? "",
                TranslateCondition(c.PhysicalCondition) ?? "",
                TranslateStatus(c.CirculationStatus) ?? "",
                c.ReceivedAt,
                c.PurchasePriceVnd.ToString("N0")
            ];
            sb.AppendLine(string.Join(",", cells.Select(EscapeCsv)));
        }

        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show("Xuất file CSV thành công!", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string EscapeCsv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

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
        CirculationStatus.Disposed    => "Thanh lý",
        _                             => st.ToString()
    };

    // ============================================================
    //  HELPER UI
    // ============================================================
    private static Button CreateActionButton(string text, string glyph, Color back, Color fore)
    {
        var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        using (var f = new Font(IconFont, 13F, GraphicsUnit.Pixel))
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            TextRenderer.DrawText(
                g,
                glyph,
                f,
                new Rectangle(0, 0, 16, 16),
                fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );
        }

        var btn = new Button
        {
            Text = " " + text,
            FlatStyle = FlatStyle.Flat,
            BackColor = back,
            ForeColor = fore,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, 34),
            MaximumSize = new Size(0, 34),
            Image = bmp,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(8, 0, 10, 0),
            Margin = new Padding(6, 0, 0, 0),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            UseCompatibleTextRendering = false,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = BorderGray;
        return btn;
    }

    private static Button CreateCompactBtn(string text, string glyph, Color back, Color fore)
    {
        var bmp = new Bitmap(15, 15);
        using (var g = Graphics.FromImage(bmp))
        using (var f = new Font(IconFont, 12F, GraphicsUnit.Pixel))
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            TextRenderer.DrawText(
                g,
                glyph,
                f,
                new Rectangle(0, 0, 15, 15),
                fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );
        }

        var btn = new Button
        {
            Text = " " + text,
            FlatStyle = FlatStyle.Flat,
            BackColor = back,
            ForeColor = fore,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, 32),
            MaximumSize = new Size(0, 32),
            Image = bmp,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(7, 0, 9, 0),
            Margin = new Padding(0, 0, 6, 0),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            UseCompatibleTextRendering = false,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = BorderGray;
        return btn;
    }

    private static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }
}

// ================================================================
//  DIALOG THÊM / SỬA BẢN SÁCH
// ================================================================
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
            PhysicalCondition = BookCondition.New,
            CirculationStatus = CirculationStatus.Available
        };

        Text = copy == null ? "Thêm mới bản sách vật lý" : $"Cập nhật bản sách - {copy.CopyCode}";
        Size = new Size(520, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        BuildUi();
        BindData();
    }

    private void BuildUi()
    {
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(232, 240, 250) };
        pnlHeader.Controls.Add(new Label
        {
            Text = Text.ToUpper(),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(19, 98, 160),
            AutoSize = true,
            Location = new Point(16, 13)
        });

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(16, 12, 16, 8)
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _cboBook     = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _txtCopyCode = new TextBox  { Dock = DockStyle.Fill };
        _txtShelf    = new TextBox  { Dock = DockStyle.Fill, PlaceholderText = "VD: Kệ A2 - Tầng 3" };
        _dtpReceived = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        _numPrice    = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 100_000_000, Increment = 5000 };
        _txtSource   = new TextBox  { Dock = DockStyle.Fill, PlaceholderText = "VD: NXB Trẻ / Mua mới / Biếu tặng" };
        _txtNote     = new TextBox  { Dock = DockStyle.Fill };

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
            CirculationStatus.Disposed
        });

        AddRow(tbl, 0, "Đầu sách (*):", _cboBook);
        AddRow(tbl, 1, "Mã bản sách:", _txtCopyCode);
        AddRow(tbl, 2, "Vị trí kệ sách:", _txtShelf);
        AddRow(tbl, 3, "Ngày nhập sách:", _dtpReceived);
        AddRow(tbl, 4, "Giá nhập (VNĐ):", _numPrice);
        AddRow(tbl, 5, "Nguồn nhập:", _txtSource);
        AddRow(tbl, 6, "Tình trạng vật lý:", _cboCondition);
        AddRow(tbl, 7, "Trạng thái lưu thông:", _cboStatus);
        AddRow(tbl, 8, "Ghi chú:", _txtNote);

        var pnlBottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            BackColor = Color.FromArgb(245, 247, 250),
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16, 7, 16, 7)
        };
        var btnCancel = new Button { Text = "Hủy", Width = 85, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
        var btnSave = new Button
        {
            Text = "Lưu bản sách",
            Width = 120,
            Height = 32,
            BackColor = Color.FromArgb(19, 98, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        btnSave.Click += BtnSave_Click;

        pnlBottom.Controls.Add(btnCancel);
        pnlBottom.Controls.Add(btnSave);

        Controls.Add(tbl);
        Controls.Add(pnlBottom);
        Controls.Add(pnlHeader);
    }

    private static void AddRow(TableLayoutPanel tbl, int row, string label, Control c)
    {
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.Controls.Add(new Label
        {
            Text = label,
            ForeColor = Color.FromArgb(80, 90, 105),
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
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

        _txtShelf.Text = _copy.ShelfLocation;
        if (DateTime.TryParse(_copy.ReceivedAt, out var dt)) _dtpReceived.Value = dt;
        _numPrice.Value = _copy.PurchasePriceVnd;
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
            _copy.CopyCode = _txtCopyCode.Text.Trim();
            _copy.ShelfLocation = _txtShelf.Text;
            _copy.ReceivedAt = _dtpReceived.Value.ToString("yyyy-MM-dd");
            _copy.PurchasePriceVnd = (int)_numPrice.Value;
            _copy.Source = _txtSource.Text;
            _copy.PhysicalCondition = _cboCondition.SelectedItem is BookCondition cond ? cond : BookCondition.Good;
            _copy.CirculationStatus = _cboStatus.SelectedItem is CirculationStatus st ? st : CirculationStatus.Available;
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