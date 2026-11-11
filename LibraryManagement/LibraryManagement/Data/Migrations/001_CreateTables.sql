-- =====================================================
-- LIBRARY MANAGEMENT SYSTEM
-- Migration: 001_CreateCoreTables.sql
-- Database: SQLite
-- Convention-aligned with EF Core (PascalCase)
-- =====================================================

-- 1. BOOK CATEGORIES
CREATE TABLE IF NOT EXISTS "BookCategories" (
                                                "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
                                                "IdParent"        INTEGER NULL,
                                                "CategoryCode"    TEXT    NOT NULL UNIQUE,
                                                "CategoryName"    TEXT    NOT NULL,
                                                "MaxLoanDays"     INTEGER NOT NULL DEFAULT 14,
                                                "MaxRenewals"     INTEGER NOT NULL DEFAULT 1,
                                                "Status"          TEXT    NOT NULL DEFAULT 'Active',
                                                "CreatedAt"       TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                                "UpdatedAt"       TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                                FOREIGN KEY ("IdParent")
                                                    REFERENCES "BookCategories"("Id")
                                                    ON UPDATE CASCADE
                                                    ON DELETE RESTRICT,

                                                CHECK ("IdParent" IS NULL OR "IdParent" <> "Id"),
                                                CHECK ("MaxLoanDays" > 0),
                                                CHECK ("MaxRenewals" >= 0),
                                                CHECK ("Status" IN ('Active', 'Inactive'))
);

-- 2. BOOKS
CREATE TABLE IF NOT EXISTS "Books" (
                                       "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "IdCategory"      INTEGER NOT NULL,
                                       "BookCode"        TEXT    NOT NULL UNIQUE,
                                       "Isbn"            TEXT    UNIQUE,
                                       "Title"           TEXT    NOT NULL,
                                       "Author"          TEXT    NOT NULL,
                                       "Publisher"       TEXT,
                                       "PublishedYear"   INTEGER,
                                       "Description"     TEXT,
                                       "CreatedAt"       TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "UpdatedAt"       TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                       FOREIGN KEY ("IdCategory")
                                           REFERENCES "BookCategories"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT,

                                       CHECK ("PublishedYear" IS NULL OR "PublishedYear" BETWEEN 1000 AND 9999)
);

-- 3. BOOK COPIES
CREATE TABLE IF NOT EXISTS "BookCopies" (
                                            "Id"                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                            "IdBook"              INTEGER NOT NULL,
                                            "CopyCode"            TEXT    NOT NULL UNIQUE,
                                            "ShelfLocation"       TEXT,
                                            "ReceivedAt"          TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                            "PurchasePriceVnd"    INTEGER NOT NULL DEFAULT 0,
                                            "PhysicalCondition"   TEXT    NOT NULL DEFAULT 'Good',
                                            "CirculationStatus"   TEXT    NOT NULL DEFAULT 'Available',
                                            "Note"                TEXT,
                                            "CreatedAt"           TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                            "UpdatedAt"           TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                            FOREIGN KEY ("IdBook")
                                                REFERENCES "Books"("Id")
                                                ON UPDATE CASCADE
                                                ON DELETE RESTRICT,

                                            CHECK ("PurchasePriceVnd" >= 0),
                                            CHECK ("PhysicalCondition" IN ('New', 'Good', 'Worn', 'Damaged')),
                                            CHECK ("CirculationStatus" IN ('Available', 'Borrowed', 'Reserved', 'Maintenance', 'Lost'))
);

-- 5. READERS
CREATE TABLE IF NOT EXISTS "Readers" (
                                         "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                         "MaxActiveCopies"   INTEGER NOT NULL DEFAULT 3,
                                         "ReaderCode"        TEXT    NOT NULL UNIQUE,
                                         "FullName"          TEXT    NOT NULL,
                                         "DateOfBirth"       TEXT,
                                         "Phone"             TEXT,
                                         "Email"             TEXT,
                                         "Address"           TEXT,
                                         "CreatedAt"         TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                         "UpdatedAt"         TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                         CHECK ("MaxActiveCopies" > 0)
);

