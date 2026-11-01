using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Forms.Books;

public partial class BookCreateForm : Form
{
    private readonly IBookService _bookService;

    public BookCreateForm(IBookService bookService)
    {
        InitializeComponent();
        _bookService = bookService;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            _bookService.AddBook(
                txtISBN.Text,
                txtTitle.Text,
                txtAuthor.Text,
                (int)nudQuantity.Value
            );

            MessageBox.Show(
                "Thêm sách thành công!",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }

    private void ClearForm()
    {
        txtISBN.Clear();
        txtTitle.Clear();
        txtAuthor.Clear();
        nudQuantity.Value = 1;

        txtISBN.Focus();
    }
}