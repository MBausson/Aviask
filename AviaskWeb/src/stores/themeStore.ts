import { defineStore } from "pinia";
import { computed, ref } from "vue";

type Theme = "light" | "dark";

const useThemeStore = defineStore("themeStore", () => {
    const theme = ref<Theme>("light");

    const isDark = computed<boolean>(() => theme.value === "dark");

    function updateTheme(newTheme: Theme) {
        //  1 - Update Store
        theme.value = newTheme;

        //  2 - Update localstorage theme attribute
        localStorage.theme = newTheme;

        //  3 - Update HTML dark class
        const html = document.documentElement;

        if (newTheme == "dark") {
            html.classList.add("dark");
            return;
        }

        html.classList.remove("dark");
    }

    //  Updates the current theme if user changes his system theme.
    function onSystemThemeChange(e: MediaQueryListEvent) {
        updateTheme(e.matches ? "dark" : "light");
    }

    (() => {
        let themeFound: Theme = "light";
        const colorSchemeMedia = window.matchMedia("(prefers-color-scheme: dark)");

        if (localStorage.theme) {
            themeFound = (localStorage.theme as Theme) || "light";
        } else {
            themeFound = colorSchemeMedia.matches ? "dark" : "light";
        }

        colorSchemeMedia.addEventListener("change", onSystemThemeChange);
        updateTheme(themeFound);
    })();

    return { theme, isDark, updateTheme };
});

export default useThemeStore;
