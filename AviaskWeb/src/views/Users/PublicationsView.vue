<script lang="ts" setup>
import {
    BadgeComponent,
    ButtonComponent,
    DateTime,
    HeroIcon,
    TableComponent,
    PaginatorComponent,
    TitleComponent,
} from "@/components";
import useUsersRepository from "@/repositories/usersRepository";
import useUserStore from "@/stores/userStore";
import { ref } from "vue";

const userStore = useUserStore();
const userRepository = useUsersRepository();

const page = ref(1);
const publications = ref(await userRepository.publications(page.value));

async function onPageChange(newPage: number) {
    page.value = newPage;

    publications.value = await userRepository.publications(page.value);
}
</script>
<template>
    <section class="flex justify-between gap-4 items-baseline mb-4">
        <TitleComponent class="my-6">Your publications</TitleComponent>

        <div class="flex items-center gap-3">
            <PaginatorComponent :filtered="publications" :page @update:model-value="onPageChange" />
        </div>
    </section>

    <TableComponent
        :headers="['Published at', 'Status', 'Category', 'Title']"
        :data="publications.elements"
        class="w-full h-[80vh]">
        <template #row="{ item }">
            <td>
                <DateTime :date="item.publishedAt" class="hidden md:inline" />
            </td>
            <td>
                <BadgeComponent
                    :badge-type="
                        item.status.name === 'Accepted' ? 'green'
                        : item.status.name === 'Declined' ? 'red'
                        : 'orange'
                    ">
                    {{ item.status.name }}
                </BadgeComponent>
            </td>
            <td class="hidden sm:table-cell">{{ item.category.name }} [{{ item.category.code }}]</td>
            <td class="font-medium">
                {{ item.title }}
            </td>
            <td>
                <div class="flex justify-end">
                    <ButtonComponent
                        v-if="item.status.name === 'Accepted'"
                        state="cta"
                        :router-link="`/question/${item.id}`">
                        <HeroIcon icon="ArrowLongRightIcon" />
                    </ButtonComponent>

                    <ButtonComponent
                        v-else-if="item.status.name === 'Pending' && userStore.userHasRole('manager')"
                        state="cta"
                        :router-link="`/question/${item.id}/edit`">
                        <HeroIcon icon="PencilIcon"></HeroIcon>
                    </ButtonComponent>

                    <ButtonComponent v-else state="flat" class="flex gap-1 items-center">
                        <HeroIcon icon="ExclamationCircleIcon" />
                        <span>{{ item.status.name }}</span>
                    </ButtonComponent>
                </div>
            </td>
        </template>
    </TableComponent>
</template>
