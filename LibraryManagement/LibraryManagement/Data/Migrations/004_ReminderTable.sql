-- =====================================================
-- 11. REMINDERS (Lịch sử nhắc nhở quá hạn)
-- =====================================================

CREATE TABLE IF NOT EXISTS "Reminders" (
                                           "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
                                           "IdLoanDetail"      INTEGER NOT NULL,
                                           "Channel"           TEXT NOT NULL,                  -- Kênh gửi: Email, SMS
                                           "Status"            TEXT NOT NULL DEFAULT 'Pending',-- Trạng thái: Pending, Sent, Failed
                                           "SentAt"            TEXT,                           -- Thời điểm gửi thành công (NULL nếu chưa gửi)
                                           "Content"           TEXT,                           -- Nội dung tin nhắn/email (tùy chọn lưu lại)
                                           "ErrorMessage"      TEXT,                           -- Lỗi nếu gửi thất bại
                                           "CreatedAt"         TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                           "UpdatedAt"         TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                                           FOREIGN KEY ("IdLoanDetail")
                                               REFERENCES "LoanDetails"("Id")
                                               ON UPDATE CASCADE
                                               ON DELETE CASCADE,

                                           CHECK ("Channel" IN ('Email', 'SMS')),
                                           CHECK ("Status" IN ('Pending', 'Sent', 'Failed')),
                                           CHECK (
                                               ("Status" = 'Sent' AND "SentAt" IS NOT NULL)
                                                   OR
                                               ("Status" <> 'Sent')
                                               )
);

-- Index tối ưu cho việc query trạng thái nhắc nhở mới nhất của 1 LoanDetail
CREATE INDEX IF NOT EXISTS "IX_Reminders_LoanDetail_CreatedAt"
    ON "Reminders"("IdLoanDetail", "CreatedAt" DESC);