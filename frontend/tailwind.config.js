/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        background: '#111215',
        card: '#1a1c24',
        border: '#2a2c35',
        primary: '#10b981',
        danger: '#ef4444',
      },
    },
  },
  plugins: [],
};
