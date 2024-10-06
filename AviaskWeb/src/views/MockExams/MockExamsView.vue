<script lang="ts" setup>
import { ButtonComponent, PaginatorComponent, TitleComponent } from "@/components";
import type { AviaskMockExam } from "@/models/aviaskMockExam";
import useMockExamsRepository from "@/repositories/mockExamsRepository";
import { type FilteredResult } from "@/models/DTO/responseModels";
import { ref } from "vue";
import MockExamResult from "@/components/MockExams/MockExamResult.vue";
import HeroIcon from "@/components/Widgets/HeroIcon.vue";

const mockExamsRepository = useMockExamsRepository();

const maxItems = 15;

const page = ref(1);
const mockExams = ref<FilteredResult<AviaskMockExam>>(await mockExamsRepository.getAll(page.value));
const expandedIndex = ref(-1);

function onDetailsExpand(index: number) {
    expandedIndex.value = expandedIndex.value === index ? -1 : index;
}

async function onPageChange(newPage: number) {
    page.value = newPage;

    mockExams.value = await mockExamsRepository.getAll(page.value);
}
</script>
<template>
    <div class="flex flex-col gap-6">
        <div class="my-4 flex gap-2 justify-between items-center">
            <TitleComponent>Practice exams history</TitleComponent>

            <div class="flex items-center gap-3">
                <PaginatorComponent :filtered="mockExams" :max-items :page @update:model-value="onPageChange" />
            </div>
        </div>

        <div v-if="mockExams.totalCount > 0" class="flex flex-col gap-3 px-1 md:px-4 my-6">
            <MockExamResult
                v-for="(mockExam, index) in mockExams.elements"
                :key="mockExam.id"
                :mock-exam="mockExam"
                :show-details="index === expandedIndex"
                @on-details-expand="onDetailsExpand(index)" />
        </div>
        <div v-else class="flex flex-col gap-4 mx-auto mt-6 items-center">
            <h3 class="text-xl font-medium">You haven't completed any practice exam yet</h3>

            <ButtonComponent state="cta" class="w-fit my-4 sm:my-8 flex items-center gap-2" :router-link="`/mockExam`">
                <span>Start a practice exam</span>
                <HeroIcon icon="ArrowTopRightOnSquareIcon" />
            </ButtonComponent>
        </div>
    </div>
</template>
