<script lang="ts" setup>
import ApexChart from "vue3-apexcharts";
import { ref, type PropType } from "vue";
import { AtplCategory, type CategoryStatistics } from "@/models/DTO/responseModels";

const { data } = defineProps({
    data: {
        required: true,
        type: Object as PropType<CategoryStatistics[]>,
    },
});

const options = ref({
    chart: {
        type: "polarArea",
    },
    stroke: {
        show: false,
    },
    fill: {
        colors: [
            "#CC63A4",
            "#36A2EB",
            "#FFCE56",
            "#4BC0C0",
            "#BBAAFF",
            "#FF9F40",
            "#23CE6B",
            "#FF6384",
            "#36A2EB",
            "#FFCE56",
            "#4BC0C0",
            "#9966FF",
            "#FF9F40",
            "#23CE6B",
        ],
        opacity: 1,
    },
    legend: {
        show: false,
    },
    plotOptions: {
        polarArea: {
            rings: {
                strokeWidth: 0,
            },
            spokes: {
                strokeWidth: 0,
            },
        },
    },
    yaxis: {
        show: false,
    },
    labels: data.map(e => {
        const category = AtplCategory.getCategory(e.category)!;

        return `${category.name} [${category.code}]`;
    }),
    tooltip: {
        y: {
            formatter: (value: number): string => `${value.toFixed(2)}%`,
        },
    },
});

const series = ref(data.map(e => e.correctnessRatio * 100));
</script>
<template>
    <ApexChart type="polarArea" :options="options" :series="series"></ApexChart>
</template>
