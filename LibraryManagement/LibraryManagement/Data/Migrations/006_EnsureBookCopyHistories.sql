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