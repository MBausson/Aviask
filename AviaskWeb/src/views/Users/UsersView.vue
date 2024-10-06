<script lang="ts" setup>
import {
    ButtonComponent,
    DeleteUserModal,
    RoleIcon,
    TableComponent,
    DateTime,
    PaginatorComponent,
    HeroIcon,
    TitleComponent,
} from "@/components";
import { ref, type Ref } from "vue";
import useEventBus from "@/eventBus";
import { responseModels } from "@/models";
import { useRoute } from "vue-router";
import useUsersRepository from "@/repositories/usersRepository";
import type { FilteredResult, UserExtended } from "@/models/DTO/responseModels";
import {
    DropdownMenu,
    DropdownMenuTrigger,
    DropdownMenuItem,
    DropdownMenuContent,
    DropdownMenuSeparator,
    DropdownMenuLabel,
} from "@/components/ui/dropdown-menu";

const { emitBus } = useEventBus();
const userRepository = useUsersRepository();
const route = useRoute();

const showDeleteConfirmation: Ref<boolean> = ref<boolean>(false);
const userToDelete: Ref<responseModels.UserExtended | undefined> = ref(undefined);
const page = ref(Number(route.query.route || 1));
const users: Ref<FilteredResult<UserExtended>> = ref(await userRepository.getAll(page.value));

async function copyUserId(user: responseModels.UserExtended) {
    await navigator.clipboard.writeText(user.id);
    emitBus("flashOpen", `Copied '${user.userName}' ID to your clipboard`, "success");
}

async function onDeleteRequested(user: responseModels.UserExtended) {
    showDeleteConfirmation.value = true;
    userToDelete.value = user;
}

//  Deletes a user
async function onDeleteConfirmed(user: responseModels.UserExtended) {
    if (!user) return;

    const res = await userRepository.delete(user.id);

    if (!res) {
        emitBus("flashOpen", `Could not delete user '${user.userName}'`, "danger");

        userToDelete.value = undefined;
        return;
    }

    users.value.elements = users.value.elements.splice(users.value.elements.indexOf(user), 1);
    emitBus("flashOpen", `Successfully deleted user '${user.userName}'`, "success");
    userToDelete.value = undefined;
}

async function onPageChange(newPage: number) {
    page.value = newPage;

    users.value = await userRepository.getAll(page.value);
}
</script>

<template>
    <DeleteUserModal
        v-if="userToDelete"
        v-model:show="showDeleteConfirmation"
        :user="userToDelete"
        @delete-confirmed="onDeleteConfirmed" />

    <TitleComponent class="my-6 flex justify-between gap-4">
        <span class="text-4xl font-bold dark:text-white">Manage users</span>
        <div class="flex gap-3 items-center">
            <PaginatorComponent :filtered="users" :page @update:model-value="onPageChange" />
        </div>
    </TitleComponent>

    <TableComponent
        :headers="['Created at', 'Username', 'Email', 'Role', '']"
        :data="users.elements"
        class="w-full min-h-[75vh]">
        <template #row="{ item }">
            <td class="tracking-wide">
                <span class="flex items-center gap-2">
                    <DropdownMenu>
                        <DropdownMenuTrigger>
                            <HeroIcon
                                icon="Cog6ToothIcon"
                                type="solid"
                                class="transition-transform active:rotate-90 mb-0.5" />
                        </DropdownMenuTrigger>

                        <DropdownMenuContent class="min-w-52">
                            <DropdownMenuLabel>{{ item.userName }}</DropdownMenuLabel>

                            <DropdownMenuSeparator />

                            <DropdownMenuItem>
                                <button
                                    class="w-full flex items-center gap-3 justify-between"
                                    @click="copyUserId(item)">
                                    <span>Copy user ID</span>
                                    <HeroIcon icon="ClipboardDocumentIcon" />
                                </button>
                            </DropdownMenuItem>

                            <DropdownMenuItem>
                                <RouterLink
                                    :to="`/user/${item.id}/edit`"
                                    class="w-full flex items-center gap-3 justify-between">
                                    <span>Edit</span>
                                    <HeroIcon icon="PencilIcon" />
                                </RouterLink>
                            </DropdownMenuItem>

                            <DropdownMenuSeparator />

                            <DropdownMenuItem>
                                <button
                                    class="w-full flex gap-2 items-center justify-between font-medium text-red-400"
                                    @click="onDeleteRequested(item)">
                                    <span>Delete</span>
                                    <HeroIcon icon="TrashIcon" />
                                </button>
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>

                    <DateTime :date="new Date(item.createdAt)" />
                </span>
            </td>
            <td class="font-semibold">
                <RouterLink :to="`/user/${item.id}/profile`">{{ item.userName }}</RouterLink>
            </td>
            <td class="text-muted-dark dark:text-muted">{{ item.email }}</td>
            <td>
                <div class="flex items-center gap-2 font-medium">
                    <RoleIcon :role="item.role" />

                    <HeroIcon
                        v-if="item.isPremium"
                        icon="CurrencyDollarIcon"
                        type="solid"
                        class="text-purple-600 dark:text-purple-400" />

                    <p class="inline first-letter:capitalize">{{ item.role }}</p>
                </div>
            </td>
            <td>
                <ButtonComponent class="flex float-right" state="cta" :router-link="`/user/${item.id}/profile`">
                    <HeroIcon icon="ArrowLongRightIcon" />
                </ButtonComponent>
            </td>
        </template>
    </TableComponent>
</template>
