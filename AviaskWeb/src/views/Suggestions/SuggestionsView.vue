<script lang="ts" setup>
import {
    HeroIcon,
    ButtonComponent,
    TableComponent,
    BadgeComponent,
    PaginatorComponent,
    TitleComponent,
} from "@/components";
import { onMounted, ref, watch } from "vue";
import useEventBus from "@/eventBus";
import { useRoute } from "vue-router";
import type { AviaskQuestionDetails, responseModels } from "@/models";
import { QuestionStatus } from "@/models/DTO/responseModels";
import useSuggestionsRepository from "@/repositories/suggestionsRepository";
import useUsersRepository from "@/repositories/usersRepository";
import {
    DropdownMenu,
    DropdownMenuTrigger,
    DropdownMenuItem,
    DropdownMenuContent,
    DropdownMenuLabel,
} from "@/components/ui/dropdown-menu";

const { emitBus } = useEventBus();
const userRepository = useUsersRepository();
const suggestionsRepository = useSuggestionsRepository();
const route = useRoute();

const page = ref(Number(route.query.page || 1));
const suggestions = ref(await suggestionsRepository.getAll(page.value));
const loadedPublishers = ref<Record<string, responseModels.UserProfile>>({});

async function onPageChange(newPage: number) {
    page.value = newPage;

    suggestions.value = await suggestionsRepository.getAll(page.value);
}

async function onUpdateStatus(suggestion: AviaskQuestionDetails, status: QuestionStatus) {
    //  Avoid updating the same status
    if (suggestion.status == status) return;

    //  We shouldn't update non-pending questions
    if (suggestion.status.name != "Pending") {
        emitBus("flashOpen", "This question status can no longer be changed", "danger");
        return;
    }

    const res = await suggestionsRepository.updateStatus(suggestion.id, status.value);

    //  Error
    if (!res) {
        emitBus("flashOpen", "Could not update this suggestion's status", "danger");
        return;
    }

    //  Success
    suggestion.status = status;
    emitBus("flashOpen", "Updated succesffully this suggestion's status", "success");
}

async function loadPublisher(suggestion: AviaskQuestionDetails) {
    if (suggestion.publisherId in loadedPublishers.value) return;

    const res = await userRepository.profile(suggestion.publisherId);
    if (!res.success || !res.result) return;

    loadedPublishers.value[suggestion.publisherId] = res.result;
}

onMounted(() => {
    suggestions.value.elements.forEach(async item => {
        await loadPublisher(item);
    });

    watch(suggestions, async newSuggestions => {
        for (const item of newSuggestions.elements) {
            await loadPublisher(item);
        }
    });
});
</script>

<template>
    <section class="flex justify-between items-center gap-4">
        <TitleComponent class="my-6">Question suggestions</TitleComponent>

        <div class="flex items-center gap-3">
            <PaginatorComponent :filtered="suggestions" :page @update:model-value="onPageChange" />
        </div>
    </section>

    <TableComponent
        :headers="['Status', 'Category', 'Title', 'Publisher', '']"
        :data="suggestions.elements"
        class="w-full h-[80vh]">
        <template #row="{ item }">
            <td>
                <div class="flex items-center">
                    <DropdownMenu v-if="item.status.name === 'Pending'" :modal="false">
                        <DropdownMenuTrigger>
                            <button
                                class="flex items-center gap-4 font-medium border px-4 py-2 rounded-md bg-black/5 hover:bg-black/10 transition-all dark:border-neutral-600 dark:bg-white/5 dark:hover:bg-white/10">
                                <HeroIcon
                                    v-if="item.status.name == 'Pending'"
                                    icon="EllipsisHorizontalCircleIcon"
                                    class="text-orange-400" />
                                <HeroIcon
                                    v-else-if="item.status.name == 'Accepted'"
                                    icon="CheckIcon"
                                    class="text-emerald-400" />
                                <HeroIcon v-else icon="XMarkIcon" class="text-red-500" />

                                <span class="font-bold">{{ item.status.name }}</span>
                            </button>
                        </DropdownMenuTrigger>

                        <DropdownMenuContent class="min-w-52">
                            <DropdownMenuLabel>Change status</DropdownMenuLabel>

                            <DropdownMenuItem v-for="status in QuestionStatus.status" :key="status.value">
                                <button
                                    class="w-full flex items-center gap-4 justify-between"
                                    @click="onUpdateStatus(item, status)">
                                    <span>{{ status.name }}</span>

                                    <HeroIcon
                                        v-if="status.name == 'Pending'"
                                        icon="EllipsisHorizontalCircleIcon"
                                        class="text-orange-400" />
                                    <HeroIcon
                                        v-else-if="status.name == 'Accepted'"
                                        icon="CheckIcon"
                                        class="text-emerald-400" />
                                    <HeroIcon v-else icon="XMarkIcon" class="text-red-500" />
                                </button>
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>

                    <BadgeComponent v-else :badge-type="item.status.name == 'Accepted' ? 'green' : 'red'">
                        {{ item.status.name }}
                    </BadgeComponent>
                </div>
            </td>

            <td class="font-semibold">
                <RouterLink :to="`/questions?categoryFilter=${item.category.code}`">
                    {{ item.category.name }} [{{ item.category.code }}]
                </RouterLink>
            </td>

            <td>{{ item.title }}</td>

            <td class="font-semibold">
                <RouterLink v-if="item.publisherId in loadedPublishers" :to="`/user/${item.publisherId}/profile`">
                    {{ loadedPublishers[item.publisherId].userDetails.userName }}
                </RouterLink>
                <p v-else class="flex gap-2 items-center">
                    <HeroIcon icon="ArrowPathIcon" class="animate-spin" />
                </p>
            </td>

            <td>
                <div class="flex flex-row-reverse">
                    <ButtonComponent
                        v-if="item.status.name == 'Pending'"
                        state="cta"
                        class="flex items-center"
                        :router-link="`/suggestion/${item.id}`">
                        <HeroIcon icon="ArrowTopRightOnSquareIcon" />
                    </ButtonComponent>

                    <ButtonComponent
                        v-else-if="item.status.name == 'Accepted'"
                        state="cta"
                        class="flex items-center"
                        :router-link="`/question/${item.id}`">
                        <HeroIcon icon="ArrowLongRightIcon" />
                    </ButtonComponent>

                    <ButtonComponent v-else state="flat" class="flex items-center" disabled>
                        <span>Declined.</span>
                    </ButtonComponent>
                </div>
            </td>
        </template>
    </TableComponent>
</template>
