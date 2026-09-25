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
  background: linear-gradient(135deg, #f7f9fc 0%, #ffffff 100%);
  padding: 42px 58px;
}
section h1 {
  font-size: 42px;
  color: #123b6d;
  margin-bottom: 16px;
}
section h2 {
  font-size: 30px;
  color: #1769aa;
}
section h3 {
  color: #315b7d;
}
section strong {
  color: #0f5b8d;
}
section table {
  font-size: 20px;
}
section blockquote {
  border-left: 7px solid #0f7a8a;
  background: #eef8f9;
  padding: 14px 22px;
  color: #164653;
}
.hero {
  background: linear-gradient(135deg, #0d2f57 0%, #145a83 55%, #0b7a78 100%);
  color: white;
}
.hero h1, .hero h2, .hero h3, .hero strong {
  color: white;
}
.hero p, .hero li {
  color: #eef7ff;
}
.dark {
  background: linear-gradient(135deg, #172033 0%, #253a54 100%);
  color: white;
}
.dark h1, .dark h2, .dark h3, .dark strong {
  color: #ffffff;
}
.big {
  font-size: 30px;
  line-height: 1.45;
}
.kpi {
  display: flex;
  gap: 18px;
  margin-top: 24px;
}
.card {
  flex: 1;
  background: white;
  border-radius: 18px;
  padding: 18px 20px;
  box-shadow: 0 8px 24px rgba(20,40,70,.10);
}
.metric {
  font-size: 34px;
  font-weight: 700;
  color: #0c7b6f;
}
.small {
  font-size: 18px;
}
.center {
  text-align: center;
}
</style>

<!-- _class: hero -->

# FVN REGISTER
## Số hóa công việc — giảm chi phí vận hành

### Từ **giấy • bảng tính • ký • tìm • đối chiếu • nhắc việc**
### thành **một quy trình số hóa thống nhất**

<div class="big">

**Đăng ký → Phê duyệt → Thực tế → Đối soát → Xử lý → Báo cáo**

</div>

> **Không chỉ bỏ giấy. Quan trọng hơn là giảm công việc lặp lại phía sau tờ giấy.**

---

# 01 — Vấn đề thật sự không phải là tờ giấy

<div class="kpi">
<div class="card"><div class="metric">⌨</div><b>Nhập lại</b><br><span class="small">Một thông tin phải đi qua nhiều tệp, phiếu và bảng tổng hợp.</span></div>
<div class="card"><div class="metric">🔎</div><b>Tìm kiếm</b><br><span class="small">Mất thời gian tìm hồ sơ, trạng thái và lịch sử.</span></div>
<div class="card"><div class="metric">✓</div><b>Đối chiếu</b><br><span class="small">Nhân sự phải kiểm tra kế hoạch và thực tế thủ công.</span></div>
<div class="card"><div class="metric">⏰</div><b>Nhắc việc</b><br><span class="small">Phụ thuộc vào việc con người nhớ đúng lúc.</span></div>
</div>

### Chi phí ẩn

**Giờ công + giấy tờ + tìm kiếm + sửa sai + xử lý chậm + nguy cơ bỏ sót**

---

# 02 — QUY TRÌNH HIỆN TẠI

```mermaid
flowchart LR
    A[Nhân viên] --> B[Phiếu / bảng tính / thư điện tử]
    B --> C[Quản lý ký]
    C --> D[Nhân sự tiếp nhận]
    D --> E[Nhập lại / tổng hợp]
    E --> F[Kiểm tra chấm công]
    F --> G[Hỏi lại / sửa]
    G --> F
    F --> H[Chốt]
```

### Mỗi vòng lặp đều tiêu tốn thời gian

**Ghi → gửi → ký → nhập → kiểm tra → hỏi → sửa → kiểm tra lại**

> Khi số lượng nhân viên tăng, khối lượng kiểm tra tăng gần như theo số giao dịch.

---

# 03 — FVN REGISTER THAY ĐỔI ĐIỀU GÌ?

```mermaid
flowchart LR
    A[Đăng ký điện tử] --> B[Kiểm tra dữ liệu]
    B --> C[Phê duyệt]
    C --> D[Lưu vết]
    D --> E[Kế hoạch đã duyệt]
    H[Chấm công HRM] --> F[Thực tế]
    E --> G[Đối soát]
    F --> G
    G --> I{Có sai lệch?}
    I -->|Không| J[Hoàn tất]
    I -->|Có| K[Việc cần xử lý]
    K --> L[Thông báo]
    K --> M[Bằng chứng / xác nhận]
    M --> N[Nhân sự xử lý]
    N --> J
```

### Ý nghĩa kinh tế

**Hệ thống làm phần việc lặp lại → con người tập trung vào trường hợp ngoại lệ.**

---

# 04 — CHẤM CÔNG • PHÉP • OT • CÔNG TÁC

## Trước

```mermaid
flowchart TD
    A[Phiếu phép / OT / công tác] --> B[Ký]
    B --> C[Nhân sự lưu]
    C --> D[Nhập bảng tính]
    D --> E[Nhận bảng công]
    E --> F[Đối chiếu từng người / từng ngày]
    F --> G{Sai?}
    G -->|Có| H[Điện thoại / thư điện tử]
    H --> I[Sửa hồ sơ]
    I --> F
    G -->|Không| J[Chốt bảng công]
```

## Sau

```mermaid
flowchart TD
    A[Đăng ký điện tử] --> B[Kiểm tra tự động]
    B --> C[Phê duyệt]
    C --> D[Kế hoạch đã duyệt]
    H[Chấm công HRM] --> E[Thực tế]
    D --> F[Đối soát]
    E --> F
    F --> G{Sai lệch?}
    G -->|Không| H1[Hoàn tất]
    G -->|Có| I[Hiển thị dấu ?]
    I --> J[Việc cần xử lý]
    J --> K[Nhân sự xác nhận]
    K --> L[Hoàn tất]
```

### Một số lỗi hệ thống có thể phát hiện

- **OT thực tế nhưng chưa có đăng ký.**
- **OT đã duyệt nhưng không có OT thực tế.**
- **Nghỉ phép/công tác đã duyệt nhưng vẫn có chấm công.**
- **Chênh lệch giờ thực tế và giờ yêu cầu.**

> Không cần chờ đến cuối kỳ mới bắt đầu tìm lỗi.

---

# 05 — LỊCH LÀM VIỆC CỦA TÔI

## Từ nhiều nơi → một màn hình theo ngày

```mermaid
flowchart TB
    A[Ca làm việc] --> G[Lịch của tôi]
    B[Chấm công] --> G
    C[Phép] --> G
    D[OT] --> G
    E[Công tác] --> G
    F[Việc cần xử lý] --> G
```

### Người dùng nhìn thấy ngay

**Ca • Vào/Ra • Giờ thực tế • Giờ yêu cầu • Phép • OT • Công tác • Dấu ?**

### Giá trị

**Ít tìm kiếm hơn • ít đối chiếu hơn • phát hiện vấn đề sớm hơn**

---

# 06 — DẤU “?” KHÔNG PHẢI TRANG TRÍ

### Mỗi dấu “?” là một vấn đề có ngữ cảnh

```mermaid
flowchart LR
    A[?] --> B[Biết nguyên nhân]
    B --> C[Biết hồ sơ nguồn]
    C --> D[Biết người xử lý]
    D --> E[Biết việc cần làm]
```

### Ví dụ

🟡 **Chênh lệch giờ:** kiểm tra lại hoặc đăng ký OT.

🔴 **OT đã duyệt nhưng không có thực tế:** xác nhận / xử lý hủy theo quy trình.

🔴 **Đã duyệt nghỉ nhưng có chấm công:** gửi xử lý cho nhân sự.

> **Từ “có lỗi” → “biết phải làm gì”.**

---

# 07 — QUẢN LÝ THIẾT BỊ: TỪ SỔ GIẤY ĐẾN QR

## Trước

**Phiếu → Sổ → Tìm → Phiếu kiểm tra giấy → Ký → Lưu**

## Sau

```mermaid
flowchart LR
    A[Thiết bị] --> B[Mã QR]
    B --> C[Thông tin tài sản]
    C --> D[Lịch sử]
    C --> E[Phiếu kiểm tra]
    E --> F[Người phụ trách]
    E --> G[Hạn kiểm tra]
    E --> H[Bằng chứng]
    H --> I[Phê duyệt]
```

### Một lần quét QR

**→ đúng thiết bị  
→ đúng bộ phận  
→ đúng phiếu kiểm tra  
→ đúng lịch sử**

---

# 08 — PHIẾU KIỂM TRA KHÔNG CÒN LÀ “NHỚ THÌ LÀM”

```mermaid
flowchart LR
    A[Lịch định kỳ] --> B[Tự sinh nhiệm vụ]
    B --> C[Người phụ trách]
    C --> D[Hạn xử lý]
    D --> E[Nhắc việc]
    E --> F[Kiểm tra]
    F --> G[Ảnh / bằng chứng]
    G --> H[Phê duyệt]
    H --> I[Lịch sử]
```

### Hiện hệ thống đã có

- Phiếu kiểm tra theo ngày / tuần / tháng / quý / năm.
- Tự sinh nhiệm vụ kiểm tra theo lịch.
- Nhắc trước hạn.
- Việc cần xử lý trên hệ thống.
- Bằng chứng hình ảnh.
- Phiên bản phiếu kiểm tra.
- Lịch sử kiểm tra/sửa chữa.

---

# 09 — PHẦN MỀM CHỦ ĐỘNG LÀM VIỆC

<div class="kpi">
<div class="card"><div class="metric">5'</div><b>Đồng bộ HRM</b><br><span class="small">Theo chu kỳ hiện tại</span></div>
<div class="card"><div class="metric">5'</div><b>Hàng đợi thư điện tử</b><br><span class="small">Tách gửi thông báo khỏi nghiệp vụ</span></div>
<div class="card"><div class="metric">10'</div><b>Kiểm tra thiết bị</b><br><span class="small">Sinh nhiệm vụ và nhắc việc</span></div>
<div class="card"><div class="metric">30'</div><b>Đối soát</b><br><span class="small">Quét sai lệch gần nhất</span></div>
</div>

### Ngoài ra

**Tính công tự động hằng ngày • Nhắc phê duyệt • Chuyển cấp khi quá hạn**

> Mục tiêu là **không bắt nhân sự phải nhớ mọi việc**.

---

# 10 — TỪ “AI NHỚ THÌ LÀM” → “HỆ THỐNG NHẮC VIỆC”

## Cách cũ

```mermaid
flowchart LR
    A[Việc cần làm] --> B[Con người phải nhớ]
    B -->|Quên| C[Quá hạn]
    C --> D[Phát hiện muộn]
    D --> E[Khắc phục]
```

## Cách mới

```mermaid
flowchart LR
    A[Lịch / sự kiện] --> B[Việc cần xử lý]
    B --> C[Hạn xử lý]
    C --> D[Thông báo]
    D --> E[Người phụ trách]
    E --> F[Lưu lịch sử]
```

### Lợi ích

**Đúng người • đúng việc • đúng hạn • có truy vết**

---

# 11 — KIỂM SOÁT TRƯỚC KHI CHỐT BẢNG LƯƠNG

```mermaid
flowchart LR
    A[Chấm công] --> D[Đối soát]
    B[Phép / OT / công tác] --> D
    C[Điều chỉnh] --> D
    D --> E{Còn sai lệch?}
    E -->|Có| F[Xử lý tiếp]
    E -->|Không| G[Chụp dữ liệu kỳ]
    G --> H[Khóa / xuất]
```

### Điểm quan trọng

**Phát hiện trước khi chốt tốt hơn phát hiện sau khi đã chốt.**

Hệ thống có cơ chế kiểm tra sai lệch và điều chỉnh còn tồn trước khi khóa/xuất kỳ lương.

---

# 12 — BÁO CÁO KHÔNG CÒN PHỤ THUỘC VÀO NHIỀU tệp

## Trước

```mermaid
flowchart LR
    A[tệp phép] --> H[Nhân sự tổng hợp]
    B[tệp OT] --> H
    C[tệp công tác] --> H
    D[tệp thiết bị] --> H
    E[tệp chấm công] --> H
    H --> F[Báo cáo]
```

## Sau

```mermaid
flowchart LR
    A[Dữ liệu nghiệp vụ] --> B[Bảng điều hành]
    A --> C[Báo cáo]
    C --> D[Xuất dữ liệu theo quyền]
```

### Giá trị

**Một nguồn dữ liệu → nhiều góc nhìn quản trị**

---

# 13 — 4 NGUỒN TẠO RA HIỆU QUẢ NGÂN SÁCH

<div class="kpi">
<div class="card"><div class="metric">①</div><b>Giờ công</b><br><span class="small">Giảm nhập lại, tìm kiếm, tổng hợp, đối chiếu</span></div>
<div class="card"><div class="metric">②</div><b>Giấy tờ</b><br><span class="small">Giảm in, quét, lưu trữ, luân chuyển</span></div>
<div class="card"><div class="metric">③</div><b>Sai sót</b><br><span class="small">Giảm sửa lại và kiểm tra lặp</span></div>
<div class="card"><div class="metric">④</div><b>Rủi ro</b><br><span class="small">Giảm bỏ sót, quá hạn, thiếu lịch sử</span></div>
</div>

> Đây là **cơ chế tạo lợi ích**. Mức tiết kiệm thực tế phải đo bằng số liệu trước/sau triển khai.

---

# 14 — CÁCH TÍNH TIỀN

### 1. Giờ công giải phóng

**Giờ tiết kiệm = số giao dịch × phút tiết kiệm / 60**

### 2. Giá trị nhân công

**Giá trị = giờ tiết kiệm × chi phí nhân công quy đổi/giờ**

### 3. Lợi ích năm

**Lợi ích năm = nhân công + giấy tờ + giảm sửa sai + tránh tổn thất**

### 4. Hiệu quả đầu tư

**Hiệu quả đầu tư = lợi ích ròng / tổng mức đầu tư**

### 5. Thời gian hoàn vốn

**Thời gian hoàn vốn = vốn đầu tư ban đầu / lợi ích ròng mỗi tháng**

---

# 15 — VÍ DỤ MINH HỌA

> ⚠️ **Đây là số liệu minh họa, không phải số liệu thực tế của FCC Việt Nam.**

<div class="kpi">
<div class="card"><div class="metric">1.500</div><b>giao dịch/tháng</b></div>
<div class="card"><div class="metric">16'</div><b>tiết kiệm/giao dịch</b></div>
<div class="card"><div class="metric">120h</div><b>giảm tổng hợp báo cáo/tháng</b></div>
<div class="card"><div class="metric">75.000đ</div><b>chi phí/giờ</b></div>
</div>

### Mô hình tính

**≈ 520 giờ được giải phóng/tháng**

**≈ 39 triệu đồng/tháng**

**≈ 468 triệu đồng/năm**

> Con số chính thức phải được thay bằng **số liệu hiện trạng thực tế + kết quả thử nghiệm**.

---

# 16 — KHÔNG ĐỒNG NGHĨA “GIẢM NGƯỜI”

## 1 giờ công được giải phóng có thể dùng để:

- giảm làm thêm;
- giảm thuê ngoài;
- xử lý được nhiều hồ sơ hơn với cùng đội ngũ;
- chuyển nhân sự sang kiểm soát và phân tích;
- đáp ứng tăng trưởng mà chưa phải bổ sung nhân sự tương ứng.

### Vì vậy nên trình 3 lớp

**Tiết kiệm tiền mặt**

**Giải phóng năng lực**

**Giảm rủi ro**

---

# 17 — ĐO HIỆU QUẢ THỰC TẾ

```mermaid
flowchart LR
    A[Đo hiện trạng 2–4 tuần] --> B[Thử nghiệm 1–2 phòng ban]
    B --> C[Đo lại]
    C --> D[So sánh trước / sau]
    D --> E[Tính lợi ích]
    E --> F[Quyết định mở rộng]
```

### 8 chỉ số nên đo

| Chỉ số | Mục tiêu |
|---|---|
| Phút xử lý / hồ sơ | Giảm |
| Số lần nhập lại / hồ sơ | Giảm |
| Thời gian đối chiếu | Giảm |
| Thời gian lập báo cáo | Giảm |
| Tỷ lệ phiếu kiểm tra đúng hạn | Tăng |
| Tỷ lệ sửa lại | Giảm |
| Tỷ lệ thiếu lịch sử | Giảm |
| Số sai lệch phát hiện trước chốt lương | Tăng khả năng phát hiện sớm |

---

# 18 — TỪ PHẦN MỀM ĐĂNG KÝ → NỀN TẢNG VẬN HÀNH

```mermaid
flowchart LR
    A[Đăng ký] --> B[Phê duyệt]
    B --> C[Thực tế]
    C --> D[Đối soát]
    D --> E[Việc cần xử lý]
    E --> F[Nhân sự giải quyết]
    F --> G[Báo cáo]
    G --> H[Quản trị]
```

### Đây là giá trị khác biệt

**Không chỉ lưu thông tin.**

**Hệ thống theo dõi toàn bộ vòng đời công việc.**

---

<!-- _class: dark -->

# 19 — MỘT HỆ THỐNG, NHIỀU BÀI TOÁN

<div class="center big">

### 👤 Nhân viên
Đăng ký • Theo dõi • Nhận thông báo

### 👔 Người phê duyệt
Duyệt • Từ chối • Theo dõi quá hạn

### 🧑‍💼 Nhân sự
Đối soát • Xử lý sai lệch • Kiểm soát chốt lương

### 🖥 Quản trị
Phân quyền • Cấu hình • Theo dõi

### 🏢 Ban lãnh đạo
Số liệu • Chi phí • Rủi ro • Hiệu quả

</div>

---

<!-- _class: hero -->

# 20 — THÔNG ĐIỆP CUỐI CÙNG

<div class="big">

## Trước đây

**Con người nhớ việc → tìm hồ sơ → nhập lại → đối chiếu → nhắc → sửa**

## Với FVN REGISTER

**Hệ thống lưu → tự kiểm tra → tự phát hiện → tự nhắc → con người xử lý ngoại lệ**

</div>

### **FVN REGISTER**
## Số hóa công việc để giảm chi phí vận hành và tăng khả năng kiểm soát.

---

<!--
Gợi ý trình bày:
1. Nói về chi phí ẩn trước, không nói về công nghệ trước.
2. Cho xem một tình huống chấm công + OT + phép.
3. Chuyển sang lịch làm việc và dấu ?.
4. Cho xem thiết bị + QR + phiếu kiểm tra.
5. Kết thúc bằng công thức tiền và phương án thử nghiệm 1–2 phòng ban.
6. Không dùng số minh họa như kết quả đã đạt được.
-->
