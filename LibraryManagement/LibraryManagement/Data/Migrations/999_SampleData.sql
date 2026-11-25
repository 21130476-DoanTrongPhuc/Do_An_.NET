PRAGMA foreign_keys = ON;

-- =============================================================
-- 1. BOOK CATEGORIES
-- =============================================================
INSERT OR IGNORE INTO "BookCategories"
("Id", "IdParent", "CategoryCode", "CategoryName", "MaxLoanDays", "MaxRenewals", "Status")
VALUES
    (1, NULL, 'CAT-FIC',  'Văn học',              21, 2, 'Active'),
    (2, NULL, 'CAT-TECH', 'Công nghệ thông tin',  14, 1, 'Active'),
    (3, NULL, 'CAT-LANG', 'Ngoại ngữ',            21, 2, 'Active'),
    (4, 1,    'CAT-VN',   'Văn học Việt Nam',     21, 2, 'Active'),
    (5, 2,    'CAT-PROG', 'Lập trình',            14, 1, 'Active'),
    (6, 2,    'CAT-DB',   'Cơ sở dữ liệu',        14, 1, 'Active');

-- =============================================================
-- 2. USERS
-- =============================================================
INSERT OR IGNORE INTO "Users"
("Id", "UserCode", "Username", "PasswordHash", "FullName", "DateOfBirth",
 "Phone", "Email", "Address", "UserType", "MaxActiveCopies", "Status")
VALUES
    (1,  'RD0001', 'reader001', '123123', 'Hoàng Minh Phong', '2006-12-24', '0973248024', 'reader001@example.com', '187 Đường mẫu, TP.HCM', 'Reader', 3, 'Active'),
    (2,  'RD0002', NULL,        NULL,     'Bùi Ngọc Hùng',    '1985-09-11', '0940692264', 'reader002@example.com', '207 Đường mẫu, TP.HCM', 'Reader', 3, 'Active'),
    (3,  'RD0003', NULL,        NULL,     'Huỳnh Hữu Nga',    '1992-09-05', '0957234631', 'reader003@example.com', '180 Đường mẫu, TP.HCM', 'Reader', 3, 'Active'),
    (4,  'RD0004', NULL,        NULL,     'Đặng Thanh Yến',   '1992-12-08', '0933436157', 'reader004@example.com', '145 Đường mẫu, TP.HCM', 'Reader', 3, 'Active'),
    (5,  'RD0005', NULL,        NULL,     'Bùi Thanh Nga',    '1986-03-06', '0937694000', 'reader005@example.com', '68 Đường mẫu, TP.HCM',  'Reader', 3, 'Active'),
    (6,  'RD0006', NULL,        NULL,     'Đỗ Thị Chi',       '1989-08-02', '0920976941', 'reader006@example.com', '48 Đường mẫu, TP.HCM',  'Reader', 3, 'Active'),
    (7,  'RD0007', NULL,        NULL,     'Đỗ Quốc Trang',    '1987-08-06', '0921870117', 'reader007@example.com', '56 Đường mẫu, TP.HCM',  'Reader', 3, 'Active'),
    (8,  'RD0008', NULL,        NULL,     'Võ Thanh Linh',    '1982-10-25', '0927769135', 'reader008@example.com', '206 Đường mẫu, TP.HCM', 'Reader', 3, 'Active'),
    (9,  'RD0009', NULL,        NULL,     'Nguyễn Thị Hiếu',  '1991-03-07', '0976603921', 'reader009@example.com', '78 Đường mẫu, TP.HCM',  'Reader', 3, 'Active'),
    (10, 'RD0010', NULL,        NULL,     'Lê Thị Hải',       '1981-10-10', '0989630100', 'reader010@example.com', '6 Đường mẫu, TP.HCM',   'Reader', 5, 'Active'),
    (11, 'RD0011', NULL,        NULL,     'Đặng Ngọc An',     '1994-04-25', '0943416557', 'reader011@example.com', '195 Đường mẫu, TP.HCM', 'Reader', 3, 'Active'),
    (12, 'RD0012', NULL,        NULL,     'Bùi Thanh Lan',    '2007-03-26', '0972531067', 'reader012@example.com', '50 Đường mẫu, TP.HCM',  'Reader', 3, 'Active'),
    (13, 'RD0013', NULL,        NULL,     'Vũ Văn Vy',        '1988-01-26', '0948680824', 'reader013@example.com', '83 Đường mẫu, TP.HCM',  'Reader', 3, 'Active'),
    (14, 'RD0014', NULL,        NULL,     'Phan Minh Nam',    '2000-06-02', '0915653550', 'reader014@example.com', '3 Đường mẫu, TP.HCM',   'Reader', 3, 'Active'),
    (15, 'RD0015', NULL,        NULL,     'Vũ Văn Giang',     '1982-07-03', '0998941018', 'reader015@example.com', '5 Đường mẫu, TP.HCM',   'Reader', 3, 'Active'),
    (16, 'NV001',  'admin',     '123456', 'Nguyễn Minh Anh',  NULL,         '0905587265', 'admin@example.com',     'Thư viện trung tâm',    'Librarian', NULL, 'Active');

