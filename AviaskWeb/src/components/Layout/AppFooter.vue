<script lang="ts" setup>
import { ThemeSwitch } from "..";
</script>

<template>
    <div class="dark:bg-dark-primary">
        <div
            class="border-t-2 border-neutral-300 dark:border-neutral-700 mx-6 sm:mx-8 md:mx-24 py-5 flex justify-between flex-col lg:flex-row gap-y-4 items-center dark:text-white">
            <div class="flex items-center gap-5 lg:justify-start">
                <RouterLink to="/" class="flex gap-3 items-center">
                    <img class="h-9" src="@/assets/icon.svg" alt="Aviask logo" />
                    <span class="text-lg font-medium">
                        <span>Aviask</span>
                        <sup>&copy;</sup>
                    </span>

                    <span class="mr-4 md:mr-8">Learn together.</span>
                </RouterLink>
            </div>

            <div class="links flex gap-4 items-center text-sm sm:text-base">
                <ThemeSwitch />

                <span class="text-neutral-600 dark:text-neutral-400 pointer-events-none select-none">|</span>

                <RouterLink to="/payments/pricing">Pricing</RouterLink>
                <RouterLink to="/aboutus">About us</RouterLink>
                <RouterLink to="/tos">Terms of Service</RouterLink>
                <RouterLink to="/privacy">Privacy policy</RouterLink>
            </div>
        </div>
    </div>
</template>
