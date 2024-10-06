<script lang="ts" async setup>
import useEventBus from "@/eventBus";
import { responseModels } from "@/models";
import useUserStore from "@/stores/userStore";
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
    ButtonComponent,
    DateTime,
    DeleteAnswerRecordModal,
    TableComponent,
    BadgeComponent,
    HeroIcon,
    PaginatorComponent,
    TitleComponent,
} from "@/components";
import { AviaskAnswerRecordDetails } from "@/models/aviaskAnswerRecordDetails";
import useAnswerRecordsRepository from "@/repositories/answerRecordsRepository";
import useUsersRepository from "@/repositories/usersRepository";
import type { FilteredResult } from "@/models/DTO/responseModels";

const userStore = useUserStore();
const userRepository = useUsersRepository();
const answerRecordsRepository = useAnswerRecordsRepository();
const router = useRouter();
const route = useRoute();
const { emitBus } = useEventBus();

const showDeleteConfirmation = ref(false);
const answerRecordToDelete = ref<AviaskAnswerRecordDetails | undefined>(undefined);
const userId = route.params.id as string;

const user = ref<responseModels.UserProfile | null>();
const answerRecords = ref<FilteredResult<AviaskAnswerRecordDetails>>({ elements: [], totalCount: 0 });
const fetchDone = ref(false);
const page = ref(1);

onMounted(async () => {
    if (!userId) return router.push("/404");

    const profileResult = await userRepository.profile(userId);

    user.value = profileResult.result;

    if (!user.value) {
        router.push("/");
        return;
    }

    answerRecords.value = await answerRecordsRepository.getAll(userId, page.value);
    fetchDone.value = true;
});

function onDeleteRequested(answerRecordDetails: AviaskAnswerRecordDetails) {
    showDeleteConfirmation.value = true;
    answerRecordToDelete.value = answerRecordDetails;
}

async function onDeleteConfirmed() {
    const res = await answerRecordsRepository.delete(answerRecordToDelete.value!.id);

    if (!res) {
        emitBus("flashOpen", `Could not delete answer record '${answerRecordToDelete.value?.id}'`, "danger");
        answerRecordToDelete.value = undefined;
        return;
    }

    answerRecords.value = await answerRecordsRepository.getAll(userId);

    emitBus("flashOpen", `Successfully deleted answer record '${answerRecordToDelete.value?.question.title}'`);
    answerRecordToDelete.value = undefined;
}

async function onPageChange(newPage: number) {
    page.value = newPage;

    answerRecords.value = await answerRecordsRepository.getAll(userId, page.value);
}
</script>

<template>
    <DeleteAnswerRecordModal
        v-if="answerRecordToDelete"
        v-model:show="showDeleteConfirmation"
        :answer-record="answerRecordToDelete"
        @delete-confirmed="onDeleteConfirmed"></DeleteAnswerRecordModal>

    <div v-if="answerRecords.elements.length && fetchDone" class="flex flex-col gap-6">
        <span class="my-4 flex flex-col gap-2">
            <section class="flex gap-2 justify-between items-center">
                <TitleComponent>Answer history</TitleComponent>

                <div class="flex items-center gap-3">
                    <PaginatorComponent :filtered="answerRecords" :page @update:model-value="onPageChange" />
                </div>
            </section>

            <span class="first-letter:capitalize text-muted-dark dark:text-muted text-lg font-medium">
                <RouterLink :to="`/user/${user?.userDetails.id}/profile`" class="font-bold flex gap-1">
                    <HeroIcon icon="UserCircleIcon" />
                    <span>{{ user?.userDetails.userName }}</span>
                </RouterLink>
            </span>
        </span>

        <TableComponent
            :headers="['Date', 'Category', 'Question title', 'Your answer', '']"
            :data="answerRecords.elements">
            <template #row="{ item }">
                <td>
                    <DateTime display="since" :date="new Date(item.answeredAt)" class="hidden md:inline" />
                </td>

                <td>
                    <RouterLink :to="`/questions?categoryFilter=${item.question.category.code}`">
                        [ {{ item.question.category.code }} ] {{ item.question.category.name }}
                    </RouterLink>
                </td>

                <td>
                    <RouterLink :to="`/question/${item.question.id}`">
                        {{ item.question.title }}
                    </RouterLink>
                </td>

                <td class="font-medium">
                    <BadgeComponent :badge-type="item.isCorrect ? 'green' : 'red'">
                        {{ item.answered.slice(0, 24) }}
                    </BadgeComponent>
                </td>

                <td>
                    <div class="flex gap-2 justify-end">
                        <ButtonComponent
                            v-if="userStore.userHasRole('admin')"
                            button-type="important"
                            state="danger"
                            @click="onDeleteRequested(item)">
                            Delete
                        </ButtonComponent>

                        <ButtonComponent class="group grid" state="cta" :router-link="`/question/${item.question.id}`">
                            <HeroIcon icon="ArrowTopRightOnSquareIcon" />
                        </ButtonComponent>
                    </div>
                </td>
            </template>
        </TableComponent>
    </div>

    <div v-else-if="fetchDone" class="flex justify-center">
        <span class="text-2xl font-semibold my-4 dark:text-muted">You haven't answered any question yet !</span>
    </div>
</template>
