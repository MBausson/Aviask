<script setup lang="ts">
import { RouterView } from "vue-router";
import { SpinnerComponent, FlashComponent } from "@/components";
import { AppHeader, AppFooter } from "@/components/Layout";
</script>

<template>
    <Suspense timeout="0">
        <template #fallback>
            <main class="dark:bg-dark-primary h-full">
                <SpinnerComponent />
            </main>
        </template>

        <template #default>
            <div>
                <KeepAlive>
                    <AppHeader class="relative" />
                </KeepAlive>

                <div class="dark:bg-dark-primary">
                    <main class="min-h-screen max-w-[1200px] mx-auto p-5 relative">
                        <RouterView :key="$route.path" />
                    </main>
                </div>

                <AppFooter class="pt-8" />
            </div>
        </template>
    </Suspense>

    <FlashComponent />
</template>

<style>
@font-face {
    font-family: "Geist";
    src: url("assets/fonts/Geist/Geist.ttf");
}
</style>
