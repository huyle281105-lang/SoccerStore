import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './Playwright',

  timeout: 60 * 1000,

  expect: {
    timeout: 10000,
  },

  fullyParallel: true,

  forbidOnly: !!process.env.CI,

  retries: process.env.CI ? 2 : 0,

  workers: 4,

  reporter: [
    ['list'],
    ['html', { open: 'never' }],
  ],

  use: {
    actionTimeout: 15000,

    baseURL: 'http://localhost:44163',

    trace: 'retain-on-failure',

    screenshot: 'only-on-failure',

    video: 'retain-on-failure',

    headless: true,
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },

    // Tạm thời comment để debug trước
    // {
    //   name: 'firefox',
    //   use: { ...devices['Desktop Firefox'] },
    // },

    // {
    //   name: 'webkit',
    //   use: { ...devices['Desktop Safari'] },
    // },
  ],
});