-- =============================================================
-- 3. BOOKS
-- =============================================================
INSERT OR IGNORE INTO "Books"
("Id", "IdCategory", "BookCode", "Isbn", "Title", "Author", "Publisher", "PublishedYear", "Description")
VALUES
    (1,  4, 'BK0001', '9786041000001', 'Dế Mèn Phiêu Lưu Ký',        'Tô Hoài',                    'NXB Kim Đồng',    1941, 'Tác phẩm thiếu nhi kinh điển.'),
    (2,  4, 'BK0002', '9786041000002', 'Tuổi Thơ Dữ Dội',            'Phùng Quán',                 'NXB Kim Đồng',    1988, 'Tiểu thuyết lịch sử thiếu nhi.'),
    (3,  4, 'BK0003', '9786041000003', 'Số Đỏ',                      'Vũ Trọng Phụng',             'NXB Văn Học',     1936, 'Tiểu thuyết trào phúng.'),
    (4,  1, 'BK0004', '9786041000004', 'Nhà Giả Kim',                'Paulo Coelho',               'NXB Hội Nhà Văn', 1988, 'Tiểu thuyết nổi tiếng.'),
    (5,  5, 'BK0005', '9786041000005', 'Clean Code',                 'Robert C. Martin',           'Prentice Hall',   2008, 'Kỹ thuật viết code sạch.'),
    (6,  5, 'BK0006', '9786041000006', 'The Pragmatic Programmer',   'David Thomas, Andrew Hunt',  'Addison-Wesley',  1999, 'Kinh điển lập trình viên.'),
    (7,  6, 'BK0007', '9786041000007', 'Learning SQL',               'Alan Beaulieu',              'O''Reilly',       2020, 'Học SQL thực hành.'),
    (8,  6, 'BK0008', '9786041000008', 'Database System Concepts',   'Abraham Silberschatz et al.','McGraw-Hill',     2019, 'Giáo trình CSDL.'),
    (9,  3, 'BK0009', '9786041000009', 'English Grammar in Use',     'Raymond Murphy',             'Cambridge',       2019, 'Ngữ pháp tiếng Anh.'),
    (10, 3, 'BK0010', '9786041000010', 'Minna no Nihongo I',         '3A Corporation',             '3A',              2012, 'Giáo trình tiếng Nhật.'),
    (11, 5, 'BK0011', '9786041000011', 'Head First Java',            'Kathy Sierra, Bert Bates',   'O''Reilly',       2005, 'Học Java qua hình ảnh.'),
    (12, 6, 'BK0012', '9786041000012', 'Introduction to Algorithms', 'Thomas H. Cormen et al.',    'MIT Press',       2009, 'Giáo trình giải thuật.'),
    (13, 1, 'BK0013', '9786041000013', 'Đắc Nhân Tâm',               'Dale Carnegie',              'NXB Tổng hợp',    1936, 'Kỹ năng giao tiếp.'),
    (14, 2, 'BK0014', '9786041000014', 'Operating System Concepts',  'Abraham Silberschatz et al.','Wiley',           2018, 'Giáo trình hệ điều hành.'),
    (15, 3, 'BK0015', '9786041000015', 'Oxford Word Skills',         'Ruth Gairns, Stuart Redman', 'Oxford',          2016, 'Từ vựng tiếng Anh.');

