/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "**/*.cshtml",
    "wwwroot/**/*.html"
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#f5f3ff',
          100: '#ede9fe',
          200: '#ddd6fe',
          300: '#c4b5fd',
          400: '#a78bfa',
          500: '#6366f1', // Indigo 500
          600: '#4f46e5', // Indigo 600
          700: '#4338ca', // Indigo 700
          800: '#3730a3',
          900: '#312e81',
        }
      }
    },
  },
  plugins: [],
}
