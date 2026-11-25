-- =============================================================
-- LIBRARY MANAGEMENT SYSTEM
-- File: 001_CreateTables.sql
-- Database: SQLite
-- Run order: 001 -> 002 -> 003 -> 999
-- =============================================================

PRAGMA foreign_keys = ON;

-- =============================================================
-- 1. USERS
-- Reader, Librarian
-- =============================================================
CREATE TABLE IF NOT EXISTS "Users" (
                                       "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "UserCode"        TEXT NOT NULL UNIQUE,
                                       "Username"        TEXT UNIQUE,
                                       "PasswordHash"    TEXT,
                                       "FullName"        TEXT NOT NULL,
                                       "DateOfBirth"     TEXT,
                                       "Phone"           TEXT,
                                       "Email"           TEXT,
                                       "Address"         TEXT,
                                       "UserType"        TEXT NOT NULL,
                                       "MaxActiveCopies" INTEGER,
                                       "Status"          TEXT NOT NULL DEFAULT 'Active',
                                       "CreatedAt"       TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "UpdatedAt"       TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                       CHECK ("UserType" IN ('Reader', 'Librarian')),
                                       CHECK ("Status" IN ('Active', 'Inactive')),
                                       CHECK (
                                           ("UserType" = 'Reader'
                                               AND "MaxActiveCopies" IS NOT NULL
                                               AND "MaxActiveCopies" > 0)
                                               OR
                                           ("UserType" IN ('Librarian')
                                               AND "MaxActiveCopies" IS NULL)
                                           ),
                                       CHECK (
                                           ("Username" IS NULL AND "PasswordHash" IS NULL)
                                               OR
                                           ("Username" IS NOT NULL AND "PasswordHash" IS NOT NULL)
                                           )
);

-- Chỉ cho phép tối đa một Librarian đang hoạt động.
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Users_SingleActiveLibrarian"
    ON "Users"("UserType")
    WHERE "UserType" = 'Librarian' AND "Status" = 'Active';

CREATE INDEX IF NOT EXISTS "IX_Users_UserType_Status"
    ON "Users"("UserType", "Status");

-- =============================================================
-- 2. BOOK CATEGORIES
-- =============================================================
CREATE TABLE IF NOT EXISTS "BookCategories" (
                                                "Id"           INTEGER PRIMARY KEY AUTOINCREMENT,
                                                "IdParent"     INTEGER,
                                                "CategoryCode" TEXT NOT NULL UNIQUE,
                                                "CategoryName" TEXT NOT NULL,
                                                "MaxLoanDays"  INTEGER NOT NULL DEFAULT 14,
                                                "MaxRenewals"  INTEGER NOT NULL DEFAULT 1,
                                                "Status"       TEXT NOT NULL DEFAULT 'Active',
                                                "CreatedAt"    TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                                "UpdatedAt"    TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                                FOREIGN KEY ("IdParent")
                                                    REFERENCES "BookCategories"("Id")
                                                    ON UPDATE CASCADE
                                                    ON DELETE RESTRICT,

                                                CHECK ("IdParent" IS NULL OR "IdParent" <> "Id"),
                                                CHECK ("MaxLoanDays" > 0),
                                                CHECK ("MaxRenewals" >= 0),
                                                CHECK ("Status" IN ('Active', 'Inactive'))
);

CREATE INDEX IF NOT EXISTS "IX_BookCategories_IdParent"
    ON "BookCategories"("IdParent");

-- Ngăn tạo chu trình phân cấp danh mục (A -> B -> C -> A).
CREATE TRIGGER IF NOT EXISTS "TR_BookCategories_PreventCycle_Insert"
    BEFORE INSERT ON "BookCategories"
    WHEN NEW."IdParent" IS NOT NULL