-- =============================================================
-- 4. READER CARDS
-- =============================================================
WITH CardSeed (
               CardCode,
               UserCode,
               IssuedAt,
               ExpiresAt,
               Status,
               Note
    ) AS (
    VALUES
        ('RC00001', 'RD0001', '2026-08-04', '2027-08-25', 'Active',  'Thẻ mẫu'),
        ('RC00002', 'RD0002', '2026-08-09', '2028-03-09', 'Active',  'Thẻ mẫu'),
        ('RC00003', 'RD0003', '2026-04-21', '2027-03-15', 'Active',  'Thẻ mẫu'),
        ('RC00004', 'RD0004', '2026-07-27', '2027-04-16', 'Active',  'Thẻ mẫu'),
        ('RC00005', 'RD0005', '2026-10-14', '2027-11-28', 'Active',  'Thẻ mẫu'),
        ('RC00006', 'RD0006', '2026-09-13', '2027-01-05', 'Active',  'Thẻ mẫu'),
        ('RC00007', 'RD0007', '2026-05-17', '2027-02-28', 'Active',  'Thẻ mẫu'),
        ('RC00008', 'RD0008', '2026-10-11', '2027-12-28', 'Active',  'Thẻ mẫu'),
        ('RC00009', 'RD0009', '2026-09-21', '2027-09-25', 'Active',  'Thẻ mẫu'),
        ('RC00010', 'RD0010', '2026-10-10', '2027-11-16', 'Active',  'Thẻ mẫu'),
        ('RC00011', 'RD0011', '2026-06-10', '2027-09-26', 'Active',  'Thẻ mẫu'),
        ('RC00012', 'RD0012', '2026-07-19', '2027-07-29', 'Active',  'Thẻ mẫu'),
        ('RC00013', 'RD0013', '2026-10-15', '2027-01-13', 'Active',  'Thẻ mẫu'),
        ('RC00014', 'RD0014', '2026-07-27', '2028-03-23', 'Active',  'Thẻ mẫu'),
        ('RC00015', 'RD0015', '2026-10-07', '2027-05-16', 'Active',  'Thẻ mẫu'),

        ('RC00101', 'RD0001', '2023-10-11', '2024-08-12', 'Locked',  'Thẻ cũ'),
        ('RC00102', 'RD0002', '2025-01-21', '2025-04-10', 'Expired', 'Thẻ cũ'),
        ('RC00103', 'RD0003', '2025-06-20', '2025-09-05', 'Locked',  'Thẻ cũ')
)
INSERT OR IGNORE INTO "ReaderCards"
(
    "CardCode",
    "IdReader",
    "IssuedAt",
    "ExpiresAt",
    "Status",
    "Note"
)
SELECT
    cs.CardCode,
    u.Id,
    cs.IssuedAt,
    cs.ExpiresAt,
    cs.Status,
    cs.Note
FROM CardSeed cs
         INNER JOIN "Users" u
                    ON u.UserCode = cs.UserCode
WHERE u.UserType = 'Reader';

-- =============================================================
-- 5. BOOK COPIES
-- =============================================================
INSERT OR IGNORE INTO "BookCopies"
("Id", "IdBook", "CopyCode", "ShelfLocation", "ReceivedAt", "PurchasePriceVnd",
 "PhysicalCondition", "CirculationStatus", "Note")
