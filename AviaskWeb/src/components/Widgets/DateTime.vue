<script lang="ts" setup>
import { type PropType } from "vue";
import moment from "moment";

type DateTimeDisplay = "raw" | "since";

const { display, date } = defineProps({
    display: {
        type: String as PropType<DateTimeDisplay>,
        required: false,
        default: () => "raw",
    },
    date: {
        type: Date,
        required: false,
        default: new Date(),
    },
    showSince: {
        type: Boolean,
        required: false,
        default: () => false,
    },
    dateTimeFormat: {
        type: Object as PropType<Intl.DateTimeFormatOptions>,
        required: false,
        default: () => {
            return {
                day: "2-digit",
                month: "2-digit",
                year: "numeric",
            };
        },
    },
});
</script>

<template>
    <span v-if="display == 'raw'" :title="showSince ? moment(date).fromNow() : ''">
        {{ date.toLocaleDateString(undefined, dateTimeFormat) }}
    </span>
    <span v-else :title="date.toLocaleDateString(undefined, dateTimeFormat)">
        {{ moment(date).fromNow() }}
    </span>
</template>
