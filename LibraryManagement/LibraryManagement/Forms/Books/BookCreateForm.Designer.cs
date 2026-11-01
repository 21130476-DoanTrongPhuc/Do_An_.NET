using System.ComponentModel;

namespace LibraryManagement.Forms.Books;

partial class BookCreateForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        label1 = new System.Windows.Forms.Label();
        lblISBN = new System.Windows.Forms.Label();
        lblTitle = new System.Windows.Forms.Label();
        lblAuthor = new System.Windows.Forms.Label();
        lblQuantity = new System.Windows.Forms.Label();
        txtISBN = new System.Windows.Forms.TextBox();
        txtTitle = new System.Windows.Forms.TextBox();
        txtAuthor = new System.Windows.Forms.TextBox();
        nudQuantity = new System.Windows.Forms.NumericUpDown();
        btnSave = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)nudQuantity).BeginInit();
        SuspendLayout();
        // 
        // label1
        // 
        label1.Location = new System.Drawing.Point(12, 9);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(173, 31);
        label1.TabIndex = 0;
        label1.Text = "label1";
        // 
        // lblISBN
        // 
        lblISBN.Location = new System.Drawing.Point(42, 63);
        lblISBN.Name = "lblISBN";
        lblISBN.Size = new System.Drawing.Size(209, 37);
        lblISBN.TabIndex = 1;
        lblISBN.Text = "ISBN";
        // 
        // lblTitle
        // 
        lblTitle.Location = new System.Drawing.Point(42, 138);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(209, 37);
        lblTitle.TabIndex = 2;
        lblTitle.Text = "Tên sách";
        // 
        // lblAuthor
        // 
        lblAuthor.Location = new System.Drawing.Point(42, 215);
        lblAuthor.Name = "lblAuthor";
        lblAuthor.Size = new System.Drawing.Size(209, 37);
        lblAuthor.TabIndex = 3;
        lblAuthor.Text = "Tác giả";
        // 
        // lblQuantity
        // 
        lblQuantity.Location = new System.Drawing.Point(42, 295);
        lblQuantity.Name = "lblQuantity";
        lblQuantity.Size = new System.Drawing.Size(209, 37);
        lblQuantity.TabIndex = 4;
        lblQuantity.Text = "Số lượng";
        // 
        // txtISBN
        // 
        txtISBN.Location = new System.Drawing.Point(42, 86);
        txtISBN.Name = "txtISBN";
        txtISBN.Size = new System.Drawing.Size(253, 27);
        txtISBN.TabIndex = 5;
        // 
        // txtTitle
        // 
        txtTitle.Location = new System.Drawing.Point(42, 161);
        txtTitle.Name = "txtTitle";
        txtTitle.Size = new System.Drawing.Size(253, 27);
        txtTitle.TabIndex = 6;
        // 
        // txtAuthor
        // 
        txtAuthor.Location = new System.Drawing.Point(42, 237);
        txtAuthor.Name = "txtAuthor";
        txtAuthor.Size = new System.Drawing.Size(253, 27);
        txtAuthor.TabIndex = 7;
        // 
        // nudQuantity
        // 
        nudQuantity.Location = new System.Drawing.Point(44, 319);
        nudQuantity.Name = "nudQuantity";
        nudQuantity.Size = new System.Drawing.Size(251, 27);
        nudQuantity.TabIndex = 8;
        nudQuantity.Minimum = 1;
        nudQuantity.Value = 1;
        // 
        // btnSave
        // 
        btnSave.Location = new System.Drawing.Point(44, 370);
        btnSave.Name = "btnSave";
        btnSave.Size = new System.Drawing.Size(258, 36);
        btnSave.TabIndex = 9;
        btnSave.Text = "Thêm sách";
        btnSave.UseVisualStyleBackColor = true;
        btnSave.Click += btnSave_Click;
        // 
        // BookCreateForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 450);
        Controls.Add(btnSave);
        Controls.Add(nudQuantity);
        Controls.Add(txtAuthor);
        Controls.Add(txtTitle);
        Controls.Add(txtISBN);
        Controls.Add(lblQuantity);
        Controls.Add(lblAuthor);
        Controls.Add(lblTitle);
        Controls.Add(lblISBN);
        Controls.Add(label1);
        Text = "BookCreateForm";
        ((System.ComponentModel.ISupportInitialize)nudQuantity).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Button btnSave;

    private System.Windows.Forms.NumericUpDown nudQuantity;

    private System.Windows.Forms.TextBox txtISBN;
    private System.Windows.Forms.TextBox txtTitle;
    private System.Windows.Forms.TextBox txtAuthor;

    private System.Windows.Forms.Label lblAuthor;
    private System.Windows.Forms.Label lblQuantity;

    private System.Windows.Forms.Label lblISBN;
    private System.Windows.Forms.Label lblTitle;

    private System.Windows.Forms.Label label1;

    #endregion
}