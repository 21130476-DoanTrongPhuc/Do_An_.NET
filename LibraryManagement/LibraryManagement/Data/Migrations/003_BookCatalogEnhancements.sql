PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS "BookCopyHistory" (
                                                 "Id"           INTEGER PRIMARY KEY AUTOINCREMENT,
                                                 "IdBookCopy"   INTEGER NOT NULL,
                                                 "ChangedAt"    TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                                 "ActionType"   TEXT NOT NULL,
                                                 "OldStatus"    TEXT,
                                                 "NewStatus"    TEXT,
                                                 "OldCondition" TEXT,
                                                 "NewCondition" TEXT,
                                                 "OldLocation"  TEXT,
                                                 "NewLocation"  TEXT,
                                                 "Note"         TEXT,

                                                 FOREIGN KEY ("IdBookCopy")
                                                     REFERENCES "BookCopies"("Id")
                                                     ON UPDATE CASCADE
                                                     ON DELETE CASCADE,

                                                 CHECK ("OldStatus" IS NULL OR "OldStatus" IN ('Available', 'Borrowed', 'Reserved', 'Disposed', 'Maintenance')),
                                                 CHECK ("NewStatus" IS NULL OR "NewStatus" IN ('Available', 'Borrowed', 'Reserved', 'Disposed', 'Maintenance')),
                                                 CHECK ("OldCondition" IS NULL OR "OldCondition" IN ('New', 'Good', 'Worn', 'Damaged', 'Lost')),
                                                 CHECK ("NewCondition" IS NULL OR "NewCondition" IN ('New', 'Good', 'Worn', 'Damaged', 'Lost'))
);

CREATE INDEX IF NOT EXISTS "IX_BookCopyHistory_IdBookCopy"
    ON "BookCopyHistory" ("IdBookCopy");