<template>
    <div class="flex justify-center h-screen items-center rotate-180">
        <span class="loader before:bg-accent after:bg-accent" />
    </div>
</template>

<style scoped>
.loader {
    display: inline-block;
    position: relative;
}
.loader::after,
.loader::before {
    content: "";
    width: 70px;
    height: 70px;
    border-radius: 50%;
    position: absolute;
    left: 0;
    top: 0;
    box-sizing: border-box;
    animation: animloader 1.8s ease-in-out infinite;
}

@keyframes animloader {
    0%,
    100% {
        transform: scale(0);
        opacity: 1;
    }
    50% {
        transform: scale(1);
        opacity: 0;
    }
}
</style>
