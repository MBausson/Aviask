import defaultTheme from "tailwindcss/defaultTheme";

const animate = require("tailwindcss-animate");

/** @type {import('tailwindcss').Config} **/
module.exports = {
    darkMode: ["class"],
    content: [
        "./pages/**/*.{ts,tsx,vue}",
        "./components/**/*.{ts,tsx,vue}",
        "./app/**/*.{ts,tsx,vue}",
        "./src/**/*.{ts,tsx,vue}",
    ],
    prefix: "",
    theme: {
        container: {
            center: true,
            padding: "2rem",
            screens: {
                "2xl": "1400px",
            },
        },
        screens: {
            xs: "475px",
            ...defaultTheme.screens,
        },
        extend: {
            keyframes: {
                "accordion-down": {
                    from: { height: 0 },
                    to: { height: "var(--radix-accordion-content-height)" },
                },
                "accordion-up": {
                    from: { height: "var(--radix-accordion-content-height)" },
                    to: { height: 0 },
                },
            },
            animation: {
                "accordion-down": "accordion-down 0.2s ease-out",
                "accordion-up": "accordion-up 0.2s ease-out",
            },
            colors: {
                accent: "#1c71ed",
                complementary: "#ed991c",
                "dark-primary": "#1b1b1b",
                "dark-secondary": "#181818",
                "dark-top-gradient": "#404040",
                "dark-bottom-gradient": "#282828",
                muted: "#e5e5e5",
                "muted-dark": "#404040",
                "olive-light": "#faf0e2",
                "olive-dark": "#D4D975",
                "lavendar-light": "#e5e1f9",
                "lavendar-dark": "#e1bee7",
                "skyblue-light": "#c9eef7",
                "skyblue-dark": "#67A7D6",
                "coral-light": "#F08080",
                "coral-dark": "#CD5B45",
            },
        },
    },
    plugins: [animate],
};

