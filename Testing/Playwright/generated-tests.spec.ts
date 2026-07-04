// import { test, expect, Page } from '@playwright/test';
// import fs from 'fs';
// import path from 'path';
// import xlsx from 'xlsx';

// type TestCaseRow = {
//   sheet: string;
//   tc: string;
//   title: string;
//   summary: string;
//   steps: string;
//   data: string;
//   expected: string;
//   postCondition: string;
// };

// const BASE_URL = process.env.BASE_URL || 'http://localhost:44163';
// const workbookPath = path.resolve(process.cwd(), 'SoccerStore_TestCases.xlsx');

// function normalizeText(value: unknown): string {
//   return String(value ?? '').trim();
// }

// function loadWorkbookCases(filePath: string): TestCaseRow[] {
//   if (!fs.existsSync(filePath)) {
//     throw new Error(`Workbook not found: ${filePath}`);
//   }

//   const wb = xlsx.readFile(filePath);
//   const allCases: TestCaseRow[] = [];

//   for (const sheetName of wb.SheetNames) {
//     const ws = wb.Sheets[sheetName];
//     const rows: unknown[][] = xlsx.utils.sheet_to_json(ws, { header: 1, defval: '' }) as unknown[][];

//     for (let i = 6; i < rows.length; i++) {
//       const row = rows[i] ?? [];
//       const tc = normalizeText(row[0]);
//       if (!tc.startsWith('TC')) continue;

//       allCases.push({
//         sheet: sheetName,
//         tc,
//         title: normalizeText(row[1]),
//         summary: normalizeText(row[2]),
//         steps: normalizeText(row[3]),
//         data: normalizeText(row[4]),
//         expected: normalizeText(row[5]),
//         postCondition: normalizeText(row[6]),
//       });
//     }
//   }

//   return allCases;
// }

// async function loginAsUser(page: Page) {
//   await page.goto(`${BASE_URL}/Auth/SignIn`);
//   await page.fill('input[name="txtName"]', 'Tuan@gmail.com');
//   await page.fill('input[name="txtPass"]', '123');
//   await page.click('form.form-signin input[type="submit"]');
// }

// async function loginAsAdmin(page: Page) {
//   await page.goto(`${BASE_URL}/Auth/SignIn`);
//   await page.fill('input[name="txtName"]', 'Admin@gmail.com');
//   await page.fill('input[name="txtPass"]', 'admin123');
//   await page.click('form.form-signin input[type="submit"]');
// }

// function routeBySheet(caseInfo: TestCaseRow): string {
//   return `${caseInfo.sheet} - ${caseInfo.tc} - ${caseInfo.title}`;
// }

// async function expectDanger(page: Page, message?: string) {
//   const alert = page.locator('.alert-danger');
//   await expect(alert).toBeVisible();
//   if (message) {
//     await expect(alert).toContainText(message);
//   }
// }

// async function executeMappedCase(page: Page, caseInfo: TestCaseRow) {
//   switch (caseInfo.sheet) {
//     case '01_DANGKITAIKHOAN': {
//       if (caseInfo.tc === 'TC01') {
//         const email = `playwright_${Date.now()}@mail.com`;
//         await page.goto(`${BASE_URL}/Auth/SignUp`);
//         await page.fill('input[name="txtEmail"]', email);
//         await page.fill('input[name="txtPhone"]', '0909123456');
//         await page.fill('input[name="txtPass"]', '123456');
//         await page.fill('input[name="txtRepass"]', '123456');
//         await page.click('form.form-signup input[type="submit"]');
//         await expect(page.locator('body')).toContainText(email);
//         return;
//       }

//       if (caseInfo.tc === 'TC02') {
//         await page.goto(`${BASE_URL}/Auth/SignUp`);
//         await page.fill('input[name="txtEmail"]', 'Tuan@gmail.com');
//         await page.fill('input[name="txtPhone"]', '0909123456');
//         await page.fill('input[name="txtPass"]', '123456');
//         await page.fill('input[name="txtRepass"]', '123456');
//         await page.click('form.form-signup input[type="submit"]');
//         await expectDanger(page, 'Email đã được sử dụng');
//         return;
//       }

