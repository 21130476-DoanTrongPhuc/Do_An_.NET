PRAGMA foreign_keys = ON;

-- Bổ sung trường cho Books (Nghiệp vụ 1.1)
ALTER TABLE "Books" ADD COLUMN "Language" TEXT DEFAULT 'Tiếng Việt';
ALTER TABLE "Books" ADD COLUMN "PageCount" INTEGER;
ALTER TABLE "Books" ADD COLUMN "CoverImagePath" TEXT;
ALTER TABLE "Books" ADD COLUMN "Status" TEXT NOT NULL DEFAULT 'Active';

-- Bổ sung trường nguồn nhập & bảng lịch sử trạng thái bản sách (Nghiệp vụ 1.2)
ALTER TABLE "BookCopies" ADD COLUMN "Source" TEXT;

CREATE TABLE IF NOT EXISTS "BookCopyHistories" (
                                                   "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
                                                   "IdBookCopy"    INTEGER NOT NULL,
                                                   "ChangedAt"     TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                                   "ActionType"    TEXT NOT NULL,
                                                   "OldStatus"     TEXT,
                                                   "NewStatus"     TEXT,
                                                   "OldCondition"  TEXT,
                                                   "NewCondition"  TEXT,
                                                   "OldLocation"   TEXT,
                                                   "NewLocation"   TEXT,
                                                   "Note"          TEXT,
                                                   FOREIGN KEY ("IdBookCopy") REFERENCES "BookCopies"("Id") ON DELETE CASCADE
    );