VALUES
    (1,  1,  'CP00001', 'Kệ A-01', '2025-07-13',  65000, 'Good',    'Available', NULL),
    (2,  1,  'CP00002', 'Kệ A-01', '2024-02-03',  85000, 'Good',    'Borrowed',  'Đang được mượn'),
    (3,  1,  'CP00003', 'Kệ A-01', '2024-05-13', 150000, 'Good',    'Available', NULL),
    (4,  2,  'CP00004', 'Kệ B-01', '2024-05-08', 200000, 'Good',    'Available', NULL),
    (5,  2,  'CP00005', 'Kệ B-01', '2024-03-16',  65000, 'Good',    'Available', NULL),
    (6,  2,  'CP00006', 'Kệ B-01', '2026-01-29', 200000, 'Good',    'Available', NULL),
    (7,  3,  'CP00007', 'Kệ C-01', '2026-02-16',  65000, 'Good',    'Borrowed',  'Đang được mượn'),
    (8,  3,  'CP00008', 'Kệ C-01', '2025-10-22', 200000, 'Good',    'Reserved',  'Đã gán cho RS0001'),
    (9,  3,  'CP00009', 'Kệ C-01', '2025-10-25',  65000, 'Worn',    'Borrowed',  'Đang được mượn'),
    (10, 4,  'CP00010', 'Kệ D-01', '2023-02-05',  45000, 'Worn',    'Available', 'Đã trả, bìa sách trầy nhẹ'),
    (11, 4,  'CP00011', 'Kệ D-01', '2022-05-12', 150000, 'Good',    'Available', NULL),
    (12, 4,  'CP00012', 'Kệ D-01', '2026-06-01',  65000, 'Good',    'Reserved',  'Đã gán cho RS0002'),
    (13, 5,  'CP00013', 'Kệ E-01', '2023-01-07', 200000, 'Worn',    'Borrowed',  'Đang được mượn'),
    (14, 5,  'CP00014', 'Kệ E-01', '2022-03-31', 150000, 'Good',    'Available', NULL),
    (15, 5,  'CP00015', 'Kệ E-01', '2022-01-02', 200000, 'Good',    'Available', NULL),
    (16, 6,  'CP00016', 'Kệ F-01', '2024-05-20', 200000, 'Good',    'Available', NULL),
    (17, 6,  'CP00017', 'Kệ F-01', '2022-02-17',  65000, 'Good',    'Available', NULL),
    (18, 6,  'CP00018', 'Kệ F-01', '2022-04-16', 150000, 'Good',    'Available', NULL),
    (19, 7,  'CP00019', 'Kệ G-01', '2024-05-16', 150000, 'Good',    'Borrowed',  'Đang được mượn'),
    (20, 7,  'CP00020', 'Kệ G-01', '2023-01-23',  65000, 'Good',    'Available', NULL),
    (21, 7,  'CP00021', 'Kệ G-01', '2023-08-21', 150000, 'Good',    'Available', NULL),
    (22, 8,  'CP00022', 'Kệ H-01', '2022-11-16', 120000, 'Good',    'Available', NULL),
    (23, 8,  'CP00023', 'Kệ H-01', '2022-03-01',  65000, 'Good',    'Available', NULL),
    (24, 8,  'CP00024', 'Kệ H-01', '2021-12-10',  45000, 'Good',    'Available', NULL),
    (25, 9,  'CP00025', 'Kệ A-02', '2025-11-02', 200000, 'Good',    'Borrowed',  'Đang được mượn'),
    (26, 9,  'CP00026', 'Kệ A-02', '2026-01-16',  65000, 'Good',    'Available', NULL),
    (27, 9,  'CP00027', 'Kệ A-02', '2024-09-12',  45000, 'Worn',    'Available', NULL),
    (28, 10, 'CP00028', 'Kệ B-02', '2023-05-14',  65000, 'Worn',    'Borrowed',  'Đang được mượn'),
    (29, 10, 'CP00029', 'Kệ B-02', '2022-12-01',  65000, 'Worn',    'Available', NULL),
    (30, 10, 'CP00030', 'Kệ B-02', '2024-11-05', 120000, 'Good',    'Available', NULL),
    (31, 11, 'CP00031', 'Kệ C-02', '2022-02-05', 200000, 'Good',    'Available', NULL),
    (32, 11, 'CP00032', 'Kệ C-02', '2026-04-12', 150000, 'Good',    'Available', NULL),
    (33, 11, 'CP00033', 'Kệ C-02', '2024-08-23',  85000, 'Good',    'Available', NULL),
    (34, 12, 'CP00034', 'Kệ D-02', '2022-06-10',  65000, 'Good',    'Available', NULL),
    (35, 12, 'CP00035', 'Kệ D-02', '2025-03-13', 150000, 'Good',    'Available', NULL),
    (36, 12, 'CP00036', 'Kệ D-02', '2023-03-03',  85000, 'Damaged', 'Available', NULL),
    (37, 13, 'CP00037', 'Kệ E-02', '2022-07-17',  85000, 'Good',    'Available', NULL),
    (38, 13, 'CP00038', 'Kệ E-02', '2022-02-15',  85000, 'Good',    'Available', NULL),
    (39, 13, 'CP00039', 'Kệ E-02', '2025-08-14', 200000, 'Worn',    'Available', NULL),
    (40, 14, 'CP00040', 'Kệ F-02', '2023-05-11', 150000, 'Good',    'Available', NULL),
    (41, 14, 'CP00041', 'Kệ F-02', '2023-06-11', 200000, 'Worn',    'Available', NULL),
    (42, 14, 'CP00042', 'Kệ F-02', '2024-02-27', 150000, 'Good',    'Available', NULL),
    (43, 15, 'CP00043', 'Kệ G-02', '2025-01-05', 150000, 'Good',    'Available', NULL),
    (44, 15, 'CP00044', 'Kệ G-02', '2024-05-16', 120000, 'Good',    'Available', NULL),
    (45, 15, 'CP00045', 'Kệ G-02', '2022-03-20', 150000, 'Good',    'Available', NULL);

