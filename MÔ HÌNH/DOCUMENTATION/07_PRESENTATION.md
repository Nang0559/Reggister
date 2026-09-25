---
marp: true
theme: default
paginate: true
size: 16:9
---

<style>
section {
  font-family: "Aptos", "Segoe UI", Arial, sans-serif;
  color: #172033;
  background: #f7f9fc;
  padding: 34px 46px;
}
section h1 { font-size: 39px; color: #123b6d; margin: 0 0 12px; }
section h2 { font-size: 27px; color: #1769aa; margin: 8px 0; }
section h3 { color: #315b7d; }
section table { width: 100%; font-size: 17px; border-collapse: separate; border-spacing: 8px; }
.flowrow { display:flex; align-items:center; justify-content:center; gap:7px; margin:10px 0 14px; }
.flowstep { min-width:105px; padding:10px 8px; border-radius:12px; text-align:center; font-size:16px; font-weight:700; line-height:1.15; box-shadow:0 4px 12px rgba(20,40,70,.12); }
.flowstep small { display:block; font-size:11px; font-weight:500; margin-top:4px; }
.manualStep { background:#fff1ee; border:2px solid #dc7668; color:#8d392f; }
.digitalStep { background:#eaf8f4; border:2px solid #37a38e; color:#086858; }
.flowArrow { font-size:23px; font-weight:800; color:#708090; }
.flowTitle { font-size:13px; font-weight:800; letter-spacing:.08em; margin:5px 0 2px; }
.flowPanel { background:#fff; border-radius:13px; padding:7px 10px; margin:7px 0; box-shadow:0 4px 14px rgba(20,40,70,.08); }
.flowPanel.before { border-left:7px solid #dc7668; }
.flowPanel.after { border-left:7px solid #37a38e; }
section th { background: #123b6d; color: white; padding: 10px; border-radius: 8px; }
section td { background: white; padding: 11px 13px; vertical-align: top; border-radius: 9px; }
section strong { color: #075f88; }
.hero { background: linear-gradient(135deg,#0b2f57,#126b83); color: white; }
.hero h1,.hero h2,.hero strong { color: white; }
.hero td { background: rgba(255,255,255,.95); color: #172033; }
.dark { background: #172033; color: white; }
.dark h1,.dark h2,.dark strong { color: white; }
.big { font-size: 27px; line-height: 1.35; }
.center { text-align:center; }
.kpi { display:flex; gap:14px; }
.card { flex:1; background:white; border-radius:14px; padding:15px; box-shadow:0 5px 18px rgba(20,40,70,.10); }
.metric { font-size:30px; font-weight:700; color:#087a6e; }
.flow { font-size:17px; line-height:1.55; text-align:center; }
.flow b { display:inline-block; padding:6px 9px; margin:2px; border-radius:7px; background:#eaf2f8; }
.before b { background:#fff0ed; color:#9b3d2f; }
.after b { background:#e8f7f3; color:#08705f; }
.arrow { color:#718096; font-weight:bold; }
.callout { background:#e9f6f4; border-left:6px solid #0b8274; padding:12px 16px; border-radius:8px; }
</style>

<!-- _class: hero -->

# FVN REGISTER
## Nhìn 5 giây: **trước → sau → thay đổi → hiệu quả**

| 🔴 TRƯỚC | 🟢 SAU | 🔄 THAY ĐỔI | 📈 HIỆU QUẢ |
|---|---|---|---|
| **Phiếu / Excel / email**<br>→ ký → nhập lại → tìm hồ sơ → đối chiếu | **Đăng ký số**<br>→ phê duyệt → đồng bộ → đối soát → xử lý | Một dữ liệu được dùng xuyên suốt thay vì nhập lại nhiều lần | **Ít thao tác lặp**<br>**Ít tìm kiếm**<br>**Phát hiện sớm** |
| **Con người phải nhớ việc** | **Hệ thống tạo việc + nhắc việc** | Từ nhớ thủ công → quản lý theo hạn | **Giảm bỏ sót / quá hạn** |
| **Cuối kỳ mới phát hiện sai** | **Sai lệch xuất hiện ngay trên lịch / việc cần xử lý** | Từ kiểm tra sau → xử lý ngoại lệ sớm | **Giảm sửa lại** |

<div class="big center">

**Từ “làm hồ sơ” → “quản lý toàn bộ vòng đời công việc”.**

</div>

---

# 01 — MỘT VIỆC, HAI CÁCH VẬN HÀNH

<div class="flowPanel before">
<div class="flowTitle">🔴 BEFORE</div>
<div class="flowrow">
<div class="flowstep manualStep">✍️<small>Ghi phiếu</small></div><div class="flowArrow">→</div>
<div class="flowstep manualStep">📄<small>Ký giấy</small></div><div class="flowArrow">→</div>
<div class="flowstep manualStep">✉️<small>Gửi email</small></div><div class="flowArrow">→</div>
<div class="flowstep manualStep">⌨️<small>Nhập Excel</small></div><div class="flowArrow">→</div>
<div class="flowstep manualStep">🔎<small>Đối chiếu</small></div>
</div>
</div>

<div class="flowPanel after">
<div class="flowTitle">🟢 AFTER</div>
<div class="flowrow">
<div class="flowstep digitalStep">💻<small>Đăng ký</small></div><div class="flowArrow">→</div>
<div class="flowstep digitalStep">🔐<small>Kiểm tra</small></div><div class="flowArrow">→</div>
<div class="flowstep digitalStep">✅<small>Phê duyệt</small></div><div class="flowArrow">→</div>
<div class="flowstep digitalStep">🔄<small>Thực tế</small></div><div class="flowArrow">→</div>
<div class="flowstep digitalStep">⚠️<small>Ngoại lệ</small></div>
</div>
</div>

| 🔄 THAY ĐỔI | 📈 HIỆU QUẢ |
|---|---|
| **Một hồ sơ – một luồng dữ liệu** | Giảm nhập lại và giảm tìm kiếm |
| Phần lặp lại do hệ thống xử lý | Con người tập trung vào quyết định / ngoại lệ |

**Nguyên tắc:** sơ đồ cho thấy **cách làm**, bảng phía dưới giải thích **vì sao hiệu quả**.

---

# 02 — MỘT QUY TRÌNH, HAI CÁCH VẬN HÀNH

| 🔴 TRƯỚC — nhiều vòng lặp | 🟢 SAU — một luồng có kiểm soát |
|---|---|
| <div class="flow before"><b>Nhân viên</b><span class="arrow"> → </span><b>Phiếu / Excel / email</b><br><span class="arrow">↓</span><br><b>Quản lý ký</b><span class="arrow"> → </span><b>Nhân sự nhận</b><br><span class="arrow">↓</span><br><b>Nhập / tổng hợp</b><span class="arrow"> → </span><b>Đối chiếu</b><br><span class="arrow">↺ hỏi lại / sửa / kiểm tra lại</span></div> | <div class="flow after"><b>Đăng ký số</b><span class="arrow"> → </span><b>Kiểm tra</b><span class="arrow"> → </span><b>Phê duyệt</b><br><span class="arrow">↓</span><br><b>Kế hoạch</b><span class="arrow"> + </span><b>Thực tế HRM</b><span class="arrow"> → </span><b>Đối soát</b><br><span class="arrow">↓</span><br><b>Hoàn tất</b> <span class="arrow">|</span> <b>Chỉ xử lý ngoại lệ</b></div> |

### Điểm thay đổi lớn
**Không cố gắng tự động hóa mọi quyết định — tự động hóa phần lặp lại để con người xử lý đúng chỗ.**

---

# 03 — CHẤM CÔNG • PHÉP • OT • CÔNG TÁC

| 🔴 TRƯỚC | 🟢 SAU | 🔄 THAY ĐỔI | 📈 HIỆU QUẢ |
|---|---|---|---|
| Phiếu → ký → lưu → nhập bảng → nhận bảng công → đối chiếu từng người | Đăng ký → duyệt → kế hoạch → HRM → đối soát | **Kế hoạch và thực tế được nối trực tiếp** | Giảm đối chiếu thủ công |
| Sai lệch phải gọi / email hỏi lại | Sai lệch xuất hiện trên lịch và danh sách xử lý | **Sai lệch trở thành một việc cụ thể** | Xử lý sớm hơn |
| Kiểm tra dồn vào cuối kỳ | Có thể phát hiện trong quá trình vận hành | **Từ kiểm tra muộn → kiểm tra liên tục** | Giảm dồn việc cuối kỳ |

<div class="flow after">
<b>Đăng ký</b> → <b>Phê duyệt</b> → <b>Kế hoạch</b> → <b>HRM thực tế</b> → <b>Đối soát</b> → <b>Hoàn tất / Xử lý ngoại lệ</b>
</div>

---

# 04 — CÁC SAI LỆCH ĐƯỢC NHÌN THẤY TRƯỚC KHI CHỐT

| Tình huống | Hệ thống làm gì | Việc còn lại của con người |
|---|---|---|
| **OT thực tế nhưng chưa đăng ký** | Đánh dấu sai lệch | Xác nhận / bổ sung hồ sơ |
| **OT đã duyệt nhưng không có thực tế** | Đưa vào xử lý | Xác nhận tình trạng |
| **Đã duyệt nghỉ nhưng vẫn có chấm công** | Hiển thị cảnh báo | Nhân sự kiểm tra |
| **Giờ thực tế khác giờ yêu cầu** | Đưa vào danh sách ngoại lệ | Xác nhận / điều chỉnh |

<div class="callout">

**Thay đổi quan trọng:** từ **“tìm lỗi trong dữ liệu”** → **“hệ thống chỉ ra trường hợp cần xử lý”.**

</div>

---

# 05 — LỊCH LÀM VIỆC: MỘT MÀN HÌNH THAY CHO NHIỀU NƠI TÌM KIẾM

| 🔴 TRƯỚC | 🟢 SAU | 🔄 THAY ĐỔI | 📈 HIỆU QUẢ |
|---|---|---|---|
| Tìm ca ở một nơi, chấm công ở nơi khác, phép/OT ở hồ sơ khác | **Một lịch theo ngày** | Gom dữ liệu theo **ngữ cảnh thời gian** | Ít chuyển màn hình |
| Muốn biết “hôm đó có gì?” phải tra nhiều nguồn | Một ngày hiển thị **ca + vào/ra + phép + OT + công tác + cảnh báo** | **Một ngày = một bức tranh hoàn chỉnh** | Nhìn nhanh hơn |
| Sai lệch nằm trong dữ liệu chi tiết | Dấu **?** ngay tại ngày | Đưa vấn đề đến đúng vị trí | Phát hiện sớm |

<div class="flow after">
<b>Ca</b> + <b>Chấm công</b> + <b>Phép</b> + <b>OT</b> + <b>Công tác</b> + <b>Sai lệch</b> → <b>Lịch của tôi</b>
</div>

---

# 06 — DẤU “?” = VIỆC CẦN XỬ LÝ, KHÔNG PHẢI BIỂU TƯỢNG TRANG TRÍ

| 🔴 CÁCH CŨ | 🟢 CÁCH MỚI |
|---|---|
| **Có vấn đề** → phải tự tìm nguyên nhân | **?** → xem nguyên nhân ngay |
| Tìm hồ sơ nguồn | Có liên kết tới **hồ sơ / dữ liệu nguồn** |
| Tự hỏi ai xử lý | Có **người / đơn vị phụ trách** |
| Tự nhớ phải làm gì | Có **việc cần xử lý + hạn** |

<div class="flow after">
<b>?</b> → <b>Nguyên nhân</b> → <b>Hồ sơ nguồn</b> → <b>Người xử lý</b> → <b>Việc cần làm</b> → <b>Hoàn tất</b>
</div>

---

# 07 — QUẢN LÝ THIẾT BỊ: TỪ SỔ THEO DÕI → QR + LỊCH SỬ

| 🔴 TRƯỚC | 🟢 SAU | 🔄 THAY ĐỔI | 📈 HIỆU QUẢ |
|---|---|---|---|
| Tìm thiết bị trong sổ / Excel | **Quét QR** | Nhận diện trực tiếp thiết bị | Giảm thời gian tìm |
| Tìm phiếu kiểm tra cũ | QR → thông tin → lịch sử | Hồ sơ gắn với thiết bị | Dễ truy vết |
| Nhớ lịch kiểm tra | Hệ thống sinh nhiệm vụ | Theo dõi theo lịch | Giảm bỏ sót |

<div class="flow after">
<b>QR</b> → <b>Thiết bị</b> → <b>Thông tin</b> → <b>Phiếu</b> → <b>Bằng chứng</b> → <b>Phê duyệt</b> → <b>Lịch sử</b>
</div>

---

# 08 — PHIẾU KIỂM TRA: TỪ “NHỚ THÌ LÀM” → “HỆ THỐNG GIAO VIỆC”

| 🔴 TRƯỚC | 🟢 SAU | 🔄 THAY ĐỔI | 📈 HIỆU QUẢ |
|---|---|---|---|
| Có lịch nhưng người phụ trách phải tự nhớ | Tự sinh nhiệm vụ theo lịch | **Lịch → nhiệm vụ** | Giảm bỏ sót |
| Không rõ việc nào quá hạn | Có hạn xử lý + nhắc việc | **Theo dõi trạng thái** | Giảm quá hạn |
| Ảnh / giấy tờ rời khỏi hồ sơ | Bằng chứng gắn với phiếu | **Một hồ sơ có đủ bằng chứng** | Dễ kiểm tra |
| Khó biết lịch sử | Lưu lịch sử kiểm tra / sửa chữa | **Có truy vết** | Tăng khả năng kiểm soát |

---

# 09 — PHẦN MỀM CHỦ ĐỘNG THỰC HIỆN CÔNG VIỆC LẶP LẠI

| 🔴 TRƯỚC | 🟢 SAU |
|---|---|
| Nhân sự phải nhớ thời điểm đồng bộ | **Đồng bộ HRM theo chu kỳ** |
| Người dùng phải nhớ gửi thông báo | **Hàng đợi gửi thông báo** |
| Phải kiểm tra thủ công nhiệm vụ thiết bị | **Tự sinh nhiệm vụ / nhắc việc** |
| Đợi cuối kỳ mới rà sai lệch | **Đối soát định kỳ** |
| Quá hạn mới đi tìm | **Nhắc trước / chuyển cấp theo quy trình** |

<div class="kpi">
<div class="card"><div class="metric">5'</div><b>Đồng bộ HRM</b></div>
<div class="card"><div class="metric">5'</div><b>Hàng đợi thông báo</b></div>
<div class="card"><div class="metric">10'</div><b>Kiểm tra thiết bị</b></div>
<div class="card"><div class="metric">30'</div><b>Đối soát</b></div>
</div>

<p class="small">Các chu kỳ trên là cấu hình vận hành hiện tại, không phải cam kết hiệu năng.</p>

---

# 10 — TỪ “AI NHỚ THÌ LÀM” → “HỆ THỐNG QUẢN LÝ HẠN”

| 🔴 CÁCH CŨ | 🟢 CÁCH MỚI |
|---|---|
| <div class="flow before"><b>Việc cần làm</b> → <b>Con người phải nhớ</b> → <b>Quên</b> → <b>Quá hạn</b> → <b>Phát hiện muộn</b></div> | <div class="flow after"><b>Lịch / sự kiện</b> → <b>Tạo việc</b> → <b>Hạn xử lý</b> → <b>Thông báo</b> → <b>Người phụ trách</b> → <b>Lưu lịch sử</b></div> |

### Giá trị
**Đúng người • đúng việc • đúng hạn • có truy vết**

---

# 11 — KIỂM SOÁT TRƯỚC KHI CHỐT BẢNG LƯƠNG

| 🔴 TRƯỚC | 🟢 SAU | 🔄 THAY ĐỔI | 📈 HIỆU QUẢ |
|---|---|---|---|
| Tổng hợp nhiều nguồn rồi mới phát hiện vấn đề | Chấm công + phép + OT + công tác được đối soát | **Kiểm tra trước khi khóa** | Giảm sửa sau chốt |
| Sai lệch nằm rải rác | Có danh sách ngoại lệ | **Tập trung việc cần xử lý** | Giảm thời gian rà |
| Điều chỉnh khó truy vết | Có dữ liệu và lịch sử xử lý | **Có dấu vết** | Tăng kiểm soát |

<div class="flow after">
<b>Chấm công</b> + <b>Phép / OT / công tác</b> + <b>Điều chỉnh</b> → <b>Đối soát</b> → <b>Xử lý ngoại lệ</b> → <b>Khóa / xuất kỳ</b>
</div>

---

# 12 — BÁO CÁO: TỪ NHIỀU TỆP → MỘT NGUỒN DỮ LIỆU

| 🔴 TRƯỚC | 🟢 SAU |
|---|---|
| <div class="flow before"><b>Tệp phép</b> + <b>Tệp OT</b> + <b>Tệp công tác</b> + <b>Tệp thiết bị</b> + <b>Tệp chấm công</b><br>↓<br><b>Nhân sự tổng hợp</b> → <b>Báo cáo</b></div> | <div class="flow after"><b>Dữ liệu nghiệp vụ</b><br>↓<br><b>Bảng điều hành</b> + <b>Báo cáo</b> + <b>Xuất dữ liệu theo quyền</b></div> |

### Thay đổi
**Từ “gom dữ liệu để làm báo cáo” → “báo cáo lấy từ dữ liệu đã quản lý”.**

---

# 13 — HIỆU QUẢ ĐẾN TỪ 4 NHÓM

| 🔴 CHI PHÍ / LÃNG PHÍ | 🟢 CƠ CHẾ GIẢM |
|---|---|
| **Giờ công** | Giảm nhập lại, tìm kiếm, tổng hợp, đối chiếu |
| **Giấy tờ** | Giảm in, ký, quét, lưu, luân chuyển |
| **Sai sót** | Kiểm tra tự động + phát hiện ngoại lệ |
| **Rủi ro** | Nhắc việc + hạn xử lý + lịch sử + truy vết |

<div class="callout">

**Đây là cơ chế tạo lợi ích. Không nên biến số liệu minh họa thành “kết quả đã đạt được”.**

</div>

---

# 14 — CÁCH CHUYỂN HIỆU QUẢ THÀNH TIỀN

| Bước | Cách tính |
|---|---|
| **01. Đo thời gian** | Số giao dịch × phút tiết kiệm / giao dịch |
| **02. Quy đổi giờ** | Tổng phút / 60 |
| **03. Quy đổi tiền** | Giờ tiết kiệm × chi phí nhân công quy đổi / giờ |
| **04. Cộng lợi ích khác** | Giấy tờ + giảm sửa sai + giảm tổn thất có thể đo |
| **05. Tính hiệu quả đầu tư** | Lợi ích ròng / tổng mức đầu tư |
| **06. Tính hoàn vốn** | Vốn đầu tư ban đầu / lợi ích ròng mỗi tháng |

### Nguyên tắc
**Đo trước → triển khai → đo sau → mới kết luận.**

---

# 15 — VÍ DỤ MINH HỌA: CÁCH TRÌNH BÀY, KHÔNG PHẢI KẾT QUẢ THỰC TẾ

> ⚠️ **Số liệu dưới đây chỉ để minh họa phương pháp tính.**

| Chỉ số | Ví dụ |
|---|---:|
| Giao dịch / tháng | 1.500 |
| Thời gian tiết kiệm / giao dịch | 16 phút |
| Giảm tổng hợp báo cáo | 120 giờ / tháng |
| Chi phí nhân công quy đổi | 75.000 đ / giờ |

**Mô hình minh họa:** khoảng **520 giờ/tháng** được giải phóng → khoảng **39 triệu đồng/tháng** → khoảng **468 triệu đồng/năm**.

> Số chính thức phải thay bằng **số liệu hiện trạng + kết quả đo sau triển khai**.

---

# 16 — “GIẢI PHÓNG NĂNG LỰC” KHÁC VỚI “GIẢM NGƯỜI”

| Giá trị | Có thể chuyển thành |
|---|---|
| Giờ nhập liệu giảm | Xử lý hồ sơ khác |
| Giờ tìm kiếm giảm | Kiểm soát / phân tích |
| Giờ đối chiếu giảm | Phòng ngừa sai lệch |
| Giờ làm báo cáo giảm | Phân tích dữ liệu |
| Ít việc lặp lại | Có thêm năng lực đáp ứng tăng trưởng |

<div class="callout">

**Mục tiêu của số hóa là giảm công việc lặp lại, không mặc định đồng nghĩa với giảm nhân sự.**

</div>

---

# 17 — ĐO HIỆU QUẢ THỰC TẾ

<div class="flow after">
<b>Đo hiện trạng 2–4 tuần</b> → <b>Thử nghiệm 1–2 phòng ban</b> → <b>Đo lại</b> → <b>So sánh trước / sau</b> → <b>Tính lợi ích</b> → <b>Quyết định mở rộng</b>
</div>

| Chỉ số | Cách nhìn |
|---|---|
| Phút xử lý / hồ sơ | ↓ |
| Số lần nhập lại / hồ sơ | ↓ |
| Thời gian đối chiếu | ↓ |
| Thời gian lập báo cáo | ↓ |
| Tỷ lệ phiếu kiểm tra đúng hạn | ↑ |
| Tỷ lệ sửa lại | ↓ |
| Tỷ lệ thiếu lịch sử | ↓ |
| Sai lệch phát hiện trước chốt | ↑ |

---

# 18 — TỪ PHẦN MỀM ĐĂNG KÝ → NỀN TẢNG VẬN HÀNH

| 🔴 CÁCH NHÌN CŨ | 🟢 CÁCH NHÌN MỚI |
|---|---|
| **Đăng ký** là điểm kết thúc của một chức năng | **Đăng ký** là điểm bắt đầu của một vòng đời |
| <div class="flow before"><b>Đăng ký</b> → <b>Phê duyệt</b> → <b>Lưu hồ sơ</b></div> | <div class="flow after"><b>Đăng ký</b> → <b>Phê duyệt</b> → <b>Thực tế</b> → <b>Đối soát</b> → <b>Việc cần xử lý</b> → <b>Giải quyết</b> → <b>Báo cáo</b> → <b>Quản trị</b></div> |

### Giá trị cốt lõi
**Hệ thống không chỉ lưu thông tin — hệ thống theo dõi vòng đời công việc.**

---

# 19 — MỘT HỆ THỐNG, NHIỀU GÓC NHÌN

| Người dùng | Giá trị nhận được |
|---|---|
| 👤 **Nhân viên** | Đăng ký • Theo dõi • Xem lịch • Nhận thông báo |
| 👔 **Người phê duyệt** | Duyệt • Từ chối • Theo dõi việc quá hạn |
| 🧑‍💼 **Nhân sự** | Đối soát • Xử lý sai lệch • Kiểm soát kỳ lương |
| 🖥 **Quản trị** | Phân quyền • Cấu hình • Theo dõi |
| 🏢 **Quản lý** | Số liệu • Chi phí • Rủi ro • Hiệu quả |

---

<!-- _class: dark -->

# 20 — KẾT LUẬN TRONG 5 GIÂY

| 🔴 TRƯỚC | 🟢 FVN REGISTER |
|---|---|
| **Con người nhớ việc** | **Hệ thống quản lý việc** |
| Tìm hồ sơ | Hồ sơ tập trung |
| Nhập lại | Dùng lại dữ liệu |
| Đối chiếu thủ công | Đối soát tự động |
| Phát hiện muộn | Phát hiện sớm |
| Tự nhắc nhau | Hệ thống nhắc việc |
| Sửa sau khi sai | Xử lý ngoại lệ theo quy trình |

<div class="big center">

## **Từ “quản lý hồ sơ” → “quản lý công việc”.**

### FVN REGISTER
**Số hóa để giảm thao tác lặp, tăng khả năng kiểm soát và đo được hiệu quả.**

</div>

---

<!--
NGUYÊN TẮC TRÌNH BÀY:
1. Slide 1 phải trả lời ngay: TRƯỚC LÀ GÌ? SAU LÀ GÌ? THAY ĐỔI GÌ? HIỆU QUẢ GÌ?
2. Các slide sau luôn ưu tiên bố cục 4 cột: Before | After | Thay đổi | Hiệu quả.
3. Không dùng số minh họa như thành tích thực tế.
4. Sơ đồ dùng HTML/CSS đơn giản để preview Marp ổn định, không phụ thuộc bộ render Mermaid.
5. Khi thuyết trình: nói “cơ chế tạo hiệu quả” trước, sau đó mới đưa số liệu đo thực tế.
-->
