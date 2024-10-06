<script lang="ts" setup>
import { type Ref, ref } from "vue";
import { requestModels, AviaskQuestionDetails } from "@/models";
import useUserStore from "@/stores/userStore";
import useEventBus from "@/eventBus";
import { useRoute, useRouter } from "vue-router";
import {
    ButtonComponent,
    TextInput,
    DeleteQuestionModal,
    TableComponent,
    PaginatorComponent,
    HeroIcon,
    CategorySelector,
    TitleComponent,
} from "@/components";
import { getQueryParams } from "@/helper";
import { z } from "zod";
import { AtplCategory, type FilteredResult } from "@/models/DTO/responseModels";
import useFilteringStore from "@/stores/filteringStore";
import useQuestionsRepository from "@/repositories/questionsRepository";
import {
    DropdownMenu,
    DropdownMenuTrigger,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuContent,
    DropdownMenuLabel,
} from "@/components/ui/dropdown-menu";
import SourceSelector from "@/components/Questions/SourceSelector.vue";

const questionsRepository = useQuestionsRepository();
const userStore = useUserStore();
const filteringStore = useFilteringStore();
const route = useRoute();
const { emitBus } = useEventBus();
const router = useRouter();

const paramsSchema = z.object({
    page: z.coerce.number().gte(1).catch(1),
    titleFilter: z.string().catch(""),
    categoryFilter: z.array(z.number()).default([]),
    sourceFilter: z.string().catch(""),
});

const showDeleteConfirmation: Ref<boolean> = ref<boolean>(false);
const questionToDelete: Ref<AviaskQuestionDetails | undefined> = ref(undefined);
const questionsCount: Ref<number> = ref(0);
const questionsResult: Ref<FilteredResult<AviaskQuestionDetails>> = ref<FilteredResult<AviaskQuestionDetails>>({
    elements: [],
    totalCount: 0,
});
const searchFilters: Ref<requestModels.QuestionsIndexParameters> = ref<requestModels.QuestionsIndexParameters>({
    page: 1,
    titleFilter: "",
    categoryFilter: [],
    sourceFilter: "",
});

//  Actual question search request
async function search(): Promise<FilteredResult<AviaskQuestionDetails>> {
    //  Apply search filters
    searchFilters.value.categoryFilter = filteringStore.categories.map(c => c.code);
    if (filteringStore.hasSourceFilter) searchFilters.value.sourceFilter = filteringStore.source!;

    const result = await questionsRepository.getAll(searchFilters.value);
    router.replace(`/questions?${getQueryParams(searchFilters.value as unknown as Record<string, unknown>)}`); // Updates URL with filters

    return result;
}

//  Shows confirmation modal
async function onDeleteRequested(questionDetails: AviaskQuestionDetails) {
    showDeleteConfirmation.value = true;
    questionToDelete.value = questionDetails;
}

//  Deletes a question
async function onDeleteConfirmed() {
    if (!questionToDelete.value) {
        return;
    }

    const res = await questionsRepository.delete(questionToDelete.value.id);

    //  Delete error
    if (!res) {
        emitBus("flashOpen", `Could not delete question ${questionToDelete.value.title}`, "danger");
        questionToDelete.value = undefined;
        return;
    }

    //  Delete success -> remove the question from our list
    questionsResult.value.elements.splice(questionsResult.value.elements.indexOf(questionToDelete.value), 1);
    questionsResult.value.totalCount--;
    questionsCount.value--;

    emitBus("flashOpen", `Successfully deleted question ${questionToDelete.value.title}`, "success");
    questionToDelete.value = undefined;
}

async function onSourceStoreChange() {
    let source = filteringStore.source;

    if (source === undefined || source === searchFilters.value.sourceFilter) source = "";

    searchFilters.value.page = 1;
    searchFilters.value.sourceFilter = source;

    questionsResult.value = await search();
}

async function onCategoryChange() {
    searchFilters.value.page = 1;
    questionsResult.value = await search();
}

async function onPageChange(newPage: number) {
    searchFilters.value.page = newPage;

    questionsResult.value = await search();
}

async function loadMoreQuestions() {
    searchFilters.value.page += 1;

    const result = await search();

    questionsResult.value.elements = questionsResult.value.elements.concat(result.elements);
}

if (userStore.isAuthenticated) questionsCount.value = await questionsRepository.count();

const searchParams = route.query;

searchFilters.value = paramsSchema.parse({
    page: Number(searchParams.page) || 1,
    titleFilter: searchParams.titleFilter,
    sourceFilter: searchParams.sourceFilter,
    categoryFilter:
        typeof searchParams.categoryFilter === "string"
            ? [+searchParams.categoryFilter]
            : Array.isArray(searchParams.categoryFilter)
              ? (searchParams.categoryFilter as string[])
                    .map(v => +v || 0)
                    .filter(c => AtplCategory.categoryCodeExist(c))
              : searchParams.categoryFilter || [],
});

//  Updates category filter if the URL contains some
if (searchFilters.value.categoryFilter.length) {
    filteringStore.categories = searchFilters.value.categoryFilter.map(
        c => AtplCategory.getCategory(c) || AtplCategory.getCategory(-1)!,
    );
}