-- 6. READER CARDS
CREATE TABLE IF NOT EXISTS "ReaderCards" (
                                             "Id"          INTEGER PRIMARY KEY AUTOINCREMENT,
                                             "IdReader"    INTEGER NOT NULL,
                                             "CardCode"    TEXT    NOT NULL UNIQUE,
                                             "IssuedAt"    TEXT    NOT NULL,
                                             "ExpiresAt"   TEXT    NOT NULL,
                                             "Status"      TEXT    NOT NULL DEFAULT 'Active',
                                             "Note"        TEXT,

                                             FOREIGN KEY ("IdReader")
                                                 REFERENCES "Readers"("Id")
                                                 ON UPDATE CASCADE
                                                 ON DELETE RESTRICT,

                                             CHECK ("ExpiresAt" >= "IssuedAt"),
                                             CHECK ("Status" IN ('Active', 'Expired', 'Locked'))
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_ReaderCards_OneActive"
    ON "ReaderCards"("IdReader")
    WHERE "Status" = 'Active';

-- 7. LIBRARIANS
CREATE TABLE IF NOT EXISTS "Librarians" (
                                            "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
                                            "EmployeeCode"    TEXT    NOT NULL UNIQUE,
                                            "Username"        TEXT    NOT NULL UNIQUE,
                                            "PasswordHash"    TEXT    NOT NULL,
                                            "FullName"        TEXT    NOT NULL,
                                            "Phone"           TEXT,
                                            "Email"           TEXT,
                                            "Address"         TEXT,
                                            "Role"            TEXT    NOT NULL DEFAULT 'Librarian',
                                            "Status"          TEXT    NOT NULL DEFAULT 'Active',
                                            "CreatedAt"       TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                            "UpdatedAt"       TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                            CHECK ("Role" IN ('Admin', 'Librarian')),
                                            CHECK ("Status" IN ('Active', 'Inactive'))
);

-- 8. LOANS
CREATE TABLE IF NOT EXISTS "Loans" (
                                       "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "LoanCode"      TEXT    NOT NULL UNIQUE,
                                       "IdReader"      INTEGER NOT NULL,
                                       "IdLibrarian"   INTEGER NOT NULL,
                                       "BorrowedAt"    TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "Note"          TEXT,

                                       FOREIGN KEY ("IdReader")
                                           REFERENCES "Readers"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT,

                                       FOREIGN KEY ("IdLibrarian")
                                           REFERENCES "Librarians"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT
);

-- 9. LOAN DETAILS
CREATE TABLE IF NOT EXISTS "LoanDetails" (
                                             "Id"                        INTEGER PRIMARY KEY AUTOINCREMENT,
                                             "IdLoan"                    INTEGER NOT NULL,
                                             "IdBookCopy"                INTEGER NOT NULL,
                                             "DueAt"                     TEXT    NOT NULL,
                                             "ReturnedAt"                TEXT,
                                             "PhysicalConditionBefore"   TEXT    NOT NULL,
                                             "PhysicalConditionAfter"    TEXT,
                                             "IdLibrarianReturn"         INTEGER,
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

                                             FOREIGN KEY ("IdLibrarianReturn")
                                                 REFERENCES "Librarians"("Id")
                                                 ON UPDATE CASCADE
                                                 ON DELETE RESTRICT,

                                             CHECK ("RenewalCount" >= 0),
                                             CHECK ("PhysicalConditionBefore" IN ('New', 'Good', 'Worn', 'Damaged')),
                                             CHECK ("PhysicalConditionAfter" IS NULL
                                                 OR "PhysicalConditionAfter" IN ('New', 'Good', 'Worn', 'Damaged'))
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_LoanDetails_ActiveCopy"
    ON "LoanDetails"("IdBookCopy")
    WHERE "ReturnedAt" IS NULL;

-- 10. RESERVATIONS
CREATE TABLE IF NOT EXISTS "Reservations" (
                                              "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                              "ReservationCode"   TEXT    NOT NULL UNIQUE,
                                              "IdReader"          INTEGER NOT NULL,
                                              "IdBook"            INTEGER NOT NULL,
                                              "IdBookCopy"        INTEGER,
                                              "IdLibrarian"       INTEGER,
                                              "IdLoan"            INTEGER UNIQUE,
                                              "ReservedAt"        TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                              "ExpiresAt"         TEXT,
                                              "AssignedAt"        TEXT,
                                              "FulfilledAt"       TEXT,
                                              "CancelledAt"       TEXT,
                                              "Status"            TEXT    NOT NULL DEFAULT 'Pending',
                                              "Note"              TEXT,

                                              FOREIGN KEY ("IdReader")
                                                  REFERENCES "Readers"("Id")
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

                                              FOREIGN KEY ("IdLibrarian")
                                                  REFERENCES "Librarians"("Id")
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

-- 11. FINES
CREATE TABLE IF NOT EXISTS "Fines" (
                                       "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                       "FineCode"          TEXT    NOT NULL UNIQUE,
                                       "IdLoanDetail"      INTEGER NOT NULL,
                                       "IdLibrarian"       INTEGER NOT NULL,
                                       "CreatedAt"         TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                       "ViolationType"     TEXT    NOT NULL,
                                       "AmountVnd"         INTEGER NOT NULL,
                                       "Description"       TEXT,

                                       FOREIGN KEY ("IdLoanDetail")
                                           REFERENCES "LoanDetails"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT,

                                       FOREIGN KEY ("IdLibrarian")
                                           REFERENCES "Librarians"("Id")
                                           ON UPDATE CASCADE
                                           ON DELETE RESTRICT,

                                       CHECK ("AmountVnd" > 0)
);

-- 12. FINE PAYMENTS
CREATE TABLE IF NOT EXISTS "FinePayments" (
                                              "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                              "PaymentCode"       TEXT    NOT NULL UNIQUE,
                                              "IdFine"            INTEGER NOT NULL,
                                              "IdLibrarian"       INTEGER NOT NULL,
                                              "PaidAt"            TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                              "AmountVnd"         INTEGER NOT NULL,
                                              "PaymentMethod"     TEXT    NOT NULL DEFAULT 'Cash',
                                              "Note"              TEXT,

                                              FOREIGN KEY ("IdFine")
                                                  REFERENCES "Fines"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              FOREIGN KEY ("IdLibrarian")
                                                  REFERENCES "Librarians"("Id")
                                                  ON UPDATE CASCADE
                                                  ON DELETE RESTRICT,

                                              CHECK ("AmountVnd" > 0),
                                              CHECK ("PaymentMethod" IN ('Cash', 'Other'))
);