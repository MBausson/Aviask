<script lang="ts" setup>
import { onMounted, onUnmounted, ref } from "vue";

const { startDate } = defineProps({
    startDate: {
        type: Date,
        required: true,
    },
});

const timeString = ref(formatElapsedTime(startDate));
let interval: NodeJS.Timeout | undefined;

function formatElapsedTime(date: Date): string {
    const elapsedTimeInSeconds = Math.floor((Date.now() - date.getTime()) / 1000);
    const hours = Math.floor(elapsedTimeInSeconds / 3600);
    const minutes = Math.floor((elapsedTimeInSeconds % 3600) / 60);
    const seconds = elapsedTimeInSeconds % 60;

    const formattedHours = hours < 10 ? `0${hours}` : hours.toString();
    const formattedMinutes = minutes < 10 ? `0${minutes}` : minutes.toString();
    const formattedSeconds = seconds < 10 ? `0${seconds}` : seconds.toString();

    return `${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
}

onMounted(() => {
    interval = setInterval(() => {
        timeString.value = formatElapsedTime(startDate);
    }, 1000);
});

onUnmounted(() => {
    clearTimeout(interval);
});
</script>
<template>{{ timeString }}</template>
