<template>
    <main class="flex items-center justify-center flex-col h-[83vh]">
        <h1
            class="text-6xl md:text-7xl pointer-events-none select-none my-8 font-black tracking-widest dark:text-white">
            404
        </h1>

        <RouterLink to="/" class="text-2xl md:text-3xl text-shade-accent font-semibold my-4 inline-block">
            Get me back on track
        </RouterLink>
    </main>
</template>

<style scoped>
h1 {
    animation: 1.5s infinite pulse;
}

@keyframes pulse {
    0% {
        transform: scale(0.95);
        letter-spacing: -1px;
    }

    70% {
        transform: scale(1);
        letter-spacing: normal;
    }

    100% {
        transform: scale(0.95);
        letter-spacing: -1px;
    }
}
</style>
