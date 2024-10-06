<script lang="ts" setup>
import { ref, type PropType } from "vue";
import ApexChart from "vue3-apexcharts";
import { responseModels } from "@/models";

const { correctData, wrongData } = defineProps({
    correctData: {
        type: Object as PropType<responseModels.DayActivity[]>,
        required: true,
    },
    wrongData: {
        type: Object as PropType<responseModels.DayActivity[]>,
        required: true,
    },
});

const correctDataDates = correctData.map(d => new Date(d.day));
const wrongDataDates = wrongData.map(d => new Date(d.day));

//  Append both correctData & wrongData dates in a set to remove doublons
//  Sort them from oldest to newest
//  Convert the dates to local date string
const combinedDates = [
    ...new Set(
        [...correctDataDates, ...wrongDataDates]
            .sort((a, b) => a.getTime() - b.getTime())
            .map(d =>
                d.toLocaleDateString(navigator.language || "en-GB", {
                    day: "2-digit",
                    month: "2-digit",
                }),
            ),
    ),
];

const series = ref([
    {
        name: "Correct answers",
        data: correctData.map(d => d.answerCount),
    },
    {
        name: "Wrong answers",
        data: wrongData.map(d => d.answerCount),
    },
]);

const options = ref({
    legend: {
        show: false,
    },
    stroke: {
        curve: "smooth",
    },
    colors: ["#1e6eff", "#F34C59"],
    chart: {
        toolbar: {
            show: false,
        },
        zoom: {
            enabled: false,
        },
    },
    tooltip: {
        x: {
            show: false,
        },
    },
    grid: {
        show: false,
    },
    dataLabels: {
        enabled: false,
    },
    xaxis: {
        categories: combinedDates,
        axisBorder: {
            height: 0,
        },
        labels: {
            style: {
                fontWeight: "bold",
                fontSize: 14,
            },
        },
    },
    yaxis: {
        labels: {
            style: {
                fontWeight: "bold",
                fontSize: 14,
            },
        },
    },
});
</script>

<template>
    <ApexChart height="250" type="area" :options="options" :series="series"></ApexChart>
</template>
