PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS "FavoriteBooks" (
                                               "IdReader"  INTEGER NOT NULL,
                                               "IdBook"    INTEGER NOT NULL,
                                               "AddedAt"   TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                               PRIMARY KEY ("IdReader", "IdBook"),
    FOREIGN KEY ("IdReader") REFERENCES "Users"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("IdBook")   REFERENCES "Books"("Id") ON DELETE CASCADE
    );