-- =============================================================
-- 6. LOANS (Gán Id cố định để khớp 100% với LoanDetails)
-- =============================================================
INSERT OR IGNORE INTO "Loans"
(
    "Id",
    "LoanCode",
    "IdReader",
    "BorrowedAt",
    "Note"
)
SELECT
    seed."Id",
    seed."LoanCode",
    u."Id",
    seed."BorrowedAt",
    seed."Note"
FROM
    (
        SELECT 1  AS "Id", 'LN0001' AS "LoanCode", 'RD0001' AS "UserCode", '2026-10-01 09:00:00' AS "BorrowedAt", 'Mượn thường' AS "Note"
        UNION ALL SELECT 2,  'LN0002', 'RD0002', '2026-10-03 10:15:00', 'Mượn thường'
        UNION ALL SELECT 3,  'LN0003', 'RD0003', '2026-10-05 14:00:00', 'Mượn thường'
        UNION ALL SELECT 4,  'LN0004', 'RD0004', '2026-10-07 08:30:00', 'Mượn thường'
        UNION ALL SELECT 5,  'LN0005', 'RD0005', '2026-10-09 16:45:00', 'Mượn thường'
        UNION ALL SELECT 6,  'LN0006', 'RD0006', '2026-10-11 11:00:00', 'Mượn thường'
        UNION ALL SELECT 7,  'LN0007', 'RD0007', '2026-10-13 13:20:00', 'Mượn thường'
        UNION ALL SELECT 8,  'LN0008', 'RD0008', '2026-10-15 09:40:00', 'Mượn thường'
        UNION ALL SELECT 9,  'LN0009', 'RD0009', '2026-10-17 15:10:00', 'Mượn thường'
        UNION ALL SELECT 10, 'LN0010', 'RD0010', '2026-10-19 10:00:00', 'Mượn thường'
    ) AS seed
        INNER JOIN "Users" u
                   ON u."UserCode" = seed."UserCode"
                       AND u."UserType" = 'Reader';

-- =============================================================
-- 7. LOAN DETAILS
-- =============================================================
INSERT OR IGNORE INTO "LoanDetails"
("Id", "IdLoan", "IdBookCopy", "DueAt", "ReturnedAt",
 "PhysicalConditionBefore", "PhysicalConditionAfter", "RenewalCount", "Note")