BEGIN
    SELECT CASE
               WHEN EXISTS (
                   WITH RECURSIVE ancestors("Id") AS (
                       SELECT NEW."IdParent"
                       UNION ALL
                       SELECT bc."IdParent"
                       FROM "BookCategories" bc
                                JOIN ancestors a ON bc."Id" = a."Id"
                       WHERE bc."IdParent" IS NOT NULL
                   )
                   SELECT 1 FROM ancestors WHERE "Id" = NEW."Id"
               )
                   THEN RAISE(ABORT, 'Category hierarchy cannot contain a cycle')
               END;
END;

CREATE TRIGGER IF NOT EXISTS "TR_BookCategories_PreventCycle_Update"
    BEFORE UPDATE OF "IdParent" ON "BookCategories"
    WHEN NEW."IdParent" IS NOT NULL
BEGIN
    SELECT CASE
               WHEN NEW."IdParent" = NEW."Id"
                   OR EXISTS (
                       WITH RECURSIVE descendants("Id") AS (
                           SELECT NEW."IdParent"
                           UNION ALL
                           SELECT bc."IdParent"
                           FROM "BookCategories" bc
                                    JOIN descendants d ON bc."Id" = d."Id"
                           WHERE bc."IdParent" IS NOT NULL
                       )
                       SELECT 1 FROM descendants WHERE "Id" = NEW."Id"
                   )
                   THEN RAISE(ABORT, 'Category hierarchy cannot contain a cycle')
               END;
END;

-- =============================================================
-- 3. BOOKS
-- =============================================================
CREATE TABLE IF NOT EXISTS "Books" (
                                       "Id"             INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "IdCategory"     INTEGER NOT NULL,
                                       "BookCode"       TEXT NOT NULL UNIQUE,
                                       "Isbn"           TEXT UNIQUE,
                                       "Title"          TEXT NOT NULL,
                                       "Author"         TEXT NOT NULL,
                                       "Publisher"      TEXT,
                                       "PublishedYear"  INTEGER,
                                       "Language"       TEXT,
                                       "CoverImagePath" TEXT,
                                       "Status"         TEXT NOT NULL DEFAULT 'Active',
                                       "Description"    TEXT,
                                       "CreatedAt"      TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "UpdatedAt"      TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                       FOREIGN KEY ("IdCategory")
                                           REFERENCES "BookCategories"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT,

                                       CHECK ("Status" IN ('Active', 'Inactive')),
                                       CHECK ("PublishedYear" IS NULL OR "PublishedYear" BETWEEN 1000 AND 9999)
);

CREATE INDEX IF NOT EXISTS "IX_Books_IdCategory"
    ON "Books"("IdCategory");

CREATE INDEX IF NOT EXISTS "IX_Books_Title"
    ON "Books"("Title");

-- =============================================================
-- 4. BOOK COPIES
-- =============================================================
CREATE TABLE IF NOT EXISTS "BookCopies" (
                                            "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                            "IdBook"            INTEGER NOT NULL,
                                            "CopyCode"          TEXT NOT NULL UNIQUE,
                                            "ShelfLocation"     TEXT,
                                            "ReceivedAt"        TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                            "PurchasePriceVnd"  INTEGER NOT NULL DEFAULT 0,
                                            "PhysicalCondition" TEXT NOT NULL DEFAULT 'Good',
                                            "CirculationStatus" TEXT NOT NULL DEFAULT 'Available',
                                            "Note"              TEXT,
                                            "Source"            TEXT,
                                            "CreatedAt"         TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                            "UpdatedAt"         TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                            FOREIGN KEY ("IdBook")
                                                REFERENCES "Books"("Id")
                                                ON UPDATE CASCADE
                                                ON DELETE RESTRICT,

    -- Dùng cho composite FK ở Reservations để bảo đảm bản sao thuộc đúng đầu sách.
                                            UNIQUE ("Id", "IdBook"),

                                            CHECK ("PurchasePriceVnd" >= 0),
                                            CHECK ("PhysicalCondition" IN ('New', 'Good', 'Worn', 'Damaged', 'Lost')),
                                            CHECK ("CirculationStatus" IN ('Available', 'Borrowed', 'Reserved', 'Disposed', 'Maintenance'))
);