//       if (caseInfo.tc === 'TC03') {
//         await page.goto(`${BASE_URL}/Auth/SignUp`);
//         await page.fill('input[name="txtEmail"]', `err_${Date.now()}@mail.com`);
//         await page.fill('input[name="txtPhone"]', '0909123456');
//         await page.fill('input[name="txtPass"]', '123456');
//         await page.fill('input[name="txtRepass"]', '1234567');
//         await page.click('form.form-signup input[type="submit"]');
//         await expectDanger(page);
//         return;
//       }

//       if (caseInfo.tc === 'TC08') {
//         await page.goto(`${BASE_URL}/Auth/SignUp`);
//         await page.fill('input[name="txtEmail"]', 'user_invalid_gmail.com');
//         await page.fill('input[name="txtPhone"]', '0909123456');
//         await page.fill('input[name="txtPass"]', '123456');
//         await page.fill('input[name="txtRepass"]', '123456');
//         await page.click('form.form-signup input[type="submit"]');
//         await expectDanger(page);
//         return;
//       }

//       throw new Error(`Unhandled test case: ${caseInfo.sheet} ${caseInfo.tc}`);
//     }

//     case '02_DANGNHAP': {
//       if (caseInfo.tc === 'TC01') {
//         await page.goto(`${BASE_URL}/Auth/SignIn`);
//         await page.fill('input[name="txtName"]', 'Tuan@gmail.com');
//         await page.fill('input[name="txtPass"]', '123');
//         await page.click('form.form-signin input[type="submit"]');
//         await expect(page).toHaveURL(/.*Home\/Index.*/);
//         return;
//       }

//       if (caseInfo.tc === 'TC02') {
//         await page.goto(`${BASE_URL}/Auth/SignIn`);
//         await page.fill('input[name="txtName"]', 'Admin@gmail.com');
//         await page.fill('input[name="txtPass"]', 'admin123');
//         await page.click('form.form-signin input[type="submit"]');
//         await expect(page).toHaveURL(/.*Home\/Dashboard.*/);
//         return;
//       }

//       if (caseInfo.tc === 'TC03') {
//         await page.goto(`${BASE_URL}/Auth/SignIn`);
//         await page.fill('input[name="txtName"]', 'Tuan@gmail.com');
//         await page.fill('input[name="txtPass"]', 'wrongpass');
//         await page.click('form.form-signin input[type="submit"]');
//         await expectDanger(page);
//         return;
//       }

//       throw new Error(`Unhandled test case: ${caseInfo.sheet} ${caseInfo.tc}`);
//     }

//     case '03_DANGXUAT': {
//       await page.goto(`${BASE_URL}/Auth/Logout`);
//       await expect(page).toHaveURL(/.*Auth\/SignIn.*/);
//       return;
//     }

//     case '04_TRANGCHU': {
//       await page.goto(`${BASE_URL}/Home/Index`);
//       await expect(page.locator('body')).toContainText('Sản phẩm nổi bật');
//       return;
//     }

//     case '05_DANHMUCSP': {
//       const routes: Record<string, string> = {
//         TC01: `${BASE_URL}/Home/Giay`,
//         TC02: `${BASE_URL}/Home/PhuKien`,
//         TC03: `${BASE_URL}/Home/LocSanPham?MAL=L002`,
//         TC04: `${BASE_URL}/Home/ThuongHieu?math=TH001`,
//       };
//       const route = routes[caseInfo.tc];
//       if (!route) throw new Error(`Unhandled test case: ${caseInfo.sheet} ${caseInfo.tc}`);
//       await page.goto(route);
//       await expect(page.locator('body')).toBeVisible();
//       return;
//     }

//     case '06_CHITIETSP': {
//       await page.goto(`${BASE_URL}/Home/ChiTietSanPham?masp=SP001`);
//       await expect(page.locator('body')).toContainText('Mã SP: SP001');
//       return;
//     }

//     case '07_TIMKIEMSP': {
//       await page.goto(`${BASE_URL}/Home/Index`);
//       await page.fill('input[name="Search"]', caseInfo.tc === 'TC01' ? 'Nike' : 'GiayKhongCo');
//       await page.click('button[type="submit"]');
//       if (caseInfo.tc === 'TC01') {
//         await expect(page).toHaveURL(/.*SearchProducts.*/);
//       } else {
//         await expect(page.locator('.alert-warning')).toBeVisible();
//       }
//       return;
//     }

