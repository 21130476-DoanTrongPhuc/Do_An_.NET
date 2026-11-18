-- Cột khóa quyền mượn của độc giả
ALTER TABLE "Readers" ADD COLUMN "BorrowStatus" TEXT NOT NULL DEFAULT 'Allowed'
    CHECK ("BorrowStatus" IN ('Allowed', 'Blocked'));
ALTER TABLE "Readers" ADD COLUMN "BorrowBlockReason" TEXT;

-- Bảng cấu hình hệ thống
CREATE TABLE IF NOT EXISTS "Settings" (
                                          "Key"   TEXT PRIMARY KEY,
                                          "Value" TEXT NOT NULL
);

INSERT OR IGNORE INTO "Settings" ("Key", "Value") VALUES ('MaxUnpaidFineVnd', '50000');