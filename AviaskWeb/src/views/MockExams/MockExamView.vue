<script lang="ts" setup>
import { TitleComponent, MockExamForm, MockExam, ButtonComponent } from "@/components";
import HeroIcon from "@/components/Widgets/HeroIcon.vue";
import useMockExamsRepository from "@/repositories/mockExamsRepository";
import { requestModels } from "@/models";
import useEventBus from "@/eventBus";
import useMockExamStore from "@/stores/mockExamStore";

const eventBus = useEventBus();
const mockExamStore = useMockExamStore();
const mockExamsRepository = useMockExamsRepository();

async function onCreate(model: requestModels.CreateMockExamModel) {
    const res = await mockExamsRepository.create(model);

    if (!res.success || !res.result) {
        eventBus.emitBus("flashOpen", "Could not create a mock exam", "danger");
        return;
    }

    mockExamStore.current = res.result;
}

await mockExamStore.updateCurrent();
</script>
<template>
    <div class="flex flex-col gap-2 md:gap-6">
        <div class="flex justify-between gap-4 flex-col md:flex-row">
            <section class="flex gap-2 flex-col">
                <TitleComponent>Practice exams</TitleComponent>

                <span class="text-lg text-muted-dark dark:text-muted">
                    Test your knowledge in a practice exam, bringing you closer to real conditions
                </span>
            </section>

            <ButtonComponent
                v-if="mockExamStore.current?.questionId"
                class="h-fit flex gap-2 items-center justify-center"
                state="accent"
                :router-link="`/mockExam/question/${mockExamStore.current.questionId}`">
                <span>Resume my exam</span>
                <HeroIcon icon="ForwardIcon" size="lg" />
            </ButtonComponent>
        </div>

        <ButtonComponent
            state="cta"
            router-link="/mockExams"
            class="mx-auto mt-4 sm:mx-0 sm:mt-2 w-fit flex gap-2 items-center">
            <HeroIcon icon="ArrowTopRightOnSquareIcon" />
            <span>Practice exams history</span>
        </ButtonComponent>

        <MockExamForm v-if="!mockExamStore.current" class="mt-10 w-full" @form-sent="onCreate" />
        <MockExam v-else v-model="mockExamStore.current" class="mt-10 w-full" />
    </div>
</template>
