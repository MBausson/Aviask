<script lang="ts" setup>
import { ref } from "vue";
import { ButtonComponent, ModalComponent, HeroIcon } from "..";
import { AviaskQuestionReport } from "@/models/aviaskQuestionReport";

const { report } = defineProps({
    report: {
        type: AviaskQuestionReport,
        required: true,
    },
});

const showModal = ref<boolean>(false);
</script>

<template>
    <ModalComponent v-if="showModal" v-model:show="showModal">
        <template #title>
            <h3 class="text-2xl font-semibold leading-6">Question report</h3>
        </template>

        <template #description>
            <section class="flex flex-col gap-4">
                <div class="flex gap-2">
                    <label class="font-medium">Reason :</label>
                    <span class="font-bold">{{ report.category.name }}</span>
                </div>

                <div class="flex gap-2">
                    <label class="font-medium">Reported question :</label>
                    <RouterLink
                        :to="`/question/${report.questionId}`"
                        class="font-bold flex items-center gap-2"
                        target="_blank">
                        <span>{{ report.question.title }}</span>
                        <HeroIcon icon="ArrowTopRightOnSquareIcon" />
                    </RouterLink>
                </div>

                <div class="flex gap-2">
                    <label class="font-medium">Issuer :</label>
                    <RouterLink
                        :to="`/user/${report.issuerId}/profile`"
                        class="font-bold flex items-center gap-2"
                        target="_blank">
                        <span>{{ report.issuer.userName }}</span>
                        <HeroIcon icon="ArrowTopRightOnSquareIcon" />
                    </RouterLink>
                </div>

                <div class="flex gap-1 flex-col">
                    <label class="font-medium">Message :</label>
                    <p class="font-medium self-center break-all">
                        {{ report.message }}
                    </p>
                </div>
            </section>
        </template>

        <template #buttons>
            <ButtonComponent state="cta" @click="showModal = false">Cancel</ButtonComponent>
        </template>
    </ModalComponent>

    <ButtonComponent state="cta" class="flex" @click="showModal = true">
        <HeroIcon icon="ArrowTopRightOnSquareIcon" />
    </ButtonComponent>
</template>