//     case '08_FLASHSALE': {
//       await page.goto(`${BASE_URL}/Home/flashSale`);
//       await expect(page).toHaveURL(/.*flashSale.*/i);
//       return;
//     }

//     case '09_DICHVU_INAN': {
//       await page.goto(`${BASE_URL}/Home/InAnQuanAo`);
//       await expect(page.locator('body')).toContainText('In ấn');
//       return;
//     }

//     case '10_DICHVU_THANHLY': {
//       await page.goto(`${BASE_URL}/Home/ThanhLyVaKyGui`);
//       await expect(page.locator('body')).toContainText('Thanh lý');
//       return;
//     }

//     case '11_HUONGDAN_CHINHSACH': {
//       const policyRoutes = [
//         `${BASE_URL}/Home/HuongDanMuaHang`,
//         `${BASE_URL}/Home/HuongDanChonSize`,
//         `${BASE_URL}/Home/ChinhSachDoiTra`,
//         `${BASE_URL}/Home/ChinhSachBaoHanh`,
//         `${BASE_URL}/Home/ChinhSachBaoMat`,
//       ];
//       const index = Number(caseInfo.tc.replace('TC', '')) - 1;
//       const route = policyRoutes[index];
//       if (!route) throw new Error(`Unhandled test case: ${caseInfo.sheet} ${caseInfo.tc}`);
//       await page.goto(route);
//       await expect(page.locator('body')).toBeVisible();
//       return;
//     }

//     case '12_GIOHANG': {
//       await loginAsUser(page);
//       await page.goto(`${BASE_URL}/Home/ChiTietSanPham?masp=SP001`);
//       await page.fill('input[name="quantity"]', '2');
//       await page.click('button:has-text("Thêm vào giỏ hàng")');
//       await expect(page).toHaveURL(/.*Cart.*/);
//       return;
//     }

//     case '13_THANHTOAN': {
//       await loginAsUser(page);
//       await page.goto(`${BASE_URL}/Order/ThanhToan`);
//       await expect(page).toHaveURL(/.*Order\/ThanhToan.*/);
//       return;
//     }

//     case '14_DONHANG_SUCCESS': {
//       await page.goto(`${BASE_URL}/Order/DatHanhThanhCong`);
//       await expect(page.locator('body')).toBeVisible();
//       return;
//     }

//     case '15_ADMIN_DASHBOARD': {
//       await loginAsAdmin(page);
//       await page.goto(`${BASE_URL}/Home/Dashboard`);
//       await expect(page.locator('body')).toContainText('QUẢN LÝ SẢN PHẨM');
//       return;
//     }

//     case '16_ADMIN_SANPHAM': {
//       await loginAsAdmin(page);
//       await page.goto(`${BASE_URL}/Home/ThemSanPham`);
//       await expect(page.locator('body')).toBeVisible();
//       return;
//     }

//     case '17_ADMIN_HOADON': {
//       await loginAsAdmin(page);
//       await page.goto(`${BASE_URL}/Home/HoaDon`);
//       await expect(page.locator('body')).toBeVisible();
//       return;
//     }

//     case '18_ADMIN_NGUOIDUNG': {
//       await loginAsAdmin(page);
//       await page.goto(`${BASE_URL}/Home/NguoiDung`);
//       await expect(page.locator('body')).toBeVisible();
//       return;
//     }

//     case '19_BAOMAT_PHICHUCNANG': {
//       await page.goto(`${BASE_URL}/Home/Index`);
//       await expect(page.locator('body')).toBeVisible();
//       return;
//     }

//     default:
//       throw new Error(`Unhandled sheet: ${caseInfo.sheet}`);
//   }
// }

// const allCases = loadWorkbookCases(workbookPath);

// test.describe('SoccerStore generated cases from Excel', () => {
//   for (const caseInfo of allCases) {
//     test(routeBySheet(caseInfo), async ({ page }) => {
//       await test.step('Workbook metadata', async () => {
//         console.log({
//           sheet: caseInfo.sheet,
//           tc: caseInfo.tc,
//           title: caseInfo.title,
//           summary: caseInfo.summary,
//           steps: caseInfo.steps,
//           data: caseInfo.data,
//           expected: caseInfo.expected,
//           postCondition: caseInfo.postCondition,
//         });
//       });

//       await executeMappedCase(page, caseInfo);
//     });
//   }
// });