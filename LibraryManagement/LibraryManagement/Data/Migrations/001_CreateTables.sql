-- =====================================================
-- LIBRARY MANAGEMENT SYSTEM (Single Librarian Version)
-- Migration: 001_CreateCoreTables.sql
-- Database: SQLite
-- =====================================================

PRAGMA foreign_keys = ON;

-- =====================================================
-- 1. USERS
-- Reader, Librarian (duy nhất) và Admin
-- =====================================================

CREATE TABLE IF NOT EXISTS "Users" (
                                       "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "UserCode"          TEXT    NOT NULL UNIQUE,
                                       "Username"          TEXT    UNIQUE,
                                       "PasswordHash"      TEXT,
                                       "FullName"          TEXT    NOT NULL,
                                       "DateOfBirth"       TEXT,
                                       "Phone"             TEXT,
                                       "Email"             TEXT,
                                       "Address"           TEXT,
                                       "UserType"          TEXT    NOT NULL,
                                       "MaxActiveCopies"   INTEGER,
                                       "Status"            TEXT    NOT NULL DEFAULT 'Active',
                                       "CreatedAt"         TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "UpdatedAt"         TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                       CHECK ("UserType" IN ('Reader', 'Librarian', 'Admin')),
                                       CHECK ("Status" IN ('Active', 'Inactive')),
                                       CHECK (
                                           ("UserType" = 'Reader'
                                               AND "MaxActiveCopies" IS NOT NULL
                                               AND "MaxActiveCopies" > 0)
                                               OR
                                           ("UserType" IN ('Librarian', 'Admin')
                                               AND "MaxActiveCopies" IS NULL)
                                           ),
                                       CHECK (
                                           ("Username" IS NULL AND "PasswordHash" IS NULL)
                                               OR
                                           ("Username" IS NOT NULL AND "PasswordHash" IS NOT NULL)
                                           )
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Users_Username"
    ON "Users"("Username")
    WHERE "Username" IS NOT NULL;

-- Đảm bảo chỉ có duy nhất 1 Librarian đang Active (tùy chọn)
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Users_SingleLibrarian"
    ON "Users"("UserType")
    WHERE "UserType" = 'Librarian' AND "Status" = 'Active';


-- =====================================================
-- 2. BOOK CATEGORIES
-- =====================================================

CREATE TABLE IF NOT EXISTS "BookCategories" (
                                                "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
                                                "IdParent"        INTEGER,
                                                "CategoryCode"    TEXT NOT NULL UNIQUE,
                                                "CategoryName"    TEXT NOT NULL,
                                                "MaxLoanDays"     INTEGER NOT NULL DEFAULT 14,
                                                "MaxRenewals"     INTEGER NOT NULL DEFAULT 1,
                                                "Status"          TEXT NOT NULL DEFAULT 'Active',
                                                "CreatedAt"       TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                                "UpdatedAt"       TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                                FOREIGN KEY ("IdParent")
                                                    REFERENCES "BookCategories"("Id")
                                                    ON UPDATE CASCADE
                                                    ON DELETE RESTRICT,

                                                CHECK ("IdParent" IS NULL OR "IdParent" <> "Id"),
                                                CHECK ("MaxLoanDays" > 0),
                                                CHECK ("MaxRenewals" >= 0),
                                                CHECK ("Status" IN ('Active', 'Inactive'))
);


-- =====================================================
-- 3. BOOKS
-- =====================================================

CREATE TABLE IF NOT EXISTS "Books" (
                                       "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "IdCategory"      INTEGER NOT NULL,
                                       "BookCode"        TEXT NOT NULL UNIQUE,
                                       "Isbn"            TEXT UNIQUE,
                                       "Title"           TEXT NOT NULL,
                                       "Author"          TEXT NOT NULL,
                                       "Publisher"       TEXT,
                                       "PublishedYear"   INTEGER,
                                       "Description"     TEXT,
                                       "CreatedAt"       TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "UpdatedAt"       TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                       FOREIGN KEY ("IdCategory")
                                           REFERENCES "BookCategories"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT,

                                       CHECK (
                                           "PublishedYear" IS NULL
                                               OR "PublishedYear" BETWEEN 1000 AND 9999
                                           )
);


-- =====================================================
-- 4. BOOK COPIES
-- =====================================================

CREATE TABLE IF NOT EXISTS "BookCopies" (
                                            "Id"                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                            "IdBook"              INTEGER NOT NULL,
                                            "CopyCode"            TEXT NOT NULL UNIQUE,
                                            "ShelfLocation"       TEXT,
                                            "ReceivedAt"          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                            "PurchasePriceVnd"    INTEGER NOT NULL DEFAULT 0,
                                            "PhysicalCondition"   TEXT NOT NULL DEFAULT 'Good',
                                            "CirculationStatus"   TEXT NOT NULL DEFAULT 'Available',
                                            "Note"                TEXT,
                                            "CreatedAt"           TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                            "UpdatedAt"           TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                            FOREIGN KEY ("IdBook")
                                                REFERENCES "Books"("Id")
                                                ON UPDATE CASCADE
                                                ON DELETE RESTRICT,

                                            CHECK ("PurchasePriceVnd" >= 0),
                                            CHECK ("PhysicalCondition" IN ('New', 'Good', 'Worn', 'Damaged')),
                                            CHECK ("CirculationStatus" IN (
                                                                           'Available', 'Borrowed', 'Reserved', 'Maintenance', 'Lost'
                                                ))
);


-- =====================================================
-- 5. READER CARDS
-- =====================================================

CREATE TABLE IF NOT EXISTS "ReaderCards" (
                                             "Id"          INTEGER PRIMARY KEY AUTOINCREMENT,
                                             "IdReader"    INTEGER NOT NULL,
                                             "CardCode"    TEXT NOT NULL UNIQUE,
                                             "IssuedAt"    TEXT NOT NULL,
                                             "ExpiresAt"   TEXT NOT NULL,
                                             "Status"      TEXT NOT NULL DEFAULT 'Active',
                                             "Note"        TEXT,

                                             FOREIGN KEY ("IdReader")
                                                 REFERENCES "Users"("Id")
                                                 ON UPDATE CASCADE
                                                 ON DELETE RESTRICT,

                                             CHECK ("ExpiresAt" >= "IssuedAt"),
                                             CHECK ("Status" IN ('Active', 'Expired', 'Locked'))
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_ReaderCards_OneActive"
    ON "ReaderCards"("IdReader")
    WHERE "Status" = 'Active';


-- =====================================================
-- 6. LOANS (Bỏ IdLibrarian)
-- =====================================================

CREATE TABLE IF NOT EXISTS "Loans" (
                                       "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "LoanCode"      TEXT NOT NULL UNIQUE,
                                       "IdReader"      INTEGER NOT NULL,
                                       "BorrowedAt"    TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "Note"          TEXT,

                                       FOREIGN KEY ("IdReader")
                                           REFERENCES "Users"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT
);


-- =====================================================
-- 7. LOAN DETAILS (Bỏ IdLibrarianReturn)
-- =====================================================

CREATE TABLE IF NOT EXISTS "LoanDetails" (
                                             "Id"                        INTEGER PRIMARY KEY AUTOINCREMENT,
                                             "IdLoan"                    INTEGER NOT NULL,
                                             "IdBookCopy"                INTEGER NOT NULL,
                                             "DueAt"                     TEXT NOT NULL,
                                             "ReturnedAt"                TEXT,
                                             "PhysicalConditionBefore"   TEXT NOT NULL,
                                             "PhysicalConditionAfter"    TEXT,
                                             "RenewalCount"              INTEGER NOT NULL DEFAULT 0,
                                             "Note"                      TEXT,

                                             FOREIGN KEY ("IdLoan")
                                                 REFERENCES "Loans"("Id")
                                                 ON UPDATE CASCADE
                                                 ON DELETE RESTRICT,

                                             FOREIGN KEY ("IdBookCopy")
                                                 REFERENCES "BookCopies"("Id")
                                                 ON UPDATE CASCADE
                                                 ON DELETE RESTRICT,

                                             CHECK ("RenewalCount" >= 0),
                                             CHECK ("PhysicalConditionBefore" IN (
                                                                                  'New', 'Good', 'Worn', 'Damaged'
                                                 )),
                                             CHECK (
                                                 "PhysicalConditionAfter" IS NULL
                                                     OR "PhysicalConditionAfter" IN (
                                                                                     'New', 'Good', 'Worn', 'Damaged'
                                                     )
                                                 )
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_LoanDetails_ActiveCopy"
    ON "LoanDetails"("IdBookCopy")
    WHERE "ReturnedAt" IS NULL;


-- =====================================================
-- 8. RESERVATIONS (Bỏ IdLibrarian)
-- =====================================================

CREATE TABLE IF NOT EXISTS "Reservations" (
                                              "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                              "ReservationCode"   TEXT NOT NULL UNIQUE,
                                              "IdReader"          INTEGER NOT NULL,
                                              "IdBook"            INTEGER NOT NULL,
                                              "IdBookCopy"        INTEGER,
                                              "IdLoan"            INTEGER UNIQUE,
                                              "ReservedAt"        TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                              "ExpiresAt"         TEXT,
                                              "AssignedAt"        TEXT,
                                              "FulfilledAt"       TEXT,
                                              "CancelledAt"       TEXT,
                                              "Status"            TEXT NOT NULL DEFAULT 'Pending',
                                              "Note"              TEXT,

                                              FOREIGN KEY ("IdReader")
                                                  REFERENCES "Users"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              FOREIGN KEY ("IdBook")
                                                  REFERENCES "Books"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              FOREIGN KEY ("IdBookCopy")
                                                  REFERENCES "BookCopies"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              FOREIGN KEY ("IdLoan")
                                                  REFERENCES "Loans"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              CHECK ("Status" IN (
                                                                  'Pending', 'ReadyForPickup', 'Completed',
                                                                  'Cancelled', 'Expired'
                                                  ))
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Reservations_ActiveReaderBook"
    ON "Reservations"("IdReader", "IdBook")
    WHERE "Status" IN ('Pending', 'ReadyForPickup');

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Reservations_AssignedCopy"
    ON "Reservations"("IdBookCopy")
    WHERE "IdBookCopy" IS NOT NULL
        AND "Status" = 'ReadyForPickup';


-- =====================================================
-- 9. FINES (Bỏ IdLibrarian)
-- =====================================================

CREATE TABLE IF NOT EXISTS "Fines" (
                                       "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "FineCode"          TEXT    NOT NULL UNIQUE,
                                       "IdLoanDetail"      INTEGER,                          -- nullable: cho phép phạt không gắn phiếu mượn
                                       "CreatedAt"         TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "ViolationType"     TEXT    NOT NULL,
                                       "AmountVnd"         INTEGER NOT NULL,
                                       "Description"       TEXT,

    -- Trạng thái thanh toán
                                       "Status"            TEXT    NOT NULL DEFAULT 'Unpaid',

    -- Hủy phiếu (chỉ dùng khi Status = 'Cancelled')
                                       "CancelledAt"       TEXT,
                                       "CancelReason"      TEXT,

    -- Audit điều chỉnh
                                       "UpdatedAt"         TEXT,

                                       FOREIGN KEY ("IdLoanDetail")
                                           REFERENCES "LoanDetails"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT,


                                       CHECK ("AmountVnd" > 0),
                                       CHECK ("Status" IN ('Unpaid', 'Partial', 'Paid', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS "IX_Fines_IdLoanDetail" ON "Fines"("IdLoanDetail");
CREATE INDEX IF NOT EXISTS "IX_Fines_Status"       ON "Fines"("Status");
CREATE INDEX IF NOT EXISTS "IX_Fines_CreatedAt"    ON "Fines"("CreatedAt");


-- =====================================================
-- 10. FINE PAYMENTS (Bỏ IdLibrarian)
-- =====================================================

CREATE TABLE IF NOT EXISTS "FinePayments" (
                                              "Id"                 INTEGER PRIMARY KEY AUTOINCREMENT,
                                              "PaymentCode"        TEXT    NOT NULL UNIQUE,
                                              "IdFine"             INTEGER NOT NULL,
                                              "PaidAt"             TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                              "AmountVnd"          INTEGER NOT NULL,
                                              "PaymentMethod"      TEXT    NOT NULL DEFAULT 'Cash',
                                              "Note"               TEXT,
                                              "CreatedAt"          TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP, -- THÊM: thời điểm ghi nhận (khác PaidAt)

                                              FOREIGN KEY ("IdFine")
                                                  REFERENCES "Fines"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              CHECK ("AmountVnd" > 0),
                                              CHECK ("PaymentMethod" IN ('Cash', 'BankTransfer', 'Other'))
);

CREATE INDEX IF NOT EXISTS "IX_FinePayments_IdFine" ON "FinePayments"("IdFine");
CREATE INDEX IF NOT EXISTS "IX_FinePayments_PaidAt" ON "FinePayments"("PaidAt");


CREATE TABLE IF NOT EXISTS "FineAuditLogs" (
                                               "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
                                               "IdFine"        INTEGER NOT NULL,
                                               "Action"        TEXT    NOT NULL,          -- 'Created', 'Adjusted', 'Cancelled', 'Paid'
                                               "BeforeAmount"  INTEGER NOT NULL DEFAULT 0,
                                               "AfterAmount"   INTEGER NOT NULL DEFAULT 0,
                                               "Reason"        TEXT,
                                               "ActorUserId"   INTEGER NOT NULL,
                                               "At"            TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                               FOREIGN KEY ("IdFine")
                                                   REFERENCES "Fines"("Id")
                                                   ON UPDATE CASCADE
                                                   ON DELETE CASCADE,          -- xóa phiếu thì xóa luôn audit (hoặc RESTRICT nếu muốn giữ)

                                               FOREIGN KEY ("ActorUserId")
                                                   REFERENCES "Users"("Id")
                                                   ON UPDATE CASCADE
                                                   ON DELETE RESTRICT,

                                               CHECK ("Action" IN ('Created', 'Adjusted', 'Cancelled', 'Paid'))
);

CREATE INDEX IF NOT EXISTS "IX_FineAuditLogs_IdFine" ON "FineAuditLogs"("IdFine");
CREATE INDEX IF NOT EXISTS "IX_FineAuditLogs_At"     ON "FineAuditLogs"("At");