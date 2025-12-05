/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,jsx}",
  ],
  theme: {
    extend: {
      colors: {
        synBg: "#F7F5FF",        // page background
        synPurple: "#7C3AED",    // primary purple
        synPurpleSoft: "#EDE9FE" // soft purple accents
      },
      boxShadow: {
        soft: "0 10px 25px rgba(15, 23, 42, 0.08)",
      },
      borderRadius: {
        xl2: "1.25rem",
      },
    },
  },
  plugins: [],
};
