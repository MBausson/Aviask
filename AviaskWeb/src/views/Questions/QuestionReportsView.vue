<script lang="ts" setup>
import {
    ShowQuestionReport,
    ButtonComponent,
    HeroIcon,
    TableComponent,
    BadgeComponent,
    PaginatorComponent,
    TitleComponent,
} from "@/components";
import { QuestionReportState, AviaskQuestionReport } from "@/models/aviaskQuestionReport";
import { ref } from "vue";
import useEventBus from "@/eventBus";
import { useRoute } from "vue-router";
import useQuestionReportsRepository from "@/repositories/questionReportsRepository";
import {
    DropdownMenu,
    DropdownMenuTrigger,
    DropdownMenuItem,
    DropdownMenuContent,
    DropdownMenuSeparator,
    DropdownMenuLabel,
} from "@/components/ui/dropdown-menu";

const questionReportsRepositroy = useQuestionReportsRepository();
const route = useRoute();
const { emitBus } = useEventBus();

const page = ref(Number(route.query.page || 1));
const reports = ref(await questionReportsRepositroy.getAll(page.value));

function copyReportId(id: string) {
    navigator.clipboard.writeText(id);
    emitBus("flashOpen", `Copied ID of this report`, "success");
}

async function onPageChange(newPage: number) {
    page.value = newPage;

    reports.value = await questionReportsRepositroy.getAll(page.value);
}

async function onStateUpdated(report: AviaskQuestionReport, state: QuestionReportState) {
    if (report.state == state) return;

    const res = await questionReportsRepositroy.edit(report.id, state);

    if (!res) {
        emitBus("flashOpen", "Could not update the report's state", "danger");
        return;
    }

    report.state = state;
    emitBus("flashOpen", "Successfully updated report state", "success");
}

async function onDelete(report: AviaskQuestionReport) {
    const res = await questionReportsRepositroy.delete(report.id);

    if (!res) {
        emitBus("flashOpen", "Could not delete this question report", "danger");
        return;
    }

    reports.value.elements = reports.value.elements.splice(reports.value.elements.indexOf(report), 1);
    emitBus("flashOpen", "Successfully deleted the question report", "success");
}
</script>

<template>
    <section class="flex justify-between gap-4">
        <TitleComponent class="my-6">Reports</TitleComponent>

        <div class="flex items-center gap-3">
            <PaginatorComponent :filtered="reports" :page="page" @update:model-value="onPageChange" />
        </div>
    </section>

    <TableComponent
        :headers="['State', 'Reason', 'Question', 'Issuer', '']"
        :data="reports.elements"
        class="w-full h-[80vh]">
        <template #row="{ item }">
            <td>
                <DropdownMenu v-if="item.state == QuestionReportState.Pending" :modal="false">
                    <DropdownMenuTrigger>
                        <button
                            class="flex items-center gap-4 font-medium border px-4 py-2 rounded-md bg-black/5 hover:bg-black/10 transition-all dark:border-neutral-600 dark:bg-white/5 dark:hover:bg-white/10">
                            <HeroIcon
                                v-if="item.state == QuestionReportState.Pending"
                                icon="EllipsisHorizontalCircleIcon"
                                class="text-orange-400" />
                            <HeroIcon
                                v-else-if="item.state == QuestionReportState.Declined"
                                icon="XMarkIcon"
                                class="text-red-400" />
                            <HeroIcon v-else icon="CheckIcon" class="text-emerald-500" />

                            <span>
                                {{ QuestionReportState[item.state] }}
                            </span>
                        </button>
                    </DropdownMenuTrigger>

                    <DropdownMenuContent class="min-w-52">
                        <DropdownMenuLabel>Change status</DropdownMenuLabel>

                        <DropdownMenuItem
                            v-for="(state, i) in Object.keys(QuestionReportState).filter(k => isNaN(Number(k)))"
                            :key="state">
                            <button
                                class="w-full flex gap-2 items-center justify-between"
                                @click="onStateUpdated(item, i)">
                                <span>{{ state }}</span>

                                <HeroIcon
                                    v-if="i == QuestionReportState.Pending"
                                    icon="EllipsisHorizontalCircleIcon"
                                    class="text-orange-400" />
                                <HeroIcon
                                    v-else-if="i == QuestionReportState.Declined"
                                    icon="XMarkIcon"
                                    class="text-red-400" />
                                <HeroIcon v-else icon="CheckIcon" class="text-emerald-500" />
                            </button>
                        </DropdownMenuItem>

                        <DropdownMenuSeparator />

                        <DropdownMenuItem>
                            <button
                                class="w-full flex gap-2 items-center justify-between font-medium text-red-400"
                                @click="onDelete(item)">
                                <span>Delete</span>
                                <HeroIcon icon="TrashIcon" />
                            </button>
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>

                <BadgeComponent v-else :badge-type="item.state == QuestionReportState.Treated ? 'green' : 'red'">
                    {{ item.state == QuestionReportState.Treated ? "Treated" : "Declined" }}
                </BadgeComponent>
            </td>

            <td class="font-semibold">
                {{ item.category.name }}
            </td>

            <td>
                <RouterLink :to="`/question/${item.questionId}`" class="font-medium">
                    {{ item.question.title }}
                </RouterLink>
            </td>

            <td>
                <RouterLink :to="`/user/${item.issuerId}/profile`" class="font-medium">
                    {{ item.issuer.userName }}
                </RouterLink>
            </td>

            <td class="flex gap-2 justify-end">
                <ButtonComponent state="flat" class="flex items-center gap-2 text-sm" @click="copyReportId(item.id)">
                    <HeroIcon icon="ClipboardDocumentIcon" />
                </ButtonComponent>

                <ShowQuestionReport :report="item" />
            </td>
        </template>
    </TableComponent>
</template>