CREATE INDEX IF NOT EXISTS "IX_BookCopies_IdBook"
    ON "BookCopies"("IdBook");

CREATE INDEX IF NOT EXISTS "IX_BookCopies_Status"
    ON "BookCopies"("CirculationStatus");

-- =============================================================
-- 5. READER CARDS
-- =============================================================
CREATE TABLE IF NOT EXISTS "ReaderCards" (
                                             "Id"        INTEGER PRIMARY KEY AUTOINCREMENT,
                                             "IdReader"  INTEGER NOT NULL,
                                             "CardCode"  TEXT NOT NULL UNIQUE,
                                             "IssuedAt"  TEXT NOT NULL,
                                             "ExpiresAt" TEXT NOT NULL,
                                             "Status"    TEXT NOT NULL DEFAULT 'Active',
                                             "Note"      TEXT,

                                             FOREIGN KEY ("IdReader")
                                                 REFERENCES "Users"("Id")
                                                 ON UPDATE CASCADE
                                                 ON DELETE RESTRICT,

                                             CHECK ("ExpiresAt" >= "IssuedAt"),
                                             CHECK ("Status" IN ('Active', 'Expired', 'Locked', 'Missing'))
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_ReaderCards_OneActive"
    ON "ReaderCards"("IdReader")
    WHERE "Status" = 'Active';

CREATE INDEX IF NOT EXISTS "IX_ReaderCards_IdReader"
    ON "ReaderCards"("IdReader");

-- Chỉ Reader mới được cấp thẻ.
CREATE TRIGGER IF NOT EXISTS "TR_ReaderCards_OnlyReader_Insert"
    BEFORE INSERT ON "ReaderCards"
BEGIN
    SELECT CASE
               WHEN NOT EXISTS (
                   SELECT 1 FROM "Users"
                   WHERE "Id" = NEW."IdReader" AND "UserType" = 'Reader'
               )
                   THEN RAISE(ABORT, 'Reader card can only be issued to a Reader')
               END;
END;

CREATE TRIGGER IF NOT EXISTS "TR_ReaderCards_OnlyReader_Update"
    BEFORE UPDATE OF "IdReader" ON "ReaderCards"
BEGIN
    SELECT CASE
               WHEN NOT EXISTS (
                   SELECT 1 FROM "Users"
                   WHERE "Id" = NEW."IdReader" AND "UserType" = 'Reader'
               )
                   THEN RAISE(ABORT, 'Reader card can only be issued to a Reader')
               END;
END;

-- =============================================================
-- 6. LOANS
-- =============================================================
CREATE TABLE IF NOT EXISTS "Loans" (
                                       "Id"         INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "LoanCode"   TEXT NOT NULL UNIQUE,
                                       "IdReader"   INTEGER NOT NULL,
                                       "BorrowedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "Note"       TEXT,

                                       FOREIGN KEY ("IdReader")
                                           REFERENCES "Users"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_Loans_IdReader"
    ON "Loans"("IdReader");

CREATE INDEX IF NOT EXISTS "IX_Loans_BorrowedAt"
    ON "Loans"("BorrowedAt");

-- Chỉ Reader mới được tạo phiếu mượn.
CREATE TRIGGER IF NOT EXISTS "TR_Loans_OnlyReader_Insert"
    BEFORE INSERT ON "Loans"
BEGIN
    SELECT CASE
               WHEN NOT EXISTS (
                   SELECT 1 FROM "Users"
                   WHERE "Id" = NEW."IdReader" AND "UserType" = 'Reader'
               )
                   THEN RAISE(ABORT, 'Only Reader users can borrow books')
               END;
END;

CREATE TRIGGER IF NOT EXISTS "TR_Loans_OnlyReader_Update"
    BEFORE UPDATE OF "IdReader" ON "Loans"
BEGIN
    SELECT CASE
               WHEN NOT EXISTS (
                   SELECT 1 FROM "Users"
                   WHERE "Id" = NEW."IdReader" AND "UserType" = 'Reader'
               )
                   THEN RAISE(ABORT, 'Only Reader users can borrow books')
               END;
END;

-- =============================================================
-- 7. LOAN DETAILS
-- =============================================================
CREATE TABLE IF NOT EXISTS "LoanDetails" (
                                             "Id"                      INTEGER PRIMARY KEY AUTOINCREMENT,
                                             "IdLoan"                  INTEGER NOT NULL,
                                             "IdBookCopy"              INTEGER NOT NULL,
                                             "DueAt"                   TEXT NOT NULL,
                                             "ReturnedAt"              TEXT,
                                             "PhysicalConditionBefore" TEXT NOT NULL,
                                             "PhysicalConditionAfter"  TEXT,
                                             "RenewalCount"            INTEGER NOT NULL DEFAULT 0,
                                             "Note"                    TEXT,

                                             FOREIGN KEY ("IdLoan")
                                                 REFERENCES "Loans"("Id")
                                                 ON UPDATE CASCADE
                                                 ON DELETE RESTRICT,

                                             FOREIGN KEY ("IdBookCopy")
                                                 REFERENCES "BookCopies"("Id")
                                                 ON UPDATE CASCADE
                                                 ON DELETE RESTRICT,

                                             CHECK ("RenewalCount" >= 0),
                                             CHECK ("PhysicalConditionBefore" IN ('New', 'Good', 'Worn', 'Damaged', 'Lost')),
                                             CHECK (
                                                 "PhysicalConditionAfter" IS NULL
                                                     OR "PhysicalConditionAfter" IN ('New', 'Good', 'Worn', 'Damaged', 'Lost')
                                                 ),
                                             CHECK ("ReturnedAt" IS NULL OR "ReturnedAt" >= '')
);

-- Một bản sách không thể nằm trong hai lượt mượn chưa trả.
CREATE UNIQUE INDEX IF NOT EXISTS "UX_LoanDetails_ActiveCopy"
    ON "LoanDetails"("IdBookCopy")
    WHERE "ReturnedAt" IS NULL;

CREATE INDEX IF NOT EXISTS "IX_LoanDetails_IdLoan"
    ON "LoanDetails"("IdLoan");

CREATE INDEX IF NOT EXISTS "IX_LoanDetails_DueAt"
    ON "LoanDetails"("DueAt");

-- =============================================================
-- 8. RESERVATIONS
-- =============================================================
CREATE TABLE IF NOT EXISTS "Reservations" (
                                              "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
                                              "ReservationCode" TEXT NOT NULL UNIQUE,
                                              "IdReader"        INTEGER NOT NULL,
                                              "IdBook"          INTEGER NOT NULL,
                                              "IdBookCopy"      INTEGER,
                                              "IdLoan"          INTEGER UNIQUE,
                                              "ReservedAt"      TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                              "ExpiresAt"       TEXT,
                                              "AssignedAt"      TEXT,
                                              "FulfilledAt"     TEXT,
                                              "CancelledAt"     TEXT,
                                              "Status"          TEXT NOT NULL DEFAULT 'Pending',
                                              "Note"            TEXT,

                                              FOREIGN KEY ("IdReader")
                                                  REFERENCES "Users"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              FOREIGN KEY ("IdBook")
                                                  REFERENCES "Books"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              FOREIGN KEY ("IdBookCopy", "IdBook")
                                                  REFERENCES "BookCopies"("Id", "IdBook")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              FOREIGN KEY ("IdLoan")
                                                  REFERENCES "Loans"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              CHECK ("Status" IN ('Pending', 'ReadyForPickup', 'Completed', 'Cancelled', 'Expired'))
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Reservations_ActiveReaderBook"
    ON "Reservations"("IdReader", "IdBook")
    WHERE "Status" IN ('Pending', 'ReadyForPickup');

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Reservations_AssignedCopy"
    ON "Reservations"("IdBookCopy")
    WHERE "IdBookCopy" IS NOT NULL AND "Status" = 'ReadyForPickup';

CREATE INDEX IF NOT EXISTS "IX_Reservations_Status_ExpiresAt"
    ON "Reservations"("Status", "ExpiresAt");

CREATE INDEX IF NOT EXISTS "IX_Reservations_IdReader"
    ON "Reservations"("IdReader");

-- Chỉ Reader mới được đặt trước.
CREATE TRIGGER IF NOT EXISTS "TR_Reservations_OnlyReader_Insert"
    BEFORE INSERT ON "Reservations"
BEGIN
    SELECT CASE
               WHEN NOT EXISTS (
                   SELECT 1 FROM "Users"
                   WHERE "Id" = NEW."IdReader" AND "UserType" = 'Reader'
               )
                   THEN RAISE(ABORT, 'Only Reader users can create reservations')
               END;
END;

CREATE TRIGGER IF NOT EXISTS "TR_Reservations_OnlyReader_Update"
    BEFORE UPDATE OF "IdReader" ON "Reservations"
BEGIN
    SELECT CASE
               WHEN NOT EXISTS (
                   SELECT 1 FROM "Users"
                   WHERE "Id" = NEW."IdReader" AND "UserType" = 'Reader'
               )
                   THEN RAISE(ABORT, 'Only Reader users can create reservations')
               END;
END;

-- =============================================================
-- 9. FINES
-- =============================================================
CREATE TABLE IF NOT EXISTS "Fines" (
                                       "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "FineCode"      TEXT NOT NULL UNIQUE,
                                       "IdLoanDetail"  INTEGER,
                                       "CreatedAt"     TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "ViolationType" TEXT NOT NULL,
                                       "AmountVnd"     INTEGER NOT NULL,
                                       "Description"   TEXT,
                                       "Status"        TEXT NOT NULL DEFAULT 'Unpaid',
                                       "CancelledAt"   TEXT,
                                       "CancelReason"  TEXT,
                                       "UpdatedAt"     TEXT,

                                       FOREIGN KEY ("IdLoanDetail")
                                           REFERENCES "LoanDetails"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT,

                                       CHECK ("AmountVnd" > 0),
                                       CHECK ("Status" IN ('Unpaid', 'Partial', 'Paid', 'Cancelled')),
                                       CHECK ("ViolationType" IN ('Overdue', 'Damaged', 'Lost', 'Other')),
                                       CHECK (
                                           ("Status" = 'Cancelled' AND "CancelledAt" IS NOT NULL)
                                               OR "Status" <> 'Cancelled'
                                           )
);

CREATE INDEX IF NOT EXISTS "IX_Fines_IdLoanDetail" ON "Fines"("IdLoanDetail");
CREATE INDEX IF NOT EXISTS "IX_Fines_Status" ON "Fines"("Status");
CREATE INDEX IF NOT EXISTS "IX_Fines_CreatedAt" ON "Fines"("CreatedAt");

-- =============================================================
-- 10. FINE PAYMENTS
-- =============================================================
CREATE TABLE IF NOT EXISTS "FinePayments" (
                                              "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
                                              "PaymentCode"   TEXT NOT NULL UNIQUE,
                                              "IdFine"        INTEGER NOT NULL,
                                              "PaidAt"        TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                              "AmountVnd"     INTEGER NOT NULL,
                                              "PaymentMethod" TEXT NOT NULL DEFAULT 'Cash',
                                              "Note"          TEXT,
                                              "CreatedAt"     TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                              FOREIGN KEY ("IdFine")
                                                  REFERENCES "Fines"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              CHECK ("AmountVnd" > 0),
                                              CHECK ("PaymentMethod" IN ('Cash', 'Card'))
);

CREATE INDEX IF NOT EXISTS "IX_FinePayments_IdFine" ON "FinePayments"("IdFine");
CREATE INDEX IF NOT EXISTS "IX_FinePayments_PaidAt" ON "FinePayments"("PaidAt");

-- Không cho thanh toán khoản phạt đã hủy hoặc thu vượt số tiền còn phải thu.
CREATE TRIGGER IF NOT EXISTS "TR_FinePayments_Validate_Insert"
    BEFORE INSERT ON "FinePayments"
BEGIN
    SELECT CASE
               WHEN (SELECT "Status" FROM "Fines" WHERE "Id" = NEW."IdFine") = 'Cancelled'
                   THEN RAISE(ABORT, 'Cannot pay a cancelled fine')
               END;

    SELECT CASE
               WHEN NEW."AmountVnd" + COALESCE((
                                                   SELECT SUM("AmountVnd")
                                                   FROM "FinePayments"
                                                   WHERE "IdFine" = NEW."IdFine"
                                               ), 0) > (
                        SELECT "AmountVnd" FROM "Fines" WHERE "Id" = NEW."IdFine"
                    )
                   THEN RAISE(ABORT, 'Payment amount exceeds fine amount')
               END;
END;

CREATE TRIGGER IF NOT EXISTS "TR_FinePayments_Validate_Update"
    BEFORE UPDATE OF "IdFine", "AmountVnd" ON "FinePayments"
BEGIN
    SELECT CASE
               WHEN (SELECT "Status" FROM "Fines" WHERE "Id" = NEW."IdFine") = 'Cancelled'
                   THEN RAISE(ABORT, 'Cannot pay a cancelled fine')
               END;

    SELECT CASE
               WHEN NEW."AmountVnd" + COALESCE((
                                                   SELECT SUM("AmountVnd")
                                                   FROM "FinePayments"
                                                   WHERE "IdFine" = NEW."IdFine"
                                                     AND "Id" <> OLD."Id"
                                               ), 0) > (
                        SELECT "AmountVnd" FROM "Fines" WHERE "Id" = NEW."IdFine"
                    )
                   THEN RAISE(ABORT, 'Payment amount exceeds fine amount')
               END;
END;

-- Tự đồng bộ Unpaid / Partial / Paid sau khi thêm / sửa / xóa thanh toán.
CREATE TRIGGER IF NOT EXISTS "TR_FinePayments_AfterInsert"
    AFTER INSERT ON "FinePayments"
BEGIN
    UPDATE "Fines"
    SET "Status" = CASE
                       WHEN (SELECT COALESCE(SUM("AmountVnd"), 0)
                             FROM "FinePayments" WHERE "IdFine" = NEW."IdFine") = 0
                           THEN 'Unpaid'
                       WHEN (SELECT SUM("AmountVnd")
                             FROM "FinePayments" WHERE "IdFine" = NEW."IdFine") < "AmountVnd"
                           THEN 'Partial'
                       ELSE 'Paid'
        END,
        "UpdatedAt" = CURRENT_TIMESTAMP
    WHERE "Id" = NEW."IdFine" AND "Status" <> 'Cancelled';
END;

CREATE TRIGGER IF NOT EXISTS "TR_FinePayments_AfterUpdate"
    AFTER UPDATE OF "IdFine", "AmountVnd" ON "FinePayments"
BEGIN
    UPDATE "Fines"
    SET "Status" = CASE
                       WHEN COALESCE((SELECT SUM("AmountVnd") FROM "FinePayments"
                                      WHERE "IdFine" = OLD."IdFine"), 0) = 0 THEN 'Unpaid'
                       WHEN (SELECT SUM("AmountVnd") FROM "FinePayments"
                             WHERE "IdFine" = OLD."IdFine") < "AmountVnd" THEN 'Partial'
                       ELSE 'Paid'
        END,
        "UpdatedAt" = CURRENT_TIMESTAMP
    WHERE "Id" = OLD."IdFine" AND "Status" <> 'Cancelled';

    UPDATE "Fines"
    SET "Status" = CASE
                       WHEN COALESCE((SELECT SUM("AmountVnd") FROM "FinePayments"
                                      WHERE "IdFine" = NEW."IdFine"), 0) = 0 THEN 'Unpaid'
                       WHEN (SELECT SUM("AmountVnd") FROM "FinePayments"
                             WHERE "IdFine" = NEW."IdFine") < "AmountVnd" THEN 'Partial'
                       ELSE 'Paid'
        END,
        "UpdatedAt" = CURRENT_TIMESTAMP
    WHERE "Id" = NEW."IdFine" AND "Status" <> 'Cancelled';
END;

CREATE TRIGGER IF NOT EXISTS "TR_FinePayments_AfterDelete"
    AFTER DELETE ON "FinePayments"
BEGIN
    UPDATE "Fines"
    SET "Status" = CASE
                       WHEN COALESCE((SELECT SUM("AmountVnd") FROM "FinePayments"
                                      WHERE "IdFine" = OLD."IdFine"), 0) = 0 THEN 'Unpaid'
                       WHEN (SELECT SUM("AmountVnd") FROM "FinePayments"
                             WHERE "IdFine" = OLD."IdFine") < "AmountVnd" THEN 'Partial'
                       ELSE 'Paid'
        END,
        "UpdatedAt" = CURRENT_TIMESTAMP
    WHERE "Id" = OLD."IdFine" AND "Status" <> 'Cancelled';
END;

-- =============================================================
-- 11. FINE AUDIT LOGS
-- =============================================================
CREATE TABLE IF NOT EXISTS "FineAuditLogs" (
                                               "Id"           INTEGER PRIMARY KEY AUTOINCREMENT,
                                               "IdFine"       INTEGER NOT NULL,
                                               "Action"       TEXT NOT NULL,
                                               "BeforeAmount" INTEGER NOT NULL DEFAULT 0,
                                               "AfterAmount"  INTEGER NOT NULL DEFAULT 0,
                                               "Reason"       TEXT,
                                               "At"           TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                               FOREIGN KEY ("IdFine")
                                                   REFERENCES "Fines"("Id")
                                                   ON UPDATE CASCADE
                                                   ON DELETE CASCADE,

                                               CHECK ("Action" IN ('Created', 'Adjusted', 'Cancelled', 'Paid')),
                                               CHECK ("BeforeAmount" >= 0),
                                               CHECK ("AfterAmount" >= 0)
);

CREATE INDEX IF NOT EXISTS "IX_FineAuditLogs_IdFine" ON "FineAuditLogs"("IdFine");
CREATE INDEX IF NOT EXISTS "IX_FineAuditLogs_At" ON "FineAuditLogs"("At");

-- =============================================================
-- 12. REMINDERS
-- =============================================================
CREATE TABLE IF NOT EXISTS "Reminders" (
                                           "Id"           INTEGER PRIMARY KEY AUTOINCREMENT,
                                           "IdLoanDetail" INTEGER NOT NULL,
                                           "Channel"      TEXT NOT NULL,
                                           "Status"       TEXT NOT NULL DEFAULT 'Pending',
                                           "SentAt"       TEXT,
                                           "Content"      TEXT,
                                           "ErrorMessage" TEXT,
                                           "CreatedAt"    TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                           "UpdatedAt"    TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                           FOREIGN KEY ("IdLoanDetail")
                                               REFERENCES "LoanDetails"("Id")
                                               ON UPDATE CASCADE
                                               ON DELETE CASCADE,

                                           CHECK ("Channel" IN ('Email', 'SMS', 'Both')),
                                           CHECK ("Status" IN ('Pending', 'Sent', 'Failed')),
                                           CHECK (
                                               ("Status" = 'Sent' AND "SentAt" IS NOT NULL)
                                                   OR ("Status" <> 'Sent')
                                               )
);

CREATE INDEX IF NOT EXISTS "IX_Reminders_LoanDetail_CreatedAt"
    ON "Reminders"("IdLoanDetail", "CreatedAt" DESC);

CREATE INDEX IF NOT EXISTS "IX_Reminders_Status"
    ON "Reminders"("Status");

-- =============================================================
-- END OF SCHEMA
-- =============================================================