VALUES
    (1,  1,  1,  '2026-10-22', '2026-10-20', 'Good', 'Good', 0, NULL),
    (2,  1,  2,  '2026-10-22', NULL,         'Good', NULL,   0, 'Đang mượn'),
    (3,  2,  4,  '2026-10-17', '2026-10-16', 'Good', 'Good', 1, 'Đã gia hạn 1 lần'),
    (4,  3,  7,  '2026-10-19', NULL,         'Good', NULL,   0, 'Đang mượn'),
    (5,  3,  9,  '2026-10-19', NULL,         'Worn', NULL,   0, 'Đang mượn'),
    (6,  4,  10, '2026-10-21', '2026-10-18', 'Good', 'Worn', 0, 'Trả hơi cũ'),
    (7,  5,  13, '2026-10-23', NULL,         'Worn', NULL,   0, 'Đang mượn'),
    (8,  6,  16, '2026-10-25', '2026-10-24', 'Good', 'Good', 0, NULL),
    (9,  7,  19, '2026-10-27', NULL,         'Good', NULL,   0, 'Đang mượn'),
    (10, 8,  22, '2026-10-29', '2026-10-28', 'Good', 'Good', 0, NULL),
    (11, 9,  25, '2026-10-31', NULL,         'Good', NULL,   0, 'Đang mượn'),
    (12, 10, 28, '2026-11-02', NULL,         'Worn', NULL,   0, 'Đang mượn');

-- =============================================================
-- 8. RESERVATIONS
-- =============================================================
INSERT OR IGNORE INTO "Reservations"
("Id", "ReservationCode", "IdReader", "IdBook", "IdBookCopy", "IdLoan",
 "ReservedAt", "ExpiresAt", "AssignedAt", "FulfilledAt", "CancelledAt", "Status", "Note")
VALUES
    (1, 'RS0001', 11, 3,  8,  NULL, '2026-11-20 09:00:00', '2026-11-27 09:00:00',
     '2026-11-21 09:00:00', NULL, NULL, 'ReadyForPickup', 'Đã gán copy CP00008'),
    (2, 'RS0002', 12, 4,  12, NULL, '2026-11-21 10:00:00', '2026-11-28 10:00:00',
     '2026-11-22 10:00:00', NULL, NULL, 'ReadyForPickup', 'Đã gán copy CP00012'),
    (3, 'RS0003', 13, 1,  NULL, NULL, '2026-11-22 11:00:00', NULL,
     NULL, NULL, NULL, 'Pending', 'Chờ xử lý'),
    (4, 'RS0004', 14, 5,  NULL, NULL, '2026-11-16 12:00:00', NULL,
     NULL, NULL, '2026-11-18 12:00:00', 'Cancelled', 'Độc giả hủy'),
    (5, 'RS0005', 15, 2,  NULL, NULL, '2026-10-17 13:00:00', '2026-10-24 13:00:00',
     NULL, NULL, NULL, 'Expired', 'Hết hạn');

-- =============================================================
-- 9. FINES
-- =============================================================
INSERT OR IGNORE INTO "Fines"
("Id", "FineCode", "IdLoanDetail", "CreatedAt", "ViolationType", "AmountVnd", "Description", "Status")
VALUES
    (1, 'FN0001', 6,  '2026-10-18 09:00:00', 'Damaged', 50000, 'Bìa sách bị rách nhẹ', 'Unpaid'),
    (2, 'FN0002', 3,  '2026-10-16 10:00:00', 'Overdue', 20000, 'Trả trễ 2 ngày',       'Unpaid'),
    (3, 'FN0003', 10, '2026-10-28 11:00:00', 'Overdue', 10000, 'Trả trễ 1 ngày',       'Unpaid');

-- =============================================================
-- 10. FINE PAYMENTS
-- =============================================================
INSERT OR IGNORE INTO "FinePayments"
("Id", "PaymentCode", "IdFine", "PaidAt", "AmountVnd", "PaymentMethod", "Note")
VALUES
    (1, 'FP0001', 1, '2026-10-18 09:30:00', 50000, 'Cash', 'Đã thu đủ'),
    (2, 'FP0002', 2, '2026-10-16 10:30:00', 20000, 'Cash', 'Đã thu đủ');

