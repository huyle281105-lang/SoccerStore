## 🧪 Test Coverage — Module 11–14

Phần này mô tả 4 module test tiếp theo trong bộ test case của dự án **SoccerStore**, được thiết kế thủ công (Excel) và tự động hóa song song bằng **Playwright + TypeScript**.

---

### 11. Hướng dẫn & Chính sách (`11_HUONGDAN_CHINHSACH`)

Kiểm tra các trang nội dung tĩnh: Hướng dẫn mua hàng, Hướng dẫn chọn size, Chính sách đổi trả, Chính sách bảo hành, Chính sách bảo mật.

- **10 test case**, tất cả **PASS**.
- Phạm vi kiểm thử: hiển thị nội dung đúng, liên kết không lỗi 404, hình ảnh bảng size không vỡ, font chữ/định dạng hiển thị tốt với tiếng Việt, tốc độ tải trang (< 1s vì là nội dung tĩnh), và khả năng truy cập ở cả hai trạng thái đăng nhập/chưa đăng nhập (trang công khai).
- **Automation**: 10 test tương ứng trong `11 — Hướng dẫn & Chính sách`, dùng vòng lặp (`for...of`) để test đồng loạt các route chính sách thay vì lặp code, cộng thêm kiểm tra hiệu năng tải trang bằng đo thời gian thực (`Date.now()`).

---

### 12. Giỏ hàng (`12_GIOHANG`)

Kiểm tra toàn bộ nghiệp vụ giỏ hàng: thêm, cập nhật, xóa sản phẩm, tính tiền, và các trường hợp biên.

- **15 test case**, tất cả **PASS**.
- Điểm nổi bật: phát hiện và ghi nhận 2 **hành vi cần lưu ý** dù không phải lỗi chặn (đã note lại trong cột *Notes* để dev cân nhắc cải thiện):
  - Số lượng = 0 hoặc âm chỉ bị chặn ở **frontend** (thuộc tính `min="1"`), backend chưa có validation nếu bỏ qua frontend.
  - Sản phẩm hết hàng (tồn kho = 0) vẫn có thể thêm vào giỏ — hệ thống chỉ chặn ở bước đặt hàng, chưa ẩn nút mua trên trang chi tiết.
- Cũng kiểm tra logic cộng dồn số lượng khi thêm trùng sản phẩm, tính đúng thành tiền/tổng tiền, và độ tin cậy khi F5 lại trang.
- **Automation**: 15 test tương ứng trong `12 — Giỏ hàng`, có các helper dùng chung (`addProductToCart`, `ensureCartHasItem`, `clearCart`) để tái sử dụng logic setup giỏ hàng giữa các test case.

---

### 13. Thanh toán đơn hàng (`13_THANHTOAN`)

Module quan trọng nhất về mặt nghiệp vụ — kiểm tra luồng đặt hàng COD, validation form, và các rule tồn kho/ngày giao hàng.

- **15 test case** — **13 PASS, 2 FAIL** (2 bug thật sự được phát hiện):
  - **FAIL — TC08**: Số điện thoại sai định dạng (chứa chữ/ký tự đặc biệt) vẫn được hệ thống chấp nhận và tạo hóa đơn thành công → thiếu validation định dạng SĐT ở backend.
  - **FAIL — TC09**: Ngày hẹn giao ở quá khứ vẫn được lưu vào hóa đơn thành công → thiếu validation logic nghiệp vụ (ngày hẹn giao phải ≥ ngày hiện tại).
- Các case PASS khác xác nhận: chặn thanh toán khi giỏ hàng trống, chặn khi vượt tồn kho (kiểm tra ở backend, không phải chỉ frontend), validate các trường bắt buộc qua HTML5, cơ chế mặc định ngày hẹn giao (+3 ngày nếu để trống), chống double-click tạo trùng hóa đơn, và giỏ hàng được dọn sạch sau khi đặt hàng thành công.
- **Automation**: 15 test tương ứng trong `13 — Thanh toán đơn hàng`. Với 2 bug đã biết, script không throw lỗi cứng làm crash pipeline mà dùng `console.warn` để ghi nhận hành vi thực tế — giúp CI/CD chạy ổn định trong khi vẫn lưu vết bug cho việc theo dõi.

---

### 14. Đặt hàng thành công (`14_DONHANG_SUCCESS`)

Kiểm tra trang xác nhận đơn hàng sau khi thanh toán — nơi phát hiện ra lỗi bảo mật đáng chú ý nhất của dự án.

- **6 test case** — **5 PASS, 1 FAIL (lỗi bảo mật nghiêm trọng)**:
  - **FAIL — TC02**: Copy link `/Order/DatHanhThanhCong?id=HD001` và mở ở trình duyệt/tab ẩn danh khác (chưa đăng nhập) vẫn xem được toàn bộ thông tin đơn hàng của người khác. Đây là lỗi **IDOR (Insecure Direct Object Reference)** — hệ thống không kiểm tra chủ sở hữu hóa đơn khớp với người dùng đang đăng nhập trước khi hiển thị dữ liệu.
- Các case PASS khác: hiển thị đầy đủ thông tin hóa đơn, nút quay lại trang chủ hoạt động đúng, mã hóa đơn tự tăng đúng thứ tự, redirect về trang chủ khi ID hóa đơn không tồn tại, và tồn kho sản phẩm bị trừ chính xác sau khi đặt hàng.
- **Automation**: 6 test tương ứng trong `14 — Đặt hàng thành công`, dùng `test.beforeAll` để tạo sẵn 1 hóa đơn thật (đăng nhập → thêm giỏ → đặt hàng) và trích mã hóa đơn từ URL, sau đó tái sử dụng cho các test case còn lại — tránh lặp lại luồng đặt hàng ở từng test.

---

