
-- =====================================================
-- LIBRARY MANAGEMENT SYSTEM
-- Migration: 001_CreateCoreTables.sql
-- Database: SQLite
-- =====================================================

-- 1. BOOK CATEGORIES
CREATE TABLE IF NOT EXISTS book_categories (
                                               id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                               id_parent           INTEGER NULL,
                                               category_code       TEXT NOT NULL UNIQUE,
                                               category_name       TEXT NOT NULL,
                                               max_loan_days       INTEGER NOT NULL DEFAULT 14,
                                               max_renewals        INTEGER NOT NULL DEFAULT 1,
                                               status              TEXT NOT NULL DEFAULT 'Active',
                                               created_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                               updated_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                               FOREIGN KEY (id_parent)
    REFERENCES book_categories(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    CHECK (id_parent IS NULL OR id_parent <> id),
    CHECK (max_loan_days > 0),
    CHECK (max_renewals >= 0),
    CHECK (status IN ('Active', 'Inactive'))
    );


-- 2. BOOKS
-- Mỗi đầu sách chỉ thuộc một thể loại và có một tác giả.
CREATE TABLE IF NOT EXISTS books (
                                     id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                     id_category         INTEGER NOT NULL,
                                     book_code           TEXT NOT NULL UNIQUE,
                                     isbn                TEXT UNIQUE,
                                     title               TEXT NOT NULL,
                                     author              TEXT NOT NULL,
                                     publisher           TEXT,
                                     published_year      INTEGER,
                                     description         TEXT,
                                     created_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                     updated_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                     FOREIGN KEY (id_category)
    REFERENCES book_categories(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    CHECK (
              published_year IS NULL
              OR published_year BETWEEN 1000 AND 9999
          )
    );


-- 3. BOOK COPIES
-- Mỗi bản sách vật lý là một bản ghi riêng.
CREATE TABLE IF NOT EXISTS book_copies (
                                           id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                           id_book             INTEGER NOT NULL,
                                           copy_code           TEXT NOT NULL UNIQUE,
                                           shelf_location      TEXT,
                                           received_at         TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                           purchase_price_vnd  INTEGER NOT NULL DEFAULT 0,
                                           physical_condition  TEXT NOT NULL DEFAULT 'Good',
                                           circulation_status  TEXT NOT NULL DEFAULT 'Available',
                                           note                TEXT,
                                           created_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                           updated_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                           FOREIGN KEY (id_book)
    REFERENCES books(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    CHECK (purchase_price_vnd >= 0),

    CHECK (
              physical_condition IN (
              'New', 'Good', 'Worn', 'Damaged'
                                    )
    ),

    CHECK (
              circulation_status IN (
              'Available',
              'Borrowed',
              'Reserved',
              'Maintenance',
              'Lost'
                                    )
    )
    );



-- 5. READERS
CREATE TABLE IF NOT EXISTS readers (
                                       id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                       max_active_copies   INTEGER NOT NULL DEFAULT 3,
                                       reader_code         TEXT NOT NULL UNIQUE,
                                       full_name           TEXT NOT NULL,
                                       date_of_birth       TEXT,
                                       phone               TEXT,
                                       email               TEXT,
                                       address             TEXT,
                                       created_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       updated_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                       CHECK (max_active_copies > 0)
    );


-- 6. READER CARDS
-- Một độc giả có thể có nhiều thẻ trong lịch sử,
-- nhưng chỉ được có tối đa một thẻ Active.
CREATE TABLE IF NOT EXISTS reader_cards (
                                            id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                            id_reader           INTEGER NOT NULL,
                                            card_code           TEXT NOT NULL UNIQUE,
                                            issued_at           TEXT NOT NULL,
                                            expires_at          TEXT NOT NULL,
                                            status              TEXT NOT NULL DEFAULT 'Active',
                                            note                TEXT,

                                            FOREIGN KEY (id_reader)
    REFERENCES readers(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    CHECK (expires_at >= issued_at),
    CHECK (status IN ('Active', 'Expired', 'Locked'))
    );

CREATE UNIQUE INDEX IF NOT EXISTS ux_reader_cards_one_active
    ON reader_cards(id_reader)
    WHERE status = 'Active';


-- 7. LIBRARIANS
CREATE TABLE IF NOT EXISTS librarians (
                                          id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                          employee_code       TEXT NOT NULL UNIQUE,
                                          username            TEXT NOT NULL UNIQUE,
                                          password_hash       TEXT NOT NULL,
                                          full_name           TEXT NOT NULL,
                                          phone               TEXT,
                                          email               TEXT,
                                          address             TEXT,
                                          role                TEXT NOT NULL DEFAULT 'Librarian',
                                          status              TEXT NOT NULL DEFAULT 'Active',
                                          created_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                          updated_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                          CHECK (role IN ('Admin', 'Librarian')),
    CHECK (status IN ('Active', 'Inactive'))
    );


-- 8. LOANS
-- Một phiếu mượn có thể chứa nhiều bản sách.
CREATE TABLE IF NOT EXISTS loans (
                                     id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                     loan_code           TEXT NOT NULL UNIQUE,
                                     id_reader           INTEGER NOT NULL,
                                     id_librarian        INTEGER NOT NULL,
                                     borrowed_at         TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                     note                TEXT,

                                     FOREIGN KEY (id_reader)
    REFERENCES readers(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_librarian)
    REFERENCES librarians(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT
    );


-- 9. LOAN DETAILS
-- Mỗi dòng tương ứng với một bản sách vật lý được mượn.
CREATE TABLE IF NOT EXISTS loan_details (
                                            id                          INTEGER PRIMARY KEY AUTOINCREMENT,
                                            id_loan                     INTEGER NOT NULL,
                                            id_book_copy                INTEGER NOT NULL,
                                            due_at                      TEXT NOT NULL,
                                            returned_at                 TEXT,
                                            physical_condition_before   TEXT NOT NULL,
                                            physical_condition_after    TEXT,
                                            id_librarian_return         INTEGER,
                                            renewal_count               INTEGER NOT NULL DEFAULT 0,
                                            note                        TEXT,

                                            FOREIGN KEY (id_loan)
    REFERENCES loans(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_book_copy)
    REFERENCES book_copies(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_librarian_return)
    REFERENCES librarians(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    CHECK (renewal_count >= 0),

    CHECK (
              physical_condition_before IN (
              'New', 'Good', 'Worn', 'Damaged'
                                           )
    ),

    CHECK (
              physical_condition_after IS NULL
              OR physical_condition_after IN (
              'New', 'Good', 'Worn', 'Damaged'
                                             )
    ),

    CHECK (
              returned_at IS NULL
              OR returned_at >= due_at
              OR returned_at < due_at
          )
    );

-- Một bản sách không thể có hai lượt mượn chưa trả.
CREATE UNIQUE INDEX IF NOT EXISTS ux_loan_details_active_copy
    ON loan_details(id_book_copy)
    WHERE returned_at IS NULL;


-- 10. RESERVATIONS
CREATE TABLE IF NOT EXISTS reservations (
                                            id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                                            reservation_code        TEXT NOT NULL UNIQUE,
                                            id_reader               INTEGER NOT NULL,
                                            id_book                 INTEGER NOT NULL,
                                            id_book_copy            INTEGER,
                                            id_librarian            INTEGER,
                                            id_loan                 INTEGER UNIQUE,
                                            reserved_at             TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                            expires_at              TEXT,
                                            assigned_at             TEXT,
                                            fulfilled_at            TEXT,
                                            cancelled_at            TEXT,
                                            status                  TEXT NOT NULL DEFAULT 'Pending',
                                            note                    TEXT,

                                            FOREIGN KEY (id_reader)
    REFERENCES readers(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_book)
    REFERENCES books(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_book_copy)
    REFERENCES book_copies(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_librarian)
    REFERENCES librarians(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_loan)
    REFERENCES loans(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    CHECK (
              status IN (
              'Pending',
              'ReadyForPickup',
              'Completed',
              'Cancelled',
              'Expired'
                        )
    )
    );

-- Một độc giả không thể đặt trùng cùng đầu sách
-- khi đã có yêu cầu đang chờ hoặc sẵn sàng nhận.
CREATE UNIQUE INDEX IF NOT EXISTS ux_reservations_active_reader_book
    ON reservations(id_reader, id_book)
    WHERE status IN ('Pending', 'ReadyForPickup');

-- Một bản sách chỉ được gán cho tối đa một yêu cầu
-- đang ở trạng thái ReadyForPickup.
CREATE UNIQUE INDEX IF NOT EXISTS ux_reservations_assigned_copy
    ON reservations(id_book_copy)
    WHERE id_book_copy IS NOT NULL
    AND status = 'ReadyForPickup';


-- 11. FINES
-- Một lượt mượn có thể phát sinh nhiều khoản phạt.
CREATE TABLE IF NOT EXISTS fines (
                                     id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                     fine_code           TEXT NOT NULL UNIQUE,
                                     id_loan_detail      INTEGER NOT NULL,
                                     id_librarian        INTEGER NOT NULL,
                                     created_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                     violation_type      TEXT NOT NULL,
                                     amount_vnd          INTEGER NOT NULL,
                                     description         TEXT,

                                     FOREIGN KEY (id_loan_detail)
    REFERENCES loan_details(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_librarian)
    REFERENCES librarians(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    CHECK (amount_vnd > 0)
    );


-- 12. FINE PAYMENTS
-- Mỗi lần thanh toán là một bản ghi riêng.
CREATE TABLE IF NOT EXISTS fine_payments (
                                             id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                             payment_code        TEXT NOT NULL UNIQUE,
                                             id_fine             INTEGER NOT NULL,
                                             id_librarian        INTEGER NOT NULL,
                                             paid_at             TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                             amount_vnd          INTEGER NOT NULL,
                                             payment_method      TEXT NOT NULL DEFAULT 'Cash',
                                             note                TEXT,

                                             FOREIGN KEY (id_fine)
    REFERENCES fines(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    FOREIGN KEY (id_librarian)
    REFERENCES librarians(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT,

    CHECK (amount_vnd > 0),
    CHECK (payment_method IN ('Cash', 'Other'))
    );


-- =====================================================
-- INDEXES FOR COMMON QUERIES
-- =====================================================

CREATE INDEX IF NOT EXISTS ix_books_category
    ON books(id_category);

CREATE INDEX IF NOT EXISTS ix_books_title
    ON books(title);

CREATE INDEX IF NOT EXISTS ix_book_copies_book
    ON book_copies(id_book);

CREATE INDEX IF NOT EXISTS ix_book_copies_status
    ON book_copies(circulation_status);

CREATE INDEX IF NOT EXISTS ix_readers_name
    ON readers(full_name);

CREATE INDEX IF NOT EXISTS ix_loans_reader
    ON loans(id_reader);

CREATE INDEX IF NOT EXISTS ix_loans_borrowed_at
    ON loans(borrowed_at);

CREATE INDEX IF NOT EXISTS ix_loan_details_loan
    ON loan_details(id_loan);

CREATE INDEX IF NOT EXISTS ix_loan_details_due_at
    ON loan_details(due_at);

CREATE INDEX IF NOT EXISTS ix_reservations_book_status
    ON reservations(id_book, status);

CREATE INDEX IF NOT EXISTS ix_fines_loan_detail
    ON fines(id_loan_detail);

CREATE INDEX IF NOT EXISTS ix_fine_payments_fine
    ON fine_payments(id_fine);


-- -- =====================================================
-- -- TRIGGERS: PREVENT OVERPAYMENT
-- -- =====================================================

CREATE TRIGGER IF NOT EXISTS trg_fine_payment_before_insert
BEFORE INSERT ON fine_payments
FOR EACH ROW
WHEN (
    COALESCE(
        (SELECT SUM(amount_vnd)
         FROM fine_payments
         WHERE id_fine = NEW.id_fine),
        0
    ) + NEW.amount_vnd
) > (
    SELECT amount_vnd
    FROM fines
    WHERE id = NEW.id_fine
)
BEGIN
SELECT RAISE(ABORT, 'Payment exceeds fine amount');
END;


CREATE TRIGGER IF NOT EXISTS trg_fine_payment_before_update
BEFORE UPDATE OF id_fine, amount_vnd ON fine_payments
    FOR EACH ROW
    WHEN (
    COALESCE(
    (SELECT SUM(amount_vnd)
    FROM fine_payments
    WHERE id_fine = NEW.id_fine
    AND id <> OLD.id),
    0
    ) + NEW.amount_vnd
    ) > (
    SELECT amount_vnd
    FROM fines
    WHERE id = NEW.id_fine
    )
BEGIN
SELECT RAISE(ABORT, 'Payment exceeds fine amount');
END;


-- Không cho giảm tiền phạt xuống thấp hơn tổng tiền đã thu.
CREATE TRIGGER IF NOT EXISTS trg_fine_amount_before_update
BEFORE UPDATE OF amount_vnd ON fines
    FOR EACH ROW
    WHEN NEW.amount_vnd < COALESCE(
    (SELECT SUM(amount_vnd)
    FROM fine_payments
    WHERE id_fine = OLD.id),
    0
    )
BEGIN
SELECT RAISE(ABORT, 'Fine amount is lower than payments received');
END;