-- =============================================================
-- 11. FINE AUDIT LOGS
-- =============================================================
INSERT OR IGNORE INTO "FineAuditLogs"
("Id", "IdFine", "Action", "BeforeAmount", "AfterAmount", "Reason", "At")
VALUES
    (1, 1, 'Created', 0,     50000, 'Tạo phạt do sách bị hư hỏng',  '2026-10-18 09:00:00'),
    (2, 1, 'Paid',    50000, 50000, 'Thanh toán đủ bằng tiền mặt',  '2026-10-18 09:30:00'),
    (3, 2, 'Created', 0,     20000, 'Tạo phạt do trả sách quá hạn', '2026-10-16 10:00:00'),
    (4, 2, 'Paid',    20000, 20000, 'Thanh toán đủ bằng tiền mặt',  '2026-10-16 10:30:00'),
    (5, 3, 'Created', 0,     10000, 'Tạo phạt do trả sách quá hạn', '2026-10-28 11:00:00');

-- =============================================================
-- 12. REMINDERS
-- =============================================================
INSERT OR IGNORE INTO "Reminders"
("Id", "IdLoanDetail", "Channel", "Status", "SentAt", "Content", "ErrorMessage", "CreatedAt", "UpdatedAt")
VALUES
    (1, 2, 'Email', 'Sent',    '2026-11-20 08:00:00', 'Nhắc trả sách LN0001', NULL,
     '2026-11-20 07:55:00', '2026-11-20 08:00:00'),
    (2, 4, 'SMS',   'Sent',    '2026-11-21 08:00:00', 'Nhắc trả sách LN0003', NULL,
     '2026-11-21 07:55:00', '2026-11-21 08:00:00'),
    (3, 7, 'Both',  'Failed',  NULL, 'Nhắc trả sách LN0005', 'Không gửi được tin nhắn',
     '2026-11-22 08:00:00', '2026-11-22 08:01:00'),
    (4, 9, 'Email', 'Pending', NULL, 'Nhắc trả sách LN0007', NULL,
     '2026-11-24 08:00:00', '2026-11-24 08:00:00');

-- =============================================================
-- 13. BOOK COPY HISTORIES
-- =============================================================
INSERT OR IGNORE INTO "BookCopyHistory"
("Id", "IdBookCopy", "ChangedAt", "ActionType", "OldStatus", "NewStatus",
 "OldCondition", "NewCondition", "OldLocation", "NewLocation", "Note")
VALUES
    (1, 2,  '2026-10-01 09:05:00', 'Borrow',  'Available', 'Borrowed',
     NULL, NULL, NULL, NULL, 'Mượn theo LN0001'),
    (2, 7,  '2026-10-05 14:05:00', 'Borrow',  'Available', 'Borrowed',
     NULL, NULL, NULL, NULL, 'Mượn theo LN0003'),
    (3, 9,  '2026-10-05 14:06:00', 'Borrow',  'Available', 'Borrowed',
     NULL, NULL, NULL, NULL, 'Mượn theo LN0003'),
    (4, 10, '2026-10-18 10:00:00', 'Return',  'Borrowed',  'Available',
     'Good', 'Worn', NULL, NULL, 'Trả theo LN0004'),
    (5, 8,  '2026-11-21 09:00:00', 'Reserve', 'Available', 'Reserved',
     NULL, NULL, NULL, NULL, 'Gán cho RS0001'),
    (6, 12, '2026-11-22 10:00:00', 'Reserve', 'Available', 'Reserved',
     NULL, NULL, NULL, NULL, 'Gán cho RS0002');

-- =============================================================
-- 14. FAVORITE BOOKS
-- ============================================================= 
INSERT OR IGNORE INTO "FavoriteBooks" ("IdReader", "IdBook", "AddedAt")
VALUES
    (1,  5, '2026-10-01 08:00:00'),
    (1,  7, '2026-10-02 09:00:00'),
    (3,  1, '2026-10-05 10:00:00'),
    (5,  9, '2026-10-10 11:00:00'),
    (10, 12, '2026-10-20 12:00:00');

-- =============================================================
-- END OF SEED
-- =============================================================