//  Same for source filter
if (searchFilters.value.sourceFilter.length) {
    filteringStore.source = searchFilters.value.sourceFilter;
}

questionsResult.value = await search();
</script>

<template>
    <DeleteQuestionModal
        v-if="showDeleteConfirmation"
        v-model:show="showDeleteConfirmation"
        :question="questionToDelete!"
        @delete-confirmed="onDeleteConfirmed"></DeleteQuestionModal>

    <div class="flex flex-col gap-6">
        <div class="my-4">
            <span class="flex justify-between items-baseline mb-4">
                <TitleComponent class="mb-4">Question bank</TitleComponent>

                <ButtonComponent
                    v-if="userStore.userHasRole('manager')"
                    router-link="/questions/new"
                    class="flex gap-2 items-center"
                    state="accent">
                    <HeroIcon icon="PencilIcon" type="solid" />
                    <span class="hidden xs:inline">Create a question</span>
                </ButtonComponent>
                <ButtonComponent
                    v-else-if="userStore.isAuthenticated"
                    router-link="/suggestions/new"
                    state="accent"
                    class="flex gap-2 items-center">
                    <HeroIcon icon="PlusIcon" />
                    <span class="hidden xs:inline">Suggest a question</span>
                </ButtonComponent>
            </span>

            <p v-if="questionsCount > 0" class="mb-1 text-muted-dark dark:text-muted">
                The question bank currently contains
                <b>{{ questionsCount }}</b>
                questions
            </p>

            <p v-if="!userStore.isAuthenticated" class="mb-3 text-muted-dark dark:text-muted">
                As you are not
                <RouterLink to="/users/login" class="font-semibold">logged in</RouterLink>
                you can only see a limited set of questions.
            </p>
            <p v-else-if="!userStore.userDetails?.isPremium" class="mb-3 text-muted-dark font-medium dark:text-muted">
                As a non
                <RouterLink to="/payments/pricing" class="font-bold">Premium</RouterLink>
                user, you are restricted to today's free questions pool.
            </p>
        </div>

        <div class="flex flex-col gap-6">
            <div class="flex justify-center gap-2 items-center">
                <TextInput
                    v-model="searchFilters.titleFilter"
                    placeholder="Search a question title..."
                    :delay="450"
                    class="w-full sm:w-4/5"
                    @delayed-input="async () => (questionsResult = await search())"></TextInput>
            </div>

            <div class="flex justify-center items-center gap-4">
                <CategorySelector v-model="filteringStore.categories" @category-update="onCategoryChange" />

                <SourceSelector v-model="filteringStore.source" @source-update="onSourceStoreChange" />
            </div>
        </div>

        <TableComponent :data="questionsResult.elements" :headers="['Category', 'Title', 'Source', '']" class="sm:mt-4">
            <template #row="{ item }">
                <td class="font-medium md:font-semibold">
                    <span class="flex items-center gap-2">
                        <DropdownMenu v-if="userStore.userHasRole('manager')" :modal="false">
                            <DropdownMenuTrigger>
                                <HeroIcon
                                    icon="Cog6ToothIcon"
                                    type="solid"
                                    class="transition-transform active:rotate-90 mb-0.5 relative" />
                            </DropdownMenuTrigger>

                            <DropdownMenuContent class="min-w-32">
                                <DropdownMenuLabel>
                                    {{ item.title }}
                                </DropdownMenuLabel>

                                <DropdownMenuSeparator />

                                <DropdownMenuItem>
                                    <RouterLink
                                        :to="`/question/${item.id}/edit`"
                                        class="w-full flex items-center justify-between gap-4">
                                        <p>Edit</p>
                                        <HeroIcon icon="PencilIcon" size="sm" />
                                    </RouterLink>
                                </DropdownMenuItem>

                                <DropdownMenuItem>
                                    <button
                                        class="w-full flex items-center justify-between gap-4 font-semibold text-red-400"
                                        @click="onDeleteRequested(item)">
                                        <p>Delete</p>
                                        <HeroIcon icon="TrashIcon" size="sm" />
                                    </button>
                                </DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>

                        <span>[{{ item.category.code }}] {{ item.category.name }}</span>
                    </span>
                </td>

                <td>{{ item.title }}</td>
                <td>
                    <span class="hidden sm:table-cell">{{ item.source ?? "Unknown source" }}</span>
                </td>
                <td>
                    <div class="flex gap-2 justify-end">
                        <ButtonComponent class="flex" state="cta" :router-link="`/question/${item.id}`">
                            <HeroIcon icon="ArrowLongRightIcon" />
                        </ButtonComponent>
                    </div>
                </td>
            </template>
        </TableComponent>

        <div class="flex justify-around md:justify-between gap-4">
            <span class="hidden md:inline"></span>

            <ButtonComponent
                v-if="questionsResult.elements.length && questionsResult.elements.length % 25 == 0"
                state="cta"
                class="w-fit flex gap-2 items-center"
                @click="loadMoreQuestions">
                <HeroIcon icon="ArrowPathIcon" type="solid" />
                <span>Load more questions</span>
            </ButtonComponent>
            <span v-else></span>

            <PaginatorComponent
                :filtered="questionsResult"
                :page="searchFilters.page"
                @update:model-value="onPageChange" />
        </div>
    </div>
</template>
